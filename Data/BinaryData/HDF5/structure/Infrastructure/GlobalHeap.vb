#Region "Microsoft.VisualBasic::dd3f689b9ed8f154cc55f5ff2ed72a3d, Data\BinaryData\HDF5\structure\Infrastructure\GlobalHeap.vb"

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

    '   Total Lines: 182
    '    Code Lines: 76 (41.76%)
    ' Comment Lines: 78 (42.86%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 28 (15.38%)
    '     File Size: 7.67 KB


    '     Class GlobalHeap
    ' 
    '         Properties: collectionSize, objects, signature, version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: printValues
    ' 
    '     Class GlobalHeapObject
    ' 
    '         Properties: data, index, objectSize, references
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    ''' <summary>
    ''' Each HDF5 file has a global heap which stores various types of information 
    ''' which is typically shared between datasets. The global heap was designed 
    ''' to satisfy these goals:
    ''' 
    ''' 1. Repeated access to a heap object must be efficient without resulting in 
    '''    repeated file I/O requests. Since global heap objects will typically be 
    '''    shared among several datasets, it is probable that the object will be 
    '''    accessed repeatedly.
    ''' 2. Collections of related global heap objects should result in fewer And 
    '''    larger I/O requests. For instance, a dataset of object references will have 
    '''    a global heap object for each reference. Reading the entire set of object 
    '''    references should result in a few large I/O requests instead of one small 
    '''    I/O request for each reference.
    ''' 3. It should be possible To remove objects from the Global heap And the resulting 
    '''    file hole should be eligible To be reclaimed For other uses.
    '''    
    ''' The implementation of the heap makes use of the memory management already available
    ''' at the file level and combines that with a new object called a collection to achieve 
    ''' goal B. The global heap is the set of all collections. Each global heap object 
    ''' belongs to exactly one collection, and each collection contains one or more global 
    ''' heap objects. For the purposes of disk I/O and caching, a collection is treated as 
    ''' an atomic object, addressing goal A.
    '''
    ''' When a global heap object Is deleted from a collection (which occurs when its 
    ''' reference count falls to zero), objects located after the deleted object in the 
    ''' collection are packed down toward the beginning of the collection, And the 
    ''' collection's global heap object 0 is created (if possible), or its size is increased 
    ''' to account for the recently freed space. There are no gaps between objects in each 
    ''' collection, with the possible exception of the final space in the collection, if 
    ''' it is not large enough to hold the header for the collection’s global heap object 0. 
    ''' These features address goal C.
    '''
    ''' The HDF5 Library creates Global heap collections As needed, so there may be multiple 
    ''' collections throughout the file. The Set Of all Of them Is abstractly called the 
    ''' “global heap”, although they Do Not actually link To Each other, And there Is no 
    ''' Global place In the file where you can discover all Of the collections. The collections 
    ''' are found simply by finding a reference To one through another Object In the file. 
    ''' For example, data Of variable-length datatype elements Is stored In the Global heap 
    ''' And Is accessed via a Global heap ID. The format For Global heap IDs Is described at 
    ''' the End Of this section.
    ''' </summary>
    Public Class GlobalHeap : Inherits HDF5Ptr

        ''' <summary>
        ''' 4 bytes (GCOL)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property signature As Byte()
        ''' <summary>
        ''' 1 byte
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property version As Integer
        ''' <summary>
        ''' [4 bytes] This is the size in bytes of the entire collection including this field. 
        ''' The default (and minimum) collection size is 4096 bytes which is a typical file 
        ''' system block size. This allows for 127 16-byte heap objects plus their overhead 
        ''' (the collection header of 16 bytes and the 16 bytes of information about each heap 
        ''' object).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property collectionSize As Integer

        Public ReadOnly Property objects As New Dictionary(Of Integer, GlobalHeapObject)

        Public Sub New(sb As Superblock, address As Long)
            MyBase.New(address)

            Dim reader As BinaryReader = sb.FileReader(-1)
            Dim headerSize As Integer = 4 + 1 + 3 + sb.sizeOfLengths

            reader.offset = address
            reader.Mark()

            signature = reader.readBytes(4)
            version = reader.readByte

            reader.Reset()
            reader.offset += 8

            collectionSize = device.readL(reader, sb)

            reader.Reset()
            reader.offset += headerSize

            Dim size = collectionSize

            While size > 8
                Dim [object] As New GlobalHeapObject(Me, sb)

                size -= reader.deltaSize

                If [object].index = 0 Then
                    Exit While
                Else
                    objects(key:=[object].index) = [object]
                End If
            End While
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Throw New NotImplementedException()
        End Sub
    End Class

    Public Class GlobalHeapObject

        ''' <summary>
        ''' 2 bytes
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property index As Integer
        ''' <summary>
        ''' Reference Count
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property references As Integer
        ''' <summary>
        ''' This is the size of the object data stored for the object. The actual storage 
        ''' space allocated for the object data is rounded up to a multiple of eight.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property objectSize As Integer
        ''' <summary>
        ''' The object data is treated as a one-dimensional array of bytes to be interpreted 
        ''' by the caller.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property data As Byte()

        Sub New(gh As GlobalHeap, sb As Superblock)
            Dim reader As BinaryReader = sb.FileReader(-1)

            Call reader.Mark()

            index = reader.readShort
            references = reader.readShort

            ' Skip 4 reserved bytes
            Call reader.skipBytes(4)

            ' index 等于零的时候是读取操作结束的标志，在这里就不读取后面的数据了，如果index等于零的时候
            If index > 0 Then
                objectSize = device.readO(reader, sb)

                If objectSize = -1 Then
                    data = {}
                Else
                    data = reader.readBytes(objectSize)
                End If

                Call device.seekBufferToNextMultipleOfEight(reader)
            Else
#If DEBUG Then
                Call "index = ZERO!".Warning
#End If
            End If
        End Sub

        Public Overrides Function ToString() As String
            Dim debugView$

            If data.All(Function(b) Not ASCII.IsNonPrinting(b)) Then
                debugView = Encoding.ASCII.GetString(data)
            Else
                debugView = data _
                    .Select(Function(b) b.ToString("X2")) _
                    .JoinBy("-")
            End If

            Return $"#{index} > ({objectSize} bytes) {debugView}"
        End Function
    End Class
End Namespace
