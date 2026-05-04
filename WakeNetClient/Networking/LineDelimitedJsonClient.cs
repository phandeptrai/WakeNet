using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WakeNetClient.Protocol;

namespace WakeNetClient.Networking
{
    internal sealed class LineDelimitedJsonClient : IDisposable
    {
        private TcpClient _tcp;
        private StreamReader _reader;
        private StreamWriter _writer;

        public bool IsConnected => _tcp != null && _tcp.Connected;

        public async Task ConnectAsync(string host, int port, CancellationToken ct)
        {
            Dispose();

            _tcp = new TcpClient();
            using (ct.Register(() => { try { _tcp.Close(); } catch { } }))
            {
                await _tcp.ConnectAsync(host, port).ConfigureAwait(false);
            }

            var stream = _tcp.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);
            _writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true,
                NewLine = "\n"
            };
        }

        public Task SendAsync(object message, CancellationToken ct)
        {
            if (_writer == null) throw new InvalidOperationException("Not connected.");
            var json = JsonUtil.Serialize(message);
            return _writer.WriteLineAsync(json);
        }

        public async Task<string> ReadLineAsync(CancellationToken ct)
        {
            if (_reader == null) throw new InvalidOperationException("Not connected.");

            using (ct.Register(() =>
            {
                try { _tcp.Close(); } catch { }
            }))
            {
                return await _reader.ReadLineAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            try { _writer?.Dispose(); } catch { }
            try { _reader?.Dispose(); } catch { }
            try { _tcp?.Close(); } catch { }

            _writer = null;
            _reader = null;
            _tcp = null;
        }
    }
}

