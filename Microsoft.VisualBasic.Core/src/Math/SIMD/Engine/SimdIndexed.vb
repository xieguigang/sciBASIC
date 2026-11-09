Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' 索引内核：按下标列表做 gather / scatter / 归约。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 点云算法的内层循环几乎都是“对某个点的 k 个近邻做一次归约”
    ''' （法向协方差、FPFH 直方图、ICP 距离、ISS 特征值）。
    ''' 邻域下标是不连续的随机访问，<see cref="System.Numerics.Vector(Of T)"/> 没有可用的
    ''' gather 指令（<c>AVX2</c> 的 <c>vpgatherdd</c> 不支持 <see cref="Double"/>，
    ''' 且 .NET 的 <see cref="Vector(Of T)"/> 抽象层也不暴露它），
    ''' 因此这里<b>不假装</b>能做“向量化 gather”。
    ''' </para>
    ''' <para>
    ''' 这些函数提供的真实收益是：
    ''' </para>
    ''' <list type="bullet">
    ''' <item>
    ''' <description>
    ''' <b>一次到位</b>：把散落的下标访问收敛到一个函数里，避免每个调用点重复写
    ''' 边界检查、临时数组与累加顺序，也避免在热路径上被 <c>LINQ</c> 的枚举器/委托开销吃掉。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>后续可向量化</b>：<see cref="Gather3"/>
    ''' 把邻域数据<b>压实</b>到连续缓冲后，后续的协方差/直方图计算就能整块走 SIMD；
    ''' 这是“gather 一次 + 向量化多次”的经典折衷。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>零分配</b>：所有函数只写调用方提供的目标数组或返回标量；
    ''' 需要临时缓冲的调用方（例如 <c>NeighborhoodQuery</c>）可以使用线程本地池化缓冲。
    ''' </description>
    ''' </item>
    ''' </list>
    ''' <para>
    ''' <b>线程安全</b>：<see cref="ScatterAdd"/>
    ''' 的累加不是原子操作，同一目标下标只能被同一个线程写入；
    ''' 多线程场景请按“每个线程独立的直方图/累加器，最后合并”的方式使用。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdIndexed

        Private Sub New()
        End Sub

#Region "helpers"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CheckNull(v As Array, name As String)
            If v Is Nothing Then
                Throw New ArgumentNullException(name, "the input array can not be NULL!")
            End If
        End Sub

        Private Shared Sub CheckRange(len As Integer, start As Integer, ends As Integer)
            If start < 0 OrElse ends > len OrElse start > ends Then
                Throw New ArgumentOutOfRangeException($"invalid range [{start}, {ends}) for arrays of length {len}!")
            End If
        End Sub

        ''' <summary>
        ''' 校验目标数组长度足以容纳 <c>[start, ends)</c> 个元素。
        ''' </summary>
        Private Shared Sub CheckDestination(dstLength As Integer, required As Integer, name As String)
            If dstLength < required Then
                Throw New ArgumentException($"the destination array is too small: {dstLength} < {required}!", name)
            End If
        End Sub

#End Region

#Region "gather"

        ''' <summary>
        ''' 按下标列表取值：<c>dst(k) = src(indices(start + k))</c>。
        ''' </summary>
        Public Shared Sub Gather(src As Double(), indices As Integer(), dst As Double())
            If indices Is Nothing Then Throw New ArgumentNullException(NameOf(indices))
            Call Gather(src, indices, 0, indices.Length, dst)
        End Sub

        ''' <summary>
        ''' 按下标列表区间取值：<c>dst(k) = src(indices(start + k))</c>。
        ''' </summary>
        Public Shared Sub Gather(src As Double(), indices As Integer(), start As Integer, ends As Integer, dst As Double())
            Call CheckNull(src, NameOf(src))
            Call CheckNull(indices, NameOf(indices))
            Call CheckNull(dst, NameOf(dst))
            Call CheckRange(indices.Length, start, ends)
            Call CheckDestination(dst.Length, ends - start, NameOf(dst))

            Dim k As Integer = 0

            For i As Integer = start To ends - 1
                Dim index As Integer = indices(i)

                If index < 0 OrElse index >= src.Length Then
                    Throw New ArgumentOutOfRangeException(NameOf(indices), $"index {index} is out of the source range [0, {src.Length})!")
                End If

                dst(k) = src(index)
                k += 1
            Next
        End Sub

        ''' <summary>
        ''' 按下标列表取值（三维 SoA）：把邻域点压实成三条连续缓冲。
        ''' </summary>
        ''' <remarks>
        ''' 这是“gather 一次，向量化多次”的入口：法向估计、FPFH 邻域加权、
        ''' MLS 局部拟合都先把 k 个邻域点压实到一段连续内存，
        ''' 之后就可以直接调用
        ''' <see cref="SimdVector3.Covariance3(Double(), Double(), Double(), Integer, Integer)"/> 等内核。
        ''' </remarks>
        Public Shared Sub Gather3(x As Double(), y As Double(), z As Double(),
                                  indices As Integer(), start As Integer, ends As Integer,
                                  outX As Double(), outY As Double(), outZ As Double())
            Call CheckNull(x, NameOf(x))
            Call CheckNull(y, NameOf(y))
            Call CheckNull(z, NameOf(z))
            Call CheckNull(indices, NameOf(indices))
            Call CheckRange(indices.Length, start, ends)

            Dim n As Integer = ends - start

            If outX.Length < n OrElse outY.Length < n OrElse outZ.Length < n Then
                Throw New ArgumentException($"the destination arrays are too small: {outX.Length}/{outY.Length}/{outZ.Length} < {n}!")
            End If

            Dim k As Integer = 0

            For i As Integer = start To ends - 1
                Dim index As Integer = indices(i)

                If index < 0 OrElse index >= x.Length Then
                    Throw New ArgumentOutOfRangeException(NameOf(indices), $"index {index} is out of the source range [0, {x.Length})!")
                End If

                outX(k) = x(index)
                outY(k) = y(index)
                outZ(k) = z(index)
                k += 1
            Next
        End Sub

