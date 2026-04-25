using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerSinhVien.Data;
using ServerSinhVien.Network;
using ServerSinhVien.Services;

    /// <summary>
    /// File: Program.cs
    /// Chức năng: Entry point (điểm bắt đầu) của Server.
    /// Khởi động TcpListener, lắng nghe kết nối từ Client, theo dõi số lượng Client đang hoạt động.
    /// </summary>
    class Program
    {
        public static int ActiveClients = 0;

        static void Main()
    {
        var repo    = new SinhVienRepository();
        repo.Load();

        var service = new SinhVienService(repo);
        var server  = new TcpListener(IPAddress.Any, 5000);
        server.Start();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║   SERVER QUẢN LÝ SINH VIÊN - v4.0   ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Đang lắng nghe cổng 5000...  (Nhấn Ctrl+C để dừng)");

        while (true)
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();
                string    ep     = client.Client.RemoteEndPoint?.ToString() ?? "?";

                Interlocked.Increment(ref ActiveClients);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client kết nối: {ep}. Số client đang hoạt động: {ActiveClients}");
                Console.ResetColor();

                Action onDisconnect = () => 
                {
                    Interlocked.Decrement(ref ActiveClients);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client [{ep}] ngắt kết nối. Số client còn lại: {ActiveClients}");
                    Console.ResetColor();
                };

                var handler = new ClientHandler(client, ep, service, onDisconnect);
                var thread  = new Thread(handler.Handle) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Lỗi accept: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
