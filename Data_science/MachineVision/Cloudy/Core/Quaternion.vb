Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 单位四元数：三轴姿态的最小无奇异表示。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 在点云流水线里四元数主要有三个用途：
    ''' </para>
    ''' <list type="bullet">
    ''' <item>
    ''' <description>
    ''' <b>运动畸变补偿</b>：一帧内的 IMU 姿态是一串离散采样，
    ''' 逐点去畸变需要对相邻两帧姿态做球面线性插值
    ''' （<see cref="Slerp(Quaternion, Quaternion, Double)"/>）。
    ''' 用欧拉角插值会在万向锁附近产生跳变，用旋转矩阵插值则破坏正交性。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>刚体变换估计</b>：Horn 的闭式解把“求最优旋转”转化为求 4x4 对称阵的
    ''' 最大特征向量，得到的正是四元数（见 <c>Linalg.Kabsch</c>）。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>位姿图增量</b>：SE(3) 的旋转增量用四元数做局部参数化比直接对
    ''' 旋转矩阵加噪声更稳定（旋转矩阵的正交性只在切线空间内保持）。
    ''' </description>
    ''' </item>
    ''' </list>
    ''' <para>
    ''' 约定：<c>q = w + x·i + y·j + z·k</c>，右手坐标系，
    ''' 旋转作用于<b>列向量</b>（<c>v' = R·v</c>），
    ''' 与 <see cref="RigidTransform"/> 的 3x3 矩阵约定一致。
    ''' </para>
    ''' </remarks>
    Public Structure Quaternion

        ''' <summary>单位四元数（不旋转）。</summary>
        Public Shared ReadOnly Identity As New Quaternion(1, 0, 0, 0)

        Public ReadOnly W As Double
        Public ReadOnly X As Double
        Public ReadOnly Y As Double
        Public ReadOnly Z As Double

        Public Sub New(w As Double, x As Double, y As Double, z As Double)
            Me.W = w
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        ''' <summary>
        ''' 由旋转轴与旋转角构造（轴会被归一化；零轴返回单位四元数）。
        ''' </summary>
        ''' <param name="axis">旋转轴（右手定则）</param>
        ''' <param name="angle">旋转角（弧度）</param>
        Public Shared Function FromAxisAngle(axis As Vector3d, angle As Double) As Quaternion
            Dim n As Vector3d = axis.Normalize()

            If n.IsZero() Then Return Identity

            Dim half As Double = angle * 0.5
            Dim s As Double = std.Sin(half)

            Return New Quaternion(std.Cos(half), n.X * s, n.Y * s, n.Z * s)
        End Function

        ''' <summary>
        ''' 由 3x3 旋转矩阵构造（Shepperd 方法：选择最大对角元分支以保证数值稳定）。
        ''' </summary>
        ''' <param name="r">3x3 行主序旋转矩阵</param>
        ''' <remarks>
        ''' 直接对 <c>tr(R)</c> 分支求 <c>w</c> 在旋转角接近 π 时会出现
        ''' <c>arccos</c> 的数值病态（<c>w → 0</c>，此时 <c>x/y/z</c> 的符号完全不可解析）。
        ''' Shepperd 方法按 <c>w, x, y, z</c> 中最大者分支，保证除数始终远离 0。
        ''' </remarks>
        Public Shared Function FromRotationMatrix(r As Double()()) As Quaternion
            If r Is Nothing OrElse r.Length < 3 Then
                Throw New ArgumentException("the rotation matrix should be at least 3x3!", NameOf(r))
            End If

            Dim m00 As Double = r(0)(0), m01 As Double = r(0)(1), m02 As Double = r(0)(2)
            Dim m10 As Double = r(1)(0), m11 As Double = r(1)(1), m12 As Double = r(1)(2)
            Dim m20 As Double = r(2)(0), m21 As Double = r(2)(1), m22 As Double = r(2)(2)

            Dim trace As Double = m00 + m11 + m22

            If trace > 0.0 Then
                Dim s As Double = std.Sqrt(trace + 1.0) * 2.0

                Return New Quaternion(0.25 * s,
                                      (m21 - m12) / s,
                                      (m02 - m20) / s,
                                      (m10 - m01) / s).Normalize()
            End If

            If m00 > m11 AndAlso m00 > m22 Then
                Dim s As Double = std.Sqrt(1.0 + m00 - m11 - m22) * 2.0

                Return New Quaternion((m21 - m12) / s,
                                      0.25 * s,
                                      (m01 + m10) / s,
                                      (m02 + m20) / s).Normalize()
            End If

            If m11 > m22 Then
                Dim s As Double = std.Sqrt(1.0 + m11 - m00 - m22) * 2.0

                Return New Quaternion((m02 - m20) / s,
                                      (m01 + m10) / s,
                                      0.25 * s,
                                      (m12 + m21) / s).Normalize()
            End If

            Dim s2 As Double = std.Sqrt(1.0 + m22 - m00 - m11) * 2.0

            Return New Quaternion((m10 - m01) / s2,
                                  (m02 + m20) / s2,
                                  (m12 + m21) / s2,
                                  0.25 * s2).Normalize()
        End Function

        ''' <summary>
        ''' 转换为 3x3 行主序旋转矩阵。
        ''' </summary>
        Public Function ToRotationMatrix() As Double()()
            Dim q As Quaternion = Normalize()
            Dim w As Double = q.W, x As Double = q.X, y As Double = q.Y, z As Double = q.Z
            Dim xx As Double = x * x, yy As Double = y * y, zz As Double = z * z
            Dim xy As Double = x * y, xz As Double = x * z, yz As Double = y * z
            Dim wx As Double = w * x, wy As Double = w * y, wz As Double = w * z

            Return {
                New Double() {1 - 2 * (yy + zz), 2 * (xy - wz), 2 * (xz + wy)},
                New Double() {2 * (xy + wz), 1 - 2 * (xx + zz), 2 * (yz - wx)},
                New Double() {2 * (xz - wy), 2 * (yz + wx), 1 - 2 * (xx + yy)}
            }
        End Function

        ''' <summary>
        ''' 范数（理论上恒为 1，数值累积后会漂移）。
        ''' </summary>
        Public ReadOnly Property Norm As Double
            Get
                Return std.Sqrt(W * W + X * X + Y * Y + Z * Z)
            End Get
        End Property

        ''' <summary>
        ''' 归一化；零四元数返回单位四元数。
        ''' </summary>
        ''' <remarks>
        ''' 反复相乘会累积浮点误差；不归一化的四元数转换回矩阵时不再是正交阵，
        ''' 会把点云“剪切”变形，因此每次串接后都应归一化。
        ''' </remarks>
        Public Function Normalize() As Quaternion
            Dim n As Double = Norm

            If n <= 0.0 Then Return Identity

            Return New Quaternion(W / n, X / n, Y / n, Z / n)
        End Function

        ''' <summary>
        ''' 共轭（= 单位四元数的逆，等价于反向旋转）。
        ''' </summary>
        Public Function Conjugate() As Quaternion
            Return New Quaternion(W, -X, -Y, -Z)
        End Function

        ''' <summary>
        ''' 逆四元数：<c>conj(q) / |q|²</c>。
        ''' </summary>
        Public Function Inverse() As Quaternion
            Dim n2 As Double = W * W + X * X + Y * Y + Z * Z

            If n2 <= 0.0 Then Return Identity

            Return New Quaternion(W / n2, -X / n2, -Y / n2, -Z / n2)
        End Function

        ''' <summary>
        ''' 四元数乘法（Hamilton 积）：<c>q = a · b</c> 表示先施加 <c>b</c> 再施加 <c>a</c>。
        ''' </summary>
        Public Shared Operator *(a As Quaternion, b As Quaternion) As Quaternion
            Return New Quaternion(
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
                a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W)
        End Operator

        ''' <summary>
        ''' 四元数点积（用于判断两个姿态是否落在同一个半球）。
        ''' </summary>
        Public Shared Function Dot(a As Quaternion, b As Quaternion) As Double
            Return a.W * b.W + a.X * b.X + a.Y * b.Y + a.Z * b.Z
        End Function

        ''' <summary>
        ''' 球面线性插值。
        ''' </summary>
        ''' <param name="a">起始姿态</param>
        ''' <param name="b">结束姿态</param>
        ''' <param name="t">插值参数 <c>[0, 1]</c></param>
        ''' <remarks>
        ''' <para>
        ''' 四元数 <c>q</c> 与 <c>−q</c> 表示同一个旋转，但线性插值时
        ''' 走的是完全不同的弧长；因此插值前必须先把 <paramref name="b"/> 翻到与
        ''' <paramref name="a"/> 同一半球（<see cref="Dot(Quaternion, Quaternion)"/> 为负时取负）。
        ''' 这一条如果漏掉，运动畸变补偿会在某些姿态下把点云“翻转”过来。
        ''' </para>
        ''' <para>
        ''' 两个姿态几乎重合时（<c>dot → 1</c>）改用归一化线性插值(nlerp)，
        ''' 避免 <c>sin(θ) → 0</c> 造成的除零放大。
        ''' </para>
        ''' </remarks>
        Public Shared Function Slerp(a As Quaternion, b As Quaternion, t As Double) As Quaternion
            Dim bb As Quaternion = b

            If t <= 0.0 Then Return a.Normalize()
            If t >= 1.0 Then Return b.Normalize()

            Dim d As Double = Dot(a, b)

            If d < 0.0 Then
                bb = New Quaternion(-b.W, -b.X, -b.Y, -b.Z)
                d = -d
            End If

            If d > 0.9995 Then
                Return New Quaternion(a.W + (bb.W - a.W) * t,
                                      a.X + (bb.X - a.X) * t,
                                      a.Y + (bb.Y - a.Y) * t,
                                      a.Z + (bb.Z - a.Z) * t).Normalize()
            End If

            Dim theta As Double = std.Acos(d)
            Dim sinTheta As Double = std.Sin(theta)
            Dim wa As Double = std.Sin((1.0 - t) * theta) / sinTheta
            Dim wb As Double = std.Sin(t * theta) / sinTheta

            Return New Quaternion(a.W * wa + bb.W * wb,
                                  a.X * wa + bb.X * wb,
                                  a.Y * wa + bb.Y * wb,
                                  a.Z * wa + bb.Z * wb).Normalize()
        End Function

        ''' <summary>
        ''' 用四元数旋转一个向量：<c>q · v · q⁻¹</c>（展开为不含中间四元数的乘加形式）。
        ''' </summary>
        Public Function Rotate(v As Vector3d) As Vector3d
            Dim q As Quaternion = Normalize()
            Dim u As New Vector3d(q.X, q.Y, q.Z)
            Dim t As Vector3d = Vector3d.Cross(u, v) * 2.0

            Return v + t * q.W + Vector3d.Cross(u, t)
        End Function

        ''' <summary>
        ''' 旋转角（弧度，<c>[0, 2π)</c>）。
        ''' </summary>
        Public ReadOnly Property Angle As Double
            Get
                Dim w As Double = std.Abs(W)

                If w > 1.0 Then w = 1.0

                Return 2.0 * std.Acos(w)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"w={W:G6}, ({X:G6}, {Y:G6}, {Z:G6})"
        End Function
    End Structure
End Namespace
