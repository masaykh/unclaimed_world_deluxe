using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UWGame.Control.Replays;

/// <summary>
/// PORT DEVIATION 21. The work schedule of a recorded session: how many time-sliced AI cycles
/// each frame ran.
///
/// WHY IT HAS TO BE RECORDED. CycleManager gives the time-sliced systems a nine-millisecond
/// WALL-CLOCK budget per frame and runs CycleOnce until it is spent. Every cycle can create an
/// entity or update a sensor, and every one of those draws from the gameplay random stream. So
/// the number of draws in a frame is decided by how fast the machine ran that frame, and a
/// replay - which is not loading anything, and is therefore faster - gets ahead.
///
/// Measured on Fields of Tau Ceti - Cudgel Hills (L): at frame 5 the recording had made 6885
/// draws and the replay 10115, with the simulation clock identical to the last digit and the
/// representative entity in exactly the same place. The recording reached 10122 at frame 8. The
/// replay was three frames ahead in work, not wrong in its arithmetic.
///
/// One line per frame, plain text so it can be read and diffed. A replay whose folder has no such
/// file falls back to the clock and behaves exactly as before.
///
/// PORT DEVIATION 22. The first line names the simulation's step rate ("steps-per-second 60";
/// 0 is the studio's one variable step per frame), because a replay has to step as the
/// recording did. Each frame's line then holds one count per simulation update in that frame,
/// separated by spaces - none on a frame with no step. A file from before has no first line and
/// one count per frame, which is exactly one update a frame at the variable step.
/// </summary>
public sealed class CycleSchedule : IDisposable
{
    /// <summary>The file, beside Replay.UWRep.</summary>
    public const string FileName = "Cycles.UWRepCycles";

    /// <summary>PORT DEVIATION 22. The first line's key.</summary>
    private const string RateKey = "steps-per-second ";

    private StreamWriter writer;

    private int[][] recorded;

    /// <summary>Whether a schedule was found to replay against.</summary>
    public bool HasRecording => recorded != null && recorded.Length > 0;

    /// <summary>
    /// PORT DEVIATION 22. The step rate the replay was recorded at: 0 for the variable step, and for
    /// any recording made before there was another.
    /// </summary>
    public int StepsPerSecond { get; private set; }

    public void BeginRecording(string replayFolder, int stepsPerSecond)
    {
        try
        {
            writer = new StreamWriter(Path.Combine(replayFolder, FileName), append: false)
            {
                AutoFlush = false
            };
            writer.WriteLine(RateKey + stepsPerSecond.ToString(CultureInfo.InvariantCulture));
        }
        catch (Exception)
        {
            // A schedule that cannot be written costs a later replay its determinism and nothing
            // else. It must not stop a game from being recorded.
            writer = null;
        }
    }

    /// <summary>One frame's counts, one per update. Flushed on the same interval as the draw trace.</summary>
    public void Record(int frameIndex, IReadOnlyList<int> cycles)
    {
        if (writer == null)
        {
            return;
        }
        var line = new StringBuilder();
        for (int i = 0; i < cycles.Count; i++)
        {
            if (i > 0)
            {
                line.Append(' ');
            }
            line.Append(cycles[i].ToString(CultureInfo.InvariantCulture));
        }
        writer.WriteLine(line.ToString());
        if ((frameIndex % 60) == 0)
        {
            writer.Flush();
        }
    }

    public void BeginReplaying(string replayFolder)
    {
        StepsPerSecond = 0;
        try
        {
            string path = Path.Combine(replayFolder, FileName);
            if (!File.Exists(path))
            {
                recorded = null;
                return;
            }
            string[] lines = File.ReadAllLines(path);
            var values = new List<int[]>(lines.Length);
            int first = 0;
            if (lines.Length > 0 && lines[0].StartsWith(RateKey, StringComparison.Ordinal)
                && int.TryParse(lines[0].Substring(RateKey.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out int rate))
            {
                StepsPerSecond = rate;
                first = 1;
            }
            for (int i = first; i < lines.Length; i++)
            {
                // A final line cut in half by a session that was killed is dropped rather than
                // read as a zero, which would stall the replay's work for that frame.
                if (TryParseCounts(lines[i], out int[] counts))
                {
                    values.Add(counts);
                }
            }
            recorded = values.ToArray();
        }
        catch (Exception)
        {
            recorded = null;
        }
    }

    private static bool TryParseCounts(string line, out int[] counts)
    {
        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        counts = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out counts[i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// The number of cycles each update of this frame must run, or null past the end of the
    /// recording - where the clock decides again, because there is nothing to match.
    /// </summary>
    public int[] For(int frameIndex)
    {
        if (recorded == null || frameIndex < 0 || frameIndex >= recorded.Length)
        {
            return null;
        }
        return recorded[frameIndex];
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
