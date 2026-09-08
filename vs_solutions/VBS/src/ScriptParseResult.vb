Imports Microsoft.VisualBasic.CommandLine

''' <summary>
''' 脚本文件的解析结果
''' </summary>
Public Class ScriptParseResult

    ''' <summary>被解析的脚本源代码文件路径</summary>
    Public Property ScriptFile As String

    ''' <summary>解析得到的虚拟命令行参数对象</summary>
    Public Property CommandLine As CommandLine

    ''' <summary>#include所引用的外部程序集路径列表(统一为绝对路径)</summary>
    Public Property [Imports] As List(Of String)

    ''' <summary>重构之后的可以直接被Roslyn编译的完整VB.NET源代码</summary>
    Public Property GeneratedCode As String
End Class