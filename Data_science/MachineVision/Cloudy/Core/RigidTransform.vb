Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 刚体变换 <c>v' = R·v + t</c>：3x3 旋转矩阵 + 平移向量。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这是整个库的“公共货币”：预处理的坐标统一、配准的输出、多站拼接的位姿、
    ''' 位姿图的节点与边、CAD 比对的对齐变换、抓取位姿的手眼转换，
    ''' 全部以这个类型传递。
    ''' </para>
    ''' <para>
    ''' <b>为什么不用 4x4 齐次矩阵作为主存储</b>：应用变换的内核
    ''' （<see cref="SimdVector3.Transform3Into(Double(), Double(), Double(), Double, Double, Double, Double, Double, Double, Double, Double, Double, Double, Double, Double, Double(), Double(), Double())"/>）
    ''' 需要的是 9 个标量 + 3 个平移量；4x4 矩阵每次都多读 4 个恒为 0/1 的元素，
    ''' 而且在乘法里引入无意义的项。4x4 只在需要与外部系统
    ''' （OpenGL/glTF/其他库）交换时通过 <see cref="ToMatrix4x4"/> / <see cref="FromMatrix4x4"/> 生成。
    ''' </para>
    ''' <para>
    ''' <b>不可变</b>：所有运算返回新实例。配准算法每轮都会产生新变换，
    ''' 原地修改会让“上一轮的估计”被下一次迭代悄悄覆盖，
    ''' 这是调试配准问题时最难定位的一类错误。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class RigidTransform

        ''' <summary>单位变换（不旋转、不平移）。</summary>
        Public Shared ReadOnly Identity As New RigidTransform(
            New Double()() {
                New Double() {1, 0, 0},
                New Double() {0, 1, 0},
                New Double() {0, 0, 1}
            },
            Vector3d.Zero)

        Private ReadOnly m_rotation As Double()()
        Private ReadOnly m_translation As Vector3d

        ''' <summary>
        ''' 由 3x3 旋转矩阵（行主序）与平移量构造。
        ''' </summary>
        Public Sub New(rotation As Double()(), translation As Vector3d)
            If rotation Is Nothing Then Throw New ArgumentNullException(NameOf(rotation))

            If rotation.Length < 3 OrElse rotation(0).Length < 3 OrElse
                rotation(1).Length < 3 OrElse rotation(2).Length < 3 Then
                Throw New ArgumentException("the rotation matrix should be 3x3!", NameOf(rotation))
            End If

            m_rotation = rotation
            m_translation = translation
        End Sub

        ''' <summary>
        ''' 只有旋转、没有平移。
        ''' </summary>
        Public Sub New(rotation As Double()())
            Me.New(rotation, Vector3d.Zero)
        End Sub

        ''' <summary>
        ''' 3x3 旋转矩阵（行主序）。
        ''' </summary>
        Public ReadOnly Property Rotation As Double()()
            Get
                Return m_rotation
            End Get
        End Property

        Public ReadOnly Property Translation As Vector3d
            Get
                Return m_translation
            End Get
        End Property

        ''' <summary>
        ''' 单轴旋转（无平移）。
        ''' </summary>
        Public Shared Function FromAxisAngle(axis As Vector3d, angle As Double) As RigidTransform
            Return New RigidTransform(Quaternion.FromAxisAngle(axis, angle).ToRotationMatrix(), Vector3d.Zero)
        End Function

        ''' <summary>
        ''' 单轴旋转 + 平移。
        ''' </summary>
        Public Shared Function FromAxisAngle(axis As Vector3d, angle As Double, translation As Vector3d) As RigidTransform
            Return New RigidTransform(Quaternion.FromAxisAngle(axis, angle).ToRotationMatrix(), translation)
        End Function

        ''' <summary>
        ''' 仅平移。
        ''' </summary>
        ''' <remarks>
        ''' 这里刻意重新分配旋转矩阵而不复用 <see cref="Identity"/> 的数组：
        ''' 变换对象本身不可变，但它的 <see cref="Rotation"/> 属性返回的是原始数组，
        ''' 调用方若就地改写（例如某些外部数值库要求原地分解）会污染共享的单位矩阵。
        ''' </remarks>
        Public Shared Function FromTranslation(t As Vector3d) As RigidTransform
            Dim r As Double()() = {
                New Double() {1, 0, 0},
                New Double() {0, 1, 0},
                New Double() {0, 0, 1}
            }

            Return New RigidTransform(r, t)
        End Function

        ''' <summary>
        ''' 由四元数构造（无平移）。
        ''' </summary>
        Public Shared Function FromQuaternion(q As Quaternion) As RigidTransform
            Return New RigidTransform(q.ToRotationMatrix(), Vector3d.Zero)
        End Function

        ''' <summary>
        ''' 由四元数与平移量构造。
        ''' </summary>
        Public Shared Function FromQuaternion(q As Quaternion, translation As Vector3d) As RigidTransform
            Return New RigidTransform(q.ToRotationMatrix(), translation)
        End Function

        ''' <summary>
        ''' 由 4x4 齐次矩阵构造（忽略最后一行，按刚体处理）。
        ''' </summary>
        Public Shared Function FromMatrix4x4(m As Double(,)) As RigidTransform
            If m Is Nothing Then Throw New ArgumentNullException(NameOf(m))
            If m.GetLength(0) < 4 OrElse m.GetLength(1) < 4 Then
                Throw New ArgumentException($"the matrix should be 4x4, but got {m.GetLength(0)}x{m.GetLength(1)}!", NameOf(m))
            End If

            Dim r As Double()() = {
                New Double() {m(0, 0), m(0, 1), m(0, 2)},
                New Double() {m(1, 0), m(1, 1), m(1, 2)},
                New Double() {m(2, 0), m(2, 1), m(2, 2)}
            }

            Return New RigidTransform(r, New Vector3d(m(0, 3), m(1, 3), m(2, 3)))
        End Function

        ''' <summary>
        ''' 转换为 4x4 齐次矩阵（行主序）。
        ''' </summary>
        Public Function ToMatrix4x4() As Double(,)
            Dim m As Double(,) = New Double(3, 3) {}

            For i As Integer = 0 To 2
                For j As Integer = 0 To 2
                    m(i, j) = m_rotation(i)(j)
                Next

                m(i, 3) = m_translation.ToArray()(i)
            Next

            m(3, 0) = 0
            m(3, 1) = 0
            m(3, 2) = 0
            m(3, 3) = 1
            Return m
        End Function

        ''' <summary>
        ''' 变换组合：先施加 <c>Me</c>，再施加 <paramref name="following"/>，
        ''' 即 <c>result = following ∘ Me</c>。
        ''' </summary>
        ''' <remarks>
        ''' 约定“左乘后施加”，与机器人学里 <c>T_world_base = T_world_cam · T_cam_base</c>
        ''' 的书写顺序一致：外参（相机→基座）在右，世界位姿在左。
        ''' 多站拼接时按“后一站相对于前一站”的顺序从左到右串接。
        ''' </remarks>
        Public Function Compose(following As RigidTransform) As RigidTransform
            If following Is Nothing Then Throw New ArgumentNullException(NameOf(following))

            Dim r As Double()() = {
                New Double() {0, 0, 0},
                New Double() {0, 0, 0},
                New Double() {0, 0, 0}
            }

            For i As Integer = 0 To 2
                For j As Integer = 0 To 2
                    r(i)(j) = following.m_rotation(i)(0) * m_rotation(0)(j) +
                        following.m_rotation(i)(1) * m_rotation(1)(j) +
                        following.m_rotation(i)(2) * m_rotation(2)(j)
                Next
            Next

            Dim moved As Vector3d = following.TransformPoint(m_translation)

            Return New RigidTransform(r, moved)
        End Function

        ''' <summary>
        ''' 逆变换：<c>R⁻¹ = Rᵀ</c>，<c>t⁻¹ = −Rᵀ·t</c>。
        ''' </summary>
        ''' <remarks>
        ''' 刚体矩阵的逆不需要通用求逆：旋转部分直接用转置，
        ''' 平移部分只做一次矩阵向量乘。这也是所有点云算法里最常用的“回代”操作
        ''' （把目标坐标系下的结果变换回源坐标系，或者把配准改进量套回原初值）。
        ''' </remarks>
        Public Function Inverse() As RigidTransform
            Dim r As Double()() = {
                New Double() {m_rotation(0)(0), m_rotation(1)(0), m_rotation(2)(0)},
                New Double() {m_rotation(0)(1), m_rotation(1)(1), m_rotation(2)(1)},
                New Double() {m_rotation(0)(2), m_rotation(1)(2), m_rotation(2)(2)}
            }

            Dim t As New Vector3d(
                -(r(0)(0) * m_translation.X + r(0)(1) * m_translation.Y + r(0)(2) * m_translation.Z),
                -(r(1)(0) * m_translation.X + r(1)(1) * m_translation.Y + r(1)(2) * m_translation.Z),
                -(r(2)(0) * m_translation.X + r(2)(1) * m_translation.Y + r(2)(2) * m_translation.Z))

            Return New RigidTransform(r, t)
        End Function

        ''' <summary>
        ''' 变换单个点。
        ''' </summary>
        Public Function TransformPoint(p As Vector3d) As Vector3d
            Return New Vector3d(
                m_rotation(0)(0) * p.X + m_rotation(0)(1) * p.Y + m_rotation(0)(2) * p.Z + m_translation.X,
                m_rotation(1)(0) * p.X + m_rotation(1)(1) * p.Y + m_rotation(1)(2) * p.Z + m_translation.Y,
                m_rotation(2)(0) * p.X + m_rotation(2)(1) * p.Y + m_rotation(2)(2) * p.Z + m_translation.Z)
        End Function

        ''' <summary>
        ''' 只施加旋转（方向/法向变换，忽略平移）。
        ''' </summary>
        Public Function TransformDirection(v As Vector3d) As Vector3d
            Return New Vector3d(
                m_rotation(0)(0) * v.X + m_rotation(0)(1) * v.Y + m_rotation(0)(2) * v.Z,
                m_rotation(1)(0) * v.X + m_rotation(1)(1) * v.Y + m_rotation(1)(2) * v.Z,
                m_rotation(2)(0) * v.X + m_rotation(2)(1) * v.Y + m_rotation(2)(2) * v.Z)
        End Function

        ''' <summary>
        ''' 批量变换点集。
        ''' </summary>
        Public Function TransformPoints(points As Vector3d()) As Vector3d()
            If points Is Nothing Then Throw New ArgumentNullException(NameOf(points))

            Dim out(points.Length - 1) As Vector3d

            For i As Integer = 0 To points.Length - 1
                out(i) = TransformPoint(points(i))
            Next

            Return out
        End Function

        ''' <summary>
        ''' 就地变换一条 SoA 点云（走 SIMD 内核）。
        ''' </summary>
        Public Sub ApplyInPlace(x As Double(), y As Double(), z As Double())
            If x Is Nothing OrElse y Is Nothing OrElse z Is Nothing Then
                Throw New ArgumentNullException(NameOf(x), "the coordinate arrays can not be NULL!")
            End If

            Call SimdVector3.Transform3InPlace(x, y, z,
                m_rotation(0)(0), m_rotation(0)(1), m_rotation(0)(2),
                m_rotation(1)(0), m_rotation(1)(1), m_rotation(1)(2),
                m_rotation(2)(0), m_rotation(2)(1), m_rotation(2)(2),
                m_translation.X, m_translation.Y, m_translation.Z)
        End Sub

        ''' <summary>
        ''' 就地变换点云（坐标、法向一并变换；法向只做旋转）。
        ''' </summary>
        ''' <remarks>
        ''' 法向必须只施加旋转部分：平移作用于位置，方向向量在平移下不变。
        ''' 对法向施加平移会得到长度不为 1 的“伪法向”，后续所有夹角计算都会失真。
        ''' </remarks>
        Public Sub ApplyInPlace(cloud As PointCloudBuffer)
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Call ApplyInPlace(cloud.X, cloud.Y, cloud.Z)

            If cloud.HasNormals Then
                Call SimdVector3.Transform3InPlace(cloud.Nx, cloud.Ny, cloud.Nz,
                    m_rotation(0)(0), m_rotation(0)(1), m_rotation(0)(2),
                    m_rotation(1)(0), m_rotation(1)(1), m_rotation(1)(2),
                    m_rotation(2)(0), m_rotation(2)(1), m_rotation(2)(2),
                    0, 0, 0)
            End If
        End Sub

        ''' <summary>
        ''' 返回变换后的点云副本（原云不变）。
        ''' </summary>
        Public Function ApplyToCloud(cloud As PointCloudBuffer) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim copy As PointCloudBuffer = cloud.Clone()

            Call ApplyInPlace(copy)

            Return copy
        End Function

        ''' <summary>
        ''' 旋转矩阵的行列式（刚体应为 +1；接近 −1 表示发生了镜像翻转）。
        ''' </summary>
        Public ReadOnly Property Determinant As Double
            Get
                Return m_rotation(0)(0) * (m_rotation(1)(1) * m_rotation(2)(2) - m_rotation(1)(2) * m_rotation(2)(1)) -
                    m_rotation(0)(1) * (m_rotation(1)(0) * m_rotation(2)(2) - m_rotation(1)(2) * m_rotation(2)(0)) +
                    m_rotation(0)(2) * (m_rotation(1)(0) * m_rotation(2)(1) - m_rotation(1)(1) * m_rotation(2)(0))
            End Get
        End Property

        ''' <summary>
        ''' 与正交矩阵的最大偏差 <c>MAX|Rᵀ·R − I|</c>。
        ''' </summary>
        ''' <remarks>
        ''' 配准算法迭代数十轮之后，旋转矩阵的正交性会因浮点误差缓慢退化；
        ''' 这个值用于在结果对象里做一次“健康检查”，
        ''' 超过 1e-6 通常意味着迭代中出现了除零或非刚体更新。
        ''' </remarks>
        Public ReadOnly Property OrthogonalityError As Double
            Get
                Dim worst As Double = 0.0

                For i As Integer = 0 To 2
                    For j As Integer = 0 To 2
                        Dim dot As Double = 0.0

                        For k As Integer = 0 To 2
                            dot += m_rotation(k)(i) * m_rotation(k)(j)
                        Next

                        Dim expected As Double = If(i = j, 1.0, 0.0)
                        Dim err As Double = std.Abs(dot - expected)

                        If err > worst Then worst = err
                    Next
                Next

                Return worst
            End Get
        End Property

        ''' <summary>
        ''' 是否为一个合法的刚体变换（正交且行列式为 +1）。
        ''' </summary>
        Public Function IsRigid(Optional tolerance As Double = 1.0E-6) As Boolean
            Return OrthogonalityError <= tolerance AndAlso std.Abs(Determinant - 1.0) <= tolerance
        End Function

        ''' <summary>
        ''' 旋转角（弧度）：衡量两个位姿之间的角度差异。
        ''' </summary>
        Public ReadOnly Property RotationAngle As Double
            Get
                Dim trace As Double = m_rotation(0)(0) + m_rotation(1)(1) + m_rotation(2)(2)
                Dim c As Double = (trace - 1.0) * 0.5

                If c > 1.0 Then c = 1.0
                If c < -1.0 Then c = -1.0

                Return std.Acos(c)
            End Get
        End Property

        ''' <summary>
        ''' 与另一个变换的差异度量：旋转角（弧度）与平移距离。
        ''' </summary>
        Public Function DistanceTo(other As RigidTransform) As (rotation As Double, translation As Double)
            If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))

            Dim diff As RigidTransform = Me.Inverse().Compose(other)

            Return (diff.RotationAngle, diff.Translation.Length)
        End Function

        Public Overrides Function ToString() As String
            Return $"R=[[{m_rotation(0)(0):G4},{m_rotation(0)(1):G4},{m_rotation(0)(2):G4}]," &
                $"[{m_rotation(1)(0):G4},{m_rotation(1)(1):G4},{m_rotation(1)(2):G4}]," &
                $"[{m_rotation(2)(0):G4},{m_rotation(2)(1):G4},{m_rotation(2)(2):G4}]], t={m_translation}"
        End Function
    End Class
End Namespace
