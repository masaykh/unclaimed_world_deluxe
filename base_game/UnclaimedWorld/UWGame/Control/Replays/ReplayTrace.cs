using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace UWGame.Control.Replays;

/// <summary>
/// PORT DEVIATION 20. A machine-readable record of what the simulation did, so that "the replay is
/// not deterministic" can be a measurement with a frame number on it instead of a belief.
///
/// WHAT WAS ALREADY HERE. The studio built all of this in outline and then took the middle out of
/// it. Every random draw in the game goes through RandomGenerator with a LABEL describing its call
/// site - "Common - RandomBetween", "GoalEat - pick food" - and each one calls
/// Controller.SaveOrVerifyRandomGet(label), which was an empty method. Recorder declared a
/// randomWriter and a stateWriter, closed them in StopRecording, and never opened or wrote to
/// either; OpenNewReplayFiles computed the paths "RandomCalls.UWRepRand" and "AIStates.UWRepStates"
/// and discarded the strings. Replayer.GetCurrentSavedRandomGet returned null.
///
/// So the instrument that would have localised a divergence was the one piece missing, and
/// "possibly because of drifting floating point values" is the explanation you settle on when you
/// have no instrument. That guess is worth testing rather than inheriting.
///
/// WHAT THIS RECORDS, and why it is not simply every draw. A long session draws millions of times;
/// writing each one would produce a file nobody opens and slow the recording enough to change what
/// it is recording. Instead, per FRAME:
///
///     frame  draws  hash  entityX entityY  camX camY
///
/// `hash` is a running FNV-1a over the sequence of draw LABELS. Two runs that make the same
/// decisions in the same order produce the same hash; one that takes a different branch does not,
/// at the first frame where it differs. The world columns are the studio's own
/// ReplayVerificationData - the representative entity's position and the camera's - which catch a
/// divergence that somehow consumed the same randomness.
///
/// One line per frame, tab-separated: greppable, diffable, and about 8 MB for an hour at 60fps.
///
/// AND WHEN IT DIFFERS, the last 64 labels are still in a ring buffer, so the report names the
/// draws around the break rather than only the frame. That is the difference between "it diverged"
/// and "it diverged at GoalEat - pick food, four draws after the frame began".
/// </summary>
public sealed class ReplayTrace : IDisposable
{
    /// <summary>The name the studio chose for this file. Kept, so an old replay folder reads.</summary>
    public const string FileName = "RandomCalls.UWRepRand";

    /// <summary>Where a replay writes what it found. Absent means nothing diverged.</summary>
    public const string DivergenceFileName = "Divergence.txt";

    private const int RingSize = 64;

    // simDays is the SIMULATION'S OWN CLOCK, and it is here because of what the first real
    // divergence turned out to be. Draws and hash matched and only the position differed, which by
    // the legend in Divergence.txt reads as a floating-point difference - but laid against the
    // recorded series the replay was simply four frames AHEAD in its movement, which is a
    // different fault entirely. With the sim clock in the line, "the same decisions over a
    // different amount of game time" can be told apart from "the same decisions over the same
    // time, different arithmetic" without decoding Replay.UWRep by hand.
    private const string Header = "# frame\tdraws\thash\tentityX\tentityY\tcamX\tcamY\tsimDays";

    private readonly string[] ring = new string[RingSize];

    private StreamWriter writer;

    private string[] recordedLines;

    private string folder;

    private int ringNext;

    private long drawsThisFrame;

    private long drawsTotal;

    private ulong hash = 14695981039346656037uL;

    private bool reportedDivergence;

    /// <summary>Set when the recorded trace was written by a build with different columns.</summary>
    private bool wrongFormat;

    /// <summary>Whether a comparison has already failed. Once true, nothing more is reported.</summary>
    public bool Diverged => reportedDivergence;

    // ---- recording -----------------------------------------------------------------------

    public void BeginRecording(string replayFolder)
    {
        folder = replayFolder;
        try
        {
            writer = new StreamWriter(Path.Combine(replayFolder, FileName), append: false)
            {
                AutoFlush = false
            };
            writer.WriteLine(Header);
        }
        catch (Exception)
        {
            // A trace that cannot be written must not stop a game from being recorded. The replay
            // itself is still perfectly usable; only the divergence report is lost.
            writer = null;
        }
    }

    // ---- replaying -----------------------------------------------------------------------

