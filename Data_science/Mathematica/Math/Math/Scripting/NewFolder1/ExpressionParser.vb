Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Public Class ExpressionParser

    Dim text As CharPtr
    Dim buf As New CharBuffer

    ReadOnly operators As Index(Of Char) = {"+"c, "-"c, "*"c, "/"c, "^"c, "%"c}

    Sub New(text As CharPtr)
        Me.text = text
    End Sub

    Public Iterator Function GetTokens() As IEnumerable(Of MathToken)
        Dim token As New Value(Of MathToken)

        Do While Not text
            If Not (token = walkChar(++text)) Is Nothing Then
                Yield token

                If buf = 1 AndAlso buf(Scan0) Like operators Then
                    Yield New MathToken(MathTokens.Operator, buf.ToString)

                    buf *= 0
                End If
            End If
        Loop

        If buf > 0 Then
            Yield populateToken(Nothing)
        End If
    End Function

    Private Function walkChar(c As Char) As MathToken
        Static numbers As Index(Of Char) = {"."c, "0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c, "!"c}
        Static whitespaces As Index(Of Char) = {" "c, ASCII.TAB, ASCII.CR, ASCII.LF}

        If c Like numbers Then
            buf += c
        ElseIf c Like operators Then
            If buf = 0 AndAlso c = "-"c Then
                ' is negative number
                buf += "-"c
            ElseIf buf > 0 Then
                ' number or symbol
                Return populateToken(cacheNext:=c)
            Else
                Return New MathToken(MathTokens.Operator, c)
            End If
        ElseIf c Like whitespaces Then
            If buf > 0 Then
                Return populateToken(cacheNext:=Nothing)
            End If
        ElseIf c = "("c Then
            If buf > 0 Then
                Return populateToken(cacheNext:=c)
            Else
                Return New MathToken(MathTokens.Open, "("c)
            End If
        ElseIf c = ")"c Then
            If buf > 0 Then
                Return populateToken(cacheNext:=c)
            Else
                Return New MathToken(MathTokens.Close, ")"c)
            End If
        Else
            buf += c
        End If

        Return Nothing
    End Function

    Private Function populateToken(cacheNext As Char?) As MathToken
        Dim text As String = buf.ToString

        buf *= 0

        If Not cacheNext Is Nothing Then
            buf += cacheNext
        End If

        If Char.IsLetter(text.First) Then
            Return New MathToken(MathTokens.Symbol, text)
        ElseIf text.IsNumeric Then
            Return New MathToken(MathTokens.Literal, text)
        ElseIf text.Last = "!"c AndAlso Mid(text, 1, text.Length - 1).IsNumeric() Then
            Return New MathToken(MathTokens.Literal, text)
        ElseIf text = "(" Then
            Return New MathToken(MathTokens.Open, "(")
        ElseIf text = ")" Then
            Return New MathToken(MathTokens.Close, ")")
        Else
            Throw New NotImplementedException(text)
        End If
    End Function
End Class
