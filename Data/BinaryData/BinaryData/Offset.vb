#Region "Microsoft.VisualBasic::d6abbfcc0b91e3371019651f7a899474, ..\sciBASIC#\Data\BinaryData\BinaryData\Offset.vb"

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
''' Represents a space of 4 bytes reserved in the underlying stream of a <see cref="BinaryDataWriter"/> which can
''' be comfortably satisfied later on.
''' </summary>
Public Class Offset

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Offset"/> class reserving an offset with the specified
    ''' <see cref="BinaryDataWriter"/> at the current position.
    ''' </summary>
    ''' <param name="writer__1">The <see cref="BinaryDataWriter"/> holding the stream in which the offset will be
    ''' reserved.</param>
    Public Sub New(writer__1 As BinaryDataWriter)
		Writer = writer__1
		Position = CUInt(Writer.Position)
		Writer.Position += 4
	End Sub

    ''' <summary>
    ''' Gets the <see cref="BinaryDataWriter"/> in which underlying stream the allocation is made.
    ''' </summary>
    Public Property Writer() As BinaryDataWriter
    ''' <summary>
    ''' Gets the address at which the allocation is made.
    ''' </summary>
    Public Property Position() As UInteger

    ''' <summary>
    ''' Satisfies the offset by writing the current position of the underlying stream at the reserved
    ''' <see cref="Position"/>, then seeking back to the current position.
    ''' </summary>
    Public Sub Satisfy()
		' Temporarily seek back to the allocation offset and write the previous address there, then seek back.
		Dim oldPosition As UInteger = CUInt(Writer.Position)
		Writer.Position = Position
		Writer.Write(oldPosition)
		Writer.Position = oldPosition
	End Sub
End Class
