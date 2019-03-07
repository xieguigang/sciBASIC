#Region "Microsoft.VisualBasic::62b86db8efac97c8587be7787d9fca7d, mime\application%netcdf\HDF5\structure\SymbolicLinkScratchpadFormat.vb"

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

    '     Class SymbolicLinkScratchpadFormat
    ' 
    '         Properties: address, offsetToLinkValue, totalSymbolicLinkScratchpadFormatSize
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


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]


    Public Class SymbolicLinkScratchpadFormat
        Private m_address As Long
        Private m_offsetToLinkValue As Integer

        Private m_totalSymbolicLinkScratchpadFormatSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)

            [in].offset = address

            Me.m_address = address

            Me.m_offsetToLinkValue = [in].readInt()

            Me.m_totalSymbolicLinkScratchpadFormatSize = 4
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property offsetToLinkValue() As Integer
            Get
                Return Me.m_offsetToLinkValue
            End Get
        End Property

        Public Overridable ReadOnly Property totalSymbolicLinkScratchpadFormatSize() As Integer
            Get
                Return Me.m_totalSymbolicLinkScratchpadFormatSize
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("SymbolicLinkScratchpadFormat >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("offset to link value : " & Me.m_offsetToLinkValue)

            Console.WriteLine("total symbolic link scratchpad format size : " & Me.m_totalSymbolicLinkScratchpadFormatSize)
            Console.WriteLine("SymbolicLinkScratchpadFormat <<<")
        End Sub
    End Class

End Namespace

