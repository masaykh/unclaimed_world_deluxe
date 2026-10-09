#!/usr/bin/perl
# The string-table helpers of tools/build/37-make-strings.sh. Not run on its own.
#
#   perl 37-strings.pl compare TEMPLATE TRANSLATION   what a translation lacks, has extra, or left English
#   perl 37-strings.pl pseudo  TEMPLATE OUT            a pseudo-language: every value marked and lengthened
#   perl 37-strings.pl unrouted FILE...                interface text not yet through Locale, per file
use strict;
use warnings;

binmode STDOUT, ':encoding(UTF-8)';

sub read_table {
    my ($path) = @_;
    open my $fh, '<:encoding(UTF-8)', $path or die "cannot read $path: $!\n";
    local $/;
    my $xml = <$fh>;
    close $fh;
    my (@keys, %value);
    while ($xml =~ m{<Key>(.*?)</Key>\s*<Value>(.*?)</Value>}gs) {
        my ($k, $v) = ($1, $2);
        $k =~ s/\r\n/\n/g; $v =~ s/\r\n/\n/g;
        push @keys, $k unless exists $value{$k};
        $value{$k} = $v;
    }
    return (\@keys, \%value);
}

my $mode = shift @ARGV // '';

if ($mode eq 'compare') {
    my ($template, $translation) = @ARGV;
    my ($tkeys, $tval) = read_table($template);
    my ($xkeys, $xval) = read_table($translation);
    my @missing = grep { !exists $xval->{$_} } @$tkeys;
    my @obsolete = grep { !exists $tval->{$_} } @$xkeys;
    my @english = grep { exists $xval->{$_} && $xval->{$_} eq $tval->{$_} && $tval->{$_} =~ /[A-Za-z]{3}/ } @$tkeys;
    my @placeholders = grep {
        my $k = $_;
        exists $xval->{$k} && join(',', sort $tval->{$k} =~ /\{(\d+)\}/g) ne join(',', sort $xval->{$k} =~ /\{(\d+)\}/g)
            && $k !~ /^\(COUNT\)/
    } @$tkeys;
    my $done = @$tkeys - @missing - @english;
    printf "%s: %d of %d translated (%d%%)\n", $translation, $done, scalar @$tkeys, @$tkeys ? 100 * $done / @$tkeys : 0;
    for ([missing => \@missing, 'not in the translation - copy these from the template'],
         [obsolete => \@obsolete, 'no longer in the game - safe to delete'],
         [english => \@english, 'still the English value'],
         [placeholders => \@placeholders, 'a {0}/{1} differs from the English - the game will show the wrong number or crash']) {
        my ($name, $list, $what) = @$_;
        next unless @$list;
        printf "\n%d %s: %s\n", scalar @$list, $name, $what;
        print "  $_\n" for @$list[0 .. ($#$list < 39 ? $#$list : 39)];
        print "  ... and ", @$list - 40, " more\n" if @$list > 40;
    }
    exit(@placeholders ? 1 : 0);
}

if ($mode eq 'pseudo') {
    my ($template, $out) = @ARGV;
    my ($keys, $val) = read_table($template);
    my %accent = (a => "\x{E0}", e => "\x{EB}", i => "\x{EF}", o => "\x{F6}", u => "\x{FC}",
                  A => "\x{C5}", E => "\x{CB}", I => "\x{CF}", O => "\x{D6}", U => "\x{DC}");
    open my $fh, '>:encoding(UTF-8)', $out or die "cannot write $out: $!\n";
    print $fh qq{<?xml version="1.0" encoding="utf-8"?>\r\n<ArrayOfString>\r\n};
    for my $k (@$keys) {
        my @forms = split /\|/, $val->{$k}, -1;
        for (@forms) {
            # Accent the vowels outside {0}-style placeholders and &...; entities; lengthen by 30%.
            my @parts = split /(\{\d+\}|&[a-z#0-9]+;)/;
            for my $p (@parts) { $p =~ s/([aeiouAEIOU])/$accent{$1}/g unless $p =~ /^(\{\d+\}|&[a-z#0-9]+;)$/; }
            my $text = join '', @parts;
            my $pad = '~' x int(length($_) * 0.3);
            $_ = "[\x{416} $text$pad \x{416}]";
        }
        print $fh "  <String>\r\n    <Key>$k</Key>\r\n    <Value>", join('|', @forms), "</Value>\r\n  </String>\r\n";
    }
    print $fh "</ArrayOfString>\r\n";
    close $fh;
    printf "==> wrote %s (%d entries)\n", $out, scalar @$keys;
    exit 0;
}

sub without_holes {
    my ($s) = @_;
    $s =~ s/\{[^\}]*\}//g;
    return $s;
}

# ---- interface text not yet routed through Locale ----------------------------------------------
#
# WHERE ENGLISH REACHES THE SCREEN. An assignment to a control's Text, ToolTip, Title, Summary or
# Caption; or a whole argument of one of the SINKS - the message boxes, the log, and the interface's
# own helpers that make a button, a heading, a tooltip or a list entry. In the interface's folders
# also the tooltip builders (Append, AppendLine...), panel constructors (base) and string.Format:
# elsewhere those write files and logs.
my $literal = qr/"(?:[^"\\]|\\.)*"/;
my %SINKS = map { $_ => 1 } qw(
    ShowError ShowMessage AddLogEvent ComposeHeadingAndBlobText AddBottomButtonInSequence
    CreateTooltip AddLowerButton AddEntry AddSubHeader CreateTextButton SetMinimizedOrExpandedContentProperties
    AddCollapsablePanelAndGrid AddZoneNameAndHeader AddTextButton AppendImpossibleActionText AppendPossibleActionText
    AddButton CreateGridAndHeader CreateCheckBox AddSpeedButton AddTabPage AppendIndentedLine AppendHeaderOnLightBG
    CreateImageButton CreateRadioButton DisplayCommunication ShowErrorDialog AddEntityAmountRow AddBlackTextButton
    AddCRTCaptionAndLabel SetButtonText AddEntryRightJustifyValue ShowImageAndText SetHeaderText ToLabel);
my %INTERFACE_SINKS = map { $_ => 1 } qw(Append AppendLine AppendFormat Format base Tuple);
# Sinks with a key among their arguments: only these positions are text. AddEntry(key, text) -
# the key is how the grid finds the entry again, and must not change with the language.
my %TEXT_ARGS = (AddEntry => [1], Format => [0]);
my $interface_dir = qr{ClientSide/(?:Interface|MainMenu|Screens)/|Client/MainMenu/};

# A literal a player reads, rather than a key, an icon, a path or a format. "OK", "NAME", "Name of
# food type" are text; "HUD_icon_sword", "itemKey", "item:knife", "Fonts/x", "#COLOR" are not.
sub is_text {
    my ($lit) = @_;
    my $s = without_holes(substr($lit, 1, -1));
    return 0 unless $s =~ /[A-Za-z]{2}/;
    return 0 if $s =~ /^[a-z][A-Za-z0-9_]*$/;                    # camelCase key
    return 0 if $s =~ /^[A-Za-z0-9]+_[A-Za-z0-9_]*$/;             # snake_case, icons
    return 0 if $s =~ /^[A-Za-z0-9_.]+[\/\\:][\w\/\\.:-]+$/;      # paths, keys with a colon (not "EFFECTS:")
    return 0 if $s =~ /^#[A-Z]/;                                  # colour tags
    return 0 if $s =~ /^[A-Z][a-z0-9]+[A-Z][A-Za-z0-9]*$/;        # PascalCase names
    return 1;
}

# The whole-argument literals of the sinks on one line, as [start, length, literal, interpolated].
sub sink_literals {
    my ($line, $file) = @_;
    my @found;
    while ($line =~ /\b(\w+)(?:<[^<>()]*>)?\(/g) {
        my ($name, $open) = ($1, pos($line));
        next unless $SINKS{$name} || ($INTERFACE_SINKS{$name} && $file =~ $interface_dir);
        # Split the arguments at top-level commas, minding strings and nested brackets.
        my ($depth, $start, @argspans) = (0, $open);
        my $i = $open;
        while ($i < length $line) {
            my $c = substr($line, $i, 1);
            if ($c eq '"' || ($c eq '$' && substr($line, $i + 1, 1) eq '"')) {
                pos($line) = $c eq '$' ? $i + 1 : $i;
                last unless $line =~ /\G$literal/gc;
                $i = pos($line);
                next;
            }
            if ($c eq '(' || $c eq '[' || $c eq '{') { $depth++ }
            elsif ($c eq ')' || $c eq ']' || $c eq '}') {
                if ($depth == 0) { push @argspans, [$start, $i - $start]; last }
                $depth--;
            }
            elsif ($c eq ',' && $depth == 0) { push @argspans, [$start, $i - $start]; $start = $i + 1 }
            $i++;
        }
        push @argspans, [$start, $i - $start] if $i >= length $line && $i > $start;
        my @spans = $TEXT_ARGS{$name} ? grep { defined } @argspans[@{ $TEXT_ARGS{$name} }] : @argspans;
        for my $span (@spans) {
            my $arg = substr($line, $span->[0], $span->[1]);
            next unless $arg =~ /^(\s*)(\$?)($literal)\s*$/;
            my ($lead, $interp, $lit) = ($1, $2, $3);
            push @found, [$span->[0] + length($lead), length($interp) + length($lit), $lit, $interp ne ''] if is_text($lit);
        }
        pos($line) = $open;
    }
    return @found;
}

# The literals of an assignment to a control, one by one, left to right.
sub assigned_literals {
    my ($line) = @_;
    my @found;
    # A control's member - lbl.ToolTip = ... - or, inside the control's own class, a bare one.
    return @found unless $line =~ /(?:\.|^\s*)(?:Text|ToolTip|Title|Summary|Caption)\s*(?:=|\+=)/g;
    my $from = pos($line);
    # The spans of Locale calls, arguments and all: a literal inside one is routed already.
    my $args = qr/(?:[^()"]|$literal|\((?:[^()"]|$literal)*\))*/;
    my @routed;
    while ($line =~ /Locale\.(?:Text|Count)\($args\)/g) { push @routed, [$-[0], $+[0]] }
    pos($line) = $from;
    while ($line =~ /(\$?)($literal)/g) {
        my ($interp, $lit) = ($1, $2);
        my $start = $-[0];
        next if grep { $start >= $_->[0] && $start < $_->[1] } @routed;
        push @found, [$start, length($interp) + length($lit), $lit, $interp ne ''] if without_holes($lit) =~ /[A-Za-z]{2}/;
    }
    return @found;
}

# In the interface's folders: a label given by a switch arm or a return - => "COLONY MEMBERS",
# return "Very low"; - the way GetName and DefenseRatingToString hand text to a control.
sub returned_literals {
    my ($line, $file) = @_;
    my @found;
    return @found unless $file =~ $interface_dir;
    while ($line =~ /(?:=>\s*|\breturn\s+)(\$?)($literal)\s*[,;]/g) {
        my ($interp, $lit) = ($1, $2);
        push @found, [$-[1], length($interp) + length($lit), $lit, $interp ne ''] if is_text($lit);
    }
    return @found;
}

if ($mode eq 'unrouted' || $mode eq 'route') {
    my $total = 0;
    for my $file (@ARGV) {
        open my $fh, '<:raw', $file or next;
        my @lines = <$fh>;
        close $fh;
        my ($n, $changed) = (0, 0);
        for my $line (@lines) {
            next if $line =~ m{^\s*(//|/\*|\*)};
            my %seen;
            my @hits = grep { !$seen{$_->[0]}++ } (assigned_literals($line), sink_literals($line, $file), returned_literals($line, $file));
            $n += @hits;
            next unless $mode eq 'route';
            # Wrap the plain ones, right to left; an interpolated $"..." needs a format by hand.
            for my $h (sort { $b->[0] <=> $a->[0] } grep { !$_->[3] } @hits) {
                substr($line, $h->[0], $h->[1]) = "UWGame.Locale.Text($h->[2])";
                $changed++;
                $n--;
            }
        }
        if ($mode eq 'route' && $changed) {
            open my $out, '>:raw', $file or die "$file: $!";
            print $out @lines;
            close $out;
        }
        $total += $n;
        print "$n\t$file\n" if $n > 0 && $mode eq 'unrouted';
        print "$changed routed, $n left\t$file\n" if $mode eq 'route' && ($changed || $n);
    }
    exit 0;
}

die "usage: perl 37-strings.pl compare|pseudo|unrouted|route ...\n";
