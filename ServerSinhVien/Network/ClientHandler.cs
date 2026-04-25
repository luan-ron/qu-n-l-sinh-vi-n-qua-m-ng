using System;
using System.Net.Sockets;
using System.Text;
using ServerSinhVien.Services;

namespace ServerSinhVien.Network
{
    /// <summary>
    /// File: ClientHandler.cs
    /// Chức năng: Xử lý giao tiếp độc lập với từng Client trên một Thread riêng biệt.
    /// Nhận yêu cầu (request) từ Client, gửi qua SinhVienService xử lý, và trả kết quả (response) về Client.
    /// </summary>
    public class ClientHandler
    {
        private readonly TcpClient        _client;
        private readonly string           _ep;
        private readonly SinhVienService  _service;
        private readonly Action           _onDisconnect;

        public ClientHandler(TcpClient client, string endpoint, SinhVienService service, Action onDisconnect)
        {
            _client       = client;
            _ep           = endpoint;
            _service      = service;
            _onDisconnect = onDisconnect;
        }

        public void Handle()
        {
            try
            {
                using (_client) // Đảm bảo TCP Client sẽ được đóng sau khi sử dụng xong
                using (var stream = _client.GetStream()) // Lấy luồng dữ liệu (NetworkStream) để đọc/ghi với client
                {
                    byte[] buf = new byte[65536]; // Tạo mảng byte đệm kích thước 64KB để chứa dữ liệu nhận được
                    while (true) // Vòng lặp liên tục chờ nhận dữ liệu từ client
                    {
                        int n;
                        try   { n = stream.Read(buf, 0, buf.Length); } // Đọc dữ liệu từ luồng vào mảng đệm và lấy số byte thực tế đọc được (n)
                        catch { break; } // Nếu lỗi trong quá trình đọc (ví dụ rớt mạng), thoát khỏi vòng lặp
                        if (n == 0) break; // Nếu không đọc được byte nào (n=0), tức là client đã ngắt kết nối, thoát vòng lặp

                        string req = Encoding.UTF8.GetString(buf, 0, n).Trim(); // Chuyển đổi mảng byte nhận được thành chuỗi văn bản (UTF8) và cắt bỏ khoảng trắng thừa
                        if (req == "") continue; // Nếu chuỗi rỗng thì bỏ qua và tiếp tục vòng lặp mới

                        string cmdName = req.Split(';')[0].ToUpper(); // Lấy tên lệnh (phần tử đầu tiên trước dấu chấm phẩy) và chuyển thành chữ in hoa
                        Log($"[{_ep}] >> {cmdName}", ConsoleColor.White); // Ghi log ra màn hình server cho biết client nào vừa gửi lệnh gì

                        string resp      = _service.Process(req); // Chuyển chuỗi yêu cầu vào Service để xử lý và lấy chuỗi kết quả trả về
                        byte[] respBytes = Encoding.UTF8.GetBytes(resp); // Chuyển đổi chuỗi kết quả thành mảng byte (UTF8)
                        stream.Write(respBytes, 0, respBytes.Length); // Ghi mảng byte kết quả vào luồng để gửi lại cho client
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[{_ep}] Lỗi: {ex.Message}", ConsoleColor.Yellow); // Ghi log ra màn hình nếu có bất kỳ lỗi ngoại lệ nào xảy ra trong quá trình xử lý
            }
            finally
            {
                _onDisconnect?.Invoke(); // Gọi hàm callback thông báo cho hệ thống rằng client này đã ngắt kết nối (để cập nhật lại số lượng client)
            }
        }

        private static void Log(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}");
            Console.ResetColor();
        }
    }
}
