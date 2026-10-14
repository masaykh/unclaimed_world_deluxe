# The colonist's skeleton, skinned mesh and chosen studio clips as a Blender scene, from what the
# game holds in memory (contentprobe --export-rig). Blender cannot open the studio's FBX 6.1
# sources, so a new colonist animation is made on this rig.
#
#   blender -b --factory-startup --python make_rig.py -- <rig.json> <out.blend> [units per metre]
#
# EXACTNESS FIRST. Every bone keeps the game's own local frame, so a key set in Blender is the key
# the game plays: rest = the bind pose (equal to the bones' stored transforms in man_idle), and a
# clip key K becomes the pose bone's basis rest_local^-1 * K. The game's bones point along their
# local X (Maya's convention); Blender draws a bone along Y - so the bones look sideways here, and
# nothing about that changes what is exported.
#
# Coordinates: the game is Y up, its matrices XNA row-major for row vectors; Blender is Z up with
# column vectors. The rig is built IN THE GAME'S OWN SPACE - bones, mesh and keys exactly as the
# game holds them, translations divided by the units per metre (13.75, as
# tools/build/38-build-models.sh builds) - and only the armature OBJECT is turned +90 degrees
# about X, so it stands up in Blender. Blender's FBX export turns the scene back from Z up to Y up
# on that same object, the two cancel, and the armature node leaves as identity: every bone-local
# key, the root's included, is the game's. (Turning each bone instead looks the same in Blender
# and comes back with the axes of every key swapped.)
import bpy, json, math, sys
from mathutils import Matrix, Quaternion, Vector

args = sys.argv[sys.argv.index("--") + 1:]
rig_path, out = args[0], args[1]
UNITS = float(args[2]) if len(args) > 2 else 13.75
TICKS_PER_FRAME = 333333  # the clips are resampled at 30 fps (AnimatedModelProcessor.sampleDistance)

with open(rig_path) as f:
    rig = json.load(f)

C = Matrix.Rotation(math.radians(90.0), 4, 'X')   # Y up -> Z up: (x, y, z) -> (x, -z, y)


def scaled(m):
    m = m.copy()
    m.translation = m.translation / UNITS
    return m


def decompose(m):
    """loc, rotation, scale of m - keeping a mirror as a negative scale on Y, which the studio's
    hand attachments carry (AttachorPalmRight is keyed with scale (1, -1, 1) in every clip)."""
    if m.determinant() < 0:
        loc, rot, sc = (m @ Matrix.Diagonal((1.0, -1.0, 1.0, 1.0))).decompose()
        return loc, rot, Vector((sc.x, -sc.y, sc.z))
    return m.decompose()


def xna(a):
    return Matrix((a[0:4], a[4:8], a[8:12], a[12:16])).transposed()


bones = {b["name"]: b for b in rig["bones"]}
mesh_bones = {m["parentBone"] for m in rig["meshes"]}
# The skeleton: every bone below the scene root that is not a mesh's node.
skeleton = [b["name"] for b in rig["bones"] if b["parent"] is not None and b["name"] not in mesh_bones]

game_global = {}


def global_of(name):
    if name not in game_global:
        b = bones[name]
        local = xna(b["local"])
        game_global[name] = local if b["parent"] is None else global_of(b["parent"]) @ local
    return game_global[name]


bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.render.fps = 30

arm = bpy.data.armatures.new("colonist")
arm.display_type = 'STICK'
rig_obj = bpy.data.objects.new("colonist_rig", arm)
rig_obj.matrix_world = C   # stands the game's Y-up space up in Blender; see the top
scene.collection.objects.link(rig_obj)
bpy.context.view_layer.objects.active = rig_obj
bpy.ops.object.mode_set(mode='EDIT')
rest = {}
for name in skeleton:
    g = scaled(global_of(name))
    rest[name] = g
    eb = arm.edit_bones.new(name)
    eb.head = (0, 0, 0)
    eb.tail = (0, 0.04, 0)
    eb.matrix = g
    parent = bones[name]["parent"]
    if parent in skeleton:
        eb.parent = arm.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT')

