Imports System.Collections.Generic

Namespace Html

    ''' <summary>
    ''' 基于标签栈的 HTML 文档树构建器
    ''' </summary>
    ''' <remarks>
    ''' 构建器是宽容的(lenient)，对于真实世界之中常见的畸形文档不会抛出异常：
    '''
    ''' <list type="bullet">
    ''' <item>没有开始标签的孤儿结束标签会被忽略掉</item>
    ''' <item>文档结束的时候标签栈之中还没有闭合的元素会被自动的闭合</item>
    ''' <item>``&lt;li&gt;`` ``&lt;p&gt;`` 这类标签会隐式的闭合掉同级的还没有结束的同类型标签</item>
    ''' </list>
    ''' </remarks>
    Friend Module HtmlDocumentBuilder

        ''' <summary>
        ''' 将词法记号序列构建为一棵文档树
        ''' </summary>
        Friend Function Build(tokens As List(Of HtmlToken)) As HtmlNode
            Dim root As New HtmlNode(HtmlNodeType.Document, "#document")
            ' 标签栈，栈顶元素就是当前正在填充子节点的元素
            Dim stack As New List(Of HtmlNode) From {root}

            For Each token As HtmlToken In tokens
                Dim top As HtmlNode = stack(stack.Count - 1)

                Select Case token.Type
                    Case HtmlTokenType.Text
                        Call AddChild(top, TextNode(HtmlNodeType.Text, "#text", token.Text))

                    Case HtmlTokenType.Comment
                        Call AddChild(top, TextNode(HtmlNodeType.Comment, "#comment", token.Text))

                    Case HtmlTokenType.CData
                        Call AddChild(top, TextNode(HtmlNodeType.CData, "#cdata", token.Text))

                    Case HtmlTokenType.Doctype
                        Call AddChild(top, TextNode(HtmlNodeType.Doctype, "#doctype", token.Text))

                    Case HtmlTokenType.SelfClosingTag
                        Call AddChild(top, ElementNode(top, token.Name, token.Attributes))

                    Case HtmlTokenType.OpenTag
                        ' 新标签可能会隐式的闭合掉一些还没有结束的同级标签
                        Call CloseImplicit(stack, token.Name)

                        Dim node As HtmlNode = ElementNode(stack(stack.Count - 1), token.Name, token.Attributes)

                        Call stack(stack.Count - 1).Add(node)
                        Call stack.Add(node)

                    Case HtmlTokenType.CloseTag
                        Call CloseTag(stack, token.Name)
                End Select
            Next

            Return root
        End Function

        Private Function TextNode(type As HtmlNodeType, name As String, text As String) As HtmlNode
            Dim node As New HtmlNode(type, name)

            Call node.SetText(text)

            Return node
        End Function

        Private Function ElementNode(parent As HtmlNode, name As String, attrs As List(Of KeyValuePair(Of String, String))) As HtmlNode
            Dim type As HtmlNodeType = HtmlNodeType.Element

            If String.Equals(name, "script", StringComparison.OrdinalIgnoreCase) Then
                type = HtmlNodeType.Script
            ElseIf String.Equals(name, "style", StringComparison.OrdinalIgnoreCase) Then
                type = HtmlNodeType.Style
            End If

            Dim node As New HtmlNode(type, name)

            Call node.SetAttrs(attrs)

            If parent IsNot Nothing Then
                Call node.SetParent(parent)
            End If

            Return node
        End Function

        Private Sub AddChild(parent As HtmlNode, child As HtmlNode)
            If parent Is Nothing Then
                Return
            End If

            Call child.SetParent(parent)
            Call parent.Add(child)
        End Sub

        ''' <summary>
        ''' 应用隐式闭合规则：新打开的标签会闭合掉栈顶那些它可以隐式闭合的标签
        ''' </summary>
        Private Sub CloseImplicit(stack As List(Of HtmlNode), name As String)
            If stack.Count <= 1 Then
                Return
            End If

            Dim closes As String() = Nothing

            If Not HtmlTags.ImplicitClose.TryGetValue(name, closes) Then
                Return
            End If

            While stack.Count > 1
                Dim top As String = stack(stack.Count - 1).Name

                If Array.IndexOf(closes, top) < 0 Then
                    Exit While
                End If

                Call stack.RemoveAt(stack.Count - 1)
            End While
        End Sub

        ''' <summary>
        ''' 处理结束标签：从栈顶向下回溯查找同名元素，找到之后将栈上它以及它之上的元素全部弹出
        ''' </summary>
        Private Sub CloseTag(stack As List(Of HtmlNode), name As String)
            For i As Integer = stack.Count - 1 To 1 Step -1
                If String.Equals(stack(i).Name, name, StringComparison.Ordinal) Then
                    Call stack.RemoveRange(i, stack.Count - i)
                    Return
                End If
            Next

            ' 这是一个没有对应开始标签的孤儿结束标签，直接忽略掉
        End Sub
    End Module

End Namespace
