#Region "Microsoft.VisualBasic::d813ef73df179417ef25e8c374add697, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\Strategies\DoubleQuoteStrategy.vb"

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

    '     Class DoubleQuoteStrategy
    ' 
    '         Properties: ForWhichCharacter
    ' 
    '         Sub: Execute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization.JSON.Formatter.Internals.Strategies
    Friend NotInheritable Class DoubleQuoteStrategy
        Implements ICharacterStrategy
        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If Not context.IsProcessingSingleQuoteInitiatedString AndAlso Not context.WasLastCharacterABackSlash Then
                context.IsProcessingDoubleQuoteInitiatedString = Not context.IsProcessingDoubleQuoteInitiatedString
            End If

            context.AppendCurrentChar()
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return """"c
            End Get
        End Property
    End Class
End Namespace
