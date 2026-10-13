Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Microsoft.VisualBasic.Engine.Physics
Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

''' <summary>
''' 把宿主的 <see cref="WorldHost"/> / <see cref="CameraRig"/> / <see cref="SceneRenderer"/>
''' 暴露成 JS 全局对象树。LiteJs 没有反射式对象绑定，唯一可调用的形态是
''' <see cref="Func(Of Object(), Object)"/>，因此所有 API 都是"字典 + lambda"。
''' </summary>
Public Class JsApi

    Private ReadOnly host As WorldHost
    Private ReadOnly rig As CameraRig
    Private ReadOnly renderer As SceneRenderer
    Private ReadOnly script As ScriptHost

    ''' <summary>当前鼠标在视口中的像素位置（负值表示不在视口内）。</summary>
    Public Property MouseX As Double = -1.0
    Public Property MouseY As Double = -1.0
    ''' <summary>当前按下的按钮（0=无，1=左，2=右，3=中）。</summary>
    Public Property MouseButton As Double = 0.0
    ''' <summary>手动步进时剩余的帧数。</summary>
    Public Property PendingSteps As Double = 0.0

    Public Sub New(host As WorldHost, rig As CameraRig, renderer As SceneRenderer, script As ScriptHost)
        Me.host = host
        Me.rig = rig
        Me.renderer = renderer
        Me.script = script
    End Sub

    ' /********************************************************************************/
    '  参数解析
    ' /********************************************************************************/

    ''' <summary>从 JS 对象里取数值，支持多个候选键名。</summary>
    Private Shared Function Num(o As Object, keys As String(), def As Double) As Double
        Dim d = TryCast(o, Dictionary(Of String, Object))

        If d Is Nothing Then Return def

        For Each k As String In keys
            Dim v As Object = Nothing

            If d.TryGetValue(k, v) AndAlso v IsNot Nothing Then
                Return JsRuntime.JsNum(v)
            End If
        Next

        Return def
    End Function

    Private Shared Function Str(o As Object, keys As String(), def As String) As String
        Dim d = TryCast(o, Dictionary(Of String, Object))

        If d Is Nothing Then Return def

        For Each k As String In keys
            Dim v As Object = Nothing

            If d.TryGetValue(k, v) AndAlso v IsNot Nothing Then
                Return JsRuntime.JsStr(v)
            End If
        Next

        Return def
    End Function

    Private Shared Function Bool(o As Object, key As String, def As Boolean) As Boolean
        Dim d = TryCast(o, Dictionary(Of String, Object))
        Dim v As Object = Nothing

        If d Is Nothing Then Return def
        If Not d.TryGetValue(key, v) OrElse v Is Nothing Then Return def

        Return JsRuntime.JsTruthy(v)
    End Function

    Private Shared Function Vec3(o As Object, keys As String(), def As Vector3) As Vector3
        Dim d = TryCast(o, Dictionary(Of String, Object))
        Dim v As Object = Nothing

        If d Is Nothing Then Return def
        If Not d.TryGetValue(keys(0), v) OrElse v Is Nothing Then Return def

        Dim list = TryCast(v, List(Of Object))

        If list Is Nothing OrElse list.Count < 3 Then Return def

        Return New Vector3(JsRuntime.JsNum(list(0)), JsRuntime.JsNum(list(1)), JsRuntime.JsNum(list(2)))
    End Function

    Private Shared Function ParseColor(o As Object, def As Color) As Color
        If o Is Nothing Then Return def

        Dim list = TryCast(o, List(Of Object))

        If list IsNot Nothing AndAlso list.Count >= 3 Then
            Return Color.FromArgb(CInt(JsRuntime.JsNum(list(0))), CInt(JsRuntime.JsNum(list(1))), CInt(JsRuntime.JsNum(list(2))))
        End If

        Dim s As String = JsRuntime.JsStr(o)

        If String.IsNullOrEmpty(s) Then Return def

        Try
            Return ColorTranslator.FromHtml(s)
        Catch
            Return def
        End Try
    End Function

    ''' <summary>解析朝向：支持 rot:{axis,angle} / rot:{euler} / rot:{q} / rotx,roty,rotz。</summary>
    Private Shared Function ParseOrientation(o As Object) As Quaternion
        Dim d = TryCast(o, Dictionary(Of String, Object))

        If d Is Nothing Then Return Quaternion.Identity

        Dim rx As Double = Num(o, {"rotx"}, 0.0)
        Dim ry As Double = Num(o, {"roty"}, 0.0)
        Dim rz As Double = Num(o, {"rotz"}, 0.0)

        Dim q As Quaternion = Quaternion.Identity

        If std.Abs(rx) > 0.0 OrElse std.Abs(ry) > 0.0 OrElse std.Abs(rz) > 0.0 Then
            q = Quaternion.Compose(
                    Quaternion.FromAxisAngle(New Vector3(0, 0, 1), rz * std.PI / 180.0),
                    Quaternion.Compose(
                        Quaternion.FromAxisAngle(New Vector3(0, 1, 0), ry * std.PI / 180.0),
                        Quaternion.FromAxisAngle(New Vector3(1, 0, 0), rx * std.PI / 180.0)))
        End If

        Dim rot As Object = Nothing

        If Not d.TryGetValue("rot", rot) OrElse rot Is Nothing Then Return q

        Dim rd = TryCast(rot, Dictionary(Of String, Object))

        If rd Is Nothing Then Return q

        Dim axis As Object = Nothing
        Dim angle As Object = Nothing

        If rd.TryGetValue("axis", axis) AndAlso axis IsNot Nothing AndAlso rd.TryGetValue("angle", angle) Then
            Dim al = TryCast(axis, List(Of Object))

            If al IsNot Nothing AndAlso al.Count >= 3 Then
                Dim a As New Vector3(JsRuntime.JsNum(al(0)), JsRuntime.JsNum(al(1)), JsRuntime.JsNum(al(2)))

                If Vector3Math.Length(a) < 1.0E-9 Then a = New Vector3(0, 1, 0)

                Return Quaternion.FromAxisAngle(a, JsRuntime.JsNum(angle))
            End If
        End If

        Dim euler As Object = Nothing

        If rd.TryGetValue("euler", euler) AndAlso euler IsNot Nothing Then
            Dim el = TryCast(euler, List(Of Object))

            If el IsNot Nothing AndAlso el.Count >= 3 Then
                Return Quaternion.Compose(
                        Quaternion.FromAxisAngle(New Vector3(0, 0, 1), JsRuntime.JsNum(el(2)) * std.PI / 180.0),
                        Quaternion.Compose(
                            Quaternion.FromAxisAngle(New Vector3(0, 1, 0), JsRuntime.JsNum(el(1)) * std.PI / 180.0),
                            Quaternion.FromAxisAngle(New Vector3(1, 0, 0), JsRuntime.JsNum(el(0)) * std.PI / 180.0)))
            End If
        End If

        Dim qv As Object = Nothing

        If rd.TryGetValue("q", qv) AndAlso qv IsNot Nothing Then
            Dim ql = TryCast(qv, List(Of Object))

            If ql IsNot Nothing AndAlso ql.Count >= 4 Then
                Return New Quaternion(JsRuntime.JsNum(ql(0)), JsRuntime.JsNum(ql(1)),
                                      JsRuntime.JsNum(ql(2)), JsRuntime.JsNum(ql(3)))
            End If
        End If

        Return q
    End Function

    ''' <summary>把 JS 配置对象解析成 <see cref="SpawnOptions"/>。</summary>
    Private Shared Function ParseOptions(kind As String, o As Object) As SpawnOptions
        ' "size" 是统一尺寸的简写：某个轴向单独给了键名就以该键名为准
        Dim hasSize As Boolean = HasKey(o, {"size", "s"})
        Dim size As Double = Num(o, {"size", "s"}, 1.0)

        Dim opt As New SpawnOptions With {
            .Kind = kind,
            .Label = Str(o, {"label", "name", "id"}, ""),
            .X = Num(o, {"x"}, 0.0),
            .Y = Num(o, {"y"}, 0.0),
            .Z = Num(o, {"z"}, 0.0),
            .SizeX = If(HasKey(o, {"w", "width", "sx", "dx"}), Num(o, {"w", "width", "sx", "dx"}, 1.0), If(hasSize, size, 1.0)),
            .SizeY = If(HasKey(o, {"h", "height", "sy", "dy"}), Num(o, {"h", "height", "sy", "dy"}, 1.0), If(hasSize, size, 1.0)),
            .SizeZ = If(HasKey(o, {"d", "depth", "sz", "dz"}), Num(o, {"d", "depth", "sz", "dz"}, 1.0), If(hasSize, size, 1.0)),
            .Radius = Num(o, {"r", "radius"}, 0.5),
            .Height = Num(o, {"h", "height"}, 1.5),
            .Segment = CInt(Num(o, {"seg", "segments", "sides"}, 14)),
            .Edge = Num(o, {"edge", "e"}, 1.0),
            .Width = Num(o, {"w", "width"}, 8.0),
            .Depth = Num(o, {"d", "depth"}, 8.0),
            .Mass = Num(o, {"mass", "m"}, 1.0),
            .Density = Num(o, {"density"}, 0.0),
            .Friction = Num(o, {"friction", "mu"}, 0.4),
            .Restitution = Num(o, {"restitution", "bounce"}, 0.0),
            .Orientation = ParseOrientation(o),
            .Color = ParseColor(PropOf(o, "color"), Color.CornflowerBlue)
        }

        Dim v = TryCast(PropOf(o, "v"), List(Of Object))

        If v IsNot Nothing AndAlso v.Count >= 3 Then
            opt.Velocity = New Vector3(JsRuntime.JsNum(v(0)), JsRuntime.JsNum(v(1)), JsRuntime.JsNum(v(2)))
        Else
            Dim vx As Double = Num(o, {"vx"}, 0.0)
            Dim vy As Double = Num(o, {"vy"}, 0.0)
            Dim vz As Double = Num(o, {"vz"}, 0.0)

            If std.Abs(vx) + std.Abs(vy) + std.Abs(vz) > 0.0 Then
                opt.Velocity = New Vector3(vx, vy, vz)
            End If
        End If

        ' mass <= 0 视为静态
        If Bool(o, "static", opt.Mass <= 0.0) Then
            opt.Mass = 0.0
        End If

        Return opt
    End Function

    Private Shared Function PropOf(o As Object, key As String) As Object
        Dim d = TryCast(o, Dictionary(Of String, Object))
        Dim v As Object = Nothing

        If d Is Nothing Then Return Nothing
        If Not d.TryGetValue(key, v) Then Return Nothing

        Return v
    End Function

    ''' <summary>JS 对象里是否存在任意一个候选键。</summary>
    Private Shared Function HasKey(o As Object, keys As String()) As Boolean
        Dim d = TryCast(o, Dictionary(Of String, Object))

        If d Is Nothing Then Return False

        For Each k As String In keys
            If d.ContainsKey(k) Then Return True
        Next

        Return False
    End Function

    ''' <summary>把刚体状态打包成 JS 对象。</summary>
    Private Shared Function BodyState(v As BodyVisual) As Object
        Dim b As RigidBody3D = v.Body

        Return New Dictionary(Of String, Object) From {
            {"id", v.Id},
            {"label", v.Label},
            {"x", CDbl(b.Position.x)},
            {"y", CDbl(b.Position.y)},
            {"z", CDbl(b.Position.z)},
            {"qw", CDbl(b.Orientation.w)},
            {"qx", CDbl(b.Orientation.x)},
            {"qy", CDbl(b.Orientation.y)},
            {"qz", CDbl(b.Orientation.z)},
            {"vx", CDbl(b.Velocity.x)},
            {"vy", CDbl(b.Velocity.y)},
            {"vz", CDbl(b.Velocity.z)},
            {"mass", CDbl(b.Mass)},
            {"sleeping", b.IsSleeping},
            {"static", v.IsStaticGeometry},
            {"color", v.Color.Name}
        }
    End Function

    ' /********************************************************************************/
    '  全局对象树
    ' /********************************************************************************/

    Private Shared Function F(fn As Func(Of Object(), Object)) As Object
        Return fn
    End Function

    ''' <summary>
    ''' <c>print(...)</c> / <c>log(...)</c> 的实现。
    ''' 注意不要在这里用 <c>args.Select(AddressOf JsRuntime.JsStr)</c>：
    ''' <c>JsStr</c> 有重载，方法组会在后期绑定阶段被当成委托传给 <c>String.Join</c> 而抛异常。
    ''' </summary>
    Private Function PrintImpl(args As Object()) As Object
        Dim parts As New List(Of String)()

        If args IsNot Nothing Then
            For Each a As Object In args
                parts.Add(ToText(a))
            Next
        End If

        Call script.Log(String.Join(" ", parts))

        Return JsRuntime.Undef
    End Function

    Private Shared Function ToText(x As Object) As String
        Return JsRuntime.JsStr(x)
    End Function

    ''' <summary>构造并注册全部全局 API。</summary>
    Public Sub Register()
        script.DefineGlobal("world", BuildWorld())
        script.DefineGlobal("camera", BuildCamera())
        script.DefineGlobal("input", BuildInput())
        script.DefineGlobal("sim", BuildSim())
        script.DefineGlobal("render", BuildRender())

        script.DefineGlobal("print", F(AddressOf PrintImpl))
        script.DefineGlobal("log", F(AddressOf PrintImpl))
        script.DefineGlobal("version", F(Function(args As Object()) As Object
                                             Return CDbl(1)
                                         End Function))
    End Sub

    Private Function BuildWorld() As Object
        Return New Dictionary(Of String, Object) From {
            {"gravity", F(Function(a As Object()) As Object
                              host.SetGravity(JsRuntime.JsNum(Arg(a, 0)), JsRuntime.JsNum(Arg(a, 1)), JsRuntime.JsNum(Arg(a, 2)))
                              Return JsRuntime.Undef
                          End Function)},
            {"ground", F(Function(a As Object()) As Object
                             host.AddGround(JsRuntime.JsNum(Arg(a, 0)))
                             renderer.GroundY = JsRuntime.JsNum(Arg(a, 0))
                             Return JsRuntime.Undef
                         End Function)},
            {"add", F(Function(a As Object()) As Object
                          Dim kind As String = JsRuntime.JsStr(Arg(a, 0))
                          Dim cfg As Object = If(a.Length > 1, a(1), Nothing)

                          Return Id(host.Spawn(ParseOptions(kind, cfg)))
                      End Function)},
            {"addBox", F(Function(a As Object()) As Object
                             Return Id(host.Spawn(ParseOptions("box", Arg(a, 0))))
                         End Function)},
            {"addCube", F(Function(a As Object()) As Object
                              Return Id(host.Spawn(ParseOptions("cube", Arg(a, 0))))
                          End Function)},
            {"addCuboid", F(Function(a As Object()) As Object
                                Return Id(host.Spawn(ParseOptions("cuboid", Arg(a, 0))))
                            End Function)},
            {"addSphere", F(Function(a As Object()) As Object
                                Return Id(host.Spawn(ParseOptions("sphere", Arg(a, 0))))
                            End Function)},
            {"addBall", F(Function(a As Object()) As Object
                              Return Id(host.Spawn(ParseOptions("ball", Arg(a, 0))))
                          End Function)},
            {"addCone", F(Function(a As Object()) As Object
                              Return Id(host.Spawn(ParseOptions("cone", Arg(a, 0))))
                          End Function)},
            {"addCylinder", F(Function(a As Object()) As Object
                                  Return Id(host.Spawn(ParseOptions("cylinder", Arg(a, 0))))
                              End Function)},
            {"addTetra", F(Function(a As Object()) As Object
                               Return Id(host.Spawn(ParseOptions("tetra", Arg(a, 0))))
                           End Function)},
            {"addRamp", F(Function(a As Object()) As Object
                              Return Id(host.Spawn(ParseOptions("ramp", Arg(a, 0))))
                          End Function)},
            {"addStaticBox", F(Function(a As Object()) As Object
                                   Dim o As SpawnOptions = ParseOptions("box", Arg(a, 0))
                                   o.Mass = 0.0
                                   Return Id(host.Spawn(o))
                               End Function)},
            {"addStaticRamp", F(Function(a As Object()) As Object
                                    Dim o As SpawnOptions = ParseOptions("ramp", Arg(a, 0))
                                    o.Mass = 0.0
                                    Return Id(host.Spawn(o))
                                End Function)},
            {"remove", F(Function(a As Object()) As Object
                             Return If(host.Remove(JsRuntime.JsStr(Arg(a, 0))), 1.0, 0.0)
                         End Function)},
            {"clear", F(Function(a As Object()) As Object
                            Call host.Clear()
                            Return JsRuntime.Undef
                        End Function)},
            {"clearDynamic", F(Function(a As Object()) As Object
                                   Call host.ClearDynamic()
                                   Return JsRuntime.Undef
                               End Function)},
            {"count", F(Function(a As Object()) As Object
                            Return CDbl(host.Bodies.Count)
                        End Function)},
            {"dynamicCount", F(Function(a As Object()) As Object
                                   Return CDbl(host.DynamicCount)
                               End Function)},
            {"bodyLimit", F(Function(a As Object()) As Object
                                host.BodyLimit = CInt(JsRuntime.JsNum(Arg(a, 0)))
                                Return JsRuntime.Undef
                            End Function)},
            {"get", F(Function(a As Object()) As Object
                          Dim v As BodyVisual = host.Find(JsRuntime.JsStr(Arg(a, 0)))

                          Return If(v Is Nothing, JsRuntime.Undef, BodyState(v))
                      End Function)},
            {"list", F(Function(a As Object()) As Object
                           Dim ids As New List(Of Object)()

                           For Each v In host.Bodies
                               ids.Add(v.Id)
                           Next

                           Return ids
                       End Function)},
            {"applyImpulse", F(Function(a As Object()) As Object
                                   Dim v As BodyVisual = host.Find(JsRuntime.JsStr(Arg(a, 0)))

                                   If v Is Nothing OrElse v.Body.IsStatic Then Return JsRuntime.Undef

                                   Dim p As New Vector3(JsRuntime.JsNum(Arg(a, 1)), JsRuntime.JsNum(Arg(a, 2)), JsRuntime.JsNum(Arg(a, 3)))
                                   Call v.Body.ApplyImpulse(p, Vector3.Zero)
                                   Call v.Body.Wake()
                                   Return JsRuntime.Undef
                               End Function)},
            {"applyForce", F(Function(a As Object()) As Object
                                 Dim v As BodyVisual = host.Find(JsRuntime.JsStr(Arg(a, 0)))

                                 If v Is Nothing OrElse v.Body.IsStatic Then Return JsRuntime.Undef

                                 Dim f As New Vector3(JsRuntime.JsNum(Arg(a, 1)), JsRuntime.JsNum(Arg(a, 2)), JsRuntime.JsNum(Arg(a, 3)))
                                 Call v.Body.ApplyForce(f)
                                 Call v.Body.Wake()
                                 Return JsRuntime.Undef
                             End Function)},
            {"setVelocity", F(Function(a As Object()) As Object
                                  Dim v As BodyVisual = host.Find(JsRuntime.JsStr(Arg(a, 0)))

                                  If v Is Nothing OrElse v.Body.IsStatic Then Return JsRuntime.Undef

                                  v.Body.Velocity = New Vector3(JsRuntime.JsNum(Arg(a, 1)), JsRuntime.JsNum(Arg(a, 2)), JsRuntime.JsNum(Arg(a, 3)))
                                  Call v.Body.Wake()
                                  Return JsRuntime.Undef
                              End Function)}
        }
    End Function

    Private Function BuildCamera() As Object
        Return New Dictionary(Of String, Object) From {
            {"orbit", F(Function(a As Object()) As Object
                            rig.Orbit(JsRuntime.JsNum(Arg(a, 0)), JsRuntime.JsNum(Arg(a, 1)))
                            Return JsRuntime.Undef
                        End Function)},
            {"zoom", F(Function(a As Object()) As Object
                           rig.Zoom(JsRuntime.JsNum(Arg(a, 0)))
                           Return JsRuntime.Undef
                       End Function)},
            {"angles", F(Function(a As Object()) As Object
                             rig.SetAngles(JsRuntime.JsNum(Arg(a, 0)), JsRuntime.JsNum(Arg(a, 1)))
                             Return JsRuntime.Undef
                         End Function)},
            {"distance", F(Function(a As Object()) As Object
                               rig.SetDistance(JsRuntime.JsNum(Arg(a, 0)))
                               Return JsRuntime.Undef
                           End Function)},
            {"fov", F(Function(a As Object()) As Object
                          rig.SetFieldOfView(JsRuntime.JsNum(Arg(a, 0)))
                          Return JsRuntime.Undef
                      End Function)},
            {"focus", F(Function(a As Object()) As Object
                            rig.SetFocus(JsRuntime.JsNum(Arg(a, 0)), JsRuntime.JsNum(Arg(a, 1)), JsRuntime.JsNum(Arg(a, 2)))
                            Return JsRuntime.Undef
                        End Function)},
            {"reset", F(Function(a As Object()) As Object
                            Call rig.Reset()
                            Return JsRuntime.Undef
                        End Function)},
            {"home", F(Function(a As Object()) As Object
                           Call rig.CaptureHome()
                           Return JsRuntime.Undef
                       End Function)},
            {"screenToRay", F(Function(a As Object()) As Object
                                  Dim ox, oy, oz, dx, dy, dz As Double

                                  If Not rig.ScreenToRay(JsRuntime.JsNum(Arg(a, 0)), JsRuntime.JsNum(Arg(a, 1)), ox, oy, oz, dx, dy, dz) Then
                                      Return JsRuntime.Undef
                                  End If

                                  Return New Dictionary(Of String, Object) From {
                                      {"x", CDbl(ox)}, {"y", CDbl(oy)}, {"z", CDbl(oz)},
                                      {"dx", CDbl(dx)}, {"dy", CDbl(dy)}, {"dz", CDbl(dz)}
                                  }
                              End Function)},
            {"screenToPlane", F(Function(a As Object()) As Object
                                    Dim px, py, pz As Double

                                    If Not rig.ScreenToPlane(JsRuntime.JsNum(Arg(a, 0)), JsRuntime.JsNum(Arg(a, 1)),
                                                             JsRuntime.JsNum(Arg(a, 2)), JsRuntime.JsNum(Arg(a, 3)), JsRuntime.JsNum(Arg(a, 4)),
                                                             JsRuntime.JsNum(Arg(a, 5)), px, py, pz) Then
                                        Return JsRuntime.Undef
                                    End If

                                    Return New Dictionary(Of String, Object) From {
                                        {"x", CDbl(px)}, {"y", CDbl(py)}, {"z", CDbl(pz)}
                                    }
                                End Function)},
            {"info", F(Function(a As Object()) As Object
                           Return New Dictionary(Of String, Object) From {
                               {"angleX", CDbl(rig.AngleX)},
                               {"angleY", CDbl(rig.AngleY)},
                               {"distance", CDbl(rig.Distance)},
                               {"focusX", CDbl(rig.FocusX)},
                               {"focusY", CDbl(rig.FocusY)},
                               {"focusZ", CDbl(rig.FocusZ)}
                           }
                       End Function)}
        }
    End Function

    Private Function BuildInput() As Object
        Return New Dictionary(Of String, Object) From {
            {"mouse", F(Function(a As Object()) As Object
                            Return New Dictionary(Of String, Object) From {
                                {"x", CDbl(MouseX)}, {"y", CDbl(MouseY)}, {"button", CDbl(MouseButton)}
                            }
                        End Function)},
            {"isDown", F(Function(a As Object()) As Object
                             Dim which As Double = JsRuntime.JsNum(Arg(a, 0))

                             Return If(std.Abs(MouseButton - which) < 0.5, True, False)
                         End Function)}
        }
    End Function

    Private Function BuildSim() As Object
        Return New Dictionary(Of String, Object) From {
            {"pause", F(Function(a As Object()) As Object
                            host.Paused = True
                            Return JsRuntime.Undef
                        End Function)},
            {"resume", F(Function(a As Object()) As Object
                             host.Paused = False
                             Return JsRuntime.Undef
                         End Function)},
            {"isPaused", F(Function(a As Object()) As Object
                               Return If(host.Paused, True, False)
                           End Function)},
            {"step", F(Function(a As Object()) As Object
                           PendingSteps += JsRuntime.JsNum(Arg(a, 0))
                           Return JsRuntime.Undef
                       End Function)},
            {"time", F(Function(a As Object()) As Object
                           Return CDbl(host.SimTime)
                       End Function)},
            {"contacts", F(Function(a As Object()) As Object
                               Return CDbl(host.ContactCount)
                           End Function)},
            {"warmStarting", F(Function(a As Object()) As Object
                                   host.World.WarmStarting = JsRuntime.JsTruthy(Arg(a, 0))
                                   Return JsRuntime.Undef
                               End Function)},
            {"allowSleep", F(Function(a As Object()) As Object
                                 host.World.AllowSleep = JsRuntime.JsTruthy(Arg(a, 0))
                                 Return JsRuntime.Undef
                             End Function)},
            {"iterations", F(Function(a As Object()) As Object
                                 host.World.Iterations = CInt(JsRuntime.JsNum(Arg(a, 0)))
                                 Return JsRuntime.Undef
                             End Function)},
            {"substeps", F(Function(a As Object()) As Object
                               host.World.Substeps = CInt(JsRuntime.JsNum(Arg(a, 0)))
                               Return JsRuntime.Undef
                           End Function)},
            {"reload", F(Function(a As Object()) As Object
                             Call script.Log("sim.reload() 由脚本调用，宿主未实现自动重载")
                             Return JsRuntime.Undef
                         End Function)}
        }
    End Function

    Private Function BuildRender() As Object
        Return New Dictionary(Of String, Object) From {
            {"wireframe", F(Function(a As Object()) As Object
                                renderer.Wireframe = JsRuntime.JsTruthy(Arg(a, 0))
                                Return JsRuntime.Undef
                            End Function)},
            {"grid", F(Function(a As Object()) As Object
                           renderer.ShowGrid = JsRuntime.JsTruthy(Arg(a, 0))
                           Return JsRuntime.Undef
                       End Function)},
            {"axes", F(Function(a As Object()) As Object
                           renderer.ShowAxes = JsRuntime.JsTruthy(Arg(a, 0))
                           Return JsRuntime.Undef
                       End Function)},
            {"background", F(Function(a As Object()) As Object
                                 Try
                                     renderer.BackgroundColor = ColorTranslator.FromHtml(JsRuntime.JsStr(Arg(a, 0)))
                                 Catch
                                 End Try
                                 Return JsRuntime.Undef
                             End Function)},
            {"hud", F(Function(a As Object()) As Object
                          renderer.HudText = JsRuntime.JsStr(Arg(a, 0))
                          Return JsRuntime.Undef
                      End Function)}
        }
    End Function

    ' /********************************************************************************/
    '  小工具
    ' /********************************************************************************/

    Private Shared Function Arg(a As Object(), i As Integer) As Object
        If a Is Nothing OrElse i >= a.Length Then Return JsRuntime.Undef

        Return a(i)
    End Function

    ''' <summary>把刚体 id 交回脚本（字符串）。</summary>
    Private Shared Function Id(bodyId As String) As Object
        Return bodyId
    End Function
End Class
