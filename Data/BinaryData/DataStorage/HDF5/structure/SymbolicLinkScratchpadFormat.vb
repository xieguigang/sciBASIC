#Region "Microsoft.VisualBasic::0a49e7672acd12cdfc25ca8e378f4c94, Data\BinaryData\DataStorage\HDF5\structure\SymbolicLinkScratchpadFormat.vb"

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


Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]


    Public Class SymbolicLinkScratchpadFormat : Inherits HDF5Ptr

        Public Overridable ReadOnly Property offsetToLinkValue As Integer
        Public Overridable ReadOnly Property totalSymbolicLinkScratchpadFormatSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            Me.offsetToLinkValue = [in].readInt()
            Me.totalSymbolicLinkScratchpadFormatSize = 4
        End Sub

        Protected Friend Overrides Sub printValues(console As System.IO.StringWriter)
            console.WriteLine("SymbolicLinkScratchpadFormat >>>")
            console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("offset to link value : " & Me.offsetToLinkValue)

            Console.WriteLine("total symbolic link scratchpad format size : " & Me.totalSymbolicLinkScratchpadFormatSize)
            Console.WriteLine("SymbolicLinkScratchpadFormat <<<")
        End Sub
    End Class

End Namespace
