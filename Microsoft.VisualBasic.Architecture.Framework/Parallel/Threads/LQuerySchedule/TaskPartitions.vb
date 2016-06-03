Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Parallel.Linq

    ''' <summary>
    ''' 对大量的短时间的任务进行分区的操作是在这里完成的
    ''' </summary>
    Public Module TaskPartitions

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