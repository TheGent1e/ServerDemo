using SimpleServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Net
{
    public class ServerSocket : Singleton<ServerSocket>//泛型单例
    {
        public string PublicKey = "abc123";
        public string SecretKey = "123456";
        private string m_IP = "127.0.0.1";//本地IP
        private int m_Port = 8888;//端口号
        private Socket m_ListenSocker;//监听的socket

        //存储所有socket的集合
        private List<Socket> m_SocketList = new List<Socket>();
        //存储所有客户端的socket的字典   键是客户端的socket，值是客户端的socket对象
        private Dictionary<Socket, ClientSocket> m_ClientSocketDic = new Dictionary<Socket, ClientSocket>();

        //心跳包间隔时间
        private long m_HeartBeatInterval = 30;

        //存储所有心跳包超时的客户端socket的集合
        private List<ClientSocket> m_HeartBeatTimeoutList = new List<ClientSocket>();
        public void Init()
        {
            //将string类型的IP地址转换为IPAddress类型
            IPAddress IP = IPAddress.Parse(m_IP);
            //创建IPEndPoint对象 IPEndPoint是一个终结点类，包含IP地址和端口号
            IPEndPoint iPEndPoint = new IPEndPoint(IP, m_Port);
            //传输协议 因特网          传输方式：TCP流式传输         TCP协议
            m_ListenSocker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //绑定端口号。已有服务实例运行时，避免未处理异常导致启动窗口崩溃。
            try
            {
                m_ListenSocker.Bind(iPEndPoint);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                Debug.LogError("服务端启动失败：127.0.0.1:8888 已被占用。请关闭已有的 SimpleServer.exe，只保留一个服务端实例。\n{0}", ex.Message);
                m_ListenSocker.Close();
                m_ListenSocker = null;
                return;
            }

            m_ListenSocker.Listen(100);//设置最大连接数

            //等待客户端连接
            while (true)
            {
                CheckSocketRead();
                //使用Select方法检测Socket的可读性
                try
                {
                    //多路复用socket    
                    //参数:1.检测的Socket集合 2.检测可写的Socket集合 3.检测错误的Socket集合 4.超时时间
                    Socket.Select(m_SocketList, null, null, 1000);
                    //选择完后，m_ClientSockets集合中就只剩下可读的Socket了（也就是带数据的连接）
                }
                catch (Exception e)
                {
                    Debug.LogError("select异常{0}", e);
                }
                for (int i = m_SocketList.Count - 1; i >= 0; i--)
                {
                    //两种情况
                    if (m_SocketList[i] == m_ListenSocker)
                    {
                        //说明该 socket未连接 请求连接
                        //处理监听socket，解决连接
                        ReadListen(m_ListenSocker);
                    }
                    else
                    {
                        //已经连接上的
                        //具体业务处理  处理客户端发来的数据
                        ReadClient(m_SocketList[i]);

                    }
                    long timeNow=getTimeStamp();
                    m_HeartBeatTimeoutList.Clear();
                    foreach (ClientSocket client in m_ClientSocketDic.Values)
                    {
                        if (timeNow - client.LastPingTime > m_HeartBeatInterval*4)
                        {
                            m_HeartBeatTimeoutList.Add(client);
                        }
                    }
                    //m_HeartBeatTimeoutList 的作用就是暂存需要关闭的客户端，使检查过程和删除过程分离。
                    //统一关闭心跳包超时的客户端socket
                    foreach (ClientSocket client in m_HeartBeatTimeoutList)
                    {
                        Debug.Log("连接超时，关闭客户端连接{0}", client.socket.RemoteEndPoint.ToString());
                        CloseClient(client);
                    }

                }

            }
        }
        //检测Socket的可读性
        public void CheckSocketRead()
        {
            //清空m_ClientSockets集合
            m_SocketList.Clear();
            if (m_ListenSocker != null)
            {
                m_SocketList.Add(m_ListenSocker);
            }
            //将m_ClientSocketDic字典中的所有key（Socket）添加到m_SocketList集合中
            foreach (Socket item in m_ClientSocketDic.Keys)
            {
                m_SocketList.Add(item);
            }

        }

        //处理监听socket，解决连接
        void ReadListen(Socket listen)
        {
            try
            {
                Socket client = listen.Accept();//等待客户端连接 连接后返回一个新的socket专门用于通信
                ClientSocket clientSocket = new ClientSocket();
                clientSocket.socket = client;
                clientSocket.LastPingTime = getTimeStamp();
                //  将新的socket添加到字典中                         
                m_ClientSocketDic.Add(client, clientSocket);
                //                  LocalEndPoint是获取本地终结点的属性，返回一个EndPoint对象，表示本地终结点的IP地址和端口号 
                Debug.LogInfo("一个客户端连接成功{0}，当前连接数为{1}", client.LocalEndPoint.ToString(), m_ClientSocketDic.Count);
            }
            catch (SocketException e)
            {
                Debug.LogError("连接异常{0}", e);
            }
        }
        //时间戳处理
        public long getTimeStamp()
        {
            //获取当前时间的时间戳
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            //返回时间戳的秒数  Conver.ToInt64是将double类型转换为long类型 
            return Convert.ToInt64(ts.TotalSeconds);
        }

        public void ReadClient(Socket client)
        {
            ClientSocket clientSocket = m_ClientSocketDic[client];//通过字典获取客户端socket对象
            //接收客户端发来的数据
            ByteArray byteArray = clientSocket.byteArray;
            int count = 0;//接收到的字节数
            if(byteArray.Remain<=0)
            {//两种情况 1.缓冲区刚好满了 2.缓冲区没有空间了

                //对数据进行处理
                OnReceiveData(clientSocket);

                //处理结果分为两种情况
                //1.数据包不完整  如果数据包不完整，说明客户端发来的数据还没有接收完，需要继续接收数据
                //1.1那么就需要刷新缓冲区，将未读数据移动到缓冲区的起始位置，保证缓冲区有足够的空间接收新的数据

                //2.数据包完整  如果数据包完整，说明客户端发来的数据已经接收完，可以进行处理
                //2.1处理完后也需要刷新缓冲区，将未读数据移动到缓冲区的起始位置，保证缓冲区有足够的空间接收新的数据


                //刷新缓冲区
                byteArray.CheckAndMoveBytes();//将未读数据移动到缓冲区的起始位置

                while (byteArray.Remain <= 0)
                {
                    //缓冲区满了 需要扩容
                    //防御性编程 一般来说走不到三目运算符左侧这一步 因为进入这个循环的前提是缓冲区已经满了 但是为了防止出现意外情况 还是加上这个判断
                    int expandSize = byteArray.Length < ByteArray.DefaultSize ? ByteArray.DefaultSize : byteArray.Length; 
                    byteArray.ExpandCapacity(expandSize * 2);
                }

            }

            try
            {
                //接收数据  Receive方法是阻塞的，直到有数据到达才会返回
                //  参数: 1.接收数据的缓冲区 2.缓冲区的起始位置 3.最多可以接收的字节数 4.标志位
                count=client.Receive(byteArray.bytes, byteArray.writeIndex,byteArray.Remain,0);
                
            }
            catch (SocketException e)
            {
                Debug.LogError("接收数据异常{0}", e);
                CloseClient(clientSocket);
            }
            if(count <= 0)
            {
                //客户端断开连接
                CloseClient(clientSocket);
                return;
            }
            //接收数据成功
            byteArray.writeIndex += count;//更新写入索引
            //对写入的数据进行处理  解析数据包
            OnReceiveData(clientSocket);
            //刷新缓冲区
            byteArray.CheckAndMoveBytes();

        }
        //处理接收的数据  解析数据包
        public void OnReceiveData(ClientSocket clientSocket)
        {
            ByteArray byteArray = clientSocket.byteArray;
            while (byteArray.Length >= 4)
            {
                int packetStart = byteArray.readIndex;
                int availableLength = byteArray.Length;
                if (packetStart < 0 || packetStart + 4 > byteArray.writeIndex)
                {
                    return;
                }
                Byte[] bytes = byteArray.bytes;
                int bodylenth = BitConverter.ToInt32(bytes, packetStart);
                Debug.Log("收到数据：Length={0}, ReadIndex={1}, WriteIndex={2}, BodyLength={3}",
                    availableLength, byteArray.readIndex, byteArray.writeIndex, bodylenth);
                if (bodylenth <= 0)
                {
                    Debug.LogError("协议包长度无效:{0}", bodylenth);
                    CloseClient(clientSocket);
                    return;
                }
                if (availableLength < bodylenth + 4)
                {
                    return;
                }
                int namecount = 0;
                ProtocolEnum protocol = ProtocolEnum.None;
                try
                {
                    protocol=MsgBase.DecodeName(bytes, packetStart + 4, out namecount);
                }
                catch (Exception e)
                {
                    Debug.LogError("解析协议名错误:" + e);
                    CloseClient(clientSocket);
                    return;
                }
                if (namecount <= 0 || namecount > bodylenth)
                {
                    Debug.LogError("协议名长度非法:{0}", namecount);
                    CloseClient(clientSocket);
                    return;
                }
                int bodyCount = bodylenth - namecount;
                MsgBase msgBase = null;
                try
                {
                    msgBase=MsgBase.Decode(protocol, bytes, packetStart + 4 + namecount, bodyCount);
                }
                catch (Exception e)
                {
                    Debug.LogError("{0}协议解析异常", protocol.ToString() + e);
                    CloseClient(clientSocket);
                    return; 
                }
                if(msgBase==null)
                {
                    Debug.LogError("{0}协议解析失败", protocol.ToString());
                    CloseClient(clientSocket);
                    return;
                }
                Debug.Log("协议解析成功：Protocol={0}, BeforeLength={1}, BodyCount={2}",
                    protocol.ToString(), availableLength, bodyCount);
                byteArray.readIndex = packetStart + 4 + bodylenth;
                Debug.Log("协议解析完成：ReadIndex={0}, WriteIndex={1}, RemainingLength={2}",
                    byteArray.readIndex, byteArray.writeIndex, byteArray.Length);

                //处理解析出来的协议数据
                //根据对应的协议类型，调用对应的处理方法
                //通过反射获取对应协议类型 调用MsgHandler类中的对应方法
                //typeof(MsgHandler) 获取MsgHandler类的类型信息
                MethodInfo methodInfo = typeof(MsgHandler).GetMethod(protocol.ToString());
                if(methodInfo!= null)
                {
                    //构建参数列表
                    object[] obj = { clientSocket, msgBase };
                    //Invoke 在运行时动态地调用指定对象的方法。
                    //参数：
                    //1、要调用方法的对象实例（如果是静态方法，则传入null）
                    //2、一个包含参数的对象数组
                    methodInfo.Invoke(null, obj);//调用方法  null表示静态方法
                    Debug.Log("根据反射找到了{0}协议所对应的函数，进行分发处理！", protocol.ToString());
                }
                else
                {
                    Debug.LogError("没有找到{0}协议对应的处理方法", protocol.ToString());
                }
            }
        }
        //发送数据
        public void Send(ClientSocket clientSocket,MsgBase msgBase)
        {
           //协议名
            byte[] NameBytes = MsgBase.EncodeName(msgBase);
            //协议体数据
            byte[] BodyBytes = MsgBase.Encode(msgBase);
            //协议名长度+协议体长度
            int len =NameBytes.Length + BodyBytes.Length;
            //头部长度         //BitConverter.GetBytes() 是将一个整数转换为字节数组的方法，返回的是一个长度为4的字节数组，表示整数的二进制形式
            byte[] HeadBytes = BitConverter.GetBytes(len);
            //发送数组                  头长 + 协议名长 + 数据体长
            byte[] SendBytes = new byte[HeadBytes.Length + len];
            // 将头部数据拷贝到发送数组 中
            Array.Copy(HeadBytes, 0, SendBytes, 0, HeadBytes.Length);
            //将协议名数据拷贝到发送数组中
            Array.Copy(NameBytes,0,SendBytes, HeadBytes.Length, NameBytes.Length);
            //将协议体数据拷贝到发送数组中
            Array.Copy(BodyBytes, 0, SendBytes, HeadBytes.Length+NameBytes.Length, BodyBytes.Length);

            //发送
            try
            {
                //开始发送       BeginSend 参数:1、要发送的数据缓冲区 2、起始下标 3、最多发送的字节数
                //4、不使用特殊发送标志 5、发送完成后的回调方法 6、传给回调的状态对象
                clientSocket.socket.BeginSend(SendBytes, 0, SendBytes.Length, 0,null, null);
            }   
            catch (SocketException e)
            {
                Debug.LogError("发送数据异常" + e);
                CloseClient(clientSocket);
            }
        }
        //关闭连接Close
        public void CloseClient(ClientSocket clientSocket)
        {
            clientSocket.socket.Close();
            m_ClientSocketDic.Remove(clientSocket.socket);
            Debug.Log("一个客户端断开连接，当前连接数为{0}", m_ClientSocketDic.Count);
        }
    }
}
