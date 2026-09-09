#Region "Microsoft.VisualBasic::2ea94b6413cc2c55af3ae32e8477bda0, vs_solutions\dev\VisualStudio\IL\Decompiler\IlOpcodeInfo.vb"

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

    '   Total Lines: 298
    '    Code Lines: 238 (79.87%)
    ' Comment Lines: 23 (7.72%)
    '    - Xml Docs: 56.52%
    ' 
    '   Blank Lines: 37 (12.42%)
    '     File Size: 12.75 KB


    '     Module IlOpcodeInfo
    ' 
    '         Function: IsComparison, IsLoadArg, IsLoadConstant, IsLoadElement, IsLoadLocal
    '                   IsSkip, IsStoreArg, IsStoreElement, IsStoreLocal, TryGetBinaryOperator
    '                   TryGetConvertType, TryGetUnaryOperator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' IL opcode 归类助手
'
' 把"这条指令是不是 ldloc / starg / ldc.r4 / add / conv.r4 ..."这类判定集中到一处，
' 供 SSA 重命名（找定义与引用）与栈模拟（归约表达式）共用，避免两处各写一套
' 而逐渐不一致。
'
' 这里统一用 OpCode.Name（"ldloc.0" / "conv.r4" 这样的助记符）做判定：
' 比逐个比较 OpCodes 常量更不容易写错，也不会因为漏掉某个带 _S 后缀的短格式而出错。
' ---------------------------------------------------------------------------

