Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Terminal

    Public Class Span

        Public Property text As String
        Public Property style As ConsoleFontStyle
        Public Property IsEndByNewLine As Boolean

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Print()
            Call My.Log4VB.Print(Me)
        End Sub

        Public Overrides Function ToString() As String
            Dim text$ = Me.text

            If text.StringEmpty Then
                text = "<whitespace>"
            Else
                text = $"""{text}"""
            End If

            Return style.ForeColor.DoCall(AddressOf ConsoleFontStyle.HtmlColorCode) & " " & text
        End Function
    End Class
End Namespace