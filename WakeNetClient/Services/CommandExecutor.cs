using System;
using System.Diagnostics;

namespace WakeNetClient.Services
{
    internal static class CommandExecutor
    {
        public static void Execute(string action)
        {
            action = (action ?? string.Empty).Trim().ToLowerInvariant();

            if (action == "shutdown")
            {
                StartShutdownProcess("/s /t 0");
                return;
            }

            if (action == "restart")
            {
                StartShutdownProcess("/r /t 0");
                return;
            }
        }

        private static void StartShutdownProcess(string args)
        {
            // Using shutdown.exe is the simplest way for Windows Service to request power actions.
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
    }
}

