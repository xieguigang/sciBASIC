#Region "Microsoft.VisualBasic::827a4e281df612eb04044ede91a72ff8, Data\BinaryData\BinaryData\Bzip2\Algorithm\BZip2BlockCompressor.vb"

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

    '   Total Lines: 265
    '    Code Lines: 140
    ' Comment Lines: 76
    '   Blank Lines: 49
    '     File Size: 10.23 KB


    '     Class BZip2BlockCompressor
    ' 
    '         Properties: CRC, IsEmpty
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Write
    ' 
    '         Sub: Close, WriteRun, WriteSymbolMap
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports Microsoft.VisualBasic.Data.IO.Bzip2.Math
Imports stdNum = System.Math

Namespace Bzip2
    ''' <summary>Compresses and writes a single BZip2 block</summary>
    ''' <remarks>
    ''' Block encoding consists of the following stages:
	''' 1. Run-Length Encoding[1] - write()
    ''' 2. Burrows Wheeler Transform - close() (through BZip2DivSufSort)
    ''' 3. Write block header - close()
    ''' 4. Move To Front Transform - close() (through BZip2HuffmanStageEncoder)
    ''' 5. Run-Length Encoding[2] - close()  (through BZip2HuffmanStageEncoder)
    ''' 6. Create and write Huffman tables - close() (through BZip2HuffmanStageEncoder)
    ''' 7. Huffman encode and write data - close() (through BZip2HuffmanStageEncoder)
    ''' </remarks>
    Friend Class BZip2BlockCompressor
#Region "Private fields"
        ' The stream to which compressed BZip2 data is written
        Private ReadOnly bitOutputStream As BZip2BitOutputStream

        ' CRC builder for the block
        Private ReadOnly crcField As CRC32 = New CRC32()

        ' The RLE'd block data
        Private ReadOnly block As Byte()

        ' Current length of the data within the block array
        Private blockLength As Integer

        ' A limit beyond which new data will not be accepted into the block
        Private ReadOnly blockLengthLimit As Integer

        ' The values that are present within the RLE'd block data. For each index, true if that
        ' value is present within the data, otherwise false
        Private ReadOnly blockValuesPresent As Boolean() = New Boolean(255) {}

        ' The Burrows Wheeler Transformed block data
        Private ReadOnly bwtBlock As Integer()

        ' The current RLE value being accumulated (undefined when rleLength is 0)
        Private rleCurrentValue As Integer = -1

        ' The repeat count of the current RLE value
        Private rleLength As Integer
#End Region

#Region "Public fields"
        ' First three bytes of the block header marker
        Public Const BLOCK_HEADER_MARKER_1 As UInteger = &H314159

        ' Last three bytes of the block header marker
        Public Const BLOCK_HEADER_MARKER_2 As UInteger = &H265359
#End Region

#Region "Public properties"
        ' *
        '  Determines if any bytes have been written to the block
        '  @return true if one or more bytes has been written to the block, otherwise false
        ' 

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return blockLength = 0 AndAlso rleLength = 0
            End Get
        End Property

        ' *
        '  Gets the CRC of the completed block. Only valid after calling Close()
        '  @return The block's CRC
        ' 

        Public ReadOnly Property CRC As UInteger
            Get
                Return crcField.CRC
            End Get
        End Property
#End Region

#Region "Public methods"
        ' *
        '  Public constructor
        '  @param bitOutputStream The BZip2BitOutputStream to which compressed BZip2 data is written
        '  @param blockSize The declared block size in bytes. Up to this many bytes will be accepted
        '                   into the block after Run-Length Encoding is applied
        ' 

        Public Sub New(bitOutputStream As BZip2BitOutputStream, blockSize As Integer)
            Me.bitOutputStream = bitOutputStream

            ' One extra byte is added to allow for the block wrap applied in close()
            block = New Byte(blockSize + 1 - 1) {}
            bwtBlock = New Integer(blockSize + 1 - 1) {}
            blockLengthLimit = blockSize - 6 ' 5 bytes for one RLE run plus one byte - see Write(int)
        End Sub

        ' *
        '  Writes a byte to the block, accumulating to an RLE run where possible
        '  @param value The byte to write
        '  @return true if the byte was written, or false if the block is already full
        ' 

        Public Function Write(value As Integer) As Boolean
            If blockLength > blockLengthLimit Then Return False

            If rleLength = 0 Then
                rleCurrentValue = value
                rleLength = 1
            ElseIf rleCurrentValue <> value Then
                ' This path commits us to write 6 bytes - one RLE run (5 bytes) plus one extra
                WriteRun(rleCurrentValue And &HFF, rleLength)
                rleCurrentValue = value
                rleLength = 1
            Else

                If rleLength = 254 Then
                    WriteRun(rleCurrentValue And &HFF, 255)
                    rleLength = 0
                Else
                    rleLength += 1
                End If
            End If

            Return True
        End Function

        ' *
        '  Writes an array to the block
        '  @param data The array to write
        '  @param offset The offset within the input data to write from
        '  @param length The number of bytes of input data to write
        '  @return The actual number of input bytes written. May be less than the number requested, or
        '          zero if the block is already full
        ' 

        Public Function Write(data As Byte(), offset As Integer, length As Integer) As Integer
            Dim written = 0

            While stdNum.Max(Threading.Interlocked.Decrement(length), length + 1) > 0
                If Not Write(data(stdNum.Min(Threading.Interlocked.Increment(offset), offset - 1))) Then Exit While
                written += 1
            End While

            Return written
        End Function

        ' *
        '  Compresses and writes out the block
        '  Exception on any I/O error writing the data
        ' 

        Public Sub Close()
            ' If an RLE run is in progress, write it out
            If rleLength > 0 Then WriteRun(rleCurrentValue And &HFF, rleLength)

            ' Apply a one byte block wrap required by the BWT implementation
            block(blockLength) = block(0)

            ' Perform the Burrows Wheeler Transform
            Dim divSufSort = New BZip2DivSufSort(block, bwtBlock, blockLength)
            Dim bwtStartPointer = divSufSort.BWT()

            ' Write out the block header
            bitOutputStream.WriteBits(24, BLOCK_HEADER_MARKER_1)
            bitOutputStream.WriteBits(24, BLOCK_HEADER_MARKER_2)
            bitOutputStream.WriteInteger(crcField.CRC)
            bitOutputStream.WriteBoolean(False) ' Randomised block flag. We never create randomised blocks
            bitOutputStream.WriteBits(24, bwtStartPointer)

            ' Write out the symbol map
            WriteSymbolMap()

            ' Perform the Move To Front Transform and Run-Length Encoding[2] stages 
            Dim mtfEncoder = New BZip2MTFAndRLE2StageEncoder(bwtBlock, blockLength, blockValuesPresent)
            mtfEncoder.Encode()

            ' Perform the Huffman Encoding stage and write out the encoded data
            Dim huffmanEncoder = New BZip2HuffmanStageEncoder(bitOutputStream, mtfEncoder.MtfBlock, mtfEncoder.MtfLength, mtfEncoder.MtfAlphabetSize, mtfEncoder.MtfSymbolFrequencies)
            huffmanEncoder.Encode()
        End Sub
#End Region

#Region "Private methods"
        ' *
        '  Write the Huffman symbol to output byte map
        '  @Exception on any I/O error writing the data
        ' 

        Private Sub WriteSymbolMap()
            Dim condensedInUse = New Boolean(15) {}

            For i = 0 To 16 - 1
                Dim j = 0, k As Integer = i << 4

                While j < 16

                    If blockValuesPresent(k) Then
                        condensedInUse(i) = True
                    End If

                    j += 1
                    k += 1
                End While
            Next

            For i = 0 To 16 - 1
                bitOutputStream.WriteBoolean(condensedInUse(i))
            Next

            For i = 0 To 16 - 1

                If condensedInUse(i) Then
                    Dim j = 0, k As Integer = i * 16

                    While j < 16
                        bitOutputStream.WriteBoolean(blockValuesPresent(k))
                        j += 1
                        k += 1
                    End While
                End If
            Next
        End Sub

        ' *
        '  Writes an RLE run to the block array, updating the block CRC and present values array as required
        '  @param value The value to write
        '  @param runLength The run length of the value to write
        ' 

        Private Sub WriteRun(value As Integer, runLength As Integer)
            blockValuesPresent(value) = True
            crcField.UpdateCrc(value, runLength)
            Dim byteValue = CByte(value)

            Select Case runLength
                Case 1
                    block(blockLength) = byteValue
                    blockLength = blockLength + 1
                Case 2
                    block(blockLength) = byteValue
                    block(blockLength + 1) = byteValue
                    blockLength = blockLength + 2
                Case 3
                    block(blockLength) = byteValue
                    block(blockLength + 1) = byteValue
                    block(blockLength + 2) = byteValue
                    blockLength = blockLength + 3
                Case Else
                    runLength -= 4
                    blockValuesPresent(runLength) = True
                    block(blockLength) = byteValue
                    block(blockLength + 1) = byteValue
                    block(blockLength + 2) = byteValue
                    block(blockLength + 3) = byteValue
                    block(blockLength + 4) = CByte(runLength)
                    blockLength = blockLength + 5
            End Select
        End Sub
#End Region
    End Class
End Namespace
