Namespace Parser

    ''' <summary>
    ''' Enumerates the types of tokens that can be classified by the tokenizer.
    ''' </summary>
    Public Enum TokenType
        ''' <summary>
        ''' This is reserved for the NullToken.
        ''' </summary>
        NotAToken

        ''' <summary>
        ''' An identifier can be either a class or property name.  The tokenizer does 
        ''' not have enough information to make that distinction.
        ''' </summary>
        Identifier

        ''' <summary>
        ''' An operator like + or *.
        ''' </summary>
        [Operator]

        ''' <summary>
        ''' A comma
        ''' </summary>
        Comma

        ''' <summary>
        ''' A dot (".").
        ''' </summary>
        Dot

        ''' <summary>
        ''' A primitive like a quoted string, boolean value, or number.
        ''' </summary>
        Primitive

        ''' <summary>
        ''' Open parenthesis
        ''' </summary>
        OpenParens

        ''' <summary>
        ''' Close parenthesis
        ''' </summary>
        CloseParens

        ''' <summary>
        ''' Open bracket
        ''' </summary>
        OpenBracket

        ''' <summary>
        ''' Close bracket
        ''' </summary>
        CloseBracket

        ''' <summary>
        ''' Double quote token, only used internally by tokenizer.
        ''' </summary>
        Quote
    End Enum

    ''' <summary>
    ''' Indicates priority in order of operations.
    ''' </summary>
    Public Enum TokenPriority
        ''' <summary>
        ''' Default
        ''' </summary>
        None

        ''' <summary>
        ''' Bitwise or
        ''' </summary>
        [Or]

        ''' <summary>
        ''' Bitwise and
        ''' </summary>
        [And]

        ''' <summary>
        ''' Bitwise not
        ''' </summary>
        [Not]

        ''' <summary>
        ''' Equality comparisons like &gt;, &lt;=, ==, etc.
        ''' </summary>
        Equality

        ''' <summary>
        ''' Plus or minus
        ''' </summary>
        PlusMinus

        ''' <summary>
        ''' Modulus
        ''' </summary>
        [Mod]

        ''' <summary>
        ''' Multiply or divide
        ''' </summary>
        MulDiv

        ''' <summary>
        ''' Unary minus
        ''' </summary>
        UnaryMinus
    End Enum
End Namespace