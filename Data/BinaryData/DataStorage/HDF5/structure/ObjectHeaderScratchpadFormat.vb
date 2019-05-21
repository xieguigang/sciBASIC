#Region "Microsoft.VisualBasic::90e8afa939016d17373175dd0c19c2ae, Data\BinaryData\DataStorage\HDF5\structure\ObjectHeaderScratchpadFormat.vb"

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

    '     Class ObjectHeaderScratchpadFormat
    ' 
    '         Properties: address, addressOfBTree, addressOfNameHeap, totalObjectHeaderScratchpadFormatSize
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


Imports Microsoft.VisualBasic.Data.IO.HDF5.IO

Namespace HDF5.[Structure]

    Public Class ObjectHeaderScratchpadFormat
        Private m_address As Long
        Private m_addressOfBTree As Long
        Private m_addressOfNameHeap As Long

        Private m_totalObjectHeaderScratchpadFormatSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)

            [in].offset = address

            Me.m_address = address
            Me.m_addressOfBTree = ReadHelper.readO([in], sb)
            Me.m_addressOfNameHeap = ReadHelper.readO([in], sb)

            Me.m_totalObjectHeaderScratchpadFormatSize = sb.sizeOfOffsets * 2
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property addressOfBTree() As Long
            Get
                Return Me.m_addressOfBTree
            End Get
        End Property

        Public Overridable ReadOnly Property addressOfNameHeap() As Long
            Get
                Return Me.m_addressOfNameHeap
            End Get
        End Property

        Public Overridable ReadOnly Property totalObjectHeaderScratchpadFormatSize() As Integer
            Get
                Return Me.m_totalObjectHeaderScratchpadFormatSize
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("ObjectHeaderScratchpadFormat >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("address of BTree : " & Me.m_addressOfBTree)
            Console.WriteLine("address of name heap : " & Me.m_addressOfNameHeap)

            Console.WriteLine("total object header scratchpad format size : " & Me.m_totalObjectHeaderScratchpadFormatSize)
            Console.WriteLine("ObjectHeaderScratchpadFormat <<<")
        End Sub
    End Class

End Namespace
