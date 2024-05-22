#Region "Microsoft.VisualBasic::8c2105f35fb9ff44552c829999b8ab84, Data\BinaryData\HDF5\structure\Infrastructure\LocalHeap.vb"

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

    '   Total Lines: 123
    '    Code Lines: 81 (65.85%)
    ' Comment Lines: 15 (12.20%)
    '    - Xml Docs: 46.67%
    ' 
    '   Blank Lines: 27 (21.95%)
    '     File Size: 4.67 KB


    '     Class LocalHeap
    ' 
    '         Properties: addressOfDataSegment, data, dataSegmentSize, magic, offsetToHeadOfFreeList
    '                     totalLocalHeapSize, version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getString, ToString
    ' 
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
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    ''' <summary>
    ''' A local heap is a collection of small pieces of data that are particular to a single object 
    ''' in the HDF5 file. Objects can be inserted and removed from the heap at any time. The 
    ''' address of a heap does not change once the heap is created. For example, a group stores 
    ''' addresses of objects in symbol table nodes with the names of links stored in the group's 
    ''' local heap.
    ''' </summary>
    Public Class LocalHeap : Inherits HDF5Ptr
        Implements IMagicBlock

        Public Const signature$ = "HEAP"

        Public ReadOnly Property version() As Integer
        Public ReadOnly Property dataSegmentSize() As Long
        Public ReadOnly Property offsetToHeadOfFreeList() As Long
        Public ReadOnly Property addressOfDataSegment() As Long
        Public ReadOnly Property totalLocalHeapSize() As Integer
        Public ReadOnly Property data() As Byte()
        Public ReadOnly Property magic As String Implements IMagicBlock.magic

        Dim reserved0 As Byte()

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            ' signature
            Me.magic = Encoding.ASCII.GetString([in].readBytes(4))

            If Not Me.VerifyMagicSignature(signature) Then
                Throw New IOException("signature is not valid")
            End If

            Me.version = [in].readByte()

            If Me.version > 0 Then
                Throw New IOException("version not implemented")
            End If

            Me.reserved0 = [in].readBytes(3)
            Me.totalLocalHeapSize = 8
            Me.dataSegmentSize = ReadHelper.readL([in], sb)
            Me.offsetToHeadOfFreeList = ReadHelper.readL([in], sb)
            Me.totalLocalHeapSize += sb.sizeOfLengths * 2
            Me.addressOfDataSegment = ReadHelper.readO([in], sb)
            Me.totalLocalHeapSize += sb.sizeOfOffsets

            ' data
            [in].offset = Me.addressOfDataSegment

            Me.data = [in].readBytes(CInt(Me.dataSegmentSize))
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Dim signature = Encoding.ASCII.GetBytes(magic)

            console.WriteLine("LocalHeap >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("signature : " &
                              (signature(0) And &HFF).ToString("x") &
                              (signature(1) And &HFF).ToString("x") &
                              (signature(2) And &HFF).ToString("x") &
                              (signature(3) And &HFF).ToString("x")
                             )

            console.WriteLine("version : " & Me.version)
            console.WriteLine("data segment size : " & Me.dataSegmentSize)
            console.WriteLine("offset to head of free list : " & Me.offsetToHeadOfFreeList)
            console.WriteLine("address of data segment : " & Me.addressOfDataSegment)

            console.WriteLine("total local heap size : " & Me.totalLocalHeapSize)

            If Me.data IsNot Nothing Then
                For i As Integer = 0 To Me.data.Length - 1
                    console.WriteLine("data[" & i & "] : " & Me.data(i))
                Next
            End If

            console.WriteLine("LocalHeap <<<")
        End Sub

        Public Function getString(offset As Integer) As String
            Dim count As Integer = 0

            While Me.data(offset + count) <> 0
                count += 1
            End While

            Return Me.data.ByteString(offset, count)
        End Function

        Public Overrides Function ToString() As String
            Return data.Split(4) _
                .Select(Function(int) BitConverter.ToInt32(int, Scan0)) _
                .Select(Function(n)
                            If n = 0 Then
                                Return " "c
                            Else
                                Return ChrW(n)
                            End If
                        End Function) _
                .JoinBy("")
        End Function
    End Class

End Namespace
