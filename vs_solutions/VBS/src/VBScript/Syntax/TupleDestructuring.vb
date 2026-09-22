#Region "Microsoft.VisualBasic::e573300fb4960ea5f45f8ed5b86b5237, vs_solutions\VBS\src\VBScript\TupleDestructuring.vb"

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

    '   Total Lines: 146
    '    Code Lines: 97 (66.44%)
    '   Comment Lines: 22 (15.07%)
    '    - Xml Docs: 68.18%
    ' 
    '   Blank Lines: 27 (18.49%)
    '     File Size: 5.59 KB


    '     Module TupleDestructuring
    ' 
    '         Function: Expand, HasTopLevelComma, NextTempName, TryExpandDeclaration, TryExpandForEach,
    '                   TryParseForEach, FindTopLevelAs, IndexOfComment, SplitTopLevel
    ' 
    '         Sub: ExpandNames, SplitLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Namespace Script

    ''' <summary>
    ''' 预处理 VB 源码之中的元组分解语法。
    '''
    ''' 支持的写法:
    '''   Dim (a, b) = expr                         ' 变量声明解构
    '''   Dim (a As Integer, b As String) = expr
    '''   Dim (a, , c) = expr                       ' 跳过中间项
    '''   Dim (a, (b, c)) = expr                    ' 嵌套分解
    '''   For Each (a, b) In expr                   ' 循环变量解构
    '''   For Each (a As Integer, b As String) In expr
    '''
    ''' 展开结果会保留原行的前导空白与行尾注释; 没有命中上述写法的行一律原样输出,
    ''' 因此 ``For Each x In list``、``For Each kv As KeyValuePair(Of K, V) In dict``
    ''' 这类普通写法不会被改写。
    ''' </summary>
    Public Module TupleDestructuring

        ''' <summary>
        ''' 预处理 VB 源码中的元组分解语法(保留行尾注释与前后空白)
        ''' </summary>
        Public Function Expand(source As String) As String
            Dim outLines As New List(Of String)
            ' 整个脚本共用一个序号, 保证 Dim(...) 与 For Each(...) 生成的临时变量名互不冲突
            Dim seq As Integer = 0

            For Each raw As String In source.LineTokens
                Dim lead As String = Nothing
                Dim body As String = Nothing
                Dim comment As String = Nothing

                ' 提取前导空白和行尾注释
                Call SplitLine(raw, lead, body, comment)

                If TryExpandDeclaration(body, lead, comment, outLines, seq) Then
                    Continue For
                End If

                If TryExpandForEach(body, lead, comment, outLines, seq) Then
                    Continue For
                End If

                Call outLines.Add(raw)
            Next

            Return String.Join(vbCrLf, outLines)
        End Function

        ''' <summary>
        ''' 拆分一行为 前导空白 / 正文 / 行尾注释 三部分
        ''' </summary>
        Private Sub SplitLine(raw As String, ByRef lead As String, ByRef body As String, ByRef comment As String)
            lead = Regex.Match(raw, "^\s*").Value
            body = raw.Substring(lead.Length)
            comment = ""

            Dim cIdx As Integer = IndexOfComment(body)

            If cIdx >= 0 Then
                comment = body.Substring(cIdx)
                body = body.Substring(0, cIdx).TrimEnd()
            End If
        End Sub

        ' ==================================================================
        ' 展开: Dim (names) = expr
        ' ==================================================================

        ''' <summary>
        ''' 尝试展开 <c>Dim (names) = expr</c> 形式的变量声明解构
        ''' </summary>
        Private Function TryExpandDeclaration(body As String, lead As String, comment As String,
                                              outLines As List(Of String), ByRef seq As Integer) As Boolean

            Dim m As Match = Regex.Match(
                body, "^Dim\s*\((?<names>.+)\)\s*=\s*(?<expr>.+)$",
                RegexOptions.IgnoreCase)

            If Not m.Success Then
                Return False
            End If

            Dim expr As String = m.Groups("expr").Value.Trim()
            Dim tmp As String = NextTempName(seq)
            Dim declIdx As Integer = 0

            ' ---- 生成临时元组变量声明 ----
            Call outLines.Add($"{lead}Dim {tmp} = {expr}{comment}")

            ' ---- 递归展开分解变量 ----
            Call ExpandNames(m.Groups("names").Value, tmp, lead, outLines, declIdx)

            Return True
        End Function

        ' ==================================================================
        ' 展开: For Each (names) In expr
        ' ==================================================================

        ''' <summary>
        ''' 尝试展开 <c>For Each (names) In expr</c> 形式的循环变量解构。
        ''' 展开为「普通 For Each + 循环体开头的逐项解构赋值」:
        ''' <code>
        ''' For Each (a, b) In tuples
        '''     body
        ''' Next
        ''' =>
        ''' For Each __tuple1 In tuples
        '''     Dim a = __tuple1.Item1
        '''     Dim b = __tuple1.Item2
        '''     body
        ''' Next
        ''' </code>
        ''' </summary>
        Private Function TryExpandForEach(body As String, lead As String, comment As String,
                                          outLines As List(Of String), ByRef seq As Integer) As Boolean

            Dim names As String = Nothing
            Dim expr As String = Nothing

            If Not TryParseForEach(body, names, expr) Then
                Return False
            End If

            ' 只有一个名字的括号只是普通的分组写法(For Each (x) In list), 不需要解构
            If Not HasTopLevelComma(names) Then
                Return False
            End If

            Dim tmp As String = NextTempName(seq)
            Dim declIdx As Integer = 0
            ' 解构语句位于循环体内部, 因此要多缩进一级
            Dim bodyIndent As String = lead & "    "

            ' ---- 循环变量替换为临时元组变量 ----
            Call outLines.Add($"{lead}For Each {tmp} In {expr}{comment}")

            ' ---- 在循环体开头逐项解构 ----
            Call ExpandNames(names, tmp, bodyIndent, outLines, declIdx)

            Return True
        End Function

        ''' <summary>
        ''' 解析 <c>For Each (names) In expr</c>:
        ''' 括号采用配平扫描定位(支持 <c>For Each ((a, b), c) In ...</c> 这样的嵌套解构),
        ''' 且要求配对的右括号之后必须紧跟 In 子句, 否则视为未命中。
        ''' </summary>
        Private Function TryParseForEach(body As String, ByRef names As String, ByRef expr As String) As Boolean
            names = Nothing
            expr = Nothing

            Dim head As Match = Regex.Match(body, "^For\s+Each\s*\(", RegexOptions.IgnoreCase)

            If Not head.Success Then
                Return False
            End If

            Dim start As Integer = head.Index + head.Length
            Dim depth As Integer = 1
            Dim [end] As Integer = -1

            For i As Integer = start To body.Length - 1
                Select Case body(i)
                    Case "("c
                        depth += 1
                    Case ")"c
                        depth -= 1

                        If depth = 0 Then
                            [end] = i
                            Exit For
                        End If
                End Select
            Next

            ' 括号不配平
            If [end] < 0 Then
                Return False
            End If

            Dim tail As Match = Regex.Match(body.Substring([end] + 1), "^\s*In\s+(?<expr>.+)$", RegexOptions.IgnoreCase)

            If Not tail.Success Then
                Return False
            End If

            names = body.Substring(start, [end] - start)
            expr = tail.Groups("expr").Value.Trim()

            Return True
        End Function

        ''' <summary>判定名字列表之中是否存在顶层逗号(即至少两个解构项)</summary>
        Private Function HasTopLevelComma(names As String) As Boolean
            Dim depth As Integer = 0

            For i As Integer = 0 To names.Length - 1
                Select Case names(i)
                    Case "("c
                        depth += 1
                    Case ")"c
                        depth -= 1
                    Case ","c
                        If depth = 0 Then
                            Return True
                        End If
                End Select
            Next

            Return False
        End Function

        ' ==================================================================
        ' 名字列表展开
        ' ==================================================================

        ''' <summary>
        ''' 递归展开左侧变量名列表(支持嵌套)
        ''' </summary>
        Private Sub ExpandNames(namesText As String, tmp As String,
                                lead As String, outLines As List(Of String),
                                ByRef index As Integer)

            ' 切分顶层逗号(忽略括号内的逗号)
            For Each part As String In SplitTopLevel(namesText)
                Dim p As String = part.Trim()

                If String.IsNullOrEmpty(p) OrElse p = "_"c Then
                    index += 1
                    Continue For
                End If

                ' ---- 嵌套元组: 递归展开 ----
                If p.StartsWith("("c) Then
                    Dim inner As String = p.Trim("("c, ")"c)
                    Dim nestedTmp As String = $"{tmp}_n{index}"

                    Call outLines.Add($"{lead}Dim {nestedTmp} = {tmp}.Item{index + 1}")
                    Call ExpandNames(inner, nestedTmp, lead, outLines, 0)

                    index += 1
                    Continue For
                End If

                ' ---- 命名别名: alias:=varName ----
                Dim varName As String = p
                Dim colonIdx As Integer = p.IndexOf(":"c)
                If colonIdx > 0 Then
                    varName = p.Substring(colonIdx + 1).Trim()
                End If

                ' ---- 类型声明: varName As Type ----
                Dim typeClause As String = ""
                Dim asIdx As Integer = FindTopLevelAs(varName)
                If asIdx >= 0 Then
                    typeClause = " " & varName.Substring(asIdx).Trim()
                    varName = varName.Substring(0, asIdx).Trim()
                End If

                Call outLines.Add(
                    $"{lead}Dim {varName}{typeClause} = {tmp}.Item{index + 1}")

                index += 1
            Next
        End Sub

        ''' <summary>
        ''' 生成一个唯一的临时元组变量名。
        ''' 注意: VB 之中并不存在 ``++`` 前缀自增运算符(``++i`` 会被解析为一元 ``+(+i)``,
        ''' 不会产生递增的效果), 所以这里必须显式的进行自增, 否则同一脚本之中的
        ''' 多次解构会生成同名的临时变量而导致重定义错误。
        ''' </summary>
        Private Function NextTempName(ByRef seq As Integer) As String
            seq += 1

            Return $"__tuple{seq}"
        End Function

        ' ==================================================================
        ' 基础文本工具
        ' ==================================================================

        ''' <summary>按顶层逗号切分(不切括号内的)</summary>
        Private Iterator Function SplitTopLevel(s As String) As IEnumerable(Of String)
            Dim depth As Integer = 0
            Dim start As Integer = 0

            For i As Integer = 0 To s.Length - 1
                Select Case s(i)
                    Case "("c : depth += 1
                    Case ")"c : depth -= 1
                    Case ","c
                        If depth = 0 Then
                            Yield s.Substring(start, i - start)
                            start = i + 1
                        End If
                End Select
            Next

            Yield s.Substring(start)
        End Function

        ''' <summary>
        ''' 定位 As 关键字的起始位置(前导空白处); 没有类型子句时返回 -1。
        ''' 注意: Match.Index 在匹配失败时返回的是 0 而不是 -1, 所以必须显式判断 Success。
        ''' </summary>
        Private Function FindTopLevelAs(s As String) As Integer
            Dim m As Match = Regex.Match(s, "\s+As\s+", RegexOptions.IgnoreCase)

            If m.Success Then
                Return m.Index
            Else
                Return -1
            End If
        End Function

        ''' <summary>定位行尾注释的起点(排除字符串内的)</summary>
        Private Function IndexOfComment(s As String) As Integer
            Dim inStr As Boolean = False

            For i As Integer = 0 To s.Length - 1
                Dim c As Char = s(i)
                If c = """"c AndAlso i > 0 AndAlso s(i - 1) <> "\"c Then
                    inStr = Not inStr
                ElseIf c = "'"c AndAlso Not inStr Then
                    Return i
                End If
            Next

            Return -1
        End Function
    End Module

End Namespace
