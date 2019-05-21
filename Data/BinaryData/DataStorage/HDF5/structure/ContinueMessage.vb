#Region "Microsoft.VisualBasic::311aeb8a671cf1341a3f7fc0df33a715, mime\application%netcdf\HDF5\structure\ContinueMessage.vb"

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

    '     Class ContinueMessage
    ' 
    '         Properties: address, length, offset, totalObjectHeaderMessageContinueSize
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


    Public Class ContinueMessage
        Private m_address As Long
        Private m_offset As Long
        Private m_length As Long

        Private m_totalObjectHeaderMessageContinueSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)

            [in].offset = address

            Me.m_address = address
            Me.m_offset = ReadHelper.readO([in], sb)
            Me.m_length = ReadHelper.readL([in], sb)

            Me.m_totalObjectHeaderMessageContinueSize = sb.sizeOfOffsets + sb.sizeOfLengths
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property offset() As Long
            Get
                Return Me.m_offset
            End Get
        End Property

        Public Overridable ReadOnly Property length() As Long
            Get
                Return Me.m_length
            End Get
        End Property

        Public Overridable ReadOnly Property totalObjectHeaderMessageContinueSize() As Integer
            Get
                Return Me.m_totalObjectHeaderMessageContinueSize
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("ObjectHeaderMessageContinue >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("offset : " & Me.m_offset)
            Console.WriteLine("length : " & Me.m_length)
            Console.WriteLine("total header message continue size : " & Me.m_totalObjectHeaderMessageContinueSize)
            Console.WriteLine("ObjectHeaderMessageContinue <<<")
        End Sub
    End Class

End Namespace
