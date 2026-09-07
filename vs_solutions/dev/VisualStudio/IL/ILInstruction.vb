#Region "Microsoft.VisualBasic::b00440d4f7706e4bc0f20dd433ca5f2f, vs_solutions\dev\VisualStudio\IL\ILInstruction.vb"

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

    '   Total Lines: 116
    '    Code Lines: 81 (69.83%)
    ' Comment Lines: 12 (10.34%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 23 (19.83%)
    '     File Size: 4.12 KB


    '     Class ILInstruction
    ' 
    '         Properties: Code, Offset, Operand, OperandData
    ' 
    '         Function: buildInlineMethodCode, GetCode, GetExpandedOffset, GetOperandCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Reflection.Emit

Namespace IL

    Public Class ILInstruction

        Public Property Code As OpCode
        Public Property Operand As Object
        Public Property OperandData As Byte()
        Public Property Offset As Integer

        ''' <summary>
        ''' 该指令的总字节长度（操作码 + 操作数）。由 <see cref="MethodBodyReader"/> 在解析时回填。
        ''' </summary>
        Public Property Size As Integer

        ''' <summary>下一条指令的 IL 偏移（顺序执行时的落点）</summary>
        Public ReadOnly Property NextOffset As Integer
            Get
                Return Offset + Size
            End Get
        End Property

        ''' <summary>无条件跳转（br / br.s / leave / leave.s）</summary>
        Public ReadOnly Property IsUnconditionalBranch As Boolean
            Get
                Return Code.FlowControl = FlowControl.Branch OrElse Code.FlowControl = FlowControl.Throw
            End Get
        End Property

        ''' <summary>条件跳转（brtrue / brfalse / 各类 beq bne bgt blt ...）</summary>
        Public ReadOnly Property IsConditionalBranch As Boolean
            Get
                Return Code.FlowControl = FlowControl.Cond_Branch AndAlso
                       Code.OperandType <> OperandType.InlineSwitch
            End Get
        End Property

        ''' <summary>多分支跳转（switch）</summary>
        Public ReadOnly Property IsSwitch As Boolean
            Get
                Return Code.OperandType = OperandType.InlineSwitch
            End Get
        End Property

        ''' <summary>是否为任何一种跳转指令</summary>
        Public ReadOnly Property IsBranch As Boolean
            Get
                Return IsUnconditionalBranch OrElse IsConditionalBranch OrElse IsSwitch
            End Get
        End Property

        ''' <summary>ret</summary>
        Public ReadOnly Property IsReturn As Boolean
            Get
                Return Code.FlowControl = FlowControl.Return
            End Get
        End Property

        ''' <summary>
        ''' 该指令的全部跳转目标（绝对 IL 偏移）。
        ''' 条件/无条件跳转返回单元素数组，switch 返回全部 case 目标，其余返回空数组。
        ''' </summary>
        Public ReadOnly Property BranchTargets As Integer()
            Get
                If Code.OperandType = OperandType.InlineSwitch Then
                    Dim targets = TryCast(Operand, Integer())
                    Return If(targets, New Integer() {})
                End If

                If IsBranch Then
                    If TypeOf Operand Is Integer Then Return New Integer() {CInt(Operand)}
                    Return New Integer() {}
                End If

                Return New Integer() {}
            End Get
        End Property

        ''' <summary>该指令是否会把控制流交给后面的指令（用于基本块切分）</summary>
        Public ReadOnly Property CanFallThrough As Boolean
            Get
                Return Not (Code.FlowControl = FlowControl.Branch OrElse
                            Code.FlowControl = FlowControl.Return OrElse
                            Code.FlowControl = FlowControl.Throw)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return GetCode()
        End Function

        ''' <summary>
        ''' Returns a friendly strign representation of this instruction
        ''' </summary>
        ''' <returns></returns>
        Public Function GetCode() As String
            Dim result = GetExpandedOffset(Offset) & " : " & Code.ToString

            If Operand IsNot Nothing Then
                result = result & GetOperandCode()
            End If

            Return result
        End Function

        Private Function buildInlineMethodCode() As String
            Dim result = " "

            Try
                Dim mOperand = CType(Operand, MethodInfo)

                If Not mOperand.IsStatic Then
                    result &= "instance "
                End If

                result &= ProcessSpecialTypes(mOperand.ReturnType.ToString()) & " " & ProcessSpecialTypes(mOperand.ReflectedType.ToString()) & "::" & mOperand.Name & "()"
            Catch
                Try
                    Dim mOperand = CType(Operand, ConstructorInfo)

                    If Not mOperand.IsStatic Then
                        result &= "instance "
                    End If

                    result &= "void " & ProcessSpecialTypes(mOperand.ReflectedType.ToString()) & "::" & mOperand.Name & "()"
                Catch

                End Try
            End Try

            Return result
        End Function

        Private Function GetOperandCode() As String
            Select Case Code.OperandType
                Case OperandType.InlineField
                    With CType(Operand, FieldInfo)
                        Return " " & ProcessSpecialTypes(.FieldType.ToString()) & " " & ProcessSpecialTypes(.ReflectedType.ToString()) & "::" & .Name & ""
                    End With
                Case OperandType.InlineMethod
                    Return buildInlineMethodCode()
                Case OperandType.ShortInlineBrTarget, OperandType.InlineBrTarget
                    Return " " & GetExpandedOffset(CInt(Operand))
                Case OperandType.InlineType
                    Return " " & ProcessSpecialTypes(Operand.ToString())
                Case OperandType.InlineString

                    If Equals(Operand.ToString(), vbCrLf) Then
                        Return " ""\r\n"""
                    Else
                        Return " """ & Operand.ToString() & """"
                    End If

                Case OperandType.ShortInlineVar
                    Return Operand.ToString()
                Case OperandType.InlineI, OperandType.InlineI8, OperandType.InlineR, OperandType.ShortInlineI, OperandType.ShortInlineR
                    Return Operand.ToString()
                Case OperandType.InlineTok

                    If TypeOf Operand Is Type Then
                        Return CType(Operand, Type).FullName
                    Else
                        Return "not supported"
                    End If

                Case OperandType.InlineSwitch
                    Return " (" & String.Join(", ", BranchTargets.Select(Function(t) GetExpandedOffset(t))) & ")"

                Case Else
                    Return "not supported"
            End Select
        End Function

        ''' <summary>
        ''' Add enough zeros to a number as to be represented on 4 characters
        ''' </summary>
        ''' <param name="offset">
        ''' The number that must be represented on 4 characters
        ''' </param>
        ''' <returns>
        ''' </returns>
        Private Function GetExpandedOffset(offset As Long) As String
            Dim result As String = offset.ToString()
            Dim i = 0

            While result.Length < 4
                result = "0" & result
                i += 1
            End While

            Return result
        End Function
    End Class
End Namespace
