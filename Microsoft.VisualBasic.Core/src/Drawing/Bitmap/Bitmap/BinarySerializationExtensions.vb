Imports System.Runtime.InteropServices

Namespace Imaging.BitmapImage.FileStream

    Public Module BinarySerializationExtensions
        ''' <summary>
        ''' Convert struct to byte[]
        ''' </summary>
        Public Function Serialize(Of T As Structure)(data As T) As Byte()
            Dim size = Marshal.SizeOf(data)    ' how much bytes we need ?
            Dim bufferArray = New Byte(size - 1) {}   ' init buffer

            Dim pointer = Marshal.AllocHGlobal(size)    ' alocate memory for buffer and get pointer
            Marshal.StructureToPtr(data, pointer, True)    ' copy data from struct to alocated memory
            Marshal.Copy(pointer, bufferArray, 0, size)    ' copy data from alocated memory to buffer array
            Marshal.FreeHGlobal(pointer)           ' free alocated memory
            Return bufferArray                     ' return bufferArray
        End Function

        ''' <summary>
        ''' Convert byte[] to struct
        ''' </summary>
        Public Function Deserialize(Of T As Structure)(array As Byte()) As T
            Dim [structure] = New T()

            Dim size = Marshal.SizeOf([structure])   ' how much bytes we need ?
            Dim pointer = Marshal.AllocHGlobal(size)    ' mem alloc.

            Marshal.Copy(array, 0, pointer, size)      ' copy bytes to alloc. mem

            [structure] = CType(Marshal.PtrToStructure(pointer, [structure].GetType()), T)    ' conver aloc. mem to structure
            Marshal.FreeHGlobal(pointer)   ' free memory

            Return [structure]   ' return new structure
        End Function
    End Module
End Namespace
