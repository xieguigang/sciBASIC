#Region "Microsoft.VisualBasic::3ec0f424cfde435fa7a4ba4fe4cef64e, Data_science\Visualization\Expression3DPlotter\MainForm.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 337
'    Code Lines: 267 (79.23%)
' Comment Lines: 19 (5.64%)
'    - Xml Docs: 26.32%
' 
'   Blank Lines: 51 (15.13%)
'     File Size: 14.69 KB


' Class MainForm
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: Btn, Cbo, Chk, Lbl
'               Num, Txt
' 
'     Sub: ApplyMode, AxesChanged, InitializeComponent, MainForm_Load, ModeChanged
'          OnDraw, OnReset, OnZoom, UpdateStatus
' 
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.Scripting

''' <summary>
''' 三维数学表达式绘图器主窗体（light 亮色主题）。
''' 提供表达式输入框、采样范围/分辨率、配色选择、绘制与重置按钮，
''' 以及可交互的三维画布（左键旋转 / 右键平移 / 滚轮缩放）。
''' </summary>
Public Class MainForm : Inherits Form

    Private WithEvents canvas As SurfaceCanvas

    Private WithEvents statusStrip As StatusStrip
    Private WithEvents lblStatus As ToolStripStatusLabel


    ' 脚本模式相关
    Private WithEvents pic2D As PictureBox


    Friend WithEvents ToolStrip1 As ToolStrip
    Friend WithEvents ToolStripButton1 As ToolStripButton
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents ToolStripSplitButton1 As ToolStripSplitButton
    Friend WithEvents chkAxes As ToolStripMenuItem
    Friend WithEvents ToolStripLabel1 As ToolStripLabel
    Friend WithEvents cboScheme As ToolStripComboBox
    Friend WithEvents ToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem2 As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
    Friend WithEvents ToolStripLabel2 As ToolStripLabel
    Friend WithEvents ToolStripButton2 As ToolStripButton
    Friend WithEvents ToolStripButton3 As ToolStripButton
    Friend WithEvents ToolStripLabel3 As ToolStripLabel
    Friend WithEvents ToolStripComboBox1 As ToolStripComboBox

    Private editor As ScriptEditorForm = Nothing

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        ToolStrip1 = New ToolStrip()
        ToolStripButton3 = New ToolStripButton()
        ToolStripButton1 = New ToolStripButton()
        ToolStripSeparator1 = New ToolStripSeparator()
        ToolStripSplitButton1 = New ToolStripSplitButton()
        chkAxes = New ToolStripMenuItem()
        ToolStripMenuItem1 = New ToolStripMenuItem()
        ToolStripMenuItem2 = New ToolStripMenuItem()
        ToolStripLabel1 = New ToolStripLabel()
        cboScheme = New ToolStripComboBox()
        ToolStripSeparator2 = New ToolStripSeparator()
        ToolStripLabel2 = New ToolStripLabel()
        ToolStripButton2 = New ToolStripButton()
        pic2D = New PictureBox()
        statusStrip = New StatusStrip()
        lblStatus = New ToolStripStatusLabel()
        canvas = New SurfaceCanvas()
        ToolStripLabel3 = New ToolStripLabel()
        ToolStripComboBox1 = New ToolStripComboBox()
        ToolStrip1.SuspendLayout()
        CType(pic2D, ComponentModel.ISupportInitialize).BeginInit()
        statusStrip.SuspendLayout()
        SuspendLayout()
        ' 
        ' ToolStrip1
        ' 
        ToolStrip1.Items.AddRange(New ToolStripItem() {ToolStripButton3, ToolStripButton1, ToolStripSeparator1, ToolStripSplitButton1, ToolStripLabel3, ToolStripComboBox1, ToolStripLabel1, cboScheme, ToolStripSeparator2, ToolStripLabel2, ToolStripButton2})
        ToolStrip1.Location = New Point(0, 0)
        ToolStrip1.Name = "ToolStrip1"
        ToolStrip1.RenderMode = ToolStripRenderMode.Professional
        ToolStrip1.Size = New Size(1256, 25)
        ToolStrip1.TabIndex = 0
        ToolStrip1.Text = "ToolStrip1"
        ' 
        ' ToolStripButton3
        ' 
        ToolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Image
        ToolStripButton3.Image = CType(resources.GetObject("ToolStripButton3.Image"), Image)
        ToolStripButton3.ImageTransparentColor = Color.Magenta
        ToolStripButton3.Name = "ToolStripButton3"
        ToolStripButton3.Size = New Size(23, 22)
        ToolStripButton3.Text = "打开绘图脚本编辑器"
        ' 
        ' ToolStripButton1
        ' 
        ToolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image
        ToolStripButton1.Image = CType(resources.GetObject("ToolStripButton1.Image"), Image)
        ToolStripButton1.ImageTransparentColor = Color.Magenta
        ToolStripButton1.Name = "ToolStripButton1"
        ToolStripButton1.Size = New Size(23, 22)
        ToolStripButton1.Text = "重置视角"
        ' 
        ' ToolStripSeparator1
        ' 
        ToolStripSeparator1.Name = "ToolStripSeparator1"
        ToolStripSeparator1.Size = New Size(6, 25)
        ' 
        ' ToolStripSplitButton1
        ' 
        ToolStripSplitButton1.DisplayStyle = ToolStripItemDisplayStyle.Image
        ToolStripSplitButton1.DropDownItems.AddRange(New ToolStripItem() {chkAxes, ToolStripMenuItem1, ToolStripMenuItem2})
        ToolStripSplitButton1.Image = CType(resources.GetObject("ToolStripSplitButton1.Image"), Image)
        ToolStripSplitButton1.ImageTransparentColor = Color.Magenta
        ToolStripSplitButton1.Name = "ToolStripSplitButton1"
        ToolStripSplitButton1.Size = New Size(32, 22)
        ToolStripSplitButton1.Text = "ToolStripSplitButton1"
        ' 
        ' chkAxes
        ' 
        chkAxes.CheckOnClick = True
        chkAxes.Name = "chkAxes"
        chkAxes.Size = New Size(178, 22)
        chkAxes.Text = "绘制坐标轴"
        ' 
        ' ToolStripMenuItem1
        ' 
        ToolStripMenuItem1.CheckOnClick = True
        ToolStripMenuItem1.Image = CType(resources.GetObject("ToolStripMenuItem1.Image"), Image)
        ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        ToolStripMenuItem1.Size = New Size(178, 22)
        ToolStripMenuItem1.Text = "显示盒子网格面"
        ' 
        ' ToolStripMenuItem2
        ' 
        ToolStripMenuItem2.Checked = True
        ToolStripMenuItem2.CheckOnClick = True
        ToolStripMenuItem2.CheckState = CheckState.Checked
        ToolStripMenuItem2.Name = "ToolStripMenuItem2"
        ToolStripMenuItem2.Size = New Size(178, 22)
        ToolStripMenuItem2.Text = "显示带刻度坐标轴"
        ' 
        ' ToolStripLabel1
        ' 
        ToolStripLabel1.Name = "ToolStripLabel1"
        ToolStripLabel1.Size = New Size(59, 22)
        ToolStripLabel1.Text = "颜色模式"
        ' 
        ' cboScheme
        ' 
        cboScheme.DropDownStyle = ComboBoxStyle.DropDownList
        cboScheme.Items.AddRange(New Object() {"viridis", "magma", "inferno", "plasma", "turbo", "jet", "rainbow", "cividis", "mako", "rocket"})
        cboScheme.Name = "cboScheme"
        cboScheme.Size = New Size(121, 25)
        ' 
        ' ToolStripSeparator2
        ' 
        ToolStripSeparator2.Name = "ToolStripSeparator2"
        ToolStripSeparator2.Size = New Size(6, 25)
        ' 
        ' ToolStripLabel2
        ' 
        ToolStripLabel2.Name = "ToolStripLabel2"
        ToolStripLabel2.Size = New Size(85, 22)
        ToolStripLabel2.Text = "画布背景色："
        ' 
        ' ToolStripButton2
        ' 
        ToolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image
        ToolStripButton2.Image = CType(resources.GetObject("ToolStripButton2.Image"), Image)
        ToolStripButton2.ImageTransparentColor = Color.Magenta
        ToolStripButton2.Name = "ToolStripButton2"
        ToolStripButton2.Size = New Size(23, 22)
        ToolStripButton2.Text = "选择颜色"
        ' 
        ' pic2D
        ' 
        pic2D.Dock = DockStyle.Fill
        pic2D.Location = New Point(0, 25)
        pic2D.Name = "pic2D"
        pic2D.Size = New Size(1256, 789)
        pic2D.TabIndex = 0
        pic2D.TabStop = False
        ' 
        ' statusStrip
        ' 
        statusStrip.Items.AddRange(New ToolStripItem() {lblStatus})
        statusStrip.Location = New Point(0, 814)
        statusStrip.Name = "statusStrip"
        statusStrip.Size = New Size(1256, 22)
        statusStrip.TabIndex = 2
        ' 
        ' lblStatus
        ' 
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(0, 17)
        ' 
        ' canvas
        ' 
        canvas.BackColor = Color.FromArgb(CByte(200), CByte(213), CByte(215))
        canvas.Dock = DockStyle.Fill
        canvas.Location = New Point(0, 25)
        canvas.Name = "canvas"
        canvas.Scene = Nothing
        canvas.Size = New Size(1256, 789)
        canvas.TabIndex = 0
        canvas.TabStop = True
        ' 
        ' ToolStripLabel3
        ' 
        ToolStripLabel3.Name = "ToolStripLabel3"
        ToolStripLabel3.Size = New Size(124, 22)
        ToolStripLabel3.Text = "三维图形渲染模式："
        ' 
        ' ToolStripComboBox1
        ' 
        ToolStripComboBox1.DropDownStyle = ComboBoxStyle.DropDownList
        ToolStripComboBox1.Items.AddRange(New Object() {"surface", "point cloud", "edge"})
        ToolStripComboBox1.Name = "ToolStripComboBox1"
        ToolStripComboBox1.Size = New Size(121, 25)
        ToolStripComboBox1.ToolTipText = "三维图形渲染模式"
        ' 
        ' MainForm
        ' 
        BackColor = SystemColors.Control
        ClientSize = New Size(1256, 836)
        Controls.Add(canvas)
        Controls.Add(pic2D)
        Controls.Add(statusStrip)
        Controls.Add(ToolStrip1)
        Font = New Font("Segoe UI", 9F)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MinimumSize = New Size(600, 420)
        Name = "MainForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "三维数学表达式绘图器"
        ToolStrip1.ResumeLayout(False)
        ToolStrip1.PerformLayout()
        CType(pic2D, ComponentModel.ISupportInitialize).EndInit()
        statusStrip.ResumeLayout(False)
        statusStrip.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call ImageDriver.Register()
        Call LoadScriptInput()

        canvas.Scene = New PlotScene
        canvas.Scene.ShowAxes = chkAxes.Checked
        canvas.Scene.ShowBox = ToolStripMenuItem1.Checked
        canvas.Scene.ShowTicks = ToolStripMenuItem2.Checked
        canvas.Scene.Camera.AngleX = 125
        canvas.Scene.Camera.AngleX = 0
        canvas.Scene.Camera.AngleX = 0
        canvas.Scene.BackgroundColor = Color.FromArgb(200, 213, 215)

        ToolStripComboBox1.SelectedIndex = 0
        canvas.Scene.RenderMode = CType(ToolStripComboBox1.SelectedIndex, RenderMode3D)

        ' 默认演示：启动即渲染一个三维曲面
        Call OnDraw(Me, EventArgs.Empty)
        Call OnReset(Me, EventArgs.Empty)
    End Sub

    Private Sub AxesChanged(sender As Object, e As EventArgs) Handles chkAxes.CheckedChanged
        If canvas.Scene IsNot Nothing Then
            canvas.Scene.ShowAxes = chkAxes.Checked
            canvas.Invalidate()
        End If
    End Sub

    Private Sub OnBoxChecked(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        If canvas.Scene IsNot Nothing Then
            canvas.Scene.ShowBox = ToolStripMenuItem1.Checked
            canvas.Invalidate()
        End If
    End Sub

    Private Sub OnTicksChecked(sender As Object, e As EventArgs) Handles ToolStripMenuItem2.Click
        If canvas.Scene IsNot Nothing Then
            canvas.Scene.ShowTicks = ToolStripMenuItem2.Checked
            canvas.Invalidate()
        End If
    End Sub

    Private Sub LoadScriptInput() Handles ToolStripButton3.Click
        If editor Is Nothing OrElse editor.IsDisposed Then
            editor = New ScriptEditorForm()
            AddHandler editor.ScriptExecuted, AddressOf OnScriptExecuted
        End If
        If editor.Visible Then editor.BringToFront() Else editor.Show(Me)
    End Sub

    Private Sub OnDraw(sender As Object, e As EventArgs)
        If canvas.Scene Is Nothing Then Return
        canvas.Visible = True
        If pic2D IsNot Nothing Then pic2D.Visible = False
        Try
            Dim zEval = New ExpressionEvaluator("sin(sqrt(x*x + y*y))")
            canvas.Scene.SetSurface(zEval,
                    -8, 8,
                    -8, 8,
                    120, GetColorSchemaName())
            canvas.Invalidate()
            UpdateStatus("就绪")
        Catch ex As Exception
            UpdateStatus("表达式错误: " & ex.Message)
        End Try
    End Sub

    Private Function GetColorSchemaName() As String
        If cboScheme.SelectedIndex < 0 Then
            cboScheme.SelectedIndex = 0
        End If

        Return CStr(cboScheme.Items(cboScheme.SelectedIndex))
    End Function

    Private Sub OnReset(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        If canvas.Scene Is Nothing Then Return
        canvas.Scene.Camera.AngleX = 125
        canvas.Scene.Camera.AngleY = 0
        canvas.Scene.Camera.AngleZ = 0
        canvas.Scene.Camera.Offset = New PointF(0, 0)
        canvas.Scene.FitView()
        canvas.Invalidate()
        UpdateStatus()
    End Sub

    Private Sub OnZoom(delta As Integer) Handles canvas.Zoom
        If canvas.Scene Is Nothing Then Return
        Dim factor = If(delta > 0, 0.9F, 1.1F)
        canvas.Scene.Camera.ViewDistance = Math.Max(1.0F, canvas.Scene.Camera.ViewDistance * factor)
        canvas.Invalidate()
        UpdateStatus()
    End Sub

    Private Sub UpdateStatus(Optional msg$ = "")
        If canvas.Scene Is Nothing Then Return
        Dim s = canvas.Scene
        lblStatus.Text =
            $"{msg}  |  角度X:{s.Camera.AngleX:F1} Y:{s.Camera.AngleY:F1} Z:{s.Camera.AngleZ:F1} |  " &
            $"视距:{s.Camera.ViewDistance:F1}  |  半径:{s.ModelRadius:F1}  |  " &
            $"面:{s.SurfaceCount} 点:{s.PointCount}  |  配色:{GetColorSchemaName()}"
    End Sub

    Private Sub OnScriptExecuted(result As ScriptResult)
        If result Is Nothing OrElse Not result.Success Then
            UpdateStatus(If(result Is Nothing, "无结果", "脚本错误：" & result.ErrorMessage))
            Return
        End If
        If result.Commands.Count = 0 Then
            UpdateStatus("脚本未产生任何绘图指令")
            Return
        End If

        Dim cmds3D As New List(Of PlotCommand)()
        For Each c In result.Commands
            If c.Is3D Then cmds3D.Add(c)
        Next

        If cmds3D.Count > 0 Then
            canvas.Scene.Clear()
            For Each c In cmds3D
                Select Case c.Kind
                    Case PlotKind.Surface
                        canvas.Scene.SetSurface(c.X, c.Y, c.ZGrid)
                    Case PlotKind.Scatter
                        canvas.Scene.SetScatter(c.X, c.Y, c.Z)
                    Case PlotKind.Line
                        canvas.Scene.SetLine(c.X, c.Y, c.Z)
                End Select
            Next
            canvas.Visible = True
            If pic2D IsNot Nothing Then pic2D.Visible = False
            canvas.Invalidate()
            UpdateStatus("已渲染 " & cmds3D.Count & " 个三维绘图指令")
        Else
            Dim w = Math.Max(10, canvas.Width)
            Dim h = Math.Max(10, canvas.Height)
            Dim bmp = DataPlotView.Render(result.Commands, w, h)
            If pic2D.Image IsNot Nothing Then pic2D.Image.Dispose()
            pic2D.Image = bmp
            pic2D.Visible = True
            canvas.Visible = False
            UpdateStatus("已渲染 " & result.Commands.Count & " 个二维绘图指令")
        End If
    End Sub

    Private Sub cboScheme_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboScheme.SelectedIndexChanged
        If canvas.Scene Is Nothing Then Return
        canvas.Scene.ColorScheme = GetColorSchemaName()
        ' 立即按新调色板重新着色当前图形，无需等到下一次绘制
        canvas.Scene.Recolor()
        canvas.Invalidate()
        UpdateStatus()
    End Sub

    Private Sub ToolStripComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ToolStripComboBox1.SelectedIndexChanged
        If canvas.Scene Is Nothing Then Return
        If ToolStripComboBox1.SelectedIndex >= 0 Then
            canvas.Scene.RenderMode = CType(ToolStripComboBox1.SelectedIndex, RenderMode3D)
        End If
        ' 切换渲染模式后立即刷新当前三维图形
        canvas.Invalidate()
        UpdateStatus()
    End Sub

    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        Using color As New ColorDialog
            If color.ShowDialog = DialogResult.OK Then
                canvas.BackColor = color.Color
                canvas.Scene.BackgroundColor = color.Color
            End If
        End Using
    End Sub
End Class

