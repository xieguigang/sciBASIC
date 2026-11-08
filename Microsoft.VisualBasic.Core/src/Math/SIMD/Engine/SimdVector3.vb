Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' SoA(structure of arrays)布局的三维点云批量内核。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 三维点云在几何算法里通常以 <c>X(i), Y(i), Z(i)</c> 三条独立数组存放
    ''' （SoA），而不是 <c>Point3D</c> 结构体数组（AoS）。原因很直接：
    ''' <see cref="System.Numerics.Vector(Of T)"/> 的批量装载
    ''' <c>New Vector(Of Double)(array, offset)</c> 只能作用在<b>连续内存</b>上，
    ''' 而 AoS 布局中相邻点的同一分量之间隔着 3 个 <see cref="Double"/>，
    ''' 属于跨步(strided)访问，向量化后不降反升。
    ''' </para>
    ''' <para>
    ''' <b>与 <see cref="SimdEngine"/> 的分工</b>：<see cref="SimdEngine"/> 是
    ''' “一条一维数组上的一次逐元素运算”，这里则是“三条数组组成一个三维向量场”，
    ''' 把三次独立的 <see cref="SimdEngine"/> 调用合并成一次内存往返，
    ''' 并且避免为每个中间结果分配临时数组。
    ''' </para>
    ''' <para>
    ''' <b>零分配契约</b>：所有 <c>*Into</c> / <c>InPlace</c> 结尾的方法都只写入调用方
    ''' 提供的目标数组，不产生任何托管堆分配；带返回值的方法只在被显式调用时分配
    ''' 结果（三元素锯齿数组 <c>Double()()</c>）。热路径请一律使用 <c>*Into</c> 版本。
    ''' </para>
    ''' <para>
    ''' <b>就地运算</b>：<c>*InPlace</c> 方法允许输入与输出是同一块内存，
    ''' 因此尾部补齐<b>不能</b>使用“末块与末尾重叠”的技巧（重叠区会被重复计算），
    ''' 剩余元素退化为标量循环。
    ''' </para>
    ''' <para>
    ''' <b>回退</b>：所有循环都会检查 <see cref="SIMDEnvironment.IsEnabled"/>，
    ''' 因此 <see cref="SIMDConfiguration.disable"/> 仍然是全局有效的逃生开关；
    ''' 数据长度小于一个向量宽度时自动走标量分支。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdVector3

        Private Sub New()
        End Sub

