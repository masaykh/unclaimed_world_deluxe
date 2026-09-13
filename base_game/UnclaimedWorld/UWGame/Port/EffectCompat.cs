using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.Port;

/// <summary>
/// Setting an effect parameter that the effect might not declare.
///
/// WHY THIS IS NEEDED, AND ONLY HERE. The game reaches parameters by name and does not check -
/// <c>effect.Parameters["WindowPosition"].SetValue(...)</c> - which is correct against the
/// effects it shipped with. The OpenGL build cannot use those: they are profile 1 (DirectX_11)
/// and <c>Effect.ReadHeader</c> refuses a profile mismatch, so every effect is recompiled from
/// the studio's own HLSL. That rebuild is faithful but NARROWER, because the two compilers prune
/// differently: mgfxc's DirectX path keeps a uniform the shaders never read, and its OpenGL /
/// Shader Model 3 path drops it. Compiling the studio's unmodified sources for DirectX_11
/// reproduces the shipped parameter counts exactly (5, 10, 41, 19, 20, 19), so nothing about the
/// source or the rendering has changed - only which dead uniforms survive into the parameter
/// table.
///
/// Eight parameters across six effects are dropped that way. Three of them the game sets:
///
///   OverlayGroundSpriteEffect.WindowPosition    -> DrawOverlayGroundSprites
///   RoadsAndPaths.AlphaAdjustment               -> DrawGroundOutlineUserVertices
///   Vehicle.DirLight2SpecularColor              -> StiffAnimatedModel.Draw
///
/// and each was a NullReferenceException the first frame that draw ran. The other five
/// (Billboard's BillboardWidth/Height, water's xAmbient/xEnableLighting, multiTex's
/// xLightDirection) are never set, so they cost nothing.
///
/// SILENCE IS SAFE HERE, AND ONLY HERE, because the parameter is dead in the shader on BOTH
/// backends - the value was never read on DirectX either, so skipping the write changes nothing
/// that was ever drawn. What keeps that from becoming a way to hide real breakage is that the
/// set is not a matter of judgement: <c>mgfxtranscode compare-params game/Content
/// content/effects-gl</c> lists exactly which parameters each rebuilt effect lost, and
/// <c>build/31-build-gl-effects.sh</c> fails if one appears that is not accounted for here. Use
/// this only for a name that check has reported; a parameter that goes missing for any other
/// reason should still crash.
/// </summary>
public static class EffectCompat
{
    /// <summary>
    /// Looks a parameter up and tells <see cref="UWGame.Port.RenderTrace"/> whether it was there.
    ///
    /// Every setter below goes through this, so one render trace lists exactly which parameters
    /// this backend dropped on paths the game actually draws - which is the difference between
    /// "tolerated" and "silently lost".
    /// </summary>
    private static EffectParameter Find(Effect effect, string name)
    {
        EffectParameter parameter = effect?.Parameters[name];
        if (RenderTrace.Recording)
        {
            // Effects loaded as part of a Model carry no Name - the ModelMeshPart owns them and
            // nothing sets one - so fall back to the current technique, which identifies the
            // effect well enough to act on ("StandardRender" is Vehicle, "SkinnedRender" skinFX).
            string label = !string.IsNullOrEmpty(effect?.Name)
                ? effect.Name
                : "(model effect, technique " + (effect?.CurrentTechnique?.Name ?? "?") + ")";
            RenderTrace.Parameter(label, name, parameter != null);
        }
        return parameter;
    }

    /// <summary>Sets <paramref name="name"/> if the effect declares it; otherwise does nothing.</summary>
    public static void SetIfDeclared(Effect effect, string name, float value) =>
        Find(effect, name)?.SetValue(value);

    /// <inheritdoc cref="SetIfDeclared(Effect, string, float)"/>
    public static void SetIfDeclared(Effect effect, string name, Vector2 value) =>
        Find(effect, name)?.SetValue(value);

    /// <inheritdoc cref="SetIfDeclared(Effect, string, float)"/>
    public static void SetIfDeclared(Effect effect, string name, Vector3 value) =>
        Find(effect, name)?.SetValue(value);

    /// <inheritdoc cref="SetIfDeclared(Effect, string, float)"/>
    public static void SetIfDeclared(Effect effect, string name, Vector4 value) =>
        Find(effect, name)?.SetValue(value);

    /// <summary>
    /// A texture, for the FNA build.
    ///
    /// UNLIKE THE OTHERS, THIS ONE MIGHT LOSE SOMETHING REAL - it is not yet known.
    ///
    /// The parameters above are dead in the shader on every backend, so skipping the write
    /// changes nothing. <c>NormalMap</c> is less clear. ShadowDusk's FNA target does not expose it
    /// on multiTex or RoadsAndPaths while MonoGame's compiler does, and it LOOKS reachable -
    /// <c>SpriteShader.fxh</c> declares it, <c>ApplyLighting</c> samples it through
    /// <c>NormalMapSampler</c>, <c>RenderRocksPS</c> calls <c>ApplyLighting</c>, and the other
    /// uniforms from that same header are exposed. Against that: <c>texture3</c> and
    /// <c>texture4</c> are dropped too, which points at dead-code elimination rather than a lost
    /// binding, and two reduced repros failed to reproduce it.
    ///
    /// So this exists so the FNA build RUNS rather than dying with a NullReferenceException in
    /// GameWorldRenderer.DrawTerrainUsingBatchList, and whether anything is actually lost is an
    /// open question. See todo.md.
    /// </summary>
    public static void SetIfDeclared(Effect effect, string name, Texture value) =>
        Find(effect, name)?.SetValue(value);
}
