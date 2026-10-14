# Exports one action of the colonist rig (make_rig.py) as an FBX animation clip, the way an
# animator would from Blender: the rig and its mesh, one baked take.
#
#   blender -b <colonist_rig.blend> --python export_clip.py -- <action> <out.fbx>
import bpy, sys

action_name, out = sys.argv[sys.argv.index("--") + 1:][:2]
rig = bpy.data.objects["colonist_rig"]
action = bpy.data.actions[action_name]
rig.animation_data.action = action
start, end = action.frame_range
scene = bpy.context.scene
scene.frame_start, scene.frame_end = int(start), int(end)
for o in bpy.data.objects:
    o.select_set(o == rig or o.parent == rig)
bpy.ops.export_scene.fbx(
    filepath=out,
    use_selection=True,
    object_types={'ARMATURE', 'MESH'},
    apply_scale_options='FBX_SCALE_ALL',
    # The default axis conversion (Z up -> Y up) lands on the armature's node and cancels the
    # +90 degrees make_rig.py stood it up with: the node leaves as identity and the root bone's
    # keys stay relative to the game's unrotated scene root. bake_space_transform would undo that.
    add_leaf_bones=False,
    bake_anim=True,
    bake_anim_use_all_actions=False,
    bake_anim_use_nla_strips=False,
    bake_anim_force_startend_keying=True,
    bake_anim_simplify_factor=0.0,
)
print("WROTE", out, action_name, int(start), int(end))
