Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Registration

    ''' <summary>
    ''' 位姿图的一个节点：某站点（或轨迹关键帧）的全局位姿。
    ''' </summary>
    Public Class PoseNode

        ''' <summary>节点编号（由 <see cref="PoseGraph"/> 自动分配）。</summary>
        Public Property Id As Integer

        ''' <summary>该节点在全局坐标系中的位姿（局部 → 全局）。</summary>
        Public Property Pose As RigidTransform

        ''' <summary>是否固定（固定节点用于锚定整体坐标系，避免优化结果整体漂移）。</summary>
        Public Property IsFixed As Boolean

        Public Sub New(pose As RigidTransform, Optional isFixed As Boolean = False)
            Me.Pose = pose
            Me.IsFixed = isFixed
        End Sub

        Public Overrides Function ToString() As String
            Return $"node {Id}: {Pose}"
        End Function
    End Class

    ''' <summary>
    ''' 位姿图的一条边：两个节点之间的相对位姿观测。
    ''' </summary>
    ''' <remarks>
    ''' 语义约定：<c>T_to = T_from ∘ Measurement</c>，即测量值是“从 from 的坐标系
    ''' 到 to 的坐标系”的变换。多站配准时它来自两站的配准结果；
    ''' 里程计/SLAM 中它来自相邻帧的帧间配准。
    ''' </remarks>
    Public Class PoseEdge

        Public Property FromId As Integer
        Public Property ToId As Integer
        Public Property Measurement As RigidTransform

        ''' <summary>观测权重（信息矩阵的标量近似；默认 1）。</summary>
        Public Property Weight As Double = 1.0

        ''' <summary>是否参与优化（可用来剔除被判定为错误的回环）。</summary>
        Public Property Enabled As Boolean = True

        Public Sub New(fromId As Integer, toId As Integer, measurement As RigidTransform, Optional weight As Double = 1.0)
            Me.FromId = fromId
            Me.ToId = toId
            Me.Measurement = measurement
            Me.Weight = weight
        End Sub

        Public Overrides Function ToString() As String
            Return $"{FromId} -> {ToId} (w={Weight:G3})"
        End Function
    End Class

    ''' <summary>
    ''' 位姿图容器。
    ''' </summary>
    Public Class PoseGraph

        Private ReadOnly m_nodes As New List(Of PoseNode)()
        Private ReadOnly m_edges As New List(Of PoseEdge)()

        ''' <summary>全部节点。</summary>
        Public ReadOnly Property Nodes As List(Of PoseNode)
            Get
                Return m_nodes
            End Get
        End Property

        ''' <summary>全部边。</summary>
        Public ReadOnly Property Edges As List(Of PoseEdge)
            Get
                Return m_edges
            End Get
        End Property

        ''' <summary>添加节点并返回编号。</summary>
        Public Function AddNode(pose As RigidTransform, Optional isFixed As Boolean = False) As Integer
            Dim node As New PoseNode(pose, isFixed) With {.Id = m_nodes.Count}

            Call m_nodes.Add(node)

            Return node.Id
        End Function

        ''' <summary>添加相对位姿观测。</summary>
        Public Sub AddEdge(fromId As Integer, toId As Integer, measurement As RigidTransform,
                           Optional weight As Double = 1.0)
            If fromId < 0 OrElse fromId >= m_nodes.Count Then
                Throw New ArgumentOutOfRangeException(NameOf(fromId), $"invalid node id {fromId}!")
            End If
            If toId < 0 OrElse toId >= m_nodes.Count Then
                Throw New ArgumentOutOfRangeException(NameOf(toId), $"invalid node id {toId}!")
            End If

            Call m_edges.Add(New PoseEdge(fromId, toId, measurement, weight))
        End Sub

        Public Overrides Function ToString() As String
            Return $"pose graph: {m_nodes.Count} nodes, {m_edges.Count} edges"
        End Function
    End Class

    ''' <summary>
    ''' 位姿图优化结果。
    ''' </summary>
    Public Class PoseGraphResult

        ''' <summary>优化后的全局位姿（按节点编号索引）。</summary>
        Public Property Poses As RigidTransform()

        ''' <summary>迭代次数。</summary>
        Public Property Iterations As Integer

        ''' <summary>初始残差平方和。</summary>
        Public Property InitialError As Double

        ''' <summary>最终残差平方和。</summary>
        Public Property FinalError As Double

        ''' <summary>是否收敛。</summary>
        Public Property Converged As Boolean

        Public Overrides Function ToString() As String
            Return $"pose graph: {Iterations} iterations, error {InitialError:G4} -> {FinalError:G4}, converged={Converged}"
        End Function
    End Class

    ''' <summary>
    ''' 位姿图优化：SE(3) 上的 Gauss-Newton 最小二乘。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么多站配准需要它</b>：逐站串接（A→B→C→D）会把每一站的配准误差累加，
    ''' 一台大型工件扫描 8 站之后，首尾站之间可能已经错开数毫米。
    ''' 位姿图把所有站点的相对观测放在同一个最小二乘问题里，
    ''' 让误差被“均匀分摊”，并利用回环（首站与末站的重复扫描区域）消除累积漂移。
    ''' </para>
    ''' <para>
    ''' <b>残差定义</b>：每条边给出一组观测 <c>Z</c>，残差取
    ''' <c>r = log( (T_i ∘ Z)⁻¹ ∘ T_j ) ∈ R⁶</c>（旋转向量 + 平移）。
    ''' 使用 <c>log</c> 而不是直接减矩阵元素，是因为旋转矩阵只有 3 个自由度，
    ''' 直接相减会引入“非正交分量”被无意义地惩罚。
    ''' </para>
    ''' <para>
    ''' <b>本实现的取舍</b>：雅可比用<b>数值差分</b>（每节点 6 个参数、中心扰动），
    ''' 正规方程按稠密矩阵求解。这在站点数 &lt; 200 的多站扫描场景下
    ''' 比手写解析雅可比更快落地、更不容易出错；站点数上万（SLAM）时应改用
    ''' 稀疏求解器与解析雅可比。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class PoseGraphOptimizer

        Private Sub New()
        End Sub

        ''' <summary>优化参数。</summary>
        Public Class Options

            ''' <summary>最大迭代次数（默认 30）。</summary>
            Public Property MaxIterations As Integer = 30

            ''' <summary>收敛阈值（参数增量的最大绝对值）。</summary>
            Public Property Epsilon As Double = 1.0E-10

            ''' <summary>数值雅可比的扰动步长（弧度/长度）。</summary>
            Public Property FiniteDifferenceStep As Double = 1.0E-6

            ''' <summary>Levenberg 阻尼初值（正规方程病态时自动放大）。</summary>
            Public Property Damping As Double = 1.0E-9
        End Class

        ''' <summary>
        ''' 执行优化（不修改传入的 <see cref="PoseGraph"/>，结果在返回值中）。
        ''' </summary>
        Public Shared Function Optimize(graph As PoseGraph, Optional options As Options = Nothing) As PoseGraphResult
            If graph Is Nothing Then Throw New ArgumentNullException(NameOf(graph))

            Dim opt As Options = If(options, New Options())
            Dim nodeCount As Integer = graph.Nodes.Count

            Dim poses(nodeCount - 1) As RigidTransform
            Dim freeIndex(nodeCount - 1) As Integer
            Dim freeCount As Integer = 0

            For i As Integer = 0 To nodeCount - 1
                poses(i) = graph.Nodes(i).Pose

                If graph.Nodes(i).IsFixed OrElse graph.Nodes(i).Pose Is Nothing Then
                    freeIndex(i) = -1
                Else
                    freeIndex(i) = freeCount
                    freeCount += 1
                End If
            Next

            Dim initialError As Double = ComputeError(graph, poses)
            Dim finalError As Double = initialError
            Dim converged As Boolean = False
            Dim iterations As Integer = 0

            If freeCount = 0 OrElse graph.Edges.Count = 0 Then
                Return New PoseGraphResult With {
                    .Poses = poses,
                    .Iterations = 0,
                    .InitialError = initialError,
                    .FinalError = initialError,
                    .Converged = True
                }
            End If

            Dim paramCount As Integer = freeCount * 6

            For iteration As Integer = 1 To std.Max(1, opt.MaxIterations)
                iterations = iteration

                Dim h As Double()() = New Double(paramCount - 1)() {}
                Dim g As Double() = New Double(paramCount - 1) {}

                For i As Integer = 0 To paramCount - 1
                    h(i) = New Double(paramCount - 1) {}
                Next

                For Each edge As PoseEdge In graph.Edges
                    If Not edge.Enabled Then Continue For

                    Dim iIdx As Integer = freeIndex(edge.FromId)
                    Dim jIdx As Integer = freeIndex(edge.ToId)
                    Dim r0 As Double() = Residual(poses(edge.FromId), edge.Measurement, poses(edge.ToId))
                    Dim weight As Double = std.Max(0.0, edge.Weight)

                    If weight <= 0.0 Then Continue For

                    ' 数值雅可比：分别扰动两个节点的 6 个参数
                    Dim ji As Double()() = Nothing
                    Dim jj As Double()() = Nothing

                    ' 扰动必须施加在“当前估计”上，而不是节点的初始位姿：
                    ' 用初始位姿做差分会让雅可比始终停留在第一轮，后续迭代方向错误。
                    If iIdx >= 0 Then ji = NumericJacobian(poses(edge.FromId), poses, edge, True)
                    If jIdx >= 0 Then jj = NumericJacobian(poses(edge.ToId), poses, edge, False)

                    ' 正规方程累加
                    For r As Integer = 0 To 5
                        If iIdx >= 0 Then
                            For c As Integer = 0 To 5
                                Dim rowI As Integer = iIdx * 6 + r
                                Dim colI As Integer = iIdx * 6 + c
                                Dim sum As Double = 0.0

                                For k As Integer = 0 To 5
                                    sum += ji(k)(r) * ji(k)(c)
                                Next

                                h(rowI)(colI) += weight * sum
                            Next

                            g(iIdx * 6 + r) += weight * DotColumn(ji, r0, r)
                        End If

                        If jIdx >= 0 Then
                            For c As Integer = 0 To 5
                                Dim rowJ As Integer = jIdx * 6 + r
                                Dim colJ As Integer = jIdx * 6 + c
                                Dim sum As Double = 0.0

                                For k As Integer = 0 To 5
                                    sum += jj(k)(r) * jj(k)(c)
                                Next

                                h(rowJ)(colJ) += weight * sum
                            Next

                            g(jIdx * 6 + r) += weight * DotColumn(jj, r0, r)
                        End If

                        If iIdx >= 0 AndAlso jIdx >= 0 Then
                            For c As Integer = 0 To 5
                                Dim sum As Double = 0.0

                                For k As Integer = 0 To 5
                                    sum += ji(k)(r) * jj(k)(c)
                                Next

                                h(iIdx * 6 + r)(jIdx * 6 + c) += weight * sum
                                h(jIdx * 6 + c)(iIdx * 6 + r) += weight * sum
                            Next
                        End If
                    Next
                Next

                Dim delta As Double()

                Try
                    delta = Linalg.SolveN(h, Negate(g))
                Catch ex As InvalidOperationException
                    ' 系统奇异：加阻尼再试一次（Levenberg 风格）
                    Try
                        For i As Integer = 0 To paramCount - 1
                            h(i)(i) += std.Max(opt.Damping, 1.0E-9) * std.Max(1.0, std.Abs(h(i)(i)))
                        Next

                        delta = Linalg.SolveN(h, Negate(g))
                    Catch ex2 As InvalidOperationException
                        Exit For
                    End Try
                End Try

                Dim maxStep As Double = 0.0

                For i As Integer = 0 To nodeCount - 1
                    Dim index As Integer = freeIndex(i)

                    If index < 0 Then Continue For

                    Dim deltaStep(5) As Double

                    For k As Integer = 0 To 5
                        deltaStep(k) = delta(index * 6 + k)
                        maxStep = std.Max(maxStep, std.Abs(deltaStep(k)))
                    Next

                    poses(i) = Exp(deltaStep).Compose(poses(i))
                Next

                finalError = ComputeError(graph, poses)

                If maxStep < opt.Epsilon Then
                    converged = True

                    Exit For
                End If
            Next

            Return New PoseGraphResult With {
                .Poses = poses,
                .Iterations = iterations,
                .InitialError = initialError,
                .FinalError = finalError,
                .Converged = converged
            }
        End Function

        ''' <summary>
        ''' 数值雅可比：<c>J(k, r) = ∂r_r / ∂δ_k</c>（扰动某一节点的位姿）。
        ''' </summary>
        Private Shared Function NumericJacobian(currentPose As RigidTransform, poses As RigidTransform(),
                                                edge As PoseEdge, perturbFrom As Boolean) As Double()()
            Dim jacobian As Double()() = New Double(5)() {}
            Dim epsilon As Double = 1.0E-6

            For i As Integer = 0 To 5
                jacobian(i) = New Double(5) {}
            Next

            ' 差分必须减去基线残差：漏掉这一步会让雅可比带上 r0/ε 的巨大常数项
            ' （r0 约 1e-3、ε=1e-6 时为 1e3 量级），从而彻底破坏正规方程的方向。
            Dim baseline As Double() = Residual(If(perturbFrom, currentPose, poses(edge.FromId)),
                                               edge.Measurement,
                                               If(perturbFrom, poses(edge.ToId), currentPose))

            For k As Integer = 0 To 5
                Dim perturbation(5) As Double

                perturbation(k) = epsilon

                Dim bumped As RigidTransform = Exp(perturbation).Compose(currentPose)
                Dim fromPose As RigidTransform = If(perturbFrom, bumped, poses(edge.FromId))
                Dim toPose As RigidTransform = If(perturbFrom, poses(edge.ToId), bumped)
                Dim r As Double() = Residual(fromPose, edge.Measurement, toPose)

                For rIdx As Integer = 0 To 5
                    jacobian(k)(rIdx) = (r(rIdx) - baseline(rIdx)) / epsilon
                Next
            Next

            Return jacobian
        End Function

        Private Shared Function DotColumn(jacobian As Double()(), residual As Double(), row As Integer) As Double
            Dim sum As Double = 0.0

            For k As Integer = 0 To 5
                sum += jacobian(k)(row) * residual(k)
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 单条边的残差：<c>log( (T_i ∘ Z)⁻¹ ∘ T_j )</c>。
        ''' </summary>
        Private Shared Function Residual(fromPose As RigidTransform, measurement As RigidTransform,
                                         toPose As RigidTransform) As Double()
            ' T_to = T_from ∘ Z：先施加测量 Z，再施加 T_from。
            ' Compose 的语义是“左乘后施加”，所以写成 measurement.Compose(fromPose)。
            Dim predicted As RigidTransform = measurement.Compose(fromPose)
            Dim delta As RigidTransform = predicted.Inverse().Compose(toPose)

            Return LogOf(delta)
        End Function

        ''' <summary>
        ''' 残差平方和（加权）。
        ''' </summary>
        Private Shared Function ComputeError(graph As PoseGraph, poses As RigidTransform()) As Double
            Dim total As Double = 0.0

            For Each edge As PoseEdge In graph.Edges
                If Not edge.Enabled Then Continue For

                Dim r As Double() = Residual(poses(edge.FromId), edge.Measurement, poses(edge.ToId))

                For k As Integer = 0 To 5
                    total += edge.Weight * r(k) * r(k)
                Next
            Next

            Return total
        End Function

        ''' <summary>
        ''' 刚体变换的 SE(3) 对数映射：旋转向量 + 平移。
        ''' </summary>
        Private Shared Function LogOf(transform As RigidTransform) As Double()
            Dim quaternion As Quaternion = Quaternion.FromRotationMatrix(transform.Rotation)

            If quaternion.W < 0.0 Then
                quaternion = New Quaternion(-quaternion.W, -quaternion.X, -quaternion.Y, -quaternion.Z)
            End If

            Dim angle As Double = quaternion.Angle
            Dim axis As New Vector3d(quaternion.X, quaternion.Y, quaternion.Z)
            Dim rotation As Vector3d

            If axis.IsZero(1.0E-12) Then
                ' 小角度：rotvec ≈ 2·(x, y, z)
                rotation = New Vector3d(quaternion.X * 2.0, quaternion.Y * 2.0, quaternion.Z * 2.0)
            Else
                rotation = axis.Normalize() * angle
            End If

            Return {rotation.X, rotation.Y, rotation.Z,
                    transform.Translation.X, transform.Translation.Y, transform.Translation.Z}
        End Function

        ''' <summary>
        ''' SE(3) 指数映射：旋转向量 + 平移 → 刚体变换。
        ''' </summary>
        Private Shared Function Exp(v As Double()) As RigidTransform
            Dim omega As New Vector3d(v(0), v(1), v(2))
            Dim translation As New Vector3d(v(3), v(4), v(5))
            Dim angle As Double = omega.Length

            If angle <= 1.0E-15 Then
                Return RigidTransform.FromTranslation(translation)
            End If

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