#Region "helpers"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CheckNull(v As Array, name As String)
            If v Is Nothing Then
                Throw New ArgumentNullException(name, "the input array can not be NULL!")
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CheckAgree(len As Integer, actual As Integer, name As String)
            If actual <> len Then
                Throw New ArgumentException($"the length of '{name}' ({actual}) not agree with the x array ({len})!")
            End If
        End Sub

        ''' <summary>
        ''' 校验三/六/九条数组长度一致并返回公共长度。
        ''' </summary>
        Private Shared Function CheckLengths(name As String, ParamArray arrays As Double()()) As Integer
            If arrays Is Nothing OrElse arrays.Length = 0 Then
                Throw New ArgumentException("no array was given!", name)
            End If

            Dim len As Integer = 0

            For i As Integer = 0 To arrays.Length - 1
                Call CheckNull(arrays(i), name)

                If i = 0 Then
                    len = arrays(i).Length
                Else
                    Call CheckAgree(len, arrays(i).Length, name)
                End If
            Next

            Return len
        End Function

        ''' <summary>
        ''' 逐元素二元内核（非就地，允许“末块与末尾重叠”覆盖尾部）。
        ''' </summary>
        Private Shared Sub BinaryInto(a As Double(), b As Double(), dst As Double(), len As Integer,
                                      isAdd As Boolean)
            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= count Then
                Dim last As Integer = len - count

                Do While i <= last
                    Dim va As New Vector(Of Double)(a, i)
                    Dim vb As New Vector(Of Double)(b, i)
                    Dim r As Vector(Of Double) = If(isAdd, Vector.Add(Of Double)(va, vb), Vector.Subtract(Of Double)(va, vb))

                    r.CopyTo(dst, i)
                    i += count
                Loop
                If i < len Then
                    Dim va As New Vector(Of Double)(a, last)
                    Dim vb As New Vector(Of Double)(b, last)
                    Dim r As Vector(Of Double) = If(isAdd, Vector.Add(Of Double)(va, vb), Vector.Subtract(Of Double)(va, vb))

                    r.CopyTo(dst, last)
                End If

                Return
            End If

            If isAdd Then
                Do While i < len
                    dst(i) = a(i) + b(i)
                    i += 1
                Loop
            Else
                Do While i < len
                    dst(i) = a(i) - b(i)
                    i += 1
                Loop
            End If
        End Sub

        ''' <summary>
        ''' 逐元素乘标量内核（非就地）。
        ''' </summary>
        Private Shared Sub ScaleInto(a As Double(), scalar As Double, dst As Double(), len As Integer)
            Dim count As Integer = Vector(Of Double).Count
            Dim splat As New Vector(Of Double)(scalar)
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= count Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Multiply(Of Double)(New Vector(Of Double)(a, i), splat).CopyTo(dst, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Multiply(Of Double)(New Vector(Of Double)(a, last), splat).CopyTo(dst, last)
                End If

                Return
            End If

            Do While i < len
                dst(i) = a(i) * scalar
                i += 1
            Loop
        End Sub

        ''' <summary>
        ''' 区间点积：4 路累加器打断浮点加法依赖链。
        ''' </summary>
        Private Shared Function DotRange(a As Double(), b As Double(), start As Integer, ends As Integer) As Double
            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = start
            Dim total As Double = 0

            If SIMDEnvironment.IsEnabled AndAlso ends - start >= step4 Then
                Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
                Dim last4 As Integer = ends - step4

                Do While i <= last4
                    acc0 = Vector.Add(Of Double)(acc0, Vector.Multiply(Of Double)(New Vector(Of Double)(a, i), New Vector(Of Double)(b, i)))
                    acc1 = Vector.Add(Of Double)(acc1, Vector.Multiply(Of Double)(New Vector(Of Double)(a, i + count), New Vector(Of Double)(b, i + count)))
                    acc2 = Vector.Add(Of Double)(acc2, Vector.Multiply(Of Double)(New Vector(Of Double)(a, i + count * 2), New Vector(Of Double)(b, i + count * 2)))
                    acc3 = Vector.Add(Of Double)(acc3, Vector.Multiply(Of Double)(New Vector(Of Double)(a, i + count * 3), New Vector(Of Double)(b, i + count * 3)))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1),
                                             Vector.Add(Of Double)(acc2, acc3))

                total = Vector.Dot(Of Double)(acc0, ones)
            End If

            Do While i < ends
                total += a(i) * b(i)
                i += 1
            Loop

            Return total
        End Function

        ''' <summary>
        ''' 区间平方距离：<c>SUM((a(i) - b(i)) ^ 2)</c>，单趟、零分配。
        ''' </summary>
        Private Shared Function DistanceSquaredRange(a As Double(), b As Double(), start As Integer, ends As Integer) As Double
            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = start
            Dim total As Double = 0

            If SIMDEnvironment.IsEnabled AndAlso ends - start >= step4 Then
                Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
                Dim last4 As Integer = ends - step4

                Do While i <= last4
                    Dim d0 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(a, i), New Vector(Of Double)(b, i))
                    Dim d1 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(a, i + count), New Vector(Of Double)(b, i + count))
                    Dim d2 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(a, i + count * 2), New Vector(Of Double)(b, i + count * 2))
                    Dim d3 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(a, i + count * 3), New Vector(Of Double)(b, i + count * 3))

                    acc0 = Vector.Add(Of Double)(acc0, Vector.Multiply(Of Double)(d0, d0))
                    acc1 = Vector.Add(Of Double)(acc1, Vector.Multiply(Of Double)(d1, d1))
                    acc2 = Vector.Add(Of Double)(acc2, Vector.Multiply(Of Double)(d2, d2))
                    acc3 = Vector.Add(Of Double)(acc3, Vector.Multiply(Of Double)(d3, d3))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1),
                                             Vector.Add(Of Double)(acc2, acc3))

                total = Vector.Dot(Of Double)(acc0, ones)
            End If

            Do While i < ends
                Dim d As Double = a(i) - b(i)

                total += d * d
                i += 1
            Loop

            Return total
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Pack(x As Double(), y As Double(), z As Double()) As Double()()
            Return {x, y, z}
        End Function

#End Region

#Region "add / subtract / scale"

        ''' <summary>
        ''' 逐点向量加法（写入目标数组）：<c>out(i) = a(i) + b(i)</c>。
        ''' </summary>
        Public Shared Sub Add3Into(x1 As Double(), y1 As Double(), z1 As Double(),
                                   x2 As Double(), y2 As Double(), z2 As Double(),
                                   outX As Double(), outY As Double(), outZ As Double())
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2, outX, outY, outZ)

            Call BinaryInto(x1, x2, outX, len, True)
            Call BinaryInto(y1, y2, outY, len, True)
            Call BinaryInto(z1, z2, outZ, len, True)
        End Sub

        ''' <summary>
        ''' 逐点向量加法：返回 <c>{X, Y, Z}</c> 三条结果数组（会分配）。
        ''' </summary>
        Public Shared Function Add3(x1 As Double(), y1 As Double(), z1 As Double(),
                                    x2 As Double(), y2 As Double(), z2 As Double()) As Double()()
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2)
            Dim outX As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim outY As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim outZ As Double() = SimdEngine.NewArray(Of Double)(len)

            Call Add3Into(x1, y1, z1, x2, y2, z2, outX, outY, outZ)

            Return Pack(outX, outY, outZ)
        End Function

        ''' <summary>
        ''' 逐点向量减法（写入目标数组）：<c>out(i) = a(i) - b(i)</c>。
        ''' </summary>
        Public Shared Sub Subtract3Into(x1 As Double(), y1 As Double(), z1 As Double(),
                                        x2 As Double(), y2 As Double(), z2 As Double(),
                                        outX As Double(), outY As Double(), outZ As Double())
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2, outX, outY, outZ)

            Call BinaryInto(x1, x2, outX, len, False)
            Call BinaryInto(y1, y2, outY, len, False)
            Call BinaryInto(z1, z2, outZ, len, False)
        End Sub

        ''' <summary>
        ''' 逐点向量减法：返回 <c>{X, Y, Z}</c> 三条结果数组（会分配）。
        ''' </summary>
        Public Shared Function Subtract3(x1 As Double(), y1 As Double(), z1 As Double(),
                                         x2 As Double(), y2 As Double(), z2 As Double()) As Double()()
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2)
            Dim outX As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim outY As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim outZ As Double() = SimdEngine.NewArray(Of Double)(len)

            Call Subtract3Into(x1, y1, z1, x2, y2, z2, outX, outY, outZ)

            Return Pack(outX, outY, outZ)
        End Function

        ''' <summary>
        ''' 逐点数乘（写入目标数组）：<c>out(i) = scalar * v(i)</c>。
        ''' </summary>
        Public Shared Sub Scale3Into(x As Double(), y As Double(), z As Double(), scalar As Double,
                                     outX As Double(), outY As Double(), outZ As Double())
            Dim len As Integer = CheckLengths(NameOf(x), x, y, z, outX, outY, outZ)

            Call ScaleInto(x, scalar, outX, len)
            Call ScaleInto(y, scalar, outY, len)
            Call ScaleInto(z, scalar, outZ, len)
        End Sub

        ''' <summary>
        ''' 逐点数乘：返回 <c>{X, Y, Z}</c> 三条结果数组（会分配）。
        ''' </summary>
        Public Shared Function Scale3(x As Double(), y As Double(), z As Double(), scalar As Double) As Double()()
            Dim len As Integer = CheckLengths(NameOf(x), x, y, z)
            Dim outX As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim outY As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim outZ As Double() = SimdEngine.NewArray(Of Double)(len)

            Call Scale3Into(x, y, z, scalar, outX, outY, outZ)

            Return Pack(outX, outY, outZ)
        End Function

#End Region

#Region "dot / distance / norm"

        ''' <summary>
        ''' 三维点积之和：<c>SUM(x1(i)·x2(i) + y1(i)·y2(i) + z1(i)·z2(i))</c>。
        ''' </summary>
        ''' <remarks>
        ''' 用法典型场景是把“一组点到某个查询点的向量”与“同一组的法向”做整体点积
        ''' （平面一致性检验、FPFH 角度特征、点到面距离项）。三次归约分别作用于
        ''' 三条连续数组，寄存器内的乘加因此是满吞吐的。
        ''' </remarks>
        Public Shared Function Dot3(x1 As Double(), y1 As Double(), z1 As Double(),
                                    x2 As Double(), y2 As Double(), z2 As Double()) As Double
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2)
            If len = 0 Then Return 0.0

            Return SimdReduce.Dot(x1, x2) + SimdReduce.Dot(y1, y2) + SimdReduce.Dot(z1, z2)
        End Function

        ''' <summary>
        ''' 区间三维点积之和（用于邻域子集，避免先 <see cref="SimdIndexed.Gather3"/> 再归约）。
        ''' </summary>
        Public Shared Function Dot3(x1 As Double(), y1 As Double(), z1 As Double(),
                                    x2 As Double(), y2 As Double(), z2 As Double(),
                                    start As Integer, ends As Integer) As Double
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2)

            If start < 0 OrElse ends > len OrElse start >= ends Then
                If start = ends Then Return 0.0

                Throw New ArgumentOutOfRangeException($"invalid range [{start}, {ends}) for arrays of length {len}!")
            End If

            Return DotRange(x1, x2, start, ends) +
                DotRange(y1, y2, start, ends) +
                DotRange(z1, z2, start, ends)
        End Function

        ''' <summary>
        ''' 两个三维点集之间逐点平方距离之和：<c>SUM(|a(i) - b(i)|²)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 这是 ICP 目标函数、SOR 邻域距离统计、RANSAC 内点计数共同的核心归约；
        ''' 单趟遍历、零临时数组，与
        ''' “<see cref="Subtract3(Double(), Double(), Double(), Double(), Double(), Double())"/> 之后再
        ''' <see cref="SimdReduce.SumSquares(Double())"/>” 的两趟写法相比少了一整组中间数组。
        ''' </remarks>
        Public Shared Function DistanceSquared3(x1 As Double(), y1 As Double(), z1 As Double(),
                                                x2 As Double(), y2 As Double(), z2 As Double()) As Double
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2)
            If len = 0 Then Return 0.0

            Return SimdReduce.DistanceSquared(x1, x2) +
                SimdReduce.DistanceSquared(y1, y2) +
                SimdReduce.DistanceSquared(z1, z2)
        End Function

        ''' <summary>
        ''' 区间三维平方距离之和。
        ''' </summary>
        Public Shared Function DistanceSquared3(x1 As Double(), y1 As Double(), z1 As Double(),
                                                x2 As Double(), y2 As Double(), z2 As Double(),
                                                start As Integer, ends As Integer) As Double
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2)

            If start < 0 OrElse ends > len OrElse start > ends Then
                Throw New ArgumentOutOfRangeException($"invalid range [{start}, {ends}) for arrays of length {len}!")
            End If
            If start = ends Then Return 0.0

            Return DistanceSquaredRange(x1, x2, start, ends) +
                DistanceSquaredRange(y1, y2, start, ends) +
                DistanceSquaredRange(z1, z2, start, ends)
        End Function

        ''' <summary>
        ''' 三维向量范数：<c>SQRT(SUM(v(i)²))</c>。
        ''' </summary>
        Public Shared Function Norm3(x As Double(), y As Double(), z As Double()) As Double
            Dim len As Integer = CheckLengths(NameOf(x), x, y, z)
            If len = 0 Then Return 0.0

            Return std.Sqrt(SimdReduce.SumSquares(x) + SimdReduce.SumSquares(y) + SimdReduce.SumSquares(z))
        End Function

        ''' <summary>
        ''' 就地归一化：<c>v(i) /= ‖v(i)‖</c>。零长度向量保持为零（不产生 NaN）。
        ''' </summary>
        ''' <returns>是否所有向量都成功归一化（存在零长度向量时返回 <c>False</c>）</returns>
        Public Shared Function NormalizeInPlace3(x As Double(), y As Double(), z As Double()) As Boolean
            Dim len As Integer = CheckLengths(NameOf(x), x, y, z)
            If len = 0 Then Return True

            Dim ok As Boolean = True
            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = 0

            ' 范数必须逐点计算（每个点的缩放因子不同），因此这里无法整块向量化；
            ' 但除法部分仍然走向量化路径，避免逐元素调用处再写一遍。
            Dim norms As Double() = New Double(len - 1) {}

            For k As Integer = 0 To len - 1
                norms(k) = std.Sqrt(x(k) * x(k) + y(k) * y(k) + z(k) * z(k))
            Next

            If SIMDEnvironment.IsEnabled AndAlso len >= count Then
                Dim last As Integer = len - count

                Do While i <= last
                    Dim nx As Vector(Of Double) = New Vector(Of Double)(norms, i)
                    Dim mask As Vector(Of Double) = Vector.GreaterThan(Of Double)(nx, Vector(Of Double).Zero)
                    Dim safe As Vector(Of Double) = Vector.ConditionalSelect(Of Double)(mask, nx, Vector(Of Double).One)

                    Vector.Divide(Of Double)(New Vector(Of Double)(x, i), safe).CopyTo(x, i)
                    Vector.Divide(Of Double)(New Vector(Of Double)(y, i), safe).CopyTo(y, i)
                    Vector.Divide(Of Double)(New Vector(Of Double)(z, i), safe).CopyTo(z, i)
                    i += count
                Loop
            End If

            Do While i < len
                Dim n As Double = norms(i)

                If n > 0.0 Then
                    x(i) /= n
                    y(i) /= n
                    z(i) /= n
                Else
                    ok = False
                End If

                i += 1
            Loop

            ' 向量化分支不负责统计零长度向量，这里补一次判定以保持返回值语义
            If ok Then
                For k As Integer = 0 To len - 1
                    If norms(k) <= 0.0 Then
                        ok = False
                        Exit For
                    End If
                Next
            End If

            Return ok
        End Function

        ''' <summary>
        ''' 逐点叉积（写入目标数组）：<c>out(i) = a(i) × b(i)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 叉积的三个分量互相依赖（<c>x·y' − y·x'</c>），无法沿点方向整块向量化，
        ''' 因此保持标量循环；它的价值在于统一 SoA 入参风格，避免调用方在热路径上
        ''' 手工拆分三条数组。
        ''' </remarks>
        Public Shared Sub Cross3Into(x1 As Double(), y1 As Double(), z1 As Double(),
                                     x2 As Double(), y2 As Double(), z2 As Double(),
                                     outX As Double(), outY As Double(), outZ As Double())
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2, outX, outY, outZ)

            For i As Integer = 0 To len - 1
                Dim ax As Double = x1(i), ay As Double = y1(i), az As Double = z1(i)
                Dim bx As Double = x2(i), by As Double = y2(i), bz As Double = z2(i)

                outX(i) = ay * bz - az * by
                outY(i) = az * bx - ax * bz
                outZ(i) = ax * by - ay * bx
            Next
        End Sub

        ''' <summary>
        ''' 逐点叉积：返回 <c>{X, Y, Z}</c> 三条结果数组（会分配）。
        ''' </summary>
        Public Shared Function Cross3(x1 As Double(), y1 As Double(), z1 As Double(),
                                      x2 As Double(), y2 As Double(), z2 As Double()) As Double()()
            Dim len As Integer = CheckLengths(NameOf(x1), x1, y1, z1, x2, y2, z2)
            Dim outX As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim outY As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim outZ As Double() = SimdEngine.NewArray(Of Double)(len)

            Call Cross3Into(x1, y1, z1, x2, y2, z2, outX, outY, outZ)

            Return Pack(outX, outY, outZ)
        End Function

