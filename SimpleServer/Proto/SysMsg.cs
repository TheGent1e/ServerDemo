using Azure;
using ProtoBuf;
using SimpleServer.Proto;
using System.Reflection;
using System.Collections.Generic;

//序列化
[ProtoContract]
    public class MsgSecret : MsgBase
{
    [ProtoMember(1)]
    public override ProtocolEnum ProtocolType { get; set; }
    [ProtoMember(2)]
    public string SecretKey;
    public MsgSecret()
    {
        ProtocolType = ProtocolEnum.MsgSecret;
    }
}
[ProtoContract]
public class MsgPing : MsgBase
{
    public MsgPing()
    {
        ProtocolType = ProtocolEnum.MsgPing;
    }
    [ProtoMember(1)]
    public override ProtocolEnum ProtocolType { get; set; }
}
[ProtoContract]
public class MsgTest : MsgBase
{
    public MsgTest()
    {
        ProtocolType = ProtocolEnum.MsgTest;
    }
    [ProtoMember(1)]
    public override ProtocolEnum ProtocolType { get; set; }
    [ProtoMember(2)]
    public string CSContent { get; set; }
    [ProtoMember(3)]
    public string SCContent { get; set; }
}

[ProtoContract]
public class MsgLogin:MsgBase
{
    public MsgLogin()
    {
        ProtocolType = ProtocolEnum.MsgLogin;
    }
    [ProtoMember(1)]
    public override ProtocolEnum ProtocolType { get; set; }
    [ProtoMember(2)]
    public string account { get; set; }
    [ProtoMember(3)]
    public string password { get; set; }
    [ProtoMember(4)]
    public LoginResultType result { get; set; }
    [ProtoMember(5)]
    public long UserId { get; set; }
    [ProtoMember(6)]
    public string NickName { get; set; }
    [ProtoMember(7)]
    public int Level { get; set; }
}
[ProtoContract]
public class MsgRegister : MsgBase
{
    public MsgRegister()
    {
        ProtocolType = ProtocolEnum.MsgRegister;
    }
    [ProtoMember(1)]
    public override ProtocolEnum ProtocolType { get; set; }
    [ProtoMember(2)]
    public string account { get; set; }
    [ProtoMember(3)]
    public string password { get; set; }
    [ProtoMember(4)]
    public RegisterResultType result { get; set; }
}

[ProtoContract]
public class UserItem : MsgBase
{
    [ProtoMember(1)] public long Id { get; set; }
    [ProtoMember(2)] public long UserId { get; set; }
    [ProtoMember(3)] public int ItemId { get; set; }
    [ProtoMember(4)] public int Count { get; set; }
}



