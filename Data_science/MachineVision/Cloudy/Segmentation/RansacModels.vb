Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Segmentation

    ''' <summary>几何基元类型。</summary>
    Public Enum PrimitiveType
        Plane = 0
        Sphere = 1
        Cylinder = 2
        Cone = 3
    End Enum

    ''' <summary>
    ''' 几何基元的抽象：点到模型的有符号/无符号距离 + 参数向量。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' RANSAC 框架只需要两件东西：<b>最小样本拟合</b>（每个子类自己实现）
    ''' 与<b>点到模型的距离</b>（统一的判据）。因此这里把模型抽象成
    ''' “距离函数 + 参数向量”，让 <see cref="ModelFitting"/> 可以完全与具体基元解耦。
    ''' </para>
    ''' <para>
    ''' 距离一律用<b>无符号</b>欧氏距离（球/圆柱/圆锥的内外两侧都算内点），
    ''' 因为工业点云在配准之前无法可靠判断“哪一侧是实体内部”。
    ''' 需要符号时（例如平面分割后决定哪侧是工件），调用方用
    ''' <see cref="PlaneModel.SignedDistance(Vector3d)"/> 单独处理。
    ''' </para>
    ''' </remarks>
    Public MustInherit Class PrimitiveModel

        ''' <summary>基元类型。</summary>
        Public MustOverride ReadOnly Property Type As PrimitiveType

        ''' <summary>最小样本数（RANSAC 采样大小）。</summary>
        Public MustOverride ReadOnly Property MinimalSamples As Integer

        ''' <summary>参数向量（用于日志与尺寸提取）。</summary>
        Public MustOverride ReadOnly Property Parameters As Double()

        ''' <summary>点到模型的距离（无符号）。</summary>
        Public MustOverride Function DistanceTo(p As Vector3d) As Double

        Public Overrides Function ToString() As String
            Return $"{Type}: [{String.Join(", ", Parameters.Select(Function(d) d.ToString("G6")))}]"
        End Function
    End Class

    ''' <summary>
    ''' 平面模型：<c>n·p + d = 0</c>（<c>n</c> 为单位法向）。
    ''' </summary>
    Public NotInheritable Class PlaneModel
        Inherits PrimitiveModel

        ''' <summary>单位法向。</summary>
        Public ReadOnly Property Normal As Vector3d

        ''' <summary>平面方程的常数项。</summary>
        Public ReadOnly Property Offset As Double

        Public Sub New(normal As Vector3d, offset As Double)
            Me.Normal = normal.Normalize()
            Me.Offset = offset
        End Sub

        Public Overrides ReadOnly Property Type As PrimitiveType
            Get
                Return PrimitiveType.Plane
            End Get
        End Property

        Public Overrides ReadOnly Property MinimalSamples As Integer
            Get
                Return 3
            End Get
        End Property

        Public Overrides ReadOnly Property Parameters As Double()
            Get
                Return {Normal.X, Normal.Y, Normal.Z, Offset}
            End Get
        End Property

        ''' <summary>有符号距离（正侧为法向所指方向）。</summary>
        Public Function SignedDistance(p As Vector3d) As Double
            Return Vector3d.Dot(Normal, p) + Offset
        End Function

        Public Overrides Function DistanceTo(p As Vector3d) As Double
            Return std.Abs(SignedDistance(p))
        End Function

        ''' <summary>
        ''' 由三个不共线点构造平面。
        ''' </summary>
        Public Shared Function FromPoints(a As Vector3d, b As Vector3d, c As Vector3d) As PlaneModel
            Dim normal As Vector3d = Vector3d.Cross(b - a, c - a)

            If normal.IsZero(1.0E-12) Then Throw New ArgumentException("the three points are collinear!")

            normal = normal.Normalize()

            Return New PlaneModel(normal, -Vector3d.Dot(normal, a))
        End Function

        ''' <summary>
        ''' 最小二乘平面拟合（协方差最小特征向量）。
        ''' </summary>
        Public Shared Function Fit(points As Vector3d()) As PlaneModel
            If points Is Nothing OrElse points.Length < 3 Then
                Throw New ArgumentException("at least 3 points are required to fit a plane!", NameOf(points))
            End If

            Dim cx As Double = 0, cy As Double = 0, cz As Double = 0

            For Each p As Vector3d In points
                cx += p.X
                cy += p.Y
                cz += p.Z
            Next

            Dim inv As Double = 1.0 / points.Length

            cx *= inv
            cy *= inv
            cz *= inv

            Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
            Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

            For Each p As Vector3d In points
                Dim dx As Double = p.X - cx
                Dim dy As Double = p.Y - cy
                Dim dz As Double = p.Z - cz

                cxx += dx * dx
                cxy += dx * dy
                cxz += dx * dz
                cyy += dy * dy
                cyz += dy * dz
                czz += dz * dz
            Next

            Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(cxx, cxy, cxz, cyy, cyz, czz)
            Dim normal As Vector3d = eigen.V3
            Dim centroid As New Vector3d(cx, cy, cz)

            Return New PlaneModel(normal, -Vector3d.Dot(normal, centroid))
        End Function

        Public Overrides Function ToString() As String
            Return $"plane: n={Normal}, d={Offset:G6}"
        End Function
    End Class

    ''' <summary>
    ''' 球模型：<c>‖p − c‖ = r</c>。
    ''' </summary>
    Public NotInheritable Class SphereModel
        Inherits PrimitiveModel

        Public ReadOnly Property Center As Vector3d
        Public ReadOnly Property Radius As Double

        Public Sub New(center As Vector3d, radius As Double)
            Me.Center = center
            Me.Radius = std.Abs(radius)
        End Sub

        Public Overrides ReadOnly Property Type As PrimitiveType
            Get
                Return PrimitiveType.Sphere
            End Get
        End Property

        Public Overrides ReadOnly Property MinimalSamples As Integer
            Get
                Return 4
            End Get
        End Property

        Public Overrides ReadOnly Property Parameters As Double()
            Get
                Return {Center.X, Center.Y, Center.Z, Radius}
            End Get
        End Property

        Public Overrides Function DistanceTo(p As Vector3d) As Double
            Return std.Abs((p - Center).Length - Radius)
        End Function

        ''' <summary>
        ''' 代数最小二乘球拟合（Kasa 方法：把方程线性化）。
        ''' </summary>
        ''' <remarks>
        ''' <c>‖p‖² = 2c·p + (r² − ‖c‖²)</c> 对 <c>(2c, r²−‖c‖²)</c> 是线性的，
        ''' 因此 4 个点即可闭式求解；点数更多时是最小二乘。
        ''' 该估计对“半球/小弧段”样本偏差较大（代数误差与几何误差不等价），
        ''' 工业场景建议先分割出完整球面再用。
        ''' </remarks>
        Public Shared Function Fit(points As Vector3d()) As SphereModel
            If points Is Nothing OrElse points.Length < 4 Then
                Throw New ArgumentException("at least 4 points are required to fit a sphere!", NameOf(points))
            End If

            Dim n As Integer = points.Length
            Dim ata As Double()() = {
                New Double() {0, 0, 0, 0},
                New Double() {0, 0, 0, 0},
                New Double() {0, 0, 0, 0},
                New Double() {0, 0, 0, 0}
            }
            Dim atb As Double() = New Double(3) {}

            For Each p As Vector3d In points
                Dim row As Double() = {p.X, p.Y, p.Z, 1.0}
                Dim rhs As Double = p.X * p.X + p.Y * p.Y + p.Z * p.Z

                For r As Integer = 0 To 3
                    For c As Integer = 0 To 3
                        ata(r)(c) += row(r) * row(c)
                    Next

                    atb(r) += row(r) * rhs
                Next
            Next

            Dim coef As Double() = Linalg.SolveN(ata, atb)
            Dim center As New Vector3d(coef(0) * 0.5, coef(1) * 0.5, coef(2) * 0.5)
            Dim r2 As Double = coef(3) + Vector3d.Dot(center, center)
            Dim radius As Double = std.Sqrt(std.Max(r2, 0.0))

            Return New SphereModel(center, radius)
        End Function

        Public Overrides Function ToString() As String
            Return $"sphere: center={Center}, r={Radius:G6}"
        End Function
    End Class

    ''' <summary>
    ''' 圆柱模型：轴线 + 半径。
    ''' </summary>
    Public NotInheritable Class CylinderModel
        Inherits PrimitiveModel

        ''' <summary>轴线上的一点。</summary>
        Public ReadOnly Property AxisPoint As Vector3d

        ''' <summary>轴方向（单位向量）。</summary>
        Public ReadOnly Property AxisDirection As Vector3d

        Public ReadOnly Property Radius As Double

        Public Sub New(axisPoint As Vector3d, axisDirection As Vector3d, radius As Double)
            Me.AxisPoint = axisPoint
            Me.AxisDirection = axisDirection.Normalize()
            Me.Radius = std.Abs(radius)
        End Sub

        Public Overrides ReadOnly Property Type As PrimitiveType
            Get
                Return PrimitiveType.Cylinder
            End Get
        End Property

        Public Overrides ReadOnly Property MinimalSamples As Integer
            Get
                Return 6
            End Get
        End Property

        Public Overrides ReadOnly Property Parameters As Double()
            Get
                Return {AxisPoint.X, AxisPoint.Y, AxisPoint.Z,
                        AxisDirection.X, AxisDirection.Y, AxisDirection.Z, Radius}
            End Get
        End Property

        ''' <summary>直径（工业尺寸提取的直接结果）。</summary>
        Public ReadOnly Property Diameter As Double
            Get
                Return Radius * 2.0
            End Get
        End Property

        Public Overrides Function DistanceTo(p As Vector3d) As Double
            Dim d As Vector3d = p - AxisPoint
            Dim along As Double = Vector3d.Dot(d, AxisDirection)
            Dim radial As Vector3d = d - AxisDirection * along

            Return std.Abs(radial.Length - Radius)
        End Function

        ''' <summary>
        ''' 由点与法向拟合圆柱。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' <b>为什么需要法向</b>：只用点坐标拟合圆柱是非线性最小二乘（最小样本 5 点），
        ''' 在 RANSAC 里每次假设都要迭代求解，成本高且容易失败。
        ''' 圆柱面的法向都垂直于轴，因此 <c>Σ n·nᵀ</c> 的<b>最小特征向量</b>就是轴向，
        ''' 一步闭式求出；再把点投影到垂直于轴的平面做圆拟合（Kasa 代数解）。
        ''' </para>
        ''' <para>
        ''' 这个做法在工业扫描（法向可靠）上非常稳，也是 PCL <c>SACMODEL_CYLINDER</c>
        ''' 在正常估计法向后的实际行为。
        ''' </para>
        ''' </remarks>
        Public Shared Function Fit(points As Vector3d(), normals As Vector3d()) As CylinderModel
            If points Is Nothing OrElse normals Is Nothing Then Throw New ArgumentNullException(NameOf(points))
            If points.Length <> normals.Length Then
                Throw New ArgumentException("the point and normal arrays should have the same length!", NameOf(normals))
            End If
            If points.Length < 6 Then
                Throw New ArgumentException("at least 6 points are required to fit a cylinder!", NameOf(points))
            End If

            Dim nxx As Double = 0, nxy As Double = 0, nxz As Double = 0
            Dim nyy As Double = 0, nyz As Double = 0, nzz As Double = 0

            For i As Integer = 0 To points.Length - 1
                Dim n As Vector3d = normals(i).Normalize()

                nxx += n.X * n.X
                nxy += n.X * n.Y
                nxz += n.X * n.Z
                nyy += n.Y * n.Y
                nyz += n.Y * n.Z
                nzz += n.Z * n.Z
            Next

            Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(nxx, nxy, nxz, nyy, nyz, nzz)
            Dim axis As Vector3d = eigen.V3.Normalize()

            ' 投影到垂直于轴的平面上做圆拟合
            Dim reference As Vector3d = If(std.Abs(axis.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
            Dim u As Vector3d = (reference - axis * Vector3d.Dot(reference, axis)).Normalize()
            Dim v As Vector3d = Vector3d.Cross(axis, u)

            Dim ata As Double()() = {
                New Double() {0, 0, 0},
                New Double() {0, 0, 0},
                New Double() {0, 0, 0}
            }
            Dim atb As Double() = New Double(2) {}

            For i As Integer = 0 To points.Length - 1
                Dim x As Double = Vector3d.Dot(points(i), u)
                Dim y As Double = Vector3d.Dot(points(i), v)
                Dim row As Double() = {x, y, 1.0}
                Dim rhs As Double = x * x + y * y

                For r As Integer = 0 To 2
                    For c As Integer = 0 To 2
                        ata(r)(c) += row(r) * row(c)
                    Next

                    atb(r) += row(r) * rhs
                Next
            Next

            Dim coef As Double() = Linalg.SolveN(ata, atb)
            Dim cu As Double = coef(0) * 0.5
            Dim cv As Double = coef(1) * 0.5
            Dim r2 As Double = coef(2) + cu * cu + cv * cv
            Dim radius As Double = std.Sqrt(std.Max(r2, 0.0))

            ' 轴线上的参考点：取所有点沿轴的平均投影位置
            Dim along As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                along += Vector3d.Dot(points(i), axis)
            Next

            along /= points.Length

            Dim axisPoint As Vector3d = axis * along + u * cu + v * cv

            Return New CylinderModel(axisPoint, axis, radius)
        End Function

        Public Overrides Function ToString() As String
            Return $"cylinder: axis={AxisDirection}@{AxisPoint}, r={Radius:G6}"
        End Function
    End Class

    ''' <summary>
    ''' 圆锥模型：顶点 + 轴方向 + 半锥角。
    ''' </summary>
    Public NotInheritable Class ConeModel
        Inherits PrimitiveModel

        Public ReadOnly Property Apex As Vector3d
        Public ReadOnly Property AxisDirection As Vector3d
        ''' <summary>半锥角（弧度）。</summary>
        Public ReadOnly Property HalfAngle As Double

        Public Sub New(apex As Vector3d, axisDirection As Vector3d, halfAngle As Double)
            Me.Apex = apex
            Me.AxisDirection = axisDirection.Normalize()
            Me.HalfAngle = halfAngle
        End Sub

        Public Overrides ReadOnly Property Type As PrimitiveType
            Get
                Return PrimitiveType.Cone
            End Get
        End Property

        Public Overrides ReadOnly Property MinimalSamples As Integer
            Get
                Return 6
            End Get
        End Property

        Public Overrides ReadOnly Property Parameters As Double()
            Get
                Return {Apex.X, Apex.Y, Apex.Z, AxisDirection.X, AxisDirection.Y, AxisDirection.Z, HalfAngle}
            End Get
        End Property

        Public Overrides Function DistanceTo(p As Vector3d) As Double
            Dim d As Vector3d = p - Apex
            Dim along As Double = Vector3d.Dot(d, AxisDirection)

            If along <= 0.0 Then Return d.Length

            Dim radial As Double = (d - AxisDirection * along).Length
            Dim expected As Double = along * std.Tan(HalfAngle)

            Return std.Abs(radial - expected) * std.Cos(HalfAngle)
        End Function

        ''' <summary>
        ''' 由点与法向拟合圆锥（轴向与半锥角由法向分布估计，顶点位置最小二乘）。
        ''' </summary>
        ''' <remarks>
        ''' 圆锥面法向与轴的夹角恒为 <c>90° − 半锥角</c>，因此
        ''' 法向集合的方向分布可以给出轴向与半锥角；顶点位置再由
        ''' “半径随轴向距离线性增长”的关系做一维最小二乘。
        ''' 该估计对短锥段（&lt; 30° 弧长）不稳定，属于圆锥拟合的固有困难。
        ''' </remarks>
        Public Shared Function Fit(points As Vector3d(), normals As Vector3d()) As ConeModel
            If points Is Nothing OrElse normals Is Nothing Then Throw New ArgumentNullException(NameOf(points))
            If points.Length < 6 Then
                Throw New ArgumentException("at least 6 points are required to fit a cone!", NameOf(points))
            End If

            ' 用圆柱拟合先拿到轴向，再估计半锥角
            Dim cylinder As CylinderModel = CylinderModel.Fit(points, normals)
            Dim axis As Vector3d = cylinder.AxisDirection

            Dim averageAngle As Double = 0.0
            Dim count As Integer = 0

            For i As Integer = 0 To points.Length - 1
                Dim normal As Vector3d = normals(i).Normalize()

                If normal.IsZero(1.0E-9) Then Continue For

                Dim angle As Double = normal.AngleTo(axis)

                ' 法向与轴的夹角 = 90° ± 半锥角
                averageAngle += std.Abs(angle - std.PI * 0.5)
                count += 1
            Next

            If count = 0 Then Throw New ArgumentException("all normals are degenerate!")

            Dim halfAngle As Double = std.Max(1.0E-4, averageAngle / count)

            ' 顶点：沿轴向做半径线性回归 r = k·(s − s0)
            Dim reference As Vector3d = If(std.Abs(axis.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
            Dim u As Vector3d = (reference - axis * Vector3d.Dot(reference, axis)).Normalize()
            Dim v As Vector3d = Vector3d.Cross(axis, u)

            Dim sx As Double = 0, sy As Double = 0, sxx As Double = 0, sxy As Double = 0

            For i As Integer = 0 To points.Length - 1
                Dim s As Double = Vector3d.Dot(points(i), axis)
                Dim du As Double = Vector3d.Dot(points(i), u)
                Dim dv As Double = Vector3d.Dot(points(i), v)
                Dim r As Double = std.Sqrt(du * du + dv * dv)

                sx += s
                sy += r
                sxx += s * s
                sxy += s * r
            Next

            Dim n As Integer = points.Length
            Dim denominator As Double = n * sxx - sx * sx
            Dim slope As Double = If(std.Abs(denominator) > 1.0E-12, (n * sxy - sx * sy) / denominator, 0.0)
            Dim intercept As Double = (sy - slope * sx) / n

            ' r = slope·(s − s0) ⇒ s0 = −intercept/slope
            Dim s0 As Double = If(std.Abs(slope) > 1.0E-9, -intercept / slope, 0.0)

            If slope < 0.0 Then
                ' 顶点在负方向：翻转轴向以保持 slope > 0
                axis = -axis
                s0 = -s0
            End If

            Return New ConeModel(axis * s0, axis, halfAngle)
        End Function

        Public Overrides Function ToString() As String
            Return $"cone: apex={Apex}, axis={AxisDirection}, halfAngle={HalfAngle * 180.0 / std.PI:G4}°"
        End Function
    End Class

End Namespace
