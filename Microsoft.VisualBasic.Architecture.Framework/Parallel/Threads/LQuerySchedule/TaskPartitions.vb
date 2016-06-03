Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Parallel.Linq

    ''' <summary>
    ''' 对大量的短时间的任务进行分区的操作是在这里完成的
    ''' </summary>
    Public Module TaskPartitions

        ''' <summary>
        ''' Performance the partitioning operation on the input sequence.
        ''' (请注意，这个函数适用于数量非常多的序列。对所输入的序列进行分区操作，<paramref name="parTokens"/>函数参数是每一个分区里面的元素的数量)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="parTokens"></param>
        ''' <returns></returns>
        ''' <remarks>对于数量较少的序列，可以使用<see cref="Extensions.SplitIterator(Of T)(IEnumerable(Of T), Integer)"/>进行分区操作，
        ''' 该函数使用数组的<see cref="Array.ConstrainedCopy(Array, Integer, Array, Integer, Integer)"/>方法进行分区复制，效率较高
        ''' 
        ''' 由于本函数需要处理大量的数据，使用Array的方法会内存占用较厉害，所以在这里更改为List操作以降低内存的占用
        ''' </remarks>
        <Extension>
        Public Iterator Function SplitIterator(Of T)(source As IEnumerable(Of T), parTokens As Integer) As IEnumerable(Of T())
            Dim buf As New List(Of T)
            Dim n As Integer = 0
            Dim parts As Integer

            For Each x As T In source
                If n = parTokens Then
                    Yield buf.ToArray
                    buf.Clear()
                    n = 0
                    parts += 1
                End If

                buf.Add(x)
                n += 1
            Next

            If buf.Count > 0 Then
                Yield buf.ToArray
            End If

            Call $"Large data set data partitioning(partitions:={parts}) jobs done!".__DEBUG_ECHO
        End Function

        ''' <summary>
        ''' 进行分区之后返回一个长时间的任务组合
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Iterator Function Partitioning(Of T, out)(source As IEnumerable(Of T),
                                                         parts As Integer,
                                                         task As Func(Of T, out)) As IEnumerable(Of Func(Of out()))

            Dim buf As IEnumerable(Of T()) = source.SplitIterator(parts)

            For Each part As T() In buf
                Yield AddressOf New __taskHelper(Of T, out) With {
                    .source = part,
                    .task = task
                }.Invoke
            Next
        End Function

        Private Structure __taskHelper(Of T, out)

            Dim task As Func(Of T, out)
            Dim source As T()

            Public Overrides Function ToString() As String
                Return task.ToString
            End Function

            Public Function Invoke() As out()
                Dim __task As Func(Of T, out) = task
                Dim LQuery As out() =
                    LinqAPI.Exec(Of out) <= From x As T
                                            In source
                                            Select __task(x)
                Return LQuery
            End Function
        End Structure
    End Module
End Namespace