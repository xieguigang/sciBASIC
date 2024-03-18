Imports System.Runtime.CompilerServices

Namespace ComponentModel.Collection

    <HideModuleName>
    Public Module PriorityQueueCreator

        Public Function CreateEmptyPriorityQueue(Of T As IComparable(Of T))() As PriorityQueue(Of T)
            Return New PriorityQueue(Of T)(Function(a, b) a.CompareTo(b) < 0)
        End Function

        <Extension>
        Public Sub AddAll(Of T)([set] As HashSet(Of T), range As IEnumerable(Of T))
            For Each item As T In range
                Call [set].Add(item)
            Next
        End Sub
    End Module
End Namespace