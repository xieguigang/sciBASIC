Public MustInherit Class MemoryCollection(Of T)
    Implements IEnumerable(Of KeyValuePair(Of String, T))

    Protected ReadOnly _InnerObjectDictionary As Dictionary(Of String, T) = New Dictionary(Of String, T)
    ''' <summary>
    ''' [Cache]
    ''' </summary>
    ''' <remarks></remarks>
    Protected ReadOnly _ObjectCacheList As List(Of String) = New List(Of String)

    Public ReadOnly Property Objects As String()
        Get
            Return _ObjectCacheList.ToArray
        End Get
    End Property

    Public ReadOnly Property DictData As Dictionary(Of String, T)
        Get
            Return _InnerObjectDictionary
        End Get
    End Property

    Protected Function Add(Name As String, value As T) As Integer
        Name = Name.ToLower

        If _InnerObjectDictionary.ContainsKey(Name) Then
            Call _InnerObjectDictionary.Remove(Name)
        End If

        Call _InnerObjectDictionary.Add(Name, value)
        Call _ObjectCacheList.Clear()
        Call _ObjectCacheList.AddRange((From strName In _InnerObjectDictionary.Keys Select strName Order By Len(strName) Descending).ToArray)

        Return 0
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, T)) Implements IEnumerable(Of KeyValuePair(Of String, T)).GetEnumerator
        For Each item In _InnerObjectDictionary
            Yield item
        Next
    End Function

    Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
