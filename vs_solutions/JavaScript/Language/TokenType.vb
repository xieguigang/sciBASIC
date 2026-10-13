
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
    ''' holds a <see cref="RegexLiteral"/>.</summary>
    Regex
    Eof
End Enum
