
''' <summary>Token categories produced by <see cref="Lexer"/>.</summary>
Public Enum TokenType
    Number
    [String]
    Identifier
    Keyword
    Punct
    ''' <summary>Template literal `` `a${b}c` ``; <see cref="Token.Value"/> holds
    ''' the <see cref="TemplateChunk"/> list (cooked text + expression sources).</summary>
    Template
    ''' <summary>Regular-expression literal <c>/pattern/flags</c>; <see cref="Token.Value"/>
    ''' holds a source/flags pair object.</summary>
    Regex
    Eof
End Enum