    public void BeginComparing(string replayFolder)
    {
        folder = replayFolder;
        try
        {
            string path = Path.Combine(replayFolder, FileName);
            recordedLines = File.Exists(path) ? File.ReadAllLines(path) : null;
            // A trace written by an older build has different columns, and comparing against it
            // would report a divergence on the first frame - a change in this file read as a
            // change in the simulation. Refused outright, which comes out as NOT COMPARED.
            if (recordedLines != null
                && (recordedLines.Length == 0
                    || !string.Equals(recordedLines[0], Header, StringComparison.Ordinal)))
            {
                recordedLines = null;
                wrongFormat = true;
            }
            DropTruncatedLastLine();
        }
        catch (Exception)
        {
            recordedLines = null;
        }
    }

    /// <summary>
    /// Throws away a final line that was cut in half.
    ///
    /// The trace is flushed on an interval, so a session that ended by having its window closed,
    /// or by crashing, can leave a partial last line behind. Compared as it stands it is unequal
    /// to anything, and the replay reports a divergence at the exact frame where the file simply
    /// stops - a fault in the recording read as a fault in the simulation, which is the most
    /// misleading answer this can give.
    /// </summary>
    private void DropTruncatedLastLine()
    {
        if (recordedLines == null || recordedLines.Length == 0)
        {
            return;
        }
        int last = recordedLines.Length - 1;
        if (CountFields(recordedLines[last]) != FieldsPerLine)
        {
            var trimmed = new string[last];
            Array.Copy(recordedLines, trimmed, last);
            recordedLines = trimmed;
        }
    }

    /// <summary>frame, draws, hash, entityX, entityY, camX, camY, simDays.</summary>
    private const int FieldsPerLine = 8;

