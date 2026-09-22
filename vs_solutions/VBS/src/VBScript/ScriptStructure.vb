#Region "Microsoft.VisualBasic::fe353473a85df4a3e4ba3eefdae88846, vs_solutions\VBS\src\VBScript\ScriptStructure.vb"

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

    '   Total Lines: 560
    '    Code Lines: 352 (62.86%)
    ' Comment Lines: 106 (18.93%)
    '    - Xml Docs: 83.02%
    ' 
    '   Blank Lines: 102 (18.21%)
    '     File Size: 24.23 KB


    '     Class ScriptStatementSlot
    ' 
    ' 
    ' 
    '     Class ScriptFunctionBlock
    ' 
    '         Properties: Name, Signature, Slot, Text
    ' 
    '     Class ScriptStructure
    ' 
    '         Function: ResolveSlot, Scan
    ' 
    '         Sub: AddFunction, AddStatement, AddStatementBlock, FlushBuffer, HandleInsideBlock
    '              HandleTopLevel, ResolveFunctionSlots, ScanLines
    '         Class Scanner
    ' 
    ' 
    '             Enum Kind
    ' 
    '                 [Function], None, Statement, Type
    ' 
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: DeclaredNamesOf, FunctionNameOf, IsBlockEnd, IsControlBlockStart, IsFunctionBlockStart
    '               IsLambdaBlockStart, IsNestedBlockStart, IsTypeBlockStart, SplitTopLevel, StripComment
    '               ToLambdaSignature
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions

