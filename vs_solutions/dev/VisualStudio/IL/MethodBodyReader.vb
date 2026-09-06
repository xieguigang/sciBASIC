#Region "Microsoft.VisualBasic::75bbbd352531d2ceedbfc8dcce6a417b, vs_solutions\dev\VisualStudio\IL\MethodBodyReader.vb"

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

    '   Total Lines: 228
    '    Code Lines: 149 (65.35%)
    ' Comment Lines: 35 (15.35%)
    '    - Xml Docs: 51.43%
    ' 
    '   Blank Lines: 44 (19.30%)
    '     File Size: 8.84 KB


    '     Class MethodBodyReader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetBodyCode, GetEnumerator, GetRefferencedOperand, IEnumerable_GetEnumerator, ParseIL
    ' 
    '         Sub: ConstructInstructions, (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Reflection.Emit
Imports Microsoft.VisualBasic.Language

Namespace IL

    ''' <summary>
    ''' Parsing the IL of a Method Body
    ''' 
    ''' > https://www.codeproject.com/articles/14058/parsing-the-il-of-a-method-body
    ''' </summary>
    Public Class MethodBodyReader : Implements IEnumerable(Of ILInstruction)
        Implements IDisposable

        ReadOnly _instructions As New List(Of ILInstruction)
        ReadOnly il As Stream
        ReadOnly mi As MethodInfo = Nothing

        ''' <summary>方法的原始 IL 字节（用于回填 <see cref="ILInstruction.OperandData"/>）</summary>
        ReadOnly ilBytes As Byte()
        ''' <summary>局部变量表，下标即 ldloc / stloc 的操作数</summary>
        ReadOnly _locals As LocalVariableInfo()
        ''' <summary>异常处理子句（切分基本块时不能跨越 try/handler 边界）</summary>
        ReadOnly _exceptionClauses As ExceptionHandlingClause()
        ''' <summary>IL 偏移 -> 指令下标</summary>
        ReadOnly offsetIndex As Dictionary(Of Integer, Integer)
        ReadOnly _maxStackSize As Integer

        Private disposedValue As Boolean

        ''' <summary>
        ''' MethodBodyReader constructor
        ''' </summary>
        ''' <param name="mi">
        ''' The System.Reflection defined MethodInfo
        ''' </param>
        Public Sub New(mi As MethodInfo)
            Me.mi = mi
            Me.offsetIndex = New Dictionary(Of Integer, Integer)()
            Me._locals = New LocalVariableInfo() {}
            Me._exceptionClauses = New ExceptionHandlingClause() {}
            Me.ilBytes = New Byte() {}

            Dim body = If(mi Is Nothing, Nothing, mi.GetMethodBody())

            If body IsNot Nothing Then
                Me.ilBytes = If(body.GetILAsByteArray(), New Byte() {})
                Me._maxStackSize = body.MaxStackSize
                Me._locals = body.LocalVariables.ToArray()
                Me._exceptionClauses = body.ExceptionHandlingClauses.ToArray()
                Me.il = New MemoryStream(Me.ilBytes)

                ConstructInstructions(mi.Module)

                For i As Integer = 0 To _instructions.Count - 1
                    offsetIndex(_instructions(i).Offset) = i
                Next
            End If
        End Sub

        ''' <summary>解析得到的指令序列（按 IL 偏移升序）</summary>
        Public ReadOnly Property Instructions As IReadOnlyList(Of ILInstruction)
            Get
                Return _instructions
            End Get
        End Property

        ''' <summary>方法的最大求值栈深度</summary>
        Public ReadOnly Property MaxStackSize As Integer
            Get
                Return _maxStackSize
            End Get
        End Property

        ''' <summary>局部变量表（下标即 ldloc / stloc 的操作数）</summary>
        Public ReadOnly Property Locals As IReadOnlyList(Of LocalVariableInfo)
            Get
                Return _locals
            End Get
        End Property

        ''' <summary>异常处理子句</summary>
        Public ReadOnly Property ExceptionClauses As IReadOnlyList(Of ExceptionHandlingClause)
            Get
                Return _exceptionClauses
            End Get
        End Property

        ''' <summary>按 IL 偏移查指令下标；不存在返回 -1</summary>
        Public Function IndexByOffset(offset As Integer) As Integer
            Dim index As Integer = -1
            If offsetIndex.TryGetValue(offset, index) Then Return index
            Return -1
        End Function

        ''' <summary>按 IL 偏移取指令；不存在返回 Nothing</summary>
        Public Function InstructionAt(offset As Integer) As ILInstruction
            Dim index = IndexByOffset(offset)
            Return If(index >= 0, _instructions(index), Nothing)
        End Function

        ''' <summary>该方法是否含有 try / catch / finally 等异常结构</summary>
        Public ReadOnly Property HasExceptionHandlers As Boolean
            Get
                Return _exceptionClauses IsNot Nothing AndAlso _exceptionClauses.Length > 0
            End Get
        End Property

        ''' <summary>
        ''' Constructs the array of ILInstructions according to the IL byte code.
        ''' </summary>
        ''' <param name="module"></param>
        Private Sub ConstructInstructions([module] As [Module])
            Dim il As New BinaryReader(Me.il)

            While il.BaseStream.Position < il.BaseStream.Length
                _instructions.Add(ParseIL(il, [module], mi))
            End While
        End Sub

        Private Function ParseIL(il As BinaryReader, [module] As [Module], mi As MethodInfo) As ILInstruction
            Dim instruction As New ILInstruction()
            Dim metadataToken As Integer = 0

            ' 指令起始偏移：必须在读操作码之前记录。
            ' 原实现用 "读完之后的位置 - 1"，对 0xFE 开头的双字节操作码会算成 start+1。
            Dim startOffset As Integer = CInt(il.BaseStream.Position)

            ' get the operation code of the current instruction
            Dim code As OpCode = OpCodes.Nop
            Dim value As UShort = il.ReadByte

            If value <> &HFE Then
                code = singleByteOpCodes(value)
            Else
                value = il.ReadByte
                code = multiByteOpCodes(value)
                value = CUShort(value Or &HFE00)
            End If

            instruction.Code = code
            instruction.Offset = startOffset

            Dim operandStart As Integer = CInt(il.BaseStream.Position)

            ' get the operand of the current operation
            Select Case code.OperandType
                Case OperandType.InlineBrTarget
                    metadataToken = il.ReadInt32
                    metadataToken += CInt(il.BaseStream.Position)
                    instruction.Operand = metadataToken

                Case OperandType.InlineField
                    metadataToken = il.ReadInt32
                    instruction.Operand = ResolveOrToken([module], metadataToken, TokenKind.Field)

                Case OperandType.InlineMethod
                    metadataToken = il.ReadInt32
                    instruction.Operand = ResolveOrToken([module], metadataToken, TokenKind.Method)

                Case OperandType.InlineSig
                    metadataToken = il.ReadInt32

                    Try
                        instruction.Operand = [module].ResolveSignature(metadataToken)
                    Catch
                        instruction.Operand = metadataToken
                    End Try

                Case OperandType.InlineTok
                    metadataToken = il.ReadInt32
                    instruction.Operand = ResolveOrToken([module], metadataToken, TokenKind.Tok)

                Case OperandType.InlineType
                    metadataToken = il.ReadInt32
                    instruction.Operand = ResolveOrToken([module], metadataToken, TokenKind.Type, mi)

                Case OperandType.InlineI
                    instruction.Operand = il.ReadInt32

                Case OperandType.InlineI8
                    instruction.Operand = il.ReadInt64

                Case OperandType.InlineNone
                    instruction.Operand = Nothing

                Case OperandType.InlineR
                    instruction.Operand = il.ReadDouble

                Case OperandType.InlineString
                    metadataToken = il.ReadInt32

                    Try
                        instruction.Operand = [module].ResolveString(metadataToken)
                    Catch
                        instruction.Operand = metadataToken
                    End Try

                Case OperandType.InlineSwitch
                    Dim count = il.ReadInt32
                    Dim casesAddresses = New Integer(count - 1) {}

                    For i = 0 To count - 1
                        casesAddresses(i) = il.ReadInt32
                    Next

                    Dim baseOffset As Integer = CInt(il.BaseStream.Position)
                    Dim targets = New Integer(count - 1) {}

                    For i = 0 To count - 1
                        targets(i) = baseOffset + casesAddresses(i)
                    Next

                    ' 原实现算出了 targets 却从未写回 instruction，导致 switch 的操作数丢失
                    instruction.Operand = targets

                Case OperandType.InlineVar
                    ' 统一成 Integer，避免后面做 ldloc/stloc 下标运算时混用 UInt16 / Byte
                    instruction.Operand = CInt(il.ReadUInt16)

                Case OperandType.ShortInlineBrTarget
                    instruction.Operand = CInt(il.ReadSByte) + CInt(il.BaseStream.Position)

                Case OperandType.ShortInlineI
                    instruction.Operand = CInt(il.ReadSByte)

                Case OperandType.ShortInlineR
                    instruction.Operand = il.ReadSingle

                Case OperandType.ShortInlineVar
                    instruction.Operand = CInt(il.ReadByte)

                Case Else
                    Throw New Exception("Unknown operand type.")
            End Select

            instruction.Size = CInt(il.BaseStream.Position) - startOffset
            instruction.OperandData = Slice(operandStart, CInt(il.BaseStream.Position))

            Return instruction
        End Function

        Private Enum TokenKind
            Field
            Method
            Type
            Tok
        End Enum

        ''' <summary>
        ''' 解析元数据令牌；解析失败时退回令牌本身（而不是让整段 IL 解析崩掉），
        ''' 上层遇到 Integer 类型的操作数即可判定"该指令不受支持"。
        ''' </summary>
        Private Shared Function ResolveOrToken([module] As [Module], token As Integer,
                                               kind As TokenKind,
                                               Optional mi As MethodInfo = Nothing) As Object
            Try
                Select Case kind
                    Case TokenKind.Field
                        Return [module].ResolveField(token)
                    Case TokenKind.Method
                        Return [module].ResolveMethod(token)
                    Case TokenKind.Type
                        If mi IsNot Nothing Then
                            Return [module].ResolveType(token,
                                                        mi.DeclaringType.GetGenericArguments(),
                                                        mi.GetGenericArguments())
                        End If

                        Return [module].ResolveType(token)
                    Case Else
                        Try
                            Return [module].ResolveType(token)
                        Catch
                            Return [module].ResolveMember(token)
                        End Try
                End Select
            Catch
                Return token
            End Try
        End Function

        ''' <summary>从原始 IL 字节里切出 [startIndex, endIndex) 段</summary>
        Private Function Slice(startIndex As Integer, endIndex As Integer) As Byte()
            If ilBytes Is Nothing Then Return New Byte() {}
            If startIndex < 0 Then startIndex = 0
            If endIndex > ilBytes.Length Then endIndex = ilBytes.Length

            Dim length = endIndex - startIndex
            If length <= 0 Then Return New Byte() {}

            Dim buffer(length - 1) As Byte
            Array.Copy(ilBytes, startIndex, buffer, 0, length)

            Return buffer
        End Function

        Public Function GetRefferencedOperand([module] As [Module], metadataToken As Integer) As Object
            Dim assemblyNames As AssemblyName() = [module].Assembly.GetReferencedAssemblies()
            Dim modules As [Module]()

            For i As Integer = 0 To assemblyNames.Length - 1
                modules = Assembly.Load(assemblyNames(i)).GetModules()

                For j As Integer = 0 To modules.Length - 1
                    Try
                        Dim t = modules(j).ResolveType(metadataToken)
                        Return t
                    Catch
                    End Try
                Next
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the IL code of the method
        ''' </summary>
        ''' <returns></returns>
        Public Function GetBodyCode() As String
            Dim result = ""

            If _instructions IsNot Nothing Then
                For i As Integer = 0 To _instructions.Count - 1
                    result += _instructions(i).GetCode() & vbLf
                Next
            End If

            Return result
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' 只释放流，不清空 _instructions：
                    ' 调用方常在 Using 块外继续读取已解析好的指令列表。
                    If il IsNot Nothing Then Call il.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并替代终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of ILInstruction) Implements IEnumerable(Of ILInstruction).GetEnumerator
            For Each il As ILInstruction In _instructions
                Yield il
            Next
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class
End Namespace
