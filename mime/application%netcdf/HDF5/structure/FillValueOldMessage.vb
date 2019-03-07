#Region "Microsoft.VisualBasic::c41576ef1573acd10be92f0a6f221f28, mime\application%netcdf\HDF5\structure\FillValueOldMessage.vb"

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

    '     Class FillValueOldMessage
    ' 
    '         Properties: address, size, value
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

    Public Class FillValueOldMessage
        Private m_address As Long
        Private m_size As Integer
        Private m_value As Byte()

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Me.m_size = [in].readInt()
            Me.m_value = [in].readBytes(Me.m_size)
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property size() As Integer
            Get
                Return Me.m_size
            End Get
        End Property

        Public Overridable ReadOnly Property value() As Byte()
            Get
                Return Me.m_value
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("FillValueOldMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("size : " & Me.m_size)

            Console.WriteLine("FillValueOldMessage <<<")
        End Sub
    End Class

End Namespace

