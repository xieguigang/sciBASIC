#Region "Microsoft.VisualBasic::0b522ae3682d59e16f2330ff0ceb16ba, Data\BinaryData\BinaryData\XDR\Unpacker.vb"

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

    '     Class Unpacker
    ' 
    '         Properties: Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Read, unpack_double, unpack_int
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Xdr

    Public Class Unpacker : Implements IByteReader

        ReadOnly data As BinaryDataReader

        Public Property Position As Long
            Get
                Return data.Position
            End Get
            Set(value As Long)
                data.Position = value
            End Set
        End Property

        Sub New(data As BinaryDataReader)
            Me.data = data
        End Sub

        Public Function unpack_int() As Object
            Return XdrEncoding.DecodeInt32(Me)
        End Function

        Public Function unpack_double() As Object
            Return XdrEncoding.DecodeDouble(Me)
        End Function

        Public Function Read(count As UInteger) As Byte() Implements IByteReader.Read
            Return data.ReadBytes(count)
        End Function

        Public Function Read() As Byte Implements IByteReader.Read
            Return data.ReadByte
        End Function
    End Class
End Namespace
