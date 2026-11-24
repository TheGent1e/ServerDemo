using System.Reflection;
using System.Runtime.InteropServices;

// 程序集基本信息。
[assembly: AssemblyTitle("ServerBase")]
[assembly: AssemblyDescription("ServerBase server application")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ServerBase")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 不向 COM 公开程序集中的类型。
[assembly: ComVisible(false)]

// 程序集版本信息。
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
// log4net 配置           参数：ConfigFile - 配置文件路径，ConfigFileExtension - 配置文件扩展名，Watch - 是否监视配置文件变化
[assembly:log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", ConfigFileExtension ="config" ,Watch = true)]