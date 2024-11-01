#Region "Microsoft.VisualBasic::f78c572d997100f63001c4e0f19366b8, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Bitmap\BinarySerializationExtensions.vb"

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

    '   Total Lines: 37
    '    Code Lines: 23 (62.16%)
    ' Comment Lines: 6 (16.22%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (21.62%)
    '     File Size: 1.66 KB


    '     Module BinarySerializationExtensions
    ' 
    '         Function: Deserialize, Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
