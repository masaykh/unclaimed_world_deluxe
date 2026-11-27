using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.Port;

/// <summary>
/// PORT FIX: a model part with no texture drew in flat red (Lars, Discord 2026-11-26: "why the
/// cockpit glass is rendered a flat red? It is supposed to use a glass shader").
///
/// The skimmer's canopy and windshield use the material front_glass (skimmer.fbx: DiffuseColor
/// 0,0,0, Opacity 0.62, no texture). VehicleMaterialProcessor keeps only texture, specular and
/// emissive, so in skimmer.xnb that part is a Vehicle effect with BasicTexture unset. Vehicle.fx
/// samples BasicTexture anyway and reads its alpha as a colour-replace mask: below 0.25 it paints
/// ReplaceColor3, which Renderable passes as Color.Red.
///
/// Under the studio's XNA / Direct3D 9, sampling an empty slot returned (0,0,0,1): alpha 1, no
/// replacement, so the glass drew black with the environment-map reflection and specular on top -
/// the "glass" look. Direct3D 11 returns (0,0,0,0) for an empty slot (MonoGame binds null,
/// TextureCollection), so alpha 0 picked the red. Binding an opaque black texel restores the
/// studio's result on every backend. There is no glass shader in the studio's source to restore.
///
/// Not seen on screen: confirmed from the shader, the model and the draw code only.
/// </summary>
public static class UntexturedModelPart
{
    private static Texture2D opaqueBlack;

    /// <summary>Binds an opaque black 1x1 texture to <paramref name="effect"/>'s BasicTexture when it has none.</summary>
    public static void FillEmptyBasicTexture(Effect effect, GraphicsDevice device)
    {
        EffectParameter parameter = effect?.Parameters["BasicTexture"];
        if (parameter == null || device == null || parameter.GetValueTexture2D() != null)
        {
            return;
        }
        if (opaqueBlack == null || opaqueBlack.IsDisposed || opaqueBlack.GraphicsDevice != device)
        {
            opaqueBlack = new Texture2D(device, 1, 1);
            opaqueBlack.SetData(new[] { Color.Black });
        }
        parameter.SetValue(opaqueBlack);
    }
}
