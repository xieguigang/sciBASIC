Namespace Serialization.JSON.Formatter.Internals

    Friend Interface ICharacterStrategy
        Sub Execute(context As JsonFormatterStrategyContext)

        ReadOnly Property ForWhichCharacter() As Char
    End Interface
End Namespace
