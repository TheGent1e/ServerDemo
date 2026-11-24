using ProtoBuf;
using SimpleServer.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer.Proto
{
    //消息基类
    public class MsgBase
    {
        public virtual ProtocolEnum ProtocolType { get; set; }
        //编码协议名
        public static byte[] EncodeName(MsgBase msgBase)
        {
            //将协议名转换为字节数组
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.ProtocolType.ToString());
            //协议名长度  Int16是一个16位的有符号整数类型，表示范围从-32768到32767。它占用2个字节的存储空间。
            Int16 len = (Int16)nameBytes.Length;
            //协议名长度占两个字节，前两个字节存储协议名长度，后面是协议名
            byte[] bytes = new byte[2 + len];
            //将协议名长度存储到前两个字节中
            bytes[0] = (byte)(len % 256);
            bytes[1] = (byte)(len / 256);
            //将协议名存储到后面
            Array.Copy(nameBytes, 0, bytes, 2, len);
            return bytes;
        }
        //解码协议名                //参数:1.bytes:字节数组 2.offset:偏移量 3.count:输出参数，表示协议名的总长度
        public static ProtocolEnum DecodeName(byte[] bytes, int offset, out int count)
        {
            count = 0;
            if (offset < 0 || offset + 2 > bytes.Length)//如果偏移量加上协议名长度的两个字节超过了字节数组的长度，说明协议名不完整
            {
                return ProtocolEnum.None;
            }
            int len = bytes[offset + 1] << 8 | bytes[offset];
            if (offset + 2 + len > bytes.Length)//如果协议名长度超过了字节数组的长度，说明协议名不完整
            {
                return ProtocolEnum.None;
            }
            count = 2 + len;
            try
            {
                string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
                return (ProtocolEnum)System.Enum.Parse(typeof(ProtocolEnum), name);
            }
            catch (Exception e)
            {
                Debug.LogError("不存在协议：" + e.ToString());
                return ProtocolEnum.None;
            }
        }
        //协议内容序列化和加密
        public static byte[] Encode(MsgBase msgBase)
        {
            using (MemoryStream memory = new MemoryStream())//使用内存流来存储序列化后的数据
            {
                Serializer.Serialize(memory, msgBase);//将消息对象序列化为字节数组
                byte[] bytes = memory.ToArray();        //将内存流转换为字节数组
                string secret = ServerSocket.Instance.SecretKey; //默认是密钥加密
                if (msgBase is MsgSecret)//当前的消息是请求密钥的时候
                {
                    secret = ServerSocket.Instance.PublicKey; //使用公钥加密
                }
                bytes = AES.AESEncrypt(bytes, secret); //加密
                return bytes;
            }
        }
        //协议内容解密和反序列化
        public static MsgBase Decode(ProtocolEnum protocol, byte[] bytes, int offset, int count)
        {
            if (protocol == ProtocolEnum.None || offset < 0 || count <= 0 || offset > bytes.Length - count)
            {
                Debug.LogError("协议解密错误，协议或数据范围无效");
                return null;
            }
            try
            {
                byte[] newBytes = new byte[count];
                Array.Copy(bytes, offset, newBytes, 0, count);
                string secret = protocol == ProtocolEnum.MsgSecret
                    ? ServerSocket.Instance.PublicKey
                    : ServerSocket.Instance.SecretKey;
                newBytes = AES.AESDecrypt(newBytes, secret);
                //反序列化
                using (MemoryStream memory = new MemoryStream(newBytes, 0, newBytes.Length))
                {
                    Type type = System.Type.GetType(protocol.ToString());//根据协议名获取到对应的类类型
                    return (MsgBase)Serializer.NonGeneric.Deserialize(type, memory);//反序列化
                }

            }
            catch (Exception e)
            {
                Debug.LogError("协议内容解密异常！" + e.ToString());
                return null;
            }
        }
    }
}
