#Region "Microsoft.VisualBasic::db71483f0bafb1ae5b05372b5046e09a, vs_solutions\VBS\src\VBScript\Syntax\ScriptRefactor.vb"

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

    '   Total Lines: 379
    '    Code Lines: 194 (51.19%)
    ' Comment Lines: 121 (31.93%)
    '    - Xml Docs: 86.78%
    ' 
    '   Blank Lines: 64 (16.89%)
    '     File Size: 18.23 KB


    '     Class ScriptRefactor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: BuildCode, DefaultImports, MergeHeaders, MergeTypeBlocks, NormalizeTypeAccess
    '                   PreprocessText, PrintForwarder, Refactor, RefactorPreprocessed
    ' 
    '         Sub: AppendAssemblyAttributes, AppendFunctions, AppendPrintForwarder
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine

Namespace Script

    ''' <summary>
    ''' 把脚本源代码重构为可以直接在内存中编译的完整 VB.NET 源代码。
    ''' </summary>
    ''' <remarks>
    ''' 一次重构过程对应一个对象实例。重构被拆分为两个阶段:
    ''' <list type="number">
    ''' <item><b>扫描</b>: 由 <see cref="ScriptStructure.Scan"/> 完成, 得到与发射方式无关的
    ''' 静态结构(头部语句 / 类型定义块 / 顶层函数块 / 顶层语句槽位);</item>
    ''' <item><b>发射</b>: 由本类型的 <c>BuildCode</c> 把结构组装为固定的
    ''' <c>Namespace + Module + Main</c> 容器代码(顶层函数重写为匿名函数并按依赖落位)。</item>
    ''' </list>
    ''' 工程代码发射由 <c>ProjectCodeBuilder</c> 复用同一份 <see cref="ScriptStructure"/> 完成。
    ''' </remarks>
    Public Class ScriptRefactor

        ReadOnly _syntax As ScriptStructure
        ReadOnly magics As New List(Of String)
        ReadOnly _metadata As ScriptMetadata

        ''' <summary>被 #include 引入的其它脚本所贡献的头部 Imports</summary>
        ReadOnly _includeHeaders As New List(Of String)

        ''' <summary>被 #include 引入的其它脚本所贡献的类型定义块</summary>
        ReadOnly _includeTypes As New List(Of String)

        ''' <summary>创建一个脚本重构器</summary>
        ''' <param name="metadata">脚本头部指令解析得到的程序集元数据(可以为Nothing)</param>
        ''' <param name="magics">需要注入到 VBScriptHostMagics 模块的魔法方法源码</param>
        ''' <param name="includes">被 #include 引入的其它脚本所贡献的代码(可以为Nothing)</param>
        Sub New(metadata As ScriptMetadata, magics As IEnumerable(Of String), Optional includes As IncludeSet = Nothing)
            Me._metadata = metadata
            Me._syntax = New ScriptStructure()
            Call Me.magics.AddRange(magics)

            If includes IsNot Nothing Then
                Call _includeHeaders.AddRange(includes.HeaderImports)
                Call _includeTypes.AddRange(includes.TypeBlocks)
            End If
        End Sub

        ' ==================================================================
        ' 阶段1: 文本级预处理
        ' ==================================================================

        ''' <summary>
        ''' 移除 #include 元数据行, 展开命令行参数语法、let 声明与元组分解语法、默认参数表达式, 并做向量化改写。
        ''' 预处理是纯文本变换, 因此可以被运行期与工程期两条发射路径共用。
        ''' </summary>
        ''' <param name="source">脚本源代码</param>
        ''' <param name="vectorize">
        ''' 是否启用向量化改写; 脚本头部的 <c>#no-vectorize</c> 指令优先级更高。
        ''' 该开关不影响 <c>@</c> 数组投影的展开(那是语法糖, 始终生效)。
        ''' </param>
        ''' <param name="report">可选的向量化改写报告</param>
        ''' <param name="extraTypeBlocks">
        ''' 额外参与 <c>@</c> 元素类型解析的类型定义块(被 <c>#include</c> 引入脚本所贡献的
        ''' <see cref="IncludeSet.TypeBlocks"/>)
        ''' </param>
        ''' <param name="defaultReport">可选的默认参数表达式改写报告</param>
        Public Shared Function PreprocessText(source As String,
                                              Optional vectorize As Boolean = True,
                                              Optional report As VectorizationReport = Nothing,
                                              Optional extraTypeBlocks As IEnumerable(Of String) = Nothing,
                                              Optional defaultReport As DefaultParameterReport = Nothing) As String

            Dim code As String = Regex.Replace(source, "^\s*#include\s+""[^""]*""\s*$", "", RegexOptions.IgnoreCase Or RegexOptions.Multiline)

            ' ?"--a" => args("--a")
            code = Regex.Replace(code, "\?""(?<name>[^""]+)""", "args(""${name}"")")
            ' let x = ... => Dim x As Object = ... (不会改写 LINQ 查询之中的 Let 子句)
            code = LetStatement.Expand(code)
            code = TupleDestructuring.Expand(code)
            ' 非常数默认参数表达式: 声明改 = Nothing, Function 生成桥接函数, Sub 调用点就地展开
            ' (必须早于向量化, 好让生成出来的桥接函数体与临时变量行也能被 @ 投影与 SIMD 处理)
            code = DefaultParameterExpression.Expand(code, defaultReport)
            ' @ 数组投影展开 + 数值向量的算术表达式 => 等价的逐元素 SIMD 调用
            code = Vectorization.Expand(code, enabled:=vectorize, report:=report, extraTypeBlocks:=extraTypeBlocks)

            Return code
        End Function

        ' ==================================================================
        ' 阶段2: 扫描 + 发射
        ' ==================================================================

        ''' <summary>
        ''' 对脚本源代码进行重构, 生成运行期可直接编译的完整代码。
        ''' </summary>
        Public Function Refactor(source As String) As String
            Dim report As New VectorizationReport()
            Dim code As String = PreprocessText(source,
                                               vectorize:=True,
                                               report:=report,
                                               extraTypeBlocks:=_includeTypes)

            Return RefactorPreprocessed(code, withSimd:=report.Rewritten > 0)
        End Function

        ''' <summary>
        ''' 对<b>已经过 <see cref="PreprocessText"/> 处理</b>的脚本代码进行重构。
        ''' </summary>
        ''' <param name="code">已预处理的脚本代码</param>
        ''' <param name="withSimd">生成代码是否需要 <c>Microsoft.VisualBasic.Math.SIMD</c> 的 Imports</param>
        Public Function RefactorPreprocessed(code As String, Optional withSimd As Boolean = False) As String
            Dim syntax As ScriptStructure = ScriptStructure.Scan(code)

            Call syntax.ResolveFunctionSlots()

            Return BuildCode(syntax, withSimd)
        End Function

        ''' <summary>组装为 固定Namespace + Module + Main 的完整可编译代码</summary>
        Private Function BuildCode(syntax As ScriptStructure, withSimd As Boolean) As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine("Option Strict Off")
            Call sb.AppendLine("Option Explicit On")
            Call sb.AppendLine("Option Infer On")
            Call sb.AppendLine()

            Dim headers As String() = MergeHeaders(syntax)

            For Each header As String In headers
                Call sb.AppendLine(header)
            Next

            If headers.Length > 0 Then
                Call sb.AppendLine()
            End If

            For Each line As String In DefaultImports(withSimd)
                Call sb.AppendLine(line)
            Next

            Call AppendAssemblyAttributes(sb)

            Call sb.AppendLine($"Namespace {NamespaceName}")

            Call sb.AppendLine("     Module VBScriptHostMagics")

            For Each magic As String In magics
                Call sb.AppendLine(magic)
            Next

            Call sb.AppendLine("     End Module")


            Call sb.AppendLine($"    Module {ModuleName}")
            Call sb.AppendLine()

            Call AppendPrintForwarder(sb)

            Call sb.AppendLine()
            Call sb.AppendLine($"        Public Function {MainName}(args As CommandLine) As Integer")

            ' 槽位 -1: 不依赖任何顶层变量与其它顶层函数的匿名函数, 放在最前面
            Call AppendFunctions(sb, syntax, -1)

            For i As Integer = 0 To syntax.Slots.Count - 1
                For Each stmt As String In syntax.Slots(i).Statements
                    Call sb.AppendLine("            " & stmt)
                Next

                ' 挂在语句之后的匿名函数: 它捕获的变量到这里已经声明完毕
                Call AppendFunctions(sb, syntax, i)
            Next

            Call sb.AppendLine()
            Call sb.AppendLine("            Return 0")
            Call sb.AppendLine("        End Function")
            Call sb.AppendLine("    End Module")

            ' 类型定义块直接作为顶层命名空间的成员。
            ' 注意: 不能嵌套在 Module 之内 —— VB 不允许在 Module 内部再次声明 Module
            ' (BC30617), 而被 #include 引入的脚本常常会贡献 Module 定义。
            For Each typeBlock As String In MergeTypeBlocks(syntax)
                Call sb.AppendLine()

                Dim lines As String() = typeBlock.Split(vbLf)

                For i As Integer = 0 To lines.Length - 1
                    If i = 0 Then
                        Call sb.AppendLine("    " & NormalizeTypeAccess(lines(i)))
                    Else
                        Call sb.AppendLine("    " & lines(i))
                    End If
                Next
            Next

            Call sb.AppendLine("End Namespace")

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 脚本引擎自动注入的固定 Imports(运行期发射与工程期发射共用)。
        ''' </summary>
        ''' <param name="withSimd">
        ''' 是否注入向量化运算所需的命名空间(<c>Microsoft.VisualBasic.Math.SIMD.Vectorization</c>)。
        ''' 只有脚本确实发生了向量化改写时才需要, 从而让未使用向量化的脚本不受任何影响。
        ''' </param>
        Friend Shared Function DefaultImports(Optional withSimd As Boolean = False) As String()
            Dim list As New List(Of String) From {
                $"Imports {GetType(CommandLine).Namespace}",
                "Imports Microsoft.VisualBasic",
                "Imports System.Linq",
                "Imports System",
                "Imports System.Collections",
                "Imports System.Collections.Generic",
                "Imports System.Data",
                "Imports System.Diagnostics",
                "Imports System.Threading.Tasks",
                "Imports System.Xml.Linq"
            }

            If withSimd Then
                Call list.Add($"Imports {SimdVocabulary.SimdNamespace}")
            End If

            Return list.ToArray()
        End Function

        ''' <summary>
        ''' 脚本引擎注入的调试打印入口 <c>print</c> 的转发函数源码(运行期发射与工程期发射共用)。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' <b>为什么不直接 <c>Imports Microsoft.VisualBasic.Printing</c> 之后让脚本调用</b>:
        ''' 在生成代码的导入作用域之中, <c>Print</c> 这个名字已经被 VB 运行时的
        ''' <c>Microsoft.VisualBasic.FileSystem</c>(文件号重载)以及本框架的
        ''' <c>Microsoft.VisualBasic.CommandLine.CLITools</c> 同时导出。VB 对"两个已导入模块
        ''' 之中的同名成员"会直接报 <c>BC30561</c>(名称不明确), 实测
        ''' <c>print(1)</c> / <c>print("a")</c> / <c>print(2.5)</c> 全部无法编译 ——
        ''' 也就是说 <c>print</c> 这个名字在脚本作用域之中本来就是不可用的。
        ''' 再增加一条 Imports 只会让二义变成三方二义, 所以导入这条路是走不通的。
        ''' </para>
        ''' <para>
        ''' <b>解决办法</b>: 反过来利用 VB 的名字查找顺序 —— "本类型(Module)自身的成员"
        ''' 优先于"已导入命名空间之中的成员"。因此这里把 <c>print</c> 声明为脚本
        ''' <c>Module Program</c> 自己的成员, 一次性把上述两个同名成员全部遮蔽掉;
        ''' 脚本的顶层语句与匿名函数都在这个 Module 之内, 因此都能看到它。
        ''' </para>
        ''' <para>
        ''' 转发目标是运行时的 <c>Microsoft.VisualBasic.Printing.Print.print(data As Object, ...)</c>
        ''' (全限定调用, 从而不需要在生成代码里再增加 Imports 而引入新的二义):
        ''' 脚本是一种动态场景(<c>let</c> 得到 <see cref="Object"/>, 集合类型各不相同),
        ''' 所以只暴露一个 <see cref="Object"/> 形参, 由运行期按实际类型分派到
        ''' 二维表 / 集合 / 标量三种格式。
        ''' </para>
        ''' <para>
        ''' 参数名与缺省值(<c>width=80</c>、<c>digits=7</c>)与运行时定义保持一致,
        ''' 脚本可以直接写 <c>print(x)</c> 或 <c>print(x, width:=40)</c>。
        ''' </para>
        ''' </remarks>
        Friend Shared Function PrintForwarder() As String()
            Return {
                "''' <summary>",
                "''' 数据打印: 按 GNU R 的向量/表格格式输出数据(二维表 / 集合 / 标量)。",
                "''' </summary>",
                "''' <param name=""data"">要打印的数据</param>",
                "''' <param name=""output"">输出设备, 缺省为控制台</param>",
                "''' <param name=""width"">集合折行的每行最大字符数, 缺省 80; 非正数表示不折行</param>",
                "''' <param name=""digits"">浮点数的有效数字位数, 缺省 7</param>",
                "Public Sub print(data As Object,",
                "                 Optional output As System.IO.TextWriter = Nothing,",
                "                 Optional width As Integer = 80,",
                "                 Optional digits As Integer = 7)",
                "",
                "    Call Microsoft.VisualBasic.Printing.Print.print(data, output, width, digits)",
                "End Sub"
            }
        End Function

        ''' <summary>
        ''' 把 <see cref="PrintForwarder"/> 的源码按脚本 Module 成员的缩进(8 空格)写入生成代码。
        ''' </summary>
        ''' <param name="sb">生成代码缓冲区</param>
        ''' <remarks>
        ''' 空行不补缩进, 避免生成文件中出现只有空格的"脏行"。
        ''' </remarks>
        Friend Shared Sub AppendPrintForwarder(sb As StringBuilder)
            For Each line As String In PrintForwarder()
                If line.Length = 0 Then
                    Call sb.AppendLine()
                Else
                    Call sb.AppendLine("        " & line)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 顶层类型只允许 <c>Friend</c>/<c>Public</c>, 因此把类型声明行上的
        ''' <c>Private</c> 规范化为 <c>Friend</c>(仅作用于类型声明行, 不影响类型成员)。
        ''' </summary>
        Friend Shared Function NormalizeTypeAccess(declaration As String) As String
            Return Regex.Replace(declaration, "^\s*Private\s+", "Friend ", RegexOptions.IgnoreCase)
        End Function

        ''' <summary>
        ''' 合并"脚本自身"与"被 #include 引入的脚本"所贡献的头部 Imports/Option 语句(去重, 保持顺序)。
        ''' </summary>
        Private Function MergeHeaders(syntax As ScriptStructure) As String()
            Dim list As New List(Of String)
            Dim added As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For Each header As String In _includeHeaders
                If added.Add(header.Trim()) Then
                    Call list.Add(header)
                End If
            Next

            For Each header As String In syntax.Headers
                If added.Add(header.Trim()) Then
                    Call list.Add(header)
                End If
            Next

            Return list.ToArray()
        End Function

        ''' <summary>
        ''' 合并"脚本自身"与"被 #include 引入的脚本"所贡献的类型定义块。
        ''' 被引入脚本的类型定义块被直接复制到主脚本的顶层命名空间之中。
        ''' </summary>
        Private Function MergeTypeBlocks(syntax As ScriptStructure) As String()
            Dim list As New List(Of String)

            Call list.AddRange(syntax.TypeBlocks)
            Call list.AddRange(_includeTypes)

            Return list.ToArray()
        End Function

        ''' <summary>
        ''' 把脚本头部指令声明的 assembly 级特性输出到生成代码之中。
        ''' 按照 VB 语法要求, assembly 特性必须位于 Imports 之后、Namespace 之前。
        ''' </summary>
        Private Sub AppendAssemblyAttributes(sb As StringBuilder)
            If _metadata Is Nothing Then
                Return
            End If

            Dim attrs As String() = _metadata.BuildAttributes()

            If attrs.Length = 0 Then
                Return
            End If

            Call sb.AppendLine()

            For Each attr As String In attrs
                Call sb.AppendLine(attr)
            Next

            Call sb.AppendLine()
        End Sub

        ''' <summary>把落在指定槽位上的全部顶层函数块输出到Main之中</summary>
        Private Sub AppendFunctions(sb As StringBuilder, syntax As ScriptStructure, slot As Integer)
            For Each func As ScriptFunctionBlock In syntax.Functions
                If func.Slot <> slot Then
                    Continue For
                End If

                For Each line As String In func.Lines
                    Call sb.AppendLine("            " & line)
                Next

                Call sb.AppendLine()
            Next
        End Sub
    End Class
End Namespace
