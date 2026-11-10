Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 双精度三维向量值类型（点/方向/法向共用的基础值对象）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这是<b>逐点标量</b>运算的载体：单个点的坐标、法向、旋转轴、质心等。
    ''' 批量运算不使用这个结构体数组（AoS 会破坏向量化），而是走
    ''' <see cref="PointCloudBuffer"/> 的 SoA 布局与
    ''' <c>Microsoft.VisualBasic.Math.SIMD.SimdVector3</c> 内核；
    ''' 两者在需要时通过 <see cref="PointCloudBuffer.Item(Integer)"/> /
    ''' <see cref="PointCloudBuffer.ToVector3d"/> 互相转换。
    ''' </para>
    ''' <para>
    ''' 设计上是一个不可变值类型：所有运算返回新实例，避免共享别名导致的意外修改
    ''' （点云算法的调试成本极高，隐式的原地修改是最常见的错误来源）。
    ''' 需要在热路径上避免分配时，请改用 SoA 内核。
    ''' </para>
    ''' </remarks>
    Public Structure Vector3d

        ''' <summary>零向量。</summary>
        Public Shared ReadOnly Zero As Vector3d = New Vector3d(0, 0, 0)

        ''' <summary>坐标轴单位向量。</summary>
        Public Shared ReadOnly UnitX As Vector3d = New Vector3d(1, 0, 0)
        ''' <summary>坐标轴单位向量。</summary>
        Public Shared ReadOnly UnitY As Vector3d = New Vector3d(0, 1, 0)
        ''' <summary>坐标轴单位向量。</summary>
        Public Shared ReadOnly UnitZ As Vector3d = New Vector3d(0, 0, 1)

        Public ReadOnly X As Double
        Public ReadOnly Y As Double
        Public ReadOnly Z As Double

        Public Sub New(x As Double, y As Double, z As Double)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        Public Sub New(xyz As Double())
            If xyz Is Nothing OrElse xyz.Length < 3 Then
                Throw New ArgumentException("the coordinate array should have at least 3 elements!", NameOf(xyz))
            End If

            Me.X = xyz(0)
            Me.Y = xyz(1)
            Me.Z = xyz(2)
        End Sub

        ''' <summary>
        ''' 向量长度：<c>SQRT(x² + y² + z²)</c>。
        ''' </summary>
        Public ReadOnly Property Length As Double
            Get
                Return std.Sqrt(X * X + Y * Y + Z * Z)
            End Get
        End Property

        ''' <summary>
        ''' 平方长度（比较远近时省掉 <c>Sqrt</c>）。
        ''' </summary>
        Public ReadOnly Property LengthSquared As Double
            Get
                Return X * X + Y * Y + Z * Z
            End Get
        End Property

        ''' <summary>
        ''' 是否为（近似）零向量。
        ''' </summary>
        Public Function IsZero(Optional eps As Double = 1.0E-12) As Boolean
            Return LengthSquared <= eps * eps
        End Function

        ''' <summary>
        ''' 归一化为单位向量；零向量返回零向量（不产生 NaN）。
        ''' </summary>
        Public Function Normalize() As Vector3d
            Dim len As Double = Length

            If len <= 0.0 Then Return Zero

            Return New Vector3d(X / len, Y / len, Z / len)
        End Function

        ''' <summary>
        ''' 三点坐标中的最大绝对值（退化判定、尺度探测用）。
        ''' </summary>
        Public ReadOnly Property MaxAbs As Double
            Get
                Return std.Max(std.Abs(X), std.Max(std.Abs(Y), std.Abs(Z)))
            End Get
        End Property

        ''' <summary>
        ''' 与另一个向量的夹角（弧度，<c>[0, π]</c>）。任一侧为零向量时返回 0。
        ''' </summary>
        Public Function AngleTo(other As Vector3d) As Double
            Dim na As Double = LengthSquared
            Dim nb As Double = other.LengthSquared

            If na <= 0.0 OrElse nb <= 0.0 Then Return 0.0

            ' 先归一化点积再取 acos，避免大尺度坐标下的溢出/精度损失
            Dim c As Double = Dot(Me, other) / std.Sqrt(na * nb)

            If c > 1.0 Then c = 1.0
            If c < -1.0 Then c = -1.0

            Return std.Acos(c)
        End Function

        Public Function ToArray() As Double()
            Return {X, Y, Z}
        End Function

        ''' <summary>
        ''' 线性插值：<c>a + t·(b − a)</c>。
        ''' </summary>
        Public Shared Function Lerp(a As Vector3d, b As Vector3d, t As Double) As Vector3d
            Return New Vector3d(a.X + (b.X - a.X) * t,
                                a.Y + (b.Y - a.Y) * t,
                                a.Z + (b.Z - a.Z) * t)
        End Function

        ''' <summary>
        ''' 点积：<c>a · b</c>。
        ''' </summary>
        Public Shared Function Dot(a As Vector3d, b As Vector3d) As Double
            Return a.X * b.X + a.Y * b.Y + a.Z * b.Z
        End Function

        ''' <summary>
        ''' 叉积：<c>a × b</c>。
        ''' </summary>
        Public Shared Function Cross(a As Vector3d, b As Vector3d) As Vector3d
            Return New Vector3d(a.Y * b.Z - a.Z * b.Y,
                                a.Z * b.X - a.X * b.Z,
                                a.X * b.Y - a.Y * b.X)
        End Function

        ''' <summary>
        ''' 两点距离。
        ''' </summary>
        Public Shared Function Distance(a As Vector3d, b As Vector3d) As Double
            Return (a - b).Length
        End Function

        ''' <summary>
        ''' 分量级最小值。
        ''' </summary>
        Public Shared Function Min(a As Vector3d, b As Vector3d) As Vector3d
            Return New Vector3d(std.Min(a.X, b.X), std.Min(a.Y, b.Y), std.Min(a.Z, b.Z))
        End Function

        ''' <summary>
        ''' 分量级最大值。
        ''' </summary>
        Public Shared Function Max(a As Vector3d, b As Vector3d) As Vector3d
            Return New Vector3d(std.Max(a.X, b.X), std.Max(a.Y, b.Y), std.Max(a.Z, b.Z))
        End Function

        Public Shared Operator +(a As Vector3d, b As Vector3d) As Vector3d
            Return New Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z)
        End Operator

        Public Shared Operator -(a As Vector3d, b As Vector3d) As Vector3d
            Return New Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z)
        End Operator

        Public Shared Operator -(a As Vector3d) As Vector3d
            Return New Vector3d(-a.X, -a.Y, -a.Z)
        End Operator

        Public Shared Operator *(a As Vector3d, scalar As Double) As Vector3d
            Return New Vector3d(a.X * scalar, a.Y * scalar, a.Z * scalar)
        End Operator

        Public Shared Operator *(scalar As Double, a As Vector3d) As Vector3d
            Return New Vector3d(a.X * scalar, a.Y * scalar, a.Z * scalar)
        End Operator

        Public Shared Operator /(a As Vector3d, scalar As Double) As Vector3d
            Return New Vector3d(a.X / scalar, a.Y / scalar, a.Z / scalar)
        End Operator

        Public Shared Operator =(a As Vector3d, b As Vector3d) As Boolean
            Return a.X = b.X AndAlso a.Y = b.Y AndAlso a.Z = b.Z
        End Operator

        Public Shared Operator <>(a As Vector3d, b As Vector3d) As Boolean
            Return Not (a = b)
        End Operator

        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj IsNot Vector3d Then Return False

            Return Me = DirectCast(obj, Vector3d)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return HashCode.Combine(X, Y, Z)
        End Function

        Public Overrides Function ToString() As String
            Return $"({X:G6}, {Y:G6}, {Z:G6})"
        End Function
    End Structure
End Namespace
