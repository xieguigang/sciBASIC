' This file was Auto Generated with TokenIcer
Imports System.Collections.Generic
Imports System.Text.RegularExpressions

Namespace Scripting.TokenIcer

    ' This defines the PeekToken object
    ''' <summary>
    ''' A PeekToken object class
    ''' </summary>
    ''' <remarks>
    ''' A PeekToken is a special pointer object that can be used to Peek() several
    ''' tokens ahead in the GetToken() queue.
    ''' </remarks>
    Public Class PeekToken

        Public Property TokenIndex() As Integer
        Public Property TokenPeek() As Token

        Public Sub New(index As Integer, value As Token)
            TokenIndex = index
            TokenPeek = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{TokenIndex}]  {TokenPeek.ToString}"
        End Function
    End Class

    ' This defines the Token object
    ''' <summary>
    ''' a Token object class
    ''' </summary>
    ''' <remarks>
    ''' A Token object holds the token and token value.
    ''' </remarks>
    Public Class Token

        Public Property TokenName() As TokenParser.Tokens
        Public Property TokenValue() As String

        Public Sub New(name As TokenParser.Tokens, value As String)
            TokenName = name
            TokenValue = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{TokenName}]" & vbCrLf & TokenValue
        End Function
    End Class
End Namespace