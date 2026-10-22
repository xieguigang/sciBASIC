''' <summary>
''' 从 markdown 源码中解析出的单个标题。
''' </summary>
''' <remarks>
''' <see cref="Index"/> 与渲染后的 ``&lt;h1&gt;``..``&lt;h6&gt;`` 元素顺序一一对应
''' （见 <see cref="MarkdownHeadings.Parse"/>），因此可以作为 HTML 侧锚点、
''' 目录条目或编辑器导航的索引使用。<see cref="LineStart"/> / <see cref="LineLength"/>
''' 记录标题在原始 markdown 文本中的位置，用于把导航反查到源码。
''' </remarks>
Public Class HeadingInfo

    ''' <summary>标题在文档中的序号（从 0 开始，与渲染后的 &lt;h*&gt; 顺序一致）。</summary>
    Public Property Index As Integer

    ''' <summary>标题层级（1-6）。</summary>
    Public Property Level As Integer

    ''' <summary>标题文本（已剥离行内强调 / 代码 / 删除线标记）。</summary>
    Public Property Text As String

    ''' <summary>标题所在行首在原始 markdown 文本中的字符偏移。</summary>
    Public Property LineStart As Integer

    ''' <summary>标题所在行的字符长度（不含换行符）。</summary>
    Public Property LineLength As Integer

End Class