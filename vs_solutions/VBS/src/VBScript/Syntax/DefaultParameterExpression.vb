#Region "Microsoft.VisualBasic::ead537a4a51ae356c7f4b1edb4a52e00, vs_solutions\VBS\src\VBScript\Syntax\DefaultParameterExpression.vb"

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

    '   Total Lines: 923
    '    Code Lines: 590 (63.92%)
    ' Comment Lines: 140 (15.17%)
    '    - Xml Docs: 62.14%
    ' 
    '   Blank Lines: 193 (20.91%)
    '     File Size: 39.63 KB


    '     Class DefaultParameterReport
    ' 
    '         Properties: Bridges, InlineSubs, Required, Rewritten, Skipped
    ' 
    '     Module DefaultParameterExpression
    ' 
    ' 
    '         Class Rename
    ' 
    '             Properties: Length, Start, Text
    ' 
    '         Class LineEdit
    ' 
    '             Properties: Expansion, Renames
    ' 
    '         Class ArgBinding
    ' 
    '             Properties: Expression, IsNamed, NameLength, NameStart, ParamIndex
    ' 
    '         Enum Container
    ' 
    '             None, SingleLineIf, SingleLineLambda, Statement
    ' 
    ' 
    ' 
    '         Class Context
    ' 
    '             Properties: TempSeq
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '  
    ' 
    '     Function: ApplyRenames, CollectTargets, EnsureMangled, Expand, GetBridge
    '               IndexOfWord, IsCallee, NextBridgeName, NextNonSpace, NextTempName
    '               RewriteDeclaration, Spaces, SubsetKey, TryBindArguments, TryBuildInline
    ' 
    '     Sub: MarkAmbiguous, ScanCallSites
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions

Namespace Script

    ''' <summary>
    ''' 默认参数表达式改写的统计与诊断信息(仅供 <c>--verbose</c> 使用, 不进入编译产物)。
    ''' </summary>
    Public Class DefaultParameterReport

        ''' <summary>被改写的调用点数量</summary>
        Public Property Rewritten As Integer

        ''' <summary>以「就地展开」方式改写的 <c>Sub</c> 名</summary>
        Public ReadOnly Property InlineSubs As New List(Of String)

        ''' <summary>生成的桥接函数名</summary>
        Public ReadOnly Property Bridges As New List(Of String)

        ''' <summary>因存在无法覆盖的调用点而被退化为必填的参数(形如 <c>test.c</c>)</summary>
        Public ReadOnly Property Required As New List(Of String)

        ''' <summary>因保守条件而放弃改写的函数名</summary>
        Public ReadOnly Property Skipped As New List(Of String)
    End Class

    ''' <summary>
    ''' 默认参数表达式语法的文本预处理阶段。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' VB.NET 只允许把**常数或常数表达式**作为可选参数的默认值; 本阶段让脚本作者可以在默认值位置
    ''' 书写任意可以产生值的表达式(例如 <c>Optional c As data = If(b, New testdata(a), New testdata("default"))</c>),
    ''' 由引擎把它改写成等价且可以被 Roslyn 编译的代码。
    ''' </para>
    ''' <para>
    ''' <b>改写分两条路径</b>:
    ''' <list type="bullet">
    ''' <item><c>Function</c> 生成**桥接函数**: 调用点只把被调用名替换为桥接函数名、实参表原样保留,
    ''' 因此 <c>While test(a,b)</c>、<c>If test(...) Then</c>、<c>print(test(...))</c> 等任意表达式位置都成立,
    ''' 且不改变任何求值顺序;</item>
    ''' <item><c>Sub</c> 在调用点**就地展开**为多行临时变量: VB 之中 Sub 调用只能作为语句出现,
    ''' 因此「在该语句之前插入多行」永远安全, 也就不会给 Sub 的热点调用引入额外的函数调用;
    ''' 当语句容器容不下多行语句时(单行 Sub lambda、单行 <c>If ... Then</c>)先把该容器展开为块。</item>
    ''' </list>
    ''' </para>
    ''' <para>
    ''' <b>保守策略</b>: 任何一处不确定都放弃改写(于是保持原有行为 —— 而这类写法在本功能引入之前
    ''' 本来就无法编译, 因此不存在行为回归)。
    ''' </para>
    ''' </remarks>
    Public Module DefaultParameterExpression

        ''' <summary>把被调用名的一段字符替换为桥接函数名</summary>
        Private Class Rename
            Public Property Start As Integer
            Public Property Length As Integer
            Public Property Text As String
        End Class

        ''' <summary>一行源码上的改写指令: 改名 与 整行展开 二选一</summary>
        Private Class LineEdit
            Public ReadOnly Property Renames As New List(Of Rename)
            Public Property Expansion As List(Of String)
        End Class

        ''' <summary>一个实参与形参的绑定关系</summary>
        Private Class ArgBinding
            Public Property ParamIndex As Integer
            Public Property IsNamed As Boolean
            Public Property Expression As String
            ''' <summary>具名实参的名字在整行之中的起始下标(<see cref="IsNamed"/> 为 False 时为 -1)</summary>
            Public Property NameStart As Integer = -1
            ''' <summary>具名实参的名字长度</summary>
            Public Property NameLength As Integer
        End Class

        ''' <summary>Sub 调用点所在的语句容器形态</summary>
        Private Enum Container
            None
            Statement
            SingleLineIf
            SingleLineLambda
        End Enum

        ''' <summary>改写过程之中的共享状态</summary>
        Private Class Context
            ''' <summary>脚本之中出现过的全部标识符(用于生成的符号名查重)</summary>
            Public ReadOnly Identifiers As HashSet(Of String)
            ''' <summary>临时变量的全局序号(保证同一作用域之内的临时变量互不重名)</summary>
            Public Property TempSeq As Integer

            Public Sub New(source As String)
                Identifiers = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

                For Each m As Match In Regex.Matches(source, "[A-Za-z_]\w*")
                    Call Identifiers.Add(m.Value)
                Next
            End Sub
        End Class

        ''' <summary>具名实参: <c>name := expr</c></summary>
        Private ReadOnly NamedArgPattern As New Regex(
            "^(?<name>[A-Za-z_]\w*)\s*:=\s*(?<expr>.+)$", RegexOptions.IgnoreCase)

        ''' <summary>函数/子程序成员名(用于检测类型块内部的同名成员遮蔽)</summary>
        Private ReadOnly MethodNamePattern As New Regex(
            "^(?:(?:public|private|friend|protected|shared|static|overloads|overrides|mustoverride|notoverridable|partial)\s+)*(?:function|sub)\s+(?<name>[A-Za-z_]\w*)",
            RegexOptions.IgnoreCase)

        ' ==================================================================
        ' 阶段入口
        ' ==================================================================

        ''' <summary>
        ''' 把脚本之中的「非常数默认参数表达式」改写为可以被 Roslyn 编译的等价代码。
        ''' </summary>
        ''' <param name="source">已经过 <c>#include</c> 剔除、<c>?参数</c>/<c>let</c>/元组分解展开的脚本代码</param>
        ''' <param name="report">可选的改写报告</param>
        Public Function Expand(source As String,
                               Optional report As DefaultParameterReport = Nothing) As String

            If String.IsNullOrEmpty(source) Then
                Return source
            End If

            Dim lines As String() = source.LineTokens

            If lines.Length = 0 Then
                Return source
            End If

            ' ---- 1. 收集顶层声明 ----
            Dim targets As List(Of DefaultParameterFunction) = CollectTargets(lines, report)

            If targets.Count = 0 Then
                Return source
            End If

            ' ---- 2. 扫描并改写调用点 ----
            Dim ctx As New Context(source)
            Dim edits As LineEdit() = New LineEdit(lines.Length - 1) {}

            Call ScanCallSites(lines, targets, ctx, edits, report)

            ' ---- 3. 发射: 声明行改写 + 调用点改写 ----
            Dim declOf As New Dictionary(Of Integer, DefaultParameterFunction)()

            For Each fn As DefaultParameterFunction In targets
                declOf(fn.LineIndex) = fn
            Next

            ' 桥接函数必须紧跟在原函数块之后插入: 既有的 ResolveFunctionSlots 会「保持函数之间的源码相对顺序」,
            ' 若把桥接函数追加到脚本文末, 它会被排在它之前的函数的最小槽位拖到调用点之后而报 BC32000。
            Dim bridgeAt As New Dictionary(Of Integer, List(Of String()))()

            For Each fn As DefaultParameterFunction In targets
                If fn.BridgeBlocks.Count = 0 Then
                    Continue For
                End If

                Dim at As Integer = If(fn.EndLineIndex >= 0, fn.EndLineIndex, lines.Length - 1)
                Dim blocks As List(Of String()) = Nothing

                If Not bridgeAt.TryGetValue(at, blocks) Then
                    blocks = New List(Of String())()
                    bridgeAt(at) = blocks
                End If

                Call blocks.AddRange(fn.BridgeBlocks)
            Next

            Dim output As New List(Of String)

            For i As Integer = 0 To lines.Length - 1
                If declOf.ContainsKey(i) Then
                    Call output.Add(RewriteDeclaration(lines(i), declOf(i), report))
                Else
                    Dim edit As LineEdit = edits(i)

                    If edit Is Nothing Then
                        Call output.Add(lines(i))
                    ElseIf edit.Expansion IsNot Nothing Then
                        Call output.AddRange(edit.Expansion)
                    Else
                        Call output.Add(ApplyRenames(lines(i), edit.Renames))
                    End If
                End If

                If bridgeAt.ContainsKey(i) Then
                    For Each block As String() In bridgeAt(i)
                        Call output.Add("")
                        Call output.AddRange(block)
                    Next
                End If
            Next

            Return String.Join(vbCrLf, output)
        End Function

        ' ==================================================================
        ' 第1趟: 收集顶层声明
        ' ==================================================================

        ''' <summary>
        ''' 扫描全部代码行, 收集**顶层**的 <c>Function</c>/<c>Sub</c> 声明(带非常数默认值的那些)。
        ''' </summary>
        Private Function CollectTargets(lines As String(),
                                        report As DefaultParameterReport) As List(Of DefaultParameterFunction)

            Dim found As New List(Of DefaultParameterFunction)
            Dim nested As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim stack As New Stack(Of String)
            ''' <summary>当前正在扫描的函数块(用于记录函数块的结束行)</summary>
            Dim pending As DefaultParameterFunction = Nothing

            For i As Integer = 0 To lines.Length - 1
                Dim t As String = ScriptStructure.StripComment(lines(i)).Trim()
                Dim blockType As String = Nothing

                If stack.Count = 0 Then
                    If ScriptStructure.IsTypeBlockStart(t, blockType) Then
                        Call stack.Push(blockType.ToLower())
                    ElseIf ScriptStructure.IsFunctionBlockStart(t, blockType) Then
                        Call stack.Push(blockType.ToLower())

                        Dim fn As DefaultParameterFunction = Nothing
                        Dim bailed As Boolean = False

                        If DefaultParameterSignature.TryParseDeclaration(lines(i), i, fn, bailed) Then
                            If fn.Dynamic.Count > 0 Then
                                pending = fn
                                Call found.Add(fn)
                            End If
                        ElseIf bailed AndAlso report IsNot Nothing Then
                            Call report.Skipped.Add(MethodNamePattern.Match(t).Groups("name").Value)
                        End If
                    ElseIf ScriptStructure.IsLambdaBlockStart(t, blockType) Then
                        Call stack.Push(blockType.ToLower())
                    ElseIf ScriptStructure.IsControlBlockStart(t, blockType) Then
                        Call stack.Push(blockType.ToLower())
                    End If
                Else
                    ' 块内部的同名 Function/Sub 成员会遮蔽顶层函数
                    Dim nm As Match = MethodNamePattern.Match(t)

                    If nm.Success Then
                        Call nested.Add(nm.Groups("name").Value)
                    End If

                    If ScriptStructure.IsBlockEnd(t, stack.Peek()) Then
                        Call stack.Pop()

                        If stack.Count = 0 AndAlso pending IsNot Nothing Then
                            pending.EndLineIndex = i
                            pending = Nothing
                        End If
                    ElseIf ScriptStructure.IsNestedBlockStart(t, stack.Peek(), blockType) Then
                        Call stack.Push(blockType.ToLower())
                    End If
                End If
            Next

            ' 同名顶层函数(重载)与类型块内部的同名成员都会造成名字遮蔽 => 一律不改写
            Dim counts As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

            For Each fn As DefaultParameterFunction In found
                Dim n As Integer = 0

                Call counts.TryGetValue(fn.Name, n)
                counts(fn.Name) = n + 1
            Next

            Dim targets As New List(Of DefaultParameterFunction)

            For Each fn As DefaultParameterFunction In found
                If counts(fn.Name) > 1 OrElse nested.Contains(fn.Name) Then
                    If report IsNot Nothing Then
                        Call report.Skipped.Add(fn.Name)
                    End If

                    Continue For
                End If

                ' Function 走桥接; Sub 默认就地展开, 但出现 ByRef 或默认值里含 lambda 时退回桥接
                fn.UseBridge = (Not fn.IsSub) OrElse
                               fn.Parameters.Any(Function(p) p.IsByRef) OrElse
                               fn.Dynamic.Any(Function(k) DefaultParameterSignature.HasLambda(fn.Parameters(k).DefaultValue))

                Call targets.Add(fn)
            Next

            Return targets
        End Function

        ' ==================================================================
        ' 第2趟: 扫描调用点
        ' ==================================================================

        Private Sub ScanCallSites(lines As String(),
                                  targets As List(Of DefaultParameterFunction),
                                  ctx As Context,
                                  edits As LineEdit(),
                                  report As DefaultParameterReport)

            For i As Integer = 0 To lines.Length - 1
                Dim line As String = lines(i)
                Dim masked As String = PropertyProjection.MaskLiterals(line)

                For Each fn As DefaultParameterFunction In targets
                    ' 声明行自身不参与
                    If fn.LineIndex = i Then
                        Continue For
                    End If

                    Dim search As Integer = 0

                    Do
                        Dim at As Integer = IndexOfWord(masked, fn.Name, search)

                        If at < 0 Then
                            Exit Do
                        End If

                        search = at + 1

                        If Not IsCallee(masked, at) Then
                            Continue Do
                        End If

                        Dim openAt As Integer = NextNonSpace(masked, at + fn.Name.Length)

                        If openAt < 0 OrElse masked(openAt) <> "("c Then
                            ' 名字后面没有实参表: 可能是变量引用/声明/AddressOf, 只有确实像"无括号调用"时才记为未覆盖
                            Call MarkAmbiguous(fn, masked, at + fn.Name.Length)
                            Continue Do
                        End If

                        Dim closeAt As Integer = DefaultParameterSignature.MatchingClose(masked, openAt)

                        If closeAt < 0 Then
                            ' 实参表跨物理行(续行) => 无法改写
                            fn.Uncovered = True
                            Continue Do
                        End If

                        Dim bindings As List(Of ArgBinding) = Nothing

                        If Not TryBindArguments(fn, line.Substring(openAt + 1, closeAt - openAt - 1), openAt + 1, bindings) Then
                            fn.Uncovered = True
                            Continue Do
                        End If

                        Dim supplied As Boolean() = New Boolean(fn.Parameters.Count - 1) {}
                        Dim invalid As Boolean = False
                        Dim need As Boolean = False

                        For Each b As ArgBinding In bindings
                            supplied(b.ParamIndex) = True
                        Next

                        For k As Integer = 0 To fn.Parameters.Count - 1
                            If supplied(k) Then
                                Continue For
                            End If

                            If Not fn.Parameters(k).HasDefault Then
                                ' 必填参数被省略: 调用本身非法, 交给 Roslyn 报错
                                invalid = True
                            Else
                                ' 只要省略了任何一个带默认值的参数就需要改写:
                                ' 声明改写会把 Optional 全部去掉(运行期顶层函数是匿名函数,
                                ' 而 VB 不允许 lambda 参数声明为 Optional), 因此必须由改写后的调用点把值补齐。
                                need = True
                            End If
                        Next

                        If invalid OrElse Not need Then
                            ' 没有省略任何参数 => 原样调用即可
                            Continue Do
                        End If

                        If edits(i) Is Nothing Then
                            edits(i) = New LineEdit()
                        End If

                        ' ---- 路径 B: Sub 就地展开 ----
                        If Not fn.UseBridge AndAlso
                           edits(i).Expansion Is Nothing AndAlso
                           edits(i).Renames.Count = 0 Then

                            Dim expansion As List(Of String) = Nothing

                            If TryBuildInline(fn, bindings, line, masked, at, closeAt, ctx, expansion) Then
                                edits(i).Expansion = expansion
                                report.Rewritten += 1

                                If Not report.InlineSubs.Contains(fn.Name) Then
                                    Call report.InlineSubs.Add(fn.Name)
                                End If

                                Continue Do
                            End If
                        End If

                        ' ---- 路径 A: 桥接函数(只替换被调用名) ----
                        If edits(i).Expansion IsNot Nothing Then
                            ' 本行已经做过整行展开, 无法再插入改名
                            fn.Uncovered = True
                            Continue Do
                        End If

                        Dim bridge As String = GetBridge(fn, supplied, ctx, report)

                        If bridge Is Nothing Then
                            fn.Uncovered = True
                            Continue Do
                        End If

                        Call edits(i).Renames.Add(New Rename With {
                            .Start = at,
                            .Length = fn.Name.Length,
                            .Text = bridge
                        })

                        ' 具名实参的名字要一并改成桥接函数形参的混淆名字
                        For Each b As ArgBinding In bindings
                            If Not b.IsNamed Then
                                Continue For
                            End If

                            Call edits(i).Renames.Add(New Rename With {
                                .Start = b.NameStart,
                                .Length = b.NameLength,
                                .Text = fn.Mangled(b.ParamIndex)
                            })
                        Next

                        report.Rewritten += 1
                    Loop
                Next
            Next
        End Sub

        ''' <summary>判断名字出现的位置是否可能是一次函数调用(排除成员访问与 <c>AddressOf</c>)</summary>
        Private Function IsCallee(masked As String, at As Integer) As Boolean
            If at > 0 AndAlso masked(at - 1) = "."c Then
                Return False
            End If

            Dim before As String = masked.Substring(0, at).TrimEnd()

            If Regex.IsMatch(before, "\baddressof$", RegexOptions.IgnoreCase) Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' 名字后面没有实参表: 只有确实像「无括号调用」(<c>test a, b</c>)时才记为未覆盖;
        ''' 赋值、声明、变量引用等一律跳过, 避免把正常代码误判成未覆盖。
        ''' </summary>
        Private Sub MarkAmbiguous(fn As DefaultParameterFunction, masked As String, from As Integer)
            Dim at As Integer = NextNonSpace(masked, from)

            If at < 0 Then
                Exit Sub
            End If

            If masked(at) = "="c OrElse "(),:.".IndexOf(masked(at)) >= 0 Then
                Exit Sub
            End If

            If Regex.IsMatch(masked.Substring(at), "^(as|in|to|is)\b", RegexOptions.IgnoreCase) Then
                Exit Sub
            End If

            fn.Uncovered = True
        End Sub

        ''' <summary>解析实参表, 把每个实参绑定到形参</summary>
        Private Function TryBindArguments(fn As DefaultParameterFunction,
                                          argsText As String,
                                          argsBase As Integer,
                                          ByRef bindings As List(Of ArgBinding)) As Boolean

            bindings = New List(Of ArgBinding)()

            If argsText.Trim().Length = 0 Then
                Return True
            End If

            Dim positional As Integer = 0

            For Each part As Integer() In DefaultParameterSignature.SplitWithOffsets(argsText)
                Dim text As String = argsText.Substring(part(0), part(1)).Trim()

                If text.Length = 0 Then
                    Return False
                End If

                Dim m As Match = NamedArgPattern.Match(text)

                If m.Success Then
                    Dim idx As Integer = fn.ParamByName(m.Groups("name").Value)

                    If idx < 0 OrElse bindings.Any(Function(b) b.ParamIndex = idx) Then
                        Return False
                    End If

                    Call bindings.Add(New ArgBinding With {
                        .ParamIndex = idx,
                        .IsNamed = True,
                        .Expression = m.Groups("expr").Value.Trim(),
                        .NameStart = argsBase + part(0) +
                                     DefaultParameterSignature.LeadingSpaces(argsText.Substring(part(0), part(1))) +
                                     m.Groups("name").Index,
                        .NameLength = m.Groups("name").Length
                    })
                ElseIf bindings.Any(Function(b) b.IsNamed) Then
                    ' 位置实参出现在具名实参之后
                    Return False
                ElseIf positional >= fn.Parameters.Count Then
                    Return False
                Else
                    Call bindings.Add(New ArgBinding With {
                        .ParamIndex = positional,
                        .IsNamed = False,
                        .Expression = text
                    })

                    positional += 1
                End If
            Next

            Return True
        End Function

        ' ==================================================================
        ' 路径 B: Sub 调用点就地展开
        ' ==================================================================

        ''' <summary>
        ''' 尝试把一个 Sub 调用点就地展开为「多行临时变量 + 位置形式调用」。
        ''' </summary>
        ''' <remarks>
        ''' 展开次序固定为: 被提供的实参(书写顺序) → 被省略的参数(声明顺序) → 完整调用,
        ''' 与 VB 的「左到右求值」以及「先算实参再算默认值」的语义一致。
        ''' </remarks>
        Private Function TryBuildInline(fn As DefaultParameterFunction,
                                        bindings As List(Of ArgBinding),
                                        line As String,
                                        masked As String,
                                        nameStart As Integer,
                                        closeAt As Integer,
                                        ctx As Context,
                                        ByRef expansion As List(Of String)) As Boolean

            expansion = Nothing

            ' ---- 调用点必须一直延伸到该行结尾(后面只允许是空白或行尾注释) ----
            If masked.Substring(closeAt + 1).Trim().Length > 0 Then
                Return False
            End If

            Dim commentAt As Integer = DefaultParameterSignature.IndexOfComment(line)
            Dim comment As String = ""

            If commentAt > closeAt Then
                comment = line.Substring(commentAt)
            ElseIf commentAt >= 0 AndAlso commentAt <= closeAt Then
                ' 注释出现在调用之前(不应发生) => 放弃
                Return False
            End If

            ' ---- 前导的 Call 关键字 ----
            Dim head As String = masked.Substring(0, nameStart)
            Dim cm As Match = Regex.Match(head, "\bcall\s*$", RegexOptions.IgnoreCase)
            Dim hasCall As Boolean = cm.Success
            Dim prefixStart As Integer = If(hasCall, cm.Index, nameStart)

            ' ---- 判别语句容器 ----
            Dim before As String = line.Substring(0, prefixStart)
            Dim beforeMasked As String = masked.Substring(0, prefixStart)
            Dim kind As Container = Container.None
            Dim cond As String = Nothing
            Dim lambdaHead As String = Nothing
            Dim bodyIndent As String = Nothing
            Dim endIndent As String = Nothing

            If beforeMasked.Trim().Length = 0 Then
                kind = Container.Statement
            ElseIf Regex.IsMatch(beforeMasked, "^\s*if\s+.*\bthen\s*$", RegexOptions.IgnoreCase) AndAlso
                   Not Regex.IsMatch(beforeMasked, "\b(else|elseif)\b", RegexOptions.IgnoreCase) Then

                Dim im As Match = Regex.Match(before, "^\s*if\s+(?<cond>[\s\S]*\S)\s+then\s*$", RegexOptions.IgnoreCase)

                If Not im.Success Then
                    Return False
                End If

                cond = im.Groups("cond").Value
                kind = Container.SingleLineIf
            Else
                ' 单行 Sub lambda: xxx = Sub(参数) <调用>
                Dim trimmed As String = beforeMasked.TrimEnd()

                If trimmed.EndsWith(")") Then
                    Dim closeP As Integer = beforeMasked.LastIndexOf(")"c)
                    Dim openP As Integer = DefaultParameterSignature.MatchingOpen(beforeMasked, closeP)

                    If openP > 0 Then
                        Dim headText As String = line.Substring(0, openP)
                        Dim sm As Match = Regex.Match(headText, "\bsub\s*$", RegexOptions.IgnoreCase)

                        If sm.Success AndAlso headText.Substring(0, sm.Index).TrimEnd().EndsWith("="c) Then
                            lambdaHead = line.Substring(0, closeP + 1)
                            bodyIndent = Spaces(sm.Index + 4)
                            endIndent = Spaces(sm.Index)
                            kind = Container.SingleLineLambda
                        End If
                    End If
                End If
            End If

            If kind = Container.None Then
                Return False
            End If

            ' ---- 生成临时变量 ----
            Dim temps As String() = New String(fn.Parameters.Count - 1) {}
            Dim map As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

            For k As Integer = 0 To fn.Parameters.Count - 1
                temps(k) = NextTempName(ctx, fn.Name, fn.Parameters(k).Name)
                map(fn.Parameters(k).Name) = temps(k)
            Next

            Dim supplied As Boolean() = New Boolean(fn.Parameters.Count - 1) {}

            For Each b As ArgBinding In bindings
                supplied(b.ParamIndex) = True
            Next

            Dim statements As New List(Of String)

            ' 1. 被提供的实参(书写顺序)
            For Each b As ArgBinding In bindings
                Call statements.Add($"Dim {temps(b.ParamIndex)} = {b.Expression}")
            Next

            ' 2. 被省略的参数(声明顺序)
            For k As Integer = 0 To fn.Parameters.Count - 1
                If supplied(k) Then
                    Continue For
                End If

                Dim value As String = DefaultParameterSignature.SubstituteNames(fn.Parameters(k).DefaultValue, map)

                Call statements.Add($"Dim {temps(k)} = {value}")
            Next

            ' 3. 按声明顺序完整调用
            Call statements.Add(If(hasCall,
                $"Call {fn.Name}({String.Join(", ", temps)})",
                $"{fn.Name}({String.Join(", ", temps)})"))

            ' ---- 组装输出行 ----
            Dim lead As String = Regex.Match(line, "^\s*").Value
            Dim result As New List(Of String)
            Dim callLine As Integer = -1

            For s As Integer = 0 To statements.Count - 1
                Dim isCall As Boolean = (s = statements.Count - 1)
                Dim text As String = statements(s)

                If isCall AndAlso comment.Length > 0 Then
                    text &= comment
                End If

                Select Case kind
                    Case Container.Statement
                        Call result.Add(lead & text)
                    Case Container.SingleLineIf
                        Call result.Add(lead & "    " & text)
                    Case Container.SingleLineLambda
                        Call result.Add(bodyIndent & text)
                End Select

                If isCall Then
                    callLine = result.Count - 1
                End If
            Next

            Select Case kind
                Case Container.SingleLineIf
                    Call result.Insert(0, lead & "If " & cond & " Then")
                    Call result.Add(lead & "End If")

                    If callLine >= 0 Then
                        callLine += 1
                    End If
                Case Container.SingleLineLambda
                    Call result.Insert(0, lambdaHead)
                    Call result.Add(endIndent & "End Sub")

                    If callLine >= 0 Then
                        callLine += 1
                    End If
            End Select

            expansion = result
            Return True
        End Function

        ' ==================================================================
        ' 路径 A: 桥接函数
        ' ==================================================================

        ''' <summary>取用(或生成)一个与「实参子集」对应的桥接函数</summary>
        Private Function GetBridge(fn As DefaultParameterFunction,
                                   supplied As Boolean(),
                                   ctx As Context,
                                   report As DefaultParameterReport) As String

            Dim key As String = SubsetKey(fn, supplied)
            Dim name As String = Nothing

            If fn.BridgeIndex.TryGetValue(key, name) Then
                Return name
            End If

            name = NextBridgeName(fn, ctx)
            Dim code As String() = DefaultParameterSignature.EmitBridge(fn, supplied, name, EnsureMangled(fn, ctx))

            If code Is Nothing Then
                Return Nothing
            End If

            Call fn.BridgeIndex.Add(key, name)
            Call fn.BridgeBlocks.Add(code)

            If report IsNot Nothing Then
                Call report.Bridges.Add(name)
            End If

            Return name
        End Function

        ''' <summary>
        ''' 生成(或取用)每个参数在桥接函数之中使用的混淆名字。
        ''' </summary>
        Private Function EnsureMangled(fn As DefaultParameterFunction, ctx As Context) As String()
            If fn.Mangled.Count = fn.Parameters.Count Then
                Return fn.Mangled.ToArray()
            End If

            For Each p As DefaultParameterItem In fn.Parameters
                Dim name As String = "__vbs_" & fn.Name & "_" & p.Name
                Dim n As Integer = 2

                While ctx.Identifiers.Contains(name)
                    name = "__vbs_" & fn.Name & "_" & p.Name & "_" & n.ToString()
                    n += 1
                End While

                Call ctx.Identifiers.Add(name)
                Call fn.Mangled.Add(name)
            Next

            Return fn.Mangled.ToArray()
        End Function

        Private Function SubsetKey(fn As DefaultParameterFunction, supplied As Boolean()) As String
            Dim names As New List(Of String)

            For i As Integer = 0 To fn.Parameters.Count - 1
                If supplied(i) Then
                    Call names.Add(fn.Parameters(i).Name.ToLower())
                End If
            Next

            Return String.Join(",", names)
        End Function

        Private Function NextBridgeName(fn As DefaultParameterFunction, ctx As Context) As String
            Dim baseName As String = fn.Name & "_defaults"
            Dim name As String = baseName
            Dim n As Integer = 2

            While ctx.Identifiers.Contains(name)
                name = baseName & n.ToString()
                n += 1
            End While

            Call ctx.Identifiers.Add(name)

            Return name
        End Function

        Private Function NextTempName(ctx As Context, fnName As String, paramName As String) As String
            Do
                ctx.TempSeq += 1

                Dim name As String = $"__vbs_{fnName}_{paramName}_{ctx.TempSeq}"

                If Not ctx.Identifiers.Contains(name) Then
                    Call ctx.Identifiers.Add(name)

                    Return name
                End If
            Loop
        End Function

        ' ==================================================================
        ' 声明改写
        ' ==================================================================

        ''' <summary>
        ''' 改写声明行: 去掉全部带默认值的参数上的 <c>Optional</c> 与 <c>= 默认值</c>, 使其成为必填参数。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' <b>为什么必须去掉 <c>Optional</c></b>: 运行期发射路径会把顶层函数重写为 <c>Main</c> 之中的
        ''' 匿名函数(<see cref="ScriptStructure.ToLambdaSignature"/>), 而 VB 不允许 lambda 参数声明为
        ''' <c>Optional</c>(BC33010)。因此默认值只能由「桥接函数 / 就地展开的调用点」在调用处补齐。
        ''' </para>
        ''' <para>
        ''' 这也顺带实现了「绝不静默取到 <c>Nothing</c>」: 参数变成必填之后,
        ''' 任何**没有被改写到的**调用点都会在编译期报「未提供参数」, 而不会静默地拿到 <c>Nothing</c>。
        ''' </para>
        ''' </remarks>
        Private Function RewriteDeclaration(line As String,
                                            fn As DefaultParameterFunction,
                                            report As DefaultParameterReport) As String

            Dim spans As New List(Of Rename)

            For Each p As DefaultParameterItem In fn.Parameters
                If Not p.HasDefault Then
                    Continue For
                End If

                ' 去掉 = 默认值(连同等号前后的空白一起)
                Dim start As Integer = p.EqualsStart

                While start > 0 AndAlso Char.IsWhiteSpace(line(start - 1))
                    start -= 1
                End While

                Call spans.Add(New Rename With {
                    .Start = start,
                    .Length = (p.ValueStart + p.ValueLength) - start,
                    .Text = ""
                })

                If p.OptionalStart >= 0 Then
                    Call spans.Add(New Rename With {
                        .Start = p.OptionalStart,
                        .Length = p.OptionalLength,
                        .Text = ""
                    })
                End If

                If p.IsDynamic AndAlso fn.Uncovered AndAlso report IsNot Nothing Then
                    Call report.Required.Add($"{fn.Name}.{p.Name}")
                End If
            Next

            Return ApplyRenames(line, spans)
        End Function

        ''' <summary>从右向左应用字符区间替换, 保证左侧偏移在替换之后依然有效</summary>
        Private Function ApplyRenames(line As String, renames As List(Of Rename)) As String
            For Each r As Rename In renames.OrderByDescending(Function(x) x.Start)
                line = line.Substring(0, r.Start) & r.Text & line.Substring(r.Start + r.Length)
            Next

            Return line
        End Function

        ' ==================================================================
        ' 文本工具
        ' ==================================================================

        ''' <summary>从 <paramref name="from"/> 开始查找一个独立的标识符词元</summary>
        Private Function IndexOfWord(masked As String, word As String, from As Integer) As Integer
            For Each m As Match In Regex.Matches(masked, "\b" & Regex.Escape(word) & "\b", RegexOptions.IgnoreCase)
                If m.Index < from Then
                    Continue For
                End If

                Return m.Index
            Next

            Return -1
        End Function

        ''' <summary>跳过空白, 返回下一个非空白字符的下标; 到行尾返回 -1</summary>
        Private Function NextNonSpace(text As String, from As Integer) As Integer
            Dim i As Integer = from

            While i < text.Length AndAlso Char.IsWhiteSpace(text(i))
                i += 1
            End While

            If i >= text.Length Then
                Return -1
            End If

            Return i
        End Function

        Private Function Spaces(n As Integer) As String
            If n <= 0 Then
                Return ""
            End If

            Return New String(" "c, n)
        End Function
    End Module
End Namespace

