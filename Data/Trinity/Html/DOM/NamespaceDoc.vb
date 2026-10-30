Namespace Html

    ''' <summary>
    ''' 通用的 HTML 文档解析方法库：词法扫描、文档树构建、选择器查询与文本抽取
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这个命名空间之下的代码是通用的，与任何具体的站点都没有关系，可以被任意的
    ''' 网页数据抽取程序复用。主要的入口点是 <see cref="HtmlDocument.Parse"/>，
    ''' 解析之后得到一棵由 <see cref="HtmlNode"/> 组成的文档树，之后即可使用
    ''' <see cref="HtmlSelector"/> 所支持的类 CSS 选择器做查询。
    ''' </para>
    ''' <para>
    ''' 组成：
    ''' <list type="bullet">
    ''' <item><see cref="HtmlDocument"/>：解析入口与文档级别的查询 API</item>
    ''' <item><see cref="HtmlNode"/>：节点模型，提供属性读取、树遍历与文本抽取</item>
    ''' <item><see cref="HtmlSelector"/>：类 CSS 的选择器引擎</item>
    ''' <item><see cref="HtmlEntity"/>：HTML 字符实体的编解码</item>
    ''' <item><see cref="HtmlText"/>：纯文本抽取、空白归一化、链接绝对化等文本处理扩展</item>
    ''' </list>
    ''' </para>
    ''' <para>
    ''' 与既有的 <see cref="Html2Article"/>(基于文本密度的正文抽取) 是互补的关系：
    ''' 后者适用于在不知道页面结构的情况下猜测正文，而这里提供的 DOM 解析
    ''' 适用于已经知道页面结构、需要精确的抽取指定字段的场合。
    ''' </para>
    ''' </remarks>
    Friend Class NamespaceDoc
    End Class

End Namespace
