#Region "Microsoft.VisualBasic::810cf7879dc8dc9454150eb84a9e9e32, mime\application%netcdf\HDF5\structure\DataChunkIterator.vb"

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

    '     Class DataChunkIterator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [next], hasNext
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

    Public Class DataChunkIterator
        Private m_address As Long
        Private m_root As DataNode

        Public Sub New([in] As BinaryReader, sb As Superblock, layout As Layout)

            Me.m_address = layout.dataAddress

            [in].offset = Me.m_address

            Me.m_root = New DataNode([in], sb, layout, Me.m_address)
            Me.m_root.first([in], sb)
        End Sub

        Public Overridable Function hasNext([in] As BinaryReader, sb As Superblock) As Boolean
            Return Me.m_root.hasNext([in], sb)
        End Function

        Public Overridable Function [next]([in] As BinaryReader, sb As Superblock) As DataChunk
            Return Me.m_root.[next]([in], sb)
        End Function
    End Class

End Namespace
