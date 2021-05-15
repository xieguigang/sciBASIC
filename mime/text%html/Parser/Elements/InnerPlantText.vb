Namespace HTML

    ''' <summary>
    ''' Plant text inner the html.(HTML文档内的纯文本对象)
    ''' </summary>
    Public Class InnerPlantText

        Public Overridable Property InnerText As String

        Sub New()
        End Sub

        Sub New(text As String)
            InnerText = text
        End Sub

        Public Overrides Function ToString() As String
            Return InnerText
        End Function

        Public Overridable ReadOnly Property IsEmpty As Boolean
            Get
                Return String.IsNullOrEmpty(InnerText)
            End Get
        End Property

        Public Overridable ReadOnly Property IsPlantText As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overridable Function GetPlantText() As String
            Return InnerText
        End Function
    End Class

End Namespace