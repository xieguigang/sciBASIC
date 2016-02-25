Imports System.Runtime.InteropServices.Marshal

Namespace Marshal

    Public Class [Integer] : Inherits IntPtr(Of Integer)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy)
        End Sub
    End Class

    Public Class [Char] : Inherits IntPtr(Of Char)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy)
        End Sub
    End Class

    Public Class [Short] : Inherits IntPtr(Of Short)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy)
        End Sub
    End Class

    Public Class [Long] : Inherits IntPtr(Of Long)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy)
        End Sub
    End Class

    Public Class [Single] : Inherits IntPtr(Of Single)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy)
        End Sub
    End Class

    Public Class [Byte] : Inherits IntPtr(Of Byte)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy)
        End Sub
    End Class

    Public Class [IntPtr] : Inherits IntPtr(Of System.IntPtr)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy)
        End Sub
    End Class

    Public Class [Double] : Inherits IntPtr(Of Double)

        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy)
        End Sub
    End Class
End Namespace