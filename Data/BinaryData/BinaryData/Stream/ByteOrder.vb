#Region "Microsoft.VisualBasic::9b77a98a0c261ad51ee6f1144ee680d4, ..\sciBASIC#\Data\BinaryData\BinaryData\ByteOrder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

''' <summary>
''' Represents the possible endianness of binary data.
''' </summary>
Public Enum ByteOrder As UShort
	''' <summary>
	''' The binary data is present in big endian.
	''' </summary>
	BigEndian = &Hfeff

	''' <summary>
	''' The binary data is present in little endian.
	''' </summary>
	LittleEndian = &Hfffe
End Enum

''' <summary>
''' Represents helper methods to handle data byte order.
''' </summary>
Public Module ByteOrderHelper

    Dim _systemByteOrder As ByteOrder

    ''' <summary>
    ''' Gets the <see cref="ByteOrder"/> of the system executing the assembly.
    ''' </summary>
    Public ReadOnly Property SystemByteOrder() As ByteOrder
        Get
            If _systemByteOrder = 0 Then
                _systemByteOrder = If(BitConverter.IsLittleEndian, ByteOrder.LittleEndian, ByteOrder.BigEndian)
            End If
            Return _systemByteOrder
        End Get
    End Property
End Module
