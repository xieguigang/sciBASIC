Imports System.Text
Imports Microsoft.VisualBasic.Math.Scripting

''' <summary>
''' 独立的非模态脚本输入框窗口。用户在多行文本框中编写脚本，
''' 点击“运行”后由 MathScriptEngine 执行，并通过 ScriptExecuted 事件
''' 将结果回传给主窗口；可一边编辑一边观察主窗口渲染。
''' </summary>
Public Class ScriptEditorForm
    Inherits Form

    Public Event ScriptExecuted(result As ScriptResult)


    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ToolStrip1 As ToolStrip
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
    Friend WithEvents ToolStripLabel1 As ToolStripLabel
    Friend WithEvents ToolStripButton1 As ToolStripButton
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents ToolStripLabel2 As ToolStripLabel
    Friend WithEvents cboPreset As ToolStripComboBox
    Friend WithEvents ToolStripButton2 As ToolStripButton

    Sub New()
        InitializeComponent()
    End Sub

    Private Function SampleScript() As String
        Dim sb As New StringBuilder()
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

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ScriptEditorForm))
        StatusStrip1 = New StatusStrip()
        ToolStripStatusLabel1 = New ToolStripStatusLabel()
        ToolStrip1 = New ToolStrip()
        ToolStripLabel1 = New ToolStripLabel()
        ToolStripButton1 = New ToolStripButton()
        ToolStripSeparator1 = New ToolStripSeparator()
        ToolStripButton2 = New ToolStripButton()
        TextBox1 = New TextBox()
        cboPreset = New ToolStripComboBox()
        ToolStripLabel2 = New ToolStripLabel()
        StatusStrip1.SuspendLayout()
        ToolStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.Items.AddRange(New ToolStripItem() {ToolStripStatusLabel1})
        StatusStrip1.Location = New Point(0, 519)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Size = New Size(744, 22)
        StatusStrip1.TabIndex = 0
        StatusStrip1.Text = "StatusStrip1"
        ' 
        ' ToolStripStatusLabel1
        ' 
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New Size(39, 17)
        ToolStripStatusLabel1.Text = "Ready"
        ' 
        ' ToolStrip1
        ' 
        ToolStrip1.Items.AddRange(New ToolStripItem() {ToolStripLabel1, ToolStripButton1, ToolStripSeparator1, ToolStripButton2, ToolStripLabel2, cboPreset})
        ToolStrip1.Location = New Point(0, 0)
        ToolStrip1.Name = "ToolStrip1"
        ToolStrip1.Size = New Size(744, 25)
        ToolStrip1.TabIndex = 1
        ToolStrip1.Text = "ToolStrip1"
        ' 
        ' ToolStripLabel1
        ' 
        ToolStripLabel1.Name = "ToolStripLabel1"
        ToolStripLabel1.Size = New Size(98, 22)
        ToolStripLabel1.Text = "点击运行绘图："
        ' 
        ' ToolStripButton1
        ' 
        ToolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image
        ToolStripButton1.Image = CType(resources.GetObject("ToolStripButton1.Image"), Image)
        ToolStripButton1.ImageTransparentColor = Color.Magenta
        ToolStripButton1.Name = "ToolStripButton1"
        ToolStripButton1.Size = New Size(23, 22)
        ToolStripButton1.Text = "Run"
        ' 
        ' ToolStripSeparator1
        ' 
        ToolStripSeparator1.Name = "ToolStripSeparator1"
        ToolStripSeparator1.Size = New Size(6, 25)
        ' 
        ' ToolStripButton2
        ' 
        ToolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image
        ToolStripButton2.Image = CType(resources.GetObject("ToolStripButton2.Image"), Image)
        ToolStripButton2.ImageTransparentColor = Color.Magenta
        ToolStripButton2.Name = "ToolStripButton2"
        ToolStripButton2.Size = New Size(23, 22)
        ToolStripButton2.Text = "Demo Script"
        ' 
        ' TextBox1
        ' 
        TextBox1.Dock = DockStyle.Fill
        TextBox1.Location = New Point(0, 25)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(744, 494)
        TextBox1.TabIndex = 2
        ' 
        ' cboPreset
        ' 
        cboPreset.Name = "cboPreset"
        cboPreset.Size = New Size(121, 25)
        ' 
        ' ToolStripLabel2
        ' 
        ToolStripLabel2.Name = "ToolStripLabel2"
        ToolStripLabel2.Size = New Size(46, 22)
        ToolStripLabel2.Text = "预设："
        ' 
        ' ScriptEditorForm
        ' 
        ClientSize = New Size(744, 541)
        Controls.Add(TextBox1)
        Controls.Add(ToolStrip1)
        Controls.Add(StatusStrip1)
        Name = "ScriptEditorForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "脚本输入框 - 数学表达式可视化"
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        ToolStrip1.ResumeLayout(False)
        ToolStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        TextBox1.Text = SampleScript()
    End Sub

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Dim engine As New MathScriptEngine()
        Dim result = engine.RunScript(TextBox1.Text)
        If result.Success Then
            ToolStripLabel1.Text = "成功：" & result.Commands.Count & " 条绘图指令"
        Else
            ToolStripLabel1.Text = "错误：" & result.ErrorMessage
        End If
        RaiseEvent ScriptExecuted(result)
    End Sub
End Class
