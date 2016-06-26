Namespace Serialization.JSON.Formatter.Internals.Strategies

    Friend NotInheritable Class CloseSquareBracketStrategy
        Implements ICharacterStrategy

        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If context.IsProcessingString Then
                context.AppendCurrentChar()
                Return
            End If

            context.CloseCurrentScope()
            context.BuildContextIndents()
            context.AppendCurrentChar()
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return "]"c
            End Get
        End Property
    End Class
End Namespace
