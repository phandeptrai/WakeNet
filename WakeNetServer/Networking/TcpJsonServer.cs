using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using WakeNetServer.Protocol.Dtos;

namespace WakeNetServer.Networking;

public sealed class TcpJsonServer : IDisposable
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<string, ClientSession> _clients = new();

    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoop;
    private Task? _cleanupLoop;

    public IReadOnlyCollection<ClientSession> Clients => _clients.Values.ToArray();

    public event Action<ClientSession>? ClientUpserted;
    public event Action<string>? ClientRemoved;

    public bool IsRunning => _listener is not null;

    public void Start(IPAddress bindIp, int port, TimeSpan clientTimeout)
    {
        if (_listener is not null) return;

        _cts = new CancellationTokenSource();
        _listener = new TcpListener(bindIp, port);
        _listener.Start();

        _acceptLoop = Task.Run(() => AcceptLoopAsync(_cts.Token));
        _cleanupLoop = Task.Run(() => CleanupLoopAsync(clientTimeout, _cts.Token));
    }

    public async Task StopAsync()
    {
        var cts = _cts;
        _cts = null;

        try { cts?.Cancel(); } catch { }
        try { _listener?.Stop(); } catch { }
        _listener = null;

        try { if (_acceptLoop is not null) await _acceptLoop.ConfigureAwait(false); } catch { }
        try { if (_cleanupLoop is not null) await _cleanupLoop.ConfigureAwait(false); } catch { }

        foreach (var kv in _clients)
        {
            try { kv.Value.TcpClient.Close(); } catch { }
        }
        _clients.Clear();
    }

    public async Task SendCommandAsync(string clientId, string action, CancellationToken ct = default)
    {
        if (!_clients.TryGetValue(clientId, out var session))
            throw new InvalidOperationException("Client not found.");

        var stream = session.TcpClient.GetStream();
        var json = JsonSerializer.Serialize(new CommandMessage { action = action }, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json + "\n");
        await stream.WriteAsync(bytes, 0, bytes.Length, ct).ConfigureAwait(false);
        await stream.FlushAsync(ct).ConfigureAwait(false);
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener is not null)
        {
            TcpClient tcp;
            try
            {
                tcp = await _listener.AcceptTcpClientAsync(ct).ConfigureAwait(false);
            }
            catch
            {
                if (!ct.IsCancellationRequested) await Task.Delay(250, ct).ConfigureAwait(false);
                continue;
            }

            _ = Task.Run(() => HandleClientAsync(tcp, ct), ct);
        }
    }

    private async Task HandleClientAsync(TcpClient tcp, CancellationToken ct)
    {
        string? clientIdForCleanup = null;
        try
        {
            using var stream = tcp.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);

            while (!ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct).ConfigureAwait(false);
                if (line is null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;

                using var doc = JsonDocument.Parse(line);
                if (!doc.RootElement.TryGetProperty("type", out var typeProp)) continue;
                var type = typeProp.GetString() ?? "";

                if (string.Equals(type, "register", StringComparison.OrdinalIgnoreCase))
                {
                    var msg = JsonSerializer.Deserialize<RegisterMessage>(line, _jsonOptions);
                    if (msg?.clientId is null) continue;

                    clientIdForCleanup = msg.clientId;
                    var session = _clients.AddOrUpdate(
                        msg.clientId,
                        _ => new ClientSession
                        {
                            ClientId = msg.clientId,
                            Hostname = msg.hostname ?? "",
                            Ip = msg.ip ?? "",
                            Mac = msg.mac ?? "",
                            Os = msg.os ?? "",
                            TcpClient = tcp,
                            LastSeenUtc = DateTime.UtcNow
                        },
                        (_, existing) =>
                        {
                            // If a client reconnects with the same ClientId, replace the TCP connection.
                            if (!ReferenceEquals(existing.TcpClient, tcp))
                            {
                                try { existing.TcpClient.Close(); } catch { }
                                existing.TcpClient = tcp;
                            }

                            existing.Hostname = msg.hostname ?? existing.Hostname;
                            existing.Ip = msg.ip ?? existing.Ip;
                            existing.Mac = msg.mac ?? existing.Mac;
                            existing.Os = msg.os ?? existing.Os;
                            existing.LastSeenUtc = DateTime.UtcNow;
                            return existing;
                        });

                    session.LastSeenUtc = DateTime.UtcNow;
                    ClientUpserted?.Invoke(session);
                    continue;
                }

                if (string.Equals(type, "heartbeat", StringComparison.OrdinalIgnoreCase))
                {
                    var msg = JsonSerializer.Deserialize<HeartbeatMessage>(line, _jsonOptions);
                    if (msg?.clientId is null) continue;
                    clientIdForCleanup = msg.clientId;

                    if (_clients.TryGetValue(msg.clientId, out var session))
                    {
                        session.LastSeenUtc = DateTime.UtcNow;
                        ClientUpserted?.Invoke(session);
                    }
                }
            }
        }
        catch
        {
            // treat as disconnect
        }
        finally
        {
            try { tcp.Close(); } catch { }
            if (clientIdForCleanup is not null)
            {
                if (_clients.TryGetValue(clientIdForCleanup, out var existing) &&
                    ReferenceEquals(existing.TcpClient, tcp))
                {
                    _clients.TryRemove(clientIdForCleanup, out _);
                    ClientRemoved?.Invoke(clientIdForCleanup);
                }
            }
        }
    }

    private async Task CleanupLoopAsync(TimeSpan timeout, CancellationToken ct)
    {
        var interval = TimeSpan.FromSeconds(5);
        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(interval, ct).ConfigureAwait(false); } catch { }
            if (ct.IsCancellationRequested) break;

            var now = DateTime.UtcNow;
            foreach (var kv in _clients)
            {
                var session = kv.Value;
                if (now - session.LastSeenUtc <= timeout) continue;

                if (_clients.TryRemove(kv.Key, out var removed))
                {
                    try { removed.TcpClient.Close(); } catch { }
                    ClientRemoved?.Invoke(kv.Key);
                }
            }
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }
}

