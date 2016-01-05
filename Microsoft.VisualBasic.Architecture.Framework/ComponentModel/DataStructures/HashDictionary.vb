Imports System.Collections.ObjectModel

Namespace ComponentModel.Collection.Generic

    ''' <summary>
    ''' The key of the dictionary is string value and the keys is not sensitive to the character case.
    ''' (字典的键名为字符串，大小写不敏感，行为和哈希表类型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Class HashDictionary(Of T) : Implements System.IDisposable, System.Collections.Generic.IDictionary(Of String, T)

        Protected _InternalHashDictionary As Dictionary(Of String, T)
        Protected _InternalKeysHash As Dictionary(Of String, String)

        Sub New(DataChunk As System.Collections.Generic.IDictionary(Of String, T))
            _InternalHashDictionary = New System.Collections.Generic.Dictionary(Of String, T)
            _InternalKeysHash = New System.Collections.Generic.Dictionary(Of String, String)

            If Not DataChunk.IsNullOrEmpty Then
                For Each ItemObject In DataChunk
                    Call Add(ItemObject)
                Next
            End If
        End Sub

#Region "Implements System.IDisposable, Generic.IDictionary(Of String, T)"

        Public Sub Add(item As System.Collections.Generic.KeyValuePair(Of String, T)) Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, T)).Add
            Dim Key As String = item.Key
            Me(Key) = item.Value
        End Sub

        Public Sub Clear() Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, T)).Clear
            Call _InternalKeysHash.Clear()
            Call _InternalHashDictionary.Clear()
        End Sub

        Public Function Contains(item As System.Collections.Generic.KeyValuePair(Of String, T)) As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, T)).Contains
            Dim value = Me(item.Key)
            If item.Value Is Nothing AndAlso value Is Nothing Then
                Return True
            ElseIf value Is Nothing Then
                Return False
            ElseIf Object.Equals(item.Value, value) Then
                Return True
            End If

            Return False
        End Function

        Public ReadOnly Property Count As Integer Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, T)).Count
            Get
                Return _InternalKeysHash.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, T)).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As System.Collections.Generic.KeyValuePair(Of String, T)) As Boolean Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, T)).Remove
            Dim Key As String = item.Key.ToLower
            Call _InternalHashDictionary.Remove(Key)
            Return _InternalKeysHash.Remove(Key)
        End Function

        Public Sub Add(key As String, value As T) Implements System.Collections.Generic.IDictionary(Of String, T).Add
            Call Add(New KeyValuePair(Of String, T)(key, value))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="key">大小写不敏感</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ContainsKey(key As String) As Boolean Implements System.Collections.Generic.IDictionary(Of String, T).ContainsKey
            Return _InternalKeysHash.ContainsKey(key.ToLower)
        End Function

        ''' <summary>
        ''' 添加<see cref="Add"></see>和替换操作主要在这里进行
        ''' </summary>
        ''' <param name="key">大小写不敏感</param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public Property Item(key As String) As T Implements System.Collections.Generic.IDictionary(Of String, T).Item
            Get
                key = key.ToLower
                If _InternalKeysHash.ContainsKey(key) Then
                    Return _InternalHashDictionary(key)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As T)
                Dim QueryKey = key.ToLower

                If _InternalKeysHash.ContainsKey(QueryKey) Then
                    _InternalHashDictionary(QueryKey) = value
                Else
                    Call _InternalHashDictionary.Add(QueryKey, value)
                    Call _InternalKeysHash.Add(QueryKey, key)
                End If
            End Set
        End Property

        Public Sub CopyTo(array() As System.Collections.Generic.KeyValuePair(Of String, T), arrayIndex As Integer) Implements System.Collections.Generic.ICollection(Of System.Collections.Generic.KeyValuePair(Of String, T)).CopyTo
            Dim LQuery = (From item In _InternalKeysHash Select New KeyValuePair(Of String, T)(item.Value, _InternalHashDictionary(item.Key))).ToArray
            Call System.Array.ConstrainedCopy(LQuery, 0, array, arrayIndex, array.Length - arrayIndex)
        End Sub

        Public ReadOnly Property Keys As System.Collections.Generic.ICollection(Of String) Implements System.Collections.Generic.IDictionary(Of String, T).Keys
            Get
                Return _InternalKeysHash.Values
            End Get
        End Property

        Public Function Remove(key As String) As Boolean Implements System.Collections.Generic.IDictionary(Of String, T).Remove
            key = key.ToLower
            If Me._InternalKeysHash.ContainsKey(key) Then
                Call _InternalKeysHash.Remove(key)
                Call _InternalHashDictionary.Remove(key)

                Return True
            Else
                Return False
            End If
        End Function

        Public Function TryGetValue(key As String, ByRef value As T) As Boolean Implements System.Collections.Generic.IDictionary(Of String, T).TryGetValue
            value = Me(key)
            Return _InternalKeysHash.ContainsKey(key.ToLower)
        End Function

        Public ReadOnly Property Values As System.Collections.Generic.ICollection(Of T) Implements System.Collections.Generic.IDictionary(Of String, T).Values
            Get
                Return _InternalHashDictionary.Values
            End Get
        End Property

        Public Iterator Function GetEnumerator1() As System.Collections.Generic.IEnumerator(Of System.Collections.Generic.KeyValuePair(Of String, T)) Implements System.Collections.Generic.IEnumerable(Of System.Collections.Generic.KeyValuePair(Of String, T)).GetEnumerator
            For Each ItemObject In Me._InternalHashDictionary
                Yield New KeyValuePair(Of String, T)(Me._InternalKeysHash(ItemObject.Key), ItemObject.Value)
            Next
        End Function

        Public Iterator Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Yield GetEnumerator1()
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

#End Region
    End Class
End Namespace