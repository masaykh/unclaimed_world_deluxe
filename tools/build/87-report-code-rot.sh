#!/bin/sh
# A report, not a gate: the signs of code rot that need a person to judge. Always exits 0.
#
# Gates 85 and 86 fail the build on what is certainly dead. What is left is what might be alive:
#
#   1. Public and internal methods in the game and the mods whose name appears nowhere else. No
#      gate can remove these - an outside mod or Harmony patch may call them - but a new entry here
#      is usually the last caller of something having just been deleted.
#   2. Commented-out code: three or more consecutive // lines that read as statements.
#   3. Mod settings still TESTING (not in ModSettings.confirmedInPlay), oldest first, with the
#      date their mod first appeared. A setting that has been TESTING for months either needs
#      somebody to try it, or nobody wants it.
#   4. The PORT DEVIATION index: each deviation number and the files that cite it, so a deviation
#      whose code has gone - or a number cited by nothing - is visible in one place.
#
# Markdown on stdout. In GitHub Actions it is also written to the job summary; the weekly
# workflow (.github/workflows/code-rot-report.yml) runs it. Locally:
#
#   sh tools/build/87-report-code-rot.sh > code-rot.md
#
# Needs the full history for the dates in section 3 (fetch-depth: 0 in CI).
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

report() {
git -c core.quotepath=off ls-files -z | perl -0777 -e '
  use strict; use warnings;
  my @files = grep { length && !m{^(original_src|decomp)/} && -f } split /\0/, <STDIN>;
  my %text;
  for my $f (@files) {
    next unless $f =~ /\.(cs|md|sh|ps1|pl|py|csproj|props|targets|yml|yaml|xml|json|txt)$/i;
    open my $h, "<:raw", $f or next; local $/; my $t = <$h>; $t =~ s/\r\n/\n/g; $text{$f} = $t;
  }
  my %count;
  for my $t (values %text) { $count{$_}++ for $t =~ /\b([A-Za-z_]\w*)\b/g; }

  print "# Code rot report\n\n";

  # 1. methods named nowhere else
  my @lonely;
  for my $f (sort grep { m{^(base_game|mods)/.*\.cs$} && !m{/Generated/|^base_game/(AnimationComponentRuntime|SpriteSheetRuntime)/} } keys %text) {
    my $n = 0;
    for my $line (split /\n/, $text{$f}) {
      $n++;
      next unless $line =~ /^\s*(?:public|internal|protected)\s+(?:(?:static|virtual|sealed|new|unsafe|extern|async)\s+)*(?!class\b|struct\b|enum\b|interface\b|delegate\b|event\b|operator\b|implicit\b|explicit\b)[\w<>\[\],.?]+\s+(\w+)\s*(?:<[^>]*>)?\s*\(/;
      my $name = $1;
      next if $line =~ /\b(override|abstract)\b/ || $name =~ /^(ShouldSerialize\w*|Main|Prefix|Postfix|Transpiler|Finalizer|Prepare|Cleanup|TargetMethods?)$/;
      push @lonely, "| `$f:$n` | `$name` |" if ($count{$name} // 0) <= 1;
    }
  }
  print "## 1. Public or internal methods nothing names (", scalar(@lonely), ")\n\n";
  print @lonely ? join("\n", "| where | method |", "|---|---|", @lonely) . "\n\n" : "None.\n\n";

  # 2. commented-out code
  my @commented;
  for my $f (sort grep { m{^(base_game|mods|tools)/.*\.cs$} && !m{/Generated/} } keys %text) {
    my @lines = split /\n/, $text{$f};
    my ($run, $start) = (0, 0);
    for my $i (0 .. $#lines + 1) {
      my $l = $i <= $#lines ? $lines[$i] : "";
      if ($l =~ m{^\s*//(?!/)\s*(.*)$} && $1 =~ /(;|\{|\})\s*$/) { $start = $i + 1 unless $run++; next; }
      push @commented, "| `$f:$start` | $run lines |" if $run >= 3;
      $run = 0;
    }
  }
  print "## 2. Commented-out code blocks (", scalar(@commented), ")\n\n";
  print @commented ? join("\n", "| where | size |", "|---|---|", @commented) . "\n\n" : "None.\n\n";

  # 3. settings still TESTING
  my ($confirmed) = ($text{"base_game/UnclaimedWorld/UWGame/Mods/ModSettings.cs"} // "") =~ /confirmedInPlay\s*=\s*new[^{]*\{(.*?)\};/s;
  (my $entries = $confirmed // "") =~ s{//[^\n]*}{}g;   # the comments quote Kastuk, not setting ids
  my %stable = map { $_ => 1 } $entries =~ /"([^"]+)"/g;
  my @testing;
  for my $f (sort grep { m{^mods/[^/]+\.cs$|^base_game/UnclaimedWorld/UWGame/Mods/PortSettings\.cs$} } keys %text) {
    my ($modid) = $text{$f} =~ /const\s+string\s+ModId\s*=\s*"([^"]+)"/;
    $modid //= "port" if $f =~ /PortSettings/;
    next unless $modid;
    my $since = `git log --diff-filter=A --follow --format=%cs -- "$f"`; $since = (split /\n/, $since)[-1] // "?";
    while ($text{$f} =~ /ModSettings\.(?:Toggle|Choice|Key|Text)\(\s*(?:ModId|"[^"]*")\s*,\s*"([^"]+)"/g) {
      my $id = "$modid.$1";
      push @testing, [$since, $id, $f] unless $stable{$id};
    }
  }
  @testing = sort { $a->[0] cmp $b->[0] || $a->[1] cmp $b->[1] } @testing;
  print "## 3. Settings still TESTING (", scalar(@testing), " of ", scalar(@testing) + scalar(keys %stable), ")\n\n";
  print @testing ? join("\n", "| mod since | setting | file |", "|---|---|---|", map { "| $_->[0] | `$_->[1]` | `$_->[2]` |" } @testing) . "\n\n" : "None.\n\n";

  # 4. PORT DEVIATION index
  my %dev;
  for my $f (sort keys %text) { $dev{$1}{$f} = 1 while $text{$f} =~ /PORT DEVIATION (\d+)/g; }
  my @nums = sort { $a <=> $b } keys %dev;
  my %have = map { $_ => 1 } @nums;
  my @gaps = grep { !$have{$_} } 1 .. ($nums[-1] // 0);
  print "## 4. PORT DEVIATION index (", scalar(@nums), " cited", (@gaps ? "; not cited anywhere: @gaps" : ""), ")\n\n";
  print join("\n", "| # | cited in |", "|---|---|", map { my $n = $_; "| $n | " . join(", ", map { "`$_`" } sort keys %{ $dev{$n} }) . " |" } @nums), "\n";
'
}

if [ -n "$GITHUB_STEP_SUMMARY" ]; then
  report | tee -a "$GITHUB_STEP_SUMMARY"
else
  report
fi
exit 0