# The skinned mesh, in bind pose, weighted by palette index -> bone name.
for skin, m in zip(rig["skins"], rig["meshes"]):
    palette = {s["palette"]: s["bone"] for s in skin}
    p = m["positions"]
    verts = [Vector(p[i:i + 3]) / UNITS for i in range(0, len(p), 3)]
    t = m["triangles"]
    # XNA's front faces wind clockwise; Blender's counter-clockwise.
    faces = [(t[i], t[i + 2], t[i + 1]) for i in range(0, len(t), 3)]
    me = bpy.data.meshes.new(m["name"] or "colonist")
    me.from_pydata(verts, [], faces)
    if m["uvs"]:
        uv = me.uv_layers.new(name="UVMap")
        for poly in me.polygons:
            for li in poly.loop_indices:
                vi = me.loops[li].vertex_index
                uv.data[li].uv = (m["uvs"][2 * vi], 1.0 - m["uvs"][2 * vi + 1])
    obj = bpy.data.objects.new(m["name"] or "colonist", me)
    scene.collection.objects.link(obj)
    groups = {}
    for v in range(len(verts)):
        for k in range(4):
            w = m["blendWeights"][4 * v + k]
            if w > 0:
                bone = palette[m["blendIndices"][4 * v + k]]
                if bone not in groups:
                    groups[bone] = obj.vertex_groups.new(name=bone)
                groups[bone].add([v], w, 'ADD')
    obj.parent = rig_obj
    mod = obj.modifiers.new("Armature", 'ARMATURE')
    mod.object = rig_obj

def _fcurves(action):
    """An action's F-curves: Blender 4.4+ keeps them in layers / strips / channelbags."""
    if hasattr(action, "layers") and action.layers:
        for layer in action.layers:
            for strip in layer.strips:
                for bag in strip.channelbags:
                    yield from bag.fcurves
    else:
        yield from getattr(action, "fcurves", [])


# The studio's clips, as actions on the rig: basis = rest_local^-1 * key_local.
rig_obj.animation_data_create()
rig_obj.rotation_mode = 'QUATERNION'
for pb in rig_obj.pose.bones:
    pb.rotation_mode = 'QUATERNION'
for clip_name, clip in rig["clips"].items():
    action = bpy.data.actions.new(clip_name)
    action.use_fake_user = True
    rig_obj.animation_data.action = action
    for bone_name, keys in clip["channels"].items():
        if bone_name not in rest:
            continue
        parent = bones[bone_name]["parent"]
        rest_local = (rest[parent].inverted() @ rest[bone_name]) if parent in rest else rest[bone_name]
        pb = rig_obj.pose.bones[bone_name]
        for k in keys:
            q = Quaternion((k[4], k[1], k[2], k[3]))
            local = Matrix.Translation(Vector(k[5:8])) @ q.to_matrix().to_4x4() @ Matrix.Diagonal(Vector((k[8], k[9], k[10], 1.0)))
            basis = rest_local.inverted() @ scaled(local)
            loc, rot, scale = decompose(basis)
            frame = k[0] / TICKS_PER_FRAME
            pb.location, pb.rotation_quaternion, pb.scale = loc, rot, scale
            pb.keyframe_insert("location", frame=frame)
            pb.keyframe_insert("rotation_quaternion", frame=frame)
            pb.keyframe_insert("scale", frame=frame)
    # The game interpolates linearly between keys; so must Blender, or a re-baked clip drifts.
    for fc in _fcurves(action):
        for kp in fc.keyframe_points:
            kp.interpolation = 'LINEAR'
    print("CLIP", clip_name, len(clip["channels"]), "bones")

bpy.context.view_layer.update()
body = bpy.data.objects[rig["meshes"][0]["name"] or "colonist"]
mn = min((body.matrix_world @ v.co).z for v in body.data.vertices)
mx = max((body.matrix_world @ v.co).z for v in body.data.vertices)
print("colonist %.2f m tall in the rest pose, %d bones" % (mx - mn, len(skeleton)))
bpy.ops.wm.save_as_mainfile(filepath=out)
print("SAVED", out)
