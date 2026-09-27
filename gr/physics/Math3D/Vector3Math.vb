Imports System.Math
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math3D

    ''' <summary>
    ''' 3D 向量数学辅助模块，镜像 2D 的 <see cref="Vector2Math"/>：
    ''' 点积、叉积、长度、归一化、距离、线性插值与绕任意轴旋转。
    ''' </summary>
    ''' <remarks>
    ''' 3D 向量类型是 <see cref="Vector3"/>（与 3D 流体引擎共用的同一类型），
    ''' 因此本模块只提供静态函数，不重新定义向量类型。
    ''' </remarks>
    Public Module Vector3Math

        ''' <summary>分量取绝对值。</summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ComponentAbs(v As Vector3) As Vector3
            Return New Vector3(std.Abs(v.x), std.Abs(v.y), std.Abs(v.z))
        End Function

        ''' <summary>三维点积 a·b。</summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Dot(a As Vector3, b As Vector3) As Double
            Return a.x * b.x + a.y * b.y + a.z * b.z
        End Function

        ''' <summary>三维叉积 a×b（右手系）。</summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Cross(a As Vector3, b As Vector3) As Vector3
            Return New Vector3(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x
            )
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LengthSquared(v As Vector3) As Double
            Return v.x * v.x + v.y * v.y + v.z * v.z
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Length(v As Vector3) As Double
            Return std.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z)
        End Function

        ''' <summary>归一化；零长度向量返回零向量，避免除零。</summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Normalize(v As Vector3) As Vector3
            Dim len As Double = Length(v)
            If len < 1.0E-12 Then Return Vector3.Zero
            Return v / len
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Distance(a As Vector3, b As Vector3) As Double
            Return Length(a - b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DistanceSquared(a As Vector3, b As Vector3) As Double
            Return LengthSquared(a - b)
        End Function

        ''' <summary>线性插值。</summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Lerp(a As Vector3, b As Vector3, t As Double) As Vector3
            Return a + (b - a) * t
        End Function

        ''' <summary>
        ''' 把 <paramref name="v"/> 投影到以 <paramref name="unitAxis"/> 为法向的平面上（去掉轴向分量）。
        ''' </summary>
        Public Function ProjectOnPlane(v As Vector3, unitAxis As Vector3) As Vector3
            Return v - unitAxis * Dot(v, unitAxis)
        End Function

        ''' <summary>
        ''' 罗德里格斯旋转公式：把 <paramref name="v"/> 绕单位轴 <paramref name="axis"/> 旋转 <paramref name="angle"/> 弧度。
        ''' </summary>
        Public Function RotateAroundAxis(v As Vector3, axis As Vector3, angle As Double) As Vector3
            Dim u As Vector3 = Normalize(axis)
            Dim c As Double = std.Cos(angle)
            Dim s As Double = std.Sin(angle)

            Return v * c + Cross(u, v) * s + u * (Dot(u, v) * (1.0 - c))
        End Function

        ''' <summary>
        ''' 由三个分量构造向量（便于与 <see cref="Matrix3x3"/> 组合使用）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Vec(x As Double, y As Double, z As Double) As Vector3
            Return New Vector3(x, y, z)
        End Function

        ''' <summary>分量取小。</summary>
        Public Function ComponentMin(a As Vector3, b As Vector3) As Vector3
            Return New Vector3(std.Min(a.x, b.x), std.Min(a.y, b.y), std.Min(a.z, b.z))
        End Function

        ''' <summary>分量取大。</summary>
        Public Function ComponentMax(a As Vector3, b As Vector3) As Vector3
            Return New Vector3(std.Max(a.x, b.x), std.Max(a.y, b.y), std.Max(a.z, b.z))
        End Function

        ''' <summary>
        ''' 求与 <paramref name="v"/> 正交的任意一个单位向量（用于构造接触切线的正交基）。
        ''' </summary>
        Public Function OrthogonalOf(v As Vector3) As Vector3
            Dim n As Vector3 = Normalize(v)
            Dim helper As Vector3

            If std.Abs(n.x) < 0.9 Then
                helper = New Vector3(1, 0, 0)
            Else
                helper = New Vector3(0, 1, 0)
            End If

            Return Normalize(Cross(n, helper))
        End Function
    End Module
End Namespace
