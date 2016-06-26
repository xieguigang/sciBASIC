Namespace Serialization.JSON.Formatter.Internals.Strategies
    Friend NotInheritable Class SkipWhileNotInStringStrategy
        Implements ICharacterStrategy
        Private ReadOnly selectionCharacter As Char

        Public Sub New(selectionCharacter As Char)
            Me.selectionCharacter = selectionCharacter
        End Sub

        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If context.IsProcessingString Then
                context.AppendCurrentChar()
            End If
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return Me.selectionCharacter
            End Get
        End Property
    End Class
End Namespace
