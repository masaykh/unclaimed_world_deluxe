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

    private const string Header = "# frame\tdraws\thash\tentityX\tentityY\tcamX\tcamY";

    private readonly string[] ring = new string[RingSize];

    private StreamWriter writer;

    private string[] recordedLines;

    private string folder;

    private int ringNext;

    private long drawsThisFrame;

    private long drawsTotal;

    private ulong hash = 14695981039346656037uL;

    private bool reportedDivergence;

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
        }
        catch (Exception)
        {
            recordedLines = null;
        }
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
        string line = string.Create(CultureInfo.InvariantCulture,
            $"{frameIndex}\t{drawsTotal}\t{hash:x16}\t{world.RepresentativeEntityLocation.X:R}\t{world.RepresentativeEntityLocation.Y:R}\t{world.MapWindowLocation.X:R}\t{world.MapWindowLocation.Y:R}");

        drawsThisFrame = 0;

        if (writer != null)
        {
            writer.WriteLine(line);
            // Flushed every 600 frames rather than every frame: a crashed session keeps all but
            // the last ten seconds, and the recording does not pay for a disk write per frame.
            if ((frameIndex % 600) == 0)
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
            sb.AppendLine($"    frame     {frameIndex}");
            sb.AppendLine($"    recorded  {expected}");
            sb.AppendLine($"    this run  {actual}");
            sb.AppendLine();
            sb.AppendLine("Columns: frame, total draws, hash of the draw-label sequence, the");
            sb.AppendLine("representative entity's X and Y, the camera's X and Y.");
            sb.AppendLine();
            sb.AppendLine("Read it like this:");
            sb.AppendLine("  draws differ      - the simulation asked for randomness a different");
            sb.AppendLine("                      number of times. A different decision was taken.");
            sb.AppendLine("  draws same, hash differs");
            sb.AppendLine("                    - the same number of draws from different call");
            sb.AppendLine("                      sites, so the order of work changed. Unordered");
            sb.AppendLine("                      iteration over a HashSet or Dictionary does this.");
            sb.AppendLine("  draws and hash same, positions differ");
            sb.AppendLine("                    - identical decisions, different arithmetic. THIS");
            sb.AppendLine("                      is the floating-point case, and it is the only");
            sb.AppendLine("                      one of the three that is.");
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
                sb.AppendLine("This replay folder has no " + FileName + ", so there was nothing to");
                sb.AppendLine("check the run against. A replay recorded before the trace existed is");
                sb.AppendLine("in this state - record a new one to get a verdict.");
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
                sb.AppendLine("MATCHED. This run was deterministic.");
                sb.AppendLine();
                sb.AppendLine("    frames compared   " + framesCompared);
                sb.AppendLine("    random draws      " + drawsTotal);
                sb.AppendLine();
                sb.AppendLine("Every draw in the same order from the same call sites, and the world");
                sb.AppendLine("in the same place at every frame.");
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
