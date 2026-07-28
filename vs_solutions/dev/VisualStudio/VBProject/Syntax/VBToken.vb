Namespace Syntax

    ''' <summary>
    ''' the kind of a lexical token produced by <see cref="VBScanner"/>
    ''' </summary>
    Public Enum TokenKind
        Keyword
        Identifier
        [String]
        CharLiteral
        Number
        Punctuation
        XmlDoc
        [Attribute]
    End Enum

    ''' <summary>
    ''' a single lexical token with its source text and line number
    ''' </summary>
    Public Structure Token
        Public Kind As TokenKind
        Public Text As String
        Public Line As Integer

        Public Overrides Function ToString() As String
            Return $"[{Kind}] {Text}"
        End Function
    End Structure

End Namespace
