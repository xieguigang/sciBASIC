#Region "Microsoft.VisualBasic::6dc1683ad25833ec9918bf97721540c5, Data\BinaryData\HDF5\structure\DataChunk.vb"

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

    '   Total Lines: 101
    '    Code Lines: 37
    ' Comment Lines: 47
    '   Blank Lines: 17
    '     File Size: 4.12 KB


    '     Class DataChunk
    ' 
    '         Properties: filePosition, filterMask, offsets, sizeOfChunk
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 



Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    ''' <summary>
    ''' 所存储的数据块
    ''' </summary>
    Public Class DataChunk : Inherits HDF5Ptr

        ''' <summary>
        ''' Size of chunk in bytes.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property sizeOfChunk As Integer
        ''' <summary>
        ''' Filter mask, a 32-bit bit field indicating which filters have been skipped For 
        ''' this chunk. Each filter has an index number In the pipeline (starting at 0, 
        ''' With the first filter To apply) And If that filter Is skipped, the bit corresponding 
        ''' To its index Is Set.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property filterMask As BitSet
        Public ReadOnly Property offsets As Long()
        Public ReadOnly Property filePosition As Long

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sb"></param>
        ''' <param name="address"></param>
        ''' <param name="numberOfDimensions"></param>
        ''' <param name="last"></param>
        ''' <remarks>
        ''' For nodes of node type 1 (chunked raw data nodes), the key is formatted as follows:
        ''' 
        ''' Bytes 1-4	Size of chunk in bytes.
        ''' Bytes 4-8:	Filter mask, a 32-bit bit field indicating which filters have been skipped For 
        '''             this chunk. Each filter has an index number In the pipeline (starting at 0, 
        '''             With the first filter To apply) And If that filter Is skipped, the bit 
        '''             corresponding To its index Is Set.
        '''             
        ''' (D + 1) 64-bit fields:	The offset Of the chunk within the dataset where D Is the number 
        ''' Of dimensions Of the dataset, And the last value Is the offset within the dataset's 
        ''' datatype and should always be zero. For example, if a chunk in a 3-dimensional dataset 
        ''' begins at the position [5,5,5], there will be three such 64-bit values, each with the 
        ''' value of 5, followed by a 0 value.
        ''' </remarks>
        Friend Sub New(sb As Superblock, address As Long, numberOfDimensions As Integer, last As Boolean)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.sizeOfChunk = [in].readInt()
            Me.filterMask = New BitSet([in].readInt)
            Me.offsets = New Long(numberOfDimensions - 1) {}

            For i As Integer = 0 To numberOfDimensions - 1
                Me.offsets(i) = [in].readLong()
            Next

            ' 2019-05-24
            '
            ' 这个64bit数据总是零来的
            ' 需要读取这个字节之后，数据块的文件位置才能够被正确的读取出来
            ' 否则直接读取的话，得到的filePosition的值都是零
            Call [in].skipBytes(8)

            Me.filePosition = If(last, -1, ReadHelper.readO([in], sb))
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("DataChunk >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("size : " & Me.sizeOfChunk)
            console.WriteLine("filter mask : " & Me.filterMask.ToString)

            If Me.offsets IsNot Nothing Then
                For i As Integer = 0 To Me.offsets.Length - 1
                    console.WriteLine("offsets[" & i & "] : " & Me.offsets(i))
                Next
            End If

            console.WriteLine("file position : " & Me.filePosition)

            console.WriteLine("DataChunk <<<")
        End Sub
    End Class

End Namespace
