GL PROBE - what this machine's OpenGL driver does with the game's shaders
=========================================================================

DOUBLE-CLICK run.cmd

That is the whole of it. The window stays open at the end, and the same output is
written to contentprobe.log beside it - send that file back.

If your Steam library is somewhere unusual, drag your "Unclaimed World\Content"
folder onto run.cmd instead, or run:

    run.cmd "D:\wherever\Unclaimed World\Content"

Needs the .NET 8 runtime, same as the game:
    https://dotnet.microsoft.com/download/dotnet/8.0


IF NOTHING HAPPENS AT ALL
-------------------------

Running contentprobe.exe directly by double-click opens a console window, prints
into it and closes it again immediately - which looks exactly like "no output".
run.cmd exists to stop that; use it.

If run.cmd also prints nothing and leaves no contentprobe.log, the .NET 8 runtime
is almost certainly missing. Install it and try again.


WHAT THIS DOES
--------------

It loads every one of the game's 33 assets through a real graphics device, exactly
as the game does, and reports which ones the driver refuses - plus what the driver
is and what it allows. It draws nothing and writes nothing but its own log. A
window appears for a moment because an OpenGL context needs one.

Run it on a machine where the game fails to load a map with

    Failed to compile vertex shader <unknown>. See <unknown>.


WHAT IT IS SEPARATING
---------------------

Two explanations for the same crash, and one run tells them apart:

  1. NOT ENOUGH UNIFORM SPACE. skinFX's vertex shader needs 1048 float components.
     If MAX_VERTEX_UNIFORM_COMPONENTS is below that, the shader cannot fit on this
     driver, and fixing the GLSL dialect would not help - the fix is to pack the
     shader's constants tighter.

  2. THE GLSL DIALECT. The shader compiler warns (SD0403) that the GLSL it emits
     for that same shader contains an unsigned integer type and a non-square
     matrix - GLSL 1.30 constructs that the versionless dialect MonoGame uses
     cannot declare. Permissive drivers accept them anyway; strict ones reject the
     shader. If the limits are ample and skinFX still FAILs, this is it.

The probe prints which one it thinks it is. The raw numbers matter more than its
opinion, so send the log rather than a summary.

ONE FAILURE IS EXPECTED AND IS NOT THE PROBLEM
----------------------------------------------

    FAIL  Music/... -> Song    Could not load the specified container!

That is the music, not a shader. This probe reads your game's Content folder
directly, where the songs are .wma - and a DesktopGL build has no MediaFoundation
to play those with. The shipped game does not hit it, because the release carries
the songs converted to Ogg. Ignore this line; it says nothing about the driver.


A third possibility the probe now covers: that no graphics device can be created
here at all. The game asks for the HiDef profile; if that fails the probe retries
with Reach and says so. Failing both is a finding in itself - it would mean the
shaders were never even reached.
