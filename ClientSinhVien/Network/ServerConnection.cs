using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientSinhVien.Network
{
    /// <summary>
    /// Quản lý toàn bộ kết nối TCP đến server.
    /// Form1 và các Panel KHÔNG dùng TcpClient trực tiếp — luôn qua class này.
    /// </summary>
    public class ServerConnection
    {
        private TcpClient     _client;
        private NetworkStream _stream;

        public bool IsConnected { get; private set; } = false;

        // ── Kết nối đến server ────────────────────────────────────────────────
        public async Task ConnectAsync(string host, int port)
        {
            _client = new TcpClient();
            await Task.Run(() => _client.Connect(host, port));
            _stream      = _client.GetStream();
            IsConnected  = true;
        }

        // ── Ngắt kết nối ─────────────────────────────────────────────────────
        public void Disconnect()
        {
            try { _stream?.Close(); _client?.Close(); } catch { }
            _stream     = null;
            _client     = null;
            IsConnected = false;
        }

        // ── Gửi lệnh và nhận response (async, không block UI) ────────────────
        public async Task<string> SendAsync(string message)
        {
            if (!IsConnected || _stream == null)
                throw new InvalidOperationException("Chưa kết nối đến máy chủ.");

            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);

            byte[] buf = new byte[65536];
            int    n   = await _stream.ReadAsync(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(buf, 0, n);
        }
    }
}
