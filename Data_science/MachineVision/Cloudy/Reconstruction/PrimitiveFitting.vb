Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Segmentation
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' 基元最小二乘拟合结果（含残差与尺寸参数）。
    ''' </summary>
    Public Class FittingResult

        ''' <summary>拟合出的基元。</summary>
        Public Property Model As PrimitiveModel

        ''' <summary>参与拟合的点数。</summary>
        Public Property PointCount As Integer

        ''' <summary>点到模型的均方根残差。</summary>
        Public Property Rmse As Double

        ''' <summary>最大残差。</summary>
        Public Property MaxError As Double

        ''' <summary>残差不超过 <c>tolerance</c> 的点比例。</summary>
        Public Property InlierRatio As Double

        ''' <summary>使用的容差。</summary>
        Public Property Tolerance As Double

        ''' <summary>
        ''' 尺寸参数（直径/半径/锥角等），键名与工业习惯一致。
        ''' </summary>
        Public Function Dimensions() As Dictionary(Of String, Double)
            Dim values As New Dictionary(Of String, Double)()

            If Model Is Nothing Then Return values

            Select Case Model.Type
                Case PrimitiveType.Sphere
                    Dim sphere As SphereModel = DirectCast(Model, SphereModel)

                    values("radius") = sphere.Radius
                    values("diameter") = sphere.Radius * 2.0
                Case PrimitiveType.Cylinder
                    Dim cylinder As CylinderModel = DirectCast(Model, CylinderModel)

                    values("radius") = cylinder.Radius
                    values("diameter") = cylinder.Diameter
                    values("axisX") = cylinder.AxisDirection.X
                    values("axisY") = cylinder.AxisDirection.Y
                    values("axisZ") = cylinder.AxisDirection.Z
                Case PrimitiveType.Cone
                    Dim cone As ConeModel = DirectCast(Model, ConeModel)

                    values("halfAngleDegrees") = cone.HalfAngle * 180.0 / std.PI
                    values("apexX") = cone.Apex.X
                    values("apexY") = cone.Apex.Y
                    values("apexZ") = cone.Apex.Z
                Case PrimitiveType.Plane
                    Dim plane As PlaneModel = DirectCast(Model, PlaneModel)

                    values("normalX") = plane.Normal.X
                    values("normalY") = plane.Normal.Y
                    values("normalZ") = plane.Normal.Z
                    values("offset") = plane.Offset
            End Select

            Return values
        End Function

        Public Overrides Function ToString() As String
            Return $"{Model?.Type}: {PointCount} points, rmse={Rmse:G4}, max={MaxError:G4}, inliers={InlierRatio:P1}"
        End Function
    End Class

    ''' <summary>
    ''' 基元最小二乘拟合：平面/球/圆柱/圆锥的参数与尺寸提取。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>与 <see cref="ModelFitting"/> 的分工</b>：后者负责“在含噪点云里找出
    ''' 哪个子集属于哪个基元”（RANSAC 的鲁棒性）；本类负责“给定一组已经确认属于
    ''' 同一基元的点，求最精确的参数并输出工业尺寸”。
    ''' 工业流程通常是：聚类/裁剪 → RANSAC 定初值 → 本类精拟合 → 尺寸判定。
    ''' </para>
    ''' <para>
    ''' <b>尺寸提取的用途</b>：圆柱的直径、球的半径、锥角直接对应图纸公差，
    ''' 是最常见的“点云 → 尺寸”环节；平面则给出法向与平面度初值。
    ''' 更严格的形位公差（最小区域）见 <c>Applications.FormTolerance</c>。
    ''' </para>
    ''' <para>
    ''' <b>精度提示</b>：代数最小二乘（尤其是球的 Kasa 方法）对“小弧段/半遮挡”
    ''' 样本有系统偏差；计量级测量请保证样本覆盖 ≥ 120° 的弧长，
    ''' 或改用最小区域拟合。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class PrimitiveFitting

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 按类型拟合（自动补齐法向，供圆柱/圆锥使用）。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="type">基元类型</param>
        ''' <param name="indices">参与拟合的点下标（<c>Nothing</c> 表示全部）</param>
        ''' <param name="tolerance">内点容差；<c>≤ 0</c> 表示按点距自适应</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Fit(cloud As PointCloudBuffer, type As PrimitiveType,
                                   Optional indices As Integer() = Nothing,
                                   Optional tolerance As Double = 0.0,
                                   Optional options As CloudyOptions = Nothing) As FittingResult
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim global_ As CloudyOptions = If(options, CloudyOptions.Default)
            Dim selected As Integer() = If(indices, AllIndices(cloud.Count))

            If selected.Length < 3 Then
                Throw New ArgumentException("at least 3 points are required for primitive fitting!", NameOf(indices))
            End If

            Dim source As PointCloudBuffer = cloud

            If type = PrimitiveType.Cylinder OrElse type = PrimitiveType.Cone Then
                If Not cloud.HasNormals Then source = NormalEstimation.Estimate(cloud, 20, 0.0, global_)
            End If

            Dim points As New List(Of Vector3d)(selected.Length)
            Dim normals As New List(Of Vector3d)(selected.Length)

            For Each index As Integer In selected
                Call points.Add(source.Item(index))

                If source.HasNormals Then Call normals.Add(source.Normal(index))
            Next

            Dim model As PrimitiveModel

            Select Case type
                Case PrimitiveType.Plane
                    model = PlaneModel.Fit(points.ToArray())
                Case PrimitiveType.Sphere
                    model = SphereModel.Fit(points.ToArray())
                Case PrimitiveType.Cylinder
                    model = CylinderModel.Fit(points.ToArray(), normals.ToArray())
                Case PrimitiveType.Cone
                    model = ConeModel.Fit(points.ToArray(), normals.ToArray())
                Case Else
                    Throw New NotSupportedException($"unsupported primitive type: {type}")
            End Select

            Dim toleranceValue As Double = If(tolerance > 0.0, tolerance, std.Max(source.EstimateAverageSpacing() * 2.0, 1.0E-6))
            Dim sumSquared As Double = 0.0
            Dim maxError As Double = 0.0
            Dim inliers As Integer = 0

            For Each p As Vector3d In points
                Dim d As Double = model.DistanceTo(p)

                sumSquared += d * d
                maxError = std.Max(maxError, d)
                If d <= toleranceValue Then inliers += 1
            Next

            Return New FittingResult With {
                .Model = model,
                .PointCount = points.Count,
                .Rmse = std.Sqrt(sumSquared / points.Count),
                .MaxError = maxError,
                .InlierRatio = CDbl(inliers) / points.Count,
                .Tolerance = toleranceValue
            }
        End Function

        ''' <summary>平面拟合。</summary>
        Public Shared Function FitPlane(cloud As PointCloudBuffer, Optional indices As Integer() = Nothing,
                                        Optional tolerance As Double = 0.0,
                                        Optional options As CloudyOptions = Nothing) As FittingResult
            Return Fit(cloud, PrimitiveType.Plane, indices, tolerance, options)
        End Function

        ''' <summary>球拟合。</summary>
        Public Shared Function FitSphere(cloud As PointCloudBuffer, Optional indices As Integer() = Nothing,
                                         Optional tolerance As Double = 0.0,
                                         Optional options As CloudyOptions = Nothing) As FittingResult
            Return Fit(cloud, PrimitiveType.Sphere, indices, tolerance, options)
        End Function

        ''' <summary>圆柱拟合。</summary>
        Public Shared Function FitCylinder(cloud As PointCloudBuffer, Optional indices As Integer() = Nothing,
                                           Optional tolerance As Double = 0.0,
                                           Optional options As CloudyOptions = Nothing) As FittingResult
            Return Fit(cloud, PrimitiveType.Cylinder, indices, tolerance, options)
        End Function

        ''' <summary>圆锥拟合。</summary>
        Public Shared Function FitCone(cloud As PointCloudBuffer, Optional indices As Integer() = Nothing,
                                       Optional tolerance As Double = 0.0,
                                       Optional options As CloudyOptions = Nothing) As FittingResult
            Return Fit(cloud, PrimitiveType.Cone, indices, tolerance, options)
        End Function

        ''' <summary>
        ''' 把拟合出的平面转换成“平面坐标系”的变换（用于局部坐标系下的测量）。
        ''' </summary>
        Public Shared Function PlaneFrame(plane As PlaneModel, origin As Vector3d) As RigidTransform
            If plane Is Nothing Then Throw New ArgumentNullException(NameOf(plane))

            Dim normal As Vector3d = plane.Normal.Normalize()
            Dim reference As Vector3d = If(std.Abs(normal.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
            Dim xAxis As Vector3d = (reference - normal * Vector3d.Dot(reference, normal)).Normalize()
            Dim yAxis As Vector3d = Vector3d.Cross(normal, xAxis)

            Dim rotation As Double()() = {
                New Double() {xAxis.X, yAxis.X, normal.X},
                New Double() {xAxis.Y, yAxis.Y, normal.Y},
                New Double() {xAxis.Z, yAxis.Z, normal.Z}
            }

            Return New RigidTransform(rotation, origin)
        End Function

        Private Shared Function AllIndices(n As Integer) As Integer()
            Dim indices(n - 1) As Integer

            For i As Integer = 0 To n - 1
                indices(i) = i
            Next

            Return indices
        End Function

    End Class
End Namespace
