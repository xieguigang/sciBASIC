#Region "Microsoft.VisualBasic::d1da835377081e2b5f809752f35c8994, Data\BinaryData\DataStorage\HDF5\structure\DataBTree.vb"

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

'     Class DataBTree
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: getChunkIterator, ToString
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

Namespace HDF5.struct

    Public Class DataBTree

        ReadOnly layout As Layout

        Public Sub New(layout As Layout)
            Me.layout = layout
        End Sub

        Public Function getChunkIterator(sb As Superblock) As DataChunkIterator
            Return New DataChunkIterator(sb, Me.layout)
        End Function

        Public Overrides Function ToString() As String
            Return layout.ToString
        End Function
    End Class

End Namespace
