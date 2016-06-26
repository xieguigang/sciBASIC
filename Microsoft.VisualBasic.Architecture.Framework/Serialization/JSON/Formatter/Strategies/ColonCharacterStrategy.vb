Namespace Serialization.JSON.Formatter.Internals.Strategies
    Friend NotInheritable Class ColonCharacterStrategy
        Implements ICharacterStrategy
        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If context.IsProcessingString Then
                context.AppendCurrentChar()
                Return
            End If

            context.IsProcessingVariableAssignment = True
            context.AppendCurrentChar()
            context.AppendSpace()
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return ":"c
            End Get
        End Property
    End Class
End Namespace
