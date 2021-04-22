Imports System
Imports System.Collections.Concurrent

Namespace PdfReader
    Public Class ParseName
        Inherits ParseObjectBase

        Private _Value As String
        Private Shared _lookup As ConcurrentDictionary(Of String, ParseName) = New ConcurrentDictionary(Of String, ParseName)()
        Private Shared _nullUpdate As Func(Of String, ParseName, ParseName) = Function(x, y) y

        Public Sub New(ByVal value As String)
            Me.Value = value
        End Sub

        Public Property Value As String
            Get
                Return _Value
            End Get
            Private Set(ByVal value As String)
                _Value = value
            End Set
        End Property

        Public Shared Function GetParse(ByVal name As String) As ParseName
            Dim parseName As ParseName = Nothing

            If Not _lookup.TryGetValue(name, parseName) Then
                parseName = New ParseName(name)
                _lookup.AddOrUpdate(name, parseName, _nullUpdate)
            End If

            Return parseName
        End Function
    End Class
End Namespace
