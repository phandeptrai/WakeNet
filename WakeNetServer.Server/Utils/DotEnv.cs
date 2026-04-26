// Đọc file .env (KEY=VALUE) và set vào Environment variables.
namespace WakeNetServer.Server.Utils;

internal static class DotEnv
{
    public static string? LoadNearest(params string[] startDirectories)
    {
        foreach (var start in startDirectories.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
            var loaded = LoadNearestFrom(start);
            if (loaded is not null) return loaded;
        }

        return null;
    }

    private static string? LoadNearestFrom(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);
        if (!dir.Exists) return null;

        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate))
            {
                Load(candidate);
                NormalizePaths(candidate);
                return candidate;
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static void Load(string filePath)
    {
        if (!File.Exists(filePath)) return;

        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith('#')) continue;

            var idx = line.IndexOf('=');
            if (idx <= 0) continue;

            var key = line[..idx].Trim();
            var value = line[(idx + 1)..].Trim().Trim('"');
            if (key.Length == 0) continue;

            // Không override nếu đã có env var
            if (Environment.GetEnvironmentVariable(key) is not null) continue;

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static void NormalizePaths(string envFilePath)
    {
        // Cho phép cấu hình path tương đối trong .env để chạy được trên máy khác.
        var baseDir = Path.GetDirectoryName(envFilePath) ?? Directory.GetCurrentDirectory();

        NormalizeOne("WAKENET_DATA_DIR", baseDir);
        NormalizeOne("WAKENET_SQLITE_PATH", baseDir);
    }

    private static void NormalizeOne(string key, string baseDir)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value)) return;
        if (Path.IsPathRooted(value)) return;

        var absolute = Path.GetFullPath(Path.Combine(baseDir, value));
        Environment.SetEnvironmentVariable(key, absolute);
    }
}

