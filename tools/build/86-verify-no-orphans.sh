#!/bin/sh
# Fails on things nothing uses that no compiler warning can see.
#
# Gate 85 catches unused PRIVATE members. Most of what the September 2026 sweep removed by hand was
# not private, and no analyzer reports it, because as far as the compiler knows somebody outside
# the assembly might call it. This gate checks the cases where we know better:
#
#   stub    A member of a mod's Absent stub (base_game/UnclaimedWorld/UWGame/Mods/*.Absent.cs)
#           that no file calls. A stub exists only to satisfy the core's calls into a mod that is
#           compiled out; once the core stops calling a member, the stub keeps it alive for
#           nothing. Five were found that way (AgentMod.IsEnabled, GatherOnDemandMod.Demand, ...).
#           Checked per class: a stub file may declare more than one (UnhiddenModSmithies).
#           Enabled, ModId and RegisterSettings are exempt: they are the shape every stub has, so
#           that a stub reads like its mod, not calls anybody needs.
#   mod     A public or internal member of a mod (mods/*.cs) that is named nowhere at all except
#           its own declaration. Harmony's by-convention names (Prefix, Postfix, ...) are exempt.
#   type    A class, struct, enum, interface or delegate whose name appears nowhere but its own
#           declaration. An attribute counts as used when its short name ([Author]) appears.
#           The content runtimes (AnimationComponentRuntime, SpriteSheetRuntime) are exempt: .xnb
#           files name their types, and those files are not in the repository.
#   setting A ModSetting (mods/*.cs, PortSettings.cs) that nothing reads - it shows in the options
#           menu and does nothing - or that RegisterSettings does not touch, so it stays out of
#           ModSettings.xml and the menu until something happens to read it.
#   symbol  A DefineConstants symbol no #if tests, or an #if symbol nothing defines (and the SDK
#           does not: DEBUG, TRACE, NET*, ...) - a branch that can never compile in, or a typo.
#   item    A Compile/None/Content/EmbeddedResource entry in a .csproj/.props/.targets naming a
#           file that does not exist.
#   script  A file in tools/build that no other file in the repository mentions: not CI, not a
#           document, not another script. Nobody can find it, so nobody runs it.
#
# original_src/ and decomp/ are reference trees, never built from, and are not checked.
#
# Kept on purpose: tools/build/86-orphans-allowlist.txt, "<check> <file> <name>  # reason", with
# the same rules as gate 85's list - a reason is required, and an entry that no longer matches
# anything fails the gate.
#
# Text only: no build, a few seconds. It is a name search, so it errs towards "used" - a member
# whose name happens to appear in a comment elsewhere passes. That is the safe direction for a gate.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

ALLOW=tools/build/86-orphans-allowlist.txt
[ -f "$ALLOW" ] || { echo "missing $ALLOW"; exit 1; }

unexplained=$(tr -d '\r' < "$ALLOW" | awk '
  /^[[:space:]]*$/ { why = 0; next }
  /^[[:space:]]*#/ { why = 1; next }
  { if (!why && $0 !~ /#/) print "        " NR ": " $0 }')
if [ -n "$unexplained" ]; then
  echo "  FAIL  allowlist entries with no reason (add a # comment on the line or above its group):"
  echo "$unexplained"
  exit 1
fi

WORK=$(mktemp -d)
trap 'rm -rf "$WORK"' EXIT INT TERM

echo "==> orphan check"
git -c core.quotepath=off ls-files -z > "$WORK/files"

perl -0777 -e '
  use strict; use warnings;
  my @files = grep { length && !m{^(original_src|decomp)/} } split /\0/, do { local $/; open my $f, "<", $ARGV[0] or die; <$f> };
  my %text;
  for my $f (@files) {
    next unless $f =~ /\.(cs|md|sh|ps1|pl|py|csproj|props|targets|yml|yaml|xml|json|txt|resx|config|mgcb)$/i;
    next unless -f $f;
    next if $f =~ m{^tools/build/[^/]*allowlist\.txt$};   # listing a name must not count as using it
    open my $h, "<:raw", $f or next; local $/; my $t = <$h>; $t =~ s/\r\n/\n/g; $text{$f} = $t;
  }
  my @out;
  my $member = qr/^\s*(?:public|internal)\s+(?:(?:static|const|readonly|virtual|new|unsafe|extern|async)\s+)*(?!class\b|struct\b|enum\b|interface\b|record\b|delegate\b)[\w<>\[\],.?]+(?:<[^>]*>)?\s+(\w+)\s*(?:<[^>]*>)?\s*(?:\(|=>|=|;|\{|$)/;

  # --- stub: members of Absent stubs, per declaring class
  for my $stub (grep { m{^base_game/UnclaimedWorld/UWGame/Mods/[^/]+\.Absent\.cs$} } sort keys %text) {
    my $class;
    for my $line (split /\n/, $text{$stub}) {
      if ($line =~ /^\s*(?:public|internal)\s+(?:static\s+|sealed\s+|partial\s+)*class\s+(\w+)/) { $class = $1; next; }
      next unless defined $class && $line =~ $member;
      my $name = $1;
      next if $name eq $class || $name =~ /^(Enabled|ModId|RegisterSettings)$/;
      my $used = 0;
      for my $f (keys %text) {
        next if $f eq $stub || $f !~ /\.cs$/;
        next if $f =~ m{^mods/} && $text{$f} =~ /\bclass\s+\Q$class\E\b/;   # the real mod
        if ($text{$f} =~ /\b\Q$class\E\s*\.\s*\Q$name\E\b/) { $used = 1; last; }
      }
      push @out, "stub\t$stub\t$class.$name" unless $used;
    }
  }

  # --- mod: public members named nowhere but their own declaration
  my %harmony = map { $_ => 1 } qw(Prefix Postfix Transpiler Finalizer Prepare Cleanup TargetMethod TargetMethods);
  for my $mod (grep { m{^mods/[^/]+\.cs$} } sort keys %text) {
    for my $line (split /\n/, $text{$mod}) {
      next unless $line =~ $member;
      my $name = $1;
      next if $harmony{$name} || $line =~ /\boverride\b/;
      my $own = () = $text{$mod} =~ /\b\Q$name\E\b/g;
      next if $own > 1;
      my $elsewhere = 0;
      for my $f (keys %text) {
        next if $f eq $mod;
        if ($text{$f} =~ /\b\Q$name\E\b/) { $elsewhere = 1; last; }
      }
      push @out, "mod\t$mod\t$name" unless $elsewhere;
    }
  }

  # --- type: types named nowhere but their declaration
  my %count;
  for my $t (values %text) { $count{$_}++ for $t =~ /\b([A-Za-z_]\w*)\b/g; }
  for my $f (sort grep { /\.cs$/ } keys %text) {
    next if $f =~ m{/Generated/|^base_game/(AnimationComponentRuntime|SpriteSheetRuntime)/};
    while ($text{$f} =~ /^[ \t]*(?:(?:public|internal|private|protected|static|sealed|abstract|partial|readonly|unsafe|file)\s+)*(?:class|struct|enum|interface|record|delegate)\s+(?:[\w<>\[\],.?]+\s+(?=\w+\s*[(<;]))?(\w+)/mg) {
      my $name = $1;
      next if ($count{$name} // 0) > 1;
      next if $name =~ /^(\w+)Attribute$/ && ($count{$1} // 0) > 0;
      push @out, "type\t$f\t$name";
    }
  }

  # --- setting: declared but never read, or never registered
  my %qualified;   # "Class.Member" -> files that name it
  for my $f (grep { /\.cs$/ } keys %text) {
    my %here; $here{"$1.$2"} = 1 while $text{$f} =~ /\b(\w+)\s*\.\s*(\w+)\b/g;
    $qualified{$_}{$f} = 1 for keys %here;
  }
  for my $f (sort grep { m{^mods/[^/]+\.cs$|^base_game/UnclaimedWorld/UWGame/Mods/PortSettings\.cs$} } keys %text) {
    my $t = $text{$f};
    my ($class) = $t =~ /\bstatic\s+(?:partial\s+)?class\s+(\w+)/ or next;
    my ($reg) = $t =~ /void\s+RegisterSettings\s*\(\s*\)\s*\{(.*?)\n    \}/s;
    $reg //= "";
    for my $name ($t =~ /static\s+(?:readonly\s+)?ModSetting\s+(\w+)\s*(?:=>|=|\{)/g) {
      my $own = () = $t =~ /\b\Q$name\E\b/g;
      my $inreg = () = $reg =~ /\b\Q$name\E\b/g;
      my $outside = grep { $_ ne $f } keys %{ $qualified{"$class.$name"} // {} };
      push @out, "setting\t$f\t$name.unread" if $own - 1 - $inreg + $outside <= 0;
      push @out, "setting\t$f\t$name.unregistered" if $inreg == 0;
    }
  }

  # --- symbol: defined and never tested, or tested and never defined
  my (%defined, %tested);
  for my $p (grep { /\.(csproj|props|targets)$/ } keys %text) {
    while ($text{$p} =~ /<DefineConstants[^>]*>([^<]*)</g) {
      $defined{$_} = $p for grep { /^\w+$/ } split /\s*;\s*/, $1;
    }
  }
  for my $f (grep { /\.cs$/ } keys %text) {
    while ($text{$f} =~ /^[ \t]*#\s*(?:el)?if\b([^\n]*)/mg) {
      my $cond = $1; $cond =~ s{//.*}{};
      $tested{$_} //= $f for grep { !/^(true|false)$/ } $cond =~ /\b([A-Za-z_]\w*)\b/g;
    }
  }
  my $sdk = qr/^(DEBUG|TRACE|RELEASE|NET\w*|NETCOREAPP\w*|NETSTANDARD\w*|NETFRAMEWORK\w*|WINDOWS\w*|LINUX|OSX|MACOS\w*|ANDROID\w*|IOS\w*)$/;
  push @out, "symbol\t$defined{$_}\t$_" for grep { $_ !~ $sdk && !$tested{$_} } keys %defined;
  push @out, "symbol\t$tested{$_}\t$_" for grep { $_ !~ $sdk && !exists $defined{$_} } keys %tested;

  # --- item: project entries naming files that do not exist
  for my $p (grep { /\.(csproj|props|targets)$/ } sort keys %text) {
    (my $dir = $p) =~ s{/?[^/]*$}{};
    while ($text{$p} =~ /<(?:Compile|None|Content|EmbeddedResource)\s+(?:Remove|Include|Update)="([^"]+)"/g) {
      my $path = $1;
      next if $path =~ /[*;]/;
      $path =~ s/\$\(MSBuildThisFileDirectory\)//g;
      next if $path =~ /\$\(/;
      $path =~ tr{\\}{/};
      my $full = length $dir ? "$dir/$path" : $path;
      push @out, "item\t$p\t$path" unless -e $full;
    }
  }

  # --- script: tools/build files nobody mentions
  for my $s (grep { m{^tools/build/[^/]+$} } @files) {
    (my $base = $s) =~ s{.*/}{};
    my $found = 0;
    for my $f (keys %text) {
      next if $f eq $s;
      if (index($text{$f}, $base) >= 0) { $found = 1; last; }
    }
    push @out, "script\t$s\t-" unless $found;
  }

  my %seen; print map { "$_\n" } grep { !$seen{$_}++ } sort @out;
' "$WORK/files" > "$WORK/found.txt"

tr -d '\r' < "$ALLOW" | grep -vE '^[[:space:]]*(#|$)' | sed 's/[[:space:]]*#.*$//' |
  awk '{ print $1 "\t" $2 "\t" $3 }' | sort -u > "$WORK/allowed.txt"
sort -u "$WORK/found.txt" -o "$WORK/found.txt"

comm -23 "$WORK/found.txt" "$WORK/allowed.txt" > "$WORK/new.txt"
comm -13 "$WORK/found.txt" "$WORK/allowed.txt" > "$WORK/stale.txt"

FAIL=0
if [ -s "$WORK/new.txt" ]; then
  echo
  echo "  FAIL  nothing uses these (remove them, or add them to $ALLOW with a reason):"
  sed 's/^/        /' "$WORK/new.txt"
  FAIL=1
fi
if [ -s "$WORK/stale.txt" ]; then
  echo
  echo "  FAIL  allowlist entries that no longer match anything (delete them):"
  sed 's/^/        /' "$WORK/stale.txt"
  FAIL=1
fi
[ $FAIL -eq 0 ] || exit 1
echo "no orphans - $(wc -l < "$WORK/found.txt" | tr -d ' ') finding(s), all allowlisted."
