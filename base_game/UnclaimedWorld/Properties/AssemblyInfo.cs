using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyCopyright("Copyright © 2016")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("02eed97a-5aad-446e-aadb-8926a88abd1c")]
// Unclaimed World DELUXE 1.3. This is the Deluxe version number, not the studio's: the game it
// is built from is their 1.0.4.8.
//
// THIS IS WHAT THE GAME DISPLAYS. UnclaimedWorld.GetVersion() reads the AssemblyVersion back off
// the running assembly, so this line is the main menu, the fatal-error title, the save/load
// panel's "current version" notes and the replay header. It sat at 1.0 through the v1.1, v1.2
// and v1.3 tags, so three releases shipped in archives named for a version the game denied.
//
// It cannot drift again silently: tools/build/70-make-release.sh refuses to cut a release whose
// version disagrees with this line. Bump it in the same commit as the tag.
//
// MSBuild cannot set it - base_game/Directory.Build.props sets GenerateAssemblyInfo=false, so
// -p:Version has no effect here and this attribute is the only source.
//
// Changing it is safe, which is worth saying because two neighbouring assemblies are NOT. Every
// XNB records its ContentTypeReader by assembly-qualified name, so SpriteSheetRuntime and
// Xclna.Xna.Animationx86 must keep theirs exactly - but nothing names THIS assembly, and
// MonoGame's ContentTypeReaderManager.PrepareType strips the version from a reader name anyway.
//
// Saves are unaffected. SnapshotHeader writes ProgramVersion, but reading it back is
// `sn.Ignore(ProgramVersion)` - a literal no-op - and the only other use is a label on the load
// screen. A save made by 1.0.4.8 still loads, and shows the version it was made with.
[assembly: AssemblyFileVersion("1.3.0.0")]
[assembly: AssemblyProduct("Unclaimed World Deluxe")]
[assembly: AssemblyTitle("Unclaimed World Deluxe")]
[assembly: AssemblyVersion("1.3.0.0")]
