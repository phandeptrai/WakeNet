using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WakeNetClient.Config;
using WakeNetClient.Infrastructure;
using WakeNetClient.Networking;
using WakeNetClient.Protocol;
using WakeNetClient.Protocol.Dtos;
using WakeNetClient.Services;

namespace WakeNetClient
{
    internal sealed class ClientServiceRunner
    {
        private readonly FileLogger _logger;

        public ClientServiceRunner(FileLogger logger)
        {
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            var settings = ClientSettings.Load();
            var identityStore = ClientIdentityStore.CreateDefault();
            var clientId = identityStore.GetOrCreateClientId();

            _logger.Info("Runner started.");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var info = ClientInfoCollector.Collect(clientId);
                    using (var client = new LineDelimitedJsonClient())
                    {
                        _logger.Info($"Connecting to {settings.ServerHost}:{settings.ServerPort} ...");
                        await client.ConnectAsync(settings.ServerHost, settings.ServerPort, ct).ConfigureAwait(false);
                        _logger.Info("Connected.");

                        await client.SendAsync(new RegisterMessage
                        {
                            clientId = info.ClientId,
                            hostname = info.Hostname,
                            ip = info.Ip,
                            mac = info.Mac,
                            os = info.Os
                        }, ct).ConfigureAwait(false);

                        _logger.Info("Register sent.");

                        var heartbeatTask = settings.HeartbeatEnabled
                            ? Task.Run(() => HeartbeatLoopAsync(client, clientId, settings.HeartbeatInterval, ct))
                            : Task.CompletedTask;

                        await ReceiveLoopAsync(client, ct).ConfigureAwait(false);

                        try { await heartbeatTask.ConfigureAwait(false); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Connection loop error.", ex);
                }

                if (ct.IsCancellationRequested) break;
                _logger.Warn($"Disconnected. Reconnecting in {settings.ReconnectDelay.TotalSeconds:0}s ...");
                try { await Task.Delay(settings.ReconnectDelay, ct).ConfigureAwait(false); } catch { }
            }

            _logger.Info("Runner stopped.");
        }

        private async Task HeartbeatLoopAsync(LineDelimitedJsonClient client, string clientId, TimeSpan interval, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && client.IsConnected)
            {
                try
                {
                    await Task.Delay(interval, ct).ConfigureAwait(false);
                    if (ct.IsCancellationRequested || !client.IsConnected) break;

                    await client.SendAsync(new HeartbeatMessage { clientId = clientId }, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error("Heartbeat failed.", ex);
                    break;
                }
            }
        }

        private async Task ReceiveLoopAsync(LineDelimitedJsonClient client, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && client.IsConnected)
            {
                var line = await client.ReadLineAsync(ct).ConfigureAwait(false);
                if (line == null) break;

                line = line.Trim();
                if (line.Length == 0) continue;

                try
                {
                    var dict = JsonUtil.Deserialize<Dictionary<string, object>>(line);

                    object typeObj;
                    dict.TryGetValue("type", out typeObj);
                    var type = typeObj as string;
                    if (!string.Equals(type, "command", StringComparison.OrdinalIgnoreCase)) continue;

                    object actionObj;
                    dict.TryGetValue("action", out actionObj);
                    var action = actionObj as string;

                    _logger.Info($"Command received: {action}");
                    CommandExecutor.Execute(action);
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to parse/handle message.", ex);
                }
            }
        }
    }
}

