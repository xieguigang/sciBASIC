#Region "Microsoft.VisualBasic::3671c550d58132fc3b4056767b3e8568, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\CapacityQueue.vb"

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

    '     Class CapacityQueue
    ' 
    '         Properties: Capacity
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Enqueue, GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Clear
    ' 
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection

    Public Class CapacityQueue(Of T)
        Implements IEnumerable(Of T)

        ReadOnly queue As Queue(Of T)

        Public ReadOnly Property Capacity As Integer

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="capacity">The initial number of elements that the System.Collections.Generic.Queue`1 can
        ''' contain.</param>
        Sub New(capacity As Integer)
            queue = New Queue(Of T)(capacity)
        End Sub

        Public Function Enqueue(x As T) As T
            Dim o As T

            Call queue.Enqueue(x)

            If queue.Count = Capacity - 1 Then
                o = queue.Dequeue()
            Else
                o = queue.Peek
            End If

            Return o
        End Function

        Public Sub Clear()
            Call queue.Clear()
        End Sub

        Public Overrides Function ToString() As String
            Return GetType(T()).GetObjectJson(Me.ToArray)
        End Function

        Public Overloads Shared Operator +(q As CapacityQueue(Of T), x As T) As CapacityQueue(Of T)
            Call q.Enqueue(x)
            Return q
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In queue
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
