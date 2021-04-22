Imports System
Imports System.Collections.Concurrent

Namespace PdfReader
    Public Class ParseIdentifier
        Inherits ParseObjectBase

        Private _Value As String
        Private Shared _lookup As ConcurrentDictionary(Of String, ParseIdentifier) = New ConcurrentDictionary(Of String, ParseIdentifier)()
        Private Shared _nullUpdate As Func(Of String, ParseIdentifier, ParseIdentifier) = Function(x, y) y

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

        Public Shared Function GetParse(ByVal identifier As String) As ParseIdentifier
            Dim parseIdentifier As ParseIdentifier = Nothing

            If Not _lookup.TryGetValue(identifier, parseIdentifier) Then
                parseIdentifier = New ParseIdentifier(identifier)
                _lookup.AddOrUpdate(identifier, parseIdentifier, _nullUpdate)
            End If

            Return parseIdentifier
        End Function
    End Class
End Namespace
