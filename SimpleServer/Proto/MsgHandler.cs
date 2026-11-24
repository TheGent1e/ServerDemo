using Azure;
using Mysql.Controllers;
using SimpleServer.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mysql.Models;
namespace SimpleServer.Proto
{
    public class MsgHandler
    {
        public static void MsgSecret(ClientSocket clientSocket, MsgBase msgBase)
        {
            MsgSecret msgSecret = (MsgSecret)msgBase;
            //获取单例密钥 赋值给客户端数据包对象
            msgSecret.SecretKey = ServerSocket.Instance.SecretKey;
            //发送数据包给客户端
            ServerSocket.Instance.Send(clientSocket, msgSecret);
        }
        //心跳包处理
        public static void MsgPing(ClientSocket clientSocket, MsgBase msgBase)
        {
            //1、接收到客户端的心跳包后，更新客户端的最后心跳时间
            clientSocket.LastPingTime = ServerSocket.Instance.getTimeStamp();
            //2、构建心跳包对象，发送给客户端
            MsgPing msgPing = new MsgPing();
            ServerSocket.Instance.Send(clientSocket, msgPing);
        }
        //测试协议处理
        public static void MsgTest(ClientSocket clientSocket, MsgBase msgBase)
        {
            MsgTest msgTest = (MsgTest)msgBase;
            //打印客户端发送的内容
            Console.WriteLine($"收到客户端发送的内容：{msgTest.CSContent}");
            //构建返回给客户端的内容
            msgTest.SCContent = $"服务端返回的内容：{msgTest.CSContent}";
            //发送数据包给客户端
            ServerSocket.Instance.Send(clientSocket, msgTest);
        }

        //登录协议处理
        public static void MsgLogin(ClientSocket clientSocket, MsgBase msgBase)
        {
            MsgLogin msgLogin = (MsgLogin)msgBase;
            LoginResultType resultType = UserController.Instance.Login(msgLogin.account, msgLogin.password, out GameUser user);
            msgLogin.result = resultType;
            if (resultType == LoginResultType.Success && user != null)
            {
                clientSocket.UserId = user.Id;
                clientSocket.UserName = user.UserName;
                msgLogin.UserId = user.Id;
                msgLogin.NickName = user.NickName;
                msgLogin.Level = user.Level;
            }
            Debug.LogInfo($"客户端{clientSocket.socket.RemoteEndPoint}登录结果：{resultType}");
            ServerSocket.Instance.Send(clientSocket, msgLogin);
        }

        //注册用的协议
        public static void MsgRegister(ClientSocket clientSocket, MsgBase msgBase)
        {
            MsgRegister msgRegister = (MsgRegister)msgBase;
            msgRegister.result = UserController.Instance.Register(msgRegister.account, msgRegister.password);
            ServerSocket.Instance.Send(clientSocket, msgRegister);
        }

    }
}
