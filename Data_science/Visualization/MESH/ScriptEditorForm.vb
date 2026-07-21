Imports System.ComponentModel
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
    Friend WithEvents ToolStripButton3 As ToolStripButton
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator

    ''' <summary>帮助窗口单例，避免重复打开多个实例。</summary>
    Private helpForm As ScriptHelpForm

    Sub New()
        InitializeComponent()
        LoadPresetScripts()
    End Sub

    ''' <summary>
    ''' 预设脚本项：显示名称 + 完整绘图脚本。
    ''' 由 MainForm 的预设曲面迁移而来，按 SampleScript() 的语法规则拼装。
    ''' </summary>
    Private Class PresetScript
        Public Name As String
        Public Script As String
    End Class

    ''' <summary>
    ''' 将预设表达式（原 MainForm 的 BuildPresets）按脚本语法转换为可选择的绘图脚本，
    ''' 并填充到 cboPreset。选择后仅载入 TextBox1，不自动运行。
    ''' </summary>
    Private Sub LoadPresetScripts()
        Dim presets = BuildPresetScripts()
        cboPreset.Items.Clear()
        For Each p In presets
            cboPreset.Items.Add(p.Name)
        Next
        cboPreset.SelectedIndex = -1
    End Sub

    Private Function BuildPresetScripts() As List(Of PresetScript)
        Dim list As New List(Of PresetScript)

        ' ---- 经典曲面（z = f(x, y)）----
        list.Add(MakeScript("正弦波纹 (Sine Ripple)", "sin(sqrt(x*x + y*y))", -8, 8, -8, 8))
        list.Add(MakeScript("抛物面 (Paraboloid)", "x*x + y*y", -5, 5, -5, 5))
        list.Add(MakeScript("鞍面 (Saddle)", "x*x - y*y", -5, 5, -5, 5))
        list.Add(MakeScript("双曲抛物面 (Hyperbolic Saddle)", "x*y", -5, 5, -5, 5))
        list.Add(MakeScript("高斯钟形 (Gaussian Bell)", "exp(-(x*x + y*y)/5)", -5, 5, -5, 5))
        list.Add(MakeScript("余弦波纹 (Cosine Ripple)", "cos(x) * cos(y)", -5, 5, -5, 5))
        list.Add(MakeScript("墨西哥帽 (Mexican Hat)", "(1 - x*x - y*y) * exp(-(x*x + y*y)/2)", -4, 4, -4, 4))
        list.Add(MakeScript("倒置抛物面 (Inverted Paraboloid)", "-(x*x + y*y)", -5, 5, -5, 5))
        list.Add(MakeScript("猴鞍面 (Monkey Saddle)", "x*x*x - 3*x*y*y", -3, 3, -3, 3))
        list.Add(MakeScript("混合正弦 (Sine-Cosine Mix)", "sin(x) + cos(y)", -5, 5, -5, 5))
        list.Add(MakeScript("对数曲面 (Logarithmic)", "log(1 + x*x + y*y)", -5, 5, -5, 5))
        list.Add(MakeScript("旋臂曲面 (Spiral Arms)", "sin(x*y)", -4, 4, -4, 4))

        ' ---- 更多曲面预设 ----
        list.Add(MakeScript("蛋盒 (Eggcrate)", "sin(x) * sin(y)", -6, 6, -6, 6))
        list.Add(MakeScript("阻尼波纹 (Damped Ripple)", "exp(-(x*x + y*y)/15) * sin(sqrt(x*x + y*y))", -8, 8, -8, 8))
        list.Add(MakeScript("圆锥 (Cone)", "sqrt(x*x + y*y)", -5, 5, -5, 5))
        list.Add(MakeScript("椭圆抛物面 (Elliptic Paraboloid)", "x*x/4 + y*y/2", -6, 6, -6, 6))
        list.Add(MakeScript("波纹干涉 (Interference)", "sin(x) + sin(y)", -6, 6, -6, 6))
        list.Add(MakeScript("交叉波 (Cross Wave)", "sin(x) * cos(y)", -6, 6, -6, 6))
        list.Add(MakeScript("波纹带 (Ripple Band)", "sin(x*x + y*y)", -4, 4, -4, 4))
        list.Add(MakeScript("高斯波包 (Gaussian Wave Packet)", "exp(-(x*x + y*y)/8) * cos(x*x + y*y)", -5, 5, -5, 5))
        list.Add(MakeScript("双高斯 (Twin Gaussian)", "exp(-((x-2)*(x-2) + y*y)/3) + exp(-((x+2)*(x+2) + y*y)/3)", -6, 6, -6, 6))
        list.Add(MakeScript("涟漪衰减 (Decaying Ripple)", "sin(sqrt(x*x + y*y)) / (1 + sqrt(x*x + y*y))", -10, 10, -10, 10))
        list.Add(MakeScript("峰谷交替 (Peaks Mix)", "sin(x) * exp(-y*y/8) + cos(y) * exp(-x*x/8)", -6, 6, -6, 6))

        ' ---- 基于函数库的三维曲面（覆盖 abs/asin/acos/atan/atan2/cosh/sinh/tanh/ln/log10/max/min/pow/sign/floor 等）----
        list.Add(MakeScript("双曲余弦碗 (Cosh Bowl)", "cosh(sqrt(x*x + y*y)/3)", -4, 4, -4, 4))
        list.Add(MakeScript("双曲正弦脊 (Sinh Ridge)", "sinh(x)/2 + sinh(y)/2", -3, 3, -3, 3))
        list.Add(MakeScript("双曲正切台 (Tanh Plateau)", "tanh(x) + tanh(y)", -5, 5, -5, 5))
        list.Add(MakeScript("绝对值锥 (Abs Cone)", "abs(x) + abs(y)", -5, 5, -5, 5))
        list.Add(MakeScript("反正弦叶 (Asin Leaf)", "asin(sin(x) * cos(y))", -3.14, 3.14, -3.14, 3.14))
        list.Add(MakeScript("反余弦冠 (Acos Crown)", "acos(cos(x) * cos(y))", -3.14, 3.14, -3.14, 3.14))
        list.Add(MakeScript("方位角面 (Atan2 Azimuth)", "atan2(y, x)", -5, 5, -5, 5))
        list.Add(MakeScript("反正切鞍 (Atan Saddle)", "atan(x) - atan(y)", -5, 5, -5, 5))
        list.Add(MakeScript("自然对数面 (Ln Surface)", "ln(1 + x*x + y*y)", -5, 5, -5, 5))
        list.Add(MakeScript("常用对数面 (Log10 Surface)", "log10(1 + x*x + y*y)", -5, 5, -5, 5))
        list.Add(MakeScript("最大脊 (Max Ridge)", "max(x, y)", -4, 4, -4, 4))
        list.Add(MakeScript("最小谷 (Min Valley)", "min(x*x, y*y)", -4, 4, -4, 4))
        list.Add(MakeScript("符号阶跃 (Sign Step)", "sign(sin(x)) + sign(cos(y))", -6.28, 6.28, -6.28, 6.28))
        list.Add(MakeScript("取整阶跃 (Floor Step)", "floor(x) + floor(y)", -5, 5, -5, 5))
        list.Add(MakeScript("幂曲面 (Pow Surface)", "pow(sin(x), 2) + pow(cos(y), 2)", -6.28, 6.28, -6.28, 6.28))
        list.Add(MakeScript("取整冠 (Ceiling Crown)", "ceiling(x) + ceiling(y)", -5, 5, -5, 5))
        list.Add(MakeScript("四舍五入面 (Round Surface)", "round(x) + round(y)", -5, 5, -5, 5))

        Return list
    End Function

    Private Function MakeScript(name$, expr$, xmin#, xmax#, ymin#, ymax#) As PresetScript
        Dim sb As New StringBuilder()
        sb.AppendLine("# " & name)
        sb.AppendLine($"x = axis({xmin}, {xmax}, n=80)")
        sb.AppendLine($"y = axis({ymin}, {ymax}, n=80)")
        sb.AppendLine($"z(x, y) = {expr}")
        sb.AppendLine("surface(x, y, z)")
        Return New PresetScript With {.Name = name, .Script = sb.ToString()}
    End Function

    Private Function MakeLineScript(name$, expr$, xmin#, xmax#, n%) As PresetScript
        Dim sb As New StringBuilder()
        sb.AppendLine("# " & name)
        sb.AppendLine($"x = axis({xmin}, {xmax}, n={n})")
        sb.AppendLine($"y(x) = {expr}")
        sb.AppendLine("line(x, y)")
        Return New PresetScript With {.Name = name, .Script = sb.ToString()}
    End Function

    Private Function MakeScatter3Script(name$, expr$, xmin#, xmax#, ymin#, ymax#, n%) As PresetScript
        Dim sb As New StringBuilder()
        sb.AppendLine("# " & name)
        sb.AppendLine($"x = axis({xmin}, {xmax}, n={n})")
        sb.AppendLine($"y = axis({ymin}, {ymax}, n={n})")
        sb.AppendLine($"z3(x, y) = {expr}")
        sb.AppendLine("scatter(x, y, z3(x, y))")
        Return New PresetScript With {.Name = name, .Script = sb.ToString()}
    End Function

    Private Function SampleScript() As String
        Dim sb As New StringBuilder()
        sb.AppendLine("# 三维曲面示例")
        sb.AppendLine("# 创建自变量x和y，分辨率为80个数据点")
        sb.AppendLine("x = axis(-3, 3, n=80)")
        sb.AppendLine("y = axis(-3, 3, n=80)")
        sb.AppendLine("z(x, y) = sin(sqrt(x*x + y*y))")
        sb.AppendLine("# 绘制三维曲面图")
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
        ToolStripLabel2 = New ToolStripLabel()
        cboPreset = New ToolStripComboBox()
        ToolStripSeparator2 = New ToolStripSeparator()
        ToolStripButton3 = New ToolStripButton()
        TextBox1 = New TextBox()
        StatusStrip1.SuspendLayout()
        ToolStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.Items.AddRange(New ToolStripItem() {ToolStripStatusLabel1})
        StatusStrip1.Location = New Point(0, 469)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Size = New Size(633, 22)
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
        ToolStrip1.AutoSize = False
        ToolStrip1.Items.AddRange(New ToolStripItem() {ToolStripLabel1, ToolStripButton1, ToolStripSeparator1, ToolStripButton2, ToolStripLabel2, cboPreset, ToolStripSeparator2, ToolStripButton3})
        ToolStrip1.Location = New Point(0, 0)
        ToolStrip1.Name = "ToolStrip1"
        ToolStrip1.RenderMode = ToolStripRenderMode.System
        ToolStrip1.Size = New Size(633, 30)
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
        ToolStripButton1.Size = New Size(23, 27)
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
        ' ToolStripLabel2
        ' 
        ToolStripLabel2.Name = "ToolStripLabel2"
        ToolStripLabel2.Size = New Size(46, 22)
        ToolStripLabel2.Text = "预设："
        ' 
        ' cboPreset
        ' 
        cboPreset.DropDownStyle = ComboBoxStyle.DropDownList
        cboPreset.MaxDropDownItems = 20
        cboPreset.Name = "cboPreset"
        cboPreset.Size = New Size(200, 25)
        ' 
        ' ToolStripSeparator2
        ' 
        ToolStripSeparator2.Name = "ToolStripSeparator2"
        ToolStripSeparator2.Size = New Size(6, 25)
        ' 
        ' ToolStripButton3
        ' 
        ToolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text
        ToolStripButton3.Name = "ToolStripButton3"
        ToolStripButton3.Size = New Size(40, 22)
        ToolStripButton3.Text = "帮助"
        ' 
        ' TextBox1
        ' 
        TextBox1.Dock = DockStyle.Fill
        TextBox1.Font = New Font("Microsoft YaHei", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        TextBox1.Location = New Point(0, 30)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(633, 439)
        TextBox1.TabIndex = 2
        ' 
        ' ScriptEditorForm
        ' 
        ClientSize = New Size(633, 491)
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

    Private Sub PresetSelected(sender As Object, e As EventArgs) Handles cboPreset.SelectedIndexChanged
        If cboPreset.SelectedIndex < 0 Then Return
        Dim presets = BuildPresetScripts()
        If cboPreset.SelectedIndex >= presets.Count Then Return
        ' 仅将预设脚本载入编辑框，供用户查看/编辑后手动运行
        TextBox1.Text = presets(cboPreset.SelectedIndex).Script
    End Sub

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Dim engine As New MathScriptEngine()
        Dim result = engine.RunScript(TextBox1.Text)
        If result.Success Then
            ToolStripStatusLabel1.Text = "成功：" & result.Commands.Count & " 条绘图指令"
        Else
            ToolStripStatusLabel1.Text = "错误：" & result.ErrorMessage
        End If
        RaiseEvent ScriptExecuted(result)
    End Sub

    Private Sub ToolStripButton3_Click(sender As Object, e As EventArgs) Handles ToolStripButton3.Click
        If helpForm Is Nothing OrElse helpForm.IsDisposed Then
            helpForm = New ScriptHelpForm()
        End If
        If Not helpForm.Visible Then
            helpForm.Show(Me)
        End If
        helpForm.BringToFront()
    End Sub
End Class
