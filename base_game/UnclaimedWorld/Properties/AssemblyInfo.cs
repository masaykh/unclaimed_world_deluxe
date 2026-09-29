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
// Unclaimed World DELUXE 1.4. This is the Deluxe version number, not the studio's: the game it
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
// Saves DID depend on it, which this comment used to deny. SnapshotHeader's ProgramVersion is only
// a label, but a save names generic types with assembly-qualified arguments, version included -
// "UnclaimedWorld, Version=1.0.4.8" in every studio save - and .NET will not bind a newer version
// than the one loaded. At 1.0.0.0, Deluxe 1.0 to 1.3 loaded no vanilla save at all. Since then
// Snapshotter.ResolveSavedType matches the assembly by name alone, so this number no longer
// matters to a save in either direction (gate 80, case 22).
[assembly: AssemblyFileVersion("1.4.0.0")]
[assembly: AssemblyProduct("Unclaimed World Deluxe")]
[assembly: AssemblyTitle("Unclaimed World Deluxe")]
[assembly: AssemblyVersion("1.4.0.0")]
