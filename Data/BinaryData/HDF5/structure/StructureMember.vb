#Region "Microsoft.VisualBasic::32510d71b2aa25a3117da91d361acd99, Data\BinaryData\HDF5\structure\StructureMember.vb"

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

    '   Total Lines: 67
    '    Code Lines: 43
    ' Comment Lines: 7
    '   Blank Lines: 17
    '     File Size: 2.20 KB


    '     Class StructureMember
    ' 
    '         Properties: dims, message, name, offset
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    Public Class StructureMember : Inherits HDF5Ptr

        Public ReadOnly Property name As String
        Public ReadOnly Property offset As Integer
        Public ReadOnly Property dims As Integer
        Public ReadOnly Property message As DataTypeMessage

        Public Sub New(sb As Superblock, address As Long, version As Integer, byteSize As Integer)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.name = [in].readASCIIString()

            If version < 3 Then
                [in].skipBytes(ReadHelper.padding(Me.name.Length + 1, 8))
                Me.offset = [in].readInt()
            Else
                Me.offset = CInt(ReadHelper.readVariableSizeMax([in], byteSize))
            End If

            If version = 1 Then
                Me.dims = [in].readByte()

                [in].skipBytes(3)
                ' ignore dimension info for now
                [in].skipBytes(24)
            End If

            Me.message = New DataTypeMessage(sb, [in].offset)
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {message} = [&{address}, {offset}]"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("StructureMember >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("name : " & Me.name)
            console.WriteLine("offset : " & Me.offset)
            console.WriteLine("m_dims : " & Me.dims)

            If Me.message IsNot Nothing Then
                Me.message.printValues(console)
            End If

            console.WriteLine("StructureMember >>>")
        End Sub
    End Class

End Namespace
