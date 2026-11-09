Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Reconstruction
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Segmentation
Imports std = System.Math

Namespace Applications

    ''' <summary>形位公差类型（GB/T 1182 / ISO 1101 的常用子集）。</summary>
    Public Enum ToleranceType
        ''' <summary>平面度。</summary>
        Flatness = 0
        ''' <summary>直线度。</summary>
        Straightness = 1
        ''' <summary>圆度。</summary>
        Roundness = 2
        ''' <summary>圆柱度。</summary>
        Cylindricity = 3
        ''' <summary>平行度。</summary>
        Parallelism = 4
        ''' <summary>垂直度。</summary>
        Perpendicularity = 5
        ''' <summary>倾斜度。</summary>
        Angularity = 6
        ''' <summary>位置度。</summary>
        Position = 7
        ''' <summary>同轴度。</summary>
        Coaxiality = 8
    End Enum

    ''' <summary>
    ''' 单项形位公差的计算结果。
    ''' </summary>
    Public Class ToleranceResult

        ''' <summary>公差类型。</summary>
        Public Property Type As ToleranceType

        ''' <summary>
        ''' 最小区域法（min-zone）的公差值：这是标准规定的合格判据。
        ''' </summary>
        Public Property Value As Double

        ''' <summary>最小二乘法估计值（便于与 CAD 报告对照）。</summary>
        Public Property LeastSquaresValue As Double

        ''' <summary>图纸公差上限（&lt;= 0 表示只计算不判定）。</summary>
        Public Property Limit As Double

        ''' <summary>参与评价的点数。</summary>
        Public Property PointCount As Integer

        ''' <summary>是否合格。</summary>
        Public ReadOnly Property Passed As Boolean
            Get
                If Limit <= 0.0 Then Return True

                Return Value <= Limit
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim judge As String = If(Limit > 0.0, If(Passed, "PASS", "FAIL"), "n/a")

            Return $"{Type}: {Value:G5} (LSQ {LeastSquaresValue:G5}), limit={Limit:G5} -> {judge}"
        End Function
    End Class

    ''' <summary>
    ''' 形位公差计算：最小区域法与最小二乘法双结果。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么必须给“最小区域”而不是最小二乘</b>：ISO 1101 定义形位公差为
    ''' “包含实际要素、且宽度/直径为最小的理想要素所限定的区域”。
    ''' 最小二乘法给出的是“平均意义上的最优”，它的残差极差通常比最小区域<b>大</b>，
    ''' 因此直接用小二乘值判定会导致误判合格/不合格。
    ''' </para>
    ''' <para>
    ''' <b>求解方法</b>：最小区域判据是一个 minimax 优化
    ''' （最小化“最大偏差 − 最小偏差”）。本实现用<b>模式搜索</b>
    ''' （pattern search：沿坐标方向试探、步长逐步收缩）在 2~4 维参数空间上求解。
    ''' 相比线性规划：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>优点：实现简单、对任何几何量都通用（平面/圆/圆柱/同轴都可以套同一套搜索）。</description></item>
    ''' <item><description>缺点：理论上是局部最优；但形位公差的解空间是凸的（minimax 凸问题），
    ''' 从最小二乘解出发的模式搜索在实践中能找到全局最优。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>样本要求</b>：圆度/圆柱度需要 ≥ 120° 弧长，平面度需要覆盖整个被测面；
    ''' 样本不足时两种方法都会给出偏乐观的结果。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class FormTolerance

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 平面度：包含所有点的两平行平面之间的最小距离。
        ''' </summary>
        Public Shared Function Flatness(points As Vector3d(), Optional limit As Double = 0.0) As ToleranceResult
            If points Is Nothing OrElse points.Length < 3 Then
                Throw New ArgumentException("at least 3 points are required for flatness!", NameOf(points))
            End If

            Dim plane As PlaneModel = PlaneModel.Fit(points)
            Dim lsqWidth As Double = Width(points, plane.Normal)

            ' 最小区域：微调法向（两个小角度）使投影极差最小
            Dim baseNormal As Vector3d = plane.Normal
            Dim reference As Vector3d = If(std.Abs(baseNormal.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
            Dim u As Vector3d = (reference - baseNormal * Vector3d.Dot(reference, baseNormal)).Normalize()
            Dim v As Vector3d = Vector3d.Cross(baseNormal, u)

            Dim parameters As Double() = {0.0, 0.0}
            Dim value As Double = PatternSearch(parameters,
                Function(p)
                    Dim normal As Vector3d = (baseNormal + u * p(0) + v * p(1)).Normalize()

                    Return Width(points, normal)
                End Function, 0.02, 40)

            Return New ToleranceResult With {
                .Type = ToleranceType.Flatness,
                .Value = value,
                .LeastSquaresValue = lsqWidth,
                .Limit = limit,
                .PointCount = points.Length
            }
        End Function

        ''' <summary>
        ''' 直线度：包含所有点的最小圆柱（或两平行线）的直径。
        ''' </summary>
        Public Shared Function Straightness(points As Vector3d(), Optional limit As Double = 0.0) As ToleranceResult
            If points Is Nothing OrElse points.Length < 3 Then
                Throw New ArgumentException("at least 3 points are required for straightness!", NameOf(points))
            End If

            Dim axis As Vector3d = PrincipalAxis(points)
            Dim lsq As Double = RadialRange(points, axis, points(0))

            Dim reference As Vector3d = If(std.Abs(axis.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
            Dim u As Vector3d = (reference - axis * Vector3d.Dot(reference, axis)).Normalize()
            Dim v As Vector3d = Vector3d.Cross(axis, u)

            Dim parameters As Double() = {0.0, 0.0, 0.0, 0.0}
            Dim value As Double = PatternSearch(parameters,
                Function(p)
                    Dim direction As Vector3d = (axis + u * p(0) + v * p(1)).Normalize()
                    Dim origin As Vector3d = points(0) + u * p(2) + v * p(3)

                    Return RadialRange(points, direction, origin)
                End Function, 0.01, 40)

            Return New ToleranceResult With {
                .Type = ToleranceType.Straightness,
                .Value = value,
                .LeastSquaresValue = lsq,
                .Limit = limit,
                .PointCount = points.Length
            }
        End Function

        ''' <summary>
        ''' 圆度：投影到垂直于轴的平面后，“最大半径 − 最小半径”的最小值。
        ''' </summary>
        ''' <param name="points">被测点</param>
        ''' <param name="axis">轴线方向（<c>Nothing</c> 表示用 PCA 主轴）</param>
        ''' <param name="limit">公差上限</param>
        Public Shared Function Roundness(points As Vector3d(), Optional axis As Vector3d? = Nothing,
                                         Optional limit As Double = 0.0) As ToleranceResult
            If points Is Nothing OrElse points.Length < 3 Then
                Throw New ArgumentException("at least 3 points are required for roundness!", NameOf(points))
            End If

            Dim direction As Vector3d = If(axis.HasValue, axis.Value.Normalize(), PrincipalAxis(points))
            Dim center As Vector3d = Centroid(points)
            Dim reference As Vector3d = If(std.Abs(direction.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
            Dim u As Vector3d = (reference - direction * Vector3d.Dot(reference, direction)).Normalize()
            Dim v As Vector3d = Vector3d.Cross(direction, u)

            Dim lsq As Double = RadiusRange(points, center, u, v)
            Dim parameters As Double() = {0.0, 0.0}
            Dim value As Double = PatternSearch(parameters,
                Function(p) RadiusRange(points, center + u * p(0) + v * p(1), u, v),
                0.01, 40)

            Return New ToleranceResult With {
                .Type = ToleranceType.Roundness,
                .Value = value,
                .LeastSquaresValue = lsq,
                .Limit = limit,
                .PointCount = points.Length
            }
        End Function

        ''' <summary>
        ''' 圆柱度：圆柱面所有点到理想轴线的半径极差的最小值。
        ''' </summary>
        Public Shared Function Cylindricity(points As Vector3d(), Optional limit As Double = 0.0) As ToleranceResult
            If points Is Nothing OrElse points.Length < 6 Then
                Throw New ArgumentException("at least 6 points are required for cylindricity!", NameOf(points))
            End If

            Dim axis As Vector3d = PrincipalAxis(points)
            Dim origin As Vector3d = Centroid(points)
            Dim reference As Vector3d = If(std.Abs(axis.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
            Dim u As Vector3d = (reference - axis * Vector3d.Dot(reference, axis)).Normalize()
            Dim v As Vector3d = Vector3d.Cross(axis, u)

            Dim lsq As Double = CylinderRadiusRange(points, axis, origin)
            Dim parameters As Double() = {0.0, 0.0, 0.0, 0.0}
            Dim value As Double = PatternSearch(parameters,
                Function(p)
                    Dim direction As Vector3d = (axis + u * p(0) + v * p(1)).Normalize()
                    Dim point As Vector3d = origin + u * p(2) + v * p(3)

                    Return CylinderRadiusRange(points, direction, point)
                End Function, 0.01, 40)

            Return New ToleranceResult With {
                .Type = ToleranceType.Cylindricity,
                .Value = value,
                .LeastSquaresValue = lsq,
                .Limit = limit,
                .PointCount = points.Length
            }
        End Function

        ''' <summary>
        ''' 平行度：被测面对基准面的“两平行平面区域”宽度。
        ''' </summary>
        ''' <param name="datumNormal">基准面的法向</param>
        ''' <param name="points">被测面的点</param>
        ''' <param name="limit">公差上限</param>
        Public Shared Function Parallelism(datumNormal As Vector3d, points As Vector3d(),
                                           Optional limit As Double = 0.0) As ToleranceResult
            If points Is Nothing OrElse points.Length < 3 Then
                Throw New ArgumentException("at least 3 points are required for parallelism!", NameOf(points))
            End If

            Dim normal As Vector3d = datumNormal.Normalize()
            Dim lsqWidth As Double = Width(points, normal)

            Dim parameters As Double() = {0.0, 0.0}
            Dim reference As Vector3d = If(std.Abs(normal.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
            Dim u As Vector3d = (reference - normal * Vector3d.Dot(reference, normal)).Normalize()
            Dim v As Vector3d = Vector3d.Cross(normal, u)

            Dim value As Double = PatternSearch(parameters,
                Function(p) Width(points, (normal + u * p(0) + v * p(1)).Normalize()),
                0.02, 40)

            Return New ToleranceResult With {
                .Type = ToleranceType.Parallelism,
                .Value = value,
                .LeastSquaresValue = lsqWidth,
                .Limit = limit,
                .PointCount = points.Length
            }
        End Function

        ''' <summary>
        ''' 垂直度：被测面法向与基准法向的夹角偏离 90° 造成的区域宽度。
        ''' </summary>
        Public Shared Function Perpendicularity(datumNormal As Vector3d, points As Vector3d(),
                                                Optional limit As Double = 0.0) As ToleranceResult
            If points Is Nothing OrElse points.Length < 3 Then
                Throw new ArgumentException("at least 3 points are required for perpendicularity!", NameOf(points))
            End If

            Dim datum As Vector3d = datumNormal.Normalize()
            Dim plane As PlaneModel = PlaneModel.Fit(points)
            Dim angle As Double = plane.Normal.AngleTo(datum)
            Dim deviation As Double = std.Abs(angle - std.PI * 0.5)

            ' 把角度偏差换算成“被测面尺寸范围内的线性偏差”
            Dim box As Aabb3 = Aabb3.FromPoints(points)
            Dim extent As Double = std.Max(box.Diagonal, 1.0E-9)

            Return New ToleranceResult With {
                .Type = ToleranceType.Perpendicularity,
                .Value = 2.0 * std.Sin(deviation) * extent * 0.5,
                .LeastSquaresValue = deviation,
                .Limit = limit,
                .PointCount = points.Length
            }
        End Function

        ''' <summary>
        ''' 倾斜度：被测面相对基准的夹角偏离名义角度造成的区域宽度。
        ''' </summary>
        Public Shared Function Angularity(datumNormal As Vector3d, nominalAngleDegrees As Double,
                                          points As Vector3d(), Optional limit As Double = 0.0) As ToleranceResult
            If points Is Nothing OrElse points.Length < 3 Then
                Throw New ArgumentException("at least 3 points are required for angularity!", NameOf(points))
            End If

            Dim plane As PlaneModel = PlaneModel.Fit(points)
            Dim measured As Double = plane.Normal.AngleTo(datumNormal.Normalize()) * 180.0 / std.PI
            Dim deviation As Double = std.Abs(measured - (180.0 - nominalAngleDegrees))

            Dim box As Aabb3 = Aabb3.FromPoints(points)
            Dim extent As Double = std.Max(box.Diagonal, 1.0E-9)

            Return New ToleranceResult With {
                .Type = ToleranceType.Angularity,
                .Value = 2.0 * std.Sin(deviation * std.PI / 180.0) * extent * 0.5,
                .LeastSquaresValue = deviation,
                .Limit = limit,
                .PointCount = points.Length
            }
        End Function

        ''' <summary>
        ''' 位置度：实际点相对理想要素的位置偏差（直径值 = 2 × 偏移量）。
        ''' </summary>
        Public Shared Function Position(actual As Vector3d, nominal As Vector3d,
                                        Optional limit As Double = 0.0) As ToleranceResult
            Dim offset As Double = (actual - nominal).Length

            Return New ToleranceResult With {
                .Type = ToleranceType.Position,
                .Value = offset * 2.0,
                .LeastSquaresValue = offset,
                .Limit = limit,
                .PointCount = 1
            }
        End Function

        ''' <summary>
        ''' 同轴度：两段圆柱的轴线之间的最大径向偏移（直径值）。
        ''' </summary>
        Public Shared Function Coaxiality(pointsA As Vector3d(), pointsB As Vector3d(),
                                          Optional limit As Double = 0.0) As ToleranceResult
            If pointsA Is Nothing OrElse pointsB Is Nothing Then Throw New ArgumentNullException(NameOf(pointsA))
            If pointsA.Length < 3 OrElse pointsB.Length < 3 Then
                Throw New ArgumentException("each segment should contain at least 3 points!", NameOf(pointsB))
            End If

            Dim axisA As Vector3d = PrincipalAxis(pointsA)
            Dim axisB As Vector3d = PrincipalAxis(pointsB)
            Dim originA As Vector3d = Centroid(pointsA)
            Dim originB As Vector3d = Centroid(pointsB)

            ' 取两条轴线的公共参数范围内若干采样点的最大径向偏移
            Dim direction As Vector3d = (axisA + axisB).Normalize()
            Dim minT As Double = Double.MaxValue, maxT As Double = Double.MinValue

            For Each p As Vector3d In pointsA
                Dim t As Double = Vector3d.Dot(p - originA, direction)

                minT = std.Min(minT, t)
                maxT = std.Max(maxT, t)
            Next

            For Each p As Vector3d In pointsB
                Dim t As Double = Vector3d.Dot(p - originA, direction)

                minT = std.Min(minT, t)
                maxT = std.Max(maxT, t)
            Next

            Dim maxOffset As Double = 0.0

            For sample As Integer = 0 To 20
                Dim t As Double = minT + (maxT - minT) * sample / 20.0
                Dim pointOnA As Vector3d = originA + direction * t
                Dim pointOnB As Vector3d = originB + direction * t
                Dim offset As Double = (pointOnA - pointOnB).Length

                maxOffset = std.Max(maxOffset, offset)
            Next

            Return New ToleranceResult With {
                .Type = ToleranceType.Coaxiality,
                .Value = maxOffset * 2.0,
                .LeastSquaresValue = maxOffset,
                .Limit = limit,
                .PointCount = pointsA.Length + pointsB.Length
            }
        End Function

#Region "geometry helpers"

        ''' <summary>点集在给定法向上的投影极差。</summary>
        Private Shared Function Width(points As Vector3d(), normal As Vector3d) As Double
            Dim minValue As Double = Double.MaxValue
            Dim maxValue As Double = Double.MinValue

            For Each p As Vector3d In points
                Dim value As Double = Vector3d.Dot(p, normal)

                minValue = std.Min(minValue, value)
                maxValue = std.Max(maxValue, value)
            Next

            Return maxValue - minValue
        End Function

        ''' <summary>点到轴线的径向距离极差。</summary>
        Private Shared Function RadialRange(points As Vector3d(), direction As Vector3d, origin As Vector3d) As Double
            Dim minValue As Double = Double.MaxValue
            Dim maxValue As Double = Double.MinValue

            For Each p As Vector3d In points
                Dim d As Vector3d = p - origin
                Dim along As Double = Vector3d.Dot(d, direction)
                Dim radial As Double = (d - direction * along).Length

                minValue = std.Min(minValue, radial)
                maxValue = std.Max(maxValue, radial)
            Next

            Return maxValue - minValue
        End Function

        ''' <summary>投影到平面后的半径极差。</summary>
        Private Shared Function RadiusRange(points As Vector3d(), center As Vector3d,
                                            u As Vector3d, v As Vector3d) As Double
            Dim minValue As Double = Double.MaxValue
            Dim maxValue As Double = Double.MinValue

            For Each p As Vector3d In points
                Dim d As Vector3d = p - center
                Dim x As Double = Vector3d.Dot(d, u)
                Dim y As Double = Vector3d.Dot(d, v)
                Dim r As Double = std.Sqrt(x * x + y * y)

                minValue = std.Min(minValue, r)
                maxValue = std.Max(maxValue, r)
            Next

            Return maxValue - minValue
        End Function

        ''' <summary>圆柱面半径极差（三维，不做平面投影）。</summary>
        Private Shared Function CylinderRadiusRange(points As Vector3d(), direction As Vector3d,
                                                    origin As Vector3d) As Double
            Return RadialRange(points, direction, origin)
        End Function

        Private Shared Function Centroid(points As Vector3d()) As Vector3d
            Dim sum As Vector3d = Vector3d.Zero

            For Each p As Vector3d In points
                sum += p
            Next

            Return sum / std.Max(1, points.Length)
        End Function

        ''' <summary>PCA 主轴（最大特征向量）。</summary>
        Private Shared Function PrincipalAxis(points As Vector3d()) As Vector3d
            Dim center As Vector3d = Centroid(points)
            Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
            Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

            For Each p As Vector3d In points
                Dim d As Vector3d = p - center

                cxx += d.X * d.X
                cxy += d.X * d.Y
                cxz += d.X * d.Z
                cyy += d.Y * d.Y
                cyz += d.Y * d.Z
                czz += d.Z * d.Z
            Next

            Return Linalg.EigenSymmetric3(cxx, cxy, cxz, cyy, cyz, czz).V1
        End Function

        ''' <summary>
        ''' 模式搜索（坐标方向试探 + 步长收缩）。
        ''' </summary>
        ''' <param name="initial">初始参数（通常取最小二乘解）</param>
        ''' <param name="objective">目标函数</param>
        ''' <param name="initialStep">初始步长</param>
        ''' <param name="maxIterations">最大迭代次数</param>
        ''' <remarks>
        ''' minimax 型形位公差目标函数不可微（出现 max/min），梯度法不可用；
        ''' 模式搜索只需要函数值，且对参数维度（2~4 维）不敏感。
        ''' 步长每次收缩 0.5，直到小于 1e-9 或达到迭代上限。
        ''' </remarks>
        Private Shared Function PatternSearch(initial As Double(), objective As Func(Of Double(), Double),
                                              initialStep As Double, maxIterations As Integer) As Double
            Dim parameters As Double() = DirectCast(initial.Clone(), Double())
            Dim best As Double = objective(parameters)
            Dim stepSize As Double = initialStep

            For iteration As Integer = 1 To std.Max(1, maxIterations)
                Dim improved As Boolean = False

                For dimension As Integer = 0 To parameters.Length - 1
                    For Each sign As Double In {1.0, -1.0}
                        Dim trial As Double() = DirectCast(parameters.Clone(), Double())

                        trial(dimension) += sign * stepSize

                        Dim value As Double = objective(trial)

                        If value < best - 1.0E-15 Then
                            best = value
                            parameters = trial
                            improved = True

                            Exit For
                        End If
                    Next
                Next

                If Not improved Then
                    stepSize *= 0.5

                    If stepSize < 1.0E-9 Then Exit For
                End If
            Next

            Return best
        End Function

#End Region

    End Class
End Namespace
