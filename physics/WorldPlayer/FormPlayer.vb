#Region "Microsoft.VisualBasic::937967b34947e109523148d042f420c0, physics\WorldPlayer\FormPlayer.vb"

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

    '   Total Lines: 800
    '    Code Lines: 528 (66.00%)
    ' Comment Lines: 89 (11.12%)
    '    - Xml Docs: 56.18%
    ' 
    '   Blank Lines: 183 (22.88%)
    '     File Size: 30.34 KB


    ' Class FormPlayer
    ' 
    '     Properties: EditorText
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: FindStartScript, ParseShotRequest, ParseSize, ProbeEditor, SaveEditorSource
    ' 
    '     Sub: AppendConsole, AutoRunTimer_Tick, btnClear_Click, btnLoadJs_Click, btnOpen_Click
    '          btnPause_Click, btnReload_Click, btnResetCamera_Click, btnRunJs_Click, btnSaveJs_Click
    '          btnWireframe_Click, Canvas_MouseDown, Canvas_MouseEnter, Canvas_MouseLeave, Canvas_MouseMove
    '          Canvas_MouseUp, Canvas_MouseWheel, Canvas_Paint, Canvas_Resize, Capture
    '          EditorBox_KeyDown, EditorBox_TextChanged, FormPlayer_FormClosed, FormPlayer_KeyDown, FormPlayer_Load
    '          OpenScriptIntoEditor, RenderFrame, RunEditorSource, SetupShotCapture, SwitchTo
    '          SyncLayout, Tabs_DrawItem, Tabs_SelectedIndexChanged, Timer_Tick, TogglePause
    '          UpdateStatus
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Microsoft.VisualBasic.Imaging.Driver
Imports std = System.Math

