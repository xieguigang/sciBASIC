#Region "Microsoft.VisualBasic::aa3391f8d6a6c50c1799715871e74342, vs_solutions\VBS\src\VBScript\ScriptRefactor.vb"

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

    '   Total Lines: 621
    '    Code Lines: 389 (62.64%)
    ' Comment Lines: 120 (19.32%)
    '    - Xml Docs: 64.17%
    ' 
    '   Blank Lines: 112 (18.04%)
    '     File Size: 26.63 KB


    '     Class ScriptRefactor
    ' 
    '         Function: BuildCode, DeclaredNamesOf, FunctionNameOf, IsBlockEnd, IsControlBlockStart
    '                   IsFunctionBlockStart, IsLambdaBlockStart, IsNestedBlockStart, IsTypeBlockStart, PreprocessText
    '                   Refactor, ResolveSlot, SplitTopLevel, StripComment, ToLambdaSignature
    ' 
    '         Sub: AddFunction, AddStatement, AddStatementBlock, AppendFunctions, FlushBuffer
    '              HandleInsideBlock, HandleTopLevel, PlaceFunctions, ScanLines
    '         Class BodySlot
    ' 
    ' 
    ' 
    '         Class FuncBlock
    ' 
    '             Properties: Name, Slot, Text
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine

