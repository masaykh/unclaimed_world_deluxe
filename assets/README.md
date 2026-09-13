# assets

Asset **sources** — what a human edits. Refactored Games released these under the Unclaimed World
Community License, so they are here rather than in your Steam folder.

| | |
|---|---|
| **`Content/`** | the studio's asset sources, as released: 3343 `.png`, 269 `.tga`, 247 `.fbx`, 235 `.wav`, 48 `.mp3`, 9 `.spritefont`, and `Content.mgcb` |
| **`WindowSystem-Content/`** | the UI layer's own sources — its fonts and button sounds, which live in a separate project |
| **`effects/`** | the shader sources **the build actually compiles** — see below |
| **`MainMenuIntro.uwanim`** | the menu background, a motion-JPEG frame sequence made from the studio's `TauCetiMainMenu.wmv` |

## `effects/` and `Content/*.fx` are both here, and they are not the same file

This trips people, so it is worth stating plainly:

```
assets/Content/Billboard.fx    the studio's original   compile vs_4_0 / ps_4_0
assets/effects/Billboard.fx    what this build uses    compile vs_3_0 / ps_3_0
```

Both DesktopGL's MojoShader dialect and FNA's cap at **Shader Model 3**, so the techniques'
`compile` profiles are downgraded. That is not an invention: the studio's own sources carry
`compile vs_4_0 /*vs_3_0*/` — SM3 was the earlier state and they left it in the comment.

`tools/build/34-build-gl-effects-shadowdusk.sh` reads `effects/`. `Content/*.fx` is kept as the
reference: the thing to diff against when a shader question comes up, and the thing to go back to
if the runtime ever supports SM4.

Three of the effects in `effects/` are **recovered** rather than shipped — `BloomCombine`,
`BloomExtract` and `GaussianBlur`, from Microsoft's XNA Bloom sample, which is where the studio
took them from.

## Building them

Only the shaders are built today, by `tools/build/34-build-gl-effects-shadowdusk.sh`. Everything
else in `Content/` is compiled by MGCB, the MonoGame Content Builder, driven by `Content.mgcb` and
three custom processors that live in the studio's own pipeline projects.

That build has been run and mostly works — **425 of 425** items in `Content.mgcb`, **30 of 30** in
the WindowSystem project. What does not is the models:

```
Build 3 succeeded, 47 failed.
Assimp.AssimpException: FBX-DOM unsupported, old format version,
                        supported are only FBX 2011, FBX 2012 and FBX 2013
```

The 47 are **ASCII FBX 6.1** (`FBXVersion: 6100`), and no version of Assimp reads that — so MGCB
cannot either. Until they are converted or re-exported, a from-source content build cannot produce
a complete game, which is why the release archives ship the compiled `Content/` instead.
