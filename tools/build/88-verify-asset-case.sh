#!/bin/sh
# Lists every asset and data name the code asks for whose letter case differs from the file on
# disk, and fails on one that is not in 88-asset-case-allowlist.txt.
#
# WHY THIS EXISTS. The studio developed on Windows, where Content/Fonts/CRT_18pt opens
# crt_18pt.xnb. On Linux it does not, and the GL build stopped before the main menu (Kastuk,
# testing on Ubuntu, 2026-11-18: "call for file.xnb as File.xnb will cannot be find" - he got past
# it by mounting Content/ and data/ through ciopfs). UWGame.Port.CasePath now matches names
# ignoring case on a case-sensitive file system (UwContentManager.OpenStream, Config.GetDataFolderPath),
# so a mismatch no longer stops the game - but every one is a load that only works because of that
# fallback, and nobody on Windows would ever notice a new one. This makes them visible.
#
# What is compared:
#   names    every string literal in base_game/ and mods/ (.cs, outside // lines) and every
#            element text and attribute value in scenarios/ - '\' read as '/', a leading Content/
#            and a trailing .xnb dropped
#   against  YOUR Content/ ($UW_STEAM/Content, plus the effects assets/effects builds into it) and
#            the game's data/ ($UW_STEAM/data, with scenarios/ as data/Maps and translations/ as
#            data/BaseData/Strings, as the package lays them out)
#   FULL     the whole name is a file or folder there, in another case
#   SUFFIX   the name is the END of one, in another case, and no file ends in it as spelled -
#            the shape of a name the code prefixes at run time ("Sounds\\" + Sound)
#
# It is a name search, so it errs towards reporting: a technique called "Water" or an item called
# "Skimmer" matches water.xnb and Models/skimmer.xnb. Those are in the allowlist with the reason.
# The list has the same rules as gates 85 and 86: "<literal> -> <on disk>  # why", a reason is
# required, and an entry that no longer matches anything fails the gate.
#
# RUN IT LOCALLY, NOT IN CI, like 80 and 84: it reads your copy of the game's compiled content,
# which a runner does not have.
#
#   sh tools/build/88-verify-asset-case.sh
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

[ -d "$UW_STEAM/Content" ] || {
  echo "FATAL: no Content/ under UW_STEAM=$UW_STEAM" >&2
  echo "       This runs against YOUR copy of the game's compiled content - see license.md." >&2
  echo "       export UW_STEAM=\"/path/to/Unclaimed World\"" >&2
  exit 2
}

ALLOW=tools/build/88-asset-case-allowlist.txt
[ -f "$ALLOW" ] || { echo "missing $ALLOW"; exit 1; }

