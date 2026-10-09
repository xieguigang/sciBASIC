Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math3D

    ''' <summary>
    ''' 单位四元数 <c>q = w + xi + yj + zk</c>，用于表示 3D 刚体的姿态。
    ''' </summary>
    ''' <remarks>
    ''' 旋转约定：局部坐标系的基向量为 X=right、Y=up、Z=forward（右手系，X×Y=Z）。
    ''' <see cref="Rotate(Vector3)"/> 把<strong>局部</strong>向量变换到<strong>世界</strong>坐标；
    ''' <see cref="ToMatrix3x3"/> 返回行向量约定矩阵，满足 <c>v_world = v · M</c>。
    ''' </remarks>
    Public Structure Quaternion

        Public w As Double
        Public x As Double
        Public y As Double
        Public z As Double

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(w As Double, x As Double, y As Double, z As Double)
            Me.w = w
            Me.x = x
            Me.y = y
            Me.z = z
        End Sub

        ''' <summary>单位四元数（无旋转）。</summary>
        Public Shared ReadOnly Property Identity As Quaternion
            Get
                Return New Quaternion(1, 0, 0, 0)
            End Get
        End Property

        ''' <summary>四元数长度的平方。</summary>
        Public ReadOnly Property LengthSquared As Double
            Get
                Return w * w + x * x + y * y + z * z
            End Get
        End Property

        ''' <summary>
        ''' 归一化；零长度四元数退化为 <see cref="Identity"/>，避免姿态积分发散。
        ''' </summary>
        Public Function Normalize() As Quaternion
            Dim len As Double = std.Sqrt(LengthSquared)

            If len < 1.0E-12 Then
                Return Quaternion.Identity
            End If

            Return New Quaternion(w / len, x / len, y / len, z / len)
        End Function

        ''' <summary>共轭（单位四元数下即逆）。</summary>
        Public Function Conjugate() As Quaternion
            Return New Quaternion(w, -x, -y, -z)
        End Function

        ''' <summary>逆四元数。</summary>
        Public Function Inverse() As Quaternion
            Dim inv As Double = 1.0 / std.Max(LengthSquared, 1.0E-12)
            Return New Quaternion(w * inv, -x * inv, -y * inv, -z * inv)
        End Function

        ''' <summary>
        ''' 把局部向量旋转到世界坐标：<c>v' = v + 2·q.xyz × (q.xyz × v + w·v)</c>。
        ''' </summary>
        Public Function Rotate(v As Vector3) As Vector3
            Dim tx As Double = y * v.z - z * v.y + w * v.x
            Dim ty As Double = z * v.x - x * v.z + w * v.y
            Dim tz As Double = x * v.y - y * v.x + w * v.z

            Return New Vector3(
                v.x + 2.0 * (y * tz - z * ty),
                v.y + 2.0 * (z * tx - x * tz),
                v.z + 2.0 * (x * ty - y * tx))
        End Function

        ''' <summary>
        ''' 把世界向量旋转回局部坐标（<see cref="Rotate(Vector3)"/> 的逆变换）。
        ''' </summary>
        Public Function Unrotate(v As Vector3) As Vector3
            Return Conjugate().Rotate(v)
        End Function

        ''' <summary>
        ''' 四元数乘法（Hamilton 积）。<c>Compose(a, b)</c> 表示先施加 b 再施加 a：
        ''' <c>Rotate(a * b, v) = Rotate(a, Rotate(b, v))</c>。
        ''' </summary>
        Public Shared Function Compose(a As Quaternion, b As Quaternion) As Quaternion
            Return New Quaternion(
                a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z,
                a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
                a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
                a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w)
        End Function

        ''' <summary>绕单位轴 <paramref name="axis"/> 旋转 <paramref name="angle"/> 弧度。</summary>
        Public Shared Function FromAxisAngle(axis As Vector3, angle As Double) As Quaternion
            Dim u As Vector3 = Vector3Math.Normalize(axis)
            Dim half As Double = angle * 0.5
            Dim s As Double = std.Sin(half)

            Return New Quaternion(std.Cos(half), u.x * s, u.y * s, u.z * s)
        End Function

        ''' <summary>
        ''' 由前向量与上向量构造姿态（局部 Z=forward，局部 Y≈up）。
        ''' 退化输入（前向与上向共线）时自动选择一个正交上向。
        ''' </summary>
        Public Shared Function FromForwardUp(forward As Vector3, up As Vector3) As Quaternion
            Dim f As Vector3 = Vector3Math.Normalize(forward)
            Dim u As Vector3 = up

            If Vector3Math.Length(Vector3Math.Cross(u, f)) < 1.0E-6 Then
                u = New Vector3(0, 0, 1)
                If Vector3Math.Length(Vector3Math.Cross(u, f)) < 1.0E-6 Then
                    u = New Vector3(1, 0, 0)
                End If
            End If

            ' right = up × forward, then re-orthogonalize up = forward × right
            Dim r As Vector3 = Vector3Math.Normalize(Vector3Math.Cross(u, f))
            Dim t As Vector3 = Vector3Math.Cross(f, r)

            ' columns of the rotation matrix are (right, up, forward)
            Dim rot As New Matrix3x3(
                r.x, t.x, f.x,
                r.y, t.y, f.y,
                r.z, t.z, f.z)

            Return FromRotationMatrix(rot)
        End Function

        ''' <summary>
        ''' 由 3x3 旋转矩阵构造四元数。输入为列向量约定矩阵（列为局部基向量在世界系下的坐标）。
        ''' </summary>
        Public Shared Function FromRotationMatrix(m As Matrix3x3) As Quaternion
            Dim tr As Double = m.m11 + m.m22 + m.m33
            Dim q As Quaternion

            If tr > 0.0 Then
                Dim s As Double = std.Sqrt(tr + 1.0) * 2.0

                q = New Quaternion(
                    0.25 * s,
                    (m.m23 - m.m32) / s,
                    (m.m31 - m.m13) / s,
                    (m.m12 - m.m21) / s)
            ElseIf m.m11 > m.m22 AndAlso m.m11 > m.m33 Then
                Dim s As Double = std.Sqrt(1.0 + m.m11 - m.m22 - m.m33) * 2.0

                q = New Quaternion(
                    (m.m23 - m.m32) / s,
                    0.25 * s,
                    (m.m21 + m.m12) / s,
                    (m.m31 + m.m13) / s)
            ElseIf m.m22 > m.m33 Then
                Dim s As Double = std.Sqrt(1.0 + m.m22 - m.m11 - m.m33) * 2.0

                q = New Quaternion(
                    (m.m31 - m.m13) / s,
                    (m.m21 + m.m12) / s,
                    0.25 * s,
                    (m.m32 + m.m23) / s)
            Else
                Dim s As Double = std.Sqrt(1.0 + m.m33 - m.m11 - m.m22) * 2.0

                q = New Quaternion(
                    (m.m12 - m.m21) / s,
                    (m.m31 + m.m13) / s,
                    (m.m32 + m.m23) / s,
                    0.25 * s)
            End If

            Return q.Normalize()
        End Function

        ''' <summary>
        ''' 行向量约定的旋转矩阵，满足 <c>v_world = v_local · M</c>。
        ''' </summary>
        Public Function ToMatrix3x3() As Matrix3x3
            Dim xx As Double = x * x, yy As Double = y * y, zz As Double = z * z
            Dim xy As Double = x * y, xz As Double = x * z, yz As Double = y * z
            Dim wx As Double = w * x, wy As Double = w * y, wz As Double = w * z

            Return New Matrix3x3(
                1.0 - 2.0 * (yy + zz), 2.0 * (xy + wz), 2.0 * (xz - wy),
                2.0 * (xy - wz), 1.0 - 2.0 * (xx + zz), 2.0 * (yz + wx),
                2.0 * (xz + wy), 2.0 * (yz - wx), 1.0 - 2.0 * (xx + yy))
        End Function

        ''' <summary>
        ''' 分解为轴角（最短路径）。<paramref name="angle"/> 落在 [0, π]；
        ''' 零旋转时轴退化为 (0,1,0)。
        ''' </summary>
        Public Sub ToAxisAngle(ByRef axis As Vector3, ByRef angle As Double)
            Dim q As Quaternion = Me

            If q.w < 0.0 Then
                q = New Quaternion(-q.w, -q.x, -q.y, -q.z)
            End If

            q = q.Normalize()

            Dim sinHalf As Double = std.Sqrt(std.Max(0.0, 1.0 - q.w * q.w))

            If sinHalf < 1.0E-8 Then
                axis = New Vector3(0, 1, 0)
                angle = 0.0
                Return
            End If

            angle = 2.0 * std.Atan2(sinHalf, q.w)
            axis = New Vector3(q.x / sinHalf, q.y / sinHalf, q.z / sinHalf)
        End Sub

        ''' <summary>两个姿态之间的最短夹角（弧度）。</summary>
        Public Shared Function AngleBetween(a As Quaternion, b As Quaternion) As Double
            Dim d As Double = std.Abs(a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z)
            Return 2.0 * std.Acos(std.Min(1.0, d))
        End Function

        ''' <summary>球面线性插值。</summary>
        Public Shared Function Slerp(a As Quaternion, b As Quaternion, t As Double) As Quaternion
            Dim cosHalf As Double = a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z
            Dim target As Quaternion = b

            If cosHalf < 0.0 Then
                target = New Quaternion(-b.w, -b.x, -b.y, -b.z)
                cosHalf = -cosHalf
            End If

            If cosHalf > 0.9995 Then
                Return New Quaternion(
                    a.w + (target.w - a.w) * t,
                    a.x + (target.x - a.x) * t,
                    a.y + (target.y - a.y) * t,
                    a.z + (target.z - a.z) * t).Normalize()
            End If

            Dim half As Double = std.Acos(cosHalf)
            Dim sinHalf As Double = std.Sin(half)
            Dim wa As Double = std.Sin((1.0 - t) * half) / sinHalf
            Dim wb As Double = std.Sin(t * half) / sinHalf

            Return New Quaternion(
                a.w * wa + target.w * wb,
                a.x * wa + target.x * wb,
                a.y * wa + target.y * wb,
                a.z * wa + target.z * wb).Normalize()
        End Function

        ''' <summary>
        ''' 用世界系角速度积分姿态：<c>dq/dt = 0.5 · (0,ω) ⊗ q</c>，积分后重新归一化。
        ''' </summary>
        Public Function Integrate(angularVelocity As Vector3, dt As Double) As Quaternion
            Dim spin As New Quaternion(0, angularVelocity.x, angularVelocity.y, angularVelocity.z)
            Dim delta As Quaternion = Compose(spin, Me)
            Dim half As Double = 0.5 * dt

            Return New Quaternion(
                w + delta.w * half,
                x + delta.x * half,
                y + delta.y * half,
                z + delta.z * half).Normalize()
        End Function

        Public Overrides Function ToString() As String
            Return $"[{w:F3} {x:F3}i {y:F3}j {z:F3}k]"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(a As Quaternion, b As Quaternion) As Quaternion
            Return Compose(a, b)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(q As Quaternion, v As Vector3) As Vector3
            Return q.Rotate(v)
        End Operator
    End Structure
End Namespace
