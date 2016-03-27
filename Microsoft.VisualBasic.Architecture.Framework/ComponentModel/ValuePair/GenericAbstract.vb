
Namespace ComponentModel.Collection.Generic

    ''' <summary>
    ''' Defines a key/value pair that can be set or retrieved.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <remarks></remarks>
    Public Interface IKeyValuePairObject(Of TKey, TValue)
        ''' <summary>
        ''' Gets the key in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Identifier As TKey
        ''' <summary>
        ''' Gets the value in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Value As TValue
    End Interface

    ''' <summary>
    ''' Defines a key/value pair that only can be retrieved.
    ''' </summary>
    ''' <typeparam name="TValue"></typeparam>
    ''' <remarks></remarks>
    Public Interface IReadOnlyDataSource(Of TValue)
        ReadOnly Property Identifier As String
        ReadOnly Property Value As TValue
    End Interface

    ''' <summary>
    ''' Defines a key/value pair that can be set or retrieved.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <remarks></remarks>
    Public Class KeyValuePairObject(Of TKey, TValue) : Implements IKeyValuePairObject(Of TKey, TValue)

        ''' <summary>
        ''' Gets the key in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property Key As TKey Implements IKeyValuePairObject(Of TKey, TValue).Identifier
        ''' <summary>
        ''' Gets the value in the key/value pair.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property Value As TValue Implements IKeyValuePairObject(Of TKey, TValue).Value

        Sub New()
        End Sub

        Sub New(KEY As TKey, VALUE As TValue)
            Me.Key = KEY
            Me.Value = VALUE
        End Sub

        Sub New(raw As KeyValuePair(Of TKey, TValue))
            Key = raw.Key
            Value = raw.Value
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} -> {1}", Key.ToString, Value.ToString)
        End Function

        Public Shared Function CreateObject(key As TKey, value As TValue) As KeyValuePairObject(Of TKey, TValue)
            Return New KeyValuePairObject(Of TKey, TValue) With {.Key = key, .Value = value}
        End Function

        Public Shared Widening Operator CType(args As Object()) As KeyValuePairObject(Of TKey, TValue)
            If args.IsNullOrEmpty Then
                Return New KeyValuePairObject(Of TKey, TValue)
            End If
            If args.Length = 1 Then
                Return New KeyValuePairObject(Of TKey, TValue) With {
                    .Key = DirectCast(args(Scan0), TKey)
                }
            End If

            Return New KeyValuePairObject(Of TKey, TValue) With {
                .Key = DirectCast(args(Scan0), TKey),
                .Value = DirectCast(args(1), TValue)
            }
        End Operator
    End Class

    Public Class KeyValuePairData(Of T) : Inherits KeyValuePairObject(Of String, String)
        Implements IKeyValuePairObject(Of String, String)

        Public Property DataObject As T
    End Class
End Namespace
