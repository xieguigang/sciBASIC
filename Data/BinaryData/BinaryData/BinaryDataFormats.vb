#Region "Microsoft.VisualBasic::724512f670fd05b9e09ff8450b83aa03, ..\visualbasic_App\Data\BinaryData\BinaryData\BinaryDataFormats.vb"

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
''' Represents the set of formats of binary date and time encodings.
''' </summary>
Public Enum BinaryDateTimeFormat
	''' <summary>
	''' The <see cref="System.DateTime"/> has the time_t format of the C library.
	''' </summary>
	CTime

	''' <summary>
	''' The <see cref="System.DateTime"/> is stored as the ticks of a .NET <see cref="System.DateTime"/> instance.
	''' </summary>
	NetTicks
End Enum

''' <summary>
''' Represents the set of formats of binary string encodings.
''' </summary>
Public Enum BinaryStringFormat
	''' <summary>
	''' The string has a prefix of 1 byte determining the length of the string and no postfix.
	''' </summary>
	ByteLengthPrefix

	''' <summary>
	''' The string has a prefix of 2 bytes determining the length of the string and no postfix.
	''' </summary>
	WordLengthPrefix

	''' <summary>
	''' The string has a prefix of 4 bytes determining the length of the string and no postfix.
	''' </summary>
	DwordLengthPrefix

	''' <summary>
	''' The string has no prefix and is terminated with a byte of the value 0.
	''' </summary>
	ZeroTerminated

	''' <summary>
	''' The string has neither prefix nor postfix. This format is only valid for writing strings. For reading
	''' strings, the length has to be specified manually.
	''' </summary>
	NoPrefixOrTermination
End Enum

