Namespace Serialization.JSON.Formatter.Internals.Strategies
    Friend NotInheritable Class OpenBracketStrategy
        Implements ICharacterStrategy
        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If context.IsProcessingString Then
                context.AppendCurrentChar()
                Return
            End If

            context.AppendCurrentChar()
            context.EnterObjectScope()

            If Not IsBeginningOfNewLineAndIndentionLevel(context) Then
                Return
            End If

            context.BuildContextIndents()
        End Sub

        Private Shared Function IsBeginningOfNewLineAndIndentionLevel(context As JsonFormatterStrategyContext) As Boolean
            Return context.IsProcessingVariableAssignment OrElse (Not context.IsStart AndAlso Not context.IsInArrayScope)
        End Function

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return "{"c
            End Get
        End Property
    End Class
End Namespace
