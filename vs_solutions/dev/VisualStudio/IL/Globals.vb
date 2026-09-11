#Region "Microsoft.VisualBasic::5549d3c4d5c42d0e0c5686da86a3f038, vs_solutions\dev\VisualStudio\IL\Globals.vb"

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

    '   Total Lines: 251
    '    Code Lines: 86 (34.26%)
    ' Comment Lines: 136 (54.18%)
    '    - Xml Docs: 18.38%
    ' 
    '   Blank Lines: 29 (11.55%)
    '     File Size: 7.84 KB


    '     Module Globals
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FixedPopCount, FixedPushCount, GetOpCode, ProcessSpecialTypes
    ' 
    '         Sub: LoadOpCodes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Runtime.CompilerServices

Namespace IL
    'public enum AssemblyType
    '{
    '    None,
    '    Console,
    '    Application,
    '    Library
    '}

    'public enum BinaryOperator
    '{
    '    Add,
    '    Subtract,
    '    Multiply,
    '    Divide,
    '    Modulus,
    '    ShiftLeft,
    '    ShiftRight,
    '    IdentityEquality,
    '    IdentityInequality,
    '    ValueEquality,
    '    ValueInequality,
    '    BitwiseOr,
    '    BitwiseAnd,
    '    BitwiseExclusiveOr,
    '    BooleanOr,
    '    BooleanAnd,
    '    LessThan,
    '    LessThanOrEqual,
    '    GreaterThan,
    '    GreaterThanOrEqual
    '}

    'public enum ExceptionHandlerType
    '{
    '    Finally,
    '    Catch,
    '    Filter,
    '    Fault
    '}

    'public enum FieldVisibility
    '{
    '    Private,
    '    Public,
    '    Internal,
    '    Protected,
    '}

    'public enum MethodVisibility
    '{
    '    Private,
    '    Public,
    '    Internal,
    '    External,
    '    Protected,
    '}
    'public enum MethodModifier
    '{
    '    Static,
    '    Override,
    '    Abstract,
    '    Virtual,
    '    Final,
    '    None,
    '}


    'public enum ResourceVisibility
    '{
    '    Public,
    '    Private
    '}

    'public enum TypeVisibility
    '{
    '    vPublic,
    '    vProtected,
    '    vInternal,
    '    vProtectedInternal,
    '    vPrivate
    '}

    'public enum ClassModifiers
    '{
    '    mAbstract,
    '    mSealed,
    '    mStatic,
    '    mNone,
    '}

    'public enum UnaryOperator
    '{
    '    Negate,
    '    BooleanNot,
    '    BitwiseNot,
    '    PreIncrement,
    '    PreDecrement,
    '    PostIncrement,
    '    PostDecrement
    '}

    Public Module Globals

        ReadOnly OpcodeLookupTable As Dictionary(Of Short, OpCode) = GetType(OpCodes) _
            .GetFields() _
            .[Select](Function(f)
                          Return CType(f.GetValue(Nothing), OpCode)
                      End Function) _
            .ToDictionary(Function(op)
                              Return op.Value
                          End Function)

        Public Cache As Dictionary(Of Integer, Object) = New Dictionary(Of Integer, Object)()
        Public multiByteOpCodes As OpCode()
        Public singleByteOpCodes As OpCode()
        Public modules As [Module]() = Nothing

        Sub New()
            singleByteOpCodes = New OpCode(255) {}
            multiByteOpCodes = New OpCode(255) {}

            Call LoadOpCodes()
        End Sub

        ''' <summary>
        ''' get msil opcode from <see cref="OpcodeLookupTable"/>
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetOpCode(i As Short) As OpCode
            Return OpcodeLookupTable(key:=i)
        End Function

        Private Sub LoadOpCodes()
            For Each info1 As FieldInfo In GetType(OpCodes).GetFields()
                If Not info1.FieldType Is GetType(OpCode) Then
                    Continue For
                End If

                Dim code1 As OpCode = info1.GetValue(Nothing)
                ' OpCode.Value 是有符号的 Short: 两字节操作码(0xFExx)在这里表现为负数,
                ' 必须先按位与成无符号再转 UShort, 否则在开启了整数溢出检查的
                ' 编译配置(RemoveIntegerChecks=false, 例如 Release|AnyCPU)下会抛
                ' OverflowException, 导致本模块的静态构造函数失败。
                Dim num2 As UShort = CUShort(CInt(code1.Value) And &HFFFF)

                If num2 < &H100 Then
                    singleByteOpCodes(num2) = code1
                Else

                    If (num2 And &HFF00) <> &HFE00 Then
                        Throw New Exception("Invalid OpCode.")
                    End If

                    multiByteOpCodes(num2 And &HFF) = code1
                End If
            Next
        End Sub


        ''' <summary>
        ''' Retrieve the friendly name of a type
        ''' </summary>
        ''' <param name="typeName">
        ''' The complete name to the type
        ''' </param>
        ''' <returns>
        ''' The simplified name of the type (i.e. "int" instead f System.Int32)
        ''' </returns>
        Public Function ProcessSpecialTypes(typeName As String) As String
            Dim result = typeName

            Select Case typeName
                Case "System.string", "System.String", "String"
                    result = "string"
                Case "System.Int32", "Int", "Int32"
                    result = "int"
            End Select

            Return result
        End Function

        ''' <summary>
        ''' 由 <see cref="OpCode.StackBehaviourPop"/> 推导出该指令"固定"弹出的求值栈槽位数。
        ''' </summary>
        ''' <returns>
        ''' 0 / 1 / 2 / 3；<see cref="StackBehaviour.Varpop"/>（call / callvirt / ret 等
        ''' 弹栈数量取决于签名）返回 -1，由调用方另行处理。
        ''' </returns>
        Public Function FixedPopCount(code As OpCode) As Integer
            Select Case code.StackBehaviourPop
                Case StackBehaviour.Pop0
                    Return 0
                Case StackBehaviour.Pop1, StackBehaviour.Popi, StackBehaviour.Popref
                    Return 1
                Case StackBehaviour.Pop1_pop1, StackBehaviour.Popi_pop1, StackBehaviour.Popi_popi,
                     StackBehaviour.Popi_popi8, StackBehaviour.Popi_popr4, StackBehaviour.Popi_popr8,
                     StackBehaviour.Popref_pop1, StackBehaviour.Popref_popi
                    Return 2
                Case StackBehaviour.Popi_popi_popi, StackBehaviour.Popref_popi_popi,
                     StackBehaviour.Popref_popi_popi8, StackBehaviour.Popref_popi_popr4,
                     StackBehaviour.Popref_popi_popr8, StackBehaviour.Popref_popi_popref
                    Return 3
                Case Else
                    Return -1
            End Select
        End Function

        ''' <summary>
        ''' 由 <see cref="OpCode.StackBehaviourPush"/> 推导出该指令"固定"压入的求值栈槽位数。
        ''' </summary>
        ''' <returns>0 / 1 / 2；<see cref="StackBehaviour.Varpush"/> 返回 -1。</returns>
        Public Function FixedPushCount(code As OpCode) As Integer
            Select Case code.StackBehaviourPush
                Case StackBehaviour.Push0
                    Return 0
                Case StackBehaviour.Push1, StackBehaviour.Pushi, StackBehaviour.Pushi8,
                     StackBehaviour.Pushr4, StackBehaviour.Pushr8, StackBehaviour.Pushref
                    Return 1
                Case StackBehaviour.Push1_push1
                    Return 2
                Case Else
                    Return -1
            End Select
        End Function

        'public static string SpaceGenerator(int count)
        '{
        '    string result = "";
        '    for (int i = 0; i < count; i++) result += " ";
        '    return result;
        '}

        'public static string AddBeginSpaces(string source, int count)
        '{
        '    string[] elems = source.Split('\n');
        '    string result = "";
        '    for (int i = 0; i < elems.Length; i++)
        '    {
        '        result += SpaceGenerator(count) + elems[i] + "\n";
        '    }
        '    return result;
        '}
    End Module
End Namespace
