#Region "Microsoft.VisualBasic::9b318db41d73d9ece59fb6c5d8af9d95, vs_solutions\VBS\src\VBScript\Syntax\Vectorization\PropertyProjection.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 340
    '    Code Lines: 199 (58.53%)
    ' Comment Lines: 75 (22.06%)
    '    - Xml Docs: 74.67%
    ' 
    '   Blank Lines: 66 (19.41%)
    '     File Size: 14.27 KB


    '     Class PropertyProjection
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ExpandLine, MaskLiterals, MatchingBrace, ParseMemberList, SkipSpaces
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions

Namespace Script

    ''' <summary>
    ''' <c>@</c> 数组投影运算符的文本级展开器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <c>list@x</c> 展开为 <c>list.Select(Function(__vbs_o) __vbs_o.x).ToArray()</c>;
    ''' <c>list@{x, y}</c> 展开为
    ''' <c>list.Select(Function(__vbs_o) New With {.x = __vbs_o.x, .y = __vbs_o.y}).ToArray()</c>。
    ''' </para>
    ''' <para>
    ''' <b>为什么必须在 Roslyn 解析之前做</b>: <c>@</c> 在 VB 里不是合法字符
    ''' (它只在数值字面量后缀 <c>1.5@</c> 里作为 Decimal 类型字符出现),
    ''' 含 <c>@</c> 的行根本无法被解析成语法树, 因此只能先做文本变换。
    ''' 又因为 <c>@</c> 投影要求左侧是「标识符点号链」, 而 Decimal 后缀左侧必然是数字,
    ''' 所以两者不会互相干扰。
    ''' </para>
    ''' <para>
    ''' <b>保守策略</b>: 只有「左侧元素类型能从脚本声明解析出来 + 该类型确实声明了所请求的成员」
    ''' 才展开; 任何一项不满足都**不生成猜测性代码**, 该 <c>@</c> 原样保留(脚本按原有方式报语法错误)。
    ''' 判别粒度是**逐个 <c>@</c>**: 同一行里如果有一个可展开、另一个不可展开,
    ''' 可展开的那个照常展开, 不可展开的那个留在原文里 —— 由于含未展开 <c>@</c> 的行本来
    ''' 就无法通过编译, 这种"部分展开"不会改变任何原本可编译脚本的行为。
    ''' </para>
    ''' <para>
    ''' <b>支持与不支持</b>:
    ''' <list type="bullet">
    ''' <item>支持: 单成员 <c>list@x</c>、多成员 <c>list@{x, y}</c>、链式 <c>list@inner@x</c>、
    ''' 以及投影结果后继续写成员调用 <c>list@x.Sum()</c>;</item>
    ''' <item>不支持: 左侧是调用或下标(<c>GetList()@x</c>、<c>list(0)@x</c>)、
    ''' 标量对象的 <c>obj@x</c>(请写 <c>obj.x</c>)、字符串字面量与行尾注释内部的 <c>@</c>
    ''' (含字符串插值 <c>$"..."</c> 内部)。</item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    Public Class PropertyProjection

        ''' <summary>展开之后 lambda 参数的固定名字(前缀化以降低与脚本变量重名的概率)</summary>
        Public Const LambdaParameter As String = "__vbs_o"

        ''' <summary>左操作数: 标识符点号链(允许点号两侧有空白)</summary>
        Private ReadOnly ChainPattern As New Regex(
            "(?<chain>[A-Za-z_]\w*(?:\s*\.\s*[A-Za-z_]\w*)*)\s*$",
            RegexOptions.IgnoreCase)

        ''' <summary>右操作数: 单个成员名(不锚定结尾, 以便 <c>list@x.Sum()</c> 之后仍可续写)</summary>
        Private ReadOnly MemberNamePattern As New Regex("^[A-Za-z_]\w*", RegexOptions.IgnoreCase)

        ''' <summary><c>@{x, y}</c> 之中的单个成员名(必须整体匹配)</summary>
        Private ReadOnly MemberItemPattern As New Regex("^[A-Za-z_]\w*$", RegexOptions.IgnoreCase)

        Private ReadOnly _members As ObjectMemberTable

        Public Sub New(members As ObjectMemberTable)
            _members = If(members, ObjectMemberTable.FromTypeBlocks(Nothing))
        End Sub

        ''' <summary>
        ''' 展开一行源码之中的全部 <c>@</c> 投影; 一个都无法展开时原样返回。
        ''' </summary>
        ''' <param name="line">一行脚本源码</param>
        ''' <param name="types">当前的符号类型表(变量名 → 浅层类型)</param>
        ''' <param name="report">改写报告(可以为 Nothing)</param>
        Public Function ExpandLine(line As String,
                                   types As Dictionary(Of String, ValueTypeInfo),
                                   report As VectorizationReport) As String

            If String.IsNullOrEmpty(line) OrElse line.IndexOf("@"c) < 0 Then
                Return line
            End If

            ' 掩码与原文等长: 字符串字面量/注释内部被替换为空格,
            ' 因此可以直接在掩码上定位 `@` 并把偏移用回原文。
            Dim masked As String = MaskLiterals(line)

            If masked.IndexOf("@"c) < 0 Then
                Return line
            End If

            Dim sb As New StringBuilder()
            Dim pos As Integer = 0
            Dim search As Integer = 0
            Dim expanded As Integer = 0

            ' 上一个成功展开的投影在输出缓冲之中的结束位置与结果类型;
            ' 用于支持链式 `list@inner@x` —— 此时左操作数就是上一个投影生成的文本,
            ' 它已经写进 sb 里, 因此只需追加后缀。
            Dim previousEnd As Integer = -1
            Dim previousType As New ValueTypeInfo()

            While search < masked.Length
                Dim at As Integer = masked.IndexOf("@"c, search)

                If at < 0 Then
                    Exit While
                End If

                ' ---- 右操作数 ----
                Dim memberNames As String() = Nothing
                Dim rightEnd As Integer = -1
                Dim braced As Boolean = False
                Dim cursor As Integer = SkipSpaces(masked, at + 1)

                If cursor < masked.Length AndAlso masked(cursor) = "{"c Then
                    Dim closing As Integer = MatchingBrace(masked, cursor)

                    If closing > 0 Then
                        memberNames = ParseMemberList(masked.Substring(cursor + 1, closing - cursor - 1))
                        rightEnd = closing + 1
                        braced = True
                    End If
                Else
                    Dim m As Match = MemberNamePattern.Match(masked.Substring(cursor))

                    If m.Success Then
                        memberNames = {m.Value}
                        rightEnd = cursor + m.Length
                    End If
                End If

                If memberNames Is Nothing OrElse rightEnd < 0 Then
                    search = at + 1
                    Continue While
                End If

                ' ---- 左操作数 ----
                Dim leftType As New ValueTypeInfo()
                Dim chainStart As Integer = -1
                Dim chained As Boolean = False
                Dim gap As String = line.Substring(pos, at - pos)

                If previousEnd = sb.Length AndAlso String.IsNullOrWhiteSpace(gap) Then
                    leftType = previousType
                    chained = True
                Else
                    Dim chain As Match = ChainPattern.Match(masked.Substring(0, at))

                    If chain.Success Then
                        chainStart = chain.Groups("chain").Index

                        Dim name As String = Regex.Replace(chain.Groups("chain").Value, "\s", "")
                        Dim resolved As ValueTypeInfo = Nothing

                        If types.TryGetValue(name, resolved) Then
                            leftType = resolved
                        End If
                    End If
                End If

                ' 元素类型必须是"具名类型的一维数组"(IsObjectVector), 否则不展开
                If Not leftType.IsObjectVector Then
                    search = at + 1
                    Continue While
                End If

                ' ---- 所请求的成员必须都存在 ----
                Dim memberTypes As New List(Of ValueTypeInfo)
                Dim resolvable As Boolean = True

                For Each memberName As String In memberNames
                    Dim info As ValueTypeInfo = Nothing

                    If Not _members.TryGetMember(leftType.ElementName, memberName, info) Then
                        resolvable = False
                        Exit For
                    End If

                    Call memberTypes.Add(info)
                Next

                If Not resolvable Then
                    search = at + 1
                    Continue While
                End If

                ' ---- 生成 ----
                Dim generated As String
                Dim resultType As ValueTypeInfo

                If Not braced AndAlso memberNames.Length = 1 Then
                    ' 裸成员形式 list@x => 直接给出该成员的值数组(元素类型可继续推断)
                    generated = $".Select(Function({LambdaParameter}) {LambdaParameter}.{memberNames(0)}).ToArray()"

                    If memberTypes(0).IsVector Then
                        ' 成员本身是数组 => 投影结果是交错数组, 无法继续推断
                        resultType = New ValueTypeInfo()
                    Else
                        resultType = New ValueTypeInfo(memberTypes(0).Kind, True, memberTypes(0).ElementName)
                    End If
                Else
                    ' 花括号形式 list@{x} / list@{x, y} => 匿名类型数组(元素类型不可命名)
                    Dim fields As String = String.Join(", ",
                        memberNames.Select(Function(n) $".{n} = {LambdaParameter}.{n}"))

                    generated = $".Select(Function({LambdaParameter}) New With {{{fields}}}).ToArray()"
                    resultType = New ValueTypeInfo()
                End If

                ' ---- 回写 ----
                If chained Then
                    ' 接收者已在上一次展开的生成文本里
                    Call sb.Append(gap)
                Else
                    Dim leftText As String = line.Substring(chainStart, at - chainStart).TrimEnd()

                    Call sb.Append(line.Substring(pos, chainStart - pos))
                    Call sb.Append(leftText)
                End If

                Call sb.Append(generated)

                pos = rightEnd
                search = rightEnd
                expanded += 1
                previousEnd = sb.Length
                previousType = resultType
            End While

            If expanded = 0 Then
                Return line
            End If

            Call sb.Append(line.Substring(pos))

            If report IsNot Nothing Then
                report.Projections += expanded
            End If

            Return sb.ToString()
        End Function

        ' ==================================================================
        ' 文本工具
        ' ==================================================================

        ''' <summary>
        ''' 把字符串字面量与行尾注释替换为等长的空格, 得到一个可用于定位运算符的"掩码"。
        ''' </summary>
        ''' <remarks>
        ''' 等长是关键: 掩码上的偏移可以直接用于原文, 不需要维护偏移映射。
        ''' VB 的字符串用两个连续双引号表示一个双引号字符, 因此按 <c>""</c> 成对跳过即可。
        ''' 改写器也复用它来避免把字符串/注释里的内容当成声明。
        ''' </remarks>
        Friend Shared Function MaskLiterals(line As String) As String
            Dim chars As Char() = line.ToCharArray()
            Dim inString As Boolean = False
            Dim i As Integer = 0

            While i < chars.Length
                Dim c As Char = chars(i)

                If inString Then
                    If c = """"c Then
                        If i + 1 < chars.Length AndAlso chars(i + 1) = """"c Then
                            chars(i) = " "c
                            chars(i + 1) = " "c
                            i += 2
                            Continue While
                        End If

                        inString = False
                    End If

                    chars(i) = " "c
                ElseIf c = """"c Then
                    inString = True
                    chars(i) = " "c
                ElseIf c = "'"c Then
                    ' 行尾注释: 其后全部屏蔽
                    For j As Integer = i To chars.Length - 1
                        chars(j) = " "c
                    Next

                    Exit While
                End If

                i += 1
            End While

            Return New String(chars)
        End Function

        Private Shared Function SkipSpaces(text As String, start As Integer) As Integer
            Dim i As Integer = start

            While i < text.Length AndAlso Char.IsWhiteSpace(text(i))
                i += 1
            End While

            Return i
        End Function

        ''' <summary>定位与 <paramref name="openAt"/> 处左花括号配平的右花括号; 不配平返回 -1</summary>
        Private Shared Function MatchingBrace(text As String, openAt As Integer) As Integer
            Dim depth As Integer = 0

            For i As Integer = openAt To text.Length - 1
                Select Case text(i)
                    Case "{"c
                        depth += 1
                    Case "}"c
                        depth -= 1

                        If depth = 0 Then
                            Return i
                        End If
                End Select
            Next

            Return -1
        End Function

        ''' <summary>
        ''' 切分 <c>@{x, y}</c> 之中的成员名; 出现任何非标识符项时返回 <c>Nothing</c>(整条不展开)。
        ''' </summary>
        Private Function ParseMemberList(inner As String) As String()
            Dim names As New List(Of String)

            For Each item As String In ScriptStructure.SplitTopLevel(inner)
                Dim name As String = item.Trim()

                If Not MemberItemPattern.IsMatch(name) Then
                    Return Nothing
                End If

                Call names.Add(name)
            Next

            If names.Count = 0 Then
                Return Nothing
            End If

            Return names.ToArray()
        End Function
    End Class
End Namespace

