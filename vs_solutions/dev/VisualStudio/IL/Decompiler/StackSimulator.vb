' ---------------------------------------------------------------------------
' 栈模拟归约器（IL -> 表达式树 的核心算法）
'
' 做法：按 RPO 顺序处理每个基本块，块内维护一个"工作栈"，栈里放的不是值而是
' 表达式节点。逐条指令：
'   * 压栈类（ldc / ldarg / ldloc / ldelem / ldlen / dup）  -> 构造节点压入工作栈
'   * 运算类（add / sub / mul / div / ceq / clt / neg ...）-> 弹出操作数，构造
'     二元 / 一元节点，再把结果压回工作栈
'   * 调用类（call / callvirt）                            -> 按参数个数逆序弹栈，
'     构造调用节点压回（void 调用直接作为语句）
'   * stloc                                                -> 弹出栈顶，生成"声明 + 初值"
'   * starg                                                -> 弹出栈顶，生成赋值
'   * 条件分支（brtrue / brfalse / beq...）                 -> 弹出条件并归一化成
'     "条件成立去 true 后继"，记录在块上
'   * ret                                                  -> 弹出返回值，生成 return
'
' 变量的取值不需要符号环境：SSA 重命名已经把"这一处读到的变量叫什么"算好放在
' UseNames 里，直接取名即可。
'
' phi 的消解在这里完成：走到块末尾时，为本块各个后继块里的 phi 追加一条
' "<phi 名> = <本块出口处该变量的名字>" 的赋值语句（要求本块只有一个后继，
' 结构化代码天然满足；不满足时抛诊断而不是生成错代码）。
' ---------------------------------------------------------------------------

Imports System.Reflection
Imports System.Reflection.Emit

