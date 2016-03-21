Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.Collection

    Public Class CapacityQueue(Of T) : Inherits Language.ClassObject
        Implements IEnumerable(Of T)

        ReadOnly _queue As Queue(Of T)

        Public ReadOnly Property Capacity As Integer

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="capacity">The initial number of elements that the System.Collections.Generic.Queue`1 can
        ''' contain.</param>
        Sub New(capacity As Integer)
            _queue = New Queue(Of T)(capacity)
        End Sub

        Public Function Enqueue(x As T) As T
            Dim o As T

            Call _queue.Enqueue(x)

            If _queue.Count = Capacity - 1 Then
                o = _queue.Dequeue()
            Else
                o = _queue.Peek
            End If

            Return o
        End Function

        Public Sub Clear()
            Call _queue.Clear()
        End Sub

        Public Overrides Function ToString() As String
            Return Serialization.GetJson(Me.ToArray, GetType(T()))
        End Function

        Public Overloads Shared Operator +(q As CapacityQueue(Of T), x As T) As CapacityQueue(Of T)
            Call q.Enqueue(x)
            Return q
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In _queue
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace