#Region "Microsoft.VisualBasic::21d409851dd979f084bbbdf0f27e45fe, vs_solutions\VBS\src\VBScript\Syntax\DefaultParameterSignature.vb"

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

    '   Total Lines: 690
    '    Code Lines: 380 (55.07%)
    ' Comment Lines: 164 (23.77%)
    '    - Xml Docs: 83.54%
    ' 
    '   Blank Lines: 146 (21.16%)
    '     File Size: 28.90 KB


    '     Class DefaultParameterItem
    ' 
    '         Properties: DefaultValue, EqualsStart, HasDefault, IsByRef, IsConstant
    '                     IsDynamic, IsOptional, IsParamArray, ModifierText, Name
    '                     OptionalLength, OptionalStart, RankText, TypeText, ValueLength
    '                     ValueStart
    ' 
    '         Function: FormalWith
    ' 
    '     Class DefaultParameterFunction
    ' 
    '         Properties: BridgeBlocks, BridgeIndex, Dynamic, EndLineIndex, IsSub
    '                     LineIndex, Mangled, Name, Parameters, ReturnType
    '                     Uncovered, UseBridge
    ' 
    '         Function: ParamByName
    ' 
    '     Module DefaultParameterSignature
    ' 
    '         Function: BuildItem, EmitBridge, HasLambda, HasWord, IndexOfComment
    '                   IsConstantExpression, LeadingSpaces, MatchingClose, MatchingOpen, SplitWithOffsets
    '                   SubstituteNames, TryParseDeclaration
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions

Namespace Script

    ''' <summary>
    ''' 顶层函数声明之中的一个参数项(纯文本级模型)。
    ''' </summary>
    ''' <remarks>
    ''' 全部定位信息都以「声明行之中的字符偏移」记录, 因此改写阶段只需要替换必要的字符区间,
    ''' 而不会重建整条声明行 —— 修饰符、类型子句与常量默认参数都不会被破坏。
    ''' </remarks>
    Public Class DefaultParameterItem

        ''' <summary>参数名</summary>
        Public Property Name As String

        ''' <summary>是否带有 <c>Optional</c> 修饰符</summary>
        Public Property IsOptional As Boolean

        ''' <summary>是否带有 <c>ByRef</c> 修饰符</summary>
        Public Property IsByRef As Boolean

        ''' <summary>是否带有 <c>ParamArray</c> 修饰符</summary>
        Public Property IsParamArray As Boolean

        ''' <summary>默认值表达式的原文; 没有默认值时为 <c>Nothing</c></summary>
        Public Property DefaultValue As String

        ''' <summary>默认值表达式是否为常量表达式</summary>
        Public Property IsConstant As Boolean

        ''' <summary>去掉 <c>Optional</c> 之后的修饰符文本(例如 <c>ByVal</c>/<c>ByRef</c>)</summary>
        Public Property ModifierText As String

        ''' <summary>数组标记(参数名之后的 <c>()</c>; 没有时为空串)</summary>
        Public Property RankText As String

        ''' <summary><c>As</c> 子句给出的类型文本(没有时为空串)</summary>
        Public Property TypeText As String

        ''' <summary>
        ''' 用给定的形参名构造形参文本(去掉 <c>Optional</c> 与 <c>= 默认值</c>, 保留修饰符与类型子句)。
        ''' </summary>
        Friend Function FormalWith(name As String) As String
            Dim sb As New StringBuilder()

            If Not String.IsNullOrEmpty(ModifierText) Then
                Call sb.Append(ModifierText).Append(" "c)
            End If

            Call sb.Append(name).Append(RankText)

            If TypeText.Length > 0 Then
                Call sb.Append(" As ").Append(TypeText)
            End If

            Return sb.ToString()
        End Function

        ''' <summary><c>Optional</c> 关键字在声明行之中的起始下标(不存在时为 -1)</summary>
        Public Property OptionalStart As Integer = -1

        ''' <summary><c>Optional</c> 关键字连同其后空白的长度</summary>
        Public Property OptionalLength As Integer

        ''' <summary>默认值等号在声明行之中的下标(不存在时为 -1)</summary>
        Public Property EqualsStart As Integer = -1

        ''' <summary>默认值表达式在声明行之中的起始下标(不存在时为 -1)</summary>
        Public Property ValueStart As Integer = -1

        ''' <summary>默认值表达式的长度</summary>
        Public Property ValueLength As Integer

        ''' <summary>该参数是否带有默认值</summary>
        Public ReadOnly Property HasDefault As Boolean
            Get
                Return DefaultValue IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' 该参数的默认值是否为**非常数**表达式(即需要被引擎改写)。
        ''' </summary>
        Public ReadOnly Property IsDynamic As Boolean
            Get
                Return HasDefault AndAlso Not IsConstant
            End Get
        End Property
    End Class

    ''' <summary>
    ''' 一个被解析出来的顶层函数声明(纯文本级模型)。
    ''' </summary>
    Public Class DefaultParameterFunction

        ''' <summary>函数名</summary>
        Public Property Name As String

        ''' <summary>是否为 <c>Sub</c>(没有返回值)</summary>
        Public Property IsSub As Boolean

        ''' <summary>声明行在脚本文本之中的行号</summary>
        Public Property LineIndex As Integer

        ''' <summary>函数块结束行(<c>End Function</c>/<c>End Sub</c>)的行号; 桥接函数紧随其后插入</summary>
        Public Property EndLineIndex As Integer = -1

        ''' <summary>返回类型子句原文(形如 <c>As data</c>; 没有时为空串)</summary>
        Public Property ReturnType As String

        ''' <summary>为该函数生成的桥接函数源码块</summary>
        Public ReadOnly Property BridgeBlocks As New List(Of String())

        ''' <summary>
        ''' 每个参数在桥接函数之中使用的混淆名字(按声明顺序); 首次生成桥接函数时填充。
        ''' </summary>
        Public ReadOnly Property Mangled As New List(Of String)

        ''' <summary>全部参数(按声明顺序)</summary>
        Public ReadOnly Property Parameters As New List(Of DefaultParameterItem)

        ''' <summary>带有非常数默认值的参数下标</summary>
        Public ReadOnly Property Dynamic As New List(Of Integer)

        ''' <summary>
        ''' 是否采用「桥接函数」路径。<c>False</c> 表示该 <c>Sub</c> 可以就地展开为多行临时变量。
        ''' </summary>
        Public Property UseBridge As Boolean

        ''' <summary>是否存在无法被改写的调用点(存在时动态默认参数将退化为必填)</summary>
        Public Property Uncovered As Boolean

        ''' <summary>已经生成过的桥接函数: 「提供的参数名列表」→ 桥接函数名</summary>
        Public ReadOnly Property BridgeIndex As New Dictionary(Of String, String)

        ''' <summary>按名字查找参数下标; 不存在时返回 -1</summary>
        Public Function ParamByName(name As String) As Integer
            For i As Integer = 0 To Parameters.Count - 1
                If String.Equals(Parameters(i).Name, name, StringComparison.OrdinalIgnoreCase) Then
                    Return i
                End If
            Next

            Return -1
        End Function
    End Class

    ''' <summary>
    ''' 顶层函数签名与参数的**文本级**解析模型, 以及桥接函数的源码发射器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本模块只做文本解析: 不建立 Roslyn 语法树、不做语义分析。
    ''' 任何一处不确定都通过返回 <c>False</c> 让上层走「放弃改写」的保守策略。
    ''' </para>
    ''' <para>
    ''' <b>为什么必须记录字符偏移</b>: 声明改写只允许替换「<c>= 默认值</c>」这一段,
    ''' 因此必须精确知道默认值表达式在整条声明行之中的区间。
    ''' </para>
    ''' </remarks>
    Public Module DefaultParameterSignature

        ''' <summary>顶层函数/子程序声明: <c>[修饰符] (Function|Sub) name [(参数表)] [As 返回类型]</c></summary>
        Private ReadOnly DeclarationPattern As New Regex(
            "^\s*(?:(?:public|private|friend|protected|shared|static)\s+)*(?<kind>function|sub)\s+(?<name>[A-Za-z_]\w*)(?<tail>[\s\S]*)$",
            RegexOptions.IgnoreCase)

        ''' <summary>单个参数项: <c>[修饰符] name [()] [As 类型] [= 默认值]</c></summary>
        Private ReadOnly ParameterPattern As New Regex(
            "^(?<mods>(?:(?:byval|byref|optional|paramarray)\s+)*)(?<name>[A-Za-z_]\w*)\s*(?<rank>\(\s*,?\s*\))?\s*" &
            "(?:As\s+(?<type>.+?))?\s*(?:=(?<def>[\s\S]*))?$",
            RegexOptions.IgnoreCase)

        ''' <summary>
        ''' 受支持的类型子句: 简单类型名(允许点号限定)与一维数组标记 <c>()</c>。
        ''' </summary>
        ''' <remarks>
        ''' 泛型类型(例如 <c>List(Of Integer)</c>)一律不支持 —— 既有
        ''' <see cref="ScriptStructure.ToLambdaSignature"/> 的 <c>\([^)]*\)</c> 同样无法处理参数表之中的嵌套括号。
        ''' </remarks>
        Private ReadOnly TypePattern As New Regex("^[A-Za-z_][\w.]*(\s*\(\s*,?\s*\))?$", RegexOptions.IgnoreCase)

        ''' <summary>常量表达式之中允许出现的非数字字符</summary>
        Private Const ConstantChars As String = "+-*/\^&()."

        ' ==================================================================
        ' 声明解析
        ' ==================================================================

        ''' <summary>
        ''' 解析一条顶层函数/子程序声明首行。
        ''' </summary>
        ''' <param name="line">声明行原文</param>
        ''' <param name="lineIndex">该行在脚本文本之中的行号</param>
        ''' <param name="fn">解析结果; 失败时为 <c>Nothing</c></param>
        ''' <param name="bailed">
        ''' 该行确实是顶层函数声明、参数表之中出现了 <c>=</c>, 但因为保守条件而放弃改写。
        ''' </param>
        ''' <returns>
        ''' 解析成功时返回 <c>True</c>(<see cref="DefaultParameterFunction.Dynamic"/> 可能为空 —— 即没有需要改写的默认参数)。
        ''' </returns>
        Friend Function TryParseDeclaration(line As String, lineIndex As Integer,
                                            ByRef fn As DefaultParameterFunction,
                                            ByRef bailed As Boolean) As Boolean

            fn = Nothing
            bailed = False

            Dim m As Match = DeclarationPattern.Match(line)

            If Not m.Success Then
                Return False
            End If

            Dim isSub As Boolean = m.Groups("kind").Value.Equals("sub", StringComparison.OrdinalIgnoreCase)
            Dim name As String = m.Groups("name").Value
            Dim tailFrom As Integer = m.Groups("name").Index + m.Groups("name").Length
            Dim paramsStart As Integer = -1
            Dim paramsEnd As Integer = -1
            Dim ret As String = ""

            ' ---- 定位参数表: 跳过空白之后取第一个左括号, 再做配平扫描 ----
            Dim cursor As Integer = tailFrom

            While cursor < line.Length AndAlso Char.IsWhiteSpace(line(cursor))
                cursor += 1
            End While

            If cursor < line.Length AndAlso line(cursor) = "("c Then
                Dim closeAt As Integer = MatchingClose(line, cursor)

                ' 参数表括号不配平(典型是参数表跨物理行) => 放弃
                If closeAt < 0 Then
                    bailed = True
                    Return False
                End If

                Dim inner As String = line.Substring(cursor + 1, closeAt - cursor - 1)

                ' 方法泛型参数表 (Of T) 不支持
                If Regex.IsMatch(inner, "^\s*Of\s", RegexOptions.IgnoreCase) Then
                    bailed = inner.IndexOf("="c) >= 0
                    Return False
                End If

                paramsStart = cursor + 1
                paramsEnd = closeAt
                ret = line.Substring(closeAt + 1).Trim()
            Else
                ret = line.Substring(tailFrom).Trim()
            End If

            ' Implements / Handles 之类的尾部子句不支持
            If ret.Length > 0 AndAlso Not Regex.IsMatch(ret, "^As\s+\S", RegexOptions.IgnoreCase) Then
                bailed = True
                Return False
            End If

            Dim result As New DefaultParameterFunction With {
                .Name = name,
                .IsSub = isSub,
                .LineIndex = lineIndex,
                .ReturnType = ret
            }

            If paramsStart >= 0 Then
                Dim inner As String = line.Substring(paramsStart, paramsEnd - paramsStart)
                Dim hasEquals As Boolean = inner.IndexOf("="c) >= 0

                If inner.Trim().Length > 0 Then
                    Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

                    For Each part As Integer() In SplitWithOffsets(inner)
                        Dim raw As String = inner.Substring(part(0), part(1))
                        Dim body As String = raw.Trim()

                        ' 空片段(例如尾部多了一个逗号) => 放弃
                        If body.Length = 0 Then
                            bailed = hasEquals
                            Return False
                        End If

                        Dim pm As Match = ParameterPattern.Match(body)

                        If Not pm.Success Then
                            bailed = hasEquals
                            Return False
                        End If

                        Dim item As DefaultParameterItem = BuildItem(pm, inner, part, paramsStart)

                        If item Is Nothing OrElse Not names.Add(item.Name) Then
                            bailed = hasEquals
                            Return False
                        End If

                        Call result.Parameters.Add(item)
                    Next
                End If
            End If

            ' ---- 非常数默认值只允许引用更靠前的参数 ----
            For i As Integer = 0 To result.Parameters.Count - 1
                Dim item As DefaultParameterItem = result.Parameters(i)

                If Not item.IsDynamic Then
                    Continue For
                End If

                For j As Integer = i To result.Parameters.Count - 1
                    If HasWord(item.DefaultValue, result.Parameters(j).Name) Then
                        bailed = True
                        Return False
                    End If
                Next
            Next

            For i As Integer = 0 To result.Parameters.Count - 1
                If result.Parameters(i).IsDynamic Then
                    Call result.Dynamic.Add(i)
                End If
            Next

            fn = result
            Return True
        End Function

        ''' <summary>
        ''' 由一个参数项的正则匹配结果构造参数模型; 遇到不支持的写法时返回 <c>Nothing</c>。
        ''' </summary>
        Private Function BuildItem(pm As Match, inner As String, part As Integer(), paramsStart As Integer) As DefaultParameterItem
            Dim mods As String = pm.Groups("mods").Value
            Dim item As New DefaultParameterItem With {
                .Name = pm.Groups("name").Value,
                .IsOptional = Regex.IsMatch(mods, "\boptional\b", RegexOptions.IgnoreCase),
                .IsByRef = Regex.IsMatch(mods, "\bbyref\b", RegexOptions.IgnoreCase),
                .IsParamArray = Regex.IsMatch(mods, "\bparamarray\b", RegexOptions.IgnoreCase)
            }

            ' ParamArray 不接受改写
            If item.IsParamArray Then
                Return Nothing
            End If

            Dim typeText As String = pm.Groups("type").Value.Trim()

            ' 泛型类型的类型子句带有括号, 一律不支持
            If typeText.Length > 0 AndAlso Not TypePattern.IsMatch(typeText) Then
                Return Nothing
            End If

            ' ---- 形参文本的各组成部分(去掉 Optional 与 = 默认值, 其余原样保留) ----
            item.ModifierText = Regex.Replace(mods, "\boptional\b\s*", "", RegexOptions.IgnoreCase).Trim()
            item.RankText = If(pm.Groups("rank").Success, pm.Groups("rank").Value, "")
            item.TypeText = typeText

            If Not pm.Groups("def").Success Then
                Return item
            End If

            ' ---- 默认值表达式 ----
            Dim raw As String = pm.Groups("def").Value
            Dim lead As Integer = 0

            While lead < raw.Length AndAlso Char.IsWhiteSpace(raw(lead))
                lead += 1
            End While

            Dim text As String = raw.Trim()

            If text.Length = 0 Then
                Return Nothing
            End If

            ' part(0) 是片段在参数表之中的偏移, lead 是片段前导空白的长度
            Dim baseAt As Integer = paramsStart + part(0) + LeadingSpaces(inner.Substring(part(0), part(1)))

            item.EqualsStart = baseAt + pm.Groups("def").Index - 1
            item.ValueStart = item.EqualsStart + 1 + lead
            item.ValueLength = text.Length
            item.DefaultValue = text
            item.IsConstant = IsConstantExpression(text)

            Dim om As Match = Regex.Match(mods, "\boptional\b\s*", RegexOptions.IgnoreCase)

            If om.Success Then
                item.OptionalStart = baseAt + om.Index
                item.OptionalLength = om.Length
            End If

            Return item
        End Function

        ' ==================================================================
        ' 默认值: 常量 / 非常数
        ' ==================================================================

        ''' <summary>
        ''' 判断一个默认值表达式是否为**常量**(或常量表达式)。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 方向是「宁可判为非常数」: 误判为非常数只会多做一次等价改写(多一条 <c>Dim</c> 或一个桥接函数),
        ''' 而漏判却会让 Roslyn 报「常量表达式必需」。因此十六进制字面量(<c>&amp;HFF</c>)、
        ''' 日期字面量(<c>#1/1/2000#</c>)、枚举常量(<c>MyEnum.A</c>)等一律判为非常数。
        ''' </para>
        ''' <para>
        ''' 字符串字面量先被 <see cref="PropertyProjection.MaskLiterals"/> 屏蔽为空格,
        ''' 因此 <c>"abc"</c> 与 <c>"a" &amp; "b"</c> 都会被正确地判为常量。
        ''' </para>
        ''' </remarks>
        Friend Function IsConstantExpression(text As String) As Boolean
            If text Is Nothing Then
                Return False
            End If

            Dim masked As String = PropertyProjection.MaskLiterals(text)

            masked = Regex.Replace(masked, "\b(true|false|nothing)\b", "0", RegexOptions.IgnoreCase)

            For Each ch As Char In masked
                If Char.IsWhiteSpace(ch) OrElse Char.IsDigit(ch) Then
                    Continue For
                End If

                If ConstantChars.IndexOf(ch) < 0 Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 判断一段文本之中是否以「词」的形式引用了给定的标识符(排除成员访问 <c>.name</c>、字符串与注释)。
        ''' </summary>
        Friend Function HasWord(text As String, word As String) As Boolean
            If String.IsNullOrEmpty(text) OrElse String.IsNullOrEmpty(word) Then
                Return False
            End If

            Dim masked As String = PropertyProjection.MaskLiterals(text)

            For Each m As Match In Regex.Matches(masked, "\b" & Regex.Escape(word) & "\b", RegexOptions.IgnoreCase)
                If m.Index > 0 AndAlso masked(m.Index - 1) = "."c Then
                    Continue For
                End If

                Return True
            Next

            Return False
        End Function

        ''' <summary>默认值表达式之中是否出现了 lambda 字面量(<c>Function</c>/<c>Sub</c>)</summary>
        Friend Function HasLambda(text As String) As Boolean
            If String.IsNullOrEmpty(text) Then
                Return False
            End If

            Return Regex.IsMatch(PropertyProjection.MaskLiterals(text), "\b(function|sub)\b", RegexOptions.IgnoreCase)
        End Function

        ' ==================================================================
        ' 桥接函数发射
        ' ==================================================================

        ''' <summary>
        ''' 发射一个桥接函数。
        ''' </summary>
        ''' <param name="fn">原函数</param>
        ''' <param name="supplied">哪些参数由调用点提供(按声明顺序)</param>
        ''' <param name="bridgeName">桥接函数名</param>
        ''' <param name="mangled">
        ''' 每个参数在桥接函数之中使用的名字(按声明顺序)。运行期路径会把桥接函数重写为 <c>Main</c>
        ''' 之中的匿名函数, 而 VB 不允许 lambda 的参数/局部变量遮蔽外层局部变量(BC36641 / BC30616),
        ''' 因此桥接函数内部一律使用带 <c>__vbs_</c> 前缀的混淆名字。
        ''' </param>
        ''' <returns>桥接函数的源码行; 被省略的参数没有默认值时返回 <c>Nothing</c></returns>
        Friend Function EmitBridge(fn As DefaultParameterFunction,
                                   supplied As Boolean(),
                                   bridgeName As String,
                                   mangled As String()) As String()

            Dim formals As New List(Of String)
            Dim args As New List(Of String)
            Dim body As New List(Of String)
            Dim map As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

            For i As Integer = 0 To fn.Parameters.Count - 1
                map(fn.Parameters(i).Name) = mangled(i)
            Next

            For i As Integer = 0 To fn.Parameters.Count - 1
                Dim p As DefaultParameterItem = fn.Parameters(i)

                Call args.Add(mangled(i))

                If supplied(i) Then
                    Call formals.Add(p.FormalWith(mangled(i)))
                ElseIf p.HasDefault Then
                    Call body.Add($"    Dim {mangled(i)} = {SubstituteNames(p.DefaultValue, map)}")
                Else
                    ' 必填参数被省略(调用本身非法) => 不生成猜测性代码
                    Return Nothing
                End If
            Next

            Dim lines As New List(Of String)

            Call lines.Add($"' 由脚本引擎生成: {fn.Name} 的默认参数表达式桥接函数")

            If fn.IsSub Then
                Call lines.Add($"Sub {bridgeName}({String.Join(", ", formals)})")
            Else
                Call lines.Add($"Function {bridgeName}({String.Join(", ", formals)}) {fn.ReturnType}".Trim())
            End If

            Call lines.AddRange(body)

            If fn.IsSub Then
                Call lines.Add($"    Call {fn.Name}({String.Join(", ", args)})")
                Call lines.Add("End Sub")
            Else
                Call lines.Add($"    Return {fn.Name}({String.Join(", ", args)})")
                Call lines.Add("End Function")
            End If

            Return lines.ToArray()
        End Function

        ' ==================================================================
        ' 名字替换(Sub 就地展开使用)
        ' ==================================================================

        ''' <summary>
        ''' 把一段表达式文本之中的参数名替换为对应的临时变量名。
        ''' </summary>
        ''' <param name="text">默认值表达式原文</param>
        ''' <param name="map">参数名 → 临时变量名</param>
        ''' <remarks>
        ''' 替换建立在 <see cref="PropertyProjection.MaskLiterals"/> 的等长掩码之上,
        ''' 因此字符串字面量与注释内部不会被误替换; 成员访问(<c>obj.a</c>)之中的名字也不会被替换。
        ''' 同一个表达式之中的多处替换一律**从右向左**回写, 保证左侧偏移依然有效。
        ''' </remarks>
        Friend Function SubstituteNames(text As String, map As Dictionary(Of String, String)) As String
            If String.IsNullOrEmpty(text) OrElse map Is Nothing OrElse map.Count = 0 Then
                Return text
            End If

            Dim masked As String = PropertyProjection.MaskLiterals(text)
            Dim hits As New List(Of Match)()

            For Each m As Match In Regex.Matches(masked, "[A-Za-z_]\w*")
                If Not map.ContainsKey(m.Value) Then
                    Continue For
                End If

                If m.Index > 0 AndAlso masked(m.Index - 1) = "."c Then
                    Continue For
                End If

                Call hits.Add(m)
            Next

            For i As Integer = hits.Count - 1 To 0 Step -1
                Dim at As Integer = hits(i).Index

                text = text.Substring(0, at) & map(hits(i).Value) & text.Substring(at + hits(i).Length)
            Next

            Return text
        End Function

        ' ==================================================================
        ' 文本工具
        ' ==================================================================

        ''' <summary>
        ''' 按顶层逗号切分, 同时保留每一段在原文之中的起始下标。
        ''' </summary>
        ''' <remarks>
        ''' 逻辑与 <see cref="ScriptStructure.SplitTopLevel"/> 一致(不切圆括号/花括号内部的逗号,
        ''' 也不切字符串内部的逗号), 只是额外给出下标以便定位默认值表达式。
        ''' </remarks>
        Friend Function SplitWithOffsets(s As String) As List(Of Integer())
            Dim parts As New List(Of Integer())
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
                    Case "("c, "{"c : depth += 1
                    Case ")"c, "}"c : depth -= 1
                    Case ","c
                        If depth = 0 Then
                            Call parts.Add(New Integer() {start, i - start})
                            start = i + 1
                        End If
                End Select
            Next

            Call parts.Add(New Integer() {start, s.Length - start})

            Return parts
        End Function

        ''' <summary>定位与 <paramref name="open"/> 处左括号配平的右括号下标; 不配平返回 -1</summary>
        Friend Function MatchingClose(text As String, open As Integer) As Integer
            Dim depth As Integer = 0

            For i As Integer = open To text.Length - 1
                Select Case text(i)
                    Case "("c
                        depth += 1
                    Case ")"c
                        depth -= 1

                        If depth = 0 Then
                            Return i
                        End If
                End Select
            Next

            Return -1
        End Function

        ''' <summary>定位与 <paramref name="closeAt"/> 处右括号配平的左括号下标; 不配平返回 -1</summary>
        Friend Function MatchingOpen(text As String, closeAt As Integer) As Integer
            Dim depth As Integer = 0

            For i As Integer = closeAt To 0 Step -1
                Select Case text(i)
                    Case ")"c
                        depth += 1
                    Case "("c
                        depth -= 1

                        If depth = 0 Then
                            Return i
                        End If
                End Select
            Next

            Return -1
        End Function

        ''' <summary>定位行尾注释的起点(排除字符串内部的 <c>'</c>); 没有注释时返回 -1</summary>
        Friend Function IndexOfComment(s As String) As Integer
            Dim inString As Boolean = False

            For i As Integer = 0 To s.Length - 1
                Dim ch As Char = s(i)

                If ch = """"c Then
                    inString = Not inString
                ElseIf ch = "'"c AndAlso Not inString Then
                    Return i
                End If
            Next

            Return -1
        End Function

        ''' <summary>统计一段文本的前导空白字符个数</summary>
        Friend Function LeadingSpaces(s As String) As Integer
            Dim n As Integer = 0

            While n < s.Length AndAlso Char.IsWhiteSpace(s(n))
                n += 1
            End While

            Return n
        End Function
    End Module
End Namespace

