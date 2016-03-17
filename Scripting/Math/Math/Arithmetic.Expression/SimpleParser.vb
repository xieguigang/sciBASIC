Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Marshal
Imports Microsoft.VisualBasic.Mathematical.Types
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Module SimpleParser

    Public Function TryParse(s As String) As SimpleExpression
        Dim tokens = TokenIcer.TryParse(s.ClearOverlapOperator) 'Get all of the number that appears in this expression including factoral operator.

        If tokens.Count = 1 Then
            Dim token As Token(Of Tokens) = tokens.First

            If token.Type = Mathematical.Tokens.Number Then
                Return New SimpleExpression(Val(token.Text))
            Else  ' Syntax error
                Throw New SyntaxErrorException(s)
            End If
        Else
            Return New Pointer(Of Token(Of Tokens))(tokens).TryParse
        End If
    End Function

    <Extension>
    Public Function TryParse(tokens As Pointer(Of Token(Of Tokens))) As SimpleExpression
        Dim NewExpression As New SimpleExpression 'New object to return for this function
        Dim s As Token(Of Tokens)
        Dim n As Double
        Dim o As Char

        Do While Not tokens.EndRead
            s = +tokens

            If tokens.EndRead Then
                Call NewExpression.Add(Val(s.Text), "+c")
            Else
                o = (+tokens).Text.First
                n = Val(s.Text)

                If o = "!"c Then
                    n = Helpers.Arithmetic.Factorial(n, 0)
                    If tokens.EndRead Then
                        Call NewExpression.Add(n, "+"c)
                        Exit Do
                    Else
                        o = (+tokens).Text.First
                    End If
                End If

                Call NewExpression.Add(n, o)
            End If
        Loop

        Return NewExpression
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ClearOverlapOperator(ByRef s As String) As String
        Dim sBuilder As StringBuilder = New StringBuilder(value:="0+" & s)

        's = "0+" & sbr.ToString '0a=a; 0-a=-a; 0+a=a

        Call sBuilder.Replace("++", "+")
        Call sBuilder.Replace("--", "+")
        Call sBuilder.Replace("+-", "-")

        Return sBuilder.ToString
    End Function
End Module
