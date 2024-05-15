#Region "Microsoft.VisualBasic::339ac62392fa35348059872eeb247db2, Data\BinaryData\Feather\Impl\UnsafeArrayReader.vb"

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

    '   Total Lines: 31
    '    Code Lines: 25
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.75 KB


    '     Class UnsafeArrayReader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: ReadArray
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO.MemoryMappedFiles
Imports System.Reflection.Emit

Namespace Impl
    Friend NotInheritable Class UnsafeArrayReader(Of T)
        Private Shared ReadOnly [Delegate] As Action(Of MemoryMappedViewAccessor, Long, T(), Integer, Integer)

        Shared Sub New()
            Dim dyn = New DynamicMethod("UnsafeArrayReader_" & GetType(T).Name, Nothing, {GetType(MemoryMappedViewAccessor), GetType(Long), GetType(T()), GetType(Integer), GetType(Integer)})
            Dim il = dyn.GetILGenerator()

            Dim readArrayGen = GetType(MemoryMappedViewAccessor).GetMethod("ReadArray")
            Dim readArray = readArrayGen.MakeGenericMethod(GetType(T))

            il.Emit(OpCodes.Ldarg_0)            ' MemoryMappedViewAccessor
            il.Emit(OpCodes.Ldarg_1)            ' MemoryMappedViewAccessor long
            il.Emit(OpCodes.Ldarg_2)            ' MemoryMappedViewAccessor long T[]
            il.Emit(OpCodes.Ldarg_3)            ' MemoryMappedViewAccessor long T[] int
            il.Emit(OpCodes.Ldarg_S, CByte(4))   ' MemoryMappedViewAccessor long T[] int int
            il.Emit(OpCodes.Call, readArray)    ' int
            il.Emit(OpCodes.Pop)                ' --empty--
            il.Emit(OpCodes.Ret)                ' --empty--

            [Delegate] = CType(dyn.CreateDelegate(GetType(Action(Of MemoryMappedViewAccessor, Long, T(), Integer, Integer))), Action(Of MemoryMappedViewAccessor, Long, T(), Integer, Integer))
        End Sub

        Public Shared Sub ReadArray(view As MemoryMappedViewAccessor, position As Long, arr As T(), index As Integer, length As Integer)
            [Delegate](view, position, arr, index, length)
        End Sub
    End Class
End Namespace

