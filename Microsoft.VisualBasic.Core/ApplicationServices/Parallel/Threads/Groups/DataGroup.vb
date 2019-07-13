#Region "Microsoft.VisualBasic::19568ecf19640f9af2343dfceba0d3f4, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\Groups\DataGroup.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class TaggedGroupData
    ' 
    '         Properties: Tag
    ' 
    '         Function: ToString
    ' 
    '     Class ParallelGroup
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ParallelTask, SequentialTask
    ' 
    '     Class GroupResult
    ' 
    '         Properties: Count, Group, Tag
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    '         Sub: (+2 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Parallel.Threads

Namespace Parallel

    Public MustInherit Class TaggedGroupData(Of T)
        Public Overridable Property Tag As T

        Public Overrides Function ToString() As String
            Return Tag.ToString
        End Function
    End Class

    Public Class ParallelGroup(Of TOut)

        ReadOnly blocks As Func(Of SeqValue(Of TOut()))()

        Sub New(tasks As IEnumerable(Of Func(Of TOut)), Optional parallelism%? = Nothing)
            Dim taskPool = tasks.ToArray
            Dim num_threads% = parallelism Or LQuerySchedule.DefaultConfig
            Dim partionTokens% = TaskPartitions.PartTokens(taskPool.Length, num_threads)
            Dim blocks = taskPool.SplitIterator(partionTokens).ToArray

            Me.blocks = blocks _
                .Select(Function(block, i) As Func(Of SeqValue(Of TOut()))
                            Return Function() As SeqValue(Of TOut())
                                       Return New SeqValue(Of TOut()) With {
                                           .i = i,
                                           .value = block _
                                               .Select(Function(task) task()) _
                                               .ToArray
                                       }
                                   End Function
                        End Function) _
                .ToArray
        End Sub

        Public Iterator Function SequentialTask() As IEnumerable(Of TOut)
            For Each block As Func(Of SeqValue(Of TOut())) In blocks
                For Each x As TOut In block().value
                    Yield x
                Next
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ParallelTask() As IEnumerable(Of TOut)
            Return blocks _
                .BatchTask _
                .OrderBy(Function(block) block.i) _
                .Select(Function(block) block.value) _
                .IteratesALL
        End Function
    End Class

    ''' <summary>
    ''' 分组操作的结果
    ''' </summary>
    ''' <typeparam name="T">Group的元素的类型</typeparam>
    ''' <typeparam name="Itag">Group的Key的类型</typeparam>
    Public Class GroupResult(Of T, Itag) : Inherits TaggedGroupData(Of Itag)
        Implements IEnumerable(Of T)
        Implements IGrouping(Of Itag, T)

        Public Overrides Property Tag As Itag Implements IGrouping(Of Itag, T).Key
        Public Property Group As T()
            Get
                Return __list.ToArray
            End Get
            Set(value As T())
                Call __list.Clear()

                If Not value.IsNullOrEmpty Then
                    Call __list.AddRange(value)
                End If
            End Set
        End Property

        ReadOnly __list As New List(Of T)

        Public ReadOnly Property Count As Integer
            Get
                Return Group.Length
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(tag As Itag, data As IEnumerable(Of T))
            Me.Tag = tag
            Me.Group = data.ToArray
        End Sub

        Public Sub Add(x As T)
            __list.Add(x)
        End Sub

        Public Sub Add(source As IEnumerable(Of T))
            __list.AddRange(source)
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For i As Integer = 0 To Group.Length - 1
                Yield Group(i)
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
