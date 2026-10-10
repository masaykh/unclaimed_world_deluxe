#!/usr/bin/perl
# The string-table helpers of tools/build/37-make-strings.sh. Not run on its own.
#
#   perl 37-strings.pl compare TEMPLATE TRANSLATION   what a translation lacks, has extra, or left English
#   perl 37-strings.pl pseudo  TEMPLATE OUT            a pseudo-language: every value marked and lengthened
#   perl 37-strings.pl group   TABLE WHERE            order the template by source file, mod and table
#   perl 37-strings.pl merge   TEMPLATE TRANSLATION OUT  the translation, brought up to date with the template
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
    # Old "(GUI)..." keys are the same entries as their short forms (file_key).
    my %short;
    my %seen_key;
    $short{file_key($_)} = $xval->{$_} for @$xkeys;
    $xkeys = [grep { !$seen_key{$_}++ } map { file_key($_) } @$xkeys];
    $xval = \%short;
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

# The <String> entries of a table, raw (still XML-escaped, line endings as written), in file order.
sub read_entries {
    my ($path) = @_;
    open my $fh, '<:encoding(UTF-8)', $path or die "cannot read $path: $!\n";
    local $/;
    my $xml = <$fh>;
    close $fh;
    my @entries;
    while ($xml =~ m{<Key>(.*?)</Key>\s*<Value>(.*?)</Value>}gs) { push @entries, [$1, $2] }
    return @entries;
}

sub write_entries {
    my ($path, @blocks) = @_;
    open my $fh, '>:encoding(UTF-8)', $path or die "cannot write $path: $!\n";
    print $fh qq{<?xml version="1.0" encoding="utf-8"?>\r\n<ArrayOfString>\r\n};
    for my $b (@blocks) {
        if (!ref $b) { print $fh "  <!-- $b -->\r\n"; next }
        print $fh "  <String>\r\n    <Key>$b->[0]</Key>\r\n    <Value>$b->[1]</Value>\r\n  </String>\r\n";
    }
    print $fh "</ArrayOfString>\r\n";
    close $fh;
}

sub normal { my $s = shift; $s =~ s/\r\n/\n/g; return $s }

sub xml_unescape {
    my $s = shift;
    $s =~ s/&lt;/</g; $s =~ s/&gt;/>/g; $s =~ s/&quot;/"/g; $s =~ s/&apos;/'/g;
    $s =~ s/&#x([0-9A-Fa-f]+);/chr(hex $1)/ge; $s =~ s/&#(\d+);/chr($1)/ge;
    $s =~ s/&amp;/&/g;
    return $s;
}

# The key a language file uses for a key the code builds - Locale.ShortKey, which this must match
# exactly (group checks it against the game's for every template entry). "(ITEM DESCRIPTION)
# item:acetylene" is ItemDescriptionItemAcetylene: the area's words capitalised, the rest's words
# with the first letter raised and their case kept. Cut at 50 and given "_" and 12 hex digits of
# the full key's SHA-1 when longer, or when interface text or a count is not plain words.
use Digest::SHA qw(sha1_hex);
use Encode qw(encode_utf8);
sub short_key {
    my ($key) = @_;
    my ($area, $rest) = (undef, $key);
    ($area, $rest) = ($1, $2) if $key =~ /^\(([^)]*)\)(.*)\z/s;
    my $readable = '';
    if (defined $area) { $readable .= ucfirst lc for grep { length } split /[^A-Za-z0-9]+/, $area }
    $readable .= ucfirst for grep { length } split /[^A-Za-z0-9]+/, $rest;
    my $text = defined $area && ($area eq 'GUI' || $area eq 'COUNT');
    my $plain = !$text || $rest =~ /\A[A-Za-z0-9]+(?: [A-Za-z0-9]+)*\z/;
    return $readable if length($readable) <= 50 && $plain;
    return substr($readable, 0, 50) . '_' . substr(sha1_hex(encode_utf8($key)), 0, 12);
}

# A key as a file has it, in the short form: an old "(GUI)SAVE GAME" is converted, as the game does.
sub file_key {
    my $k = xml_unescape(normal(shift));
    return $k =~ /\A[A-Za-z0-9_]+\z/ ? $k : short_key($k);
}

