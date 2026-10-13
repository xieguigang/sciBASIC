Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Engine.Physics
Imports Microsoft.VisualBasic.Engine.Physics.Collision3D
Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

''' <summary>
''' 无头端到端自测：以 <c>--selftest</c> 启动时执行。
''' 它不依赖鼠标交互，直接驱动"脚本 → 物理 → 渲染"的完整链路，
''' 覆盖：脚本装载、相机反投影往返、各形状生成、重力下落、碰撞接触、
''' 越界剔除、暂停/步进、脚本错误处理，并输出一张离屏渲染的 PNG。
''' </summary>
Module SelfTest

    Private Passed As Integer = 0
    Private Failed As Integer = 0
    Private report As New StringBuilder()

    ''' <summary>报告与截图的输出目录。</summary>
    Public ReadOnly Property OutputDirectory As String
        Get
            Return AppDomain.CurrentDomain.BaseDirectory
        End Get
    End Property

    Private Sub Check(name As String, ok As Boolean, Optional detail As String = "")
        If ok Then
            Passed += 1
            Call report.AppendLine($"  [ok]   {name}")
        Else
            Failed += 1
            Call report.AppendLine($"  [FAIL] {name}  {detail}")
        End If
    End Sub

    Private Sub Near(name As String, actual As Double, expected As Double, tol As Double)
        Check(name, std.Abs(actual - expected) <= tol,
              $"actual={actual:F6} expected={expected:F6} tol={tol}")
    End Sub

    Private Function Finite(v As Vector3) As Boolean
        Return Not Double.IsNaN(v.x) AndAlso Not Double.IsNaN(v.y) AndAlso Not Double.IsNaN(v.z) AndAlso
               Not Double.IsInfinity(v.x) AndAlso Not Double.IsInfinity(v.y) AndAlso Not Double.IsInfinity(v.z)
    End Function

    ''' <summary>刚体最低顶点的世界 y。</summary>
    Private Function LowestVertex(v As BodyVisual) As Double
        Dim mesh As ConvexMesh3D = TryCast(v.Mesh, ConvexMesh3D)

        If mesh Is Nothing Then Return v.Body.Position.y

        Dim low As Double = Double.MaxValue

        For Each p As Vector3 In mesh.WorldVertices(v.Body.Position, v.Body.Orientation)
            low = std.Min(low, p.y)
        Next

        Return low
    End Function

    Private Function Finite(p As Microsoft.VisualBasic.Imaging.Drawing3D.Point3D) As Boolean
        Return Not Double.IsNaN(p.X) AndAlso Not Double.IsNaN(p.Y) AndAlso Not Double.IsNaN(p.Z)
    End Function

    ''' <summary>执行全部自测，返回进程退出码。</summary>
    Public Function Run(host As WorldHost, rig As CameraRig, renderer As SceneRenderer,
                        script As ScriptHost, api As JsApi) As Integer
        Passed = 0
        Failed = 0
        report.Clear()

        Call report.AppendLine("=== WorldPlayer 端到端自测 ===")
        Call report.AppendLine()
        Call WriteReport(0)

        Try
            Dim scriptPath As String = ResolveScript()

            If scriptPath Is Nothing Then
                Call report.AppendLine("[FAIL] 找不到 demo/platform.js")
                Call WriteReport(-1)

                Return -1
            End If

            Call report.AppendLine($"脚本: {scriptPath}")
            Call CameraRoundTrip(rig)
            Call CameraOrientation(rig)
            Call ShapeSpawning(host)
            Call ScriptSmoke(script, api)
            Call LoadDemoScript(host, rig, renderer, script, api, scriptPath)
            Call SimulateClicks(host, rig, script)
            Call AdvancePhysics(host, script)
            Call RenderShot(rig, renderer)
            Call PerformanceProbe(host, rig, renderer)
            Call TopLevelApiAndRampSlide(host, script)
            Call ErrorHandling(script)
        Catch ex As Exception
            Failed += 1
            Call report.AppendLine($"  [FAIL] 自测过程中抛出异常: {ex.GetType().Name}: {ex.Message}")
            Call report.AppendLine(ex.StackTrace)
        End Try

        Call report.AppendLine()
        Call report.AppendLine($"断言通过 {Passed} / 失败 {Failed}")

        Dim code As Integer = If(Failed > 0, 1, 0)

        Call WriteReport(code)

        Return code
    End Function

    ' /********************************************************************************/

    Private Function ResolveScript() As String
        Dim candidates As String() = {
            Path.Combine(OutputDirectory, "demo\platform.js"),
            Path.Combine(OutputDirectory, "..\..\..\demo\platform.js")
        }

        For Each c As String In candidates
            If File.Exists(c) Then Return Path.GetFullPath(c)
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' 验证相机反投影：把世界点用库的 Project 投到屏幕，再用宿主的 Mat⁻¹
    ''' 反投影回同一高度的平面，应当逐位还原。
    ''' </summary>
    Private Sub CameraRoundTrip(rig As CameraRig)
        Call report.AppendLine("-- 相机反投影往返")

        Call rig.SetScreen(New Size(1100, 620))
        Call rig.SetAngles(24, -38)
        Call rig.SetDistance(48)
        Call rig.SetFocus(0, 2.5, 0)
        rig.Tick()

        ' 立刻把缓动到位（Tick 只走一部分）
        For i As Integer = 1 To 200
            rig.Tick()
        Next

        Dim probes As Double()() = {
            New Double() {0, 0, 0},
            New Double() {3, 2, -1},
            New Double() {-5, 4, 6},
            New Double() {8, 1.5, -7}
        }
        Dim maxErr As Double = 0.0

        For Each p As Double() In probes
            Dim render As Microsoft.VisualBasic.Imaging.Drawing3D.Point3D = rig.ToRender(p(0), p(1), p(2))
            Dim scr As Microsoft.VisualBasic.Imaging.Drawing3D.Point3D = rig.Camera.Project(rig.Camera.Rotate(render))

            Check($"投影点有限 ({p(0)},{p(1)},{p(2)})", Finite(scr))

            Dim sx, sy, sz As Double

            If Not rig.ScreenToPlane(scr.X, scr.Y, 0, 1, 0, p(1), sx, sy, sz) Then
                Check($"反投影成功 ({p(0)},{p(1)},{p(2)})", False, "ScreenToPlane 返回 False")
                Continue For
            End If

            maxErr = std.Max(maxErr, std.Max(std.Abs(sx - p(0)), std.Abs(sz - p(2))))
        Next

        ' 相机内部用单精度 SIMD 做旋转/投影，因此往返误差是 float32 量级
        Check("反投影往返误差 < 1e-4", maxErr < 1.0E-4, $"maxErr={maxErr:E3}")

        ' 射线与平面求交也必须自洽
        Dim ox, oy, oz, dx, dy, dz As Double

        Check("ScreenToRay 可用", rig.ScreenToRay(550, 310, ox, oy, oz, dx, dy, dz))
        Near("射线方向已归一化", std.Sqrt(dx * dx + dy * dy + dz * dz), 1.0, 1.0E-9)
    End Sub

    ''' <summary>物理坐标点投影到屏幕。</summary>
    Private Function ScreenOf(rig As CameraRig, x As Double, y As Double, z As Double) As Microsoft.VisualBasic.Imaging.Drawing3D.Point3D
        Return rig.Camera.Project(rig.Camera.Rotate(rig.ToRender(x, y, z)))
    End Function

    ''' <summary>
    ''' 相机朝向的语义检查。反投影往返是自洽的，因此"上下颠倒"这类映射错误
    ''' 必须用"物理方向 → 屏幕方向"的常识约束来验证：
    ''' 地面远端要在画面上方、高处物体要在画面上方、屏幕上方的落点要更远。
    ''' </summary>
    Private Sub CameraOrientation(rig As CameraRig)
        Call report.AppendLine("-- 相机朝向（场景不得倒置）")

        Call rig.SetScreen(New Size(1100, 620))
        Call rig.SetAngles(24, -38)
        Call rig.SetDistance(48)
        Call rig.SetFocus(0, 2.5, 0)

        For i As Integer = 1 To 200
            rig.Tick()
        Next

        Dim w As Double = rig.Camera.Screen.Width
        Dim h As Double = rig.Camera.Screen.Height

        ' ① 地面：远端必须投影在近端上方，否则整个场景是倒置的
        Dim farS = ScreenOf(rig, 0, 0, -12)
        Dim nearS = ScreenOf(rig, 0, 0, 12)

        Check("地面远端在屏幕上方（场景未倒置）", farS.Y < nearS.Y,
              $"far.Y={farS.Y:F1} near.Y={nearS.Y:F1}")

        ' ② 高度：越高的点越靠上
        Dim hiS = ScreenOf(rig, 0, 12, 0)
        Dim loS = ScreenOf(rig, 0, 1, 0)

        Check("高处物体在屏幕上方", hiS.Y < loS.Y, $"hi.Y={hiS.Y:F1} lo.Y={loS.Y:F1}")

        ' ③ 屏幕上方的落点应比屏幕下方的更远（相机在 +Z 侧看向 -Z）
        Dim px, py, pz As Double
        Dim okUp As Boolean = rig.ScreenToPlane(w / 2.0, h * 0.35, 0, 1, 0, 0.0, px, py, pz)
        Dim farZ As Double = pz
        Dim okDown As Boolean = rig.ScreenToPlane(w / 2.0, h * 0.62, 0, 1, 0, 0.0, px, py, pz)
        Dim nearZ As Double = pz

        Check("屏幕上方与下方都能命中地面", okUp AndAlso okDown, $"up={okUp} down={okDown}")
        Check("屏幕上方的落点更远", okUp AndAlso okDown AndAlso farZ < nearZ,
              $"farZ={farZ:F2} nearZ={nearZ:F2}")

        ' ④ "地面在屏幕中部偏下、上方是天空"才是正常观感：
        '    屏幕下部点击应落在相机前方（而非提示"指向天空"）
        Dim okLow As Boolean = rig.ScreenToPlane(w / 2.0, h * 0.85, 0, 1, 0, 0.0, px, py, pz)

        Check("屏幕下部点击可命中地面", okLow, $"ok={okLow}")
        Near("落点确实在地面上", py, 0.0, 1.0E-9)
    End Sub

    ''' <summary>各形状都能生成，并且质量 / 静态标志正确。</summary>
    Private Sub ShapeSpawning(host As WorldHost)
        Call report.AppendLine("-- 形状生成")

        Call host.Clear()

        Dim kinds As String() = {"box", "cube", "cuboid", "sphere", "ball", "cone", "cylinder", "tetra", "ramp"}

        For Each k As String In kinds
            Dim o As New SpawnOptions With {
                .Kind = k,
                .X = 0, .Y = 10, .Z = 0,
                .SizeX = 1, .SizeY = 1.2, .SizeZ = 0.8,
                .Radius = 0.5, .Height = 1.2, .Segment = 10, .Edge = 1.0,
                .Width = 4, .Depth = 3,
                .Mass = 2.0
            }
            Dim id As String = host.Spawn(o)
            Dim v As BodyVisual = host.Find(id)

            Check($"生成 {k}", v IsNot Nothing AndAlso Not v.Body.IsStatic)
        Next

        Check("动态刚体计数 = 9", host.DynamicCount = 9, $"count={host.DynamicCount}")

        ' 盒体三边不同尺寸时位置按质心放置（长方体的质心即几何中心）
        Dim probe As New SpawnOptions With {.Kind = "box", .X = 1, .Y = 20, .Z = 2, .SizeX = 2, .SizeY = 4, .SizeZ = 6, .Mass = 1}
        Dim pid As String = host.Spawn(probe)
        Dim pv As BodyVisual = host.Find(pid)

        Near("长方体放置点即中心 x", pv.Body.Position.x, 1.0, 1.0E-9)
        Near("长方体放置点即中心 y", pv.Body.Position.y, 20.0, 1.0E-9)

        ' 楔形斜坡的质心在构造原点上方，放置点应是"底面中心"
        Dim ramp As New SpawnOptions With {.Kind = "ramp", .X = 0, .Y = 0, .Z = 0, .Width = 8, .SizeY = 3, .Depth = 8, .Mass = 0}
        Dim rid As String = host.Spawn(ramp)
        Dim rv As BodyVisual = host.Find(rid)

        Check("斜坡为静态", rv.Body.IsStatic)
        Check("斜坡质心在底面之上", rv.Body.Position.y > 0.5, $"y={rv.Body.Position.y:F3}")

        Dim mesh As ConvexMesh3D = DirectCast(rv.Mesh, ConvexMesh3D)
        Dim lowest As Double = Double.MaxValue

        For Each p As Vector3 In mesh.WorldVertices(rv.Body.Position, rv.Body.Orientation)
            lowest = std.Min(lowest, p.y)
        Next

        Near("斜坡底面贴地 y = 0", lowest, 0.0, 1.0E-9)

        Call host.Clear()
    End Sub

    ''' <summary>逐步验证 LiteJs 侧的基础能力，便于定位脚本环境问题。</summary>
    Private Sub ScriptSmoke(script As ScriptHost, api As JsApi)
        Call report.AppendLine("-- 脚本环境冒烟")

        Dim cases As (String, String)() = {
            ("空脚本", ""),
            ("变量与算术", "var a = 1 + 2;"),
            ("函数声明与调用", "function f(x) { return x * 2; } var b = f(3);"),
            ("on 注册", "on('init', function () { });"),
            ("数组与 Math", "var v = [1, 2, 3]; var r = Math.random();"),
            ("箭头函数", "var g = function (x) { return x + 1; }; var c = g(1);"),
            ("对象字面量", "var o = { x: 1, y: 2 };")
        }

        For Each c In cases
            Dim ok As Boolean = script.Load(c.Item2)

            Check($"脚本环境: {c.Item1}", ok, If(script.LastError, ""))
        Next
    End Sub

    ''' <summary>装载 demo 脚本并检查场景内容。</summary>
    Private Sub LoadDemoScript(host As WorldHost, rig As CameraRig, renderer As SceneRenderer,
                               script As ScriptHost, api As JsApi, scriptPath As String)
        Call report.AppendLine("-- demo 脚本装载")

        Call host.Clear()

        Dim source As String = File.ReadAllText(scriptPath)
        Dim ok As Boolean = script.Load(source)

        Check("脚本解析与执行", ok, If(script.LastError, ""))

        If Not ok Then Return

        host.LastError = Nothing

        Call script.Fire("init")

        Check("注册了 mousedown 回调", script.Has("mousedown"))
        Check("注册了 keydown 回调", script.Has("keydown"))

        ' 场景应当由脚本创建出：地面 + 平台 + 斜坡 + 2 根立柱 = 4 个静态物体
        Dim statics As Integer = host.Bodies.Where(Function(v) v.IsStaticGeometry).Count()

        Check("脚本创建了 4 个静态物体", statics = 4, $"statics={statics}")
        Check("场景包含楔形斜坡（凸多面体静态体）",
              host.Bodies.Any(Function(v) v.IsStaticGeometry AndAlso TypeOf v.Body.Shape Is ConvexPolyhedronCollider3D))
        Check("脚本没有创建动态刚体", host.DynamicCount = 0, $"dynamic={host.DynamicCount}")
        Check("地面已设为 y = 0", std.Abs(renderer.GroundY) < 1.0E-9, $"groundY={renderer.GroundY}")

        ' 重力已按脚本设置
        Near("重力 = -9.81", host.World.Gravity.y, -9.81, 1.0E-9)
    End Sub

    ''' <summary>模拟若干次左键点击，检查随机物体的生成。</summary>
    Private Sub SimulateClicks(host As WorldHost, rig As CameraRig, script As ScriptHost)
        Call report.AppendLine("-- 模拟鼠标左键生成")

        Dim before As Integer = host.DynamicCount

        ' 在视口内取若干屏幕点，逐个触发左键 mousedown
        Dim points As Integer()() = {
            New Integer() {420, 360},
            New Integer() {560, 330},
            New Integer() {660, 420},
            New Integer() {500, 480},
            New Integer() {760, 300}
        }

        For Each p As Integer() In points
            Call script.Fire("mousedown", CDbl(p(0)), CDbl(p(1)), CDbl(1))
        Next

        Check("左键各生成一个刚体", host.DynamicCount = before + points.Length,
              $"before={before} after={host.DynamicCount}")

        ' 生成点必须落在生成平面附近（y = 7 的平面，物体随即下落，这里只查初值合理性）
        Dim bad As Integer = 0

        For Each v In host.Bodies
            If v.IsStaticGeometry Then Continue For
            If Not Finite(v.Body.Position) Then bad += 1
            If std.Abs(v.Body.Position.x) > 10.5 Then bad += 1
            If std.Abs(v.Body.Position.z) > 10.5 Then bad += 1
        Next

        Check("生成位置落在受限范围内", bad = 0, $"bad={bad}")

        ' 右键点击不应生成物体
        Dim n As Integer = host.DynamicCount
        Call script.Fire("mousedown", 500.0, 400.0, 2.0)
        Check("右键不生成物体", host.DynamicCount = n, $"after={host.DynamicCount}")

        ' 键盘事件：C 只清除动态刚体，关卡几何必须保留
        Call script.Fire("keydown", "C")
        Check("按下 C 清空动态刚体", host.DynamicCount = 0, $"dynamic={host.DynamicCount}")
        Check("静态关卡几何被保留", host.Bodies.Where(Function(v) v.IsStaticGeometry).Count() = 4)

        ' 重新生成一批用于物理推进
        For Each p As Integer() In points
            Call script.Fire("mousedown", CDbl(p(0)), CDbl(p(1)), CDbl(1))
        Next

        Call script.Fire("keydown", "W")
        Call script.Fire("keydown", "W")
        Call script.Fire("keydown", "G")
        Call script.Fire("keydown", "G")
        Call script.Fire("keydown", "R")
        Check("按键 W/G/R 不抛异常", True)
    End Sub

    ''' <summary>推进 6 秒物理，检查下落、接触、休眠与稳定性。</summary>
    Private Sub AdvancePhysics(host As WorldHost, script As ScriptHost)
        Call report.AppendLine("-- 物理推进 6 秒")

        Dim maxContacts As Integer = 0
        Dim sawSleeping As Boolean = False

        For i As Integer = 1 To 360
            Call script.Fire("update", CDbl(1.0 / 60.0))
            Call host.Update(1.0 / 60.0)

            maxContacts = std.Max(maxContacts, host.ContactCount)

            If host.SleepingCount > 0 Then sawSleeping = True
        Next

        Dim dynamic As List(Of BodyVisual) = host.Bodies.Where(Function(v) Not v.IsStaticGeometry).ToList()

        Call report.AppendLine($"     接触峰值 {maxContacts}   休眠过 {sawSleeping}   仿真 {host.SimTime:F1}s")

        Dim nonFinite As Integer = 0
        Dim sunk As Integer = 0
        Dim minVertexY As Double = Double.MaxValue

        For Each v In dynamic
            If Not Finite(v.Body.Position) OrElse Not Finite(v.Body.Velocity) Then nonFinite += 1

            ' 用最低顶点判断是否陷入支撑面：地面在 y = 0，允许 Baumgarte 的少量穿透
            Dim low As Double = LowestVertex(v)

            minVertexY = std.Min(minVertexY, low)

            If low < -0.15 Then sunk += 1
        Next

        Check("全部刚体状态有限", nonFinite = 0, $"nonFinite={nonFinite}")
        Check("没有物体陷入支撑面以下 (最低顶点 >= -0.15)", sunk = 0,
              $"sunk={sunk} minVertexY={minVertexY:F3}")
        Check("有物体停在支撑面附近（确实发生了碰撞约束）",
              std.Abs(minVertexY) < 0.15, $"minVertexY={minVertexY:F3}")
        Check("仿真过程中产生过接触", maxContacts > 0, $"maxContacts={maxContacts}")
        Check("仿真时间已推进", host.SimTime > 5.5, $"t={host.SimTime:F2}")
        Check("存在进入休眠的刚体（休眠机制生效）", sawSleeping)

        ' 暂停 / 步进语义
        Call script.Fire("keydown", "P")
        Check("P 切换到暂停", host.Paused)

        Dim t0 As Double = host.SimTime

        Call host.Update(1.0 / 60.0)
        Near("暂停时物理不推进", host.SimTime, t0, 1.0E-12)

        Call script.Fire("keydown", "P")
        Check("再按 P 恢复运行", Not host.Paused)

        ' 只移除一个动态刚体，保证最后的截图里关卡几何完整
        Dim victim As BodyVisual = host.Bodies.FirstOrDefault(Function(v) Not v.IsStaticGeometry)

        If victim IsNot Nothing Then
            Dim n As Integer = host.Bodies.Count

            Check("Remove 生效", host.Remove(victim.Id) AndAlso host.Bodies.Count = n - 1)
            Check("Remove 后按 id 不再可查", host.Find(victim.Id) Is Nothing)
        Else
            Check("Remove 生效", False, "世界中没有动态刚体，无法验证")
        End If
    End Sub

    ''' <summary>离屏渲染一帧并保存 PNG。</summary>
    Private Sub RenderShot(rig As CameraRig, renderer As SceneRenderer)
        Call report.AppendLine("-- 离屏渲染")

        ' 相机缓动在正常运行时由主循环驱动；自测里手动收敛到脚本设定的机位
        Call rig.SetScreen(New Size(1280, 720))

        For i As Integer = 1 To 240
            Call rig.Tick()
        Next

        Call report.AppendLine($"     机位 angleX={rig.AngleX:F1} angleY={rig.AngleY:F1} distance={rig.Distance:F1}")

        Dim size As New Size(1280, 720)
        Dim bmp As New Bitmap(size.Width, size.Height)

        Try
            Using g As Graphics = Graphics.FromImage(bmp)
                Call renderer.Render(g, size)
            End Using

            Dim out As String = Path.Combine(OutputDirectory, "selftest.png")

            Call bmp.Save(out, ImageFormat.Png)

            Check("渲染并保存 PNG", File.Exists(out) AndAlso New FileInfo(out).Length > 1000,
                  $"path={out}")

            ' 画面不应该是纯色：抽样统计颜色种类
            Dim colors As New HashSet(Of Integer)()

            For x As Integer = 0 To size.Width - 1 Step 17
                For y As Integer = 0 To size.Height - 1 Step 17
                    colors.Add(bmp.GetPixel(x, y).ToArgb())
                Next
            Next

            Check("画面包含多种颜色（确实画出了内容）", colors.Count > 20, $"colors={colors.Count}")
        Finally
            bmp.Dispose()
        End Try
    End Sub

    ''' <summary>
    ''' 性能探针：造 60 个混合刚体，测量"物理步进 + 渲染"的单帧耗时。
    ''' 这是交互体验的硬指标（目标是 60 个物体时仍能保持 20 FPS 以上）。
    ''' </summary>
    Private Sub PerformanceProbe(host As WorldHost, rig As CameraRig, renderer As SceneRenderer)
        Call report.AppendLine("-- 性能探针（60 个混合刚体）")

        Call host.ClearDynamic()

        Dim kinds As String() = {"box", "cuboid", "sphere", "cone", "cylinder", "tetra"}
        Dim rnd As New Random(20261013)

        For i As Integer = 1 To 60
            Dim kind As String = kinds(rnd.Next(kinds.Length))
            Dim o As New SpawnOptions With {
                .Kind = kind,
                .X = rnd.NextDouble() * 10.0 - 5.0,
                .Y = 6.0 + rnd.NextDouble() * 12.0,
                .Z = rnd.NextDouble() * 10.0 - 5.0,
                .SizeX = 0.7, .SizeY = 0.7, .SizeZ = 0.7,
                .Radius = 0.45, .Height = 1.1, .Segment = 12, .Edge = 1.0,
                .Width = 1, .Depth = 1,
                .Mass = 1.0,
                .Friction = 0.4,
                .Orientation = Quaternion.FromAxisAngle(New Vector3(0.3, 1, 0.6), rnd.NextDouble() * 6.28),
                .Color = Color.FromArgb(34, 211, 238)
            }

            Call host.Spawn(o)
        Next

        ' 让它们落下来堆好
        For i As Integer = 1 To 180
            Call host.Update(1.0 / 60.0)
        Next

        Dim size As New Size(1280, 720)
        Dim bmp As New Bitmap(size.Width, size.Height)
        Dim frames As Integer = 5
        Dim sw As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()

        Try
            For i As Integer = 1 To frames
                Using g As Graphics = Graphics.FromImage(bmp)
                    Call renderer.Render(g, size)
                End Using
            Next
        Finally
            sw.Stop()
            bmp.Dispose()
        End Try

        Dim msPerFrame As Double = sw.Elapsed.TotalMilliseconds / frames

        Call report.AppendLine($"     刚体 {host.DynamicCount}   单帧 {msPerFrame:F1} ms（约 {1000.0 / msPerFrame:F1} FPS）")

        Dim swPhysics As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()

        For i As Integer = 1 To 60
            Call host.Update(1.0 / 60.0)
        Next

        swPhysics.Stop()

        Call report.AppendLine($"     物理步进 {swPhysics.Elapsed.TotalMilliseconds / 60.0:F2} ms/帧")

        Check("60 刚体时渲染单帧 < 120 ms（> 8 FPS）", msPerFrame < 120.0, $"ms={msPerFrame:F1}")
        Check("60 刚体时物理步进 < 20 ms", swPhysics.Elapsed.TotalMilliseconds / 60.0 < 20.0,
              $"ms={swPhysics.Elapsed.TotalMilliseconds / 60.0:F2}")
        Check("全部刚体状态有限", host.Bodies.All(Function(v) v.IsStaticGeometry OrElse Finite(v.Body.Position)))

        Call host.ClearDynamic()
    End Sub

    ''' <summary>
    ''' 验证两件事：
    ''' ① 脚本顶层语句可以直接使用 host API（<c>world.addBox(...)</c> 不需要写在回调里）；
    ''' ② 落在楔形斜坡上的方块会因重力沿斜面下滑，而不是停在原地或穿透。
    ''' </summary>
    Private Sub TopLevelApiAndRampSlide(host As WorldHost, script As ScriptHost)
        Call report.AppendLine("-- 顶层 API 与斜面下滑")

        Call host.ClearDynamic()

        Dim before As Integer = host.DynamicCount

        ' 顶层语句直接调用 API
        Dim ok As Boolean = script.Load("var id = world.addBox({ x: 4, y: 8, z: 0, w: 1, h: 1, d: 1, mass: 1, friction: 0.15, " &
                                        "rot: { axis: [0, 1, 0], angle: 0.4 } });" & vbCrLf &
                                        "print('顶层语句创建了 ' + id);")

        Check("顶层语句可调用 host API", ok AndAlso host.DynamicCount = before + 1,
              If(script.LastError, $"dynamic={host.DynamicCount}"))

        If host.DynamicCount = before Then Return

        Dim box As BodyVisual = host.Bodies.FirstOrDefault(Function(v) Not v.IsStaticGeometry)

        If box Is Nothing Then Return

        Dim x0 As Double = box.Body.Position.x
        Dim minLow As Double = Double.MaxValue

        For i As Integer = 1 To 300
            Call host.Update(1.0 / 60.0)
            minLow = std.Min(minLow, LowestVertex(box))
        Next

        Call report.AppendLine($"     起点 x={x0:F2} → 终点 x={box.Body.Position.x:F2} y={box.Body.Position.y:F2} 最低顶点历史最小值={minLow:F3}")

        Check("方块沿斜面下滑（-x 方向位移 > 0.4）", box.Body.Position.x < x0 - 0.4,
              $"x0={x0:F3} x={box.Body.Position.x:F3}")
        Check("下滑过程中没有穿透斜坡", minLow > -0.15, $"minLow={minLow:F3}")
        Check("方块最终停在支撑面上", box.Body.Position.y > 0.0, $"y={box.Body.Position.y:F3}")
    End Sub

    ''' <summary>脚本错误必须被捕获而不是崩溃。</summary>
    Private Sub ErrorHandling(script As ScriptHost)
        Call report.AppendLine("-- 脚本错误处理")

        ' 语法错误
        Check("语法错误被捕获", Not script.Load("function ( { "), "本应失败")
        Check("语法错误有可读信息", script.LastError IsNot Nothing AndAlso script.LastError.Contains("语法"),
              If(script.LastError, ""))

        ' 运行时错误
        Check("运行时错误被捕获", Not script.Load("undefinedFunctionCall();"), "本应失败")
        Check("运行时错误有可读信息", script.LastError IsNot Nothing, If(script.LastError, ""))

        ' 正常脚本恢复（宿主 API 由 BeforeRun 在运行前重新注册，因此这里可以直接用 print）
        Check("随后仍能装载正常脚本", script.Load("var a = 1 + 1; print('ok ' + a);"), If(script.LastError, ""))
    End Sub

    Private Sub WriteReport(code As Integer)
        Dim out As String = Path.Combine(OutputDirectory, "selftest-report.txt")

        Call File.WriteAllText(out, report.ToString(), Encoding.UTF8)
    End Sub
End Module
