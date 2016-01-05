Imports System.Collections.Generic
Imports System.Text

Namespace Dijkstra.PQDijkstra

    Public Structure HeapNode
        Public Sub New(i As Integer, w As Single)
            index = i

            weight = w
        End Sub
        Public index As Integer
        Public weight As Single
    End Structure
End Namespace