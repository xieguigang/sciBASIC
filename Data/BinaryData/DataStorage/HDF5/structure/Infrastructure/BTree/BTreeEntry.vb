#Region "Microsoft.VisualBasic::8838bbefb42cf7b9c7411e621936725d, Data\BinaryData\DataStorage\HDF5\structure\Infrastructure\BTree\BTreeEntry.vb"

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



Imports Microsoft.VisualBasic.Data.IO.HDF5.IO

Namespace HDF5.[Structure].BTree


    Public Class BTreeEntry : Inherits HDF5Ptr


        Private m_key As Long
        Private m_targetAddress As Long

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.m_key = ReadHelper.readL([in], sb)
            Me.m_targetAddress = ReadHelper.readO([in], sb)
        End Sub

        Public Overridable ReadOnly Property targetAddress() As Long
            Get
                Return Me.m_targetAddress
            End Get
        End Property

        Public Overridable ReadOnly Property key() As Long
            Get
                Return Me.m_key
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("BTreeEntry >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("key : " & Me.m_key)
            Console.WriteLine("target address : " & Me.m_targetAddress)
            Console.WriteLine("BTreeEntry <<<")
        End Sub
    End Class

End Namespace
