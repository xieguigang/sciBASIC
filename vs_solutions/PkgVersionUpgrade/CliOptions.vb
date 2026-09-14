
Imports Microsoft.VisualBasic.CommandLine.Reflection

''' <summary>命令行参数解析结果</summary>
Public Class CliOptions
    ''' <summary>用户显式指定的 nuget 程序包版本号，未指定时为空</summary>
    <Opt("-v", "--version")> Public Property Version As String
    ''' <summary>待处理的 slnx 解决方案文件路径。工具会解析该文件并枚举其中的 vbproj。</summary>
    <Opt("-s", "--slnx")> Public Property Slnx As String
    ''' <summary>命名空间前缀，用于过滤 slnx 中枚举到的 vbproj（按 RootNamespace 大小写不敏感匹配）。</summary>
    <Opt("-p", "--prefix", "--namespace")> Public Property NamespacePrefix As String
    ''' <summary>只打印将要发生的改动，不写盘</summary>
    <Opt("-n", "--dry-run")> Public Property DryRun As Boolean
    ''' <summary>只更新版本号，跳过过时编译配置的清理</summary>
    <Opt("--clean")> Public Property MakeClean As Boolean = False
    ''' <summary>是否修正 nuget_release|x64 配置的产物输出路径。开启时 --output 必填。</summary>
    <Opt("--fix-output-path")> Public Property FixOutputPath As Boolean
    ''' <summary>
    ''' 编译产物输出文件夹，仅当 --fix-output-path 开启时必填。
    ''' 会被展开为绝对路径并化为相对于每个目标 vbproj 的相对路径写进 OutputPath。
    ''' </summary>
    <Opt("-o", "--output")> Public Property OutputDir As String
    ''' <summary>是否请求打印用法说明</summary>
    <Opt("-h", "--help", "/?", "-?")> Public Property ShowHelp As Boolean
    ''' <summary>解析过程中出现的错误描述</summary>
    Public Property [Error] As String
End Class