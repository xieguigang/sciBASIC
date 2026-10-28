Imports System.Runtime.CompilerServices
Imports System.Collections.Generic
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' 选择类内核：在一次线性扫描（或 O(n·log k) 的有界堆）内找到最小/最大元素及其下标。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这个类存在的理由是：聚类算法的热路径里到处都是
    ''' <c>values.OrderBy(Function(d) d).First()</c> 与 <c>values.OrderBy(Function(d) d).Take(k)</c>。
    ''' 前者为了取一个最小值付出了 O(n·log n) 的全排序加比较器装箱，后者在
    ''' <c>n = 10^4..10^5</c>、<c>k = 5..50</c> 时同样是数十倍的无谓开销。
    ''' 这里的 <see cref="ArgMin(Double(), Integer, Integer)"/> 与
    ''' <see cref="TopKIndices(Double(), Integer)"/> 分别把这两类调用降回 O(n) 与 O(n·log k)。
    ''' </para>
    ''' <para>
    ''' <b>确定性</b>：所有函数在数值相等时都按下标升序 tie-break，
    ''' 与 <c>OrderBy(...).ThenBy(index)</c> 的结果完全一致，
    ''' 因此把既有 LINQ 排序改写成这些内核不会改变聚类结果。
    ''' </para>
    ''' <para>
    ''' <b>NaN</b>：NaN 被视为“最差”的值（比任何有限值都大），
    ''' 这样含 NaN 的距离不会意外被选为最近邻。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdSelect

        Private Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function CheckNotNull(v As Double(), name As String) As Integer
            If v Is Nothing Then
                Throw New ArgumentNullException(name)
            End If

            Return v.Length
        End Function

       ''' <summary>
        ''' 归一化比较键：NaN 映射为 <see cref="Double.PositiveInfinity"/>（视为最差）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Key(value As Double) As Double
            If Double.IsNaN(value) Then
                Return Double.PositiveInfinity
            End If

            Return value
        End Function

#Region "argmin / argmax"

        ''' <summary>
        ''' 最小值的下标（第一个匹配项）。空数组会抛出 <see cref="ArgumentException"/>。
        ''' </summary>
        Public Shared Function ArgMin(v As Double()) As Integer
            Dim len As Integer = CheckNotNull(v, NameOf(v))

            If len = 0 Then
                Throw New ArgumentException("the input vector can not be empty for a select operation!")
            End If

            Return ArgMin(v, 0, len)
        End Function

        ''' <summary>
        ''' 在 <c>[start, ends)</c> 区间内找最小值的下标：单趟扫描，数值相等时取较小下标。
        ''' </summary>
        Public Shared Function ArgMin(v As Double(), start As Integer, ends As Integer) As Integer
            Dim len As Integer = CheckNotNull(v, NameOf(v))

            If start < 0 OrElse ends > len OrElse start >= ends Then
                Throw New ArgumentOutOfRangeException($"invalid range [{start}, {ends}) for a vector of length {len}!")
            End If

            Dim best As Integer = start
            Dim bestValue As Double = Key(v(start))

            For i As Integer = start + 1 To ends - 1
                Dim value As Double = Key(v(i))

                If value < bestValue Then
                    bestValue = value
                    best = i
                End If
            Next

            Return best
        End Function

        ''' <summary>
        ''' 最大值的下标（第一个匹配项）。空数组会抛出 <see cref="ArgumentException"/>。
        ''' </summary>
        Public Shared Function ArgMax(v As Double()) As Integer
            Dim len As Integer = CheckNotNull(v, NameOf(v))

            If len = 0 Then
                Throw New ArgumentException("the input vector can not be empty for a select operation!")
            End If

            Return ArgMax(v, 0, len)
        End Function

        ''' <summary>
        ''' 在 <c>[start, ends)</c> 区间内找最大值的下标：单趟扫描，数值相等时取较小下标。
        ''' </summary>
        Public Shared Function ArgMax(v As Double(), start As Integer, ends As Integer) As Integer
            Dim len As Integer = CheckNotNull(v, NameOf(v))

            If start < 0 OrElse ends > len OrElse start >= ends Then
                Throw New ArgumentOutOfRangeException($"invalid range [{start}, {ends}) for a vector of length {len}!")
            End If

            Dim best As Integer = start
            Dim bestValue As Double = Key(v(start))

            For i As Integer = start + 1 To ends - 1
                Dim value As Double = Key(v(i))

                If value > bestValue Then
                    bestValue = value
                    best = i
                End If
            Next

            Return best
        End Function

        ''' <summary>
        ''' 单趟同时求最小值下标与最小值。
        ''' </summary>
        Public Shared Function ArgMin(v As Double(), ByRef value As Double) As Integer
            Dim index As Integer = ArgMin(v)

            value = v(index)

            Return index
        End Function

        ''' <summary>
        ''' 单趟同时求最大值下标与最大值。
        ''' </summary>
        Public Shared Function ArgMax(v As Double(), ByRef value As Double) As Integer
            Dim index As Integer = ArgMax(v)

            value = v(index)

            Return index
        End Function

