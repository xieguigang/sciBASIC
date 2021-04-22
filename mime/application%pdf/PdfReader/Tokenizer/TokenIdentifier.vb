Imports System
Imports System.Collections.Concurrent

Namespace PdfReader
    Public Class TokenIdentifier
        Inherits TokenObject

        Private _Value As String
        Private Shared _lookup As ConcurrentDictionary(Of String, TokenIdentifier) = New ConcurrentDictionary(Of String, TokenIdentifier)()
        Private Shared _nullUpdate As Func(Of String, TokenIdentifier, TokenIdentifier) = Function(x, y) y

        Public Sub New(ByVal identifier As String)
            Value = identifier
        End Sub

        Public Property Value As String
            Get
                Return _Value
            End Get
            Private Set(ByVal value As String)
                _Value = value
            End Set
        End Property

        Public Shared Function GetToken(ByVal identifier As String) As TokenIdentifier
            Dim tokenIdentifier As TokenIdentifier = Nothing

            If Not _lookup.TryGetValue(identifier, tokenIdentifier) Then
                tokenIdentifier = New TokenIdentifier(identifier)
                _lookup.AddOrUpdate(identifier, tokenIdentifier, _nullUpdate)
            End If

            Return tokenIdentifier
        End Function
    End Class
End Namespace
