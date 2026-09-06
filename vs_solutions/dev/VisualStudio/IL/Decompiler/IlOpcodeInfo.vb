' ---------------------------------------------------------------------------
' IL opcode 归类助手
'
' 把"这条指令是不是 ldloc / starg / ldc.r4 / add / conv.r4 ..."这类判定集中到一处，
' 供 SSA 重命名（找定义与引用）与栈模拟（归约表达式）共用，避免两处各写一套
' 而逐渐不一致。
' ---------------------------------------------------------------------------

Imports System.Reflection.Emit

Namespace IL

    ''' <summary>IL opcode 归类的静态助手（全部为纯函数）</summary>
    Public Module IlOpcodeInfo

        ''' <summary>是否读取局部变量；是则给出下标</summary>
        Public Function IsLoadLocal(ins As ILInstruction, ByRef index As Integer) As Boolean
            index = -1

            If ins Is Nothing Then Return False

            Dim c = ins.Code

            If c = OpCodes.Ldloc_0 Then index = 0
            ElseIf c = OpCodes.Ldloc_1 Then index = 1
            ElseIf c = OpCodes.Ldloc_2 Then index = 2
            ElseIf c = OpCodes.Ldloc_3 Then index = 3
            ElseIf c = OpCodes.Ldloc_S OrElse c = OpCodes.Ldloc Then
                If TypeOf ins.Operand Is Integer Then index = CInt(ins.Operand) Else Return False
            Else
                Return False
            End If

            Return True
        End Function

        ''' <summary>是否写入局部变量；是则给出下标</summary>
        Public Function IsStoreLocal(ins As ILInstruction, ByRef index As Integer) As Boolean
            index = -1

            If ins Is Nothing Then Return False

            Dim c = ins.Code

            If c = OpCodes.Stloc_0 Then index = 0
            ElseIf c = OpCodes.Stloc_1 Then index = 1
            ElseIf c = OpCodes.Stloc_2 Then index = 2
            ElseIf c = OpCodes.Stloc_3 Then index = 3
            ElseIf c = OpCodes.Stloc_S OrElse c = OpCodes.Stloc Then
                If TypeOf ins.Operand Is Integer Then index = CInt(ins.Operand) Else Return False
            Else
                Return False
            End If

            Return True
        End Function

        ''' <summary>是否读取形参；是则给出下标</summary>
        Public Function IsLoadArg(ins As ILInstruction, ByRef index As Integer) As Boolean
            index = -1

            If ins Is Nothing Then Return False

            Dim c = ins.Code

            If c = OpCodes.Ldarg_0 Then index = 0
            ElseIf c = OpCodes.Ldarg_1 Then index = 1
            ElseIf c = OpCodes.Ldarg_2 Then index = 2
            ElseIf c = OpCodes.Ldarg_3 Then index = 3
            ElseIf c = OpCodes.Ldarg_S OrElse c = OpCodes.Ldarg Then
                If TypeOf ins.Operand Is Integer Then index = CInt(ins.Operand) Else Return False
            Else
                Return False
            End If

            Return True
        End Function

        ''' <summary>是否写入形参；是则给出下标</summary>
        Public Function IsStoreArg(ins As ILInstruction, ByRef index As Integer) As Boolean
            index = -1

            If ins Is Nothing Then Return False

            Dim c = ins.Code

            If c = OpCodes.Starg_S OrElse c = OpCodes.Starg Then
                If TypeOf ins.Operand Is Integer Then index = CInt(ins.Operand) Else Return False
            Else
                Return False
            End If

            Return True
        End Function

        ''' <summary>是否加载字面量常量</summary>
        Public Function IsLoadConstant(ins As ILInstruction, ByRef value As Object,
                                       ByRef valueType As System.Type) As Boolean
            value = Nothing
            valueType = Nothing

            If ins Is Nothing Then Return False

            Dim c = ins.Code

            If c = OpCodes.Ldc_I4_M1 Then
                value = -1
                valueType = GetType(Integer)

            ElseIf c = OpCodes.Ldc_I4_0 Then
                value = 0 : valueType = GetType(Integer)
            ElseIf c = OpCodes.Ldc_I4_1 Then
                value = 1 : valueType = GetType(Integer)
            ElseIf c = OpCodes.Ldc_I4_2 Then
                value = 2 : valueType = GetType(Integer)
            ElseIf c = OpCodes.Ldc_I4_3 Then
                value = 3 : valueType = GetType(Integer)
            ElseIf c = OpCodes.Ldc_I4_4 Then
                value = 4 : valueType = GetType(Integer)
            ElseIf c = OpCodes.Ldc_I4_5 Then
                value = 5 : valueType = GetType(Integer)
            ElseIf c = OpCodes.Ldc_I4_6 Then
                value = 6 : valueType = GetType(Integer)
            ElseIf c = OpCodes.Ldc_I4_7 Then
                value = 7 : valueType = GetType(Integer)
            ElseIf c = OpCodes.Ldc_I4_8 Then
                value = 8 : valueType = GetType(Integer)

            ElseIf c = OpCodes.Ldc_I4_S OrElse c = OpCodes.Ldc_I4 Then
                If Not TypeOf ins.Operand Is Integer Then Return False
                value = CInt(ins.Operand) : valueType = GetType(Integer)

            ElseIf c = OpCodes.Ldc_I8 Then
                value = CLng(ins.Operand) : valueType = GetType(Long)
            ElseIf c = OpCodes.Ldc_R4 Then
                value = CSng(ins.Operand) : valueType = GetType(Single)
            ElseIf c = OpCodes.Ldc_R8 Then
                value = CDbl(ins.Operand) : valueType = GetType(Double)
            ElseIf c = OpCodes.Ldnull Then
                value = Nothing : valueType = Nothing
            Else
                Return False
            End If

            Return True
        End Function

        ''' <summary>是否二元运算；是则给出运算符（结果类型由调用方按操作数推导）</summary>
        Public Function TryGetBinaryOperator(code As OpCode, ByRef op As BinaryOperator) As Boolean
            If code = OpCodes.Add OrElse code = OpCodes.Add_Ovf OrElse code = OpCodes.Add_Ovf_Un Then
                op = BinaryOperator.Add
            ElseIf code = OpCodes.Sub OrElse code = OpCodes.Sub_Ovf OrElse code = OpCodes.Sub_Ovf_Un Then
                op = BinaryOperator.Subtract
            ElseIf code = OpCodes.Mul OrElse code = OpCodes.Mul_Ovf OrElse code = OpCodes.Mul_Ovf_Un Then
                op = BinaryOperator.Multiply
            ElseIf code = OpCodes.Div OrElse code = OpCodes.Div_Un Then
                op = BinaryOperator.Divide
            ElseIf code = OpCodes.Rem OrElse code = OpCodes.Rem_Un Then
                op = BinaryOperator.Modulo
            ElseIf code = OpCodes.And Then
                op = BinaryOperator.BitwiseAnd
            ElseIf code = OpCodes.Or Then
                op = BinaryOperator.BitwiseOr
            ElseIf code = OpCodes.Xor Then
                op = BinaryOperator.BitwiseXor
            ElseIf code = OpCodes.Shl Then
                op = BinaryOperator.ShiftLeft
            ElseIf code = OpCodes.Shr OrElse code = OpCodes.Shr_Un Then
                op = BinaryOperator.ShiftRight
            ElseIf code = OpCodes.Ceq Then
                op = BinaryOperator.Equal
            ElseIf code = OpCodes.Cgt OrElse code = OpCodes.Cgt_Un Then
                op = BinaryOperator.GreaterThan
            ElseIf code = OpCodes.Clt OrElse code = OpCodes.Clt_Un Then
                op = BinaryOperator.LessThan
            ElseIf code = OpCodes.Beq OrElse code = OpCodes.Beq_S Then
                op = BinaryOperator.Equal
            ElseIf code = OpCodes.Bne_Un OrElse code = OpCodes.Bne_Un_S Then
                op = BinaryOperator.NotEqual
            ElseIf code = OpCodes.Bgt OrElse code = OpCodes.Bgt_S OrElse
                   code = OpCodes.Bgt_Un OrElse code = OpCodes.Bgt_Un_S Then
                op = BinaryOperator.GreaterThan
            ElseIf code = OpCodes.Bge OrElse code = OpCodes.Bge_S OrElse
                   code = OpCodes.Bge_Un OrElse code = OpCodes.Bge_Un_S Then
                op = BinaryOperator.GreaterThanOrEqual
            ElseIf code = OpCodes.Blt OrElse code = OpCodes.Blt_S OrElse
                   code = OpCodes.Blt_Un OrElse code = OpCodes.Blt_Un_S Then
                op = BinaryOperator.LessThan
            ElseIf code = OpCodes.Ble OrElse code = OpCodes.Ble_S OrElse
                   code = OpCodes.Ble_Un OrElse code = OpCodes.Ble_Un_S Then
                op = BinaryOperator.LessThanOrEqual
            Else
                Return False
            End If

            Return True
        End Function

        ''' <summary>该二元指令是否为"比较"类（结果类型为 Boolean）</summary>
        Public Function IsComparison(code As OpCode) As Boolean
            Dim op As BinaryOperator = Nothing
            If Not TryGetBinaryOperator(code, op) Then Return False

            Select Case op
                Case BinaryOperator.Equal, BinaryOperator.NotEqual,
                     BinaryOperator.LessThan, BinaryOperator.LessThanOrEqual,
                     BinaryOperator.GreaterThan, BinaryOperator.GreaterThanOrEqual
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>是否一元运算</summary>
        Public Function TryGetUnaryOperator(code As OpCode, ByRef op As UnaryOperator) As Boolean
            If code = OpCodes.Neg Then
                op = UnaryOperator.Negate
            ElseIf code = OpCodes.Not Then
                op = UnaryOperator.LogicalNot
            Else
                Return False
            End If

            Return True
        End Function

        ''' <summary>是否数值转换指令</summary>
        Public Function TryGetConvertType(code As OpCode, ByRef targetType As System.Type,
                                          ByRef isChecked As Boolean) As Boolean
            targetType = Nothing
            isChecked = False

            If code = OpCodes.Conv_I1 Then targetType = GetType(SByte)
            ElseIf code = OpCodes.Conv_I2 Then targetType = GetType(Short)
            ElseIf code = OpCodes.Conv_I4 Then targetType = GetType(Integer)
            ElseIf code = OpCodes.Conv_I8 Then targetType = GetType(Long)
            ElseIf code = OpCodes.Conv_R4 Then targetType = GetType(Single)
            ElseIf code = OpCodes.Conv_R8 Then targetType = GetType(Double)
            ElseIf code = OpCodes.Conv_U1 Then targetType = GetType(Byte)
            ElseIf code = OpCodes.Conv_U2 Then targetType = GetType(UShort)
            ElseIf code = OpCodes.Conv_U4 Then targetType = GetType(UInteger)
            ElseIf code = OpCodes.Conv_U8 Then targetType = GetType(ULong)
            ElseIf code = OpCodes.Conv_Ovf_I1 Then targetType = GetType(SByte) : isChecked = True
            ElseIf code = OpCodes.Conv_Ovf_I2 Then targetType = GetType(Short) : isChecked = True
            ElseIf code = OpCodes.Conv_Ovf_I4 Then targetType = GetType(Integer) : isChecked = True
            ElseIf code = OpCodes.Conv_Ovf_I8 Then targetType = GetType(Long) : isChecked = True
            ElseIf code = OpCodes.Conv_Ovf_U1 Then targetType = GetType(Byte) : isChecked = True
            ElseIf code = OpCodes.Conv_Ovf_U2 Then targetType = GetType(UShort) : isChecked = True
            ElseIf code = OpCodes.Conv_Ovf_U4 Then targetType = GetType(UInteger) : isChecked = True
            ElseIf code = OpCodes.Conv_Ovf_U8 Then targetType = GetType(ULong) : isChecked = True
            Else
                Return False
            End If

            Return True
        End Function

        ''' <summary>是否数组取元素（ldelem.*）</summary>
        Public Function IsLoadElement(code As OpCode, ByRef elementType As System.Type) As Boolean
            elementType = Nothing

            If code = OpCodes.Ldelem_R4 Then elementType = GetType(Single)
            ElseIf code = OpCodes.Ldelem_R8 Then elementType = GetType(Double)
            ElseIf code = OpCodes.Ldelem_I4 Then elementType = GetType(Integer)
            ElseIf code = OpCodes.Ldelem_I8 Then elementType = GetType(Long)
            ElseIf code = OpCodes.Ldelem_U1 Then elementType = GetType(Byte)
            ElseIf code = OpCodes.Ldelem_U2 Then elementType = GetType(UShort)
            ElseIf code = OpCodes.Ldelem_U4 Then elementType = GetType(UInteger)
            ElseIf code = OpCodes.Ldelem_I1 Then elementType = GetType(SByte)
            ElseIf code = OpCodes.Ldelem_I2 Then elementType = GetType(Short)
            ElseIf code = OpCodes.Ldelem_I Then elementType = GetType(IntPtr)
            ElseIf code = OpCodes.Ldelem_Ref Then elementType = GetType(Object)
            ElseIf code = OpCodes.Ldelem Then elementType = Nothing
            Else
                Return False
            End If

            Return True
        End Function

        ''' <summary>是否数组写元素（stelem.*）</summary>
        Public Function IsStoreElement(code As OpCode) As Boolean
            Return code = OpCodes.Stelem_R4 OrElse code = OpCodes.Stelem_R8 OrElse
                   code = OpCodes.Stelem_I4 OrElse code = OpCodes.Stelem_I8 OrElse
                   code = OpCodes.Stelem_I1 OrElse code = OpCodes.Stelem_I2 OrElse
                   code = OpCodes.Stelem_I OrElse code = OpCodes.Stelem_Ref OrElse
                   code = OpCodes.Stelem
        End Function

        ''' <summary>是否为可直接忽略的指令</summary>
        Public Function IsSkip(code As OpCode) As Boolean
            Return code = OpCodes.Nop
        End Function
    End Module
End Namespace