Namespace Script

    ''' <summary>
    ''' 一个顶层语句槽位: 若干条连续的顶层语句, 以及这些语句所声明的变量名。
    ''' 槽位按源码顺序编号, 顶层函数块会被挂到某个槽位上, 从而可以插入到语句之间。
    ''' </summary>
    Public Class ScriptStatementSlot

        ''' <summary>本槽位内的顶层语句(按源码顺序)</summary>
        Public ReadOnly Statements As New List(Of String)

        ''' <summary>本槽位内所声明的顶层变量名</summary>
        Public ReadOnly Declared As New List(Of String)
    End Class

    ''' <summary>
    ''' 一个顶层函数块: <see cref="Lines"/> 的首行为重写后的匿名函数签名,
    ''' <see cref="Signature"/> 保留源码之中的原始签名(供工程代码发射器生成真实函数)。
    ''' </summary>
    Public Class ScriptFunctionBlock

        ''' <summary>函数名</summary>
        Public Property Name As String

        ''' <summary>源码之中的原始签名行(例如 <c>Public Function HelloWorld As String</c>)</summary>
        Public Property Signature As String

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

    ''' <summary>
    ''' 脚本源代码的静态结构: 头部 Imports/Option、类型定义块、顶层函数块与顶层语句槽位。
    ''' </summary>
    ''' <remarks>
    ''' 本类型是"扫描"阶段的唯一产物, 由运行期代码发射器与工程代码发射器共用:
    ''' <list type="bullet">
    ''' <item><see cref="ScriptRefactor"/> 把函数块重构为匿名函数并求解落位;</item>
    ''' <item><see cref="IncludeResolver"/> 借助扫描结果校验被 <c>#include</c> 引入的脚本是否合法;</item>
    ''' <item>工程代码发射器把函数块还原为模块内的真实 <c>Private Function/Sub</c>。</item>
    ''' </list>
    ''' </remarks>
    Public Class ScriptStructure

        ''' <summary>脚本顶层的 Imports / Option 语句(按源码顺序)</summary>
        Public ReadOnly Headers As New List(Of String)

        ''' <summary>类型定义块(Class / Structure / Interface / Enum)的原文</summary>
        Public ReadOnly TypeBlocks As New List(Of String)

        ''' <summary>顶层函数块(含顶层 Sub)</summary>
        Public ReadOnly Functions As New List(Of ScriptFunctionBlock)

        ''' <summary>顶层语句槽位</summary>
        Public ReadOnly Slots As New List(Of ScriptStatementSlot)

        ''' <summary>
        ''' 对脚本代码做逐行块扫描, 得到静态结构。
        ''' </summary>
        ''' <param name="code">已经过文本预处理的脚本代码</param>
        Public Shared Function Scan(code As String) As ScriptStructure
            Dim scanner As New Scanner()

            Call scanner.ScanLines(code)

            Return scanner.Result
        End Function

        ''' <summary>
        ''' 求解每个顶层函数块在运行期 <c>Main</c> 之中的落位。
        ''' 顶层函数被重写为匿名函数之后就是 Main 之中的一个局部变量, VB 要求"先声明后使用",
        ''' 因此它既不能早于它所捕获的顶层变量, 也不能早于它所调用的其它顶层函数;
        ''' 不依赖任何顶层变量与函数的匿名函数仍然放在全部语句之前。
        ''' </summary>
        Public Sub ResolveFunctionSlots()
            Dim declarations As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

            For i As Integer = 0 To Slots.Count - 1
                For Each name As String In Slots(i).Declared
                    declarations(name) = i
                Next
            Next

            ' 函数之间可以互相调用, 迭代到收敛之后再固定次序
            For i As Integer = 0 To Functions.Count
                Dim changed As Boolean = False

                For Each func As ScriptFunctionBlock In Functions
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

            For Each func As ScriptFunctionBlock In Functions
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
        Private Function ResolveSlot(func As ScriptFunctionBlock, declarations As Dictionary(Of String, Integer)) As Integer
            Dim slot As Integer = -1
            Dim text As String = func.Text

            For Each name As String In declarations.Keys
                If Regex.IsMatch(text, "\b" & Regex.Escape(name) & "\b", RegexOptions.IgnoreCase) Then
                    slot = Math.Max(slot, declarations(name))
                End If
            Next

            For Each other As ScriptFunctionBlock In Functions
                If other Is func OrElse String.IsNullOrEmpty(other.Name) Then
                    Continue For
                End If

                If Regex.IsMatch(text, "\b" & Regex.Escape(other.Name) & "\b", RegexOptions.IgnoreCase) Then
                    slot = Math.Max(slot, other.Slot)
                End If
            Next

            Return slot
        End Function

        ' ==================================================================
        ' 逐行块扫描器
        ' ==================================================================

        Private Class Scanner

            Friend ReadOnly Result As New ScriptStructure()
            ReadOnly _stack As New Stack(Of String)
            ReadOnly _buffer As New List(Of String)
            Dim _bufferKind As Kind = Kind.None
            ''' <summary>当前函数块的原始签名行(顶层 Sub / Function)</summary>
            Dim _signature As String = Nothing

            Private Enum Kind
                None
                Type
                [Function]
                Statement
            End Enum

            ''' <summary>按行扫描代码, 利用块栈分离出类型定义块 / 顶层函数 / 语句块 / 顶层语句</summary>
            Friend Sub ScanLines(code As String)
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

                    Call Result.Headers.Add(t)
                    Return
                End If

                Dim bt As String = Nothing

                If IsTypeBlockStart(t, bt) Then
                    Call _stack.Push(bt.ToLower)
                    _bufferKind = Kind.Type
                    Call _buffer.Add(line)
                ElseIf IsFunctionBlockStart(t, bt) Then
                    Call _stack.Push(bt.ToLower)
                    _bufferKind = Kind.Function

                    ' 保留原始签名(工程代码发射器需要), 同时把签名重构为匿名函数签名
                    _signature = t
                    Call _buffer.Add(ToLambdaSignature(t))
                ElseIf IsLambdaBlockStart(t, bt) Then
                    ' 顶层多行lambda赋值语句, 保留结构
                    Call _stack.Push(bt.ToLower)
                    _bufferKind = Kind.Statement
                    Call _buffer.Add(line)
                ElseIf IsControlBlockStart(t, bt) Then
                    ' 顶层控制流块(If/For/Try等), 保留结构进入Main
                    Call _stack.Push(bt.ToLower)
                    _bufferKind = Kind.Statement
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
                    _bufferKind = Kind.None
                    Return
                End If

                Dim blockCode As String = String.Join(vbLf, _buffer)
                Dim blockKind As Kind = _bufferKind
                Dim signature As String = _signature

                Call _buffer.Clear()
                _bufferKind = Kind.None
                _signature = Nothing

                If blockKind = Kind.Function Then
                    Call AddFunction(blockCode, signature)
                    Return
                End If

                Select Case blockKind
                    Case Kind.Type : Call Result.TypeBlocks.Add(blockCode)
                    Case Else : Call AddStatementBlock(blockCode)
                End Select
            End Sub

            ''' <summary>追加一条顶层语句(独占一个新槽位)</summary>
            Private Sub AddStatement(line As String)
                Dim slot As New ScriptStatementSlot()

                Call slot.Statements.Add(line)
                Call slot.Declared.AddRange(DeclaredNamesOf(line))
                Call Result.Slots.Add(slot)
            End Sub

            ''' <summary>追加一个顶层语句块(For/Using/If等, 独占一个新槽位)</summary>
            Private Sub AddStatementBlock(blockCode As String)
                Dim slot As New ScriptStatementSlot()

                For Each line As String In blockCode.Split(vbLf)
                    Call slot.Statements.Add(line)
                    Call slot.Declared.AddRange(DeclaredNamesOf(line))
                Next

                Call Result.Slots.Add(slot)
            End Sub

            ''' <summary>追加一个顶层函数块(匿名函数形式, 同时保留原始签名)</summary>
            Private Sub AddFunction(blockCode As String, signature As String)
                Dim lines As String() = blockCode.Split(vbLf)
                Dim func As New ScriptFunctionBlock With {
                    .Name = FunctionNameOf(lines(0)),
                    .Signature = If(signature, lines(0))
                }

                For Each line As String In lines
                    Call func.Lines.Add(line)
                Next

                Call Result.Functions.Add(func)
            End Sub
        End Class

        ' ==================================================================
        ' 块结构判定(与语言结构相关的纯函数)
        ' ==================================================================

        ''' <summary>剥离行尾注释(用于块结构检测)</summary>
        Friend Shared Function StripComment(line As String) As String
            Dim idx As Integer = line.IndexOf("'"c)

            If idx >= 0 Then
                Return line.Substring(0, idx)
            Else
                Return line
            End If
        End Function

        ''' <summary>判断顶层代码行是否为类型定义块开始</summary>
        Friend Shared Function IsTypeBlockStart(line As String, ByRef blockType As String) As Boolean
            Dim m As Match = Regex.Match(line, "^(?:(public|private|friend|protected|partial|shared|mustinherit|notinheritable)\s+)*(?<kind>class|structure|interface|enum|module)\s+", RegexOptions.IgnoreCase)

            If m.Success Then
                blockType = m.Groups("kind").Value.ToLower
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>判断顶层代码行是否为函数定义块开始</summary>
        Friend Shared Function IsFunctionBlockStart(line As String, ByRef blockType As String) As Boolean
            Dim m As Match = Regex.Match(line, "^(?:(public|private|friend|protected|shared|static)\s+)*(?<kind>function|sub)\s+", RegexOptions.IgnoreCase)

            If m.Success Then
                blockType = m.Groups("kind").Value.ToLower
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>检测多行lambda的开始(行尾形式): xxx = Function(...) As Type / Sub(...)</summary>
        Friend Shared Function IsLambdaBlockStart(line As String, ByRef blockType As String) As Boolean
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
        Friend Shared Function IsControlBlockStart(line As String, ByRef blockType As String) As Boolean
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
        Friend Shared Function IsNestedBlockStart(line As String, currentBlock As String, ByRef blockType As String) As Boolean
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
        Friend Shared Function IsBlockEnd(line As String, blockType As String) As Boolean
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
        Friend Shared Function ToLambdaSignature(line As String) As String
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

        ''' <summary>
        ''' 从一条顶层语句之中提取它所声明的变量名。
        ''' 覆盖 Dim / Const, 包括逗号分隔的多个名字与数组声明;
        ''' 这里的策略是"宁可多识别"(至多让函数不提前, 等价于源码顺序),
        ''' 不可少识别(会让 BC32000 复现)。
        ''' </summary>
        Friend Shared Function DeclaredNamesOf(stmt As String) As String()
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

        ''' <summary>
        ''' 按顶层逗号切分: 不切圆括号/花括号内部的逗号, 也不切字符串字面量内部的逗号。
        ''' </summary>
        ''' <remarks>
        ''' 花括号用于数组字面量(例如 <c>Dim items As String() = {"a", "b"}</c>),
        ''' 字符串字面量之中的逗号同样不能作为声明项的分隔符;
        ''' VB 字符串之中使用两个连续的双引号表示一个双引号字符,
        ''' 因此简单地逐字符翻转"是否处于字符串内部"即可正确处理转义。
        ''' </remarks>
        Friend Shared Function SplitTopLevel(s As String) As String()
            Dim parts As New List(Of String)
            Dim depth As Integer = 0
            Dim start As Integer = 0
            Dim inString As Boolean = False

            For i As Integer = 0 To s.Length - 1
                Dim ch As Char = s(i)

                If ch = """"c Then
                    inString = Not inString
                    Continue For
                End If

                If inString Then
                    Continue For
                End If

                Select Case ch
                    Case "("c, "{"c
                        depth += 1
                    Case ")"c, "}"c
                        depth -= 1
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
        Friend Shared Function FunctionNameOf(signature As String) As String
            Dim m As Match = Regex.Match(signature, "^\s*Dim\s+(?<name>[a-z_]\w*)\s*=", RegexOptions.IgnoreCase)

            If m.Success Then
                Return m.Groups("name").Value
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace

