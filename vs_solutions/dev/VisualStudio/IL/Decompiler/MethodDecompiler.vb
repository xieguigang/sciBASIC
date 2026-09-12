#Region "Microsoft.VisualBasic::15706fba8409c6a03501d23ed97552b1, vs_solutions\dev\VisualStudio\IL\Decompiler\MethodDecompiler.vb"

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

    '   Total Lines: 261
    '    Code Lines: 164 (62.84%)
    ' Comment Lines: 43 (16.48%)
    '    - Xml Docs: 46.51%
    ' 
    '   Blank Lines: 54 (20.69%)
    '     File Size: 11.60 KB


    '     Class DecompileDiagnostics
    ' 
    '         Properties: HasMessages, Messages
    ' 
    '         Function: ToString
    ' 
    '         Sub: Info, Warn
    ' 
    '     Class DecompileOptions
    ' 
    '         Properties: AllowUninitializedLocals, Diagnostics
    ' 
    '     Class DecompileException
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Module MethodDecompiler
    ' 
    '         Function: Decompile, DescribeStatement, DumpStructure, HoistDeclarations
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 反编译总入口：MethodInfo -> MethodSyntax（自定义 AST）
'
' 管道：读 IL -> 切基本块 -> 支配树 / 后支配 / 自然循环 -> SSA -> 栈模拟 -> 结构化还原
'
' 设计约定：
'   * 只接受 Shared（静态）方法：实例方法带 this，无法直接映射成 CUDA 内核参数；
'   * 不支持的指令立即抛 <see cref="DecompileException"/> 并带上 IL 偏移，
'     绝不"猜"着生成代码；
'   * 过程中的非致命问题写进 <see cref="DecompileDiagnostics"/>，不打断流程。
' ---------------------------------------------------------------------------

Imports System.Reflection

Namespace IL

    ''' <summary>反编译过程中产生的结构化诊断（框架层不写控制台）</summary>
    Public Class DecompileDiagnostics

        Private ReadOnly _messages As New List(Of String)()

        Public ReadOnly Property Messages As IReadOnlyList(Of String)
            Get
                Return _messages
            End Get
        End Property

        ''' <summary>记录一条与 IL 偏移相关的警告</summary>
        Public Sub Warn(offset As Integer, message As String)
            _messages.Add($"IL_{offset.ToString("X4")}: {message}")
        End Sub

        ''' <summary>记录一条与具体指令无关的提示</summary>
        Public Sub Info(message As String)
            _messages.Add(message)
        End Sub

        Public ReadOnly Property HasMessages As Boolean
            Get
                Return _messages.Count > 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Join(Environment.NewLine, _messages)
        End Function
    End Class

    ''' <summary>反编译选项</summary>
    Public Class DecompileOptions

        ''' <summary>结构化诊断的收集器；不传时内部自动建一个</summary>
        Public Property Diagnostics As DecompileDiagnostics

        ''' <summary>是否允许出现未初始化就读取的局部变量（VB 通常会先置零，允许时只记警告）</summary>
        Public Property AllowUninitializedLocals As Boolean = True
    End Class

    ''' <summary>反编译失败</summary>
    Public Class DecompileException : Inherits Exception

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class

    Public Module MethodDecompiler

        ''' <summary>
        ''' 把一个已编译的 VB.NET 方法反编译成自定义表达式/语句树。
        ''' </summary>
        ''' <param name="method">目标方法（必须是 Shared 且有 IL 体）</param>
        ''' <param name="options">可选的反编译选项</param>
        Public Function Decompile(method As MethodInfo,
                                  Optional options As DecompileOptions = Nothing) As MethodSyntax
            If method Is Nothing Then Throw New ArgumentNullException(NameOf(method))

            If options Is Nothing Then options = New DecompileOptions()
            If options.Diagnostics Is Nothing Then options.Diagnostics = New DecompileDiagnostics()

            Dim diag = options.Diagnostics

            If method.IsAbstract Then
                Throw New DecompileException($"方法 {method.Name} 是抽象方法，没有 IL 体")
            End If

            If method.ContainsGenericParameters Then
                Throw New DecompileException($"方法 {method.Name} 含有未绑定的泛型参数，暂不支持")
            End If

            If Not method.IsStatic Then
                Throw New DecompileException(
                    $"方法 {method.Name} 是实例方法：this 引用无法映射为 CUDA 内核参数，请声明为 Shared")
            End If

            Using reader As New MethodBodyReader(method)
                If reader.Instructions.Count = 0 Then
                    Throw New DecompileException($"方法 {method.Name} 没有 IL 体（可能是 extern / 抽象方法）")
                End If

                ' 1) 控制流图
                Dim cfg = ControlFlowGraph.Build(reader)
                cfg.ComputeDominators()
                cfg.ComputePostDominators()
                cfg.ComputeNaturalLoops()

                If reader.HasExceptionHandlers Then
                    diag.Info($"方法 {method.Name} 含有异常处理子句，反编译结果不包含 try/catch/finally 语义")
                End If

                ' 2) SSA
                Dim ssa = SsaBuilder.Build(cfg, method, reader)

                ' 3) 栈模拟归约
                Dim simulator As New StackSimulator(cfg, ssa, method, diag, options)
                simulator.Run()

                ' 4) 结构化还原
                Dim recovery As New StructureRecovery(cfg, diag)
                Dim body = recovery.Build()

                Dim syntax As New MethodSyntax With {
                    .Name = method.Name,
                    .Method = method,
                    .ReturnType = method.ReturnType,
                    .IsStatic = method.IsStatic,
                    .Body = body,
                    .Diagnostics = diag
                }

                Dim parameters = method.GetParameters()

                For i As Integer = 0 To parameters.Length - 1
                    syntax.Parameters.Add(New ParameterDeclaration(
                        i, parameters(i).Name, parameters(i).ParameterType,
                        SsaBuilder.ParameterBaseName(parameters(i).Name)))
                Next

                For i As Integer = 0 To reader.Locals.Count - 1
                    syntax.Locals.Add(New LocalDeclaration(
                        i, "V_" & i, reader.Locals(i).LocalType))
                Next

                ' 5) 把局部槽位、phi 变量与栈合并变量的声明提升到函数体最前面
                syntax.Body = HoistDeclarations(syntax, ssa, simulator)

                Return syntax
            End Using
        End Function

        ''' <summary>
        ''' SSA 的 phi 变量与栈合并变量在所有路径上被赋值、在汇聚点之后被读取，
        ''' 因此它们的声明必须出现在所有赋值之前。统一提到函数体最前最简单也最安全。
        ''' </summary>
        Private Function HoistDeclarations(syntax As MethodSyntax,
                                           ssa As SsaBuilder,
                                           simulator As StackSimulator) As BlockStatement
            Dim hoisted As New BlockStatement()
            Dim body = If(syntax.Body, New BlockStatement())

            ' VB 的局部变量由 CLR 保证零初始化，且在赋值前就被读取是合法的
            ' （例如循环里的 "Dim v As Single" 在循环头上会参与 phi，入边就是未赋值的初值）。
            ' C 里读未初始化变量是未定义行为，因此这里把每个局部槽位都显式声明并置零，
            ' 语义与 CLR 一致，也避免生成引用了不存在变量的赋值。
            For Each local In If(syntax.Locals, New List(Of LocalDeclaration)())
                If local.LocalType Is Nothing Then Continue For

                Dim zeroType = local.LocalType
                Dim zero As Object = Nothing

                If zeroType = GetType(Single) Then
                    zero = 0.0F
                ElseIf zeroType = GetType(Double) Then
                    zero = 0.0R
                ElseIf zeroType = GetType(Integer) Then
                    zero = 0
                ElseIf zeroType = GetType(Long) Then
                    zero = 0L
                ElseIf zeroType = GetType(Boolean) Then
                    zero = False
                End If

                hoisted.Statements.Add(New VariableDeclarationStatement(
                    local.Name,
                    zeroType,
                    New LiteralExpression(zero, zeroType),
                    local.Index))
            Next

            For Each pair In ssa.Phis
                For Each phi As SsaBuilder.PhiInfo In pair.Value
                    hoisted.Statements.Add(New VariableDeclarationStatement(phi.Name, phi.VarType))
                Next
            Next

            hoisted.Statements.AddRange(simulator.MergeDeclarations)
            hoisted.Statements.AddRange(body.Statements)

            Return hoisted
        End Function

        ' ==================================================================
        ' 诊断
        ' ==================================================================

        ''' <summary>
        ''' 诊断用：把 IL 指令流与基本块 / 支配 / 循环结构打印成文本。
        ''' 反编译失败（尤其是"某个控制流形状归约不出来"）时，先看这份转储再改代码。
        ''' </summary>
        Public Function DumpStructure(method As MethodInfo) As String
            If method Is Nothing Then Throw New ArgumentNullException(NameOf(method))

            Dim text As New System.Text.StringBuilder()

            Using reader As New MethodBodyReader(method)
                text.AppendLine("IL 指令流：")
                text.AppendLine(reader.GetBodyCode())

                Dim cfg = ControlFlowGraph.Build(reader)

                cfg.ComputeDominators()
                cfg.ComputePostDominators()
                cfg.ComputeNaturalLoops()

                text.AppendLine()
                text.AppendLine("基本块：")

                For Each b As BasicBlock In cfg.Blocks
                    Dim preds = String.Join(",", b.Predecessors.Select(Function(x) "bb" & x))
                    Dim succs = String.Join(",", b.Successors.Select(Function(x) "bb" & x))
                    Dim ipdom = If(b.ImmediatePostDominator < 0, "exit", "bb" & b.ImmediatePostDominator)

                    text.AppendLine($"  {b}  pred=[{preds}] succ=[{succs}] " &
                                    $"idom=bb{b.ImmediateDominator} ipdom={ipdom} " &
                                    $"loop={b.IsLoopHeader} latch=bb{b.LoopLatch} follow=bb{b.LoopFollow} " &
                                    $"T=bb{b.TrueSuccessor} F=bb{b.FalseSuccessor}")

                    If b.Condition IsNot Nothing Then
                        text.AppendLine($"       cond = {SyntaxWriter.WriteExpression(b.Condition)}")
                    End If

                    For Each stmt As Statement In b.Statements
                        text.AppendLine("       " & DescribeStatement(stmt))
                    Next
                Next
            End Using

            Return text.ToString()
        End Function

        Private Function DescribeStatement(stmt As Statement) As String
            Select Case stmt.Kind
                Case SyntaxKind.VariableDeclaration : Return DirectCast(stmt, VariableDeclarationStatement).ToString()
                Case SyntaxKind.Assignment : Return DirectCast(stmt, AssignmentStatement).ToString()
                Case SyntaxKind.ReturnStmt : Return DirectCast(stmt, ReturnStatement).ToString()
                Case SyntaxKind.ExpressionStatement : Return DirectCast(stmt, ExpressionStatement).ToString()
                Case Else : Return stmt.Kind.ToString()
            End Select
        End Function
    End Module
End Namespace
