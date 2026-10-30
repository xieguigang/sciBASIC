Imports System.Collections.Generic

Namespace Html

    ''' <summary>
    ''' HTML 标签的分类表：空元素(void element)、隐式闭合规则、块级元素、原始文本元素
    ''' </summary>
    ''' <remarks>
    ''' 这些分类表用于让解析器能够正确的处理真实世界之中大量存在的畸形 HTML 文档：
    '''
    ''' <list type="bullet">
    ''' <item>空元素：``&lt;br&gt;`` ``&lt;img&gt;`` 这类标签没有结束标签，不可以压入标签栈</item>
    ''' <item>隐式闭合：``&lt;li&gt;`` 会隐式的闭合上一个还没有闭合的 ``&lt;li&gt;``</item>
    ''' <item>原始文本：``&lt;script&gt;`` 之内的所有内容都不可以作为标签来解析</item>
    ''' </list>
    ''' </remarks>
    Friend Module HtmlTags

        ''' <summary>
        ''' 没有结束标签的元素集合(void elements)
        ''' </summary>
        Friend ReadOnly VoidElements As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "area", "base", "br", "col", "embed", "hr", "img", "input",
            "link", "meta", "param", "source", "track", "wbr"
        }

        ''' <summary>
        ''' 标签内部是原始文本，直到遇到对应的结束标签为止，内部的一切尖括号都不作为标签处理
        ''' </summary>
        Friend ReadOnly RawTextElements As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "script", "style", "textarea", "title", "iframe",
            "noscript", "noframes", "xmp", "plaintext"
        }

        ''' <summary>
        ''' 块级元素，用于在拼装纯文本的时候决定是否需要插入分隔符
        ''' </summary>
        Friend ReadOnly BlockElements As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "address", "article", "aside", "blockquote", "br", "caption", "dd", "details",
            "div", "dl", "dt", "fieldset", "figcaption", "figure", "footer", "form",
            "h1", "h2", "h3", "h4", "h5", "h6", "header", "hgroup", "hr", "li",
            "main", "nav", "ol", "p", "pre", "section", "summary", "table",
            "tbody", "td", "tfoot", "th", "thead", "tr", "ul"
        }

        ''' <summary>
        ''' 隐式闭合规则：key 为新打开的标签名，value 为它所能够隐式闭合掉的还没有结束的标签名
        ''' </summary>
        Friend ReadOnly ImplicitClose As New Dictionary(Of String, String())(StringComparer.OrdinalIgnoreCase) From {
            {"li", {"li"}},
            {"p", {"p"}},
            {"dt", {"dt", "dd"}},
            {"dd", {"dd", "dt"}},
            {"option", {"option"}},
            {"tr", {"tr", "td", "th"}},
            {"td", {"td", "th"}},
            {"th", {"td", "th"}},
            {"thead", {"tr", "td", "th", "thead", "tbody", "tfoot"}},
            {"tbody", {"tr", "td", "th", "thead", "tbody", "tfoot"}},
            {"tfoot", {"tr", "td", "th", "thead", "tbody", "tfoot"}},
            {"h1", {"h1", "h2", "h3", "h4", "h5", "h6"}},
            {"h2", {"h1", "h2", "h3", "h4", "h5", "h6"}},
            {"h3", {"h1", "h2", "h3", "h4", "h5", "h6"}},
            {"h4", {"h1", "h2", "h3", "h4", "h5", "h6"}},
            {"h5", {"h1", "h2", "h3", "h4", "h5", "h6"}},
            {"h6", {"h1", "h2", "h3", "h4", "h5", "h6"}}
        }

        ''' <summary>
        ''' 判断一个标签名是否为原始文本元素
        ''' </summary>
        Friend Function IsRawText(name As String) As Boolean
            Return name IsNot Nothing AndAlso RawTextElements.Contains(name)
        End Function

        ''' <summary>
        ''' 判断一个标签名是否为空元素
        ''' </summary>
        Friend Function IsVoid(name As String) As Boolean
            Return name IsNot Nothing AndAlso VoidElements.Contains(name)
        End Function

        ''' <summary>
        ''' 判断一个节点是否为块级元素
        ''' </summary>
        Friend Function IsBlock(node As HtmlNode) As Boolean
            If node Is Nothing Then
                Return False
            End If
            If node.NodeType = HtmlNodeType.Text OrElse node.NodeType = HtmlNodeType.Comment Then
                Return False
            End If
            Return BlockElements.Contains(node.Name)
        End Function
    End Module

End Namespace