#End Region

#Region "transform"

        ''' <summary>
        ''' 逐点刚体/仿射变换（写入目标数组）：
        ''' <c>out(i) = M · v(i) + t</c>，其中 <c>M</c> 为 3x3 行主序矩阵。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 三个输出分量在同一个循环体内完成，三条输入数组只被装载一次；
        ''' 9 个矩阵元素与 3 个平移量被广播(splat)到整条向量寄存器上，
        ''' 因此每个块只需要 9 次乘加，没有逐元素的分支与间接寻址。
        ''' </para>
        ''' <para>
        ''' 这是配准（把源点云变换到目标坐标系）、外参标定、多站拼接的共同原语；
        ''' 典型点云规模在 1e6 量级时，它决定了整条流水线的下限耗时。
        ''' </para>
        ''' </remarks>
        Public Shared Sub Transform3Into(x As Double(), y As Double(), z As Double(),
                                         m00 As Double, m01 As Double, m02 As Double,
                                         m10 As Double, m11 As Double, m12 As Double,
                                         m20 As Double, m21 As Double, m22 As Double,
                                         tx As Double, ty As Double, tz As Double,
                                         outX As Double(), outY As Double(), outZ As Double())
            Dim len As Integer = CheckLengths(NameOf(x), x, y, z, outX, outY, outZ)
            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= count Then
                Dim v00 As New Vector(Of Double)(m00), v01 As New Vector(Of Double)(m01), v02 As New Vector(Of Double)(m02)
                Dim v10 As New Vector(Of Double)(m10), v11 As New Vector(Of Double)(m11), v12 As New Vector(Of Double)(m12)
                Dim v20 As New Vector(Of Double)(m20), v21 As New Vector(Of Double)(m21), v22 As New Vector(Of Double)(m22)
                Dim vtx As New Vector(Of Double)(tx), vty As New Vector(Of Double)(ty), vtz As New Vector(Of Double)(tz)
                Dim last As Integer = len - count

                Do While i <= last
                    Dim vx As New Vector(Of Double)(x, i)
                    Dim vy As New Vector(Of Double)(y, i)
                    Dim vz As New Vector(Of Double)(z, i)

                    (v00 * vx + v01 * vy + v02 * vz + vtx).CopyTo(outX, i)
                    (v10 * vx + v11 * vy + v12 * vz + vty).CopyTo(outY, i)
                    (v20 * vx + v21 * vy + v22 * vz + vtz).CopyTo(outZ, i)
                    i += count
                Loop
            End If

            Do While i < len
                Dim px As Double = x(i), py As Double = y(i), pz As Double = z(i)

                outX(i) = m00 * px + m01 * py + m02 * pz + tx
                outY(i) = m10 * px + m11 * py + m12 * pz + ty
                outZ(i) = m20 * px + m21 * py + m22 * pz + tz
                i += 1
            Loop
        End Sub

        ''' <summary>
        ''' 就地刚体/仿射变换：<c>v(i) = M · v(i) + t</c>。
        ''' </summary>
        ''' <remarks>
        ''' 输入与输出共用同一块内存，因此每个块必须先把 <c>x/y/z</c> 全部装载到
        ''' 寄存器之后才能写回；这里用三个局部 <see cref="Vector(Of Double)"/> 保证这一点。
        ''' </remarks>
        Public Shared Sub Transform3InPlace(x As Double(), y As Double(), z As Double(),
                                            m00 As Double, m01 As Double, m02 As Double,
                                            m10 As Double, m11 As Double, m12 As Double,
                                            m20 As Double, m21 As Double, m22 As Double,
                                            tx As Double, ty As Double, tz As Double)
            Dim len As Integer = CheckLengths(NameOf(x), x, y, z)
            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= count Then
                Dim v00 As New Vector(Of Double)(m00), v01 As New Vector(Of Double)(m01), v02 As New Vector(Of Double)(m02)
                Dim v10 As New Vector(Of Double)(m10), v11 As New Vector(Of Double)(m11), v12 As New Vector(Of Double)(m12)
                Dim v20 As New Vector(Of Double)(m20), v21 As New Vector(Of Double)(m21), v22 As New Vector(Of Double)(m22)
                Dim vtx As New Vector(Of Double)(tx), vty As New Vector(Of Double)(ty), vtz As New Vector(Of Double)(tz)
                Dim last As Integer = len - count

                Do While i <= last
                    Dim vx As New Vector(Of Double)(x, i)
                    Dim vy As New Vector(Of Double)(y, i)
                    Dim vz As New Vector(Of Double)(z, i)

                    (v00 * vx + v01 * vy + v02 * vz + vtx).CopyTo(x, i)
                    (v10 * vx + v11 * vy + v12 * vz + vty).CopyTo(y, i)
                    (v20 * vx + v21 * vy + v22 * vz + vtz).CopyTo(z, i)
                    i += count
                Loop
            End If

            Do While i < len
                Dim px As Double = x(i), py As Double = y(i), pz As Double = z(i)

                x(i) = m00 * px + m01 * py + m02 * pz + tx
                y(i) = m10 * px + m11 * py + m12 * pz + ty
                z(i) = m20 * px + m21 * py + m22 * pz + tz
                i += 1
            Loop
        End Sub

