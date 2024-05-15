#Region "Microsoft.VisualBasic::31887ab87bfa382e2d5ea4edadb38234, Data\BinaryData\BinaryData\Bzip2\BZip2OutputStream.vb"

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

    '   Total Lines: 196
    '    Code Lines: 132
    ' Comment Lines: 31
    '   Blank Lines: 33
    '     File Size: 8.16 KB


    '     Class BZip2OutputStream
    ' 
    '         Properties: CanRead, CanSeek, CanWrite, Length, Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Read, Seek
    ' 
    '         Sub: Close, CloseBlock, Finish, Flush, InitialiseNextBlock
    '              SetLength, Write, WriteByte
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports System.IO
Imports Microsoft.VisualBasic.Language

Namespace Bzip2
    ''' <summary>An OutputStream wrapper that compresses BZip2 data</summary>
    ''' <remarks>Instances of this class are not threadsafe</remarks>
    Public Class BZip2OutputStream
        Inherits Stream
#Region "Private fields"
        ' The stream to which compressed BZip2 data is written
        Private outputStream As Stream

        ' An OutputStream wrapper that provides bit-level writes
        Private ReadOnly bitOutputStream As BZip2BitOutputStream

        ' (@code true} if the compressed stream has been finished, otherwise false
        Private streamFinished As Boolean

        ' The declared maximum block size of the stream (before final run-length decoding)
        Private ReadOnly streamBlockSize As Integer

        ' The merged CRC of all blocks compressed so far
        Private streamCRC As UInteger

        ' The compressor for the current block
        Private blockCompressor As BZip2BlockCompressor

        ' True if the underlying stream will be closed with the current Stream
        Private isOwner As Boolean
#End Region

#Region "Public fields"
        ''' <summary>The first 2 bytes of a Bzip2 marker</summary> 
        Public Const STREAM_START_MARKER_1 As UInteger = &H425A

        ''' <summary>The 'h' that distinguishes BZip from BZip2</summary> 
        Public Const STREAM_START_MARKER_2 As UInteger = &H68

        ''' <summary>First three bytes of the end of stream marker</summary> 
        Public Const STREAM_END_MARKER_1 As UInteger = &H177245

        ''' <summary>Last three bytes of the end of stream marker</summary> 
        Public Const STREAM_END_MARKER_2 As UInteger = &H385090
#End Region

#Region "Public methods"
        ''' <summary>Public constructor</summary>
        ''' <param name="outputStream">The output stream to write to</param>
        ''' <param name="blockSizeMultiplier">The BZip2 block size as a multiple of 100,000 bytes (minimum 1, maximum 9)</param>
        ''' <param name="isOwner">True if the underlying stream will be closed with the current Stream</param>
        ''' <exception cref="ArgumentException">On any I/O error writing to the output stream</exception>
        ''' <remarks>Larger block sizes require more memory for both compression and decompression,
        ''' but give better compression ratios. 9 will usually be the best value to use</remarks>
        Public Sub New(outputStream As Stream, Optional isOwner As Boolean = True, Optional blockSizeMultiplier As Integer = 9)
            If outputStream Is Nothing Then Throw New ArgumentException("Null output stream")
            If blockSizeMultiplier < 1 OrElse blockSizeMultiplier > 9 Then Throw New ArgumentException("Invalid BZip2 block size" & blockSizeMultiplier)
            streamBlockSize = blockSizeMultiplier * 100000
            Me.outputStream = outputStream
            bitOutputStream = New BZip2BitOutputStream(Me.outputStream)
            Me.isOwner = isOwner
            bitOutputStream.WriteBits(16, STREAM_START_MARKER_1)
            bitOutputStream.WriteBits(8, STREAM_START_MARKER_2)
            bitOutputStream.WriteBits(8, CUInt(Asc("0"c) + blockSizeMultiplier))
            InitialiseNextBlock()
        End Sub
#End Region

#Region "Implementation of abstract members of Stream"
        Public Overrides Sub Flush()
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Throw New NotImplementedException()
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New NotImplementedException()
        End Sub

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property Length As Long
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides Property Position As Long
            Get
                Throw New NotImplementedException()
            End Get
            Set(value As Long)
                Throw New NotImplementedException()
            End Set
        End Property

        Public Overrides Sub WriteByte(value As Byte)
            If outputStream Is Nothing Then Throw New Exception("Stream closed")
            If streamFinished Then Throw New Exception("Write beyond end of stream")

            If Not blockCompressor.Write(value And &HFF) Then
                CloseBlock()
                InitialiseNextBlock()
                blockCompressor.Write(value And &HFF)
            End If
        End Sub

        Public Overrides Sub Write(data As Byte(), offset As Integer, length As Integer)
            Dim bytesWritten As Value(Of Integer) = 0

            If outputStream Is Nothing Then Throw New Exception("Stream closed")
            If streamFinished Then Throw New Exception("Write beyond end of stream")

            While length > 0
                If (bytesWritten = blockCompressor.Write(data, offset, length)) < length Then
                    CloseBlock()
                    InitialiseNextBlock()
                End If

                offset += CInt(bytesWritten)
                length -= CInt(bytesWritten)
            End While
        End Sub

        Public Overrides Sub Close()
            If outputStream IsNot Nothing Then
                Finish()
                If isOwner Then outputStream.Close()
                outputStream = Nothing
            End If
        End Sub
#End Region

#Region "Private methods"
        ''' <summary>Initialises a new block for compression</summary> 
        Private Sub InitialiseNextBlock()
            blockCompressor = New BZip2BlockCompressor(bitOutputStream, streamBlockSize)
        End Sub

        ''' <summary>Compress and write out the block currently in progress</summary>
        ''' <remarks>If no bytes have been written to the block, it is discarded</remarks>
        ''' <exception cref="Exception">On any I/O error writing to the output stream</exception>
        Private Sub CloseBlock()
            If blockCompressor.IsEmpty Then Return
            blockCompressor.Close()
            streamCRC = streamCRC << 1 Or streamCRC >> 31 Xor blockCompressor.CRC
        End Sub

        ''' <summary>Compresses and writes out any as yet unwritten data, then writes the end of the BZip2 stream</summary>
        ''' <remarks>The underlying OutputStream is not closed</remarks>
        ''' <exception cref="Exception">On any I/O error writing to the output stream</exception>
        Private Sub Finish()
            If Not streamFinished Then
                streamFinished = True

                Try
                    CloseBlock()
                    bitOutputStream.WriteBits(24, STREAM_END_MARKER_1)
                    bitOutputStream.WriteBits(24, STREAM_END_MARKER_2)
                    bitOutputStream.WriteInteger(streamCRC)
                    bitOutputStream.Flush()
                    outputStream.Flush()
                Finally
                    blockCompressor = Nothing
                End Try
            End If
        End Sub
#End Region
    End Class
End Namespace
