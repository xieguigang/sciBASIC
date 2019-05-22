#Region "Microsoft.VisualBasic::873613120db211d7fe05e9af36dfb990, Data\BinaryData\DataStorage\HDF5\structure\Infrastructure\BTree\BTreeEntry.vb"

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

    '     Class BTreeEntry
    ' 
    '         Properties: key, targetAddress
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace HDF5.[Structure].BTree


    Public Class BTreeEntry : Inherits HDF5Ptr

        Public Overridable ReadOnly Property targetAddress() As Long
        Public Overridable ReadOnly Property key() As Long

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.key = ReadHelper.readL([in], sb)
            Me.targetAddress = ReadHelper.readO([in], sb)
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("BTreeEntry >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("key : " & Me.key)
            console.WriteLine("target address : " & Me.targetAddress)
            console.WriteLine("BTreeEntry <<<")
        End Sub
    End Class

End Namespace
