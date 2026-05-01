using System;                      // Thư viện cơ bản (Exception, kiểu dữ liệu,...)
using System.Net.Sockets;          // Thư viện làm việc với TCP/IP (TcpClient)
using System.Text;                 // Dùng để encode/decode (string <-> byte[])
using System.Threading.Tasks;      // Hỗ trợ async/await
// afas
namespace ClientSinhVien.Network   // Namespace để nhóm các class liên quan đến Network
{
    /// <summary>
    /// Quản lý toàn bộ kết nối TCP đến server.
    /// Form1 và các Panel KHÔNG dùng TcpClient trực tiếp — luôn qua class này.
    /// </summary>
    public class ServerConnection  // Khai báo class quản lý kết nối server
    {
        private TcpClient client;          // Đối tượng client TCP để kết nối server
        private NetworkStream stream;      // Luồng dữ liệu để gửi/nhận

        public bool IsConnected { get; private set; } = false; // Trạng thái kết nối (chỉ đọc từ bên ngoài)

        // ── Kết nối đến server ────────────────────────────────────────────────
        public async Task ConnectAsync(string host, int port) // Hàm kết nối server (async)
        {
            client = new TcpClient();                         // Tạo client mới
            await Task.Run(() => client.Connect(host, port)); // Kết nối (chạy ở thread khác để không block UI)
            stream = client.GetStream();                      // Lấy luồng dữ liệu
            IsConnected = true;                               // Đánh dấu đã kết nối
        }

        // ── Ngắt kết nối ─────────────────────────────────────────────────────
        public void Disconnect()                              // Hàm ngắt kết nối
        {
            try
            {
                stream?.Close();  // Đóng stream nếu khác null
                client?.Close();  // Đóng client nếu khác null
            }
            catch
            {
                // Bỏ qua lỗi nếu có (tránh crash)
            }

            stream = null;        // Xóa stream
            client = null;        // Xóa client
            IsConnected = false;  // Đánh dấu đã ngắt kết nối
        }

        // ── Gửi lệnh và nhận response (async, không block UI) ────────────────
        public async Task<string> SendAsync(string message) // Gửi dữ liệu và nhận phản hồi
        {
            if (!IsConnected || stream == null) // Kiểm tra đã kết nối chưa
                throw new InvalidOperationException("Chưa kết nối đến máy chủ."); // Nếu chưa thì báo lỗi

            byte[] data = Encoding.UTF8.GetBytes(message); // Chuyển string -> byte[]
            await stream.WriteAsync(data, 0, data.Length); // Gửi dữ liệu lên server

            byte[] buf = new byte[65536];                 // Tạo buffer 64KB để nhận dữ liệu
            int n = await stream.ReadAsync(buf, 0, buf.Length); // Đọc dữ liệu từ server (n = số byte nhận được)

            return Encoding.UTF8.GetString(buf, 0, n);    // Chuyển byte[] -> string và trả về
        }
    }
}
