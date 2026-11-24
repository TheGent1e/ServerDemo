using Mysql;
using SimpleServer.Net;

namespace SimpleServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MysqlManager.Instance.Init();
            ServerSocket.Instance.Init();
        }
    }
}
