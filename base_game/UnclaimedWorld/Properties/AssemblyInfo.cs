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
// Unclaimed World DELUXE 1.0. This is the Deluxe version number, not the studio's: the game it
// is built from is their 1.0.4.8.
//
// Changing it is safe, which is worth saying because two neighbouring assemblies are NOT. Every
// XNB records its ContentTypeReader by assembly-qualified name, so SpriteSheetRuntime and
// Xclna.Xna.Animationx86 must keep theirs exactly - but nothing names THIS assembly, and
// MonoGame's ContentTypeReaderManager.PrepareType strips the version from a reader name anyway.
//
// Saves are unaffected. SnapshotHeader writes ProgramVersion, but reading it back is
// `sn.Ignore(ProgramVersion)` - a literal no-op - and the only other use is a label on the load
// screen. A save made by 1.0.4.8 still loads, and shows the version it was made with.
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyProduct("Unclaimed World Deluxe")]
[assembly: AssemblyTitle("Unclaimed World Deluxe")]
[assembly: AssemblyVersion("1.0.0.0")]
