#Region "Microsoft.VisualBasic::cfa3c982c53764999723842bd35f3d9e, Data\BinaryData\HDF5\structure\DataBTree.vb"

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

    '   Total Lines: 40
    '    Code Lines: 24 (60.00%)
    ' Comment Lines: 6 (15.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (25.00%)
    '     File Size: 1.13 KB


    '     Class DataBTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumerateChunks, getChunkIterator, ToString
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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.device

Namespace struct

    Public Class DataBTree

        ReadOnly layout As Layout

        Public Sub New(layout As Layout)
            Me.layout = layout
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getChunkIterator(sb As Superblock) As DataChunkIterator
            Return New DataChunkIterator(sb, Me.layout)
        End Function

        Public Iterator Function EnumerateChunks(sb As Superblock) As IEnumerable(Of DataChunk)
            Dim reader As DataChunkIterator = getChunkIterator(sb)
            Dim file As BinaryReader = sb.FileReader(-1)

            Do While reader.hasNext()
                Yield reader.next(file, sb)
            Loop
        End Function

        Public Overrides Function ToString() As String
            Return layout.ToString
        End Function
    End Class

End Namespace
