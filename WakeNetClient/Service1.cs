using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using WakeNetClient.Infrastructure;

namespace WakeNetClient
{
    public partial class Service1 : ServiceBase
    {
        private CancellationTokenSource _cts;
        private Task _runner;
        private ClientServiceRunner _serviceRunner;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _cts = new CancellationTokenSource();
            _serviceRunner = new ClientServiceRunner(FileLogger.CreateDefault());
            _runner = Task.Run(() => _serviceRunner.RunAsync(_cts.Token));
        }

        protected override void OnStop()
        {
            try { _cts.Cancel(); } catch { }
            try { _runner?.Wait(TimeSpan.FromSeconds(10)); } catch { }
        }
    }
}
