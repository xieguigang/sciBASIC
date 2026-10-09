Imports Microsoft.VisualBasic.Imaging.Physics.Math3D
Imports std = System.Math

Namespace Collision3D

    ''' <summary>
    ''' 3D 碰撞体的形状种类。
    ''' </summary>
    ''' <remarks>
    ''' 枚举值的顺序同时也是窄相位的"规范顺序"：<see cref="NarrowPhase3D"/> 只实现
    ''' 前者小于后者的组合，因此顺序必须与已实现的分支一致
    ''' （球 &lt; 胶囊 &lt; 盒 &lt; 平面）。
    ''' </remarks>
    Public Enum ShapeKind3D
        ''' <summary>球体，用于头部、关节端点。</summary>
        Sphere = 0
        ''' <summary>胶囊体（线段 + 半径），用于四肢与躯干。</summary>
        Capsule = 1
        ''' <summary>轴对齐长方体（随刚体旋转成为 OBB），用于台阶、障碍等静态几何。</summary>
        Box = 2
        ''' <summary>无限平面，用于地面。</summary>
        Plane = 3
    End Enum

    ''' <summary>
    ''' 3D 碰撞体抽象基类。与 2D 的 <see cref="Collision.Collider"/> 一样，
    ''' 几何以质心为原点定义，配合刚体的位置与姿态变换到世界坐标。
    ''' </summary>
    Public MustInherit Class Collider3D

        ''' <summary>形状种类，窄相位据此分派。</summary>
        Public MustOverride ReadOnly Property Kind As ShapeKind3D

        ''' <summary>
        ''' 计算相对于质心的局部惯性张量（对角矩阵即可覆盖本库的全部形状，
        ''' 因为它们都关于局部轴对称）。
        ''' </summary>
        Public MustOverride Function ComputeInertia(mass As Double) As Matrix3x3

        ''' <summary>给定刚体位置与姿态，求世界坐标下的 AABB。</summary>
        Public MustOverride Function GetAABB(position As Vector3, rotation As Quaternion) As AABB3D

        ''' <summary>
        ''' 沿方向 <paramref name="dir"/> 的最远支撑点（局部坐标），供 SAT / GJK 使用。
        ''' </summary>
        Public MustOverride Function Support(dir As Vector3) As Vector3

        ''' <summary>
        ''' 由旋转矩阵求旋转后的半长投影：世界第 i 轴的半径 = Σ_j |M(j,i)| · half_j。
        ''' </summary>
        Protected Shared Function RotatedExtents(rotation As Quaternion, half As Vector3) As Vector3
            Dim m As Matrix3x3 = rotation.ToMatrix3x3()

            Return New Vector3(
                std.Abs(m.m11) * half.x + std.Abs(m.m21) * half.y + std.Abs(m.m31) * half.z,
                std.Abs(m.m12) * half.x + std.Abs(m.m22) * half.y + std.Abs(m.m32) * half.z,
                std.Abs(m.m13) * half.x + std.Abs(m.m23) * half.y + std.Abs(m.m33) * half.z)
        End Function
    End Class

    ''' <summary>
    ''' 球体碰撞体。惯性张量 = (2/5)·m·r² 的对角矩阵。
    ''' </summary>
    Public Class SphereCollider3D : Inherits Collider3D

        ''' <summary>半径。</summary>
        Public Radius As Double

        Sub New(radius As Double)
            Me.Radius = std.Max(radius, 1.0E-6)
        End Sub

        Public Overrides ReadOnly Property Kind As ShapeKind3D
            Get
                Return ShapeKind3D.Sphere
            End Get
        End Property

        Public Overrides Function ComputeInertia(mass As Double) As Matrix3x3
            Dim i As Double = 0.4 * mass * Radius * Radius
            Return Matrix3x3.Diagonal(i, i, i)
        End Function

        Public Overrides Function GetAABB(position As Vector3, rotation As Quaternion) As AABB3D
            Return New AABB3D(
                New Vector3(position.x - Radius, position.y - Radius, position.z - Radius),
                New Vector3(position.x + Radius, position.y + Radius, position.z + Radius))
        End Function

        Public Overrides Function Support(dir As Vector3) As Vector3
            Dim n As Vector3 = Vector3Math.Normalize(dir)
            Return n * Radius
        End Function
    End Class

    ''' <summary>
    ''' 长方体碰撞体。构造参数传入完整尺寸（与 2D 的 <c>PhysicsWorld.Box(width, height)</c> 一致）。
    ''' </summary>
    Public Class BoxCollider3D : Inherits Collider3D

        ''' <summary>半长（各轴半径）。</summary>
        Public ReadOnly Property HalfExtents As Vector3

        Sub New(width As Double, height As Double, depth As Double)
            Me.HalfExtents = New Vector3(std.Max(width, 1.0E-6) * 0.5,
                                         std.Max(height, 1.0E-6) * 0.5,
                                         std.Max(depth, 1.0E-6) * 0.5)
        End Sub

        ''' <summary>完整尺寸。</summary>
        Public ReadOnly Property Size As Vector3
            Get
                Return HalfExtents * 2.0
            End Get
        End Property

        Public Overrides ReadOnly Property Kind As ShapeKind3D
            Get
                Return ShapeKind3D.Box
            End Get
        End Property

        Public Overrides Function ComputeInertia(mass As Double) As Matrix3x3
            Dim h As Vector3 = HalfExtents
            ' I_xx = m/3·(hy² + hz²)，其余按轴对称类推
            Return Matrix3x3.Diagonal(
                mass / 3.0 * (h.y * h.y + h.z * h.z),
                mass / 3.0 * (h.x * h.x + h.z * h.z),
                mass / 3.0 * (h.x * h.x + h.y * h.y))
        End Function

        Public Overrides Function GetAABB(position As Vector3, rotation As Quaternion) As AABB3D
            Dim e As Vector3 = RotatedExtents(rotation, HalfExtents)

            Return New AABB3D(
                New Vector3(position.x - e.x, position.y - e.y, position.z - e.z),
                New Vector3(position.x + e.x, position.y + e.y, position.z + e.z))
        End Function

        Public Overrides Function Support(dir As Vector3) As Vector3
            ' 先把方向转到局部系，取局部支撑点后再转回世界
            Dim local As Vector3 = dir
            Dim h As Vector3 = HalfExtents

            Return New Vector3(
                If(local.x >= 0, h.x, -h.x),
                If(local.y >= 0, h.y, -h.y),
                If(local.z >= 0, h.z, -h.z))
        End Function
    End Class

    ''' <summary>
    ''' 胶囊体碰撞体：局部 Y 轴上的一条线段加上半径。用于火柴人的四肢与躯干。
    ''' </summary>
    Public Class CapsuleCollider3D : Inherits Collider3D

        ''' <summary>半径。</summary>
        Public Radius As Double
        ''' <summary>两个端点球心之间的距离（不含半径）。</summary>
        Public Height As Double

        Sub New(radius As Double, height As Double)
            Me.Radius = std.Max(radius, 1.0E-6)
            Me.Height = std.Max(height, 0.0)
        End Sub

        ''' <summary>胶囊总长度（含两端半球）。</summary>
        Public ReadOnly Property TotalLength As Double
            Get
                Return Height + 2.0 * Radius
            End Get
        End Property

        ''' <summary>局部坐标下线段的两端（沿局部 Y 轴）。</summary>
        Public ReadOnly Property LocalEndA As Vector3
            Get
                Return New Vector3(0, -Height * 0.5, 0)
            End Get
        End Property

        ''' <summary>局部坐标下线段的另一端。</summary>
        Public ReadOnly Property LocalEndB As Vector3
            Get
                Return New Vector3(0, Height * 0.5, 0)
            End Get
        End Property

        Public Overrides ReadOnly Property Kind As ShapeKind3D
            Get
                Return ShapeKind3D.Capsule
            End Get
        End Property

        Public Overrides Function ComputeInertia(mass As Double) As Matrix3x3
            ' 按等质量实心圆柱近似
            Dim r2 As Double = Radius * Radius
            Dim lateral As Double = mass * (3.0 * r2 + Height * Height) / 12.0
            Dim axial As Double = mass * r2 * 0.5

            Return Matrix3x3.Diagonal(lateral, axial, lateral)
        End Function

        Public Overrides Function GetAABB(position As Vector3, rotation As Quaternion) As AABB3D
            Dim half As New Vector3(Radius, Height * 0.5 + Radius, Radius)
            Dim e As Vector3 = RotatedExtents(rotation, half)

            Return New AABB3D(
                New Vector3(position.x - e.x, position.y - e.y, position.z - e.z),
                New Vector3(position.x + e.x, position.y + e.y, position.z + e.z))
        End Function

        Public Overrides Function Support(dir As Vector3) As Vector3
            Dim n As Vector3 = Vector3Math.Normalize(dir)

            If n.y >= 0 Then
                Return LocalEndB + n * Radius
            Else
                Return LocalEndA + n * Radius
            End If
        End Function
    End Class

    ''' <summary>
    ''' 无限平面碰撞体，用于地面。平面方程 <c>dot(Normal, p) = Constant</c>，
    ''' 法向指向"空气"一侧（即物体所在的一侧）。
    ''' </summary>
    Public Class PlaneCollider3D : Inherits Collider3D

        ''' <summary>单位法向。</summary>
        Public Normal As Vector3
        ''' <summary>平面到原点的有符号距离。</summary>
        Public Constant As Double

        Sub New(normal As Vector3, constant As Double)
            Me.Normal = Vector3Math.Normalize(normal)
            Me.Constant = constant
        End Sub

        ''' <summary>点 <paramref name="p"/> 到平面的有符号距离（正 = 在法向一侧）。</summary>
        Public Function SignedDistance(p As Vector3) As Double
            Return Vector3Math.Dot(Normal, p) - Constant
        End Function

        Public Overrides ReadOnly Property Kind As ShapeKind3D
            Get
                Return ShapeKind3D.Plane
            End Get
        End Property

        Public Overrides Function ComputeInertia(mass As Double) As Matrix3x3
            ' 平面恒为静态，不参与转动
            Return Matrix3x3.Zero
        End Function

        Public Overrides Function GetAABB(position As Vector3, rotation As Quaternion) As AABB3D
            Const huge As Double = 1.0E+9

            Return New AABB3D(
                New Vector3(-huge, -huge, -huge),
                New Vector3(huge, huge, huge))
        End Function

        Public Overrides Function Support(dir As Vector3) As Vector3
            ' 无限平面没有有限支撑点：返回一个远离平面的点，仅用于防御性调用
            Dim n As Vector3 = Vector3Math.Normalize(dir)
            Dim onPlane As Vector3 = Normal * Constant

            Return onPlane + n * 1.0E+9
        End Function
    End Class
End Namespace
