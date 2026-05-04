using System.ServiceProcess;
using System;
using System.Linq;
using System.Threading;
using WakeNetClient.Infrastructure;

namespace WakeNetClient
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var forceConsole = args != null && args.Any(a => string.Equals(a, "--console", StringComparison.OrdinalIgnoreCase));

            if (forceConsole || Environment.UserInteractive)
            {
                RunConsole(args);
                return;
            }

            ServiceBase[] ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }

        private static void RunConsole(string[] args)
        {
            var logger = FileLogger.CreateDefault();
            logger.Info("Starting in CONSOLE mode.");

            Console.WriteLine("WakeNetClient (console debug mode)");
            Console.WriteLine("Press Ctrl+C to stop.");

            using (var cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                var runner = new ClientServiceRunner(logger);
                try
                {
                    runner.RunAsync(cts.Token).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.Error("Console runner crashed.", ex);
                    Console.Error.WriteLine(ex);
                }
            }
        }
    }
}
