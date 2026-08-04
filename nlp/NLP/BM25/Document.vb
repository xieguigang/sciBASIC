Namespace BM25

    ''' <summary>
    ''' 文档表示：ID + 分词后的词元列表。
    ''' </summary>
    Public Class Document

        ''' <summary>文档唯一标识。</summary>
        Public Property Id As Integer

        ''' <summary>文档原始文本（可选，用于回显结果）。</summary>
        Public Property RawText As String

        ''' <summary>分词后的词元数组。</summary>
        Public Property Tokens As String()

        ''' <summary>文档长度 = Tokens.Length。</summary>
        Public ReadOnly Property Length As Integer
            Get
                Return If(Tokens?.Length, 0)
            End Get
        End Property

        Public Sub New(id As Integer, tokens As String(), Optional rawText As String = "")
            Me.Id = id
            Me.Tokens = tokens
            Me.RawText = rawText
        End Sub

    End Class
End Namespace