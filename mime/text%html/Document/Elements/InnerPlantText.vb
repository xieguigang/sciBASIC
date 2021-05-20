Imports Microsoft.VisualBasic.Text.Xml

Namespace Document

    ''' <summary>
    ''' Plant text inner the html.(HTML文档内的纯文本对象)
    ''' </summary>
    Public Class InnerPlantText : Implements IXmlNode

        Public Overridable Property InnerText As String

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

        Sub New()
        End Sub

        Sub New(text As String)
            InnerText = text
        End Sub

        Public Overrides Function ToString() As String
            Return InnerText
        End Function

        Public Overridable Function GetHtmlText() As String
            Return InnerText
        End Function

        Public Overridable Function GetPlantText() As String Implements IXmlNode.GetInnerText
            Return InnerText.UnescapeHTML
        End Function
    End Class

End Namespace