    private static int CountFields(string line)
    {
        if (line == null)
        {
            return 0;
        }
        int fields = 1;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\t')
            {
                fields++;
            }
        }
        return fields;
    }

    /// <summary>Whether there is a recorded trace to compare this replay against.</summary>
    public bool HasRecording => recordedLines != null && recordedLines.Length > 1;

    // ---- the draw hook -------------------------------------------------------------------

    /// <summary>
    /// One random draw, by the label its call site passed. Called from
    /// Controller.SaveOrVerifyRandomGet, which every RandomGenerator method routes through.
    ///
    /// Deliberately cheap: a hash update and a ring write, no allocation, no formatting. This runs
    /// millions of times in a session and anything heavier would change the thing it measures.
    /// </summary>
    public void Draw(string label)
    {
        drawsThisFrame++;
        drawsTotal++;

        string text = label ?? string.Empty;
        for (int i = 0; i < text.Length; i++)
        {
            hash ^= text[i];
            hash *= 1099511628211uL;
        }
        // The ordinal matters as much as the label: the same call site reached a different number
        // of times is a divergence, and hashing the label alone would miss it.
        hash ^= (ulong)drawsTotal;
        hash *= 1099511628211uL;

        ring[ringNext] = text;
        ringNext = (ringNext + 1) % RingSize;
    }

    // ---- per frame -----------------------------------------------------------------------

    /// <summary>
    /// Closes off a frame: writes its line when recording, compares it when replaying.
    /// Returns false when replaying and this frame did not match what was recorded.
    /// </summary>
    public bool EndFrame(int frameIndex, ReplayVerificationData world)
    {
        // The simulation's own clock, not the frame's wall time: the replay substitutes the
        // recorded GameTime, so comparing that would compare a number against itself. What can
        // differ, and did, is how far the simulation actually advanced.
        double simDays = UWGame.The.Sim?.DateAndTime == null
            ? 0.0
            : UWGame.The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays;

        string line = string.Create(CultureInfo.InvariantCulture,
            $"{frameIndex}\t{drawsTotal}\t{hash:x16}\t{world.RepresentativeEntityLocation.X:R}\t{world.RepresentativeEntityLocation.Y:R}\t{world.MapWindowLocation.X:R}\t{world.MapWindowLocation.Y:R}\t{simDays:R}");

        drawsThisFrame = 0;

        if (writer != null)
        {
            writer.WriteLine(line);
            // Flushed every 60 frames rather than every frame: a session that dies keeps all but
            // the last second, and the recording does not pay for a disk write per frame. It was
            // 600, which is ten seconds - too much to lose, and the window in which a half-written
            // final line reads as a divergence.
            if ((frameIndex % 60) == 0)
            {
                writer.Flush();
            }
            return true;
        }

        if (recordedLines == null || reportedDivergence)
        {
            return true;
        }

        // +1 for the header line.
        int index = frameIndex + 1;
        if (index >= recordedLines.Length)
        {
            return true;
        }
        if (string.Equals(recordedLines[index], line, StringComparison.Ordinal))
        {
            framesCompared++;
            return true;
        }

        ReportDivergence(frameIndex, recordedLines[index], line);
        return false;
    }

    /// <summary>
    /// Writes Divergence.txt and stops comparing. One report per replay: everything after the
    /// first break is a consequence of it, and a file full of consequences hides the cause.
    /// </summary>
    private void ReportDivergence(int frameIndex, string expected, string actual)
    {
        reportedDivergence = true;
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("The replay diverged from what was recorded.").AppendLine();
            if (frameIndex == 0)
            {
                sb.AppendLine("ON FRAME ZERO, which is not drift of any kind. The replay's first");
                sb.AppendLine("frame is meant to be the scenario as it starts, the same as the");
                sb.AppendLine("recording's first frame. If the clocks below are far apart, the");
                sb.AppendLine("world this replay ran on was NOT rebuilt from GameParams.xml - it");
                sb.AppendLine("was the previous game's world, already hours further on. Check");
                sb.AppendLine("Errors.txt for 'Stale Sim reused'.");
                sb.AppendLine();
            }
            sb.AppendLine($"    frame     {frameIndex}");
            sb.AppendLine($"    recorded  {expected}");
            sb.AppendLine($"    this run  {actual}");
            sb.AppendLine();
            sb.AppendLine("Columns: frame, total draws, hash of the draw-label sequence, the");
            sb.AppendLine("representative entity's X and Y, the camera's X and Y, and the");
            sb.AppendLine("simulation's own clock in days.");
            sb.AppendLine();
            sb.AppendLine("Read it like this:");
            sb.AppendLine("  draws differ      - the simulation asked for randomness a different");
            sb.AppendLine("                      number of times. A different decision was taken.");
            sb.AppendLine("  draws same, hash differs");
            sb.AppendLine("                    - the same number of draws from different call");
            sb.AppendLine("                      sites, so the order of work changed. Unordered");
            sb.AppendLine("                      iteration over a HashSet or Dictionary does this.");
            sb.AppendLine("  draws and hash same, simDays differs");
            sb.AppendLine("                    - the same decisions over a DIFFERENT AMOUNT OF GAME");
            sb.AppendLine("                      TIME. The positions will differ too and it will");
            sb.AppendLine("                      look like arithmetic; it is not. Read the clock");
            sb.AppendLine("                      column first.");
            sb.AppendLine("  draws, hash and simDays all same, positions differ");
            sb.AppendLine("                    - identical decisions over identical time, different");
            sb.AppendLine("                      arithmetic. THIS is the floating-point case, and");
            sb.AppendLine("                      it is the only one of the four that is.");
            sb.AppendLine();
            sb.AppendLine($"The last {RingSize} draw labels before the break, oldest first:");
            for (int i = 0; i < RingSize; i++)
            {
                string label = ring[(ringNext + i) % RingSize];
                if (!string.IsNullOrEmpty(label))
                {
                    sb.Append("    ").AppendLine(label);
                }
            }
            File.WriteAllText(Path.Combine(folder ?? ".", DivergenceFileName), sb.ToString());
        }
        catch (Exception)
        {
            // Reporting must never be the thing that takes the run down.
        }
    }

    /// <summary>
    /// Reports a divergence that the STUDIO'S OWN check found and the draw trace did not.
    ///
    /// The two look at different things and are recorded through different channels.
    /// ReplayVerificationData is written into Replay.UWRep frame by frame and compares the
    /// representative entity and the camera with a tolerance; this trace is written into
    /// RandomCalls.UWRepRand and compares the random-draw count, a hash of the draw labels, and
    /// the same two positions exactly. Either can fail alone.
    ///
    /// Without this, that case threw an exception telling the reader to look in Divergence.txt -
    /// a file nothing had written. Reported and observed on the first replay anyone has ever
    /// played back.
    /// </summary>
    public void ReportStateMismatch(int frameIndex, ReplayVerificationData recorded, ReplayVerificationData live)
    {
        if (reportedDivergence)
        {
            return;
        }
        reportedDivergence = true;
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("The replay diverged from what was recorded.").AppendLine();
            sb.AppendLine("Found by the game's own ReplayVerificationData check, NOT by the draw");
            sb.AppendLine("trace - so at this frame the simulation asked for randomness exactly as");
            sb.AppendLine("many times as it did when recording, in the same order, and the two");
            sb.AppendLine("channels still disagree about where things are.").AppendLine();
            sb.AppendLine($"    frame              {frameIndex}");
            sb.AppendLine($"    recorded entity    {Format(recorded.RepresentativeEntityLocation.X)}, {Format(recorded.RepresentativeEntityLocation.Y)}");
            sb.AppendLine($"    this run  entity    {Format(live.RepresentativeEntityLocation.X)}, {Format(live.RepresentativeEntityLocation.Y)}");
            sb.AppendLine($"    recorded camera    {Format(recorded.MapWindowLocation.X)}, {Format(recorded.MapWindowLocation.Y)}");
            sb.AppendLine($"    this run  camera    {Format(live.MapWindowLocation.X)}, {Format(live.MapWindowLocation.Y)}");
            sb.AppendLine();
            sb.AppendLine("If only the CAMERA differs, the simulation is fine and the view is not:");
            sb.AppendLine("the camera is client state, driven by the window size and by scrolling,");
            sb.AppendLine("and Verify only checks it outside CommandMode.");
            sb.AppendLine();
            sb.AppendLine($"Frames that matched before this one: {framesCompared}");
            sb.AppendLine();
            sb.AppendLine($"The last {RingSize} draw labels before the break, oldest first:");
            for (int i = 0; i < RingSize; i++)
            {
                string label = ring[(ringNext + i) % RingSize];
                if (!string.IsNullOrEmpty(label))
                {
                    sb.Append("    ").AppendLine(label);
                }
            }
            File.WriteAllText(Path.Combine(folder ?? ".", DivergenceFileName), sb.ToString());
        }
        catch (Exception)
        {
            // Reporting must never be the thing that takes the run down.
        }
    }

    private static string Format(float value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    /// <summary>The name of the file a finished replay leaves behind, matched or not.</summary>
    public const string VerdictFileName = "ReplayVerdict.txt";

    private int framesCompared;
    /// <summary>
    /// Writes the verdict of a finished replay.
    ///
    /// A replay that ran to the end used to say nothing, which made "it matched" and "it was never
    /// compared" the same observable outcome - and the second is what happens when there is no
    /// recorded trace to compare against. Saying which is the whole value of running one.
    /// </summary>
    public void Finish()
    {
        if (folder == null)
        {
            return;
        }
        try
        {
            var sb = new StringBuilder();
            if (!HasRecording)
            {
                sb.AppendLine("NOT COMPARED.");
                sb.AppendLine();
                if (wrongFormat)
                {
                    sb.AppendLine("This recording's " + FileName + " was written by an earlier build");
                    sb.AppendLine("and does not have the same columns, so there was nothing this run");
                    sb.AppendLine("could honestly be checked against. Record a new session.");
                }
                else
                {
                    sb.AppendLine("This replay folder has no " + FileName + ", so there was nothing to");
                    sb.AppendLine("check the run against. A replay recorded before the trace existed is");
                    sb.AppendLine("in this state - record a new one to get a verdict.");
                }
            }
            else if (reportedDivergence)
            {
                sb.AppendLine("DIVERGED.");
                sb.AppendLine();
                sb.AppendLine("See " + DivergenceFileName + " beside this file for the frame it");
                sb.AppendLine("happened on and the draws around it.");
            }
            else
            {
                // A replay that ran past the end of the recorded trace was never compared over
                // the tail, and saying MATCHED for frames nobody looked at is the one answer this
                // file must never give. The usual cause is a session that ended by having its
                // window closed before the trace's last buffer reached disk.
                int recordedFrames = recordedLines.Length - 1;
                if (framesCompared < recordedFrames)
                {
                    sb.AppendLine("MATCHED, AS FAR AS IT WENT.");
                    sb.AppendLine();
                    sb.AppendLine("    frames compared   " + framesCompared);
                    sb.AppendLine("    frames recorded   " + recordedFrames);
                    sb.AppendLine("    random draws      " + drawsTotal);
                    sb.AppendLine();
                    sb.AppendLine("Every frame that was compared agreed. The replay stopped before");
                    sb.AppendLine("the end of the recording, so the rest is unmeasured rather than");
                    sb.AppendLine("known to be good.");
                }
                else
                {
                    sb.AppendLine("MATCHED. This run was deterministic.");
                    sb.AppendLine();
                    sb.AppendLine("    frames compared   " + framesCompared);
                    sb.AppendLine("    random draws      " + drawsTotal);
                    sb.AppendLine();
                    sb.AppendLine("Every draw in the same order from the same call sites, and the");
                    sb.AppendLine("world in the same place at every frame.");
                }
            }
            File.WriteAllText(Path.Combine(folder, VerdictFileName), sb.ToString());
        }
        catch (Exception)
        {
            // A verdict that cannot be written is not worth taking the run down for.
        }
    }


    public void Dispose()
    {
        try
        {
            writer?.Flush();
            writer?.Dispose();
        }
        catch (Exception)
        {
        }
        writer = null;
    }
}
