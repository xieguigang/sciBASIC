#Region "Microsoft.VisualBasic::d0fe8152d882eb14e5dd42f1a84d91ee, mime\application%netcdf\HDF5\structure\LastModifiedMessage.vb"

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

    '     Class LastModifiedMessage
    ' 
    '         Properties: address, seconds, version
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

    Public Class LastModifiedMessage
        Private m_address As Long
        Private m_version As Integer
        Private m_seconds As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Me.m_version = [in].readByte()

            [in].skipBytes(3)

            Me.m_seconds = [in].readInt()
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property version() As Integer
            Get
                Return Me.m_version
            End Get
        End Property

        Public Overridable ReadOnly Property seconds() As Integer
            Get
                Return Me.m_seconds
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("LastModifiedMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("seconds : " & Me.m_seconds)

            Console.WriteLine("LastModifiedMessage <<<")
        End Sub
    End Class

End Namespace

