Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Module TokenIcer

    Public ReadOnly Property Tokens As IReadOnlyDictionary(Of Char, Tokens) =
        New Dictionary(Of Char, Tokens) From {
 _
        {"0"c, Mathematical.Tokens.Number},  ' Numbers
        {"1"c, Mathematical.Tokens.Number},
        {"2"c, Mathematical.Tokens.Number},
        {"3"c, Mathematical.Tokens.Number},
        {"4"c, Mathematical.Tokens.Number},
        {"5"c, Mathematical.Tokens.Number},
        {"6"c, Mathematical.Tokens.Number},
        {"7"c, Mathematical.Tokens.Number},
        {"8"c, Mathematical.Tokens.Number},
        {"9"c, Mathematical.Tokens.Number},
        {"."c, Mathematical.Tokens.Number},
 _
        {"+"c, Mathematical.Tokens.Operator},  ' Operators
        {"-"c, Mathematical.Tokens.Operator},
        {"*"c, Mathematical.Tokens.Operator},
        {"/"c, Mathematical.Tokens.Operator},
        {"!"c, Mathematical.Tokens.Operator},
        {"%"c, Mathematical.Tokens.Operator},
        {"^"c, Mathematical.Tokens.Operator},
 _
        {"["c, Mathematical.Tokens.OpenBracket},  ' Brackets
        {"]"c, Mathematical.Tokens.OpenBracket},
        {"{"c, Mathematical.Tokens.CloseBracket},
        {"}"c, Mathematical.Tokens.CloseBracket},
 _
        {"("c, Mathematical.Tokens.OpenStack},  ' Stacks 
        {")"c, Mathematical.Tokens.CloseStack}
    }

    Public Function TryParse(s As String) As List(Of Token(Of Tokens))
        Dim str As CharEnumerator = s.GetEnumerator
        Dim tokens As New List(Of Token(Of Tokens))
        Dim ch As Char
        Dim token As New List(Of Char)
        Dim type As Tokens = Mathematical.Tokens.UNDEFINE
        Dim exitb As Boolean = False

        If Not str.MoveNext() Then  ' Empty expression
            Return New List(Of Token(Of Tokens))
        End If

        Do While True
            ch = str.Current
            token += ch

            If TokenIcer.Tokens.ContainsKey(ch) Then
                type = TokenIcer.Tokens(ch)

                Select Case type
                    Case Mathematical.Tokens.Number
                        exitb = str.__parseDouble(token)
                        tokens += New Token(Of Tokens)(type, New String(token))
                    Case Else
                        tokens += New Token(Of Tokens)(type, CStr(ch))
                        exitb = str.MoveNext()
                End Select
            Else
                exitb = str.__parseUNDEFINE(token)
                type = Mathematical.Tokens.UNDEFINE
                tokens += New Token(Of Tokens)(type, New String(token))
            End If

            If Not exitb Then
                Exit Do
            Else
                token.Clear()
            End If
        Loop

        Return tokens
    End Function

    ''' <summary>
    ''' 枚举是否已经结束？
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="token"></param>
    ''' <returns></returns>
    <Extension> Private Function __parseDouble(str As CharEnumerator, ByRef token As List(Of Char)) As Boolean
        Do While str.MoveNext
            If Not Tokens.ContainsKey(str.Current) Then
                Return True
            ElseIf Not Tokens(str.Current) = Mathematical.Tokens.Number Then
                Return True
            Else
                Call token.Add(str.Current)
            End If
        Loop

        Return False
    End Function

    <Extension> Private Function __parseUNDEFINE(str As CharEnumerator, ByRef token As List(Of Char)) As Boolean
        Do While str.MoveNext
            If Not Tokens.ContainsKey(str.Current) Then
                Call token.Add(str.Current)
            Else
                Return True
            End If
        Loop

        Return False
    End Function
End Module

Public Enum Tokens

    ''' <summary>
    ''' Function Name, constant, variable
    ''' </summary>
    UNDEFINE
    ''' <summary>
    ''' +-*/!^%
    ''' </summary>
    [Operator]
    ''' <summary>
    ''' <see cref="Double"/>
    ''' </summary>
    Number
    ''' <summary>
    ''' ,
    ''' </summary>
    Delimiter
    ''' <summary>
    ''' [ or {
    ''' </summary>
    OpenBracket
    ''' <summary>
    ''' ] or }
    ''' </summary>
    CloseBracket
    ''' <summary>
    ''' (
    ''' </summary>
    OpenStack
    ''' <summary>
    ''' )
    ''' </summary>
    CloseStack
End Enum