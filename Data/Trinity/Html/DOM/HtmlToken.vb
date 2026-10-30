Imports System.Collections.Generic

Namespace Html

    ''' <summary>
    ''' HTML 词法扫描所产生的记号类型
    ''' </summary>
    Friend Enum HtmlTokenType
        ''' <summary>开始标签，例如 ``&lt;div class="x"&gt;``</summary>
        OpenTag
        ''' <summary>结束标签，例如 ``&lt;/div&gt;``</summary>
        CloseTag
        ''' <summary>自闭合标签，例如 ``&lt;br/&gt;`` 或者 ``&lt;img ...&gt;``</summary>
        SelfClosingTag
        ''' <summary>文本</summary>
        Text
        ''' <summary>注释</summary>
        Comment
        ''' <summary>CDATA 块</summary>
        CData
        ''' <summary>DOCTYPE 声明</summary>
        Doctype
    End Enum

    ''' <summary>
    ''' HTML 词法扫描器所产生的一个记号对象
    ''' </summary>
    Friend Class HtmlToken

        ''' <summary>
        ''' 记号类型
        ''' </summary>
        Public Property Type As HtmlTokenType

        ''' <summary>
        ''' 标签名(小写)；只有标签类型的记号才有意义
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 标签的属性列表；只有标签类型的记号才有意义
        ''' </summary>
        Public Property Attributes As List(Of KeyValuePair(Of String, String))

        ''' <summary>
        ''' 文本内容；只有文本/注释/CDATA/DOCTYPE 类型的记号才有意义
        ''' </summary>
        Public Property Text As String

        ''' <summary>
        ''' 这个记号在原始文档之中的字符下标位置
        ''' </summary>
        Public Property Position As Integer

        Friend Shared Function OfText(text As String, pos As Integer) As HtmlToken
            Return New HtmlToken With {.Type = HtmlTokenType.Text, .Text = text, .Position = pos}
        End Function

        Friend Shared Function OfComment(text As String, pos As Integer) As HtmlToken
            Return New HtmlToken With {.Type = HtmlTokenType.Comment, .Text = text, .Position = pos}
        End Function

        Friend Shared Function OfCData(text As String, pos As Integer) As HtmlToken
            Return New HtmlToken With {.Type = HtmlTokenType.CData, .Text = text, .Position = pos}
        End Function

        Friend Shared Function OfDoctype(text As String, pos As Integer) As HtmlToken
            Return New HtmlToken With {.Type = HtmlTokenType.Doctype, .Text = text, .Position = pos}
        End Function

        Public Overrides Function ToString() As String
            Select Case Type
                Case HtmlTokenType.Text
                    Return "#text"
                Case HtmlTokenType.Comment
                    Return "<!--" & Text & "-->"
                Case HtmlTokenType.CData
                    Return "<![CDATA[" & Text & "]]>"
                Case HtmlTokenType.Doctype
                    Return "<!" & Text & ">"
                Case HtmlTokenType.CloseTag
                    Return "</" & Name & ">"
                Case Else
                    Return "<" & Name & ">"
            End Select
        End Function
    End Class

End Namespace
