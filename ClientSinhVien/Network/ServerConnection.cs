using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace ClientSinhVien.Network
{

    public class ServerConnection
    {
        private TcpClient client;
        private NetworkStream stream;

        public bool IsConnected { get; private set; } = false;


        public async Task ConnectAsync(string host, int port)
        {
            client = new TcpClient();
            await Task.Run(() => client.Connect(host, port));
            stream = client.GetStream();
            IsConnected = true;
        }


        public void Disconnect()
        {
            try
            {
                stream?.Close();
                client?.Close();
            }
            catch
            {

            }

            stream = null;
            client = null;
            IsConnected = false;
        }


        public async Task<string> SendAsync(string message)
        {
            if (!IsConnected || stream == null)
                throw new InvalidOperationException("Chưa kết nối đến máy chủ.");

            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);

            byte[] buf = new byte[65536];                 // buffer 64KB để nhận dữ liệu
            int n = await stream.ReadAsync(buf, 0, buf.Length);

            return Encoding.UTF8.GetString(buf, 0, n);    // Chuyển byte[] -> string và trả về
        }
    }
}
