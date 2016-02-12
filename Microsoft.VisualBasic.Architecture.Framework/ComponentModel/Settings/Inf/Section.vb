Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Xml.Serialization

Namespace ComponentModel.Settings.Inf

    Public Class Section

        <XmlAttribute> Public Property Name As String
        <XmlElement> Public Property Items As ComponentModel.KeyValuePair()
            Get
                Return _innerHash.Values.ToArray
            End Get
            Set(value As ComponentModel.KeyValuePair())
                If value Is Nothing Then
                    value = New ComponentModel.KeyValuePair() {}
                End If

                _innerHash = value.ToDictionary(Function(obj) obj.Key.ToLower)
            End Set
        End Property

        Dim _innerHash As Dictionary(Of String, ComponentModel.KeyValuePair)

        Public Function GetValue(Key As String) As String
            Key = Key.ToLower

            If _innerHash.ContainsKey(Key) Then
                Return _innerHash(Key).Value
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' 不存在则自动添加
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="value"></param>
        Public Sub SetValue(Name As String, value As String)
            Dim KeyFind As String = Name.ToLower

            If _innerHash.ContainsKey(KeyFind) Then
                Call _innerHash.Remove(KeyFind)
            End If

            Call _innerHash.Add(KeyFind, New KeyValuePair(Name, value))
        End Sub
    End Class
End Namespace