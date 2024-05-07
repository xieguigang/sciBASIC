Imports System.IO.MemoryMappedFiles
Imports System.Reflection.Emit

Namespace FeatherDotNet.Impl
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
