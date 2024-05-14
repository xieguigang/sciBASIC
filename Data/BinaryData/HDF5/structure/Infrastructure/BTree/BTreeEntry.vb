#Region "Microsoft.VisualBasic::49926b555242b3d5810c86196797cfe1, Data\BinaryData\HDF5\structure\Infrastructure\BTree\BTreeEntry.vb"

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

    '   Total Lines: 44
    '    Code Lines: 25
    ' Comment Lines: 6
    '   Blank Lines: 13
    '     File Size: 1.32 KB


    '     Class BTreeEntry
    ' 
    '         Properties: key, targetAddress
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct.BTree


    Public Class BTreeEntry : Inherits HDF5Ptr

        Public ReadOnly Property targetAddress As Long
        Public ReadOnly Property key As Long

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.key = ReadHelper.readL([in], sb)
            Me.targetAddress = ReadHelper.readO([in], sb)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString} [{key} => &{targetAddress}]"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("BTreeEntry >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("key : " & Me.key)
            console.WriteLine("target address : " & Me.targetAddress)
            console.WriteLine("BTreeEntry <<<")
        End Sub
    End Class

End Namespace
