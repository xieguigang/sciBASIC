
''' <summary>命令行参数解析结果</summary>
Public Class CliOptions
    ''' <summary>用户显式指定的 nuget 程序包版本号，未指定时为空</summary>
    Public Property Version As String
    ''' <summary>框架根目录，未指定时自动向上回溯查找</summary>
    Public Property Root As String
    ''' <summary>只打印将要发生的改动，不写盘</summary>
    Public Property DryRun As Boolean
    ''' <summary>只更新版本号，跳过过时编译配置的清理</summary>
    Public Property NoClean As Boolean = True
    ''' <summary>是否修正 nuget_release|x64 配置的产物输出路径</summary>
    Public Property FixOutputPath As Boolean
    ''' <summary>是否请求打印用法说明</summary>
    Public Property ShowHelp As Boolean
    ''' <summary>解析过程中出现的错误描述</summary>
    Public Property [Error] As String
End Class