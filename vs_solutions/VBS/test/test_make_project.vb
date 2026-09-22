' =============================================================
' demo: vbs make-project 用例
'
'   以脚本方式运行:
'     vbs ./test/test_make_project.vb --repeat=3 --loud
'
'   就地转换为正式的 vbproj 工程:
'     vbs make-project ./test/test_make_project.vb
'
' 覆盖: 元数据指令 / #include 引入其它脚本 / 顶层函数 / 顶层函数捕获顶层变量 /
'       ?args 参数语法 / let 动态声明 / 元组分解 / 类型定义。
' =============================================================
#package "VBScript.Demo.MakeProject"
#author "xieguigang"
#title "VBS make-project Demo"
#version "1.0.0"

#include "./lib/Helper.vb"

' 顶层变量: 被顶层函数 Summarize 捕获 => 转换之后提升为模块级字段
Dim items As String() = {"alpha", "beta", "gamma"}

' ?args 参数语法: 转换之后引用模块级 args 字段
Dim repeat As Integer = ?"--repeat"
Dim loud As Boolean = ?"--loud"

' let 动态类型声明
let runningTotal = 0
let label = $"repeat={repeat}"

' 顶层函数: 转换之后成为模块级 Private Function / Private Sub
Public Function Summarize As String
    Return LibraryHelper.BuildReport(items)
End Function

Public Sub Accumulate(value As Integer)
    runningTotal += value
End Sub

' 元组分解语法
Dim (lower, upper) = (1, 2)

Call Console.WriteLine(LibraryHelper.Greet("make-project"))
Call Console.WriteLine("Summarize():")
Call Console.Write(Summarize())

For i As Integer = 1 To repeat
    Call Accumulate(i)
Next

Call Console.WriteLine($"runningTotal = {runningTotal}")
Call Console.WriteLine($"label        = {label}")
Call Console.WriteLine($"loud         = {loud}")
Call Console.WriteLine($"tuple        = {lower} / {upper}")
Call Console.WriteLine($"vector       = {LibraryAlgebra.Scale(New Vector2 With {.X = 1, .Y = 2}, 3).ToString()}")
Call Console.WriteLine($"includes     = {Includes().Length} 项")

Dim report As New ProjectSummary With {.Name = Package(), .ItemCount = items.Length}

Call Console.WriteLine(report.ToString())

Public Class ProjectSummary

    Public Property Name As String
    Public Property ItemCount As Integer

    Public Overrides Function ToString() As String
        Return $"summary      = {Name} ({ItemCount} items)"
    End Function
End Class
