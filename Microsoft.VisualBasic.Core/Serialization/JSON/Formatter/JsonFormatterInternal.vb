#Region "Microsoft.VisualBasic::82a6e9e2ddd3af5595828b70a1e6a6a9, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\JsonFormatterInternal.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class JsonFormatterInternal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Format
    ' 
    '         Sub: PrettyPrintCharacter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON.Formatter.Internals.Strategies

Namespace Serialization.JSON.Formatter.Internals

    Friend NotInheritable Class JsonFormatterInternal

        ReadOnly context As JsonFormatterStrategyContext

        Public Sub New(context As JsonFormatterStrategyContext)
            Me.context = context

            Me.context.ClearStrategies()
            Me.context.AddCharacterStrategy(New OpenBracketStrategy())
            Me.context.AddCharacterStrategy(New CloseBracketStrategy())
            Me.context.AddCharacterStrategy(New OpenSquareBracketStrategy())
            Me.context.AddCharacterStrategy(New CloseSquareBracketStrategy())
            Me.context.AddCharacterStrategy(New SingleQuoteStrategy())
            Me.context.AddCharacterStrategy(New DoubleQuoteStrategy())
            Me.context.AddCharacterStrategy(New CommaStrategy())
            Me.context.AddCharacterStrategy(New ColonCharacterStrategy())
            Me.context.AddCharacterStrategy(New SkipWhileNotInStringStrategy(ControlChars.Lf))
            Me.context.AddCharacterStrategy(New SkipWhileNotInStringStrategy(ControlChars.Cr))
            Me.context.AddCharacterStrategy(New SkipWhileNotInStringStrategy(ControlChars.Tab))
            Me.context.AddCharacterStrategy(New SkipWhileNotInStringStrategy(" "c))
        End Sub

        Public Function Format(json As String) As String
            If json Is Nothing Then
                Return String.Empty
            End If

            If json.Trim() = String.Empty Then
                Return String.Empty
            End If

            Dim output As New StringBuilder()

            Me.PrettyPrintCharacter(json.ToArray, output)

            Return output.ToString()
        End Function

        Private Sub PrettyPrintCharacter(input As Char(), output As StringBuilder)
            For i As Integer = 0 To input.Length - 1
                Me.context.PrettyPrintCharacter(input(i), output)
            Next
        End Sub
    End Class
End Namespace
