Imports System.IO
Imports System.Linq
Imports System.Text

Namespace Html

    ''' <summary>
    ''' 一个通用的 HTML 文档对象模型
    ''' </summary>
    ''' <remarks>
    ''' 这是整个 HTML 解析库的入口点：调用 <see cref="Parse"/> 之后就可以使用
    ''' 类 CSS 的选择器在文档树之上做查询，例如：
    '''
    ''' ```vbnet
    ''' Dim doc As HtmlDocument = HtmlDocument.Parse(html)
    '''
    ''' For Each li As HtmlNode In doc.QuerySelectorAll("li.aca_algo")
    '''     Dim a As HtmlNode = li.QuerySelector("h2 a")
    '''     Dim title As String = a.InnerText
    '''     Dim url As String = HtmlText.AbsoluteUrl(a.Attribute("href"), "https://cn.bing.com/")
    ''' Next
    ''' ```
    '''
    ''' 解析器是宽容的，对于未闭合的标签、多余的结束标签、``&lt;script&gt;`` 之中不规范的尖括号
    ''' 都不会抛出异常，因此可以直接用于处理真实世界之中的页面。
    ''' </remarks>
    Public Class HtmlDocument

        Private ReadOnly m_root As HtmlNode
        Private ReadOnly m_html As String

        Friend Sub New(root As HtmlNode, html As String)
            m_root = root
            m_html = html
        End Sub

        ''' <summary>
        ''' 解析一段 HTML 文档文本
        ''' </summary>
        ''' <param name="html">HTML 文档文本，允许为空</param>
        ''' <returns>解析出来的文档对象</returns>
        Public Shared Function Parse(html As String) As HtmlDocument
            Dim tokens As List(Of HtmlToken) = HtmlTokenizer.Scan(html)
            Dim root As HtmlNode = HtmlDocumentBuilder.Build(tokens)

            Return New HtmlDocument(root, If(html, ""))
        End Function

        ''' <summary>
        ''' 从文件之中加载并解析一个 HTML 文档
        ''' </summary>
        ''' <param name="path">文件路径</param>
        ''' <param name="encoding">文件编码，默认为 UTF8</param>
        Public Shared Function LoadFile(path As String, Optional encoding As Encoding = Nothing) As HtmlDocument
            If encoding Is Nothing Then
                encoding = Encoding.UTF8
            End If

            Return Parse(File.ReadAllText(path, encoding))
        End Function

        ''' <summary>
        ''' 文档的根节点(<c>#document</c>)
        ''' </summary>
        Public ReadOnly Property Root As HtmlNode
            Get
                Return m_root
            End Get
        End Property

        ''' <summary>
        ''' 原始的 HTML 文档文本
        ''' </summary>
        Public ReadOnly Property Html As String
            Get
                Return m_html
            End Get
        End Property

        ''' <summary>
        ''' 文档的根元素，一般是 ``&lt;html&gt;`` 元素；文档之中不存在的时候返回 <c>Nothing</c>
        ''' </summary>
        Public ReadOnly Property DocumentElement As HtmlNode
            Get
                Return m_root.Element("html")
            End Get
        End Property

        ''' <summary>
        ''' ``&lt;head&gt;`` 元素
        ''' </summary>
        Public ReadOnly Property Head As HtmlNode
            Get
                Dim html As HtmlNode = DocumentElement

                If html Is Nothing Then
                    Return m_root.Element("head")
                End If

                Return html.Element("head")
            End Get
        End Property

        ''' <summary>
        ''' ``&lt;body&gt;`` 元素
        ''' </summary>
        Public ReadOnly Property Body As HtmlNode
            Get
                Dim html As HtmlNode = DocumentElement

                If html Is Nothing Then
                    Return m_root.Element("body")
                End If

                Return html.Element("body")
            End Get
        End Property

        ''' <summary>
        ''' 文档标题，即 ``&lt;title&gt;`` 元素之内的文本；不存在的时候返回空字符串
        ''' </summary>
        Public Function Title() As String
            Dim node As HtmlNode = QuerySelector("title")

            If node Is Nothing Then
                Return ""
            End If

            Return node.InnerText
        End Function

        ''' <summary>
        ''' 按照元素编号查找元素，返回第一个命中的元素
        ''' </summary>
        Public Function GetElementById(id As String) As HtmlNode
            If String.IsNullOrEmpty(id) Then
                Return Nothing
            End If

            Return m_root _
                .Descendants() _
                .FirstOrDefault(Function(n) String.Equals(n.Id, id, StringComparison.Ordinal))
        End Function

        ''' <summary>
        ''' 按照标签名查找所有的元素
        ''' </summary>
        Public Function GetElementsByTagName(tagName As String) As HtmlNode()
            If String.IsNullOrEmpty(tagName) Then
                Return New HtmlNode() {}
            End If

            Return m_root _
                .Descendants() _
                .Where(Function(n) String.Equals(n.Name, tagName, StringComparison.OrdinalIgnoreCase)) _
                .ToArray()
        End Function

        ''' <summary>
        ''' 按照 css 类名查找所有的元素
        ''' </summary>
        Public Function GetElementsByClassName(className As String) As HtmlNode()
            If String.IsNullOrEmpty(className) Then
                Return New HtmlNode() {}
            End If

            Return m_root _
                .Descendants() _
                .Where(Function(n) n.HasClass(className)) _
                .ToArray()
        End Function

        ''' <summary>
        ''' 使用类 CSS 的选择器查询文档，返回所有命中的元素
        ''' </summary>
        Public Function QuerySelectorAll(selector As String) As HtmlNode()
            Return HtmlSelector.Parse(selector).SelectNodes(m_root)
        End Function

        ''' <summary>
        ''' 使用类 CSS 的选择器查询文档，返回第一个命中的元素
        ''' </summary>
        Public Function QuerySelector(selector As String) As HtmlNode
            Return QuerySelectorAll(selector).FirstOrDefault()
        End Function

        ''' <summary>
        ''' 文档之中所有的元素节点的数量
        ''' </summary>
        Public Function CountElements() As Integer
            Return m_root.Descendants().Count()
        End Function

        ''' <summary>
        ''' 抽取文档正文：基于 <see cref="Html2Article"/> 的文本密度算法
        ''' </summary>
        ''' <param name="depth">按行分析的深度</param>
        ''' <param name="limitCount">字符限定数，分析的文本达到这个数量时则认为进入正文</param>
        ''' <param name="appendMode">是否使用追加模式</param>
        Public Function GetArticle(Optional depth As Integer = 6,
                                   Optional limitCount As Integer = 180,
                                   Optional appendMode As Boolean = False) As Article

            Return New Html2Article With {
                .AppendMode = appendMode,
                .Depth = depth,
                .LimitCount = limitCount
            }.GetArticle(m_html)
        End Function

        ''' <summary>
        ''' 将文档之中的所有的相对链接补全为绝对链接
        ''' </summary>
        ''' <param name="baseUrl">基准地址</param>
        Public Function FixUrls(baseUrl As String) As String
            Return HtmlText.FixRelativeUrls(m_html, baseUrl)
        End Function

        Public Overrides Function ToString() As String
            Return Title()
        End Function
    End Class

End Namespace