#End Region

#Region "top-k"

        ''' <summary>
        ''' 返回最小的 <paramref name="k"/> 个元素的下标，按 (值升序, 下标升序) 排列。
        ''' </summary>
        ''' <param name="v">待选择的数值向量</param>
        ''' <param name="k">需要的元素个数；<c>k &lt;= 0</c> 返回空数组，<c>k &gt;= n</c> 返回全排列</param>
        ''' <remarks>
        ''' 实现是"大小为 k 的有界最大堆"：每个元素只与堆顶比较一次，
        ''' 总代价 <c>O(n·log k)</c>、额外内存 <c>O(k)</c>，
        ''' 相比 <c>OrderBy(...).Take(k)</c> 的 <c>O(n·log n)</c> 在 <c>n</c> 大而 <c>k</c> 小时优势明显。
        ''' </remarks>
        Public Shared Function TopKIndices(v As Double(), k As Integer) As Integer()
            Dim len As Integer = CheckNotNull(v, NameOf(v))

            If k <= 0 OrElse len = 0 Then
                Return Array.Empty(Of Integer)()
            End If

            If k >= len Then
                ' 全部都要：退化成一次全排序，但比较键是 (值, 下标) 的确定序
                Dim all As Integer() = New Integer(len - 1) {}
                Dim keys As Double() = New Double(len - 1) {}

                For i As Integer = 0 To len - 1
                    all(i) = i
                    keys(i) = Key(v(i))
                Next

                Array.Sort(keys, all)

                Return all
            End If

            ' 优先级 ( -值, -下标 )：Dequeue 得到"当前已选元素里值最大的那个"，
            ' 同值时下标更大的先出局 —— 最终保留的就是 (值,下标) 字典序最小的 k 个。
            Dim heap As New PriorityQueue(Of Integer, (Double, Integer))(k)

            For i As Integer = 0 To len - 1
                Dim priority As (Double, Integer) = (-Key(v(i)), -i)

                If heap.Count < k Then
                    heap.Enqueue(i, priority)
                Else
                    Dim topElement As Integer = 0
                    Dim topPriority As (Double, Integer)

                    Call heap.TryPeek(topElement, topPriority)

                    ' VB 没有为 ValueTuple 定义 < 运算符，这里显式做字典序比较
                    Dim better As Boolean =
                        priority.Item1 < topPriority.Item1 OrElse
                        (priority.Item1 = topPriority.Item1 AndAlso priority.Item2 < topPriority.Item2)

                    If better Then
                        heap.Dequeue()
                        heap.Enqueue(i, priority)
                    End If
                End If
            Next

            Dim result As Integer() = New Integer(heap.Count - 1) {}
            Dim position As Integer = result.Length - 1

            While heap.Count > 0
                result(position) = heap.Dequeue()
                position -= 1
            End While

            Return result
        End Function

        ''' <summary>
        ''' 返回最小的 <paramref name="k"/> 个元素的值，按升序排列（同值保持下标升序）。
        ''' </summary>
        Public Shared Function TopKValues(v As Double(), k As Integer) As Double()
            Dim indices As Integer() = TopKIndices(v, k)
            Dim result As Double() = New Double(indices.Length - 1) {}

            For i As Integer = 0 To indices.Length - 1
                result(i) = v(indices(i))
            Next

            Return result
        End Function

        ''' <summary>
        ''' 判断 <paramref name="v"/> 中是否存在严格小于 <paramref name="value"/> 的元素（单趟、可提前退出）。
        ''' </summary>
        Public Shared Function AnyLessThan(v As Double(), value As Double) As Boolean
            Dim len As Integer = CheckNotNull(v, NameOf(v))

            For i As Integer = 0 To len - 1
                If v(i) < value Then Return True
            Next

            Return False
        End Function

#End Region
    End Class
End Namespace
