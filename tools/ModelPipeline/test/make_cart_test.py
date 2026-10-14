# The stand-in hand cart tools/build/38-build-models.sh --selftest builds: made the way a new
# vehicle model should be (docs/modding.md, "New 3D models") - rigid meshes, the body named "hull",
# each wheel under its own joint, an empty for the driver, one 256x256 TGA - and exported the way
# Blender exports by default (binary FBX 7.4), in metres. Regenerate with:
#
#   blender -b --factory-startup --python make_cart_test.py -- <dir>/cart_test.fbx
import bpy, os, sys

out = sys.argv[sys.argv.index("--") + 1]
bpy.ops.wm.read_factory_settings(use_empty=True)

img = bpy.data.images.new("cart_base_256", 256, 256)
img.generated_color = (0.45, 0.3, 0.15, 1.0)
img.filepath_raw = os.path.join(os.path.dirname(out), "cart_base_256.tga")
img.file_format = 'TARGA'
img.save()
mat = bpy.data.materials.new("cartWood")
mat.use_nodes = True
tex = mat.node_tree.nodes.new("ShaderNodeTexImage")
tex.image = img
mat.node_tree.links.new(tex.outputs["Color"], mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"])

bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, 0.6))
hull = bpy.context.object
hull.name = "hull"
hull.scale = (1.2, 0.7, 0.3)
hull.data.materials.append(mat)
for side, y in (("left", 0.45), ("right", -0.45)):
    bpy.ops.object.empty_add(location=(0, y, 0.35))
    joint = bpy.context.object
    joint.name = f"wheel_rear_{side}_joint"
    bpy.ops.mesh.primitive_cylinder_add(radius=0.35, depth=0.08, location=(0, y, 0.35), rotation=(1.5708, 0, 0))
    wheel = bpy.context.object
    wheel.name = f"wheel_rear_{side}"
    wheel.data.materials.append(mat)
    wheel.parent = joint
    wheel.matrix_parent_inverse = joint.matrix_world.inverted()
bpy.ops.object.empty_add(location=(-1.0, 0, 0))
bpy.context.object.name = "DriversSeat"

for o in bpy.data.objects:
    if o.type == 'MESH':
        bpy.context.view_layer.objects.active = o
        o.select_set(True)
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project()
        bpy.ops.object.mode_set(mode='OBJECT')
        o.select_set(False)
bpy.ops.export_scene.fbx(filepath=out, path_mode='RELATIVE', apply_scale_options='FBX_SCALE_ALL', bake_anim=False)
print("WROTE", out)
