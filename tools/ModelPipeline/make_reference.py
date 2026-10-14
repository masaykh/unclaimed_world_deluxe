# The scene a new model is made in: the studio's colonist, woman and MULE at true relative size,
# in metres, Y up as Z up. Run by tools/build/38-build-models.sh --reference, from OBJs that
# contentprobe --export-obj writes out of the game's own compiled models (Blender cannot open the
# studio's FBX 6.1 sources).
#
#   blender -b --factory-startup --python make_reference.py -- <dir with the OBJs> <out.blend>
import bpy, os, sys, mathutils

ref, out = sys.argv[sys.argv.index("--") + 1:][:2]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.scene.unit_settings.system = 'METRIC'
col = bpy.data.collections.new("REFERENCE (do not export)")
bpy.context.scene.collection.children.link(col)


def load(name, x, scale):
    before = set(bpy.data.objects)
    bpy.ops.wm.obj_import(filepath=os.path.join(ref, name + ".obj"))
    for o in [o for o in bpy.data.objects if o not in before]:
        o.location.x = x
        o.scale = (scale, scale, scale)
        for c in o.users_collection:
            c.objects.unlink(o)
        col.objects.link(o)
        o.hide_select = True
        o.name = "REF_" + name + "_" + o.name


def size(prefix):
    pts = [o.matrix_world @ mathutils.Vector(c) for o in col.objects if o.name.startswith(prefix) for c in o.bound_box]
    return tuple(max(p[i] for p in pts) - min(p[i] for p in pts) for i in range(3))


load("man_idle", 0.0, 1.0)            # humans are drawn at scale 1
load("woman_idle", 1.0, 1.0)
load("utility_vehicle", -2.5, 2.0)    # entity:utilityvehicle is drawn at ModelScale 2
bpy.context.view_layer.update()
print("colonist %.2f m tall; MULE %.2f x %.2f x %.2f m" % ((size("REF_man_idle")[2],) + size("REF_utility_vehicle")))
bpy.ops.wm.save_as_mainfile(filepath=out)
print("SAVED", out)
