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
    Public Class PeekToken(Of Tokens)

        Public Property TokenIndex As Integer
        Public Property TokenPeek As Token(Of Tokens)

        Public Sub New(index As Integer, value As Token(Of Tokens))
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
    ''' <typeparam name="Tokens">应该是枚举类型</typeparam>
    ''' <remarks>
    ''' A Token object holds the token and token value.
    ''' </remarks>
    Public Class Token(Of Tokens)

        ''' <summary>
        ''' Token type
        ''' </summary>
        ''' <returns></returns>
        Public Property TokenName As Tokens

        ''' <summary>
        ''' The text that makes up the token.
        ''' </summary>
        ''' <returns></returns>
        Public Property TokenValue As String

        ''' <summary>
        ''' 务必要保持0为未定义
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property UNDEFINED As Boolean
            Get
                If TypeOf TokenName Is [Enum] OrElse TypeOf TokenName Is Integer Then
                    Dim o As Object = TokenName
                    Dim i As Integer = CInt(o)
                    If i = 0 Then
                        Return True
                    End If
                End If

                Return TokenName Is Nothing OrElse
                    String.IsNullOrEmpty(TokenValue)
            End Get
        End Property

        Public ReadOnly Property Type As Tokens
            Get
                Return TokenName
            End Get
        End Property

        Public Sub New(name As Tokens, value As String)
            TokenName = name
            TokenValue = value
        End Sub

        Sub New(name As Tokens)
            TokenName = name
        End Sub

        Public Overrides Function ToString() As String
            If UNDEFINED Then
                Return "UNDEFINED"
            End If
            Return $"[{TokenName}]" & vbTab & TokenValue
        End Function
    End Class
End Namespace