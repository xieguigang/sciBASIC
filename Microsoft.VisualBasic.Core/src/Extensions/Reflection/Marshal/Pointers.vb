#Region "Microsoft.VisualBasic::f13ea8556ad8d7833dfbd328e3818857, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Marshal\Pointers.vb"

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

    '   Total Lines: 68
    '    Code Lines: 43 (63.24%)
    ' Comment Lines: 8 (11.76%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (25.00%)
    '     File Size: 2.26 KB


    '     Class [Integer]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class [Char]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class [Short]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class [Long]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class [Single]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class [Byte]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class [IntPtr]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class [Double]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        ''' <remarks>
        ''' Make bytes data unsafe copy from a given memory location in this constructor
        ''' </remarks>
        Sub New(p As System.IntPtr, chunkSize As Integer)
            Call MyBase.New(p, chunkSize, AddressOf Copy, AddressOf Copy)
        End Sub

        Sub New(ByRef data As Byte(), Optional p As System.IntPtr? = Nothing)
            Call MyBase.New(data, p)
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