#End Region

#Region "scatter"

        ''' <summary>
        ''' 按下标列表写回：<c>dst(indices(start + k)) = values(k)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 用于“逐点独立计算、但结果要按原下标归位”的场景：
        ''' 例如抽样后把标签/法向写到原始点云的对应位置，
        ''' 或者把并行分块算出的结果合并回全局数组。
        ''' </remarks>
        Public Shared Sub Scatter(values As Double(), indices As Integer(), start As Integer, ends As Integer, dst As Double())
            Call CheckNull(values, NameOf(values))
            Call CheckNull(indices, NameOf(indices))
            Call CheckRange(indices.Length, start, ends)
            Call CheckNull(dst, NameOf(dst))

            Dim n As Integer = ends - start
            If values.Length < n Then
                Throw New ArgumentException($"the value array is too small: {values.Length} < {n}!", NameOf(values))
            End If

            Dim k As Integer = 0

            For i As Integer = start To ends - 1
                Dim index As Integer = indices(i)

                If index < 0 OrElse index >= dst.Length Then
                    Throw New ArgumentOutOfRangeException(NameOf(indices), $"index {index} is out of the destination range [0, {dst.Length})!")
                End If

                dst(index) = values(k)
                k += 1
            Next
        End Sub

        ''' <summary>
        ''' 按下标列表累加：<c>dst(indices(start + k)) += values(k)</c>。
        ''' </summary>
        ''' <remarks>
        ''' <b>非原子</b>：同一个目标下标必须只被一个线程写。
        ''' 并行分块时请让每个线程持有独立的目标数组，最后再合并。
        ''' </remarks>
        Public Shared Sub ScatterAdd(values As Double(), indices As Integer(), start As Integer, ends As Integer, dst As Double())
            Call CheckNull(values, NameOf(values))
            Call CheckNull(indices, NameOf(indices))
            Call CheckRange(indices.Length, start, ends)
            Call CheckNull(dst, NameOf(dst))

            Dim n As Integer = ends - start

            If values.Length < n Then
                Throw New ArgumentException($"the value array is too small: {values.Length} < {n}!", NameOf(values))
            End If

            Dim k As Integer = 0

            For i As Integer = start To ends - 1
                Dim index As Integer = indices(i)

                If index < 0 OrElse index >= dst.Length Then
                    Throw New ArgumentOutOfRangeException(NameOf(indices), $"index {index} is out of the destination range [0, {dst.Length})!")
                End If

                dst(index) += values(k)
                k += 1
            Next
        End Sub

        ''' <summary>
        ''' 按下标列表整数累加：<c>dst(indices(start + k)) += values(k)</c>（直方图 bin 计数）。
        ''' </summary>
        Public Shared Sub ScatterAdd(values As Integer(), indices As Integer(), start As Integer, ends As Integer, dst As Integer())
            Call CheckNull(values, NameOf(values))
            Call CheckNull(indices, NameOf(indices))
            Call CheckRange(indices.Length, start, ends)
            Call CheckNull(dst, NameOf(dst))

            Dim n As Integer = ends - start

            If values.Length < n Then
                Throw New ArgumentException($"the value array is too small: {values.Length} < {n}!", NameOf(values))
            End If

            Dim k As Integer = 0

            For i As Integer = start To ends - 1
                Dim index As Integer = indices(i)

                If index < 0 OrElse index >= dst.Length Then
                    Throw New ArgumentOutOfRangeException(NameOf(indices), $"index {index} is out of the destination range [0, {dst.Length})!")
                End If

                dst(index) += values(k)
                k += 1
            Next
        End Sub

#End Region

#Region "reduce over indices"

        ''' <summary>
        ''' 按下标列表求和：<c>SUM(src(indices(i)))</c>。
        ''' </summary>
        Public Shared Function SumOverIndices(src As Double(), indices As Integer()) As Double
            If indices Is Nothing Then Throw New ArgumentNullException(NameOf(indices))

            Return SumOverIndices(src, indices, 0, indices.Length)
        End Function

        ''' <summary>
        ''' 按下标列表区间求和。
        ''' </summary>
        Public Shared Function SumOverIndices(src As Double(), indices As Integer(), start As Integer, ends As Integer) As Double
            Call CheckNull(src, NameOf(src))
            Call CheckNull(indices, NameOf(indices))
            Call CheckRange(indices.Length, start, ends)

            ' 4 路累加器：即使下标随机访问无法向量化，也要打断浮点加法依赖链
            Dim acc0 As Double = 0, acc1 As Double = 0, acc2 As Double = 0, acc3 As Double = 0
            Dim i As Integer = start
            Dim last4 As Integer = ends - 4

            Do While i <= last4
                acc0 += src(indices(i))
                acc1 += src(indices(i + 1))
                acc2 += src(indices(i + 2))
                acc3 += src(indices(i + 3))
                i += 4
            Loop

            Do While i < ends
                acc0 += src(indices(i))
                i += 1
            Loop

            Return (acc0 + acc1) + (acc2 + acc3)
        End Function

        ''' <summary>
        ''' 按下标列表求均值（邻域质心）：<c>SUM(src(indices(i))) / n</c>。
        ''' </summary>
        Public Shared Function MeanOverIndices(src As Double(), indices As Integer(), start As Integer, ends As Integer) As Double
            Dim n As Integer = ends - start

            If n <= 0 Then Return 0.0

            Return SumOverIndices(src, indices, start, ends) / n
        End Function

        ''' <summary>
        ''' 三维邻域质心（SoA）：一次遍历同时累加三条轴。
        ''' </summary>
        Public Shared Function Mean3Over(x As Double(), y As Double(), z As Double(),
                                         indices As Integer(), start As Integer, ends As Integer) As Vector3Value
            Call CheckNull(x, NameOf(x))
            Call CheckNull(y, NameOf(y))
            Call CheckNull(z, NameOf(z))
            Call CheckNull(indices, NameOf(indices))
            Call CheckRange(indices.Length, start, ends)

            Dim n As Integer = ends - start
            If n <= 0 Then Return New Vector3Value(0, 0, 0)

            Dim sx As Double = 0, sy As Double = 0, sz As Double = 0

            For i As Integer = start To ends - 1
                Dim index As Integer = indices(i)

                sx += x(index)
                sy += y(index)
                sz += z(index)
            Next

            Return New Vector3Value(sx / n, sy / n, sz / n)
        End Function

        ''' <summary>
        ''' 邻域到查询点的平方距离之和（SoA）：
        ''' <c>SUM(|v(indices(i)) - q|²)</c>。
        ''' </summary>
        ''' <remarks>
        ''' ICP 的对应点距离、FPFH 的邻域半径加权、特征尺度估计都会用到这里；
        ''' 内层使用 4 路累加器与手工展开，避免委托/迭代器开销。
        ''' </remarks>
        Public Shared Function DistanceSquared3Over(x As Double(), y As Double(), z As Double(),
                                                    indices As Integer(), start As Integer, ends As Integer,
                                                    qx As Double, qy As Double, qz As Double) As Double
            Call CheckNull(x, NameOf(x))
            Call CheckNull(y, NameOf(y))
            Call CheckNull(z, NameOf(z))
            Call CheckNull(indices, NameOf(indices))
            Call CheckRange(indices.Length, start, ends)

            Dim acc0 As Double = 0, acc1 As Double = 0, acc2 As Double = 0, acc3 As Double = 0
            Dim i As Integer = start
            Dim last4 As Integer = ends - 4

            Do While i <= last4
                Dim i0 As Integer = indices(i)
                Dim i1 As Integer = indices(i + 1)
                Dim i2 As Integer = indices(i + 2)
                Dim i3 As Integer = indices(i + 3)
                Dim dx0 As Double = x(i0) - qx, dy0 As Double = y(i0) - qy, dz0 As Double = z(i0) - qz
                Dim dx1 As Double = x(i1) - qx, dy1 As Double = y(i1) - qy, dz1 As Double = z(i1) - qz
                Dim dx2 As Double = x(i2) - qx, dy2 As Double = y(i2) - qy, dz2 As Double = z(i2) - qz
                Dim dx3 As Double = x(i3) - qx, dy3 As Double = y(i3) - qy, dz3 As Double = z(i3) - qz

                acc0 += dx0 * dx0 + dy0 * dy0 + dz0 * dz0
                acc1 += dx1 * dx1 + dy1 * dy1 + dz1 * dz1
                acc2 += dx2 * dx2 + dy2 * dy2 + dz2 * dz2
                acc3 += dx3 * dx3 + dy3 * dy3 + dz3 * dz3
                i += 4
            Loop

            Do While i < ends
                Dim index As Integer = indices(i)
                Dim dx As Double = x(index) - qx
                Dim dy As Double = y(index) - qy
                Dim dz As Double = z(index) - qz

                acc0 += dx * dx + dy * dy + dz * dz
                i += 1
            Loop

            Return (acc0 + acc1) + (acc2 + acc3)
        End Function

        ''' <summary>
        ''' 按下标列表求最小/最大值。
        ''' </summary>
        Public Shared Sub MinMaxOver(src As Double(), indices As Integer(), start As Integer, ends As Integer,
                                     ByRef min As Double, ByRef max As Double)
            Call CheckNull(src, NameOf(src))
            Call CheckNull(indices, NameOf(indices))
            Call CheckRange(indices.Length, start, ends)

            If start = ends Then
                min = 0.0
                max = 0.0

                Return
            End If

            Dim lo As Double = src(indices(start))
            Dim hi As Double = lo

            For i As Integer = start + 1 To ends - 1
                Dim value As Double = src(indices(i))

                If value < lo Then lo = value
                If value > hi Then hi = value
            Next

            min = lo
            max = hi
        End Sub

        ''' <summary>
        ''' 按“值下标”列表求最小/最大值（<c>src</c> 与 <c>indices</c> 等长，
        ''' 直接比较 <c>src(indices(i))</c> 的写法用于索引重排后的扫描）。
        ''' </summary>
        Public Shared Function MinOverIndices(src As Double(), indices As Integer(), start As Integer, ends As Integer) As Double
            Dim lo As Double, hi As Double

            Call MinMaxOver(src, indices, start, ends, lo, hi)

            Return lo
        End Function

        ''' <summary>
        ''' 按“值下标”列表求最大值。
        ''' </summary>
        Public Shared Function MaxOverIndices(src As Double(), indices As Integer(), start As Integer, ends As Integer) As Double
            Dim lo As Double, hi As Double

            Call MinMaxOver(src, indices, start, ends, lo, hi)

            Return hi
        End Function

#End Region

    End Class
End Namespace