Namespace IL

    ''' <summary>把 IL 指令流归约成语句与表达式节点</summary>
    Public Class StackSimulator

        Private ReadOnly _cfg As ControlFlowGraph
        Private ReadOnly _ssa As SsaBuilder
        Private ReadOnly _method As MethodInfo
        Private ReadOnly _diagnostics As DecompileDiagnostics
        Private ReadOnly _options As DecompileOptions

        ''' <summary>块编号 -> 该块出口处的求值栈</summary>
        Private ReadOnly _exitStacks As New Dictionary(Of Integer, List(Of Expression))()

        ''' <summary>跨块栈值合并时临时引入的变量声明（需提升到函数体开头）</summary>
        Public ReadOnly Property MergeDeclarations As New List(Of VariableDeclarationStatement)

        Public Sub New(cfg As ControlFlowGraph,
                       ssa As SsaBuilder,
                       method As MethodInfo,
                       diagnostics As DecompileDiagnostics,
                       options As DecompileOptions)
            _cfg = cfg
            _ssa = ssa
            _method = method
            _diagnostics = If(diagnostics, New DecompileDiagnostics())
            _options = If(options, New DecompileOptions())
            Me.MergeDeclarations = New List(Of VariableDeclarationStatement)()
        End Sub

        ' ==================================================================
        ' 主循环
        ' ==================================================================

        ''' <summary>按 RPO 逐块做栈模拟，把结果写回各个 <see cref="BasicBlock"/></summary>
        Public Sub Run()
            For Each id As Integer In _cfg.ReversePostOrderIds()
                SimulateBlock(_cfg.Blocks(id))
            Next
        End Sub

        Private Sub SimulateBlock(b As BasicBlock)
            Dim stack As New List(Of Expression)()

            stack.AddRange(BuildEntryStack(b))

            For Each ins As ILInstruction In b.Instructions
                Reduce(b, ins, stack)
            Next

            b.ExitStack = New List(Of Expression)(stack)
            _exitStacks(b.Id) = b.ExitStack

            AppendPhiAssignments(b)
        End Sub

        ''' <summary>
        ''' 由各前驱的出口栈合成入口栈。深度不一致直接报错；同一槽位上前驱们留下的
        ''' 节点不同（短路布尔的典型形态）时引入一个合并变量，并在各前驱末尾补赋值。
        ''' </summary>
        Private Function BuildEntryStack(b As BasicBlock) As List(Of Expression)
            Dim result As New List(Of Expression)()
            Dim reference As List(Of Expression) = Nothing

            For Each p As Integer In b.Predecessors
                If Not _exitStacks.ContainsKey(p) Then
                    ' 回边：前驱尚未处理。若它将来会留下栈值，这里无法正确合并。
                    Continue For
                End If

                Dim es = _exitStacks(p)

                If reference Is Nothing Then
                    reference = es
                ElseIf reference.Count <> es.Count Then
                    Throw New DecompileException(
                        $"块 bb{b.Id} 的入口栈深度不一致（参考 {reference.Count}，前驱 bb{p} 为 {es.Count}），无法做栈模拟")
                End If
            Next

            If reference Is Nothing OrElse reference.Count = 0 Then Return result

            For i As Integer = 0 To reference.Count - 1
                Dim value As Expression = Nothing
                Dim agree As Boolean = True

                For Each p As Integer In b.Predecessors
                    If Not _exitStacks.ContainsKey(p) Then Continue For

                    Dim v = _exitStacks(p)(i)

                    If value Is Nothing Then
                        value = v
                    ElseIf Not ReferenceEquals(value, v) Then
                        agree = False
                    End If
                Next

                If agree Then
                    result.Add(value)
                Else
                    result.Add(CreateStackMerge(b, i))
                End If
            Next

            Return result
        End Function

        Private Function CreateStackMerge(b As BasicBlock, slotIndex As Integer) As Expression
            Dim name = "sm_" & b.Id & "_" & slotIndex
            Dim mergeType As System.Type = Nothing

            For Each p As Integer In b.Predecessors
                If Not _exitStacks.ContainsKey(p) Then Continue For

                Dim pred = _cfg.Blocks(p)

                If pred.Successors.Count > 1 Then
                    Throw New DecompileException(
                        $"块 bb{pred.Id} 有多个后继却需要在边上插入栈值合并赋值，该控制流形状暂不支持")
                End If

                Dim v = _exitStacks(p)(slotIndex)

                If mergeType Is Nothing AndAlso v IsNot Nothing Then mergeType = v.Type

                Dim target As New LocalExpression(-1, name, If(v Is Nothing, Nothing, v.Type))
                pred.Statements.Add(New AssignmentStatement(target, v))
            Next

            MergeDeclarations.Add(New VariableDeclarationStatement(name, mergeType))

            Return New LocalExpression(-1, name, mergeType)
        End Function

        ' ==================================================================
        ' phi 消解
        ' ==================================================================

        Private Sub AppendPhiAssignments(b As BasicBlock)
            For Each succ As Integer In b.Successors
                If Not _ssa.Phis.ContainsKey(succ) Then Continue For

                If b.Successors.Count > 1 Then
                    Throw New DecompileException(
                        $"块 bb{b.Id} 有多个后继却需要为 bb{succ} 插入 phi 赋值，该控制流形状暂不支持")
                End If

                For Each phi As SsaBuilder.PhiInfo In _ssa.Phis(succ)
                    Dim argName As String = Nothing

                    If Not phi.Args.TryGetValue(b.Id, argName) Then Continue For
                    If argName = phi.Name Then Continue For

                    b.Statements.Add(New AssignmentStatement(
                        New LocalExpression(-1, phi.Name, phi.VarType),
                        New LocalExpression(-1, argName, phi.VarType)))
                Next
            Next
        End Sub

        ' ==================================================================
        ' 单条指令的归约
        ' ==================================================================

        Private Sub Reduce(b As BasicBlock, ins As ILInstruction, stack As List(Of Expression))
            Dim code = ins.Code

            ' ---- 常量 ----
            Dim constValue As Object = Nothing
            Dim constType As System.Type = Nothing

            If IlOpcodeInfo.IsLoadConstant(ins, constValue, constType) Then
                stack.Add(New LiteralExpression(constValue, constType))
                Return
            End If

            ' ---- 局部变量 ----
            Dim index As Integer = -1

            If IlOpcodeInfo.IsLoadLocal(ins, index) Then
                Dim key = SsaBuilder.LocalKey(index)
                Dim name = NameOfUse(ins)
                Dim localType = _ssa.TypeOfSlot(key)

                stack.Add(New LocalExpression(index, name, localType))
                Return
            End If

            If IlOpcodeInfo.IsStoreLocal(ins, index) Then
                Dim value = Pop(stack, ins)
                Dim key = SsaBuilder.LocalKey(index)
                Dim name = NameOfDef(ins)

                ' SSA 保证每个名字只被定义一次，因此"声明 + 初值"永远不会重复声明
                b.Statements.Add(New VariableDeclarationStatement(name, _ssa.TypeOfSlot(key), value, index))
                Return
            End If

            ' ---- 形参 ----
            If IlOpcodeInfo.IsLoadArg(ins, index) Then
                Dim key = SsaBuilder.ArgKey(index)

                If Not _method.IsStatic AndAlso index = 0 Then
                    Throw New DecompileException(
                        $"IL_{ins.Offset.ToString("X4")}: 实例方法的 this 引用无法转译为 CUDA 内核，请把目标方法声明为 Shared")
                End If

                stack.Add(New ParameterExpression(index, NameOfUse(ins), _ssa.TypeOfSlot(key)))
                Return
            End If

            If IlOpcodeInfo.IsStoreArg(ins, index) Then
                Throw New DecompileException(
                    $"IL_{ins.Offset.ToString("X4")}: 暂不支持对形参重新赋值（starg），请改用局部变量")
            End If

            ' ---- 栈操作 ----
            If code = OpCodes.Dup Then
                If stack.Count = 0 Then Throw StackUnderflow(ins)
                stack.Add(stack(stack.Count - 1))
                Return
            End If

            If code = OpCodes.Pop Then
                Pop(stack, ins)
                Return
            End If

            If IlOpcodeInfo.IsSkip(code) Then Return

            ' ---- 一元运算 ----
            Dim unary As UnaryOperator = Nothing

            If IlOpcodeInfo.TryGetUnaryOperator(code, unary) Then
                Dim operand = Pop(stack, ins)
                stack.Add(New UnaryExpression(unary, operand, operand.Type))
                Return
            End If

            ' ---- 数值转换 ----
            Dim convType As System.Type = Nothing
            Dim isChecked As Boolean = False

            If IlOpcodeInfo.TryGetConvertType(code, convType, isChecked) Then
                Dim operand = Pop(stack, ins)
                stack.Add(New ConvertExpression(operand, convType, isChecked))
                Return
            End If

            ' ---- 二元运算 / 比较 ----
            Dim binary As BinaryOperator = Nothing

            If IlOpcodeInfo.TryGetBinaryOperator(code, binary) Then
                Dim right = Pop(stack, ins)
                Dim left = Pop(stack, ins)
                Dim resultType = If(IlOpcodeInfo.IsComparison(code),
                                    GetType(Boolean),
                                    WiderType(left.Type, right.Type))

                stack.Add(New BinaryExpression(binary, left, right, resultType))
                Return
            End If

            ' ---- 数组 ----
            Dim elementType As System.Type = Nothing

            If IlOpcodeInfo.IsLoadElement(code, elementType) Then
                Dim arrayIndex = Pop(stack, ins)
                Dim array = Pop(stack, ins)

                If elementType Is Nothing AndAlso array.Type IsNot Nothing Then
                    elementType = array.Type.GetElementType()
                End If

                stack.Add(New ArrayIndexExpression(array, arrayIndex, elementType))
                Return
            End If

            If code = OpCodes.Ldlen Then
                Dim array = Pop(stack, ins)
                stack.Add(New ArrayLengthExpression(array))
                Return
            End If

            If IlOpcodeInfo.IsStoreElement(code) Then
                Throw New DecompileException(
                    $"IL_{ins.Offset.ToString("X4")}: 暂不支持数组写入（{code.Name}）")
            End If

            ' ---- 调用 ----
            If code = OpCodes.Call OrElse code = OpCodes.Callvirt Then
                ReduceCall(b, ins, stack, code)
                Return
            End If

            ' ---- 控制流 ----
            If ins.IsConditionalBranch Then
                ReduceConditionalBranch(b, ins, stack)
                Return
            End If

            If ins.IsUnconditionalBranch Then
                ' 纯跳转，不产生语句
                Return
            End If

            If ins.IsSwitch Then
                Throw New DecompileException(
                    $"IL_{ins.Offset.ToString("X4")}: 暂不支持 switch 结构")
            End If

            If ins.IsReturn Then
                Dim value As Expression = Nothing

                If stack.Count > 0 Then value = Pop(stack, ins)

                If stack.Count > 0 Then
                    _diagnostics.Warn(ins.Offset, $"ret 处求值栈仍有 {stack.Count} 个残留项，已忽略")
                End If

                b.Statements.Add(New ReturnStatement(value))
                Return
            End If

            Throw New DecompileException(
                $"IL_{ins.Offset.ToString("X4")}: 不支持的指令 {code.Name}")
        End Sub

        Private Sub ReduceCall(b As BasicBlock, ins As ILInstruction,
                               stack As List(Of Expression), code As OpCode)
            Dim target = TryCast(ins.Operand, MethodInfo)

            If target Is Nothing Then
                Throw New DecompileException(
                    $"IL_{ins.Offset.ToString("X4")}: 无法解析被调用的方法")
            End If

            Dim parameters = target.GetParameters()
            Dim popped As New List(Of Expression)()

            For i As Integer = 0 To parameters.Length - 1
                popped.Add(Pop(stack, ins))
            Next

            popped.Reverse()

            Dim callNode As New CallExpression With {
                .Method = target,
                .MethodName = target.Name,
                .DeclaringTypeName = If(target.DeclaringType Is Nothing, Nothing, target.DeclaringType.FullName),
                .IsStaticCall = target.IsStatic,
                .Type = If(target.ReturnType Is GetType(Void), Nothing, target.ReturnType)
            }

            callNode.Arguments.AddRange(popped)

            If code = OpCodes.Callvirt OrElse (code = OpCodes.Call AndAlso Not target.IsStatic) Then
                callNode.Instance = Pop(stack, ins)
            End If

            If target.ReturnType Is GetType(Void) Then
                b.Statements.Add(New ExpressionStatement(callNode))
            Else
                stack.Add(callNode)
            End If
        End Sub

        ''' <summary>
        ''' 把条件分支归一化成"条件成立去 true 后继"：
        ''' brtrue 原样；brfalse 取反；beq/bne/bgt/bge/blt/ble 弹两个操作数构造比较。
        ''' </summary>
        Private Sub ReduceConditionalBranch(b As BasicBlock, ins As ILInstruction, stack As List(Of Expression))
            Dim code = ins.Code
            Dim condition As Expression = Nothing

            If code = OpCodes.Brtrue OrElse code = OpCodes.Brtrue_S Then
                condition = Pop(stack, ins)

            ElseIf code = OpCodes.Brfalse OrElse code = OpCodes.Brfalse_S Then
                Dim operand = Pop(stack, ins)
                condition = New UnaryExpression(UnaryOperator.LogicalNot, operand, GetType(Boolean))

            Else
                Dim binary As BinaryOperator = Nothing

                If Not IlOpcodeInfo.TryGetBinaryOperator(code, binary) OrElse
                   Not IlOpcodeInfo.IsComparison(code) Then
                    Throw New DecompileException(
                        $"IL_{ins.Offset.ToString("X4")}: 不支持的条件分支 {code.Name}")
                End If

                Dim right = Pop(stack, ins)
                Dim left = Pop(stack, ins)

                condition = New BinaryExpression(binary, left, right, GetType(Boolean))
            End If

            b.Condition = condition
        End Sub

        ' ==================================================================
        ' 小工具
        ' ==================================================================

        Private Function NameOfUse(ins As ILInstruction) As String
            Dim name As String = Nothing
            If _ssa.UseNames.TryGetValue(ins.Offset, name) Then Return name
            Return "undef_" & ins.Offset.ToString("X4")
        End Function

        Private Function NameOfDef(ins As ILInstruction) As String
            Dim name As String = Nothing
            If _ssa.DefNames.TryGetValue(ins.Offset, name) Then Return name
            Return "undef_" & ins.Offset.ToString("X4")
        End Function

        Private Function Pop(stack As List(Of Expression), ins As ILInstruction) As Expression
            If stack.Count = 0 Then Throw StackUnderflow(ins)

            Dim top = stack(stack.Count - 1)
            stack.RemoveAt(stack.Count - 1)

            Return top
        End Function

        Private Shared Function StackUnderflow(ins As ILInstruction) As DecompileException
            Return New DecompileException($"IL_{ins.Offset.ToString("X4")}: 求值栈下溢（{ins.Code.Name}）")
        End Function

        ''' <summary>二元运算的结果类型：取两侧中"更宽"的那个</summary>
        Friend Shared Function WiderType(a As System.Type, b As System.Type) As System.Type
            If a Is Nothing Then Return b
            If b Is Nothing Then Return a
            If a = b Then Return a

            Dim rankA = NumericRank(a)
            Dim rankB = NumericRank(b)

            Return If(rankA >= rankB, a, b)
        End Function

        Private Shared Function NumericRank(t As System.Type) As Integer
            If t Is GetType(Double) Then Return 6
            If t Is GetType(Single) Then Return 5
            If t Is GetType(Long) OrElse t Is GetType(ULong) Then Return 4
            If t Is GetType(Integer) OrElse t Is GetType(UInteger) Then Return 3
            If t Is GetType(Short) OrElse t Is GetType(UShort) Then Return 2
            If t Is GetType(Byte) OrElse t Is GetType(SByte) Then Return 1
            If t Is GetType(Boolean) Then Return 0

            Return 0
        End Function
    End Class
End Namespace
