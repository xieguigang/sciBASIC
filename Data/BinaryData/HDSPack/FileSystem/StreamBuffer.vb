﻿#Region "Microsoft.VisualBasic::f156490eafb6100de28f9035958bc87e, G:/GCModeller/src/runtime/sciBASIC#/Data/BinaryData/HDSPack//FileSystem/StreamBuffer.vb"

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

    '   Total Lines: 186
    '    Code Lines: 108
    ' Comment Lines: 55
    '   Blank Lines: 23
    '     File Size: 6.54 KB


    '     Class StreamBuffer
    ' 
    '         Properties: CanRead, CanSeek, CanWrite, IsPreallocated, Length
    '                     Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Read, Seek
    ' 
    '         Sub: Dispose, Flush, InvalidBlockSize, SetLength, Write
    '              writeBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices

Namespace FileSystem

    ''' <summary>
    ''' an in-memory stream buffer for write new file data
    ''' </summary>
    ''' <remarks>
    ''' size is limited to 2GB, use the <see cref="IDisposable.Dispose()"/> method
    ''' for save the memory data to the underlying stream, and this dispose method
    ''' will not close the target base stream
    ''' </remarks>
    Public Class StreamBuffer : Inherits Stream

        ''' <inheritdoc />
        Public Overrides ReadOnly Property CanRead As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return True
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides ReadOnly Property CanSeek As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return True
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides ReadOnly Property CanWrite As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return True
            End Get
        End Property

        ''' <inheritdoc />
        Public Overrides ReadOnly Property Length As Long
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

        ''' <summary>
        ''' the file write behaviour is different at here based on the buffer 
        ''' information data in this file block reference object:
        ''' 
        ''' 1. if the file block information is empty: no position and no size, 
        '''    then file data will be append to the stream last
        ''' 2. if the file block information is not empty, then file data will 
        '''    be write to a specific location, and then length of the stream 
        '''    data is fixed!
        ''' </summary>
        ReadOnly block As StreamBlock

        ''' <summary>
        ''' current stream is fixed length?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsPreallocated As Boolean
            Get
                Return block.offset > 0 AndAlso block.size > 0
            End Get
        End Property

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
            Me.buffer.Seek(Scan0, SeekOrigin.Begin)
        End Sub

        ''' <inheritdoc />
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub Flush()
            Call buffer.Flush()
        End Sub

        ''' <inheritdoc />
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub SetLength(value As Long)
            If IsPreallocated Then
                If value > block.size Then
                    Call InvalidBlockSize(size:=value)
                End If
            End If

            Call buffer.SetLength(value)
        End Sub

        Private Sub InvalidBlockSize(size As Long)
            Throw New InvalidDataException($"the required length ({StringFormats.Lanudry(size)}) is greater than the pre-allocated size ({StringFormats.Lanudry(block.size)})!")
        End Sub

        ''' <inheritdoc />
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            Call Me.buffer.Write(buffer, offset, count)

            If IsPreallocated Then
                If Me.buffer.Length > block.size Then
                    Call InvalidBlockSize(size:=Me.buffer.Length)
                End If
            End If
        End Sub

        ''' <inheritdoc />
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
            Return Me.buffer.Read(buffer, offset, count)
        End Function

        ''' <inheritdoc />
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Dim intptr As Long = buffer.Seek(offset, origin)
            If IsPreallocated Then
                If intptr > block.size Then
                    Call InvalidBlockSize(size:=intptr)
                End If
            End If
            Return intptr
        End Function

        ''' <summary>
        ''' write the in-memory content data into 
        ''' the base file stream, and then update 
        ''' the stream block content data
        ''' </summary>
        Private Sub writeBuffer()
            If Not IsPreallocated Then
                block.size = buffer.Length
                block.offset = basefile.Length
            End If

            ' move current base file position to the file block
            ' start location, and then start to copy file block
            ' data to the underlying base file stream
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
