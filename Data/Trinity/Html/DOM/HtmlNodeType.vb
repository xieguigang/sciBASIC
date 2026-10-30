Namespace Html

    ''' <summary>
    ''' HTML 文档树之中的节点类型
    ''' </summary>
    Public Enum HtmlNodeType

        ''' <summary>
        ''' 文档根节点(<c>#document</c>)，一棵文档树之中只存在一个
        ''' </summary>
        Document = 0

        ''' <summary>
        ''' 普通的元素节点，例如 ``&lt;div>`` ``&lt;span>``
        ''' </summary>
        Element = 1

        ''' <summary>
        ''' 文本节点
        ''' </summary>
        Text = 2

        ''' <summary>
        ''' 注释节点，即 ``&lt;!-- --&gt;`` 之中的文本内容
        ''' </summary>
        Comment = 3

        ''' <summary>
        ''' CDATA 节点，即 ``&lt;![CDATA[ ]]&gt;`` 之中的文本内容
        ''' </summary>
        CData = 4

        ''' <summary>
        ''' DOCTYPE 声明节点
        ''' </summary>
        Doctype = 5

        ''' <summary>
        ''' ``&lt;script&gt;`` 脚本块节点，块内的文本是原始文本，不做标签解析
        ''' </summary>
        Script = 6

        ''' <summary>
        ''' ``&lt;style&gt;`` 样式块节点，块内的文本是原始文本，不做标签解析
        ''' </summary>
        Style = 7
    End Enum

End Namespace
