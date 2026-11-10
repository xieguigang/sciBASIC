Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Reconstruction
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Registration
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Applications

    ''' <summary>
    ''' 偏差分析结果（CAD 比对的输出）。
    ''' </summary>
    Public Class DeviationResult

        ''' <summary>逐点到参考面的距离（无符号）。</summary>
        Public Property Distances As Double()

        ''' <summary>逐点偏差（带符号：正 = 在参考面法向一侧）。</summary>
        Public Property SignedDistances As Double()

        ''' <summary>最大偏差（带符号，取绝对值最大者）。</summary>
        Public Property MaxDeviation As Double

        ''' <summary>最大绝对偏差。</summary>
        Public Property MaxAbsoluteDeviation As Double

        ''' <summary>平均偏差（带符号）。</summary>
        Public Property MeanDeviation As Double

        ''' <summary>标准差。</summary>
        Public Property StdDeviation As Double

        ''' <summary>均方根偏差（RMS）。</summary>
        Public Property Rms As Double

        ''' <summary>公差带宽（用于判定）。</summary>
        Public Property Tolerance As Double

        ''' <summary>超差点数。</summary>
        Public Property OutOfTolerance As Integer

        ''' <summary>超差比例。</summary>
        Public Property OutOfToleranceRatio As Double

        ''' <summary>偏差色谱（ARGB，与点一一对应；可直接交给渲染模块）。</summary>
        Public Property Colors As Integer()

        ''' <summary>是否全部在公差内。</summary>
        Public ReadOnly Property Passed As Boolean
            Get
                Return OutOfTolerance = 0
            End Get
        End Property

        ''' <summary>偏差直方图（用于报表）。</summary>
        Public Function Histogram(Optional bins As Integer = 32) As Double()
            Dim count As Integer = std.Max(2, bins)
            Dim hist As Double() = New Double(count - 1) {}

            If Distances Is Nothing OrElse Distances.Length = 0 Then Return hist

            Dim maxValue As Double = MaxAbsoluteDeviation

            If maxValue <= 0.0 Then
                hist(0) = Distances.Length

                Return hist
            End If

            For i As Integer = 0 To Distances.Length - 1
                Dim bin As Integer = CInt(std.Floor(Distances(i) / maxValue * count))

                If bin >= count Then bin = count - 1
                If bin < 0 Then bin = 0

                hist(bin) += 1
            Next

            Return hist
        End Function

        Public Overrides Function ToString() As String
            Return $"deviation: max={MaxAbsoluteDeviation:G4}, mean={MeanDeviation:G4}, " &
                $"std={StdDeviation:G4}, rms={Rms:G4}, out-of-tolerance={OutOfToleranceRatio:P2}, passed={Passed}"
        End Function
    End Class

    ''' <summary>
    ''' 扫描与参考模型（CAD/网格/参考点云）的偏差分析。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>工业流程</b>：扫描点云 → 与 CAD（或参考网格）最佳拟合对齐
    ''' → 逐点计算偏差 → 统计（最大/平均/标准差/RMS）→ 公差判定 → 色谱图。
    ''' 这是“扫描 vs 图纸”的通用检测链路，也是无监督 3D 异常检测的经典做法
    ''' （无训练、可解释、结果直接对应图纸公差）。
    ''' </para>
    ''' <para>
    ''' <b>对齐的两种方式</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><b>最佳拟合</b>（默认）：用 ICP 找“整体最小偏差”的位姿，
    ''' 适合自由形状（叶片、铸件）。</description></item>
    ''' <item><description><b>基准对齐</b>：先识别基准面/基准孔，再按 3-2-1 或
    ''' 基准坐标系对齐。计量级检测必须用这种方式（最佳拟合会把基准本身的偏差
    ''' 平均掉，掩盖真实的形位误差）。本类提供 <see cref="AlignToReference"/> 的
    ''' 最佳拟合版本，基准对齐请先用 <c>PrimitiveFitting</c> 提取基准再自行组合。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>关于 STEP/IGES</b>：本库不解析 CAD 文件（那是 CAD 内核/外部转换器的职责）。
    ''' 调用方应先由外部工具把 CAD 转换成三角网格（本类的 <see cref="MeshData"/>）
    ''' 或参考点云，再交给这里做偏差计算。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class ModelComparison

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 与参考网格比较。
        ''' </summary>
        ''' <param name="scan">扫描点云（应已对齐到参考坐标系）</param>
        ''' <param name="reference">参考网格（CAD 三角化结果）</param>
        ''' <param name="tolerance">公差带宽（与点云同单位）；<c>≤ 0</c> 表示只做统计不判定</param>
        ''' <param name="signed">是否计算带符号偏差（需要网格法向，稍慢）</param>
        Public Shared Function Compare(scan As PointCloudBuffer, reference As MeshData,
                                       Optional tolerance As Double = 0.0,
                                       Optional signed As Boolean = True,
                                       Optional options As CloudyOptions = Nothing) As DeviationResult
            If scan Is Nothing Then Throw New ArgumentNullException(NameOf(scan))
            If reference Is Nothing Then Throw New ArgumentNullException(NameOf(reference))

            Dim distances As Double() = MeshProcessing.PointToMeshDistance(reference, scan)
            Dim signedDistances As Double() = Nothing

            If signed Then
                signedDistances = SignDistances(scan, reference, distances)

                ' 用带符号版本重新统计（绝对值不变，但均值/最大会带方向）
                Return Summarize(signedDistances, distances, tolerance)
            End If

            Return Summarize(distances, distances, tolerance)
        End Function

        ''' <summary>
        ''' 与参考点云比较（没有网格时的退路：最近点距离）。
        ''' </summary>
        Public Shared Function CompareToPointCloud(scan As PointCloudBuffer, reference As PointCloudBuffer,
                                                   Optional tolerance As Double = 0.0,
                                                   Optional options As CloudyOptions = Nothing) As DeviationResult
            If scan Is Nothing Then Throw New ArgumentNullException(NameOf(scan))
            If reference Is Nothing Then Throw New ArgumentNullException(NameOf(reference))

            Dim tree As New KdTree3D(reference)
            Dim n As Integer = scan.Count
            Dim distances As Double() = New Double(n - 1) {}
            Dim buffer As New Neighborhood(1)

            For i As Integer = 0 To n - 1
                Dim nearest As (index As Integer, distanceSquared As Double) =
                    tree.Nearest(scan.X(i), scan.Y(i), scan.Z(i))

                distances(i) = If(nearest.index >= 0, std.Sqrt(nearest.distanceSquared), Double.MaxValue)
            Next

            Return Summarize(distances, distances, tolerance)
        End Function

        ''' <summary>
        ''' 用 ICP 把扫描对齐到参考网格（最佳拟合）。
        ''' </summary>
        ''' <remarks>
        ''' 把网格顶点当作目标点云做 ICP。对自由曲面零件，这是“整体最优对齐”；
        ''' 对计量级检测，请改用基准对齐。
        ''' </remarks>
        Public Shared Function AlignToReference(scan As PointCloudBuffer, reference As MeshData,
                                                Optional initial As RigidTransform = Nothing,
                                                Optional iterations As Integer = 40,
                                                Optional options As CloudyOptions = Nothing) As RegistrationResult
            If scan Is Nothing Then Throw New ArgumentNullException(NameOf(scan))
            If reference Is Nothing Then Throw New ArgumentNullException(NameOf(reference))

            Dim target As PointCloudBuffer = reference.ToPointCloud()
            Dim icp As New IcpRegistration.IcpOptions With {.MaxIterations = std.Max(1, iterations)}

            Return IcpRegistration.AlignPointToPlane(scan, target, initial, icp, options)
        End Function

        Private Shared Function SignDistances(scan As PointCloudBuffer, reference As MeshData,
                                              distances As Double()) As Double()
            Dim result As Double() = New Double(distances.Length - 1) {}

            ' 符号来自“最近顶点法向”：<c>(p − q)·n</c> 的符号。
            ' 精确做法应使用“最近点处的插值法向”，但对平滑网格的符号判定已经足够；
            ' 在锐边附近可能翻转，这一点在文档中已说明。
            Dim cloud As PointCloudBuffer = reference.ToPointCloud()
            Dim tree As New KdTree3D(cloud)

            For i As Integer = 0 To distances.Length - 1
                Dim p As Vector3d = scan.Item(i)
                Dim nearest As (index As Integer, distanceSquared As Double) =
                    tree.Nearest(p.X, p.Y, p.Z)

                If nearest.index < 0 Then
                    result(i) = distances(i)

                    Continue For
                End If

                Dim normal As Vector3d = cloud.Normal(nearest.index)

                If normal.IsZero(1.0E-9) Then
                    result(i) = distances(i)

                    Continue For
                End If

                Dim q As New Vector3d(tree.X(nearest.index), tree.Y(nearest.index), tree.Z(nearest.index))
                Dim sign As Double = If(Vector3d.Dot(p - q, normal) >= 0.0, 1.0, -1.0)

                result(i) = distances(i) * sign
            Next

            Return result
        End Function

        Private Shared Function Summarize(signedDistances As Double(), absoluteDistances As Double(),
                                          tolerance As Double) As DeviationResult
            Dim n As Integer = absoluteDistances.Length
            Dim result As New DeviationResult With {
                .Distances = absoluteDistances,
                .SignedDistances = signedDistances,
                .Tolerance = tolerance
            }

            If n = 0 Then
                result.MaxDeviation = 0.0
                result.MaxAbsoluteDeviation = 0.0
                result.MeanDeviation = 0.0
                result.StdDeviation = 0.0
                result.Rms = 0.0
                result.OutOfTolerance = 0
                result.OutOfToleranceRatio = 0.0
                result.Colors = Array.Empty(Of Integer)()

                Return result
            End If

            Dim maxAbs As Double = 0.0
            Dim maxSigned As Double = 0.0
            Dim sum As Double = 0.0

            For i As Integer = 0 To n - 1
                Dim value As Double = signedDistances(i)
                Dim magnitude As Double = absoluteDistances(i)

                sum += value

                If magnitude > maxAbs Then
                    maxAbs = magnitude
                    maxSigned = value
                End If
            Next

            Dim mean As Double = sum / n
            Dim sumSquares As Double = 0.0
            Dim variance As Double = 0.0

            For i As Integer = 0 To n - 1
                sumSquares += absoluteDistances(i) * absoluteDistances(i)
                variance += (signedDistances(i) - mean) ^ 2
            Next

            Dim outOfTolerance As Integer = 0

            If tolerance > 0.0 Then
                For i As Integer = 0 To n - 1
                    If absoluteDistances(i) > tolerance Then outOfTolerance += 1
                Next
            End If

            result.MaxDeviation = maxSigned
            result.MaxAbsoluteDeviation = maxAbs
            result.MeanDeviation = mean
            result.StdDeviation = std.Sqrt(variance / n)
            result.Rms = std.Sqrt(sumSquares / n)
            result.OutOfTolerance = outOfTolerance
            result.OutOfToleranceRatio = CDbl(outOfTolerance) / n
            result.Colors = ColorMap(signedDistances, If(tolerance > 0.0, tolerance, std.Max(maxAbs, 1.0E-9)))

            Return result
        End Function

        ''' <summary>
        ''' 偏差色谱（蓝 = 负偏差、绿 = 接近零、红 = 正偏差）。
        ''' </summary>
        ''' <remarks>
        ''' 输出 ARGB 整数，可直接作为点云的颜色属性交给渲染模块。
        ''' 分段线性映射，超出 <c>±scale</c> 的值饱和到端点颜色。
        ''' </remarks>
        Public Shared Function ColorMap(signedDistances As Double(), scale As Double) As Integer()
            Dim colors(signedDistances.Length - 1) As Integer

            If scale <= 0.0 Then scale = 1.0

            For i As Integer = 0 To signedDistances.Length - 1
                Dim t As Double = signedDistances(i) / scale

                If t > 1.0 Then t = 1.0
                If t < -1.0 Then t = -1.0

                Dim r As Integer
                Dim g As Integer
                Dim b As Integer

                If t <= 0.0 Then
                    ' 负偏差：蓝 → 绿
                    Dim s As Double = t + 1.0

                    r = CInt(255 * (1.0 - s))
                    g = CInt(255 * s)
                    b = CInt(255 * s)
                Else
                    ' 正偏差：绿 → 红
                    r = CInt(255 * t)
                    g = CInt(255 * (1.0 - t))
                    b = 0
                End If

                colors(i) = &HFF000000 Or (CInt(r) << 16) Or (CInt(g) << 8) Or CInt(b)
            Next

            Return colors
        End Function

    End Class
End Namespace
