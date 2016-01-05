Imports System
Imports System.Collections.Generic

Namespace Microsoft.VisualBasic.CompilerServices
    Friend NotInheritable Class CacheSet(Of T)
        ' Methods
        Friend Sub New(maxSize As Integer)
            Me._dict = New Dictionary(Of T, LinkedListNode(Of T))
            Me._list = New LinkedList(Of T)
            Me._maxSize = maxSize
        End Sub

        Friend Function GetExistingOrAdd(key As T) As T
            Dim obj2 As Object = Me
            SyncLock obj2
                Dim node As LinkedListNode(Of T) = Nothing
                If Me._dict.TryGetValue(key, node) Then
                    If (Not node.Previous Is Nothing) Then
                        Me._list.Remove(node)
                        Me._list.AddFirst(node)
                    End If
                    Return node.Value
                End If
                If (Me._dict.Count = Me._maxSize) Then
                    Me._dict.Remove(Me._list.Last.Value)
                    Me._list.RemoveLast()
                End If
                node = New LinkedListNode(Of T)(key)
                Me._dict.Add(key, node)
                Me._list.AddFirst(node)
                Return key
            End SyncLock
        End Function


        ' Fields
        Private ReadOnly _dict As Dictionary(Of T, LinkedListNode(Of T))
        Private ReadOnly _list As LinkedList(Of T)
        Private ReadOnly _maxSize As Integer
    End Class
End Namespace

