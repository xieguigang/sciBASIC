#Region "Microsoft.VisualBasic::cf5492f7947e4785dc438afffdfab74c, Data\BinaryData\BinaryData\Bzip2\BZip2InputStream.vb"

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

    '   Total Lines: 236
    '    Code Lines: 149
    ' Comment Lines: 43
    '   Blank Lines: 44
    '     File Size: 9.60 KB


    '     Class BZip2InputStream
    ' 
    '         Properties: CanRead, CanSeek, CanWrite, Length, Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: InitialiseNextBlock, Read, ReadByte, Seek
    ' 
    '         Sub: Close, Flush, InitialiseStream, SetLength, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports System.IO

Namespace Bzip2

    ''' <summary>An InputStream wrapper that decompresses BZip2 data</summary>
    ''' <remarks>Instances of this class are not threadsafe</remarks>
    Public Class BZip2InputStream
        Inherits Stream
#Region "Private fields"
        ' The stream from which compressed BZip2 data is read and decoded
        Private inputStream As Stream

        ' An InputStream wrapper that provides bit-level reads
        Private bitInputStream As BZip2BitInputStream

        ' If true, the caller is assumed to have read away the stream's leading "BZ" identifier bytes
        Private ReadOnly headerless As Boolean

        ' (@code true} if the end of the compressed stream has been reached, otherwise false
        Private streamComplete As Boolean

        ' *
        '  The declared block size of the stream (before final run-length decoding). The final block
        '  will usually be smaller, but no block in the stream has to be exactly this large, and an
        '  encoder could in theory choose to mix blocks of any size up to this value. Its function is
        '  therefore as a hint to the decompressor as to how much working space is sufficient to
        '  decompress blocks in a given stream
        ' 

        Private streamBlockSize As UInteger

        ' The merged CRC of all blocks decompressed so far
        Private streamCRC As UInteger

        ' The decompressor for the current block
        Private blockDecompressor As BZip2BlockDecompressor
#End Region

#Region "Public methods"
        ''' <summary>Public constructor</summary>
        ''' <param name="inputStream">The InputStream to wrap</param>
        ''' <param name="headerless">If true, the caller is assumed to have read away the stream's 
        ''' leading "BZ" identifier bytes</param>
        Public Sub New(inputStream As Stream, headerless As Boolean)
            If inputStream Is Nothing Then
                Throw New ArgumentException("Null input stream")
            End If

            Me.inputStream = inputStream
            Me.bitInputStream = New BZip2BitInputStream(inputStream)
            Me.headerless = headerless
        End Sub
#End Region

#Region "Implementation of abstract members of Stream"
        Public Overrides Sub Flush()
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Throw New NotImplementedException()
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return False
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

        Public Overrides Function ReadByte() As Integer
            Dim nextByte = -1

            If blockDecompressor Is Nothing Then
                InitialiseStream()
            Else
                nextByte = blockDecompressor.Read()
            End If

            If nextByte <> -1 Then Return nextByte
            If InitialiseNextBlock() Then nextByte = blockDecompressor.Read()
            Return nextByte
        End Function

        Public Overrides Function Read(destination As Byte(), offset As Integer, length As Integer) As Integer
            Dim bytesRead = -1

            If blockDecompressor Is Nothing Then
                InitialiseStream()
            Else
                bytesRead = blockDecompressor.Read(destination, offset, length)
            End If

            If bytesRead <> -1 Then Return bytesRead
            bytesRead = 0
            If InitialiseNextBlock() Then
                bytesRead = blockDecompressor.Read(destination, offset, length)
            End If

            Return bytesRead
        End Function

        Public Overrides Sub Close()
            If bitInputStream Is Nothing Then Return
            streamComplete = True
            blockDecompressor = Nothing
            bitInputStream = Nothing

            Try
                inputStream.Close()
            Finally
                inputStream = Nothing
            End Try
        End Sub
#End Region

#Region "Private methods"
        ''' <summary>Reads the stream header and checks that the data appears to be a valid BZip2 stream</summary>
        ''' <exception cref="IOException">if the stream header is not valid</exception>
        Private Sub InitialiseStream()
            ' If the stream has been explicitly closed, throw an exception 
            If bitInputStream Is Nothing Then
                Throw New Exception("Stream closed")
            End If

            ' If we're already at the end of the stream, do nothing 
            If streamComplete Then Return

            ' Read the stream header 
            Try
                Dim marker1 As UInteger = If(headerless, 0, bitInputStream.ReadBits(16))
                Dim marker2 = bitInputStream.ReadBits(8)
                Dim blockSize As UInteger = bitInputStream.ReadBits(8) - Asc("0"c)

                If Not headerless AndAlso marker1 <> BZip2OutputStream.STREAM_START_MARKER_1 OrElse marker2 <> BZip2OutputStream.STREAM_START_MARKER_2 OrElse blockSize < 1 OrElse blockSize > 9 Then
                    Throw New Exception("Invalid BZip2 header")
                End If

                streamBlockSize = blockSize * 100000
            Catch __unusedIOException1__ As IOException
                ' If the stream header was not valid, stop trying to read more data
                streamComplete = True
                Throw
            End Try
        End Sub

        ''' <summary>Prepares a new block for decompression if any remain in the stream</summary>
        ''' <remarks>If a previous block has completed, its CRC is checked and merged into the stream CRC.
        ''' If the previous block was the final block in the stream, the stream CRC is validated</remarks>
        ''' <return>true if a block was successfully initialised, or false if the end of file marker was encountered</return>
        ''' <exception cref="IOException">If either the block or stream CRC check failed, if the following data is
        ''' not a valid block-header or end-of-file marker, or if the following block could not be decoded</exception>
        Private Function InitialiseNextBlock() As Boolean

            ' If we're already at the end of the stream, do nothing 
            If streamComplete Then Return False

            ' If a block is complete, check the block CRC and integrate it into the stream CRC 
            If blockDecompressor IsNot Nothing Then
                Dim blockCRC As UInteger = blockDecompressor.CheckCrc()
                streamCRC = streamCRC << 1 Or streamCRC >> 31 Xor blockCRC
            End If

            ' Read block-header or end-of-stream marker 
            Dim marker1 = bitInputStream.ReadBits(24)
            Dim marker2 = bitInputStream.ReadBits(24)

            If marker1 = BZip2BlockCompressor.BLOCK_HEADER_MARKER_1 AndAlso marker2 = BZip2BlockCompressor.BLOCK_HEADER_MARKER_2 Then
                ' Initialise a new block
                Try
                    blockDecompressor = New BZip2BlockDecompressor(bitInputStream, streamBlockSize)
                Catch __unusedIOException1__ As IOException
                    ' If the block could not be decoded, stop trying to read more data
                    streamComplete = True
                    Throw
                End Try

                Return True
            End If

            If marker1 = BZip2OutputStream.STREAM_END_MARKER_1 AndAlso marker2 = BZip2OutputStream.STREAM_END_MARKER_2 Then
                ' Read and verify the end-of-stream CRC
                streamComplete = True
                Dim storedCombinedCRC As UInteger = bitInputStream.ReadInteger()
                ' .ReadBits(32);

                If storedCombinedCRC <> streamCRC Then Throw New Exception("BZip2 stream CRC error")
                Return False
            End If

            ' If what was read is not a valid block-header or end-of-stream marker, the stream is broken 
            streamComplete = True
            Throw New Exception("BZip2 stream format error")
        End Function
#End Region
    End Class
End Namespace
