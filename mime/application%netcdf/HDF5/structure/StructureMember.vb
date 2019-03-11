#Region "Microsoft.VisualBasic::3995e8445cbe97704732b31a020de828, mime\application%netcdf\HDF5\structure\StructureMember.vb"

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

    '     Class StructureMember
    ' 
    '         Properties: address, dims, message, name, offset
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

    Public Class StructureMember
        Private m_address As Long

        Private m_name As String
        Private m_offset As Integer
        Private m_dims As Integer
        Private m_message As DataTypeMessage

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long, version As Integer, byteSize As Integer)
            Me.m_address = address

            [in].offset = address

            Me.m_name = [in].readASCIIString()
            If version < 3 Then
                [in].skipBytes(ReadHelper.padding(Me.m_name.Length + 1, 8))
                Me.m_offset = [in].readInt()
            Else
                Me.m_offset = CInt(ReadHelper.readVariableSizeMax([in], byteSize))
            End If

            If version = 1 Then
                Me.m_dims = [in].readByte()
                [in].skipBytes(3)
                ' ignore dimension info for now
                [in].skipBytes(24)
            End If

            Me.m_message = New DataTypeMessage([in], sb, [in].offset)
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property name() As String
            Get
                Return Me.m_name
            End Get
        End Property

        Public Overridable ReadOnly Property offset() As Integer
            Get
                Return Me.m_offset
            End Get
        End Property

        Public Overridable ReadOnly Property dims() As Integer
            Get
                Return Me.m_dims
            End Get
        End Property

        Public Overridable ReadOnly Property message() As DataTypeMessage
            Get
                Return Me.m_message
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("StructureMember >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("name : " & Me.m_name)
            Console.WriteLine("offset : " & Me.m_offset)
            Console.WriteLine("m_dims : " & Me.m_dims)

            If Me.m_message IsNot Nothing Then
                Me.m_message.printValues()
            End If
            Console.WriteLine("StructureMember >>>")
        End Sub
    End Class

End Namespace
