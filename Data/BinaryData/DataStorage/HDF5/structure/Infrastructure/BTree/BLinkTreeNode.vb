#Region "Microsoft.VisualBasic::f98b033a0994661b5a4918aff14e3a3b, Data\BinaryData\DataStorage\HDF5\structure\Infrastructure\BTree\BLinkTreeNode.vb"

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

    '     Class BLinkTreeNode
    ' 
    '         Properties: signature, totalBLinkTreeNodeSize, validSignature
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Language
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct.BTree


    Public Class BLinkTreeNode

        Public Shared ReadOnly BLINKTREENODE_SIGNATURE As Byte() = New CharStream() From {"T"c, "R"c, "E"c, "E"c}

        Private m_signature As Byte()
        Private m_nodeType As Integer
        Private m_nodeLevel As Integer
        Private m_entriesUsed As Integer

        Private m_addressOfLeftSibling As Long
        Private m_addressOfRightSibling As Long

        Private m_offsetToLocalHeap As List(Of Long)
        Private m_keyOfChild As List(Of Byte())

        Private m_addressOfChild As List(Of Long)

        Private m_totalBLinkTreeNodeSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock)

            ' signature
            Me.m_signature = New Byte(3) {}

            For i As Integer = 0 To 3
                Me.m_signature(i) = [in].readByte()
            Next

            If Not Me.validSignature Then
                Throw New IOException("signature is not valid")
            End If

            Me.m_nodeType = [in].readByte()
            Me.m_nodeLevel = [in].readByte()
            Me.m_entriesUsed = [in].readShort()

            Me.m_totalBLinkTreeNodeSize = 8

            Me.m_addressOfLeftSibling = ReadHelper.readO([in], sb)
            Me.m_addressOfRightSibling = ReadHelper.readO([in], sb)

            Me.m_totalBLinkTreeNodeSize += sb.sizeOfOffsets * 2

            Me.m_offsetToLocalHeap = New List(Of Long)()
            Me.m_keyOfChild = New List(Of Byte())()
            Me.m_addressOfChild = New List(Of Long)()

            For i As Integer = 0 To Me.m_entriesUsed - 1
                If Me.m_nodeType = 0 Then
                    Dim key As Long = ReadHelper.readL([in], sb)
                    Me.m_offsetToLocalHeap.Add(key)
                ElseIf Me.m_nodeType = 1 Then
                    Dim chunksize As Integer = [in].readInt()
                    Dim filtermask As Integer = [in].readInt()
                Else
                    Throw New IOException("node type is not implemented")
                End If
            Next
        End Sub

        Public  ReadOnly Property signature() As Byte()
            Get
                Return Me.m_signature
            End Get
        End Property

        Public  ReadOnly Property validSignature() As Boolean
            Get
                For i As Integer = 0 To 3
                    If Me.m_signature(i) <> BLINKTREENODE_SIGNATURE(i) Then
                        Return False
                    End If
                Next
                Return True
            End Get
        End Property

        Public  ReadOnly Property totalBLinkTreeNodeSize() As Integer
            Get
                Return Me.m_totalBLinkTreeNodeSize
            End Get
        End Property

        Public  Sub printValues()
            Console.WriteLine("BLinkTreeNode >>>")
            Console.WriteLine("signature : " & (Me.m_signature(0) And &HFF).ToString("x") & (Me.m_signature(1) And &HFF).ToString("x") & (Me.m_signature(2) And &HFF).ToString("x") & (Me.m_signature(3) And &HFF).ToString("x"))
            '
            '            System.out.println("version : " + this.m_version);
            '            System.out.println("data segment size : " + this.m_dataSegmentSize);
            '            System.out.println("offset to head of free list : " + this.m_offsetToHeadOfFreeList);
            '            System.out.println("address of data segment : " + this.m_addressOfDataSegment);
            '            
            '            System.out.println("total local heap size : " + this.m_totalLocalHeapSize);
            '            

            Console.WriteLine("BLinkTreeNode <<<")
        End Sub
    End Class

End Namespace
