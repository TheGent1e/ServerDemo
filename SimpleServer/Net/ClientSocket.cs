using System.Net.Sockets;

namespace SimpleServer.Net
{
    public class ClientSocket
    {
        //客户端的socket---服务端返回的专门用于和客户端通信的socket
        public Socket socket { get; set; }
        public long LastPingTime { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        //数据缓冲区
        public ByteArray byteArray =new ByteArray();
    }
}
