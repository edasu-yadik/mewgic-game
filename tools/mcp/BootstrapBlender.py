"""Open a new temporary Mewgic-only Blender scene and a loopback MCP bridge."""
import pathlib
import uuid
import bpy

# The launcher starts an independent process with factory settings.
# Do not load or modify any of the user's existing Blender projects.
project = pathlib.Path(__file__).resolve().parents[2]
temp = project / "Temp" / "MCP"
temp.mkdir(parents=True, exist_ok=True)
path = temp / ("Mewgic_MCP_" + uuid.uuid4().hex[:12] + ".blend")
bpy.ops.wm.read_homefile(use_empty=True, use_factory_startup=True)
module = "bl_ext.user_default.mcp"
bpy.ops.preferences.addon_enable(module=module)
prefs = bpy.context.preferences.addons[module].preferences
prefs.host = "127.0.0.1"
prefs.port = 9876
prefs.use_autostart = False
bpy.context.scene.name = "Mewgic_MCP_Temporary_Test"
bpy.ops.wm.save_as_mainfile(filepath=str(path), check_existing=True)
bpy.ops.blmcp.server_start()
print("MEWGIC_BLENDER_MCP_READY " + str(path), flush=True)
