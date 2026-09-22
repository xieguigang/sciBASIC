' =============================================================
' demo: #include 引入 nuget 程序包
'
'   vbs ./test/test_include_nuget.vb
'
' 语法:
'   #include "package-name"                 ' 自动取最新稳定版
'   #include "package-name@13.0.3"          ' 精确版本
'   #include "package-name@[13.0,14.0)"     ' 版本范围
'
' 引擎会读取包内 .nuspec 声明的依赖并递归解析全部传递依赖;
' 包被解压到本地缓存 ~/.nuget/packages/<id>/<version>/(与 NuGet 目录布局一致),
' 缓存命中时不再联网, 因此二次运行是零网络开销的。
' =============================================================
#include "Newtonsoft.Json@13.0.3"
#include "Microsoft.Extensions.Logging@8.0.1"

Imports Newtonsoft.Json

Call Console.WriteLine("---------- Newtonsoft.Json (无依赖) ----------")

Dim person As New Newtonsoft.Json.Linq.JObject()

person("name") = "asuka"
person("age") = 18
person("tags") = New Newtonsoft.Json.Linq.JArray({"vbs", "script"})

Call Console.WriteLine(JsonConvert.SerializeObject(person, Formatting.Indented))

Call Console.WriteLine("---------- Microsoft.Extensions.Logging (含传递依赖) ----------")

Dim logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

Call Console.WriteLine("logger     = " & logger.GetType().FullName)
Call Console.WriteLine("level      = " & Microsoft.Extensions.Logging.LogLevel.Warning.ToString())

Call Console.WriteLine("---------- 解析出的程序集(根包 + 全部传递依赖) ----------")
Call Console.WriteLine("Includes() = " & Includes().Length & " 项")

For Each dll In Includes()
    Call Console.WriteLine("    -> " & dll)
Next