Namespace IL

    ''' <summary>IL opcode 归类的静态助手（全部为纯函数）</summary>
    Public Module IlOpcodeInfo

        ''' <summary>是否读取局部变量；是则给出下标</summary>
        Public Function IsLoadLocal(ins As ILInstruction, ByRef index As Integer) As Boolean
            index = -1

            If ins Is Nothing Then Return False

            Select Case ins.Code.Name
                Case "ldloc.0" : index = 0
                Case "ldloc.1" : index = 1
                Case "ldloc.2" : index = 2
                Case "ldloc.3" : index = 3
                Case "ldloc.s", "ldloc"
                    If TypeOf ins.Operand Is Integer Then
                        index = CInt(ins.Operand)
                    Else
                        Return False
                    End If
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>是否写入局部变量；是则给出下标</summary>
        Public Function IsStoreLocal(ins As ILInstruction, ByRef index As Integer) As Boolean
            index = -1

            If ins Is Nothing Then Return False

            Select Case ins.Code.Name
                Case "stloc.0" : index = 0
                Case "stloc.1" : index = 1
                Case "stloc.2" : index = 2
                Case "stloc.3" : index = 3
                Case "stloc.s", "stloc"
                    If TypeOf ins.Operand Is Integer Then
                        index = CInt(ins.Operand)
                    Else
                        Return False
                    End If
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>是否读取形参；是则给出下标</summary>
        Public Function IsLoadArg(ins As ILInstruction, ByRef index As Integer) As Boolean
            index = -1

            If ins Is Nothing Then Return False

            Select Case ins.Code.Name
                Case "ldarg.0" : index = 0
                Case "ldarg.1" : index = 1
                Case "ldarg.2" : index = 2
                Case "ldarg.3" : index = 3
                Case "ldarg.s", "ldarg"
                    If TypeOf ins.Operand Is Integer Then
                        index = CInt(ins.Operand)
                    Else
                        Return False
                    End If
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>是否写入形参；是则给出下标</summary>
        Public Function IsStoreArg(ins As ILInstruction, ByRef index As Integer) As Boolean
            index = -1

            If ins Is Nothing Then Return False

            Select Case ins.Code.Name
                Case "starg.s", "starg"
                    If TypeOf ins.Operand Is Integer Then
                        index = CInt(ins.Operand)
                    Else
                        Return False
                    End If
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>是否加载字面量常量</summary>
        Public Function IsLoadConstant(ins As ILInstruction, ByRef value As Object,
                                       ByRef valueType As System.Type) As Boolean
            value = Nothing
            valueType = Nothing

            If ins Is Nothing Then Return False

            Select Case ins.Code.Name
                Case "ldc.i4.m1"
                    value = -1 : valueType = GetType(Integer)
                Case "ldc.i4.0"
                    value = 0 : valueType = GetType(Integer)
                Case "ldc.i4.1"
                    value = 1 : valueType = GetType(Integer)
                Case "ldc.i4.2"
                    value = 2 : valueType = GetType(Integer)
                Case "ldc.i4.3"
                    value = 3 : valueType = GetType(Integer)
                Case "ldc.i4.4"
                    value = 4 : valueType = GetType(Integer)
                Case "ldc.i4.5"
                    value = 5 : valueType = GetType(Integer)
                Case "ldc.i4.6"
                    value = 6 : valueType = GetType(Integer)
                Case "ldc.i4.7"
                    value = 7 : valueType = GetType(Integer)
                Case "ldc.i4.8"
                    value = 8 : valueType = GetType(Integer)
                Case "ldc.i4.s", "ldc.i4"
                    If Not TypeOf ins.Operand Is Integer Then Return False
                    value = CInt(ins.Operand) : valueType = GetType(Integer)
                Case "ldc.i8"
                    value = CLng(ins.Operand) : valueType = GetType(Long)
                Case "ldc.r4"
                    value = CSng(ins.Operand) : valueType = GetType(Single)
                Case "ldc.r8"
                    value = CDbl(ins.Operand) : valueType = GetType(Double)
                Case "ldnull"
                    value = Nothing : valueType = Nothing
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>是否二元运算；是则给出运算符（结果类型由调用方按操作数推导）</summary>
        Public Function TryGetBinaryOperator(name As String, ByRef op As BinaryOperator) As Boolean
            Select Case name
                Case "add", "add.ovf", "add.ovf.un"
                    op = BinaryOperator.Add
                Case "sub", "sub.ovf", "sub.ovf.un"
                    op = BinaryOperator.Subtract
                Case "mul", "mul.ovf", "mul.ovf.un"
                    op = BinaryOperator.Multiply
                Case "div", "div.un"
                    op = BinaryOperator.Divide
                Case "rem", "rem.un"
                    op = BinaryOperator.Modulo
                Case "and"
                    op = BinaryOperator.BitwiseAnd
                Case "or"
                    op = BinaryOperator.BitwiseOr
                Case "xor"
                    op = BinaryOperator.BitwiseXor
                Case "shl"
                    op = BinaryOperator.ShiftLeft
                Case "shr", "shr.un"
                    op = BinaryOperator.ShiftRight
                Case "ceq", "beq", "beq.s"
                    op = BinaryOperator.Equal
                Case "bne.un", "bne.un.s"
                    op = BinaryOperator.NotEqual
                Case "cgt", "cgt.un", "bgt", "bgt.s", "bgt.un", "bgt.un.s"
                    op = BinaryOperator.GreaterThan
                Case "bge", "bge.s", "bge.un", "bge.un.s"
                    op = BinaryOperator.GreaterThanOrEqual
                Case "clt", "clt.un", "blt", "blt.s", "blt.un", "blt.un.s"
                    op = BinaryOperator.LessThan
                Case "ble", "ble.s", "ble.un", "ble.un.s"
                    op = BinaryOperator.LessThanOrEqual
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>该二元指令是否为"比较"类（结果类型为 Boolean）</summary>
        Public Function IsComparison(name As String) As Boolean
            Dim op As BinaryOperator = Nothing

            If Not TryGetBinaryOperator(name, op) Then Return False

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
        Public Function TryGetUnaryOperator(name As String, ByRef op As UnaryOperator) As Boolean
            Select Case name
                Case "neg"
                    op = UnaryOperator.Negate
                Case "not"
                    op = UnaryOperator.LogicalNot
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>是否数值转换指令</summary>
        Public Function TryGetConvertType(name As String, ByRef targetType As System.Type,
                                          ByRef isChecked As Boolean) As Boolean
            targetType = Nothing
            isChecked = False

            Select Case name
                Case "conv.i1" : targetType = GetType(SByte)
                Case "conv.i2" : targetType = GetType(Short)
                Case "conv.i4" : targetType = GetType(Integer)
                Case "conv.i8" : targetType = GetType(Long)
                Case "conv.r4" : targetType = GetType(Single)
                Case "conv.r8" : targetType = GetType(Double)
                Case "conv.u1" : targetType = GetType(Byte)
                Case "conv.u2" : targetType = GetType(UShort)
                Case "conv.u4" : targetType = GetType(UInteger)
                Case "conv.u8" : targetType = GetType(ULong)
                Case "conv.ovf.i1" : targetType = GetType(SByte) : isChecked = True
                Case "conv.ovf.i2" : targetType = GetType(Short) : isChecked = True
                Case "conv.ovf.i4" : targetType = GetType(Integer) : isChecked = True
                Case "conv.ovf.i8" : targetType = GetType(Long) : isChecked = True
                Case "conv.ovf.u1" : targetType = GetType(Byte) : isChecked = True
                Case "conv.ovf.u2" : targetType = GetType(UShort) : isChecked = True
                Case "conv.ovf.u4" : targetType = GetType(UInteger) : isChecked = True
                Case "conv.ovf.u8" : targetType = GetType(ULong) : isChecked = True
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>是否数组取元素（ldelem.*）</summary>
        Public Function IsLoadElement(name As String, ByRef elementType As System.Type) As Boolean
            elementType = Nothing

            Select Case name
                Case "ldelem.r4" : elementType = GetType(Single)
                Case "ldelem.r8" : elementType = GetType(Double)
                Case "ldelem.i4" : elementType = GetType(Integer)
                Case "ldelem.i8" : elementType = GetType(Long)
                Case "ldelem.i1" : elementType = GetType(SByte)
                Case "ldelem.i2" : elementType = GetType(Short)
                Case "ldelem.u1" : elementType = GetType(Byte)
                Case "ldelem.u2" : elementType = GetType(UShort)
                Case "ldelem.u4" : elementType = GetType(UInteger)
                Case "ldelem.i" : elementType = GetType(IntPtr)
                Case "ldelem.ref" : elementType = GetType(Object)
                Case "ldelem" : elementType = Nothing
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>是否数组写元素（stelem.*）</summary>
        Public Function IsStoreElement(name As String) As Boolean
            Return name = "stelem.r4" OrElse name = "stelem.r8" OrElse
                   name = "stelem.i4" OrElse name = "stelem.i8" OrElse
                   name = "stelem.i1" OrElse name = "stelem.i2" OrElse
                   name = "stelem.i" OrElse name = "stelem.ref" OrElse
                   name = "stelem"
        End Function

        ''' <summary>是否为可直接忽略的指令</summary>
        Public Function IsSkip(name As String) As Boolean
            Return name = "nop"
        End Function
    End Module
End Namespace

