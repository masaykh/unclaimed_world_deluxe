using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
/// One integer per frame, one per line, plain text so it can be read and diffed. A replay whose
/// folder has no such file falls back to the clock and behaves exactly as before.
/// </summary>
public sealed class CycleSchedule : IDisposable
{
    /// <summary>The file, beside Replay.UWRep.</summary>
    public const string FileName = "Cycles.UWRepCycles";

    private StreamWriter writer;

    private int[] recorded;

    /// <summary>Whether a schedule was found to replay against.</summary>
    public bool HasRecording => recorded != null && recorded.Length > 0;

    public void BeginRecording(string replayFolder)
    {
        try
        {
            writer = new StreamWriter(Path.Combine(replayFolder, FileName), append: false)
            {
                AutoFlush = false
            };
        }
        catch (Exception)
        {
            // A schedule that cannot be written costs a later replay its determinism and nothing
            // else. It must not stop a game from being recorded.
            writer = null;
        }
    }

    /// <summary>One frame's count. Flushed on the same interval as the draw trace.</summary>
    public void Record(int frameIndex, int cycles)
    {
        if (writer == null)
        {
            return;
        }
        writer.WriteLine(cycles.ToString(CultureInfo.InvariantCulture));
        if ((frameIndex % 60) == 0)
        {
            writer.Flush();
        }
    }

    public void BeginReplaying(string replayFolder)
    {
        try
        {
            string path = Path.Combine(replayFolder, FileName);
            if (!File.Exists(path))
            {
                recorded = null;
                return;
            }
            string[] lines = File.ReadAllLines(path);
            var values = new List<int>(lines.Length);
            foreach (string line in lines)
            {
                // A final line cut in half by a session that was killed is dropped rather than
                // read as a zero, which would stall the replay's work for that frame.
                if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
                {
                    values.Add(n);
                }
            }
            recorded = values.ToArray();
        }
        catch (Exception)
        {
            recorded = null;
        }
    }

    /// <summary>
    /// The number of cycles this frame must run, or null past the end of the recording - where
    /// the clock decides again, because there is nothing to match.
    /// </summary>
    public int? For(int frameIndex)
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
