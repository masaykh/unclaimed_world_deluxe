using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UWGame.Port;

/// <summary>
/// PORT: finds a file whose name differs only in letter case from the one asked for, on a
/// case-sensitive file system.
///
/// The studio developed on Windows, where <c>Fonts/CRT_18pt</c> opens <c>Fonts/crt_18pt.xnb</c>
/// and <c>billboard</c> opens <c>Billboard.xnb</c>. On Linux (and a case-sensitive macOS volume)
/// neither opens, and both are loaded before the main menu, so the game could not start there
/// (Kastuk, testing the GL build on Ubuntu: "call for file.xnb as File.xnb will cannot be find";
/// he got past it by mounting Content/ and data/ through ciopfs). The files are the user's own
/// copy of the game, so the names are matched in code rather than the files renamed.
///
/// Only ever a fallback: <see cref="Resolve"/> returns the path unchanged on Windows, and
/// anywhere the exact path exists. Otherwise each segment is looked up in its directory, exact
/// spelling first, then ignoring case. A directory's listing is cached; a name it does not hold
/// re-lists the directory once before giving up, so a file created after the listing (a save, a
/// new user scenario) is still found. Segments that match nothing are kept as given, so a path
/// for a file about to be written comes back with its existing folders resolved and its new
/// name intact.
/// </summary>
public sealed class CasePath
{
    /// <summary>The real file system, with no Windows check - <see cref="Resolve"/> adds that.</summary>
    public static CasePath Disk { get; } = new CasePath(ListDirectory);

    /// <summary>The names in a directory, or null when it cannot be listed.</summary>
    private readonly Func<string, string[]> list;

    private readonly ConcurrentDictionary<string, string[]> listings = new ConcurrentDictionary<string, string[]>(StringComparer.Ordinal);

    /// <param name="list">The names (not paths) in a directory, or null when there is no such directory.</param>
    public CasePath(Func<string, string[]> list)
    {
        this.list = list;
    }

    /// <summary>
    /// <paramref name="path"/> on Windows or when it exists as spelled; otherwise the same path
    /// with each segment spelled as it is on disk, as far as the segments can be found.
    /// </summary>
    public static string Resolve(string path)
    {
        if (OperatingSystem.IsWindows() || string.IsNullOrEmpty(path) || File.Exists(path) || Directory.Exists(path))
        {
            return path;
        }
        return Disk.Find(path);
    }

    /// <summary>
    /// The case-insensitive lookup itself, with no Windows or exists check - what
    /// <see cref="Resolve"/> does once the exact path has missed. Both '/' and '\' separate
    /// segments: the studio wrote "Models\\x", and on Linux '\' is not a separator at all.
    /// Returns <paramref name="path"/> itself when every segment is already spelled as on disk
    /// and it has no '\'; otherwise the path rebuilt with '/' between its segments, which on
    /// Linux is a different path from the one with '\' in it.
    /// </summary>
    public string Find(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }
        string root = RootOf(path);
        string[] segments = path.Substring(root.Length).Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        var found = new StringBuilder(root.Replace('\\', '/'));
        bool changed = false;
        bool lost = false;
        foreach (string segment in segments)
        {
            string name = segment;
            if (!lost && segment != "." && segment != "..")
            {
                string directory = found.Length == 0 ? "." : found.ToString();
                string match = Match(directory, segment);
                if (match == null)
                {
                    // Nothing further down can be listed either: keep the rest as given.
                    lost = true;
                }
                else if (match != segment)
                {
                    name = match;
                    changed = true;
                }
            }
            if (found.Length > 0 && found[found.Length - 1] != '/')
            {
                found.Append('/');
            }
            found.Append(name);
        }
        return changed || path.IndexOf('\\') >= 0 ? found.ToString() : path;
    }

    /// <summary>The entry of <paramref name="directory"/> spelled <paramref name="name"/>, ignoring case if it must; null if none.</summary>
    private string Match(string directory, string name)
    {
        string match = Pick(Listing(directory, refresh: false), name);
        if (match == null)
        {
            match = Pick(Listing(directory, refresh: true), name);
        }
        return match;
    }

    private static string Pick(string[] entries, string name)
    {
        if (entries == null)
        {
            return null;
        }
        string best = null;
        foreach (string entry in entries)
        {
            if (string.Equals(entry, name, StringComparison.Ordinal))
            {
                return entry;
            }
            // Two entries can differ only in case here. Take the ordinal-first so the answer does
            // not depend on the order the file system lists them in.
            if (string.Equals(entry, name, StringComparison.OrdinalIgnoreCase)
                && (best == null || string.CompareOrdinal(entry, best) < 0))
            {
                best = entry;
            }
        }
        return best;
    }

    private string[] Listing(string directory, bool refresh)
    {
        if (!refresh && listings.TryGetValue(directory, out string[] cached))
        {
            return cached;
        }
        string[] entries = list(directory);
        if (entries == null)
        {
            listings.TryRemove(directory, out _);
        }
        else
        {
            listings[directory] = entries;
        }
        return entries;
    }

    /// <summary>"/", "C:\" or "" (relative) - the part of a path that is never looked up.</summary>
    private static string RootOf(string path)
    {
        if (path.Length >= 2 && path[1] == ':' && char.IsLetter(path[0]))
        {
            return path.Length >= 3 && (path[2] == '/' || path[2] == '\\') ? path.Substring(0, 3) : path.Substring(0, 2);
        }
        int slashes = 0;
        while (slashes < path.Length && (path[slashes] == '/' || path[slashes] == '\\'))
        {
            slashes++;
        }
        return path.Substring(0, slashes);
    }

    private static string[] ListDirectory(string directory)
    {
        try
        {
            var names = new List<string>();
            foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
            {
                names.Add(Path.GetFileName(entry));
            }
            return names.ToArray();
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
        {
            return null;
        }
    }
}
