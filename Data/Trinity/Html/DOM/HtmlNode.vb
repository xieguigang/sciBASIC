Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Html

    ''' <summary>
    ''' HTML 文档树之中的一个节点
    ''' </summary>
    ''' <remarks>
    ''' 这个类型是只读的查询模型：可以通过 <see cref="Nodes"/> 向下遍历，通过
    ''' <see cref="Parent"/> 向上遍历，也可以通过 <see cref="QuerySelector"/> 使用
    ''' 类 CSS 的选择器做查询。节点的构建由 <see cref="HtmlDocument.Parse"/> 在内部完成。
    ''' </remarks>
    Public Class HtmlNode

        ''' <summary>
        ''' 子节点列表
        ''' </summary>
        Friend ReadOnly m_nodes As New List(Of HtmlNode)

        ''' <summary>
        ''' 属性表，属性名是大小写不敏感的
        ''' </summary>
        Friend ReadOnly attrs As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        Private m_parent As HtmlNode
        Private m_innerText As String
        Private m_classes As String()

        ''' <summary>
        ''' 文本节点/注释节点/CDATA 节点的原始文本内容
        ''' </summary>
        Private m_raw As String

        Friend Sub New(type As HtmlNodeType, name As String)
            Me.NodeType = type
            Me.Name = If(name, "")
        End Sub

        ''' <summary>
        ''' 节点类型
        ''' </summary>
        Public ReadOnly Property NodeType As HtmlNodeType

        ''' <summary>
        ''' 标签名(小写)；文本节点为 <c>#text</c>，注释节点为 <c>#comment</c>，
        ''' CDATA 节点为 <c>#cdata</c>，文档根节点为 <c>#document</c>
        ''' </summary>
        Public ReadOnly Property Name As String

        ''' <summary>
        ''' 节点的属性值；对于文本节点，这个值就是节点的文本内容(还没有做实体解码)
        ''' </summary>
        Public ReadOnly Property RawText As String
            Get
                Return If(m_raw, "")
            End Get
        End Property

        ''' <summary>
        ''' 父节点；文档根节点没有父节点，为 <c>Nothing</c>
        ''' </summary>
        Public ReadOnly Property Parent As HtmlNode
            Get
                Return m_parent
            End Get
        End Property

        ''' <summary>
        ''' 子节点列表
        ''' </summary>
        Public ReadOnly Property Nodes As IReadOnlyList(Of HtmlNode)
            Get
                Return m_nodes
            End Get
        End Property

        ''' <summary>
        ''' 属性表
        ''' </summary>
        Public ReadOnly Property Attributes As IReadOnlyDictionary(Of String, String)
            Get
                Return attrs
            End Get
        End Property

        Friend Sub SetParent(parent As HtmlNode)
            m_parent = parent
        End Sub

        Friend Sub Add(child As HtmlNode)
            Call m_nodes.Add(child)
        End Sub

        Friend Sub SetText(text As String)
            m_raw = text
        End Sub

        Friend Sub SetAttrs(list As List(Of KeyValuePair(Of String, String)))
            If list Is Nothing Then
                Return
            End If

            For Each attr As KeyValuePair(Of String, String) In list
                If Not attrs.ContainsKey(attr.Key) Then
                    Call attrs.Add(attr.Key, attr.Value)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 元素的 id 属性，不存在的时候返回空字符串
        ''' </summary>
        Public ReadOnly Property Id As String
            Get
                Return Attribute("id")
            End Get
        End Property

        ''' <summary>
        ''' 元素的 class 属性所拆分出来的类名列表
        ''' </summary>
        Public ReadOnly Property Classes As String()
            Get
                If m_classes Is Nothing Then
                    m_classes = Regex _
                        .Split(Attribute("class"), "\s+") _
                        .Where(Function(c) c.Length > 0) _
                        .ToArray()
                End If

                Return m_classes
            End Get
        End Property

        ''' <summary>
        ''' 读取一个属性的值(已经做过 HTML 实体解码)
        ''' </summary>
        ''' <param name="name">属性名，大小写不敏感</param>
        ''' <param name="[default]">属性不存在的时候所返回的默认值</param>
        Public Function Attribute(name As String, Optional [default] As String = "") As String
            If String.IsNullOrEmpty(name) Then
                Return [default]
            End If

            Dim value As String = Nothing

            If Not attrs.TryGetValue(name, value) OrElse value Is Nothing Then
                Return [default]
            End If

            Return HtmlEntity.Decode(value)
        End Function

        ''' <summary>
        ''' 读取一个属性的原始值(不做 HTML 实体解码)
        ''' </summary>
        Public Function RawAttribute(name As String, Optional [default] As String = "") As String
            Dim value As String = Nothing

            If Not attrs.TryGetValue(name, value) OrElse value Is Nothing Then
                Return [default]
            End If

            Return value
        End Function

        ''' <summary>
        ''' 判断指定的属性是否存在
        ''' </summary>
        Public Function HasAttribute(name As String) As Boolean
            If String.IsNullOrEmpty(name) Then
                Return False
            End If

            Return attrs.ContainsKey(name)
        End Function

        ''' <summary>
        ''' 判断这个元素是否带有指定的 css 类名
        ''' </summary>
        Public Function HasClass(className As String) As Boolean
            If String.IsNullOrEmpty(className) Then
                Return False
            End If

            For Each c As String In Classes
                If String.Equals(c, className, StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 这个节点是否为元素节点(文本节点和注释节点返回 <c>False</c>)
        ''' </summary>
        Public ReadOnly Property IsElement As Boolean
            Get
                Return Queryable(Me)
            End Get
        End Property

        ''' <summary>
        ''' 子节点之中的所有的元素节点
        ''' </summary>
        Public Function Elements() As IEnumerable(Of HtmlNode)
            Dim list As New List(Of HtmlNode)()

            For Each n As HtmlNode In m_nodes
                If Queryable(n) Then
                    Call list.Add(n)
                End If
            Next

            Return list
        End Function

        ''' <summary>
        ''' 子节点之中指定标签名的所有的元素节点
        ''' </summary>
        Public Function Elements(tagName As String) As IEnumerable(Of HtmlNode)
            Return Elements().Where(Function(n) String.Equals(n.Name, tagName, StringComparison.OrdinalIgnoreCase))
        End Function

        ''' <summary>
        ''' 子节点之中指定标签名的第一个元素节点，不存在的时候返回 <c>Nothing</c>
        ''' </summary>
        Public Function Element(tagName As String) As HtmlNode
            Return Elements().FirstOrDefault(
                Function(n) String.Equals(n.Name, tagName, StringComparison.OrdinalIgnoreCase))
        End Function

        Private Shared Function Queryable(n As HtmlNode) As Boolean
            Select Case n.NodeType
                Case HtmlNodeType.Element, HtmlNodeType.Script, HtmlNodeType.Style
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' 所有的后代元素节点(不包含自身)，按照文档顺序排列
        ''' </summary>
        Public Iterator Function Descendants() As IEnumerable(Of HtmlNode)
            For Each child As HtmlNode In m_nodes
                If Queryable(child) Then
                    Yield child
                End If

                For Each d As HtmlNode In child.Descendants()
                    Yield d
                Next
            Next
        End Function

        ''' <summary>
        ''' 自身以及所有的后代元素节点，按照文档顺序排列
        ''' </summary>
        Public Iterator Function DescendantsAndSelf() As IEnumerable(Of HtmlNode)
            If Queryable(Me) Then
                Yield Me
            End If

            For Each d As HtmlNode In Descendants()
                Yield d
            Next
        End Function

        ''' <summary>
        ''' 从父节点开始一直到文档根节点的所有祖先节点
        ''' </summary>
        Public Iterator Function Ancestors() As IEnumerable(Of HtmlNode)
            Dim cur As HtmlNode = m_parent

            While cur IsNot Nothing
                If Queryable(cur) Then
                    Yield cur
                End If

                cur = cur.Parent
            End While
        End Function

        ''' <summary>
        ''' 前一个兄弟元素节点
        ''' </summary>
        Public Function PreviousSibling() As HtmlNode
            Return Sibling(-1)
        End Function

        ''' <summary>
        ''' 后一个兄弟元素节点
        ''' </summary>
        Public Function NextSibling() As HtmlNode
            Return Sibling(1)
        End Function

        Private Function Sibling(dir As Integer) As HtmlNode
            If m_parent Is Nothing Then
                Return Nothing
            End If

            Dim list As List(Of HtmlNode) = m_parent.m_nodes
            Dim idx As Integer = list.IndexOf(Me)

            If idx < 0 Then
                Return Nothing
            End If

            Dim i As Integer = idx + dir

            While i >= 0 AndAlso i < list.Count
                If Queryable(list(i)) Then
                    Return list(i)
                End If

                i += dir
            End While

            Return Nothing
        End Function

        ''' <summary>
        ''' 这个节点的内部 HTML 文本
        ''' </summary>
        Public ReadOnly Property InnerHtml As String
            Get
                Dim sb As New StringBuilder()

                For Each n As HtmlNode In m_nodes
                    Call sb.Append(n.OuterHtml)
                Next

                Return sb.ToString()
            End Get
        End Property

        ''' <summary>
        ''' 这个节点自身的完整 HTML 文本(包含自身的开始标签与结束标签)
        ''' </summary>
        Public ReadOnly Property OuterHtml As String
            Get
                Select Case NodeType
                    Case HtmlNodeType.Text
                        Return If(m_raw, "")
                    Case HtmlNodeType.Comment
                        Return "<!--" & If(m_raw, "") & "-->"
                    Case HtmlNodeType.CData
                        Return "<![CDATA[" & If(m_raw, "") & "]]>"
                    Case HtmlNodeType.Doctype
                        Return "<!" & If(m_raw, "") & ">"
                    Case HtmlNodeType.Document
                        Return InnerHtml
                End Select

                Dim sb As New StringBuilder()

                Call sb.Append("<").Append(Name)

                For Each attr As KeyValuePair(Of String, String) In attrs
                    Call sb.Append(" ").Append(attr.Key)

                    If attr.Value Is Nothing Then
                        Continue For
                    End If

                    Call sb.Append("=""") _
                          .Append(attr.Value.Replace("&", "&amp;").Replace("""", "&quot;")) _
                          .Append("""")
                Next

                If m_nodes.Count = 0 AndAlso HtmlTags.IsVoid(Name) Then
                    Return sb.Append(" />").ToString()
                End If

                Call sb.Append(">").Append(InnerHtml)
                Call sb.Append("</").Append(Name).Append(">")

                Return sb.ToString()
            End Get
        End Property

        ''' <summary>
        ''' 这个节点内部的纯文本内容：已经做过 HTML 实体解码以及空白字符归一化，
        ''' 并且自动的排除了 ``&lt;script&gt;`` ``&lt;style&gt;`` 与注释块。
        ''' </summary>
        ''' <remarks>
        ''' 文本会被归一化为单行文本，多个连续的空白字符会被折叠为一个空格。
        ''' 如果需要保留段落换行，请使用 <see cref="GetText"/> 方法。
        ''' </remarks>
        Public ReadOnly Property InnerText As String
            Get
                If m_innerText Is Nothing Then
                    m_innerText = BuildText(preserveBlocks:=False)
                End If

                Return m_innerText
            End Get
        End Property

        ''' <summary>
        ''' <see cref="InnerText"/> 的别名
        ''' </summary>
        Public ReadOnly Property Text As String
            Get
                Return InnerText
            End Get
        End Property

        ''' <summary>
        ''' 获取这个节点内部的纯文本内容
        ''' </summary>
        ''' <param name="preserveBlocks">
        ''' 为 <c>True</c> 的时候块级元素之间会保留换行符，可以用于文章的正文抽取；
        ''' 为 <c>False</c> 的时候所有的空白字符都会被折叠，返回单行文本。
        ''' </param>
        Public Function GetText(Optional preserveBlocks As Boolean = False) As String
            If preserveBlocks Then
                Return BuildText(preserveBlocks:=True)
            End If

            Return InnerText
        End Function

        Private Function BuildText(preserveBlocks As Boolean) As String
            Dim sb As New StringBuilder()

            Call AppendText(sb, Me, preserveBlocks)

            Return HtmlText.Normalize(sb.ToString(), preserveBlocks)
        End Function

        Private Shared Sub AppendText(sb As StringBuilder, node As HtmlNode, preserveBlocks As Boolean)
            Select Case node.NodeType
                Case HtmlNodeType.Text, HtmlNodeType.CData
                    Call sb.Append(node.m_raw)

                Case HtmlNodeType.Comment, HtmlNodeType.Doctype
                    ' 注释与文档声明不参与文本抽取

                Case HtmlNodeType.Script, HtmlNodeType.Style
                    ' 脚本与样式块之内的内容不是正文文本

                Case Else
                    If String.Equals(node.Name, "br", StringComparison.OrdinalIgnoreCase) Then
                        Call sb.Append(If(preserveBlocks, vbLf, " "))
                        Exit Sub
                    End If

                    Dim prev As HtmlNode = Nothing

                    For Each child As HtmlNode In node.m_nodes
                        If prev IsNot Nothing AndAlso (HtmlTags.IsBlock(prev) OrElse HtmlTags.IsBlock(child)) Then
                            Call sb.Append(If(preserveBlocks, vbLf, " "))
                        End If

                        Call AppendText(sb, child, preserveBlocks)
                        prev = child
                    Next
            End Select
        End Sub

        ''' <summary>
        ''' 在这个节点的后代之中使用类 CSS 的选择器做查询，返回所有命中的节点
        ''' </summary>
        ''' <param name="selector">
        ''' 类 CSS 的选择器，支持：
        ''' <list type="bullet">
        ''' <item>标签名，例如 <c>div</c>，<c>*</c> 匹配任意标签</item>
        ''' <item><c>.class</c> 类名</item>
        ''' <item><c>#id</c> 元素编号</item>
        ''' <item><c>[attr]</c> <c>[attr=value]</c> <c>[attr*=value]</c> <c>[attr^=value]</c> <c>[attr$=value]</c> <c>[attr~=value]</c></item>
        ''' <item>空格表示后代，<c>&gt;</c> 表示直接子元素</item>
        ''' <item>逗号分隔多个选择器表示并集</item>
        ''' </list>
        ''' </param>
        Public Function QuerySelectorAll(selector As String) As HtmlNode()
            Return HtmlSelector.Parse(selector).SelectNodes(Me)
        End Function

        ''' <summary>
        ''' 在这个节点的后代之中使用类 CSS 的选择器做查询，返回第一个命中的节点
        ''' </summary>
        Public Function QuerySelector(selector As String) As HtmlNode
            Return QuerySelectorAll(selector).FirstOrDefault()
        End Function

        ''' <summary>
        ''' 判断这个节点是否命中给定的选择器
        ''' </summary>
        Public Function Matches(selector As String) As Boolean
            Return HtmlSelector.Parse(selector).Matches(Me)
        End Function

        Public Overrides Function ToString() As String
            Select Case NodeType
                Case HtmlNodeType.Text
                    Return InnerText
                Case HtmlNodeType.Document
                    Return "#document"
                Case HtmlNodeType.Comment
                    Return "<!--" & If(m_raw, "") & "-->"
                Case Else
                    Return Name
            End Select
        End Function
    End Class

End Namespace
