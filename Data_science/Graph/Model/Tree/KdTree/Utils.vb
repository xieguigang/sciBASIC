
Imports System.Runtime.CompilerServices

Namespace KdTree

    Public Module Utils

        ''' <summary>
        ''' add new item with check duplicated item
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="list"></param>
        ''' <param name="item"></param>
        <Extension>
        Friend Sub Push(Of T)(list As List(Of KdNodeHeapItem(Of T)), item As KdNodeHeapItem(Of T))
            If list.IndexOf(item) = -1 Then
                Call list.Add(item)
            End If
        End Sub
    End Module
End Namespace