''' <summary>
''' 物理世界模拟器主窗体。
''' </summary>
''' <remarks>
''' 本窗体只提供"引擎外壳"：3D 画布、脚本编辑器、控制台、工具栏、事件转发与渲染循环。
''' 场景搭建与交互逻辑全部来自 JavaScript 脚本（见 <c>demo/platform.js</c>）。
''' </remarks>
Public Class FormPlayer

    ''' <summary>离屏渲染缓冲，尺寸始终等于画布页的客户区。</summary>
    Private renderBuffer As Bitmap

    Private host As WorldHost
    Private rig As CameraRig
    Private renderer As SceneRenderer
    Private script As ScriptHost
    Private api As JsApi

    ''' <summary>帧计数与 FPS 统计。</summary>
    Private frames As Integer = 0
    Private fps As Double = 0.0
    Private lastFpsTick As Double = 0.0
    Private lastFrameTime As Double = 0.0

    ''' <summary>右键拖拽轨道旋转的状态。</summary>
    Private dragging As Boolean = False
    Private lastMouseX As Integer = 0
    Private lastMouseY As Integer = 0

    ''' <summary>程序性改写编辑器内容时抑制自动运行。</summary>
    Private suppressAutoRun As Boolean = False

    ''' <summary>编辑器里上一次成功运行过的源码，用于提示"有未运行的改动"。</summary>
    Private lastRunSource As String = ""

    ''' <summary>上一次同步过的编辑器页尺寸。</summary>
    Private lastEditorPageSize As Size = Size.Empty
    ''' <summary>布局同步的重入守卫。</summary>
    Private syncingLayout As Boolean = False

    ''' <summary>默认演示脚本名（位于输出目录）。</summary>
    Private Const DefaultScript As String = "demo\platform.js"

    Shared Sub New()
        Call ImageDriver.Register()
    End Sub

    Public Sub New()
        InitializeComponent()
    End Sub

    ' /********************************************************************************/
    '  装载
    ' /********************************************************************************/

    Private Sub FormPlayer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        host = New WorldHost()
        rig = New CameraRig()
        renderer = New SceneRenderer(rig, host)

        script = New ScriptHost(AddressOf AppendConsole)
        api = New JsApi(host, rig, renderer, script)

        ' world / camera / sim / render 等全局必须在脚本顶层语句执行前注册
        script.BeforeRun = AddressOf api.Register

        AddHandler pageCanvas.MouseDown, AddressOf Canvas_MouseDown
        AddHandler pageCanvas.MouseMove, AddressOf Canvas_MouseMove
        AddHandler pageCanvas.MouseUp, AddressOf Canvas_MouseUp
        AddHandler pageCanvas.MouseWheel, AddressOf Canvas_MouseWheel
        AddHandler pageCanvas.MouseEnter, AddressOf Canvas_MouseEnter
        AddHandler pageCanvas.MouseLeave, AddressOf Canvas_MouseLeave
        AddHandler pageCanvas.Resize, AddressOf Canvas_Resize
        AddHandler pageCanvas.Paint, AddressOf Canvas_Paint
        AddHandler split.SplitterMoved, AddressOf Canvas_Resize
        AddHandler tabs.SelectedIndexChanged, AddressOf Tabs_SelectedIndexChanged
        AddHandler tabs.DrawItem, AddressOf Tabs_DrawItem

        AddHandler Me.KeyDown, AddressOf FormPlayer_KeyDown
        AddHandler editorBox.KeyDown, AddressOf EditorBox_KeyDown
        AddHandler editorBox.TextChanged, AddressOf EditorBox_TextChanged
        AddHandler autoRunTimer.Tick, AddressOf AutoRunTimer_Tick

        timer.Interval = 16
        AddHandler timer.Tick, AddressOf Timer_Tick
        timer.Start()

        lastFrameTime = Environment.TickCount64 / 1000.0
        lastFpsTick = lastFrameTime

        ' ---- 开发/验证用的截图模式 ----
        Dim shot As Size? = ParseShotRequest()

        If shot.HasValue Then
            Dim demo As String = FindStartScript()

            If demo IsNot Nothing Then
                Call OpenScriptIntoEditor(demo)
                Call RunEditorSource()
            End If

            Call SetupShotCapture(shot.Value)
            Return
        End If

        ' 无头自测模式：不进入常规交互循环
        If Environment.GetCommandLineArgs().Any(Function(a) a.Equals("--selftest", StringComparison.OrdinalIgnoreCase)) Then
            timer.Stop()

            AddHandler Me.Shown, Sub(s, ev)
                                     Dim code As Integer = SelfTest.Run(host, rig, renderer, script, api, AddressOf ProbeEditor)

                                     Environment.ExitCode = code
                                     Me.Close()
                                 End Sub

            Return
        End If

        Dim startPath As String = FindStartScript()

        If startPath IsNot Nothing Then
            Call OpenScriptIntoEditor(startPath)
            Call RunEditorSource()
        Else
            Call AppendConsole("未找到演示脚本 demo\platform.js", True)
            Call AppendConsole("请在【脚本编辑器】标签页里粘贴脚本，然后按 F5 运行", False)
        End If
    End Sub

    ''' <summary>定位默认脚本：优先 exe 目录下的 demo\platform.js，其次向上查找源码目录。</summary>
    Private Function FindStartScript() As String
        Dim candidates As New List(Of String)()
        Dim dir As DirectoryInfo = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)

        candidates.Add(System.IO.Path.Combine(dir.FullName, DefaultScript))

        For i As Integer = 0 To 4
            If dir Is Nothing Then Exit For

            candidates.Add(System.IO.Path.Combine(dir.FullName, DefaultScript))
            dir = dir.Parent
        Next

        Dim probe As DirectoryInfo = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)

        For i As Integer = 0 To 8
            If probe Is Nothing Then Exit For

            candidates.Add(System.IO.Path.Combine(probe.FullName, "WorldPlayer", DefaultScript))
            probe = probe.Parent
        Next

        For Each c As String In candidates
            If File.Exists(c) Then Return c
        Next

        Return Nothing
    End Function

    ' /********************************************************************************/
    '  脚本编辑器
    ' /********************************************************************************/

    ''' <summary>
    ''' 自测用：把源码放进编辑器并走一次真实的"运行"路径。
    ''' 返回 <c>ok|动态数|总数</c> 或 <c>error|错误信息</c>。
    ''' </summary>
    Public Function ProbeEditor(source As String) As String
        suppressAutoRun = True

        Try
            editorBox.Text = source
        Finally
            suppressAutoRun = False
        End Try

        Call RunEditorSource()

        If host.LastError IsNot Nothing Then Return "error|" & host.LastError

        Return $"ok|{host.DynamicCount}|{host.Bodies.Count}"
    End Function

    ''' <summary>自测用：编辑器当前内容（用于验证保存路径）。</summary>
    Public ReadOnly Property EditorText As String
        Get
            Return editorBox.Text
        End Get
    End Property

    ''' <summary>把脚本文件内容读进编辑器（自动运行由编辑器事件负责）。</summary>
    Private Sub OpenScriptIntoEditor(path As String)
        Dim source As String

        Try
            source = File.ReadAllText(path)
        Catch ex As Exception
            Call AppendConsole($"无法读取脚本: {ex.Message}", True)
            Return
        End Try

        suppressAutoRun = True

        Try
            editorBox.Text = source
        Finally
            suppressAutoRun = False
        End Try

        script.ScriptPath = path
        lblScript.Text = "脚本: " & System.IO.Path.GetFileName(path)

        Call AppendConsole($"已载入脚本 {path}（{source.Length} 字符）", False)
    End Sub

    ''' <summary>保存编辑器内容到当前脚本文件（没有路径时另存为）。</summary>
    Private Function SaveEditorSource(Optional saveAs As Boolean = False) As Boolean
        Dim path As String = script.ScriptPath

        If saveAs OrElse String.IsNullOrEmpty(path) Then
            Using dlg As New SaveFileDialog()
                dlg.Filter = "JavaScript 场景脚本 (*.js)|*.js|所有文件 (*.*)|*.*"
                dlg.Title = "保存 JavaScript 脚本"
                dlg.FileName = If(String.IsNullOrEmpty(path), "world.js", System.IO.Path.GetFileName(path))

                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return False

                path = dlg.FileName
            End Using
        End If

        Try
            File.WriteAllText(path, editorBox.Text)
        Catch ex As Exception
            Call AppendConsole($"保存失败: {ex.Message}", True)
            Return False
        End Try

        script.ScriptPath = path
        lblScript.Text = "脚本: " & System.IO.Path.GetFileName(path)
        Call AppendConsole($"已保存脚本 {path}", False)

        Return True
    End Function

    ''' <summary>
    ''' 用编辑器里的源码重建世界并运行。这是"实时编辑、实时观察"的核心动作。
    ''' </summary>
    Private Sub RunEditorSource()
        Dim source As String = editorBox.Text

        Call host.Clear()

        host.LastError = Nothing

        Dim ok As Boolean = script.Load(source)

        If Not ok Then
            host.LastError = script.LastError
            lblStatus.Text = "错误"
            lblStatus.ForeColor = Color.FromArgb(248, 113, 113)
            Call UpdateStatus()

            Return
        End If

        api.PendingSteps = 0.0
        lastRunSource = source

        Call script.Fire("init")

        lblStatus.Text = If(host.Paused, "已暂停", "运行中")
        lblStatus.ForeColor = If(host.Paused, Color.FromArgb(251, 191, 36), Color.FromArgb(52, 211, 153))

        Call UpdateStatus()
        Call RenderFrame(force:=True)
    End Sub

    Private Sub EditorBox_TextChanged(sender As Object, e As EventArgs)
        If suppressAutoRun Then Return

        If lastRunSource <> editorBox.Text Then
            lblStatus.Text = "有未运行改动"
            lblStatus.ForeColor = Color.FromArgb(251, 191, 36)
        End If

        If Not chkAutoRun.Checked Then Return

        autoRunTimer.Stop()
        autoRunTimer.Start()
    End Sub

    Private Sub AutoRunTimer_Tick(sender As Object, e As EventArgs)
        autoRunTimer.Stop()

        If suppressAutoRun Then Return

        Call RunEditorSource()
    End Sub

    Private Sub EditorBox_KeyDown(sender As Object, e As KeyEventArgs)
        ' Ctrl+Enter / F5：立即运行编辑器里的脚本
        If e.KeyCode = Keys.F5 OrElse (e.Control AndAlso e.KeyCode = Keys.Enter) Then
            Call RunEditorSource()
            e.Handled = True
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub btnLoadJs_Click(sender As Object, e As EventArgs) Handles btnLoadJs.Click
        Using dlg As New OpenFileDialog()
            dlg.Filter = "JavaScript 场景脚本 (*.js)|*.js|所有文件 (*.*)|*.*"
            dlg.Title = "加载 JavaScript 物理世界脚本"

            If dlg.ShowDialog(Me) = DialogResult.OK Then
                Call OpenScriptIntoEditor(dlg.FileName)

                If Not chkAutoRun.Checked Then
                    Call RunEditorSource()
                End If
            End If
        End Using
    End Sub

    Private Sub btnSaveJs_Click(sender As Object, e As EventArgs) Handles btnSaveJs.Click
        Call SaveEditorSource()
    End Sub

    Private Sub btnRunJs_Click(sender As Object, e As EventArgs) Handles btnRunJs.Click
        Call RunEditorSource()
    End Sub

    ' /********************************************************************************/
    '  主循环
    ' /********************************************************************************/

    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        Dim now As Double = Environment.TickCount64 / 1000.0
        Dim dt As Double = now - lastFrameTime

        lastFrameTime = now

        ' 夹住帧间隔：切窗口 / 断点后不要把大量时间灌进物理世界
        If dt <= 0.0 OrElse dt > 0.25 Then dt = 1.0 / 60.0

        ' 1) JS update 钩子（脚本可在步进前修改世界）
        If script.IsReady Then
            api.MouseButton = 0.0
            Call script.Fire("update", CDbl(dt))
        End If

        ' 2) 手动步进（sim.step(n)）
        If api.PendingSteps > 0.0 Then
            Dim n As Integer = CInt(std.Floor(api.PendingSteps))

            api.PendingSteps -= n

            For i As Integer = 1 To n
                Call host.Update(dt)
            Next
        Else
            Call host.Update(dt)
        End If

        ' 3) 相机缓动
        Call rig.Tick()

        ' 4) 布局同步（页内停靠子控件需要显式重排）
        Call SyncLayout()

        ' 5) 渲染（画布页不可见时跳过，省下整帧的投影与填充）
        Call RenderFrame()

        ' 6) 统计
        frames += 1

        If now - lastFpsTick >= 0.5 Then
            fps = frames / (now - lastFpsTick)
            frames = 0
            lastFpsTick = now
            Call UpdateStatus()
        End If
    End Sub

    ''' <summary>
    ''' 显式同步页内布局。
    ''' </summary>
    ''' <remarks>
    ''' <see cref="TabControl.PageSizes"/> 之类的缓存会让 <see cref="TabPage"/> 里的
    ''' <c>Dock</c> 子控件停留在设计尺寸，必须显式 <c>PerformLayout</c> 才会跟随。
    ''' 3D 画布不使用子控件（见 <see cref="CanvasTabPage"/>），因此这里只处理编辑器页。
    ''' </remarks>
    Private Sub SyncLayout()
        If syncingLayout Then Return
        If tabs Is Nothing OrElse pageScript Is Nothing Then Return
        If tabs.IsDisposed OrElse pageScript.IsDisposed Then Return

        syncingLayout = True

        Try
            Dim selected As TabPage = tabs.SelectedTab

            If selected Is pageScript AndAlso selected.Size <> lastEditorPageSize Then
                lastEditorPageSize = selected.Size
                selected.PerformLayout()
            End If
        Finally
            syncingLayout = False
        End Try
    End Sub

    ''' <summary>
    ''' 渲染一帧到离屏缓冲。缓冲尺寸始终跟随画布页客户区，
    ''' 因此窗口缩放时画面会立即按新尺寸重绘。
    ''' </summary>
    Private Sub RenderFrame(Optional force As Boolean = False)
        If pageCanvas Is Nothing OrElse pageCanvas.IsDisposed Then Return

        Dim size As Size = pageCanvas.ClientSize

        If size.Width <= 0 OrElse size.Height <= 0 Then Return

        ' 画布不在前台时不渲染：切回来时由 SelectedIndexChanged 强制补一帧
        If Not force AndAlso tabs.SelectedTab IsNot pageCanvas Then Return

        If renderBuffer Is Nothing OrElse renderBuffer.Size <> size Then
            If renderBuffer IsNot Nothing Then renderBuffer.Dispose()

            renderBuffer = New Bitmap(size.Width, size.Height)
        End If

        Try
            Using g As Graphics = Graphics.FromImage(renderBuffer)
                renderer.HoverX = If(dragging, lastMouseX, CInt(api.MouseX))
                renderer.HoverY = If(dragging, lastMouseY, CInt(api.MouseY))
                Call renderer.Render(g, size)
            End Using

            If tabs.SelectedTab Is pageCanvas Then
                pageCanvas.Invalidate()
            End If
        Catch
            ' 渲染失败不应中断仿真循环
        End Try
    End Sub

    ''' <summary>把离屏缓冲贴到画布页上。</summary>
    Private Sub Canvas_Paint(sender As Object, e As PaintEventArgs)
        If renderBuffer Is Nothing OrElse renderBuffer.Size <> pageCanvas.ClientSize Then
            Call RenderFrame(force:=True)
        End If

        If renderBuffer Is Nothing Then Return

        Call e.Graphics.DrawImageUnscaled(renderBuffer, 0, 0)
    End Sub

    ' /********************************************************************************/
    '  鼠标 / 键盘
    ' /********************************************************************************/

    Private Sub Canvas_MouseEnter(sender As Object, e As EventArgs)
        api.MouseX = -1.0
        api.MouseY = -1.0
    End Sub

    Private Sub Canvas_MouseLeave(sender As Object, e As EventArgs)
        api.MouseX = -1.0
        api.MouseY = -1.0
        renderer.HoverX = -1
        renderer.HoverY = -1
    End Sub

    Private Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs)
        lastMouseX = e.X
        lastMouseY = e.Y

        Dim button As Double = If(e.Button = MouseButtons.Left, 1.0, If(e.Button = MouseButtons.Right, 2.0, 3.0))

        api.MouseX = e.X
        api.MouseY = e.Y
        api.MouseButton = button

        ' 右键拖拽 = 轨道旋转（宿主持有的相机操作，脚本不参与）
        If e.Button = MouseButtons.Right Then
            dragging = True
        End If

        If script.IsReady Then
            Call script.Fire("mousedown", CDbl(e.X), CDbl(e.Y), CDbl(button))
        End If
    End Sub

    Private Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs)
        Dim dx As Double = e.X - lastMouseX
        Dim dy As Double = e.Y - lastMouseY

        lastMouseX = e.X
        lastMouseY = e.Y

        api.MouseX = e.X
        api.MouseY = e.Y

        If dragging Then
            Call rig.Orbit(dx * 0.4, dy * 0.4)
        End If

        If script.IsReady Then
            Dim button As Double = If(Control.MouseButtons = MouseButtons.Left, 1.0,
                                      If(Control.MouseButtons = MouseButtons.Right, 2.0, 3.0))

            api.MouseButton = button
            Call script.Fire("mousemove", CDbl(e.X), CDbl(e.Y), CDbl(dx), CDbl(dy), CDbl(button))
        End If
    End Sub

    Private Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs)
        Dim button As Double = If(e.Button = MouseButtons.Left, 1.0, If(e.Button = MouseButtons.Right, 2.0, 3.0))

        If e.Button = MouseButtons.Right Then
            dragging = False
        End If

        api.MouseButton = 0.0

        If script.IsReady Then
            Call script.Fire("mouseup", CDbl(e.X), CDbl(e.Y), CDbl(button))
        End If
    End Sub

    Private Sub Canvas_MouseWheel(sender As Object, e As MouseEventArgs)
        Dim delta As Double = e.Delta / 120.0

        Call rig.Zoom(std.Exp(-delta * 0.18))

        If script.IsReady Then
            Call script.Fire("wheel", CDbl(delta))
        End If
    End Sub

    ''' <summary>画布 / 分割条尺寸变化：立刻按新尺寸重建缓冲并重画。</summary>
    Private Sub Canvas_Resize(sender As Object, e As EventArgs)
        Call RenderFrame(force:=True)
    End Sub

    Private Sub Tabs_SelectedIndexChanged(sender As Object, e As EventArgs)
        ' 切换标签页时补一次页内布局；切回画布立刻补一帧，避免看到旧内容
        Call SyncLayout()
        Call RenderFrame(force:=True)
    End Sub

    ''' <summary>自绘标签头，配合整体深色工程仿真风格。</summary>
    Private Sub Tabs_DrawItem(sender As Object, e As DrawItemEventArgs)
        If e.Index < 0 OrElse e.Index >= tabs.TabPages.Count Then Return

        Dim page As TabPage = tabs.TabPages(e.Index)
        Dim active As Boolean = (tabs.SelectedIndex = e.Index)
        Dim back As Color = If(active, Color.FromArgb(30, 41, 59), Color.FromArgb(17, 28, 46))
        Dim fore As Color = If(active, Color.FromArgb(34, 211, 238), Color.FromArgb(148, 163, 184))

        Using fill As New SolidBrush(back)
            Call e.Graphics.FillRectangle(fill, e.Bounds)
        End Using

        If active Then
            Using accent As New SolidBrush(Color.FromArgb(34, 211, 238))
                Call e.Graphics.FillRectangle(accent, e.Bounds.Left, e.Bounds.Bottom - 2, e.Bounds.Width, 2)
            End Using
        End If

        Using border As New Pen(Color.FromArgb(51, 65, 85))
            Call e.Graphics.DrawLine(border, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
        End Using

        Using text As New SolidBrush(fore)
            Using format As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                Call e.Graphics.DrawString(page.Text, tabs.Font, text, e.Bounds, format)
            End Using
        End Using
    End Sub

    Private Sub FormPlayer_KeyDown(sender As Object, e As KeyEventArgs)
        If script.IsReady Then
            Call script.Fire("keydown", e.KeyCode.ToString())
        End If

        If e.KeyCode = Keys.F5 Then
            Call RunEditorSource()
            e.Handled = True
        ElseIf e.KeyCode = Keys.Space Then
            Call TogglePause()
            e.Handled = True
        End If
    End Sub

    ' /********************************************************************************/
    '  工具栏
    ' /********************************************************************************/

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        Using dlg As New OpenFileDialog()
            dlg.Filter = "JavaScript 场景脚本 (*.js)|*.js|所有文件 (*.*)|*.*"
            dlg.Title = "打开 JavaScript 物理世界脚本"

            If dlg.ShowDialog(Me) = DialogResult.OK Then
                Call OpenScriptIntoEditor(dlg.FileName)
                Call RunEditorSource()
            End If
        End Using
    End Sub

    Private Sub btnReload_Click(sender As Object, e As EventArgs) Handles btnReload.Click
        Call RunEditorSource()
    End Sub

    Private Sub btnPause_Click(sender As Object, e As EventArgs) Handles btnPause.Click
        Call TogglePause()
    End Sub

    Private Sub TogglePause()
        host.Paused = Not host.Paused
        btnPause.Text = If(host.Paused, "继续", "暂停")
        lblStatus.Text = If(host.Paused, "已暂停", "运行中")
        lblStatus.ForeColor = If(host.Paused, Color.FromArgb(251, 191, 36), Color.FromArgb(52, 211, 153))
        Call UpdateStatus()
    End Sub

    Private Sub btnWireframe_Click(sender As Object, e As EventArgs) Handles btnWireframe.Click
        renderer.Wireframe = Not renderer.Wireframe
        btnWireframe.ForeColor = If(renderer.Wireframe, Color.FromArgb(34, 211, 238), Color.FromArgb(226, 232, 240))

        Call RenderFrame(force:=True)
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        Call host.Clear()

        If script.IsReady Then
            Call script.Fire("cleared")
        End If

        Call RenderFrame(force:=True)
    End Sub

    Private Sub btnResetCamera_Click(sender As Object, e As EventArgs) Handles btnResetCamera.Click
        Call rig.Reset()
    End Sub

    ' /********************************************************************************/
    '  控制台与状态栏
    ' /********************************************************************************/

    ''' <summary>
    ''' 把一行文本追加到控制台。<paramref name="isError"/> 为 True 时用红色。
    ''' </summary>
    Private Sub AppendConsole(text As String, isError As Boolean)
        If consoleBox.IsDisposed Then Return

        If consoleBox.Lines.Length > UiScriptIO.MaxLines Then
            Dim keep As Integer = UiScriptIO.MaxLines \ 2
            Dim trimmed As String = String.Join(Environment.NewLine,
                                                consoleBox.Lines.Skip(std.Max(0, consoleBox.Lines.Length - keep)))

            consoleBox.Clear()
            consoleBox.AppendText(trimmed & Environment.NewLine)
        End If

        consoleBox.SelectionStart = consoleBox.TextLength
        consoleBox.SelectionColor = If(isError, Color.FromArgb(248, 113, 113), Color.FromArgb(148, 163, 184))
        consoleBox.AppendText(text & Environment.NewLine)
        consoleBox.SelectionStart = consoleBox.TextLength
        consoleBox.ScrollToCaret()
    End Sub

    Private Sub UpdateStatus()
        If statusText.IsDisposed Then Return

        statusText.Text = $"仿真 {host.SimTime:F1}s   刚体 {host.DynamicCount}   静态 {host.Bodies.Count - host.DynamicCount}   接触 {host.ContactCount}   FPS {fps:F0}"
    End Sub

    Private Sub FormPlayer_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        timer.Stop()
        autoRunTimer.Stop()
    End Sub

    ' /********************************************************************************/
    '  开发用：--shot 布局截图模式
    ' /********************************************************************************/

    ''' <summary>解析 <c>--shot 1400x900</c> 或 <c>--shot=1400x900</c>。</summary>
    Private Function ParseShotRequest() As Size?
        Dim args As String() = Environment.GetCommandLineArgs()

        For i As Integer = 0 To args.Length - 1
            Dim a As String = args(i)

            If a.Equals("--shot", StringComparison.OrdinalIgnoreCase) Then
                If i + 1 < args.Length Then Return ParseSize(args(i + 1))
            ElseIf a.StartsWith("--shot=", StringComparison.OrdinalIgnoreCase) Then
                Return ParseSize(a.Substring(7))
            End If
        Next

        Return Nothing
    End Function

    Private Shared Function ParseSize(text As String) As Size?
        Dim parts As String() = text.Split("x"c, "X"c)

        If parts.Length <> 2 Then Return Nothing

        Dim w As Integer
        Dim h As Integer

        If Integer.TryParse(parts(0), w) AndAlso Integer.TryParse(parts(1), h) Then
            Return New Size(w, h)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' 按指定客户区大小渲染两个标签页并落盘截图 + 控件尺寸报告。
    ''' 用于验证"窗口缩放时画布是否跟随"这类布局问题。
    ''' </summary>
    Private Sub SetupShotCapture(client As Size)
        timer.Stop()

        Me.ClientSize = client

        AddHandler Me.Shown, Sub(s, ev)
                                 For i As Integer = 1 To 20
                                     Application.DoEvents()
                                     System.Threading.Thread.Sleep(16)
                                 Next

                                 Dim outDir As String = AppDomain.CurrentDomain.BaseDirectory
                                 Dim text As New List(Of String) From {
                                     $"client = {Me.ClientSize.Width} x {Me.ClientSize.Height}",
                                     $"split = {split.Width} x {split.Height}   tabs = {tabs.Width} x {tabs.Height}",
                                     $"console = {consoleBox.Width} x {consoleBox.Height}"
                                 }

                                 ' 画布页
                                 Call SwitchTo(0)
                                 text.Add($"  [canvas] page={pageCanvas.Width}x{pageCanvas.Height} " &
                                          $"client={pageCanvas.ClientSize.Width}x{pageCanvas.ClientSize.Height} " &
                                          $"buffer={If(renderBuffer Is Nothing, "none", renderBuffer.Width & "x" & renderBuffer.Height)} " &
                                          $"camera={rig.Camera.Screen.Width}x{rig.Camera.Screen.Height}")
                                 Call Capture(Me, System.IO.Path.Combine(outDir, "shot-canvas.png"))

                                 ' 编辑器页
                                 Call SwitchTo(1)
                                 text.Add($"  [script] page={pageScript.Width}x{pageScript.Height} " &
                                          $"display={pageScript.DisplayRectangle.Width}x{pageScript.DisplayRectangle.Height} " &
                                          $"bar={editorBar.Width}x{editorBar.Height} " &
                                          $"editor={editorBox.Width}x{editorBox.Height}")
                                 Call Capture(Me, System.IO.Path.Combine(outDir, "shot-editor.png"))

                                 File.AppendAllLines(System.IO.Path.Combine(outDir, "shot-report.txt"), text)

                                 Application.Exit()
                             End Sub
    End Sub

    ''' <summary>切到指定标签页、推进布局并强制同步绘制。</summary>
    Private Sub SwitchTo(index As Integer)
        tabs.SelectedIndex = index

        For i As Integer = 1 To 3
            Application.DoEvents()
            Call SyncLayout()
            Call RenderFrame(force:=True)

            ' 强制完成一次同步绘制，保证截图里是当前画面
            If tabs.SelectedTab Is pageCanvas Then
                pageCanvas.Invalidate()
                pageCanvas.Update()
            End If

            Application.DoEvents()
        Next
    End Sub

    Private Shared Shadows Sub Capture(form As Form, path As String)
        Using bmp As New Bitmap(form.Width, form.Height)
            Call form.DrawToBitmap(bmp, New Rectangle(0, 0, form.Width, form.Height))
            Call bmp.Save(path, ImageFormat.Png)
        End Using
    End Sub
End Class
