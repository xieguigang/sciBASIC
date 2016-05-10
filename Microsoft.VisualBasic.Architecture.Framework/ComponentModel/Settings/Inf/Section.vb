Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Xml.Serialization

Namespace ComponentModel.Settings.Inf

    Public Class Section

        <XmlAttribute> Public Property Name As String
        <XmlElement> Public Property Items As HashValue()
            Get
                Return _innerHash.Values.ToArray
            End Get
            Set(value As HashValue())
                If value Is Nothing Then
                    value = New HashValue() {}
                End If

                _innerHash = New Dictionary(Of HashValue)(value.ToDictionary(Function(x) x.Identifier.ToLower))
            End Set
        End Property

        Dim _innerHash As Dictionary(Of HashValue)

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

            Call _innerHash.Add(KeyFind, New HashValue(Name, value))
        End Sub
    End Class
End Namespace