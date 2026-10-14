# Compares one animation clip between two contentprobe --export-rig files: the studio's, and the
# same clip after a trip through Blender (make_rig.py, export_clip.py) and the model pipeline.
# Every key of the studio clip is compared with the rebuilt clip at the same time, interpolated
# the way the game interpolates (slerp / lerp), because the processor drops keys it can rebuild.
#
#   python compare_clip.py <studio.rig.json> <clip> <rebuilt.rig.json> <clip> [max degrees] [max units] [all]
#
# "all" compares the mirrored hand attachment too: right for a clip read back out of man_idle,
# where UWGame.Port.ExtraClips has put the studio's key back.
#
# Run by tools/build/38-build-models.sh --selftest on Blender's own Python (no other dependency).
# Exit code: the number of channels over tolerance.
import json, math, sys

args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else sys.argv[1:]
studio_path, studio_clip, built_path, built_clip = args[:4]
MAX_DEG = float(args[4]) if len(args) > 4 else 0.5
MAX_UNITS = float(args[5]) if len(args) > 5 else 0.05
ALL = len(args) > 6 and args[6] == "all"

studio = json.load(open(studio_path))
built = json.load(open(built_path))
a = studio["clips"][studio_clip]
b = built["clips"][built_clip]

# A bone every exported studio clip holds at one and the same key - the hand attachments. The
# right one is mirrored (scale (1, -1, 1)); Blender's FBX loses a mirror, so the game must keep the
# studio's key for these when a new clip is added (docs/modding.md, "New colonist animations").
constant = set()
for bone in a["channels"]:
    keys = [tuple(round(x, 4) for x in k[1:]) for clip in studio["clips"].values() for k in clip["channels"].get(bone, [])]
    if keys and len(set(keys)) == 1 and any(v < 0 for v in keys[0][7:10]):
        constant.add(bone)


def at(keys, t):
    prev = keys[0]
    for k in keys:
        if k[0] >= t:
            if k[0] == t or k is keys[0]:
                return k
            f = (t - prev[0]) / (k[0] - prev[0])
            sign = -1 if sum(prev[i] * k[i] for i in range(1, 5)) < 0 else 1
            q = [prev[i] + (sign * k[i] - prev[i]) * f for i in range(1, 5)]
            n = math.sqrt(sum(x * x for x in q))
            return [t] + [x / n for x in q] + [prev[i] + (k[i] - prev[i]) * f for i in range(5, 11)]
        prev = k
    return keys[-1]


failures = 0
print("  duration %.3f s / %.3f s, start offset %.4f s / %.4f s, gait %s / %s" % (
    a["durationTicks"] / 1e7, b["durationTicks"] / 1e7, a["startOffsetTicks"] / 1e7, b["startOffsetTicks"] / 1e7, a["isGait"], b["isGait"]))
if abs(a["durationTicks"] - b["durationTicks"]) > 1000 or abs(a["startOffsetTicks"] - b["startOffsetTicks"]) > 1000 or a["isGait"] != b["isGait"]:
    print("  FAIL  duration, start offset or gait flag differ")
    failures += 1
worst = (0.0, 0.0, None)
for bone, keys in a["channels"].items():
    if bone in constant and not ALL:
        print("  note  %s: held at one mirrored key in every studio clip - kept from the studio's at load, not compared" % bone)
        continue
    other = b["channels"].get(bone)
    if not other:
        print("  FAIL  %s: no channel in the rebuilt clip" % bone)
        failures += 1
        continue
    deg = units = scale = 0.0
    for k in keys:
        o = at(other, k[0])
        dot = abs(sum(k[i] * o[i] for i in range(1, 5)))
        deg = max(deg, math.degrees(2 * math.acos(min(1.0, dot))))
        units = max(units, max(abs(k[i] - o[i]) for i in range(5, 8)))
        scale = max(scale, max(abs(k[i] - o[i]) for i in range(8, 11)))
    if deg > MAX_DEG or units > MAX_UNITS or scale > 0.001:
        print("  FAIL  %s: %.3f degrees, %.4f units, scale %.4f" % (bone, deg, units, scale))
        failures += 1
    if deg > worst[0]:
        worst = (deg, units, bone)
print("  %d channels compared; worst %.3f degrees (%s); tolerance %.2f degrees, %.3f units" % (
    len(a["channels"]) - (0 if ALL else len(constant)), worst[0], worst[2], MAX_DEG, MAX_UNITS))
sys.exit(failures)
