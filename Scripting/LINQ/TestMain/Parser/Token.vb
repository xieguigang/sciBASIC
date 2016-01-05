Namespace Parser


    ''' <summary>
    ''' Represents a token that is parsed out by the <see cref="Tokenizer"/>.
    ''' </summary>
    Public NotInheritable Class Token
        Private _Text As String
        Private _ParsedObject As Object
        Private _Type As TokenType
        Private _Priority As TokenPriority

        ''' <summary>
        ''' The text that makes up the token.
        ''' </summary>
        Public ReadOnly Property Text() As String
            Get
                Return _Text
            End Get
        End Property

        ''' <summary>
        ''' If the token can be parsed into a type like an integer, this property holds that value.
        ''' </summary>
        Public ReadOnly Property ParsedObject() As Object
            Get
                Return _ParsedObject
            End Get
        End Property

        ''' <summary>
        ''' Token type
        ''' </summary>
        Public ReadOnly Property Type() As TokenType
            Get
                Return _Type
            End Get
        End Property

        ''' <summary>
        ''' Token priority
        ''' </summary>
        Public ReadOnly Property Priority() As TokenPriority
            Get
                Return _Priority
            End Get
        End Property

        ''' <summary>
        ''' Constructor for tokens that are not parsed.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="type"></param>
        ''' <param name="priority"></param>
        Public Sub New(text As String, type As TokenType, priority As TokenPriority)
            _Text = text
            _Type = type
            _Priority = priority
            _ParsedObject = text
        End Sub

        ''' <summary>
        ''' Constructor for tokens that are parsed.
        ''' </summary>
        ''' <param name="parsedObj"></param>
        ''' <param name="type"></param>
        ''' <param name="priority"></param>
        Public Sub New(parsedObj As Object, type As TokenType, priority As TokenPriority)
            _ParsedObject = parsedObj
            _Text = ParsedObject.ToString()
            _Type = type
            _Priority = priority
        End Sub

        ''' <summary>
        ''' The null token represents a state where the <see cref="Tokenizer"/> encountered an error
        ''' or has not begun parsing yet.
        ''' </summary>
        Public Shared NullToken As New Token("", TokenType.NotAToken, TokenPriority.None)
    End Class
End Namespace