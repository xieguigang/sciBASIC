#Region "Microsoft.VisualBasic::4ff0825560eac68bff9a138cf0b7fca5, Data\BinaryData\DataStorage\HDF5\structure\DataChunkIterator.vb"

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
    ' 
    '         Function: [next], hasNext
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
Imports System.Runtime.CompilerServices
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.[Structure]

    Public Class DataChunkIterator : Inherits HDF5Ptr

        Dim root As DataNode

        Public Sub New([in] As BinaryReader, sb As Superblock, layout As Layout)
            Call MyBase.New(layout.dataAddress)

            [in].offset = Me.m_address

            Me.root = New DataNode([in], sb, layout, Me.m_address)
            Me.root.first([in], sb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function hasNext([in] As BinaryReader, sb As Superblock) As Boolean
            Return Me.root.hasNext([in], sb)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function [next]([in] As BinaryReader, sb As Superblock) As DataChunk
            Return Me.root.[next]([in], sb)
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Throw New NotImplementedException()
        End Sub
    End Class

End Namespace
