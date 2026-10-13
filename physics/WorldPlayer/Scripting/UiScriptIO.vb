Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

''' <summary>
''' 把 JS 的 <c>console.log</c> 输出转发到宿主控制台面板，
''' 并做行数上限保护（超过 <see cref="MaxLines"/> 后从头滚动截断），
''' 避免高频打印把 UI 卡死。
''' </summary>
Public Class UiScriptIO
    Inherits ScriptIO

    ''' <summary>最多保留的行数。</summary>
    Public Const MaxLines As Integer = 500

    Private ReadOnly sink As Action(Of String, Boolean)

    Public Sub New(sink As Action(Of String, Boolean))
        Me.sink = sink
    End Sub

    Public Overrides Sub WriteLine(text As String)
        sink(text, False)
    End Sub

    ''' <summary>写一条错误行（宿主侧使用，红色高亮）。</summary>
    Public Sub WriteError(text As String)
        sink(text, True)
    End Sub
End Class
