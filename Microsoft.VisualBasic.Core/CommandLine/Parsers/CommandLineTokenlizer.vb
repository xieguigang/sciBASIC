Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace CommandLine.Parsers

    Public Class CommandLineTokenlizer : Inherits SyntaxTokenlizer(Of CommandLineTokens, CommandLineToken)

        ReadOnly tokenEscapes As Dictionary(Of Char, TokenEscape)

        Dim tokenEscape As Boolean
        Dim start As Char
        Dim shouldEndwith As Char

        Public Sub New(text As [Variant](Of String, CharPtr))
            MyBase.New(text)
        End Sub

        Public Function ConfigTokenEscape(start As Char, ends As Char) As CommandLineTokenlizer
            Call tokenEscapes.Add(start, New TokenEscape With {.start = start, .ends = ends})
            Return Me
        End Function

        Protected Overrides Function walkChar(c As Char) As CommandLineToken
            If c = ASCII.CR OrElse c = ASCII.LF Then
                Throw New SyntaxErrorException("There is a new line character in your command line input???")
            End If

            If tokenEscape Then
                buffer += c

                If c = shouldEndwith AndAlso Not lastSplashEscape Then
                    Return popOutToken()
                End If
            ElseIf tokenEscapes.ContainsKey(c) AndAlso Not lastSplashEscape Then
                tokenEscape = True
                buffer += c
                start = c
                shouldEndwith = tokenEscapes(c).ends
            ElseIf c = " "c OrElse c = ASCII.TAB Then
                Return popOutToken()
            Else
                buffer += c
            End If

            Return Nothing
        End Function

        Protected Overrides Function popOutToken() As CommandLineToken
            Dim tokenStr As String = buffer.PopAllChars.CharString

            If tokenStr.StartsWith("-") OrElse tokenStr.StartsWith("/") OrElse tokenStr.StartsWith("!") Then
                If tokenStr.IndexOf("="c) > -1 OrElse tokenStr.IndexOf(":"c) > -1 Then
                    Return New CommandLineToken(CommandLineTokens.ArgumentAssignment, tokenStr)
                Else
                    Return New CommandLineToken(CommandLineTokens.ArgumentName, tokenStr)
                End If
            Else
                For Each test In tokenEscapes.Values
                    If tokenStr.First = test.start AndAlso tokenStr.Last = test.ends Then
                        Return New CommandLineToken(CommandLineTokens.ArgumentValue, tokenStr.Substring(1, tokenStr.Length - 2))
                    End If
                Next

                Return New CommandLineToken(CommandLineTokens.ArgumentValue, tokenStr)
            End If
        End Function

        Public Function CreateWithDefaultEscapes(input As String) As CommandLineTokenlizer
            Return New CommandLineTokenlizer(input) _
                .ConfigTokenEscape(""""c, """"c) _
                .ConfigTokenEscape("["c, "]"c)
        End Function
    End Class

    Public Class CommandLineToken : Inherits CodeToken(Of CommandLineTokens)

        Sub New(type As CommandLineTokens, value As String)
            Call MyBase.New(type, value)
        End Sub
    End Class

    Public Class TokenEscape

        Public Property start As Char
        Public Property ends As Char

        Public Overrides Function ToString() As String
            Return $"{start}token{ends}"
        End Function

    End Class

    Public Enum CommandLineTokens
        ArgumentName
        ArgumentValue
        ArgumentAssignment
    End Enum
End Namespace