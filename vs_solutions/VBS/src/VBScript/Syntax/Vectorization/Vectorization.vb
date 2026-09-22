#Region "Microsoft.VisualBasic::f5f4e433ee1992bdfc21eabae16fa0fd, vs_solutions\VBS\src\VBScript\Syntax\Vectorization\Vectorization.vb"

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

    '   Total Lines: 175
    '    Code Lines: 74 (42.29%)
    ' Comment Lines: 74 (42.29%)
    '    - Xml Docs: 95.95%
    ' 
    '   Blank Lines: 27 (15.43%)
    '     File Size: 7.97 KB


    '     Class VectorizationReport
    ' 
    '         Properties: ObjectTypes, Projections, Rewritten, Skipped, Vectors
    ' 
    '     Module Vectorization
    ' 
    ' 
    '         Enum Directive
    ' 
    '             Disable, Enable, None
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: CollapseSpace, DirectiveOf, Expand
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Script

    ''' <summary>
    ''' 向量化改写报告: 供 <c>--verbose</c> 模式输出改写规模与未能改写的可疑行。
    ''' </summary>
    Public Class VectorizationReport

        ''' <summary>发生改写的语句行数</summary>
        Public Property Rewritten As Integer

        ''' <summary>
        ''' 疑似应当改写但未能改写的行(例如跨物理行的表达式续行、语法不完整的行)。
        ''' 只有在这些行确实引用了已知向量变量时才会被记录。
        ''' </summary>
        Public ReadOnly Property Skipped As New List(Of String)

        ''' <summary>
        ''' <c>@</c> 数组投影运算符被展开的**次数**。
        ''' <c>@</c> 是语法糖(始终生效), 因此即使 SIMD 改写被 <c>--no-vectorize</c> 关闭,
        ''' 这个计数仍然可能大于 0。
        ''' </summary>
        Public Property Projections As Integer

        ''' <summary>本次改写所识别到的向量变量名</summary>
        Public ReadOnly Property Vectors As New List(Of String)

        ''' <summary>
        ''' 本次成功解析出成员表的类型名(供 <c>--verbose</c> 诊断)
        ''' </summary>
        ''' <remarks>
        ''' <c>@</c> 的失败模式是"静默不展开"(于是脚本报语法错误), 因此需要给出
        ''' 「哪些类型的成员表是可用的」这一线索: 若目标类型不在此列表里,
        ''' 就说明它的定义不在主脚本或 <c>#include</c> 脚本之中。
        ''' </remarks>
        Public ReadOnly Property ObjectTypes As New List(Of String)
    End Class

    ''' <summary>
    ''' 脚本向量化预处理阶段。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本模块是 <see cref="ScriptRefactor.PreprocessText"/> 的一个阶段, 位置在
    ''' <c>let</c> 展开与元组分解之后。它把脚本之中「数值向量参与算术运算」的标量写法
    ''' 改写为等价的运行时 SIMD 调用, 从而让脚本作者不必手写 <c>For</c> 循环。
    ''' </para>
    ''' <para>
    ''' <b>为什么放在预处理阶段</b>: <see cref="ScriptParseResult.PreprocessedCode"/>
    ''' 是运行期(<see cref="ScriptRefactor"/>)与工程期(<see cref="ProjectCodeBuilder"/>)
    ''' 两条发射路径共用的中间产物, 因此一处接入即可让 <c>vbs run.vb</c> 与
    ''' <c>vbs make-project</c> 自动保持一致。
    ''' </para>
    ''' <para>
    ''' 同一个阶段里还负责 <c>@</c> 数组投影运算符的展开
    ''' (<c>list@x</c> → <c>list.Select(Function(__vbs_o) __vbs_o.x).ToArray()</c>,
    ''' 见 <see cref="PropertyProjection"/>)。两者的差别是:
    ''' <c>@</c> 属于**语法糖**(与 <c>let</c>、元组分解同级), 始终生效;
    ''' SIMD 算术改写属于**优化**, 可以被开关关闭。
    ''' </para>
    ''' <para>
    ''' <b>开关</b>: 默认自动生效; 脚本头部写 <c>#no-vectorize</c> 可以按脚本关闭,
    ''' 命令行 <c>--no-vectorize</c> 可以按次关闭。两条指令行本身会被剔除, 不会进入生成代码;
    ''' <c>#vectorize</c> / <c>#vectorize off</c> 的形式亦可识别。
    ''' 关闭时 <c>@</c> 展开与类型登记照常进行, 只是不再发射 SIMD 调用。
    ''' </para>
    ''' </remarks>
    Public Module Vectorization

        ''' <summary>关闭向量化的头部指令</summary>
        Public Const NoVectorizeDirective As String = "#no-vectorize"

        ''' <summary>重新打开向量化的头部指令</summary>
        Public Const VectorizeDirective As String = "#vectorize"

        ''' <summary>一行源码所携带的向量化开关指令</summary>
        Private Enum Directive
            None
            Enable
            Disable
        End Enum

        ''' <summary>
        ''' 对脚本代码做 <c>@</c> 投影展开与(可选的)SIMD 向量化改写。
        ''' </summary>
        ''' <param name="source">
        ''' 已经过 <see cref="ScriptRefactor.PreprocessText"/> 之中其它阶段处理的脚本代码
        ''' (即已完成 <c>#include</c> 剔除、<c>?参数</c> 展开、<c>let</c> 展开与元组分解展开)
        ''' </param>
        ''' <param name="enabled">
        ''' 命令行给出的默认 SIMD 开关; 脚本头部的 <c>#no-vectorize</c> / <c>#vectorize</c> 指令优先。
        ''' 该开关**不影响** <c>@</c> 投影展开。
        ''' </param>
        ''' <param name="report">可选的改写报告</param>
        ''' <param name="extraTypeBlocks">
        ''' 额外参与 <c>@</c> 元素类型解析的类型定义块 —— 由调用方传入被 <c>#include</c>
        ''' 引入脚本所贡献的 <see cref="IncludeSet.TypeBlocks"/>; 主脚本自身的类型块由本方法内部扫描。
        ''' </param>
        Public Function Expand(source As String,
                               Optional enabled As Boolean = True,
                               Optional report As VectorizationReport = Nothing,
                               Optional extraTypeBlocks As IEnumerable(Of String) = Nothing) As String

            If String.IsNullOrEmpty(source) Then
                Return source
            End If

            Dim lines As New List(Of String)
            Dim effective As Boolean = enabled

            ' ---- 1. 解析并剔除开关指令 ----
            For Each raw As String In source.LineTokens
                Select Case DirectiveOf(raw)
                    Case Directive.Disable
                        effective = False
                        Continue For
                    Case Directive.Enable
                        effective = True
                        Continue For
                    Case Else
                        Call lines.Add(raw)
                End Select
            Next

            ' ---- 2. 收集类型定义: 主脚本 + 被 #include 引入脚本 ----
            Dim blocks As New List(Of String)

            Call blocks.AddRange(ScriptStructure.Scan(String.Join(vbCrLf, lines)).TypeBlocks)

            If extraTypeBlocks IsNot Nothing Then
                Call blocks.AddRange(extraTypeBlocks)
            End If

            Dim members As ObjectMemberTable = ObjectMemberTable.FromTypeBlocks(blocks)

            ' ---- 3. 逐行处理: @ 投影展开(始终) + SIMD 改写(受开关控制) ----
            Dim rewriter As New VectorExpressionRewriter(members)
            Dim output As New List(Of String)

            For Each raw As String In lines
                Call output.Add(rewriter.RewriteLine(raw, report, withSimd:=effective))
            Next

            If report IsNot Nothing Then
                Call report.Vectors.AddRange(rewriter.VectorNames)
                Call report.ObjectTypes.AddRange(members.TypeNames)
            End If

            Return String.Join(vbCrLf, output)
        End Function

        ''' <summary>解析一行是否为向量化开关指令</summary>
        Private Function DirectiveOf(line As String) As Directive
            Dim text As String = line.Trim()

            If Not text.StartsWith("#") Then
                Return Directive.None
            End If

            text = text.Substring(1).Trim().Replace(":", " ").ToLower()

            Select Case CollapseSpace(text)
                Case "no-vectorize", "vectorize off"
                    Return Directive.Disable
                Case "vectorize", "vectorize on"
                    Return Directive.Enable
                Case Else
                    Return Directive.None
            End Select
        End Function

        Private Function CollapseSpace(text As String) As String
            Return System.Text.RegularExpressions.Regex.Replace(text, "\s+", " ").Trim()
        End Function
    End Module
End Namespace