echo "==> asset names whose case differs from the file on disk"
git -c core.quotepath=off ls-files -z -- 'base_game/*.cs' 'mods/*.cs' 'scenarios/*.xml' \
  | perl -e '
  use strict; use warnings; use File::Find;
  my ($steam, $allow) = @ARGV[0, 1];
  my @sources = grep { length } split /\0/, do { local $/; <STDIN> };

  # --- what is on disk: Content/ relative to Content, data/ relative to the game root
  my %disk;
  my $index = sub {
    my ($root, $prefix) = @_;
    return unless -d $root;
    find({ no_chdir => 1, wanted => sub {
      return if $File::Find::name eq $root;
      return unless -f $File::Find::name || ($prefix =~ m{^data/Maps/$} && $File::Find::dir eq $root);
      (my $rel = substr($File::Find::name, length($root) + 1)) =~ s/\.xnb$//i;
      $disk{$prefix . $rel} = 1;
    }}, $root);
  };
  $index->("$steam/Content", "");
  $index->("$steam/data", "data/");
  $index->("scenarios", "data/Maps/");
  $index->("translations", "data/BaseData/Strings/");
  $index->("assets/effects", "");
  for my $fx (grep { /\.fx$/i } keys %disk) { (my $x = $fx) =~ s/\.fx$//i; $disk{$x} = 1; delete $disk{$fx}; }
  my %lower;
  push @{ $lower{lc $_} }, $_ for sort keys %disk;
  my %ends;   # lower-case trailing part after a "/" -> the files ending in it
  for my $path (keys %disk) {
    my @parts = split m{/}, $path;
    for my $i (1 .. $#parts) { push @{ $ends{lc join "/", @parts[$i .. $#parts]} }, $path; }
  }

  # --- what the code and data name
  my %found;
  for my $f (@sources) {
    open my $h, "<", $f or next;
    while (my $line = <$h>) {
      $line =~ s/\r?\n$//;
      next if $f =~ /\.cs$/ && $line =~ m{^\s*//};
      my @names = $f =~ /\.cs$/ ? ($line =~ /"((?:[^"\\]|\\.)*)"/g) : (($line =~ />([^<>]+)</g), ($line =~ /="([^"]*)"/g));
      for my $name (@names) {
        (my $p = $name) =~ s{\\\\}{/}g;
        $p =~ s{\\}{/}g; $p =~ s{^Content/}{}; $p =~ s{\.xnb$}{}i;
        next if length($p) < 3 || $p =~ /^\s|[<>{}*?|:]/ || $disk{$p};
        my $key;
        if (my $other = $lower{lc $p}) {
          $key = "FULL\t$p -> $other->[0]";
        } elsif (my $tails = $ends{lc $p}) {
          next if grep { substr($_, -length($p) - 1) eq "/$p" } @$tails;
          $key = "SUFFIX\t$p -> $tails->[0]";
        } else {
          next;
        }
        push @{ $found{$key} }, "$f:$.";
      }
    }
  }

  # --- against the allowlist
  my (%listed, @unexplained, $why);
  open my $a, "<", $allow or die "cannot read $allow\n";
  while (my $line = <$a>) {
    $line =~ s/\r?\n$//;
    if ($line =~ /^\s*$/) { $why = 0; next; }
    if ($line =~ /^\s*#/) { $why = 1; next; }
    push @unexplained, "        $.: $line" unless $why || $line =~ /#/;
    (my $entry = $line) =~ s/\s*#.*$//;
    $entry =~ s/^\s+//;
    $listed{$entry} = 0;
  }
  if (@unexplained) {
    print "  FAIL  allowlist entries with no reason (add a # comment on the line or above its group):\n";
    print "$_\n" for @unexplained;
    exit 1;
  }

  my $fail = 0;
  for my $key (sort keys %found) {
    my ($kind, $pair) = split /\t/, $key;
    my $where = join ", ", @{ $found{$key} }[0 .. ($#{ $found{$key} } < 2 ? $#{ $found{$key} } : 2)];
    $where .= sprintf(" (+%d)", @{ $found{$key} } - 3) if @{ $found{$key} } > 3;
    if (exists $listed{$pair}) {
      $listed{$pair} = 1;
      printf "  ok    %-6s %s   [%s]\n", $kind, $pair, $where;
    } else {
      printf "  FAIL  %-6s %s   [%s]\n", $kind, $pair, $where;
      $fail = 1;
    }
  }
  for my $entry (sort grep { !$listed{$_} } keys %listed) {
    print "  FAIL  allowlist entry matches nothing any more: $entry\n";
    $fail = 1;
  }
  printf "==> %d name(s) differ in case from disk, %d of them not in %s\n",
    scalar(keys %found), scalar(grep { !exists $listed{(split /\t/)[1]} } keys %found), $allow;
  if ($fail) {
    print "    A FAIL above loads on Windows and, through CasePath, on Linux. Spell it as on disk\n";
    print "    if nothing else depends on the spelling; otherwise list it here with the reason.\n";
  }
  exit $fail;
' "$UW_STEAM" "$ALLOW"