# GROUP: put the template in an order a translator can work through (Kastuk: "sort Strings ... by
# their classes in source code, so all related strings will be nearby"). The interface by the
# source file that asks for it, in the order the file asks; the mod settings by mod, each setting's
# label, tooltip and choices together; the data by table, each entry's name, plural and
# descriptions together. An XML comment heads each group - the game's reader skips comments.
if ($mode eq 'group') {
    my ($table, $where) = @ARGV;
    my @entries = read_entries($table);
    # Where each Locale.Text literal is: file, line, the literal as written in C#.
    my %first;    # key => [file, order]
    my $order = 0;
    open my $wh, '<:encoding(UTF-8)', $where or die "cannot read $where: $!\n";
    my %unescape = (n => "\n", t => "\t", r => "\r", '0' => "\0");
    while (my $line = <$wh>) {
        chomp $line;
        my ($file, $lineno, $lit) = split /\t/, $line, 3;
        next unless defined $lit;
        (my $text = $lit) =~ s/\\(.)/exists $unescape{$1} ? $unescape{$1} : $1/ge;
        $text =~ s/&/&amp;/g; $text =~ s/</&lt;/g; $text =~ s/>/&gt;/g;
        my $key = normal("(GUI)$text");
        $file =~ s{^base_game/UnclaimedWorld/UWGame/}{}; $file =~ s{^base_game/}{};
        my $rank = sprintf '%s %08d', $file, $lineno;
        $first{$key} = [$file, $rank] if !$first{$key} || $rank lt $first{$key}[1];
    }
    close $wh;
    my (%gui, @gui_elsewhere, @counts, %settings, %data);
    for my $e (@entries) {
        my $k = normal($e->[0]);
        if ($k =~ /^\(GUI\)/) {
            if ($first{$k}) { push @{ $gui{$first{$k}[0]} }, [$first{$k}[1], $e] } else { push @gui_elsewhere, $e }
        }
        elsif ($k =~ /^\(COUNT\)/) { push @counts, $e }
        elsif ($k =~ /^\(SETTING( TIP| GROUP| CHOICE)?\)([^.=]*)(?:\.([^=]*))?(?:=(.*))?$/s) {
            my ($kind, $mod, $id, $choice) = ($1 // '', $2, $3 // '', $4 // '');
            my $rank = { '' => 1, ' TIP' => 2, ' CHOICE' => 3, ' GROUP' => 0 }->{$kind};
            push @{ $settings{$mod} }, [join("\t", $id, $rank, $choice), $e];
        }
        elsif ($k =~ /^\(([^ )]+)(?: [^)]*)?\)([^\/@]*)/s) {
            # The entry's own key, so its name, plural and descriptions stand together; its plain
            # (name) entry first, then the rest by key.
            my ($area, $entry) = ($1, $2);
            my $plain = $k =~ /^\([^ )]+\)[^\/@]*$/s ? 0 : 1;
            push @{ $data{$area} }, [join("\t", $entry, $plain, $k), $e];
        }
        else { push @gui_elsewhere, $e }
    }
    my @blocks;
    for my $file (sort keys %gui) {
        push @blocks, $file, map { $_->[1] } sort { $a->[0] cmp $b->[0] } @{ $gui{$file} };
    }
    push @blocks, 'interface text from elsewhere', @gui_elsewhere if @gui_elsewhere;
    push @blocks, 'numbers with their nouns: forms separated by |', @counts if @counts;
    for my $mod (sort keys %settings) {
        push @blocks, "settings: $mod", map { $_->[1] } sort { $a->[0] cmp $b->[0] } @{ $settings{$mod} };
    }
    for my $area (sort keys %data) {
        push @blocks, "data: $area", map { $_->[1] } sort { $a->[0] cmp $b->[0] } @{ $data{$area} };
    }
    die "group lost entries\n" unless grep({ ref } @blocks) == @entries;
    # The file keys: the game's (DataExport writes TABLE.keys, an entry's a line, in the table's
    # order), checked against short_key's - the two copies of the rule must never drift.
    open my $kh, '<:encoding(UTF-8)', "$table.keys" or die "cannot read $table.keys: $!\n";
    chomp(my @game = <$kh>);
    close $kh;
    s/\r\z// for @game;
    die "$table.keys has " . @game . " keys for " . @entries . " entries\n" unless @game == @entries;
    my @differ;
    for my $i (0 .. $#entries) {
        my $mine = short_key(xml_unescape(normal($entries[$i][0])));
        push @differ, "  $entries[$i][0]: game $game[$i], 37-strings.pl $mine" if $mine ne $game[$i];
        $entries[$i][0] = $game[$i];
    }
    die "FAIL  37-strings.pl short_key and Locale.ShortKey disagree:\n" . join("\n", @differ[0 .. ($#differ < 9 ? $#differ : 9)]) . "\n" if @differ;
    write_entries($table, @blocks);
    exit 0;
}

# MERGE: bring a translation up to date with the template (Kastuk: "a script to merge translation
# xml files, to add new untranslated lines from English (US).xml into partially translated file").
# The result is the template - its order and its group comments - with this translation's values
# wherever it has one; what it lacks comes in as English. Entries the game no longer asks for are
# kept at the end, so nothing translated is ever lost.
if ($mode eq 'merge') {
    my ($template, $translation, $out) = @ARGV;
    my (%mine, @mine_order);
    for my $e (read_entries($translation)) {
        # Written before the keys were shortened, it says "(GUI)SAVE GAME": the same entry.
        my $k = file_key($e->[0]);
        push @mine_order, $e unless exists $mine{$k};
        $mine{$k} = $e->[1];
    }
    open my $fh, '<:encoding(UTF-8)', $template or die "cannot read $template: $!\n";
    local $/;
    my $xml = <$fh>;
    close $fh;
    my (@blocks, %used, $added);
    while ($xml =~ m{<!--\s*(.*?)\s*-->|<Key>(.*?)</Key>\s*<Value>(.*?)</Value>}gs) {
        if (defined $1) { push @blocks, $1; next }
        my ($k, $v) = ($2, $3);
        my $nk = file_key($k);
        if (exists $mine{$nk}) { push @blocks, [$k, $mine{$nk}]; $used{$nk} = 1 }
        else { push @blocks, [$k, $v]; $added++ }
    }
    my @old = grep { !$used{file_key($_->[0])} } @mine_order;
    push @blocks, 'no longer in the game - kept so nothing is lost, safe to delete', @old if @old;
    write_entries($out, @blocks);
    printf "==> %s: %d entries, %d new (in English until translated), %d no longer in the game (kept at the end)\n",
        $out, grep({ ref } @blocks) - @old, $added // 0, scalar @old;
    exit 0;
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
    AddCRTCaptionAndLabel SetButtonText AddEntryRightJustifyValue ShowImageAndText SetHeaderText ToLabel AppendComponent);
my %INTERFACE_SINKS = map { $_ => 1 } qw(Append AppendLine AppendFormat Format base Tuple AddToList);
# Sinks with a key among their arguments: only these positions are text. AddEntry(key, text) -
# the key is how the grid finds the entry again, and must not change with the language.
my %TEXT_ARGS = (AddEntry => [1], Format => [0], AppendComponent => [1], AddToList => [1]);
# The simulation's own text builders count as interface: the rating breakdowns, what a colonist
# is doing, a job's or a mission's name, the date. Elsewhere in SimSide these helpers build keys and
# debug output.
my $interface_dir = qr{ClientSide/(?:Interface|MainMenu|Screens)/|Client/MainMenu/
    |SimSide/(?:Allegiances/Statistics|AI/Goals|AI/StrategicDecisions|InGameEvents/SpecialEvents|Overland/Missions|Jobs/JobTypes|SimEffects)/
    |SimSide/(?:DateAndTime|Buildings/Structure|Scenarios/Scenario|Communication/CommunicatorType|Entities/Personality
      |Entities/Biological/BiologicalEntity|Jobs/(?:Job|AttackAreaJob|CheckProcessJob|FindPreyJob|HuntingJob|PatrolJob|ScoutingJob))\.cs}x;

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
        # Inside a /* ... */ block, nothing is code: the original source keeps whole methods there
        # beside their routed replacements (Personality.GetHappinessBreakdown, ea6bb85).
        my $in_block = 0;
        for my $line (@lines) {
            if ($in_block) {
                $in_block = 0 if $line =~ m{\*/};
                next;
            }
            $in_block = 1 if $line =~ m{/\*(?!.*\*/)};
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
