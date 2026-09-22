Imports System.Text
Imports System.Text.RegularExpressions

Namespace Script

    ''' <summary>
    ''' 将脚本之中的 <c>let</c> 动态类型声明语法展开为 <c>Dim ... As Object</c> 声明。
    ''' </summary>
    ''' <remarks>
    ''' 脚本使用者采用 <c>Dim x = "..."</c> 时由 Roslyn 自动推断为强类型;
    ''' 而采用 <c>let x = "..."</c> 时, 引擎在预处理阶段将其重构为
    ''' <c>Dim x As Object = "..."</c>, 从而得到一个可以在运行时动态绑定成员的变量。
    ''' <para/>
    ''' 需要与 LINQ 查询表达式之中的 <c>Let</c> 子句区分: 查询表达式以
    ''' <c>From ... In ...</c> 开始、以 <c>Select</c> / <c>Group</c> 子句结束,
    ''' 本模块通过一个逐行的查询状态机来避免误改写 LINQ 的 <c>Let</c> 子句。
    ''' </remarks>
    Public Module LetStatement

        ''' <summary>行首 let 声明: let name = expr</summary>
        Private ReadOnly LetPattern As New Regex(
            "^(?<lead>\s*)let\s+(?<name>[A-Za-z_]\w*)\s*=\s*(?<expr>.+?)\s*$",
            RegexOptions.IgnoreCase)

        ''' <summary>LINQ 查询表达式的开始: From ... In ...</summary>
        Private ReadOnly QueryStart As New Regex(
            "\bfrom\s+[A-Za-z_]\w*\s+in\b", RegexOptions.IgnoreCase)

        ''' <summary>LINQ 查询表达式的终止子句(位于行首): Select / Group</summary>
        Private ReadOnly QueryEnd As New Regex(
            "^\s*(select|group)\b", RegexOptions.IgnoreCase)

        ''' <summary>任意位置出现的查询终止关键字(用于识别单行查询)</summary>
        Private ReadOnly QueryEndKeyword As New Regex(
            "\b(select|group)\b", RegexOptions.IgnoreCase)

        ''' <summary>
        ''' 展开脚本源代码之中的 let 动态类型声明, 保留前导空白与行尾注释。
        ''' </summary>
        Public Function Expand(source As String) As String
            Dim outLines As New List(Of String)
            Dim inQuery As Boolean = False

            For Each raw As String In source.LineTokens
                Dim detect As String = DetectText(raw).Trim()

                If inQuery Then
                    If QueryEnd.IsMatch(detect) Then
                        inQuery = False
                    End If
                Else
                    Dim mStart As Match = QueryStart.Match(detect)

                    If mStart.Success Then
                        ' 单行查询: From 之后同一行即出现 Select / Group 终止子句
                        Dim tail As String = detect.Substring(mStart.Index + mStart.Length)
                        inQuery = Not QueryEndKeyword.IsMatch(tail)
                    End If
                End If

                Call outLines.Add(If(inQuery, raw, Rewrite(raw)))
            Next

            Return String.Join(vbCrLf, outLines)
        End Function

        ''' <summary>
        ''' 将单行的 <c>let name = expr</c> 重写为 <c>Dim name As Object = expr</c>;
        ''' 非 let 声明原样返回。
        ''' </summary>
        Private Function Rewrite(line As String) As String
            Dim m As Match = LetPattern.Match(line)

            If Not m.Success Then
                Return line
            End If

            Return $"{m.Groups("lead").Value}Dim {m.Groups("name").Value} As Object = {m.Groups("expr").Value}"
        End Function

        ''' <summary>
        ''' 提取用于结构检测的有效文本: 移除字符串字面量内容与注释,
        ''' 避免自然语言字符串之中的 "from ... in ..." 干扰查询状态机。
        ''' </summary>
        Private Function DetectText(line As String) As String
            Dim sb As New StringBuilder()
            Dim inString As Boolean = False
            Dim i As Integer = 0

            While i < line.Length
                Dim c As Char = line(i)

                If inString Then
                    If c = """"c Then
                        If i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                            i += 2
                            Continue While
                        End If

                        inString = False
                    End If
                Else
                    If c = """"c Then
                        inString = True
                    ElseIf c = "'"c Then
                        Exit While
                    Else
                        Call sb.Append(c)
                    End If
                End If

                i += 1
            End While

            Return sb.ToString()
        End Function
    End Module
End Namespace
