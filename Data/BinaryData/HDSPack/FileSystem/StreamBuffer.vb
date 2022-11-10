#Region "Microsoft.VisualBasic::e2dde5eb6e42e42a1c901cf497ec05a3, sciBASIC#\Data\BinaryData\HDSPack\FileSystem\StreamBuffer.vb"

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

    '   Total Lines: 109
    '    Code Lines: 71
    ' Comment Lines: 21
    '   Blank Lines: 17
    '     File Size: 3.24 KB


    '     Class StreamBuffer
    ' 
    '         Properties: CanRead, CanSeek, CanWrite, Length, Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Read, Seek
    ' 
    '         Sub: Dispose, Flush, SetLength, Write, writeBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace FileSystem

    ''' <summary>
    ''' an in-memory stream buffer for write new file data
    ''' </summary>
    ''' <remarks>
    ''' size is limited to 2GB
    ''' </remarks>
    Public Class StreamBuffer : Inherits Stream

        ''' <inheritdoc />
        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides ReadOnly Property Length As Long
            Get
                Return buffer.Length
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides Property Position As Long
            Get
                Return buffer.Position
            End Get
            Set(value As Long)
                buffer.Position = value
            End Set
        End Property

        ReadOnly basefile As Stream
        ReadOnly buffer As MemoryStream
        ReadOnly block As StreamBlock

        ''' <summary>
        ''' create a new temp stream for write new object data
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <param name="block"></param>
        ''' <param name="buffer_size"></param>
        Friend Sub New(buffer As Stream,
                       block As StreamBlock,
                       Optional buffer_size As Integer = 1024)

            Me.block = block
            Me.basefile = buffer
            Me.buffer = New MemoryStream(capacity:=buffer_size)
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub Flush()
            Call buffer.Flush()
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub SetLength(value As Long)
            Call buffer.SetLength(value)
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            Call Me.buffer.Write(buffer, offset, count)
        End Sub

        ''' <inheritdoc />
        Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
            Return Me.buffer.Read(buffer, offset, count)
        End Function

        ''' <inheritdoc />
        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Return buffer.Seek(offset, origin)
        End Function

        ''' <summary>
        ''' write the in-memory content data into 
        ''' the base file stream, and then update 
        ''' the stream block content data
        ''' </summary>
        Private Sub writeBuffer()
            block.size = buffer.Length
            block.offset = basefile.Length
            buffer.Flush()
            basefile.Position = block.offset
            basefile.Write(buffer.ToArray, offset:=Scan0, count:=buffer.Length)
            basefile.Flush()
        End Sub

        ''' <summary>
        ''' write the in-memory data to local file
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overrides Sub Dispose(disposing As Boolean)
            writeBuffer()
            buffer.Dispose()
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
