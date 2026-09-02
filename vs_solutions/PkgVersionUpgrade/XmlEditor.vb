Imports System.Xml.Linq

''' <summary>
''' vbproj XML 文档的原地编辑原语
''' </summary>
''' <remarks>
''' 本模块只提供与具体业务无关的 XML 操作能力，不包含任何版本号、
''' 目标框架或者编译配置的语义判断。
'''
''' 框架内的 vbproj 是以 <see cref="LoadOptions.PreserveWhitespace"/> 方式加载的，
''' 元素之间的空白全部作为 <see cref="XText"/> 节点保留在文档树里，
''' 所以这里所有新增节点的操作都必须自己补上缩进用的空白文本节点，
''' 并且沿用文件里既有的缩进风格 —— 框架内混用了 Tab 与空格缩进，
''' 由 <see cref="InferChildIndent"/> 自动探测。
''' </remarks>
Module XmlEditor

    ''' <summary>
    ''' 取出主 PropertyGroup（第一个不带 Condition 属性的 PropertyGroup）
    ''' </summary>
    ''' <param name="doc">原始文档。</param>
    ''' <param name="ns">文档根元素的命名空间。</param>
    ''' <returns>工程里不存在无条件属性组时，自动新建一个插入到根元素首位后返回。</returns>
    Public Function MainPropertyGroup(doc As XDocument, ns As XNamespace) As XElement
        For Each pg As XElement In doc.Root.Elements(ns + "PropertyGroup")
            If pg.Attribute("Condition") Is Nothing Then
                Return pg
            End If
        Next

        Dim created As New XElement(ns + "PropertyGroup")

        ' 缩进用的空白文本节点，AddElement 会把新元素插入到它前面
        created.Add(New XText(vbLf & "  "))
        doc.Root.AddFirst(New XText(vbLf))
        doc.Root.AddFirst(created)

        Return created
    End Function

    ''' <summary>
    ''' 推断出容器内部子元素的缩进字符串（换行符 + 缩进）
    ''' </summary>
    ''' <param name="parent">要探测的容器元素。</param>
    ''' <returns>默认回退到两个空格缩进。</returns>
    Public Function InferChildIndent(parent As XElement) As String
        If parent Is Nothing Then
            Return vbLf & "  "
        End If

        For Each node As XNode In parent.Nodes()
            If TypeOf node Is XText Then
                Dim text As String = DirectCast(node, XText).Value

                If text.Contains(vbLf) Then
                    Return text.Substring(text.LastIndexOf(vbLf))
                End If
            End If
        Next

        Return vbLf & "  "
    End Function

    ''' <summary>
    ''' 推断出「容器内的子元素」其自身内部再嵌套一层时应该使用的缩进
    ''' </summary>
    ''' <param name="parent">容器元素，一般是文档根元素。</param>
    ''' <param name="childName">参考用的子元素名，例如 ``PropertyGroup``。</param>
    ''' <param name="ns">命名空间。</param>
    ''' <returns>参考不到时回退到父级缩进再加两个空格。</returns>
    Public Function InferInnerIndent(parent As XElement, ns As XNamespace, childName As String) As String
        If parent IsNot Nothing Then
            For Each child As XElement In parent.Elements(ns + childName)
                If child.HasElements Then
                    Return InferChildIndent(child)
                End If
            Next
        End If

        Return InferChildIndent(parent) & "  "
    End Function

    ''' <summary>
    ''' 向容器末尾追加一个属性元素，并且尽量保持原有的缩进风格
    ''' </summary>
    Public Function AddElement(parent As XElement, ns As XNamespace, name As String, value As String) As XElement
        Dim indent As String = InferChildIndent(parent)
        Dim tail As XNode = Nothing

        ' 容器的最后一个子节点一般是结束标签之前的空白文本，
        ' 新元素需要插入到这个空白节点之前，否则结束标签会被挤到元素后面去
        If TypeOf parent.LastNode Is XText AndAlso DirectCast(parent.LastNode, XText).Value.Trim().Length = 0 Then
            tail = parent.LastNode
        End If

        Dim el As New XElement(ns + name, value)

        If tail Is Nothing Then
            parent.Add(New XText(indent))
            parent.Add(el)
        Else
            tail.AddBeforeSelf(New XText(indent))
            tail.AddBeforeSelf(el)
        End If

        Return el
    End Function

    ''' <summary>
    ''' 设置已有属性的值，属性不存在的时候按开关决定是否新建
    ''' </summary>
    ''' <param name="parent">属性所在的 PropertyGroup。</param>
    ''' <param name="ns">命名空间。</param>
    ''' <param name="name">属性元素名。</param>
    ''' <param name="value">新的属性值。</param>
    ''' <param name="allowInsert">元素不存在时是否允许新建。</param>
    ''' <returns>是否发生了改动、是否是新建出来的、以及改动之前的值。</returns>
    Public Function SetOrCreateElement(parent As XElement,
                                       ns As XNamespace,
                                       name As String,
                                       value As String,
                                       allowInsert As Boolean) As (Changed As Boolean, Inserted As Boolean, OldValue As String)

        Dim el As XElement = parent.Element(ns + name)

        If el Is Nothing Then
            If Not allowInsert Then
                Return (False, False, "")
            End If

            Call AddElement(parent, ns, name, value)

            Return (True, True, "")
        End If

        Dim oldValue As String = If(el.Value, "").Trim()

        If String.Equals(oldValue, value, StringComparison.Ordinal) Then
            Return (False, False, oldValue)
        End If

        el.Value = value

        Return (True, False, oldValue)
    End Function

    ''' <summary>
    ''' 把一个已经构造好的元素插入到锚点元素之后，前后补上空白与缩进
    ''' </summary>
    ''' <param name="anchor">锚点元素，一般取文档中最后一个同类元素。</param>
    ''' <param name="element">待插入的元素。</param>
    Public Sub InsertAfter(anchor As XElement, element As XElement)
        Dim parent As XElement = anchor.Parent
        Dim indent As String = InferChildIndent(parent)
        Dim tail As XNode = anchor.NextNode

        If tail Is Nothing Then
            parent.Add(New XText(vbLf))
            parent.Add(New XText(indent))
            parent.Add(element)
        Else
            ' 空行分隔，再补上缩进，最后才是新元素本身。
            ' AddBeforeSelf 会按调用顺序依次插入到 tail 前面，顺序即书写顺序
            tail.AddBeforeSelf(New XText(vbLf))
            tail.AddBeforeSelf(New XText(indent))
            tail.AddBeforeSelf(element)
        End If
    End Sub

    ''' <summary>
    ''' 移除元素，并且顺带移除紧跟在它后面的空白文本节点，避免留下成片的空行
    ''' </summary>
    Public Sub RemoveNode(element As XElement)
        Dim [next] As XNode = element.NextNode

        If TypeOf [next] Is XText AndAlso
            DirectCast([next], XText).Value.Trim().Length = 0 AndAlso
            [next].NextNode IsNot Nothing Then

            [next].Remove()
        End If

        element.Remove()
    End Sub

End Module
