using System;
using System.IO;

namespace WakeNetClient.Infrastructure
{
    internal sealed class ClientIdentityStore
    {
        private readonly string _path;

        public ClientIdentityStore(string path)
        {
            _path = path;
        }

        public static ClientIdentityStore CreateDefault()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WakeNetClient");
            Directory.CreateDirectory(dir);
            return new ClientIdentityStore(Path.Combine(dir, "clientid.txt"));
        }

        public string GetOrCreateClientId()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var existing = (File.ReadAllText(_path) ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(existing)) return existing;
                }
            }
            catch
            {
                // If we cannot read, we still generate a new id and try write later.
            }

            var id = Guid.NewGuid().ToString("N");
            try { File.WriteAllText(_path, id); } catch { }
            return id;
        }
    }
}