#End Region

#Region "statistics"

        ''' <summary>
        ''' 轴对齐包围盒：单趟扫描同时得到三条轴上的最小值与最大值。
        ''' </summary>
        ''' <remarks>
        ''' min/max 是<b>可结合</b>的归约，因此可以整块向量化：每个块用
        ''' <see cref="Vector.Min(Of T)(Vector(Of T), Vector(Of T))"/> 折叠到累加器，
        ''' 最后只在收尾时做一次水平归约。相比“三条数组各调一次
        ''' <see cref="SimdReduce.Min(Double())"/> 与 <see cref="SimdReduce.Max(Double())"/>”
        ''' 省掉了三倍的内存往返。
        ''' </remarks>
        Public Shared Function Bounds3(x As Double(), y As Double(), z As Double()) As Vector3Bounds
            Dim len As Integer = CheckLengths(NameOf(x), x, y, z)
            If len = 0 Then Return Vector3Bounds.Empty

            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = 0
            Dim minX As Double = Double.MaxValue, minY As Double = Double.MaxValue, minZ As Double = Double.MaxValue
            Dim maxX As Double = Double.MinValue, maxY As Double = Double.MinValue, maxZ As Double = Double.MinValue

            If SIMDEnvironment.IsEnabled AndAlso len >= count Then
                Dim accMinX As New Vector(Of Double)(x, 0)
                Dim accMinY As New Vector(Of Double)(y, 0)
                Dim accMinZ As New Vector(Of Double)(z, 0)
                Dim accMaxX As Vector(Of Double) = accMinX
                Dim accMaxY As Vector(Of Double) = accMinY
                Dim accMaxZ As Vector(Of Double) = accMinZ
                Dim last As Integer = len - count

                i = count

                Do While i <= last
                    Dim vx As New Vector(Of Double)(x, i)
                    Dim vy As New Vector(Of Double)(y, i)
                    Dim vz As New Vector(Of Double)(z, i)

                    accMinX = Vector.Min(Of Double)(accMinX, vx)
                    accMinY = Vector.Min(Of Double)(accMinY, vy)
                    accMinZ = Vector.Min(Of Double)(accMinZ, vz)
                    accMaxX = Vector.Max(Of Double)(accMaxX, vx)
                    accMaxY = Vector.Max(Of Double)(accMaxY, vy)
                    accMaxZ = Vector.Max(Of Double)(accMaxZ, vz)
                    i += count
                Loop

                For k As Integer = 0 To count - 1
                    minX = std.Min(minX, accMinX.GetElement(k))
                    minY = std.Min(minY, accMinY.GetElement(k))
                    minZ = std.Min(minZ, accMinZ.GetElement(k))
                    maxX = std.Max(maxX, accMaxX.GetElement(k))
                    maxY = std.Max(maxY, accMaxY.GetElement(k))
                    maxZ = std.Max(maxZ, accMaxZ.GetElement(k))
                Next
            End If

            Do While i < len
                If x(i) < minX Then minX = x(i)
                If x(i) > maxX Then maxX = x(i)
                If y(i) < minY Then minY = y(i)
                If y(i) > maxY Then maxY = y(i)
                If z(i) < minZ Then minZ = z(i)
                If z(i) > maxZ Then maxZ = z(i)
                i += 1
            Loop

            Return New Vector3Bounds(New Vector3Value(minX, minY, minZ), New Vector3Value(maxX, maxY, maxZ))
        End Function

        ''' <summary>
        ''' 加权质心：<c>Σ(w(i)·v(i)) / Σw(i)</c>。权重之和为零时退化为等权平均。
        ''' </summary>
        ''' <remarks>
        ''' 邻域法向估计、MLS 投影、FPFH 邻域加权都把高斯/距离权重作用在同一组邻域点上，
        ''' 这个内核避免为每条轴单独写一遍“乘权重再求和”的循环。
        ''' </remarks>
        Public Shared Function WeightedMean3(x As Double(), y As Double(), z As Double(), weights As Double()) As Vector3Value
            Dim len As Integer = CheckLengths(NameOf(x), x, y, z, weights)
            If len = 0 Then Return New Vector3Value(0, 0, 0)

            Dim sumW As Double = SimdReduce.Sum(weights)
            Dim wx As Double = SimdReduce.Dot(weights, x)
            Dim wy As Double = SimdReduce.Dot(weights, y)
            Dim wz As Double = SimdReduce.Dot(weights, z)

            If std.Abs(sumW) <= Double.Epsilon Then
                Return New Vector3Value(SimdReduce.Mean(x), SimdReduce.Mean(y), SimdReduce.Mean(z))
            End If

            Return New Vector3Value(wx / sumW, wy / sumW, wz / sumW)
        End Function

        ''' <summary>
        ''' 邻域协方差（两趟、以均值为中心），返回均值与 3x3 对称协方差矩阵的 6 个独立元素。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 先用 <see cref="WeightedMean3(Double(), Double(), Double(), Double())"/> 之外的等权均值
        ''' 求中心，再累加中心化后的二阶矩。两趟写法（而不是“平方和 − 均值平方”的一趟写法）
        ''' 是刻意的：点云坐标常常带有很大的绝对偏移（例如车间坐标系下 <c>X ≈ 1e5</c>），
        ''' 一趟写法会出现 <c>E[x²] − E[x]²</c> 的灾难性抵消，把法向估计直接毁掉；
        ''' 邻域点数通常只有 10~100，多一趟的代价可以忽略。
        ''' </para>
        ''' <para>
        ''' 协方差的 6 个分量用 6 个独立累加器整块累加（每个块内 <c>dx·dy</c> 等乘积各算一次），
        ''' 因此这是法向/曲率/ISS/Harris 响应计算里最热的那段循环。
        ''' </para>
        ''' </remarks>
        Public Shared Function Covariance3(x As Double(), y As Double(), z As Double()) As Vector3Moments
            Return Covariance3(x, y, z, 0, If(x Is Nothing, 0, x.Length))
        End Function

        ''' <summary>
        ''' 区间邻域协方差（以区间内点的等权均值为中心）。
        ''' </summary>
        Public Shared Function Covariance3(x As Double(), y As Double(), z As Double(),
                                           start As Integer, ends As Integer) As Vector3Moments
            Dim len As Integer = CheckLengths(NameOf(x), x, y, z)

            If start < 0 OrElse ends > len OrElse start > ends Then
                Throw New ArgumentOutOfRangeException($"invalid range [{start}, {ends}) for arrays of length {len}!")
            End If

            Dim n As Integer = ends - start
            If n <= 0 Then Return Vector3Moments.Empty

            Dim mx As Double = 0, my As Double = 0, mz As Double = 0

            For i As Integer = start To ends - 1
                mx += x(i)
                my += y(i)
                mz += z(i)
            Next

            mx /= n
            my /= n
            mz /= n

            Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
            Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0
            Dim count As Integer = Vector(Of Double).Count
            Dim i2 As Integer = start

            If SIMDEnvironment.IsEnabled AndAlso n >= count Then
                Dim vmx As New Vector(Of Double)(mx)
                Dim vmy As New Vector(Of Double)(my)
                Dim vmz As New Vector(Of Double)(mz)
                Dim axx As Vector(Of Double) = Vector(Of Double).Zero
                Dim axy As Vector(Of Double) = Vector(Of Double).Zero
                Dim axz As Vector(Of Double) = Vector(Of Double).Zero
                Dim ayy As Vector(Of Double) = Vector(Of Double).Zero
                Dim ayz As Vector(Of Double) = Vector(Of Double).Zero
                Dim azz As Vector(Of Double) = Vector(Of Double).Zero
                Dim last As Integer = ends - count

                Do While i2 <= last
                    Dim dx As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(x, i2), vmx)
                    Dim dy As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(y, i2), vmy)
                    Dim dz As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(z, i2), vmz)

                    axx = Vector.Add(Of Double)(axx, Vector.Multiply(Of Double)(dx, dx))
                    axy = Vector.Add(Of Double)(axy, Vector.Multiply(Of Double)(dx, dy))
                    axz = Vector.Add(Of Double)(axz, Vector.Multiply(Of Double)(dx, dz))
                    ayy = Vector.Add(Of Double)(ayy, Vector.Multiply(Of Double)(dy, dy))
                    ayz = Vector.Add(Of Double)(ayz, Vector.Multiply(Of Double)(dy, dz))
                    azz = Vector.Add(Of Double)(azz, Vector.Multiply(Of Double)(dz, dz))
                    i2 += count
                Loop

                For k As Integer = 0 To count - 1
                    cxx += axx.GetElement(k)
                    cxy += axy.GetElement(k)
                    cxz += axz.GetElement(k)
                    cyy += ayy.GetElement(k)
                    cyz += ayz.GetElement(k)
                    czz += azz.GetElement(k)
                Next
            End If

            Do While i2 < ends
                Dim dx As Double = x(i2) - mx
                Dim dy As Double = y(i2) - my
                Dim dz As Double = z(i2) - mz

                cxx += dx * dx
                cxy += dx * dy
                cxz += dx * dz
                cyy += dy * dy
                cyz += dy * dz
                czz += dz * dz
                i2 += 1
            Loop

            ' 除以 n 得到总体协方差（与 PCL computeCovariance 一致，不做无偏修正）
            Dim invN As Double = 1.0 / n

            Return New Vector3Moments(New Vector3Value(mx, my, mz),
                                      cxx * invN, cxy * invN, cxz * invN,
                                      cyy * invN, cyz * invN, czz * invN)
        End Function