Namespace Script

    ''' <summary>
    ''' 将脚本源代码重构为可以直接编译的完整VB.NET源代码
    ''' </summary>
    ''' <remarks>
    ''' 一次重构过程对应一个对象实例: 扫描过程之中的中间状态(块栈, 缓冲)与
    ''' 扫描结果(语句槽位, 顶层函数块, 类型定义块, 头部Imports)全部作为对象字段,
    ''' 由 <see cref="Refactor"/> 依次编排 文本预处理 -> 逐行扫描 -> 函数落位 -> 组装 四个阶段。
    ''' </remarks>
    Public Class ScriptRefactor

        ' ==================================================================
        ' 私有小类型
        ' ==================================================================

        ''' <summary>
        ''' 一个顶层语句槽位: 若干条连续的顶层语句, 以及这些语句所声明的变量名。
        ''' 槽位按源码顺序编号, 顶层函数块会被挂到某个槽位上, 从而可以插入到语句之间。
        ''' </summary>
        Private Class BodySlot
            ''' <summary>本槽位内的顶层语句(按源码顺序)</summary>
            Public ReadOnly Statements As New List(Of String)
            ''' <summary>本槽位内所声明的顶层变量名</summary>
            Public ReadOnly Declared As New List(Of String)
        End Class

        ''' <summary>
        ''' 一个顶层函数块: 函数签名已被重写为匿名函数(Dim f = Function/Sub ...)。
        ''' </summary>
        Private Class FuncBlock
            ''' <summary>函数名</summary>
            Public Property Name As String
            ''' <summary>函数块的全部代码行(首行为匿名函数签名)</summary>
            Public ReadOnly Lines As New List(Of String)
            ''' <summary>落位槽位; -1 表示放在全部语句之前</summary>
            Public Property Slot As Integer = -1

            ''' <summary>函数块的完整文本(用于依赖扫描)</summary>
            Public ReadOnly Property Text As String
                Get
                    Return String.Join(vbLf, Lines)
                End Get
            End Property
        End Class

        ' ==================================================================
        ' 扫描状态
        ' ==================================================================

        ReadOnly _stack As New Stack(Of String)
        ReadOnly _buffer As New List(Of String)
        Dim _bufferKind As String = Nothing      ' "type" / "func" / "stmt"

        ' ==================================================================
        ' 扫描结果
        ' ==================================================================

        ReadOnly _slots As New List(Of BodySlot)
        ReadOnly _funcs As New List(Of FuncBlock)
        ReadOnly _typeBlocks As New List(Of String)
        ReadOnly _headerLines As New List(Of String)

        ' ==================================================================
        ' 阶段编排
        ' ==================================================================

        ''' <summary>
        ''' 对脚本源代码进行重构处理, 生成最终的完整可编译代码:
        '''   1. 移除#include元数据行
        '''   2. 将 ?"--a" 替换为 args("--a") 字典访问
        '''   3. 提取类型定义块 / 顶层函数 / 顶层控制流块 / 顶层语句
        '''   4. 顶层函数重构为匿名函数, 并按照依赖关系确定其在Main之中的落位
        '''   5. 组装为 固定Namespace + Module + Main 的容器结构
        ''' </summary>
        Public Function Refactor(source As String) As String
            Dim code As String = PreprocessText(source)

            Call ScanLines(code)
            Call PlaceFunctions()

            Return BuildCode()
        End Function

        ' ==================================================================
        ' 阶段1: 文本级预处理
        ' ==================================================================

        ''' <summary>移除#include元数据, 展开命令行参数语法与元组分解语法</summary>
        Private Function PreprocessText(source As String) As String
            Dim code As String = Regex.Replace(source, "^\s*#include\s+""[^""]*""\s*$", "", RegexOptions.IgnoreCase Or RegexOptions.Multiline)

            ' ?"--a" => args("--a")
            code = Regex.Replace(code, "\?""(?<name>[^""]+)""", "args(""${name}"")")
            code = TupleDestructuring.Expand(code)

            Return code
        End Function

        ' ==================================================================
        ' 阶段2: 逐行扫描
        ' ==================================================================

        ''' <summary>按行扫描代码, 利用块栈分离出类型定义块 / 顶层函数 / 语句块 / 顶层语句</summary>
        Private Sub ScanLines(code As String)
            For Each raw As String In code.LineTokens
                Dim line As String = raw.TrimEnd()
                Dim t As String = StripComment(line).Trim()

                If _stack.Count = 0 Then
                    Call HandleTopLevel(line, t)
                Else
                    Call HandleInsideBlock(line, t)
                End If
            Next

            ' ---- 兜底处理: 存在没有正常闭合的代码块时, 将残留的块内容一并导出,
            '      避免因为块不配对(例如缺少Next/End Function)而静默丢失脚本代码 ----
            Call FlushBuffer()
        End Sub

        ''' <summary>处理处于脚本文件顶层的代码行</summary>
        Private Sub HandleTopLevel(line As String, t As String)
            If String.IsNullOrEmpty(t) OrElse t.StartsWith("#") Then
                Return
            End If

            If t.StartsWith("imports ", StringComparison.OrdinalIgnoreCase) OrElse
               t.StartsWith("option ", StringComparison.OrdinalIgnoreCase) Then

                Call _headerLines.Add(t)
                Return
            End If

            Dim bt As String = Nothing

            If IsTypeBlockStart(t, bt) Then
                Call _stack.Push(bt.ToLower)
                _bufferKind = "type"
                Call _buffer.Add(line)
            ElseIf IsFunctionBlockStart(t, bt) Then
                Call _stack.Push(bt.ToLower)
                _bufferKind = "func"
                ' 顶层函数签名重构为匿名函数签名
                Call _buffer.Add(ToLambdaSignature(t))
            ElseIf IsLambdaBlockStart(t, bt) Then
                ' 顶层多行lambda赋值语句, 保留结构
                Call _stack.Push(bt.ToLower)
                _bufferKind = "stmt"
                Call _buffer.Add(line)
            ElseIf IsControlBlockStart(t, bt) Then
                ' 顶层控制流块(If/For/Try等), 保留结构进入Main
                Call _stack.Push(bt.ToLower)
                _bufferKind = "stmt"
                Call _buffer.Add(line)
            Else
                ' 普通顶层可执行语句: 独占一个槽位, 以便函数块可以插入到语句之间
                Call AddStatement(t)
            End If
        End Sub

        ''' <summary>处理处于某个代码块内部的代码行</summary>
        Private Sub HandleInsideBlock(line As String, t As String)
            Call _buffer.Add(line)

            Dim bt As String = Nothing

            If IsBlockEnd(t, _stack.Peek()) Then
                Call _stack.Pop()

                If _stack.Count = 0 Then
                    Call FlushBuffer()
                End If
            ElseIf IsNestedBlockStart(t, _stack.Peek(), bt) Then
                Call _stack.Push(bt.ToLower)
            End If
        End Sub

        ''' <summary>将缓冲收集到的一个完整代码块, 按照块类型派发到对应的结果集合之中</summary>
        Private Sub FlushBuffer()
            If _buffer.Count = 0 Then
                _bufferKind = Nothing
                Return
            End If

            Dim blockCode As String = String.Join(vbLf, _buffer)
            Dim kind As String = _bufferKind

            Call _buffer.Clear()
            _bufferKind = Nothing

            Select Case kind
                Case "type" : Call _typeBlocks.Add(blockCode)
                Case "func" : Call AddFunction(blockCode)
                Case Else : Call AddStatementBlock(blockCode)
            End Select
        End Sub

        ''' <summary>追加一条顶层语句(独占一个新槽位)</summary>
        Private Sub AddStatement(line As String)
            Dim slot As New BodySlot()

            Call slot.Statements.Add(line)
            Call slot.Declared.AddRange(DeclaredNamesOf(line))
            Call _slots.Add(slot)
        End Sub

        ''' <summary>追加一个顶层语句块(For/Using/If等, 独占一个新槽位)</summary>
        Private Sub AddStatementBlock(blockCode As String)
            Dim slot As New BodySlot()

            For Each line As String In blockCode.Split(vbLf)
                Call slot.Statements.Add(line)
                Call slot.Declared.AddRange(DeclaredNamesOf(line))
            Next

            Call _slots.Add(slot)
        End Sub

        ''' <summary>追加一个顶层函数块(匿名函数形式)</summary>
        Private Sub AddFunction(blockCode As String)
            Dim lines As String() = blockCode.Split(vbLf)
            Dim func As New FuncBlock With {.Name = FunctionNameOf(lines(0))}

            For Each line As String In lines
                Call func.Lines.Add(line)
            Next

            Call _funcs.Add(func)
        End Sub

        ' ==================================================================
        ' 阶段3: 顶层函数落位
        ' ==================================================================

        ''' <summary>
        ''' 求解每个顶层函数在Main之中的落位。
        ''' 顶层函数被重写为匿名函数之后就是Main之中的一个局部变量, VB要求"先声明后使用",
        ''' 因此它既不能早于它所捕获的顶层变量, 也不能早于它所调用的其它顶层函数;
        ''' 不依赖任何顶层变量与函数的匿名函数仍然放在全部语句之前。
        ''' </summary>
        Private Sub PlaceFunctions()
            Dim declarations As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

            For i As Integer = 0 To _slots.Count - 1
                For Each name As String In _slots(i).Declared
                    declarations(name) = i
                Next
            Next

            ' 函数之间可以互相调用, 迭代到收敛之后再固定次序
            For i As Integer = 0 To _funcs.Count
                Dim changed As Boolean = False

                For Each func As FuncBlock In _funcs
                    Dim slot As Integer = ResolveSlot(func, declarations)

                    If slot <> func.Slot Then
                        func.Slot = slot
                        changed = True
                    End If
                Next

                If Not changed Then
                    Exit For
                End If
            Next

            ' 保持函数之间的源码相对顺序
            Dim previous As Integer = -1

            For Each func As FuncBlock In _funcs
                If func.Slot < previous Then
                    func.Slot = previous
                End If

                previous = func.Slot
            Next
        End Sub

        ''' <summary>
        ''' 计算一个函数块最早可以落在哪个槽位:
        ''' 取"所捕获的顶层变量的声明槽位"与"所调用的其它顶层函数的槽位"的最大值。
        ''' </summary>
        Private Function ResolveSlot(func As FuncBlock, declarations As Dictionary(Of String, Integer)) As Integer
            Dim slot As Integer = -1
            Dim text As String = func.Text

            For Each name As String In declarations.Keys
                If Regex.IsMatch(text, "\b" & Regex.Escape(name) & "\b", RegexOptions.IgnoreCase) Then
                    slot = Math.Max(slot, declarations(name))
                End If
            Next

            For Each other As FuncBlock In _funcs
                If other Is func OrElse String.IsNullOrEmpty(other.Name) Then
                    Continue For
                End If
                If Regex.IsMatch(text, "\b" & Regex.Escape(other.Name) & "\b", RegexOptions.IgnoreCase) Then
                    slot = Math.Max(slot, other.Slot)
                End If
            Next

            Return slot
        End Function

        ''' <summary>
        ''' 从一条顶层语句之中提取它所声明的变量名。
        ''' 覆盖 Dim / Const, 包括逗号分隔的多个名字与数组声明;
        ''' 这里的策略是"宁可多识别"(至多让函数不提前, 等价于源码顺序),
        ''' 不可少识别(会让 BC32000 复现)。
        ''' </summary>
        Private Function DeclaredNamesOf(stmt As String) As String()
            Dim m As Match = Regex.Match(stmt, "^\s*(dim|const)\s+(?<decl>.+)$", RegexOptions.IgnoreCase)

            If Not m.Success Then
                Return New String() {}
            End If

            Dim names As New List(Of String)

            For Each part As String In SplitTopLevel(m.Groups("decl").Value)
                Dim nm As Match = Regex.Match(part.Trim(), "^(?<name>[a-z_]\w*)", RegexOptions.IgnoreCase)

                If nm.Success Then
                    Call names.Add(nm.Groups("name").Value)
                End If
            Next

            Return names.ToArray()
        End Function

        ''' <summary>按顶层逗号切分(不切括号内的逗号)</summary>
        Private Function SplitTopLevel(s As String) As String()
            Dim parts As New List(Of String)
            Dim depth As Integer = 0
            Dim start As Integer = 0

            For i As Integer = 0 To s.Length - 1
                Select Case s(i)
                    Case "("c : depth += 1
                    Case ")"c : depth -= 1
                    Case ","c
                        If depth = 0 Then
                            Call parts.Add(s.Substring(start, i - start))
                            start = i + 1
                        End If
                End Select
            Next

            Call parts.Add(s.Substring(start))

            Return parts.ToArray()
        End Function

        ''' <summary>从匿名函数签名(Dim f = Function/Sub ...)之中取出函数名</summary>
        Private Function FunctionNameOf(signature As String) As String
            Dim m As Match = Regex.Match(signature, "^\s*Dim\s+(?<name>[a-z_]\w*)\s*=", RegexOptions.IgnoreCase)

            If m.Success Then
                Return m.Groups("name").Value
            Else
                Return Nothing
            End If
        End Function

        ' ==================================================================
        ' 阶段4: 组装
        ' ==================================================================

        ''' <summary>组装为 固定Namespace + Module + Main 的完整可编译代码</summary>
        Private Function BuildCode() As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine("Option Strict Off")
            Call sb.AppendLine("Option Explicit On")
            Call sb.AppendLine("Option Infer On")
            Call sb.AppendLine()

            For Each header As String In _headerLines
                Call sb.AppendLine(header)
            Next

            If _headerLines.Count > 0 Then
                Call sb.AppendLine()
            End If

            Call sb.AppendLine($"Imports {GetType(CommandLine).Namespace}")
            Call sb.AppendLine($"Imports Microsoft.VisualBasic")
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

            ' 槽位 -1: 不依赖任何顶层变量与其它顶层函数的匿名函数, 放在最前面
            Call AppendFunctions(sb, -1)

            For i As Integer = 0 To _slots.Count - 1
                For Each stmt As String In _slots(i).Statements
                    Call sb.AppendLine("            " & stmt)
                Next

                ' 挂在语句之后的匿名函数: 它捕获的变量到这里已经声明完毕
                Call AppendFunctions(sb, i)
            Next

            Call sb.AppendLine()
            Call sb.AppendLine("            Return 0")
            Call sb.AppendLine("        End Function")

            ' 类型定义块作为Module的嵌套类型
            For Each typeBlock As String In _typeBlocks
                Call sb.AppendLine()

                For Each line As String In typeBlock.Split(vbLf)
                    Call sb.AppendLine("        " & line)
                Next
            Next

            Call sb.AppendLine("    End Module")
            Call sb.AppendLine("End Namespace")

            Return sb.ToString()
        End Function

        ''' <summary>把落在指定槽位上的全部顶层函数块输出到Main之中</summary>
        Private Sub AppendFunctions(sb As StringBuilder, slot As Integer)
            For Each func As FuncBlock In _funcs
                If func.Slot <> slot Then
                    Continue For
                End If

                For Each line As String In func.Lines
                    Call sb.AppendLine("            " & line)
                Next

                Call sb.AppendLine()
            Next
        End Sub

        ' ==================================================================
        ' 块结构判定(与语言结构相关的纯函数)
        ' ==================================================================

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
            Dim m As Match = Regex.Match(line, "^(?:(public|private|friend|protected|partial|shared|mustinherit|notinheritable)\s+)*(?<kind>class|structure|interface|enum)\s+", RegexOptions.IgnoreCase)

            If m.Success Then
                blockType = m.Groups("kind").Value.ToLower
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>判断顶层代码行是否为函数定义块开始</summary>
        Private Function IsFunctionBlockStart(line As String, ByRef blockType As String) As Boolean
            Dim m As Match = Regex.Match(line, "^(?:(public|private|friend|protected|shared|static)\s+)*(?<kind>function|sub)\s+", RegexOptions.IgnoreCase)

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
        Private Function IsNestedBlockStart(line As String, currentBlock As String, ByRef blockType As String) As Boolean
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

        ''' <summary>
        ''' 判断代码行是否为指定类型块的结束标记
        ''' </summary>
        ''' <remarks>
        ''' 栈中所保存的块类型名称统一为小写形式(全部经由``bt.ToLower``入栈),
        ''' 而VB的Select Case在默认的``Option Compare Binary``之下是区分大小写的,
        ''' 所以这里必须先统一大小写之后再进行块类型判定, 否则``For``/``Do``块
        ''' 将永远无法被``Next``/``Loop``所闭合。
        ''' </remarks>
        Private Function IsBlockEnd(line As String, blockType As String) As Boolean
            Select Case blockType.ToLower()
                Case "for" : Return Regex.IsMatch(line, "^next\b", RegexOptions.IgnoreCase)
                Case "do" : Return Regex.IsMatch(line, "^loop\b", RegexOptions.IgnoreCase)
                Case Else
                    Return Regex.IsMatch(line, "^end\s+" & Regex.Escape(blockType) & "\b", RegexOptions.IgnoreCase)
            End Select
        End Function

        ''' <summary>
        ''' 将顶层函数定义签名重构为匿名函数定义签名:
        '''   Public Function HelloWorld As String => Dim HelloWorld = Function() As String
        '''   Public Sub Foo(a As Integer)         => Dim Foo = Sub(a As Integer)
        ''' </summary>
        Private Function ToLambdaSignature(line As String) As String
            Dim m As Match = Regex.Match(line, "^(?:(public|private|friend|protected|shared|static)\s+)*(?<kind>function|sub)\s+(?<name>[a-z_]\w*)\s*(?<params>\([^)]*\))?\s*(?<ret>as\s+.+?)?\s*$", RegexOptions.IgnoreCase)

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
    End Class
End Namespace
