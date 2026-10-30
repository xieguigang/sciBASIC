Imports System.Collections.Generic
Imports System.Linq

Namespace Html

    ''' <summary>
    ''' 属性匹配条件，对应选择器之中的 ``[attr]`` ``[attr=value]`` 这一类写法
    ''' </summary>
    Friend Class AttributeCondition

        Public Property Name As String
        Public Property Op As String
        Public Property Value As String

        Public Function Match(node As HtmlNode) As Boolean
            Dim has As Boolean = node.HasAttribute(Name)

            If Op = "" Then
                Return has
            End If
            If Not has Then
                Return False
            End If

            Dim v As String = node.Attribute(Name)

            Select Case Op
                Case "="
                    Return String.Equals(v, Value, StringComparison.OrdinalIgnoreCase)
                Case "*="
                    Return v IsNot Nothing AndAlso v.IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0
                Case "^="
                    Return v IsNot Nothing AndAlso v.StartsWith(Value, StringComparison.OrdinalIgnoreCase)
                Case "$="
                    Return v IsNot Nothing AndAlso v.EndsWith(Value, StringComparison.OrdinalIgnoreCase)
                Case "~="
                    Return v IsNot Nothing AndAlso
                        v.Split(" "c).Any(Function(x) String.Equals(x, Value, StringComparison.OrdinalIgnoreCase))
                Case Else
                    Return False
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return "[" & Name & Op & Value & "]"
        End Function
    End Class

    ''' <summary>
    ''' 选择器之中的一个复合步骤，例如 ``li.aca_algo``
    ''' </summary>
    Friend Class SelectorStep

        Public Property Tag As String = ""
        Public Property Id As String = ""
        Public ReadOnly Classes As New List(Of String)()
        Public ReadOnly Conditions As New List(Of AttributeCondition)()

        Public Function Match(node As HtmlNode) As Boolean
            If node Is Nothing Then
                Return False
            End If
            If Not node.IsElement Then
                Return False
            End If

            If Tag.Length > 0 AndAlso Tag <> "*" Then
                If Not String.Equals(Tag, node.Name, StringComparison.OrdinalIgnoreCase) Then
                    Return False
                End If
            End If

            If Id.Length > 0 AndAlso Not String.Equals(Id, node.Id, StringComparison.Ordinal) Then
                Return False
            End If

            For Each c As String In Classes
                If Not node.HasClass(c) Then
                    Return False
                End If
            Next

            For Each cond As AttributeCondition In Conditions
                If Not cond.Match(node) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New System.Text.StringBuilder()

            Call sb.Append(If(Tag.Length = 0, "*", Tag))

            If Id.Length > 0 Then
                Call sb.Append("#").Append(Id)
            End If
            For Each c As String In Classes
                Call sb.Append(".").Append(c)
            Next
            For Each cond As AttributeCondition In Conditions
                Call sb.Append(cond.ToString())
            Next

            Return sb.ToString()
        End Function
    End Class

    ''' <summary>
    ''' 类 CSS 的选择器引擎
    ''' </summary>
    ''' <remarks>
    ''' 支持的选择器语法：
    '''
    ''' <list type="bullet">
    ''' <item>标签名：<c>div</c>，<c>*</c> 匹配任意标签，自定义标签(例如 <c>acf-badge</c>)同样可以匹配</item>
    ''' <item>类名：<c>.aca_algo</c>，可以叠加多个类名</item>
    ''' <item>元素编号：<c>#b_results</c></item>
    ''' <item>属性：<c>[href]</c> <c>[class=aca_algo]</c> <c>[href*=bing]</c> <c>[href^=/academic]</c> <c>[href$=pdf]</c> <c>[class~=algo]</c></item>
    ''' <item>组合：空格表示后代(<c>li.aca_algo h2 a</c>)，<c>&gt;</c> 表示直接子元素(<c>div.aca_caption &gt; p</c>)</item>
    ''' <item>并集：逗号分隔(<c>li.aca_algo, li.b_algo</c>)</item>
    ''' </list>
    ''' </remarks>
    Public Class HtmlSelector

        ' groups(g) 之内的步骤之间使用 ops(g) 之中的组合符连接
        ' ops(g)(i) 是 groups(g)(i) 与 groups(g)(i + 1) 之间的组合符
        Private ReadOnly groups As New List(Of List(Of SelectorStep))()
        Private ReadOnly ops As New List(Of List(Of String))()

        ''' <summary>
        ''' 解析一个选择器表达式
        ''' </summary>
        ''' <param name="selector">选择器表达式，为空的时候返回一个不会命中任何节点的选择器</param>
        Public Shared Function Parse(selector As String) As HtmlSelector
            Dim s As New HtmlSelector()

            If String.IsNullOrWhiteSpace(selector) Then
                Return s
            End If

            For Each part As String In selector.Split(","c)
                If part.Trim().Length > 0 Then
                    Call ParseGroup(s, part.Trim())
                End If
            Next

            Return s
        End Function

        Private Shared Sub ParseGroup(s As HtmlSelector, text As String)
            Dim steps As New List(Of SelectorStep)()
            Dim combinators As New List(Of String)()
            Dim i As Integer = 0
            Dim n As Integer = text.Length
            Dim pendingOp As String = Nothing
            Dim cur As SelectorStep = Nothing

            While i < n
                Dim ch As Char = text(i)

                If Char.IsWhiteSpace(ch) Then
                    i += 1
                    Continue While
                End If
                If ch = ">"c Then
                    pendingOp = ">"
                    i += 1
                    Continue While
                End If

                If cur IsNot Nothing Then
                    Call combinators.Add(If(pendingOp, " "))
                End If

                pendingOp = Nothing
                cur = New SelectorStep()
                Call steps.Add(cur)

                ' 标签名（标签名之中不包含 '.'，否则 'li.aca_algo' 会被整个吃掉）
                If IsTagChar(ch) Then
                    Dim start As Integer = i

                    While i < n AndAlso IsTagChar(text(i))
                        i += 1
                    End While

                    cur.Tag = text.Substring(start, i - start)
                End If

                ' #id .class [attr]
                Dim done As Boolean = False

                While i < n AndAlso Not done
                    Dim c As Char = text(i)

                    Select Case c
                        Case "#"c
                            i += 1
                            Dim start As Integer = i
                            While i < n AndAlso IsNameChar(text(i))
                                i += 1
                            End While
                            cur.Id = text.Substring(start, i - start)

                        Case "."c
                            i += 1
                            Dim start As Integer = i
                            While i < n AndAlso IsNameChar(text(i))
                                i += 1
                            End While
                            Call cur.Classes.Add(text.Substring(start, i - start))

                        Case "["c
                            i += 1
                            Dim close As Integer = text.IndexOf("]"c, i)

                            If close < 0 Then
                                close = n
                            End If

                            Dim body As String = text.Substring(i, close - i)
                            i = close + 1

                            Call cur.Conditions.Add(ParseCondition(body))

                        Case Else
                            done = True
                    End Select
                End While
            End While

            If steps.Count > 0 Then
                Call s.groups.Add(steps)
                Call s.ops.Add(combinators)
            End If
        End Sub

        Private Shared Function ParseCondition(body As String) As AttributeCondition
            Dim cond As New AttributeCondition()

            body = If(body, "").Trim()

            For Each op As String In New String() {"~=", "^=", "$=", "*=", "="}
                Dim p As Integer = body.IndexOf(op, StringComparison.Ordinal)

                If p > 0 Then
                    cond.Name = body.Substring(0, p).Trim().ToLowerInvariant()
                    cond.Op = op
                    cond.Value = body.Substring(p + op.Length).Trim().Trim(""""c, "'"c)
                    Return cond
                End If
            Next

            cond.Name = body.ToLowerInvariant()
            cond.Op = ""
            cond.Value = Nothing

            Return cond
        End Function

        ''' <summary>
        ''' id 与 class 名称所允许出现的字符
        ''' </summary>
        Private Shared Function IsNameChar(ch As Char) As Boolean
            If Char.IsLetterOrDigit(ch) Then
                Return True
            End If

            Return ch = "-"c OrElse ch = "_"c OrElse ch = ":"c
        End Function

        ''' <summary>
        ''' 标签名所允许出现的字符；注意这里不能包含 '.'，否则类名会与标签名粘连在一起
        ''' </summary>
        Private Shared Function IsTagChar(ch As Char) As Boolean
            If Char.IsLetterOrDigit(ch) Then
                Return True
            End If

            Return ch = "-"c OrElse ch = "_"c OrElse ch = ":"c OrElse ch = "*"c
        End Function

        ''' <summary>
        ''' 在给定的节点的所有后代之中执行这个选择器，返回所有命中的节点
        ''' </summary>
        Public Function SelectNodes(root As HtmlNode) As HtmlNode()
            If root Is Nothing OrElse groups.Count = 0 Then
                Return New HtmlNode() {}
            End If

            Dim result As New List(Of HtmlNode)()

            For g As Integer = 0 To groups.Count - 1
                Dim steps As List(Of SelectorStep) = groups(g)
                Dim current As List(Of HtmlNode) = root.Descendants().ToList()

                For i As Integer = 0 To steps.Count - 1
                    If i > 0 Then
                        Dim isChild As Boolean = (ops(g)(i - 1) = ">")
                        Dim nextNodes As New List(Of HtmlNode)()

                        For Each n As HtmlNode In current
                            If isChild Then
                                Call nextNodes.AddRange(n.Elements())
                            Else
                                Call nextNodes.AddRange(n.Descendants())
                            End If
                        Next

                        current = nextNodes
                    End If

                    Dim stepI As SelectorStep = steps(i)

                    current = current.Where(Function(n) stepI.Match(n)).ToList()
                Next

                For Each n As HtmlNode In current
                    If Not result.Contains(n) Then
                        Call result.Add(n)
                    End If
                Next
            Next

            Return result.ToArray()
        End Function

        ''' <summary>
        ''' 判断给定的节点是否命中这个选择器(会自动的向上回溯检查祖先节点)
        ''' </summary>
        Public Function Matches(node As HtmlNode) As Boolean
            If node Is Nothing OrElse groups.Count = 0 Then
                Return False
            End If

            For g As Integer = 0 To groups.Count - 1
                If MatchGroup(groups(g), ops(g), node) Then
                    Return True
                End If
            Next

            Return False
        End Function

        Private Function MatchGroup(steps As List(Of SelectorStep), combinators As List(Of String), node As HtmlNode) As Boolean
            If steps.Count = 0 Then
                Return False
            End If
            If Not steps(steps.Count - 1).Match(node) Then
                Return False
            End If

            Dim cur As HtmlNode = node

            For i As Integer = steps.Count - 1 To 1 Step -1
                If combinators(i - 1) = ">" Then
                    cur = cur.Parent

                    If cur Is Nothing OrElse Not steps(i - 1).Match(cur) Then
                        Return False
                    End If
                Else
                    Dim hit As HtmlNode = Nothing
                    Dim p As HtmlNode = cur.Parent

                    While p IsNot Nothing
                        If steps(i - 1).Match(p) Then
                            hit = p
                            Exit While
                        End If

                        p = p.Parent
                    End While

                    If hit Is Nothing Then
                        Return False
                    End If

                    cur = hit
                End If
            Next

            Return True
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New System.Text.StringBuilder()

            For g As Integer = 0 To groups.Count - 1
                If g > 0 Then
                    Call sb.Append(", ")
                End If

                For i As Integer = 0 To groups(g).Count - 1
                    If i > 0 Then
                        Call sb.Append(If(ops(g)(i - 1) = ">", " > ", " "))
                    End If

                    Call sb.Append(groups(g)(i).ToString())
                Next
            Next

            Return sb.ToString()
        End Function
    End Class

End Namespace
