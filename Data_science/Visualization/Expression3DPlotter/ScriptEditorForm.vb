Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Math.Scripting

''' <summary>
''' 独立的非模态脚本输入框窗口。用户在多行文本框中编写脚本，
''' 点击“运行”后由 MathScriptEngine 执行，并通过 ScriptExecuted 事件
''' 将结果回传给主窗口；可一边编辑一边观察主窗口渲染。
''' </summary>
Public Class ScriptEditorForm
    Inherits Form

    Public Event ScriptExecuted(result As ScriptResult)

    Private txt As New TextBox()
    Private btnRun As New Button()
    Private btnExample As New Button()
    Private btnClose As New Button()
    Private status As New Label()

    Public Sub New()
        Text = "脚本输入框 - 数学表达式可视化"
        Width = 760
        Height = 580
        StartPosition = FormStartPosition.CenterParent

        txt.Multiline = True
        txt.ScrollBars = ScrollBars.Both
        txt.Dock = DockStyle.Fill
        txt.Font = New Font("Consolas", 11)
        txt.Text = SampleScript()
        Controls.Add(txt)

        Dim panel As New Panel() With {.Dock = DockStyle.Bottom, .Height = 50}
        btnRun.Text = "运行" : btnRun.Width = 90 : btnRun.Left = 12 : btnRun.Top = 10
        btnExample.Text = "示例" : btnExample.Width = 90 : btnExample.Left = 112 : btnExample.Top = 10
        btnClose.Text = "关闭" : btnClose.Width = 90 : btnClose.Left = 212 : btnClose.Top = 10
        status.AutoSize = True : status.Top = 16 : status.Left = 320
        panel.Controls.AddRange({btnRun, btnExample, btnClose, status})
        Controls.Add(panel)

        AddHandler btnRun.Click, AddressOf OnRun
        AddHandler btnExample.Click, AddressOf OnExample
        AddHandler btnClose.Click, AddressOf OnClose
    End Sub

    Private Function SampleScript() As String
        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine("# 三维曲面示例")
        sb.AppendLine("x = axis(-3, 3, n=80)")
        sb.AppendLine("y = axis(-3, 3, n=80)")
        sb.AppendLine("z(x, y) = sin(sqrt(x*x + y*y))")
        sb.AppendLine("surface(x, y, z)")
        sb.AppendLine("")
        sb.AppendLine("# 其它可用指令（取消注释即可）：")
        sb.AppendLine("# x = axis(-5, 5, n=200)")
        sb.AppendLine("# y(x) = sin(x)")
        sb.AppendLine("# line(x, y)              # 二维曲线")
        sb.AppendLine("# scatter(x, y)           # 二维散点")
        sb.AppendLine("# z3(x, y) = x*x - y*y")
        sb.AppendLine("# scatter(x, y, z3(x, y)) # 三维散点")
        Return sb.ToString()
    End Function

    Private Sub OnRun(sender As Object, e As EventArgs)
        Dim engine As New MathScriptEngine()
        Dim result = engine.RunScript(txt.Text)
        If result.Success Then
            status.Text = "成功：" & result.Commands.Count & " 条绘图指令"
        Else
            status.Text = "错误：" & result.ErrorMessage
        End If
        RaiseEvent ScriptExecuted(result)
    End Sub

    Private Sub OnExample(sender As Object, e As EventArgs)
        txt.Text = SampleScript()
    End Sub

    Private Sub OnClose(sender As Object, e As EventArgs)
        Close()
    End Sub

End Class
