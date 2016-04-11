Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Public Module KeyValuePairExtensions

    ''' <summary>
    ''' Creates a <see cref="System.Collections.Generic.Dictionary"/>`2 from an <see cref="System.Collections.Generic.IEnumerable"/>`1
    ''' according to a specified key selector function.
    ''' </summary>
    ''' <typeparam name="T">Unique identifier provider</typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ToDictionary(Of T As sIdEnumerable)(source As IEnumerable(Of T)) As Dictionary(Of T)
        Dim hash As Dictionary(Of T) = New Dictionary(Of T)
        Dim i As Integer = 0
        Try
            For Each item As T In source
                Call hash.Add(item.Identifier, item)
                i += 1
            Next
        Catch ex As Exception
            ex = New Exception("Identifier -> [ " & source(i).Identifier & " ]", ex)
            Throw ex
        End Try

        Return hash
    End Function

    <Extension> Public Function Add(Of TKey, TValue)(ByRef list As List(Of KeyValuePair(Of TKey, TValue)), key As TKey, value As TValue) As List(Of KeyValuePair(Of TKey, TValue))
        If list Is Nothing Then
            list = New List(Of KeyValuePair(Of TKey, TValue))
        End If
        Call list.Add(New KeyValuePair(Of TKey, TValue)(key, value))
        Return list
    End Function

    ''' <summary>
    ''' Adds an object to the end of the List`1.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="list"></param>
    ''' <param name="key"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <Extension> Public Function Add(Of TKey, TValue)(ByRef list As List(Of KeyValuePairObject(Of TKey, TValue)), key As TKey, value As TValue) As List(Of KeyValuePairObject(Of TKey, TValue))
        If list Is Nothing Then
            list = New List(Of KeyValuePairObject(Of TKey, TValue))
        End If
        Call list.Add(New KeyValuePairObject(Of TKey, TValue)(key, value))
        Return list
    End Function
End Module
