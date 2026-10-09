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

if ($mode eq 'unrouted') {
    # Text written straight to an interface control, a message box or the log - each one an
    # English string Locale cannot reach. Counted per file.
    my $text = qr/"(?:[^"\\]|\\.)*[A-Za-z]{2}(?:[^"\\]|\\.)*"/;
    for my $file (@ARGV) {
        open my $fh, '<:encoding(UTF-8)', $file or next;
        my $n = 0;
        while (my $line = <$fh>) {
            next if $line =~ m{^\s*(//|/\*|\*)};
            $n++ while $line =~ /(?:\.Text|\.ToolTip|\.Title|\.Summary|\.Caption)\s*(?:=|\+=)\s*\$?$text/g;
            $n++ while $line =~ /\b(?:ShowError|ShowMessage|AddLogEvent|ComposeHeadingAndBlobText|AddBottomButtonInSequence)\([^;]*?(?<!Locale\.Text\()\$?$text/g;
        }
        close $fh;
        print "$n\t$file\n" if $n > 0;
    }
    exit 0;
}

die "usage: perl 37-strings.pl compare|pseudo|unrouted ...\n";
