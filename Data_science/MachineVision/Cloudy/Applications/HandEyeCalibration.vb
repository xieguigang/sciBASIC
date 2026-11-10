Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Applications

    ''' <summary>
    ''' 手眼标定结果（<c>AX = XB</c> 的解）。
    ''' </summary>
    Public Class HandEyeResult

        ''' <summary>
        ''' 手眼变换 <c>X</c>（传感器坐标系 → 机器人末端坐标系）。
        ''' </summary>
        Public Property Transform As RigidTransform

        ''' <summary>旋转残差（弧度，所有位姿对的 RMS）。</summary>
        Public Property RotationError As Double

        ''' <summary>平移残差（与点云同单位，所有位姿对的 RMS）。</summary>
        Public Property TranslationError As Double

        ''' <summary>迭代次数。</summary>
        Public Property Iterations As Integer

        ''' <summary>是否收敛。</summary>
        Public Property Converged As Boolean

        ''' <summary>参与标定的位姿对数量。</summary>
        Public Property PairCount As Integer

        Public Overrides Function ToString() As String
            Return $"hand-eye: rot-err={RotationError:G4} rad, trans-err={TranslationError:G4}, pairs={PairCount}, converged={Converged}"
        End Function
    End Class

    ''' <summary>
    ''' 手眼标定（AX = XB）：把传感器坐标系下的位姿转换到机器人末端坐标系。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>问题</b>：视觉系统给出的目标位姿在“相机坐标系”，机器人需要的是“末端/基座坐标系”。
    ''' 两者之间的固定变换 <c>X</c> 未知，需要通过一组“机器人移动 + 相机观测静止标定板”
    ''' 的数据求解：每次移动机器人，记录末端位姿 <c>T_ee</c> 与标定板在相机中的位姿 <c>T_cam</c>，
    ''' 相邻两次之间满足
    ''' </para>
    ''' <code>
    ''' A·X = X·B     （A = T_ee(i)⁻¹·T_ee(j)，B = T_cam(i)⁻¹·T_cam(j)）
    ''' </code>
    ''' <para>
    ''' <b>求解方法</b>：经典做法（Tsai-Lenz / Park-Martin）先闭式解旋转再解平移。
    ''' 本实现直接用<b>Gauss-Newton 迭代</b>同时优化 6 个参数：
    ''' 残差取 <c>Log((A·X)⁻¹·(X·B))</c>（旋转向量 + 平移），
    ''' 雅可比用数值差分。这样做的优点是：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>代码路径与位姿图优化共用同一套 SE(3) 工具，容易维护；</description></item>
    ''' <item><description>可以自然加入鲁棒权重（剔除标定失败的位姿对）；</description></item>
    ''' <item><description>初值为单位矩阵时也能收敛（<c>AX=XB</c> 的解空间在旋转足够激励时是单点）。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>精度要求</b>：位姿对必须包含<b>至少两个非平行旋转轴</b>，
    ''' 否则旋转不可观（解不唯一）。工业上通常做 15~30 组“绕不同轴小幅转动”的动作，
    ''' 并保证每次转动的旋转角 &gt; 5°。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class HandEyeCalibration

        Private Sub New()
        End Sub

        ''' <summary>标定参数。</summary>
        Public Class Options

            ''' <summary>最大迭代次数（默认 100）。</summary>
            Public Property MaxIterations As Integer = 100

            ''' <summary>收敛阈值（参数增量）。</summary>
            Public Property Epsilon As Double = 1.0E-10

            ''' <summary>数值雅可比步长。</summary>
            Public Property FiniteDifferenceStep As Double = 1.0E-6

            ''' <summary>初始猜测（<c>Nothing</c> 表示单位变换）。</summary>
            Public Property Initial As RigidTransform = Nothing
        End Class

        ''' <summary>
        ''' 求解手眼变换。
        ''' </summary>
        ''' <param name="robotPoses">机器人末端位姿序列（末端 → 基座）</param>
        ''' <param name="sensorPoses">对应的传感器观测位姿序列（传感器 → 标定板）</param>
        ''' <param name="options">参数</param>
        Public Shared Function Calibrate(robotPoses As RigidTransform(), sensorPoses As RigidTransform(),
                                         Optional options As Options = Nothing) As HandEyeResult
            If robotPoses Is Nothing Then Throw New ArgumentNullException(NameOf(robotPoses))
            If sensorPoses Is Nothing Then Throw New ArgumentNullException(NameOf(sensorPoses))
            If robotPoses.Length <> sensorPoses.Length Then
                Throw New ArgumentException($"the pose counts not agree: {robotPoses.Length} vs {sensorPoses.Length}!", NameOf(sensorPoses))
            End If
            If robotPoses.Length < 3 Then
                Throw New ArgumentException("at least 3 pose pairs are required for hand-eye calibration!", NameOf(robotPoses))
            End If

            Dim opt As Options = If(options, New Options())

            ' 构造运动对
            Dim motions As New List(Of (a As RigidTransform, b As RigidTransform))()

            For i As Integer = 0 To robotPoses.Length - 2
                For j As Integer = i + 1 To robotPoses.Length - 1
                    Dim a As RigidTransform = robotPoses(i).Inverse().Compose(robotPoses(j))
                    Dim b As RigidTransform = sensorPoses(i).Inverse().Compose(sensorPoses(j))

                    Call motions.Add((a, b))
                Next
            Next

            ' 闭式初值（Park–Martin）：先用线性最小二乘把 X 拉到真值附近，
            ' 再做 Gauss-Newton 精化。直接从单位矩阵迭代在 AX=XB 上很容易发散。
            Dim x As RigidTransform = If(opt.Initial, ClosedForm(motions))

            ' 统一使用 SE(3) 上的 Gauss-Newton（与位姿图优化同一套 log/exp 工具）
            Dim result As (transform As RigidTransform, iterations As Integer, converged As Boolean) =
                Optimize(motions, x, opt)

            x = result.transform

            Dim rotationSquared As Double = 0.0
            Dim translationSquared As Double = 0.0

            For Each motion In motions
                Dim r As Double() = Residual(motion.a, x, motion.b)
                Dim omega As New Vector3d(r(0), r(1), r(2))

                rotationSquared += omega.LengthSquared
                translationSquared += r(3) * r(3) + r(4) * r(4) + r(5) * r(5)
            Next

            Dim count As Integer = std.Max(1, motions.Count)

            Return New HandEyeResult With {
                .Transform = x,
                .RotationError = std.Sqrt(rotationSquared / count),
                .TranslationError = std.Sqrt(translationSquared / count),
                .Iterations = result.iterations,
                .Converged = result.converged,
                .PairCount = motions.Count
            }
        End Function

        ''' <summary>
        ''' Park–Martin 闭式解：旋转由“旋转向量相关矩阵”的极分解给出，平移解线性最小二乘。
        ''' </summary>
        ''' <remarks>
        ''' 记每组运动为 <c>(R_A, t_A)</c>、<c>(R_B, t_B)</c>。
        ''' 旋转部分：把两者的旋转向量记为 <c>a, b</c>，构造 <c>M = Σ b·aᵀ</c>，
        ''' 则 <c>R_X = (MᵀM)^(-1/2)·Mᵀ</c>（把 M 投影到最近的旋转矩阵，即极分解）。
        ''' 平移部分：<c>(R_A − I)·t_X = R_X·t_B − t_A</c> 对 <c>t_X</c> 是线性的，
        ''' 堆叠所有位姿对后解 3x3 正规方程即可。
        ''' </remarks>
        Private Shared Function ClosedForm(motions As List(Of (a As RigidTransform, b As RigidTransform))) As RigidTransform
            Dim m As Double()() = Zeros3()

            ' M = Σ b·aᵀ
            For Each motion In motions
                Dim a As Vector3d = RotationVector(motion.a)
                Dim b As Vector3d = RotationVector(motion.b)
                Dim av As Double() = {a.X, a.Y, a.Z}
                Dim bv As Double() = {b.X, b.Y, b.Z}

                For i As Integer = 0 To 2
                    For j As Integer = 0 To 2
                        ' M = Σ α·βᵀ（α 来自 A、β 来自 B），随后 R_X = (MᵀM)^(-1/2)·Mᵀ
                        m(i)(j) += av(i) * bv(j)
                    Next
                Next
            Next

            ' MtM 与其逆平方根（对称阵闭式特征分解）
            Dim mtm As Double()() = Zeros3()

            For i As Integer = 0 To 2
                For j As Integer = 0 To 2
                    Dim sum As Double = 0.0

                    For k As Integer = 0 To 2
                        sum += m(k)(i) * m(k)(j)
                    Next

                    mtm(i)(j) = sum
                Next
            Next

            Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(
                mtm(0)(0), mtm(0)(1), mtm(0)(2), mtm(1)(1), mtm(1)(2), mtm(2)(2))

            Dim vectors As Vector3d() = {eigen.V1, eigen.V2, eigen.V3}
            Dim values As Double() = {eigen.L1, eigen.L2, eigen.L3}
            Dim invSqrt As Double()() = Zeros3()

            For k As Integer = 0 To 2
                If values(k) <= 1.0E-12 Then Continue For

                Dim weight As Double = 1.0 / std.Sqrt(values(k))
                Dim v As Vector3d = vectors(k)
                Dim vv As Double() = {v.X, v.Y, v.Z}

                For i As Integer = 0 To 2
                    For j As Integer = 0 To 2
                        invSqrt(i)(j) += weight * vv(i) * vv(j)
                    Next
                Next
            Next

            ' R_X = invSqrt · Mᵀ
            Dim rotation As Double()() = Zeros3()

            For i As Integer = 0 To 2
                For j As Integer = 0 To 2
                    Dim sum As Double = 0.0

                    For k As Integer = 0 To 2
                        sum += invSqrt(i)(k) * m(j)(k)
                    Next

                    rotation(i)(j) = sum
                Next
            Next

            rotation = Linalg.Orthonormalize(rotation)

            ' 平移：(R_A − I)·t_X = R_X·t_B − t_A
            Dim ata As Double()() = Zeros3()
            Dim atb As Double() = New Double(2) {}
            Dim rx As New RigidTransform(rotation, Vector3d.Zero)
            Dim identity As Double()() = {
                New Double() {1, 0, 0},
                New Double() {0, 1, 0},
                New Double() {0, 0, 1}
            }

            For Each motion In motions
                Dim ra As Double()() = motion.a.Rotation
                Dim rows As Double()() = Zeros3()

                For i As Integer = 0 To 2
                    For j As Integer = 0 To 2
                        rows(i)(j) = ra(i)(j) - identity(i)(j)
                    Next
                Next

                Dim rhs As Vector3d = rx.TransformDirection(motion.b.Translation) - motion.a.Translation

                For i As Integer = 0 To 2
                    For j As Integer = 0 To 2
                        For k As Integer = 0 To 2
                            ata(i)(j) += rows(k)(i) * rows(k)(j)
                        Next
                    Next

                    Dim sum As Double = 0.0

                    For k As Integer = 0 To 2
                        sum += rows(k)(i) * rhs.ToArray()(k)
                    Next

                    atb(i) += sum
                Next
            Next

            Dim translation As Vector3d = Vector3d.Zero

            Try
                Dim solution As Double() = Linalg.SolveSymmetric(ata, atb)

                translation = New Vector3d(solution(0), solution(1), solution(2))
            Catch ex As InvalidOperationException
                ' 旋转激励不足时平移不可观：保持零平移，交给后续迭代/调用方判断
            End Try

            Return New RigidTransform(rotation, translation)
        End Function

        ''' <summary>旋转向量（轴 × 角）。</summary>
        Private Shared Function RotationVector(transform As RigidTransform) As Vector3d
            Dim quaternion As Quaternion = Quaternion.FromRotationMatrix(transform.Rotation)

            If quaternion.W < 0.0 Then
                quaternion = New Quaternion(-quaternion.W, -quaternion.X, -quaternion.Y, -quaternion.Z)
            End If

            Dim axis As New Vector3d(quaternion.X, quaternion.Y, quaternion.Z)

            If axis.IsZero(1.0E-12) Then Return Vector3d.Zero

            Return axis.Normalize() * quaternion.Angle
        End Function

        Private Shared Function Zeros3() As Double()()
            Return {
                New Double() {0, 0, 0},
                New Double() {0, 0, 0},
                New Double() {0, 0, 0}
            }
        End Function

        ''' <summary>
        ''' Gauss-Newton 主循环。
        ''' </summary>
        Private Shared Function Optimize(motions As List(Of (a As RigidTransform, b As RigidTransform)),
                                         initial As RigidTransform, opt As Options) As (transform As RigidTransform, iterations As Integer, converged As Boolean)
            Dim x As RigidTransform = initial
            Dim converged As Boolean = False
            Dim iterations As Integer = 0

            For iteration As Integer = 1 To std.Max(1, opt.MaxIterations)
                iterations = iteration

                Dim h As Double()() = New Double(5)() {}
                Dim g As Double() = New Double(5) {}

                For i As Integer = 0 To 5
                    h(i) = New Double(5) {}
                Next

                For Each motion In motions
                    Dim r As Double() = Residual(motion.a, x, motion.b)
                    Dim jacobian As Double()() = New Double(5)() {}

                    For k As Integer = 0 To 5
                        jacobian(k) = New Double(5) {}

                        Dim perturbation(5) As Double

                        perturbation(k) = opt.FiniteDifferenceStep

                        Dim bumped As RigidTransform = Exp(perturbation).Compose(x)
                        Dim r2 As Double() = Residual(motion.a, bumped, motion.b)

                        For row As Integer = 0 To 5
                            jacobian(k)(row) = (r2(row) - r(row)) / opt.FiniteDifferenceStep
                        Next
                    Next

                    For row As Integer = 0 To 5
                        g(row) += DotColumn(jacobian, r, row)

                        For col As Integer = 0 To 5
                            Dim sum As Double = 0.0

                            For k As Integer = 0 To 5
                                sum += jacobian(k)(row) * jacobian(k)(col)
                            Next

                            h(row)(col) += sum
                        Next
                    Next
                Next

                Dim delta As Double()

                Try
                    delta = Linalg.SolveN(h, Negate(g))
                Catch ex As InvalidOperationException
                    ' 初期残差很大时正规方程可能病态：加阻尼再试一次
                    Try
                        For i As Integer = 0 To 5
                            h(i)(i) += 1.0E-6 * std.Max(1.0, std.Abs(h(i)(i)))
                        Next

                        delta = Linalg.SolveN(h, Negate(g))
                    Catch ex2 As InvalidOperationException
                        Exit For
                    End Try
                End Try

                ' 步长限制：AX=XB 的残差是“旋转向量 + 平移”的混合量纲，
                ' 初期（X 远离真值）一步走太远会直接发散，这里把增量范数压到 0.2 以内。
                Dim stepNorm As Double = 0.0

                For i As Integer = 0 To 5
                    stepNorm += delta(i) * delta(i)
                Next

                stepNorm = std.Sqrt(stepNorm)

                If stepNorm > 0.2 Then
                    Dim inv As Double = 0.2 / stepNorm

                    For i As Integer = 0 To 5
                        delta(i) *= inv
                    Next
                End If

                Dim maxStep As Double = 0.0

                For k As Integer = 0 To 5
                    maxStep = std.Max(maxStep, std.Abs(delta(k)))
                Next

                x = Exp(delta).Compose(x)

                If maxStep < opt.Epsilon Then
                    converged = True

                    Exit For
                End If
            Next

            Return (x, iterations, converged)
        End Function

        ''' <summary>
        ''' 单组位姿的残差：<c>Log((A·X)⁻¹·(X·B))</c>。
        ''' </summary>
        Private Shared Function Residual(a As RigidTransform, x As RigidTransform, b As RigidTransform) As Double()
            ' 注意 Compose 的语义是“左乘后施加”：p.Compose(q) 表示 q ∘ p（先 p 后 q）。
            ' 因此 A·X 要写成 x.Compose(a)，X·B 写成 x.Compose(b)。
            Dim left As RigidTransform = x.Compose(a)
            Dim right As RigidTransform = x.Compose(b)
            Dim delta As RigidTransform = left.Inverse().Compose(right)

            Return LogOf(delta)
        End Function

        Private Shared Function DotColumn(jacobian As Double()(), residual As Double(), row As Integer) As Double
            Dim sum As Double = 0.0

            For k As Integer = 0 To 5
                sum += jacobian(k)(row) * residual(k)
            Next

            Return sum
        End Function

        ''' <summary>SE(3) 对数映射。</summary>
        Private Shared Function LogOf(transform As RigidTransform) As Double()
            Dim quaternion As Quaternion = Quaternion.FromRotationMatrix(transform.Rotation)

            If quaternion.W < 0.0 Then
                quaternion = New Quaternion(-quaternion.W, -quaternion.X, -quaternion.Y, -quaternion.Z)
            End If

            Dim angle As Double = quaternion.Angle
            Dim axis As New Vector3d(quaternion.X, quaternion.Y, quaternion.Z)
            Dim rotation As Vector3d

            If axis.IsZero(1.0E-12) Then
                rotation = New Vector3d(quaternion.X * 2.0, quaternion.Y * 2.0, quaternion.Z * 2.0)
            Else
                rotation = axis.Normalize() * angle
            End If

            Return {rotation.X, rotation.Y, rotation.Z,
                    transform.Translation.X, transform.Translation.Y, transform.Translation.Z}
        End Function

        ''' <summary>SE(3) 指数映射。</summary>
        Private Shared Function Exp(v As Double()) As RigidTransform
            Dim omega As New Vector3d(v(0), v(1), v(2))
            Dim translation As New Vector3d(v(3), v(4), v(5))
            Dim angle As Double = omega.Length

            If angle <= 1.0E-15 Then Return RigidTransform.FromTranslation(translation)

            Return RigidTransform.FromAxisAngle(omega / angle, angle, translation)
        End Function

        Private Shared Function Negate(v As Double()) As Double()
            Dim out(v.Length - 1) As Double

            For i As Integer = 0 To v.Length - 1
                out(i) = -v(i)
            Next

            Return out
        End Function

    End Class
End Namespace
