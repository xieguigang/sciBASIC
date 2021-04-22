Imports System
Imports System.Collections.Concurrent

Namespace PdfReader
    Public Class TokenName
        Inherits TokenObject

        Private _Value As String
        Private Shared _lookup As ConcurrentDictionary(Of String, TokenName) = New ConcurrentDictionary(Of String, TokenName)()
        Private Shared _nullUpdate As Func(Of String, TokenName, TokenName) = Function(x, y) y

        Public Sub New(ByVal name As String)
            Value = name
        End Sub

        Public Property Value As String
            Get
                Return _Value
            End Get
            Private Set(ByVal value As String)
                _Value = value
            End Set
        End Property

        Public Shared Function GetToken(ByVal name As String) As TokenName
            Dim tokenName As TokenName = Nothing

            If Not _lookup.TryGetValue(name, tokenName) Then
                tokenName = New TokenName(name)
                _lookup.AddOrUpdate(name, tokenName, _nullUpdate)
            End If

            Return tokenName
        End Function
    End Class
End Namespace
