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
