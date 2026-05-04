using System;
using System.IO;
using System.Text;

namespace WakeNetClient.Infrastructure
{
    internal sealed class FileLogger
    {
        private readonly string _path;
        private readonly object _gate = new object();

        public FileLogger(string path)
        {
            _path = path;
        }

        public static FileLogger CreateDefault()
        {
            var baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WakeNetClient",
                "Logs");
            Directory.CreateDirectory(baseDir);
            return new FileLogger(Path.Combine(baseDir, "service.log"));
        }

        public void Info(string message) => Write("INFO", message, null);
        public void Warn(string message) => Write("WARN", message, null);
        public void Error(string message, Exception ex) => Write("ERROR", message, ex);

        private void Write(string level, string message, Exception ex)
        {
            var line = new StringBuilder()
                .Append(DateTime.UtcNow.ToString("O"))
                .Append(" [").Append(level).Append("] ")
                .Append(message);

            if (ex != null)
            {
                line.Append(" | ").Append(ex.GetType().Name).Append(": ").Append(ex.Message);
            }

            lock (_gate)
            {
                File.AppendAllText(_path, line.AppendLine().ToString(), Encoding.UTF8);
            }
        }
    }
}

