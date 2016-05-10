Imports System.Runtime.InteropServices.Marshal

Namespace Emit.Marshal

    Public Class [Integer] : Inherits IntPtr(Of Integer)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy, AddressOf Copy)
        End Sub
    End Class

    Public Class [Char] : Inherits IntPtr(Of Char)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy, AddressOf Copy)
        End Sub
    End Class

    Public Class [Short] : Inherits IntPtr(Of Short)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy, AddressOf Copy)
        End Sub
    End Class

    Public Class [Long] : Inherits IntPtr(Of Long)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy, AddressOf Copy)
        End Sub
    End Class

    Public Class [Single] : Inherits IntPtr(Of Single)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy, AddressOf Copy)
        End Sub
    End Class

    ''' <summary>
    ''' Represents a pointer to an 8-bit unsigned integer array.
    ''' </summary>
    Public Class [Byte] : Inherits IntPtr(Of Byte)

        ''' <summary>
        ''' Represents a pointer to an 8-bit unsigned integer array.
        ''' </summary>
        ''' <param name="p">The start address location of the array in the memory</param>
        ''' <param name="chunkSize">array length</param>
        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy, AddressOf Copy)
        End Sub
    End Class

    Public Class [IntPtr] : Inherits IntPtr(Of System.IntPtr)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy, AddressOf Copy)
        End Sub
    End Class

    Public Class [Double] : Inherits IntPtr(Of Double)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy, AddressOf Copy)
        End Sub
    End Class
End Namespace