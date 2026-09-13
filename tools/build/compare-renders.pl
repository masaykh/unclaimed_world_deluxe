use strict;
use warnings;

# Compares two directories of raw RGBA frames written by tools/EffectRender.
#
# Reports, per pass: the mean and maximum absolute channel difference, and the share of pixels
# that differ at all. Exact equality is not the bar - two rasterisers and two shader compilers
# will not agree bit for bit on interpolated or transcendental results - so the verdict bands
# are deliberately coarse and the numbers are printed so the judgement stays visible rather
# than hidden behind a pass/fail.
#
# It also flags the case that looks like agreement and proves nothing: both frames untouched.
# A shader that draws nothing on both backends matches perfectly and has been tested for
# nothing at all.

my ($dxDir, $glDir) = @ARGV;
die "usage: compare-renders.pl <dx-dir> <gl-dir>\n" unless defined $glDir;

opendir(my $dh, $dxDir) or die "$dxDir: $!";
my @frames = sort grep { /\.rgba$/ } readdir($dh);
closedir $dh;

my $clear = chr(64) . chr(64) . chr(64) . chr(255);

my ($identical, $close, $differing, $missing, $blankBoth, $fewPixels) = (0, 0, 0, 0, 0, 0);
my @report;

for my $frame (@frames) {
    my $dxPath = "$dxDir/$frame";
    my $glPath = "$glDir/$frame";

    unless (-e $glPath) {
        $missing++;
        next;
    }

    my $dx = slurp($dxPath);
    my $gl = slurp($glPath);

    if (length($dx) != length($gl)) {
        push @report, sprintf("  SIZE  %-46s %d vs %d bytes", label($frame), length($dx), length($gl));
        $differing++;
        next;
    }

    my $pixels = length($dx) / 4;
    my ($sum, $max, $ndiff) = (0, 0, 0);
    my $dxBlank = 1;
    my $glBlank = 1;

    for (my $i = 0; $i < length($dx); $i += 4) {
        my $a = substr($dx, $i, 4);
        my $b = substr($gl, $i, 4);
        $dxBlank = 0 if $a ne $clear;
        $glBlank = 0 if $b ne $clear;
        next if $a eq $b;

        $ndiff++;
        for my $c (0 .. 3) {
            my $d = abs(ord(substr($a, $c, 1)) - ord(substr($b, $c, 1)));
            $sum += $d;
            $max = $d if $d > $max;
        }
    }

    my $mean = $pixels ? $sum / ($pixels * 4) : 0;
    my $pctDiff = $pixels ? 100 * $ndiff / $pixels : 0;

    my $verdict;
    if ($dxBlank && $glBlank) {
        $verdict = "BLANK-BOTH";
        $blankBoth++;
    }
    elsif ($ndiff == 0) {
        $verdict = "identical";
        $identical++;
    }
    elsif ($max <= 2 && $mean < 0.05) {
        $verdict = "match";
        $close++;
    }
    elsif ($max <= 16 && $mean < 1.0) {
        $verdict = "close";
        $close++;
    }
    # A handful of isolated pixels is a different finding from a frame that is wrong all over,
    # and the max alone cannot tell them apart: GUI/CRT differs on three pixels out of 16384 by
    # as much as 143, which by max lands beside RoundLine differing on half the frame. Called out
    # separately rather than folded into either - it is not agreement, but it does not mean the
    # shader computes the wrong thing.
    elsif ($pctDiff < 0.5) {
        $verdict = "few-px";
        $fewPixels++;
    }
    else {
        $verdict = "DIFFERS";
        $differing++;
    }

    push @report, sprintf("  %-11s %-46s mean=%.3f max=%3d pixels=%.1f%% (%d of %d)",
                          $verdict, label($frame), $mean, $max, $pctDiff, $ndiff, $pixels);
}

print "$_\n" for @report;
print "\n";
printf "%d identical, %d within tolerance, %d differing on a few pixels, %d differing, " .
       "%d blank on both, %d not rendered on GL.\n",
       $identical, $close, $fewPixels, $differing, $blankBoth, $missing;

if ($blankBoth) {
    print "\nNote: a pass blank on both backends agrees trivially and validates nothing -\n";
    print "its shader may simply not draw with these inputs.\n";
}

exit($differing > 0 ? 1 : 0);

sub slurp {
    my ($p) = @_;
    open my $fh, '<:raw', $p or die "$p: $!";
    local $/;
    my $d = <$fh>;
    close $fh;
    return $d;
}

sub label {
    my ($f) = @_;
    $f =~ s/\.rgba$//;
    return $f;
}