#End Region
    End Class

    ''' <summary>
    ''' 三维双精度向量值类型：SIMD 内核的标量返回值（均值、中心、法向等）。
    ''' </summary>
    ''' <remarks>
    ''' 这是一个纯值类型，只承载“内核算出来的一个三元组”；
    ''' 需要完整几何运算（旋转、插值、四元数）时请使用上层模块中的向量类型。
    ''' </remarks>
    Public Structure Vector3Value

        Public ReadOnly X As Double
        Public ReadOnly Y As Double
        Public ReadOnly Z As Double

        Public Sub New(x As Double, y As Double, z As Double)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        ''' <summary>
        ''' 向量长度 <c>SQRT(x² + y² + z²)</c>。
        ''' </summary>
        Public ReadOnly Property Length As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return std.Sqrt(X * X + Y * Y + Z * Z)
            End Get
        End Property

        ''' <summary>
        ''' 平方长度：<c>x² + y² + z²</c>（比较距离时省掉 <c>Sqrt</c>）。
        ''' </summary>
        Public ReadOnly Property LengthSquared As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return X * X + Y * Y + Z * Z
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(a As Vector3Value, b As Vector3Value) As Vector3Value
            Return New Vector3Value(a.X + b.X, a.Y + b.Y, a.Z + b.Z)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(a As Vector3Value, b As Vector3Value) As Vector3Value
            Return New Vector3Value(a.X - b.X, a.Y - b.Y, a.Z - b.Z)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(a As Vector3Value, scalar As Double) As Vector3Value
            Return New Vector3Value(a.X * scalar, a.Y * scalar, a.Z * scalar)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(a As Vector3Value, scalar As Double) As Vector3Value
            Return New Vector3Value(a.X / scalar, a.Y / scalar, a.Z / scalar)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(a As Vector3Value) As Vector3Value
            Return New Vector3Value(-a.X, -a.Y, -a.Z)
        End Operator

        ''' <summary>
        ''' 点积：<c>a · b</c>。
        ''' </summary>
        Public Shared Function Dot(a As Vector3Value, b As Vector3Value) As Double
            Return a.X * b.X + a.Y * b.Y + a.Z * b.Z
        End Function

        ''' <summary>
        ''' 叉积：<c>a × b</c>。
        ''' </summary>
        Public Shared Function Cross(a As Vector3Value, b As Vector3Value) As Vector3Value
            Return New Vector3Value(a.Y * b.Z - a.Z * b.Y,
                                    a.Z * b.X - a.X * b.Z,
                                    a.X * b.Y - a.Y * b.X)
        End Function

        ''' <summary>
        ''' 归一化；零向量返回零向量（不产生 NaN）。
        ''' </summary>
        Public Function Normalize() As Vector3Value
            Dim n As Double = Length

            If n <= 0.0 Then Return New Vector3Value(0, 0, 0)

            Return New Vector3Value(X / n, Y / n, Z / n)
        End Function

        Public Overrides Function ToString() As String
            Return $"({X:G6}, {Y:G6}, {Z:G6})"
        End Function
    End Structure

    ''' <summary>
    ''' 轴对齐包围盒（三维）：<see cref="SimdVector3.Bounds3(Double(), Double(), Double())"/> 的返回类型。
    ''' </summary>
    Public Structure Vector3Bounds

        ''' <summary>
        ''' 空包围盒：最小值取 <see cref="Double.MaxValue"/>、最大值取 <see cref="Double.MinValue"/>，
        ''' 与任意包围盒 <see cref="Union(Vector3Bounds)"/> 之后仍然是原包围盒。
        ''' </summary>
        Public Shared ReadOnly Empty As New Vector3Bounds(
            New Vector3Value(Double.MaxValue, Double.MaxValue, Double.MaxValue),
            New Vector3Value(Double.MinValue, Double.MinValue, Double.MinValue))

        Public ReadOnly Min As Vector3Value
        Public ReadOnly Max As Vector3Value

        Public Sub New(min As Vector3Value, max As Vector3Value)
            Me.Min = min
            Me.Max = max
        End Sub

        ''' <summary>
        ''' 包围盒是否有效（三个方向都有非负尺寸）。
        ''' </summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Max.X < Min.X OrElse Max.Y < Min.Y OrElse Max.Z < Min.Z
            End Get
        End Property

        ''' <summary>
        ''' 三条轴上的尺寸。
        ''' </summary>
        Public ReadOnly Property Size As Vector3Value
            Get
                Return New Vector3Value(Max.X - Min.X, Max.Y - Min.Y, Max.Z - Min.Z)
            End Get
        End Property

        ''' <summary>
        ''' 包围盒中心。
        ''' </summary>
        Public ReadOnly Property Center As Vector3Value
            Get
                Return New Vector3Value((Max.X + Min.X) * 0.5, (Max.Y + Min.Y) * 0.5, (Max.Z + Min.Z) * 0.5)
            End Get
        End Property

        ''' <summary>
        ''' 最长边长度（体素/网格尺寸的常用参考）。
        ''' </summary>
        Public ReadOnly Property Diagonal As Double
            Get
                Return Size.Length
            End Get
        End Property

        Public Function Contains(p As Vector3Value) As Boolean
            Return p.X >= Min.X AndAlso p.X <= Max.X AndAlso
                p.Y >= Min.Y AndAlso p.Y <= Max.Y AndAlso
                p.Z >= Min.Z AndAlso p.Z <= Max.Z
        End Function

        Public Function Union(other As Vector3Bounds) As Vector3Bounds
            Return New Vector3Bounds(
                New Vector3Value(std.Min(Min.X, other.Min.X), std.Min(Min.Y, other.Min.Y), std.Min(Min.Z, other.Min.Z)),
                New Vector3Value(std.Max(Max.X, other.Max.X), std.Max(Max.Y, other.Max.Y), std.Max(Max.Z, other.Max.Z)))
        End Function

        Public Function Expand(margin As Double) As Vector3Bounds
            Dim d As New Vector3Value(margin, margin, margin)

            Return New Vector3Bounds(Min - d, Max + d)
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Min} .. {Max}]"
        End Function
    End Structure

    ''' <summary>
    ''' 邻域一阶/二阶统计量：均值与 3x3 对称协方差的 6 个独立元素。
    ''' </summary>
    ''' <remarks>
    ''' 对称协方差的存储顺序固定为 <c>Cxx, Cxy, Cxz, Cyy, Cyz, Czz</c>；
    ''' 上层（法向估计、曲率、ISS、Harris）会把它直接拼成 3x3 矩阵再做闭式特征分解。
    ''' </remarks>
    Public Structure Vector3Moments

        Public Shared ReadOnly Empty As New Vector3Moments(New Vector3Value(0, 0, 0), 0, 0, 0, 0, 0, 0)

        Public ReadOnly Mean As Vector3Value
        Public ReadOnly Cxx As Double
        Public ReadOnly Cxy As Double
        Public ReadOnly Cxz As Double
        Public ReadOnly Cyy As Double
        Public ReadOnly Cyz As Double
        Public ReadOnly Czz As Double

        Public Sub New(mean As Vector3Value, cxx As Double, cxy As Double, cxz As Double,
                       cyy As Double, cyz As Double, czz As Double)
            Me.Mean = mean
            Me.Cxx = cxx
            Me.Cxy = cxy
            Me.Cxz = cxz
            Me.Cyy = cyy
            Me.Cyz = cyz
            Me.Czz = czz
        End Sub

        ''' <summary>
        ''' 协方差矩阵的迹：<c>λ1 + λ2 + λ3</c>（几何上等于邻域点在三个主轴上的方差之和）。
        ''' </summary>
        Public ReadOnly Property Trace As Double
            Get
                Return Cxx + Cyy + Czz
            End Get
        End Property

        ''' <summary>
        ''' 把对称协方差展开成 3x3 行主序矩阵。
        ''' </summary>
        Public Function ToMatrix() As Double()()
            Return {
                {Cxx, Cxy, Cxz},
                {Cxy, Cyy, Cyz},
                {Cxz, Cyz, Czz}
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"mean={Mean}, trace={Trace:G6}"
        End Function
    End Structure

End Namespace
