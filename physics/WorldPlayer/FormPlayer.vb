Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Microsoft.VisualBasic.Imaging.Driver
Imports std = System.Math

''' <summary>
''' 物理世界模拟器主窗体。
''' </summary>
''' <remarks>
''' 本窗体只提供"引擎外壳"：视口、控制台、工具栏、事件转发与渲染循环。
''' 场景搭建与交互逻辑全部来自 JavaScript 脚本（见 <c>demo/platform.js</c>）。
''' </remarks>
Public Class FormPlayer

    ''' <summary>离屏渲染缓冲，避免在 PictureBox 上直接绘制造成闪烁。</summary>
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

        AddHandler viewport.MouseDown, AddressOf Viewport_MouseDown
        AddHandler viewport.MouseMove, AddressOf Viewport_MouseMove
        AddHandler viewport.MouseUp, AddressOf Viewport_MouseUp
        AddHandler viewport.MouseWheel, AddressOf Viewport_MouseWheel
        AddHandler viewport.MouseEnter, AddressOf Viewport_MouseEnter
        AddHandler viewport.MouseLeave, AddressOf Viewport_MouseLeave
        AddHandler viewport.Resize, AddressOf Viewport_Resize
        AddHandler Me.KeyDown, AddressOf FormPlayer_KeyDown

        timer.Interval = 16
        AddHandler timer.Tick, AddressOf Timer_Tick
        timer.Start()

        lastFrameTime = Environment.TickCount64 / 1000.0
        lastFpsTick = lastFrameTime

        Dim startPath As String = FindStartScript()

        If startPath IsNot Nothing Then
            Call LoadScript(startPath)
        Else
            Call AppendConsole("未找到演示脚本 demo\platform.js", True)
        End If
    End Sub

    ''' <summary>定位要运行的脚本：优先 exe 目录下的 demo\platform.js，其次向上查找。</summary>
    Private Function FindStartScript() As String
        Dim candidates As New List(Of String)()
        Dim dir As DirectoryInfo = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)

        candidates.Add(Path.Combine(dir.FullName, DefaultScript))

        For i As Integer = 0 To 4
            If dir Is Nothing Then Exit For
            candidates.Add(Path.Combine(dir.FullName, DefaultScript))
            dir = dir.Parent
        Next

        ' 源码目录下的脚本（未复制到输出时也能跑）
        Dim probe As DirectoryInfo = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)

        For i As Integer = 0 To 8
            If probe Is Nothing Then Exit For
            candidates.Add(Path.Combine(probe.FullName, "WorldPlayer", DefaultScript))
            probe = probe.Parent
        Next

        For Each c As String In candidates
            If File.Exists(c) Then Return c
        Next

        Return Nothing
    End Function

    ''' <summary>装载并运行一个 JS 脚本，同时重建 API 绑定。</summary>
    Private Sub LoadScript(path As String)
        Dim source As String

        Try
            source = File.ReadAllText(path)
        Catch ex As Exception
            Call AppendConsole($"无法读取脚本: {ex.Message}", True)
            Return
        End Try

        Call host.Clear()

        script.ScriptPath = path
        host.LastError = Nothing

        Dim ok As Boolean = script.Load(source)

        If Not ok Then
            host.LastError = script.LastError
            lblStatus.Text = "错误"
            lblStatus.ForeColor = Color.FromArgb(248, 113, 113)
            Call UpdateStatus()
            Return
        End If

        api.Register()
        api.PendingSteps = 0.0

        Call script.Fire("init")

        lblScript.Text = "脚本: " & System.IO.Path.GetFileName(path)
        lblStatus.Text = "运行中"
        lblStatus.ForeColor = Color.FromArgb(52, 211, 153)

        Call UpdateStatus()
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
                Call AgeBodies(dt)
            Next
        Else
            Call host.Update(dt)
            Call AgeBodies(dt)
        End If

        ' 3) 相机缓动
        Call rig.Tick()

        ' 4) 渲染
        Call RenderFrame()

        ' 5) 统计
        frames += 1

        If now - lastFpsTick >= 0.5 Then
            fps = frames / (now - lastFpsTick)
            frames = 0
            lastFpsTick = now
            Call UpdateStatus()
        End If
    End Sub

    Private Sub AgeBodies(dt As Double)
        For Each v In host.Bodies
            v.Age += dt
        Next
    End Sub

    Private Sub RenderFrame()
        If viewport.Width <= 0 OrElse viewport.Height <= 0 Then Return

        If renderBuffer Is Nothing OrElse
           renderBuffer.Width <> viewport.Width OrElse
           renderBuffer.Height <> viewport.Height Then

            If renderBuffer IsNot Nothing Then renderBuffer.Dispose()
            renderBuffer = New Bitmap(viewport.Width, viewport.Height)
        End If

        Try
            Using g As Graphics = Graphics.FromImage(renderBuffer)
                renderer.HoverX = If(dragging, lastMouseX, api.MouseX)
                renderer.HoverY = If(dragging, lastMouseY, api.MouseY)
                Call renderer.Render(g, renderBuffer.Size)
            End Using

            If viewport.Image IsNot Nothing Then
                viewport.Image = Nothing
            End If

            viewport.Image = renderBuffer
        Catch
            ' 渲染失败不应中断仿真循环
        End Try
    End Sub

    ' /********************************************************************************/
    '  鼠标 / 键盘
    ' /********************************************************************************/

    Private Sub Viewport_MouseEnter(sender As Object, e As EventArgs)
        api.MouseX = -1.0
        api.MouseY = -1.0
    End Sub

    Private Sub Viewport_MouseLeave(sender As Object, e As EventArgs)
        api.MouseX = -1.0
        api.MouseY = -1.0
        renderer.HoverX = -1
        renderer.HoverY = -1
    End Sub

    Private Sub Viewport_MouseDown(sender As Object, e As MouseEventArgs)
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

    Private Sub Viewport_MouseMove(sender As Object, e As MouseEventArgs)
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

    Private Sub Viewport_MouseUp(sender As Object, e As MouseEventArgs)
        Dim button As Double = If(e.Button = MouseButtons.Left, 1.0, If(e.Button = MouseButtons.Right, 2.0, 3.0))

        If e.Button = MouseButtons.Right Then
            dragging = False
        End If

        api.MouseButton = 0.0

        If script.IsReady Then
            Call script.Fire("mouseup", CDbl(e.X), CDbl(e.Y), CDbl(button))
        End If
    End Sub

    Private Sub Viewport_MouseWheel(sender As Object, e As MouseEventArgs)
        Dim delta As Double = e.Delta / 120.0

        Call rig.Zoom(std.Exp(-delta * 0.18))

        If script.IsReady Then
            Call script.Fire("wheel", CDbl(delta))
        End If
    End Sub

    Private Sub Viewport_Resize(sender As Object, e As EventArgs)
        Call RenderFrame()
    End Sub

    Private Sub FormPlayer_KeyDown(sender As Object, e As KeyEventArgs)
        If script.IsReady Then
            Call script.Fire("keydown", e.KeyCode.ToString())
        End If

        If e.KeyCode = Keys.F5 Then
            Call ReloadCurrent()
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
                Call LoadScript(dlg.FileName)
            End If
        End Using
    End Sub

    Private Sub btnReload_Click(sender As Object, e As EventArgs) Handles btnReload.Click
        Call ReloadCurrent()
    End Sub

    Private Sub ReloadCurrent()
        If Not String.IsNullOrEmpty(script.ScriptPath) AndAlso File.Exists(script.ScriptPath) Then
            Call LoadScript(script.ScriptPath)
        End If
    End Sub

    Private Sub btnPause_Click(sender As Object, e As EventArgs) Handles btnPause.Click
        Call TogglePause()
    End Sub

    Private Sub TogglePause()
        host.Paused = Not host.Paused
        btnPause.Text = If(host.Paused, "继续", "暂停")
        Call UpdateStatus()
    End Sub

    Private Sub btnWireframe_Click(sender As Object, e As EventArgs) Handles btnWireframe.Click
        renderer.Wireframe = Not renderer.Wireframe
        btnWireframe.ForeColor = If(renderer.Wireframe, Color.FromArgb(34, 211, 238), Color.FromArgb(226, 232, 240))
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        Call host.Clear()

        If script.IsReady Then
            Call script.Fire("cleared")
        End If
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
    End Sub
End Class
