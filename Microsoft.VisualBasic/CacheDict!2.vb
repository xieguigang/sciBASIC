Imports System
Imports System.Collections.Generic
Imports System.Runtime.InteropServices

Friend Class CacheDict(Of TKey, TValue)
    ' Methods
    Friend Sub New(maxSize As Integer)
        Me._dict = New Dictionary(Of TKey, KeyInfo(Of TKey, TValue))
        Me._list = New LinkedList(Of TKey)
        Me._maxSize = maxSize
    End Sub

    Friend Sub Add(key As TKey, value As TValue)
        Dim info As New KeyInfo(Of TKey, TValue)
        If Me._dict.TryGetValue(key, info) Then
            Me._list.Remove(info.List)
        ElseIf (Me._list.Count = Me._maxSize) Then
            Dim last As LinkedListNode(Of TKey) = Me._list.Last
            Me._list.RemoveLast()
            Me._dict.Remove(last.Value)
        End If
        Dim node As New LinkedListNode(Of TKey)(key)
        Me._list.AddFirst(node)
        Me._dict.Item(key) = New KeyInfo(Of TKey, TValue)(value, node)
    End Sub

    Friend Function TryGetValue(key As TKey, <Out> ByRef value As TValue) As Boolean
        Dim info As New KeyInfo(Of TKey, TValue)
        If Me._dict.TryGetValue(key, info) Then
            Dim list As LinkedListNode(Of TKey) = info.List
            If (Not list.Previous Is Nothing) Then
                Me._list.Remove(list)
                Me._list.AddFirst(list)
            End If
            value = info.Value
            Return True
        End If
        value = CType(Nothing, TValue)
        Return False
    End Function


    ' Fields
    Private ReadOnly _dict As Dictionary(Of TKey, KeyInfo(Of TKey, TValue))
    Private ReadOnly _list As LinkedList(Of TKey)
    Private ReadOnly _maxSize As Integer

    ' Nested Types
    <StructLayout(LayoutKind.Sequential)>
    Private Structure KeyInfo
        Friend ReadOnly Value As TValue
        Friend ReadOnly List As LinkedListNode(Of TKey)
        Friend Sub New(v As TValue, l As LinkedListNode(Of TKey))
            Me = DirectCast(New KeyInfo(Of TKey, TValue), KeyInfo)
            Me.Value = v
            Me.List = l
        End Sub
    End Structure
End Class


