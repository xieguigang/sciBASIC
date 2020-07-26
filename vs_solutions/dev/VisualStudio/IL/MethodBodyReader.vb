#Region "Microsoft.VisualBasic::92fb3b9b252bde39973bff458c89c70c, vs_solutions\dev\VisualStudio\IL\MethodBodyReader.vb"

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

        ReadOnly instructions As New List(Of ILInstruction)
        ReadOnly il As Stream
        ReadOnly mi As MethodInfo = Nothing

        Private disposedValue As Boolean

        ''' <summary>
        ''' MethodBodyReader constructor
        ''' </summary>
        ''' <param name="mi">
        ''' The System.Reflection defined MethodInfo
        ''' </param>
        Public Sub New(mi As MethodInfo)
            Me.mi = mi

            If mi.GetMethodBody() IsNot Nothing Then
                il = New MemoryStream(mi.GetMethodBody().GetILAsByteArray())
                ConstructInstructions(mi.Module)
            End If
        End Sub

        ''' <summary>
        ''' Constructs the array of ILInstructions according to the IL byte code.
        ''' </summary>
        ''' <param name="module"></param>
        Private Sub ConstructInstructions([module] As [Module])
            Dim il As New BinaryReader(Me.il)

            While il.BaseStream.Position < il.BaseStream.Length
                instructions.Add(ParseIL(il, [module], mi))
            End While
        End Sub

        Private Shared Function ParseIL(il As BinaryReader, [module] As [Module], mi As MethodInfo) As ILInstruction
            Dim instruction As New ILInstruction()
            Dim metadataToken As Integer = 0
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
            instruction.Offset = il.BaseStream.Position - 1

            ' get the operand of the current operation
            Select Case code.OperandType
                Case OperandType.InlineBrTarget
                    metadataToken = il.ReadInt32
                    metadataToken += il.BaseStream.Position
                    instruction.Operand = metadataToken
                Case OperandType.InlineField
                    metadataToken = il.ReadInt32
                    instruction.Operand = [module].ResolveField(metadataToken)
                Case OperandType.InlineMethod
                    metadataToken = il.ReadInt32

                    Try
                        instruction.Operand = [module].ResolveMethod(metadataToken)
                    Catch
                        instruction.Operand = [module].ResolveMember(metadataToken)
                    End Try

                Case OperandType.InlineSig
                    metadataToken = il.ReadInt32
                    instruction.Operand = [module].ResolveSignature(metadataToken)
                Case OperandType.InlineTok
                    metadataToken = il.ReadInt32

                    Try
                        instruction.Operand = [module].ResolveType(metadataToken)
                    Catch
                        ' SSS : see what to do here
                    End Try

                Case OperandType.InlineType
                    metadataToken = il.ReadInt32
                    ' now we call the ResolveType always using the generic attributes type in order
                    ' to support decompilation of generic methods and classes

                    ' thanks to the guys from code project who commented on this missing feature
                    instruction.Operand = [module].ResolveType(metadataToken, mi.DeclaringType.GetGenericArguments(), mi.GetGenericArguments())
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
                    instruction.Operand = [module].ResolveString(metadataToken)

                Case OperandType.InlineSwitch
                    Dim count = il.ReadInt32
                    Dim casesAddresses = New Integer(count - 1) {}

                    For i = 0 To count - 1
                        casesAddresses(i) = il.ReadInt32
                    Next

                    Dim cases = New Integer(count - 1) {}
                    Dim position_i As Integer = il.BaseStream.Position

                    For i = 0 To count - 1
                        cases(i) = position_i + casesAddresses(i)
                    Next
                Case OperandType.InlineVar
                    instruction.Operand = il.ReadUInt16

                Case OperandType.ShortInlineBrTarget
                    instruction.Operand = il.ReadSByte + il.BaseStream.Position

                Case OperandType.ShortInlineI
                    instruction.Operand = il.ReadSByte

                Case OperandType.ShortInlineR
                    instruction.Operand = il.ReadSingle

                Case OperandType.ShortInlineVar
                    instruction.Operand = il.ReadByte

                Case Else
                    Throw New Exception("Unknown operand type.")
            End Select

            Return instruction
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

            If instructions IsNot Nothing Then
                For i As Integer = 0 To instructions.Count - 1
                    result += instructions(i).GetCode() & vbLf
                Next
            End If

            Return result
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call il.Dispose()
                    Call instructions.Clear()
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
            For Each il As ILInstruction In instructions
                Yield il
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace

