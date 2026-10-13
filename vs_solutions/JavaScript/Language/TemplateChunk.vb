
''' <summary>
''' One chunk of a template literal token.
''' <para>
''' <c>IsExpression = False</c>: cooked text (escapes already decoded).
''' <c>IsExpression = True</c>: the RAW source text of a <c>${...}</c> placeholder,
''' which the parser recursively lexes/parses into an <see cref="Expression"/>.
''' </para>
''' <para>Example: <c>`a${x + 1}b`</c> → [text "a", expr "x + 1", text "b"].</para>
''' </summary>
Public NotInheritable Class TemplateChunk

    Public ReadOnly Property IsExpression As Boolean
    Public ReadOnly Property Text As String

    Public Sub New(isExpression As Boolean, text As String)
        Me.IsExpression = isExpression
        Me.Text = text
    End Sub

End Class
