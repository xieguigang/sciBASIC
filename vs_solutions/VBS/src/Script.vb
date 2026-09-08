Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine

Module Script

    ' =========================================================================
    ' 函数1: 脚本源代码文件解析
    ' =========================================================================

    ''' <summary>
    ''' 对VB.NET脚本源代码文件进行解析处理
    ''' </summary>
    ''' <param name="scriptFile">.vb脚本源代码文件路径</param>
    Public Function ParseScript(scriptFile As String, Optional verbose As Boolean = False) As ScriptParseResult
        If Not File.Exists(scriptFile) Then
            Throw New FileNotFoundException("脚本源文件不存在: " & scriptFile, scriptFile)
        End If

        Dim source As String = scriptFile.ReadAllText
        Dim baseDir As String = Path.GetDirectoryName(Path.GetFullPath(scriptFile))

        ' ---- Step1: 解析 #include 元数据, 得到引用的外部程序集文件路径 ----
        Dim [imports] As New List(Of String)

        For Each m As Match In Regex.Matches(source, "#include\s+""(?<dll>[^""]+)""", RegexOptions.IgnoreCase)
            Dim dll As String = m.Groups("dll").Value

            ' 相对路径统一解析为相对于脚本文件所在文件夹的绝对路径
            If Path.IsPathRooted(dll) Then
                Call [imports].Add(dll)
            Else
                Call [imports].Add(Path.GetFullPath(Path.Combine(baseDir, dll)))
            End If
        Next

        ' ---- Step3: 代码结构重构 ----
        Dim code As String = RefactorScript(source)

        If verbose Then
            Call Console.WriteLine("----- generated code -----")
            Call Console.WriteLine(code)
        End If

        Return New ScriptParseResult With {
            .ScriptFile = scriptFile,
            .CommandLine = Nothing,
            .Imports = [imports],
            .GeneratedCode = code
        }
    End Function

    ''' <summary>
    ''' 对脚本源代码进行重构处理, 生成最终的完整可编译代码:
    '''   1. 移除#include元数据行
    '''   2. 将 ?"--a" 替换为 args("--a") 字典访问
    '''   3. 提取类型定义块 / 顶层函数 / 顶层控制流块 / 顶层语句
    '''   4. 顶层函数重构为匿名函数
    '''   5. 组装为 固定Namespace + Module + Main 的容器结构
    ''' </summary>
    Private Function RefactorScript(source As String) As String
        ' ---- 文本级预处理 ----
        Dim code As String = Regex.Replace(
            source, "^\s*#include\s+""[^""]*""\s*$", "",
            RegexOptions.IgnoreCase Or RegexOptions.Multiline)

        ' ?"--a" => args("--a")
        code = Regex.Replace(code, "\?""(?<name>[^""]+)""", "args(""${name}"")")

        ' ---- 按行扫描分离代码块 ----
        Dim mainBody As New List(Of String)     ' 顶层语句 => Main方法体
        Dim typeBlocks As New List(Of String)   ' 类型定义块
        Dim funcBlocks As New List(Of String)   ' 顶层函数定义块
        Dim headerLines As New List(Of String)  ' 顶层Imports/Option语句

        Dim stack As New Stack(Of String)
        Dim buffer As New List(Of String)
        Dim bufferKind As String = Nothing      ' "type" / "func" / "stmt"

        For Each raw As String In code.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(vbLf)
            Dim line As String = raw.TrimEnd()
            Dim t As String = StripComment(line).Trim()

            If stack.Count = 0 Then
                ' ---- 当前处于脚本文件顶层 ----
                If String.IsNullOrEmpty(t) OrElse t.StartsWith("#") Then
                    Continue For
                End If

                If t.StartsWith("imports ", StringComparison.OrdinalIgnoreCase) OrElse
                    t.StartsWith("option ", StringComparison.OrdinalIgnoreCase) Then

                    Call headerLines.Add(t)
                    Continue For
                End If

                Dim bt As String = Nothing

                If IsTypeBlockStart(t, bt) Then
                    Call stack.Push(bt.ToLower)
                    bufferKind = "type"
                    Call buffer.Add(line)
                ElseIf IsFunctionBlockStart(t, bt) Then
                    Call stack.Push(bt.ToLower)
                    bufferKind = "func"
                    ' 顶层函数签名重构为匿名函数签名
                    Call buffer.Add(ToLambdaSignature(t))
                ElseIf IsLambdaBlockStart(t, bt) Then
                    ' 顶层多行lambda赋值语句, 保留结构
                    Call stack.Push(bt.ToLower)
                    bufferKind = "stmt"
                    Call buffer.Add(line)
                ElseIf IsControlBlockStart(t, bt) Then
                    ' 顶层控制流块(If/For/Try等), 保留结构进入Main
                    Call stack.Push(bt.ToLower)
                    bufferKind = "stmt"
                    Call buffer.Add(line)
                Else
                    ' 普通顶层可执行语句
                    Call mainBody.Add(t)
                End If
            Else
                ' ---- 当前处于某个代码块内部 ----
                Call buffer.Add(line)

                Dim bt As String = Nothing

                If IsBlockEnd(t, stack.Peek()) Then
                    Call stack.Pop()

                    If stack.Count = 0 Then
                        Dim blockCode As String = String.Join(vbLf, buffer)
                        Call buffer.Clear()

                        Select Case bufferKind
                            Case "type" : Call typeBlocks.Add(blockCode)
                            Case "func" : Call funcBlocks.Add(blockCode)
                            Case Else : Call mainBody.AddRange(blockCode.Split(vbLf))
                        End Select

                        bufferKind = Nothing
                    End If
                ElseIf IsNestedBlockStart(t, stack.Peek(), bt) Then
                    Call stack.Push(bt.ToLower)
                End If
            End If
        Next

        ' ---- 组装最终的完整代码 ----
        Dim sb As New StringBuilder()

        Call sb.AppendLine("Option Strict Off")
        Call sb.AppendLine("Option Explicit On")
        Call sb.AppendLine("Option Infer On")
        Call sb.AppendLine()

        For Each header As String In headerLines
            Call sb.AppendLine(header)
        Next

        If headerLines.Count > 0 Then
            Call sb.AppendLine()
        End If

        Call sb.AppendLine($"Imports {GetType(CommandLine).Namespace}")
        Call sb.AppendLine($"Imports System.Linq")
        Call sb.AppendLine($"Imports System")
        Call sb.AppendLine($"Imports System.Collections")
        Call sb.AppendLine($"Imports System.Collections.Generic")
        Call sb.AppendLine($"Imports System.Data")
        Call sb.AppendLine($"Imports System.Diagnostics")
        Call sb.AppendLine($"Imports System.Threading.Tasks")
        Call sb.AppendLine($"Imports System.Xml.Linq")

        Call sb.AppendLine($"Namespace {NamespaceName}")
        Call sb.AppendLine($"    Module {ModuleName}")
        Call sb.AppendLine()
        Call sb.AppendLine($"        Public Function {MainName}(args As CommandLine) As Integer")

        ' 顶层函数(匿名函数形式)必须先于顶层语句声明
        For Each func As String In funcBlocks
            For Each line As String In func.Split(vbLf)
                Call sb.AppendLine("            " & line)
            Next
            Call sb.AppendLine()
        Next

        For Each stmt As String In mainBody
            Call sb.AppendLine("            " & stmt)
        Next

        Call sb.AppendLine()
        Call sb.AppendLine("            Return 0")
        Call sb.AppendLine("        End Function")

        ' 类型定义块作为Module的嵌套类型
        For Each type As String In typeBlocks
            Call sb.AppendLine()

            For Each line As String In type.Split(vbLf)
                Call sb.AppendLine("        " & line)
            Next
        Next

        Call sb.AppendLine("    End Module")
        Call sb.AppendLine("End Namespace")

        Return sb.ToString()
    End Function

    ''' <summary>剥离行尾注释(用于块结构检测)</summary>
    Private Function StripComment(line As String) As String
        Dim idx As Integer = line.IndexOf("'"c)
        If idx >= 0 Then
            Return line.Substring(0, idx)
        Else
            Return line
        End If
    End Function

    ''' <summary>判断顶层代码行是否为类型定义块开始</summary>
    Private Function IsTypeBlockStart(line As String, ByRef blockType As String) As Boolean
        Dim m As Match = Regex.Match(line,
            "^(?:(public|private|friend|protected|partial|shared|mustinherit|notinheritable)\s+)*(?<kind>class|structure|interface|enum)\s+",
            RegexOptions.IgnoreCase)

        If m.Success Then
            blockType = m.Groups("kind").Value.ToLower
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>判断顶层代码行是否为函数定义块开始</summary>
    Private Function IsFunctionBlockStart(line As String, ByRef blockType As String) As Boolean
        Dim m As Match = Regex.Match(line,
            "^(?:(public|private|friend|protected|shared|static)\s+)*(?<kind>function|sub)\s+",
            RegexOptions.IgnoreCase)

        If m.Success Then
            blockType = m.Groups("kind").Value.ToLower
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>检测多行lambda的开始(行尾形式): xxx = Function(...) As Type / Sub(...)</summary>
    Private Function IsLambdaBlockStart(line As String, ByRef blockType As String) As Boolean
        Dim m As Match = Regex.Match(line,
            "(?<kind>function|sub)\s*\([^)]*\)\s*(as\s+.+)?\s*$",
            RegexOptions.IgnoreCase)

        If m.Success Then
            blockType = m.Groups("kind").Value.ToLower
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>判断代码行是否为控制流块开始</summary>
    Private Function IsControlBlockStart(line As String, ByRef blockType As String) As Boolean
        If Regex.IsMatch(line, "^if\s+.*\sthen\s*$", RegexOptions.IgnoreCase) Then
            blockType = "If"
        ElseIf Regex.IsMatch(line, "^select\s+case\s", RegexOptions.IgnoreCase) Then
            blockType = "Select"
        ElseIf Regex.IsMatch(line, "^try\s*$", RegexOptions.IgnoreCase) Then
            blockType = "Try"
        ElseIf Regex.IsMatch(line, "^using\s", RegexOptions.IgnoreCase) Then
            blockType = "Using"
        ElseIf Regex.IsMatch(line, "^synclock\s", RegexOptions.IgnoreCase) Then
            blockType = "SyncLock"
        ElseIf Regex.IsMatch(line, "^with\s", RegexOptions.IgnoreCase) Then
            blockType = "With"
        ElseIf Regex.IsMatch(line, "^do(\s|$)", RegexOptions.IgnoreCase) Then
            blockType = "Do"
        ElseIf Regex.IsMatch(line, "^while\s", RegexOptions.IgnoreCase) Then
            blockType = "While"
        ElseIf Regex.IsMatch(line, "^for\s", RegexOptions.IgnoreCase) Then
            blockType = "For"
        Else
            Return False
        End If

        Return True
    End Function

    ''' <summary>在代码块内部检测是否进入了嵌套代码块</summary>
    Private Function IsNestedBlockStart(line As String,
                                        currentBlock As String,
                                        ByRef blockType As String) As Boolean

        Dim bt As String = Nothing

        ' 嵌套类型定义
        If IsTypeBlockStart(line, bt) Then
            blockType = bt
            Return True
        End If

        ' Interface内部的成员均为单行签名声明, 不存在方法体
        If Not String.Equals(currentBlock, "interface", StringComparison.OrdinalIgnoreCase) Then

            Dim m As Match = Regex.Match(line,
                "^(?:(public|private|protected|friend|shared|static|const|partial|overrides|overloads|mustoverride|notoverridable|readonly|writeonly|default|shadows|withevents)\s+)*(?<kind>function|sub|property|get|set|operator)\b",
                RegexOptions.IgnoreCase)

            If m.Success Then
                ' 排除自动属性单行声明: Public Property X As Integer
                If m.Groups("kind").Value.ToLower = "property" Then
                    If Regex.IsMatch(line, "as\s+[^()]+\s*(=\s*.+)?\s*$", RegexOptions.IgnoreCase) AndAlso
                        Not Regex.IsMatch(line, "\b(get|set)\b", RegexOptions.IgnoreCase) Then
                        Return False
                    End If
                End If

                blockType = m.Groups("kind").Value.ToLower
                Return True
            End If

            ' 多行lambda
            If IsLambdaBlockStart(line, bt) Then
                blockType = bt
                Return True
            End If
        End If

        Return IsControlBlockStart(line, blockType)
    End Function

    ''' <summary>判断代码行是否为指定类型块的结束标记</summary>
    Private Function IsBlockEnd(line As String, blockType As String) As Boolean
        Select Case blockType
            Case "For" : Return Regex.IsMatch(line, "^next\b", RegexOptions.IgnoreCase)
            Case "Do" : Return Regex.IsMatch(line, "^loop\b", RegexOptions.IgnoreCase)
            Case Else : Return Regex.IsMatch(line, "^end\s+" & blockType & "\b", RegexOptions.IgnoreCase)
        End Select
    End Function

    ''' <summary>
    ''' 将顶层函数定义签名重构为匿名函数定义签名:
    '''   Public Function HelloWorld As String => Dim HelloWorld = Function() As String
    '''   Public Sub Foo(a As Integer)         => Dim Foo = Sub(a As Integer)
    ''' </summary>
    Private Function ToLambdaSignature(line As String) As String
        Dim m As Match = Regex.Match(line,
            "^(?:(public|private|friend|protected|shared|static)\s+)*(?<kind>function|sub)\s+(?<name>[a-z_]\w*)\s*(?<params>\([^)]*\))?\s*(?<ret>as\s+.+?)?\s*$",
            RegexOptions.IgnoreCase)

        If Not m.Success Then
            Return line
        End If

        Dim name As String = m.Groups("name").Value
        Dim params As String = m.Groups("params").Value
        Dim ret As String = m.Groups("ret").Value

        If String.IsNullOrEmpty(params) Then
            params = "()"
        End If

        If m.Groups("kind").Value.ToLower = "sub" Then
            Return $"Dim {name} = Sub{params}"
        Else
            Return $"Dim {name} = Function{params} {ret}".Trim()
        End If
    End Function
End Module
