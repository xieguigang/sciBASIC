#Region "Microsoft.VisualBasic::ddec37ca2ce4c3dbaace17afadf3a8df, Data\BinaryData\BinaryData\Bzip2\Algorithm\BZip2BlockDecompressor.vb"

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

    '   Total Lines: 403
    '    Code Lines: 188 (46.65%)
    ' Comment Lines: 131 (32.51%)
    '    - Xml Docs: 9.16%
    ' 
    '   Blank Lines: 84 (20.84%)
    '     File Size: 19.83 KB


    '     Class BZip2BlockDecompressor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CheckCrc, decodeNextBWTByte, (+2 Overloads) Read, ReadHuffmanTables
    ' 
    '         Sub: DecodeHuffmanData, InitialiseInverseBWT
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports Microsoft.VisualBasic.Data.IO.Bzip2.Math
Imports std = System.Math

Namespace Bzip2
    ''' <summary> Reads and decompresses a single BZip2 block </summary>
    ''' <remarks>
    ''' Block decoding consists of the following stages:
    ''' 1. Read block header - BZip2BlockDecompressor()
    ''' 2. Read Huffman tables - readHuffmanTables()
    ''' 3. Read and decode Huffman encoded data - decodeHuffmanData()
    ''' 4. Run-Length Decoding[2] - decodeHuffmanData()
    ''' 5. Inverse Move To Front Transform - decodeHuffmanData()
    ''' 6. Inverse Burrows Wheeler Transform - initialiseInverseBWT()
    ''' 7. Run-Length Decoding[1] - read()
    ''' 8. Optional Block De-Randomisation - read() (through decodeNextBWTByte())
    ''' </remarks>
    Friend Class BZip2BlockDecompressor
#Region "Private fields"
        ' *
        '  The BZip2 specification originally included the optional addition of a slight pseudo-random
        '  perturbation to the input data, in order to work around the block sorting algorithm's non-
        '  optimal performance on some types of input. The current mainline bzip2 does not require this
        '  and will not create randomised blocks, but compatibility is still required for old data (and
        '  third party compressors that haven't caught up). When decompressing a randomised block, for
        '  each value N in this array, a 1 will be XOR'd onto the output of the Burrows-Wheeler
        '  transform stage after N bytes, then the next N taken from the following entry.
        ' 

        Private Shared ReadOnly RNUMS As Integer() = {619, 720, 127, 481, 931, 816, 813, 233, 566, 247, 985, 724, 205, 454, 863, 491, 741, 242, 949, 214, 733, 859, 335, 708, 621, 574, 73, 654, 730, 472, 419, 436, 278, 496, 867, 210, 399, 680, 480, 51, 878, 465, 811, 169, 869, 675, 611, 697, 867, 561, 862, 687, 507, 283, 482, 129, 807, 591, 733, 623, 150, 238, 59, 379, 684, 877, 625, 169, 643, 105, 170, 607, 520, 932, 727, 476, 693, 425, 174, 647, 73, 122, 335, 530, 442, 853, 695, 249, 445, 515, 909, 545, 703, 919, 874, 474, 882, 500, 594, 612, 641, 801, 220, 162, 819, 984, 589, 513, 495, 799, 161, 604, 958, 533, 221, 400, 386, 867, 600, 782, 382, 596, 414, 171, 516, 375, 682, 485, 911, 276, 98, 553, 163, 354, 666, 933, 424, 341, 533, 870, 227, 730, 475, 186, 263, 647, 537, 686, 600, 224, 469, 68, 770, 919, 190, 373, 294, 822, 808, 206, 184, 943, 795, 384, 383, 461, 404, 758, 839, 887, 715, 67, 618, 276, 204, 918, 873, 777, 604, 560, 951, 160, 578, 722, 79, 804, 96, 409, 713, 940, 652, 934, 970, 447, 318, 353, 859, 672, 112, 785, 645, 863, 803, 350, 139, 93, 354, 99, 820, 908, 609, 772, 154, 274, 580, 184, 79, 626, 630, 742, 653, 282, 762, 623, 680, 81, 927, 626, 789, 125, 411, 521, 938, 300, 821, 78, 343, 175, 128, 250, 170, 774, 972, 275, 999, 639, 495, 78, 352, 126, 857, 956, 358, 619, 580, 124, 737, 594, 701, 612, 669, 112, 134, 694, 363, 992, 809, 743, 168, 974, 944, 375, 748, 52, 600, 747, 642, 182, 862, 81, 344, 805, 988, 739, 511, 655, 814, 334, 249, 515, 897, 955, 664, 981, 649, 113, 974, 459, 893, 228, 433, 837, 553, 268, 926, 240, 102, 654, 459, 51, 686, 754, 806, 760, 493, 403, 415, 394, 687, 700, 946, 670, 656, 610, 738, 392, 760, 799, 887, 653, 978, 321, 576, 617, 626, 502, 894, 679, 243, 440, 680, 879, 194, 572, 640, 724, 926, 56, 204, 700, 707, 151, 457, 449, 797, 195, 791, 558, 945, 679, 297, 59, 87, 824, 713, 663, 412, 693, 342, 606, 134, 108, 571, 364, 631, 212, 174, 643, 304, 329, 343, 97, 430, 751, 497, 314, 983, 374, 822, 928, 140, 206, 73, 263, 980, 736, 876, 478, 430, 305, 170, 514, 364, 692, 829, 82, 855, 953, 676, 246, 369, 970, 294, 750, 807, 827, 150, 790, 288, 923, 804, 378, 215, 828, 592, 281, 565, 555, 710, 82, 896, 831, 547, 261, 524, 462, 293, 465, 502, 56, 661, 821, 976, 991, 658, 869, 905, 758, 745, 193, 768, 550, 608, 933, 378, 286, 215, 979, 792, 961, 61, 688, 793, 644, 986, 403, 106, 366, 905, 644, 372, 567, 466, 434, 645, 210, 389, 550, 919, 135, 780, 773, 635, 389, 707, 100, 626, 958, 165, 504, 920, 176, 193, 713, 857, 265, 203, 50, 668, 108, 645, 990, 626, 197, 510, 357, 358, 850, 858, 364, 936, 638}

        ' Maximum possible number of Huffman table selectors
        Private Const HUFFMAN_MAXIMUM_SELECTORS As Integer = 900000 / BZip2HuffmanStageEncoder.HUFFMAN_GROUP_RUN_LENGTH + 1

        ' Provides bits of input to decode
        Private ReadOnly bitInputStream As BZip2BitInputStream

        ' Calculates the block CRC from the fully decoded bytes of the block
        Private ReadOnly crc As CRC32 = New CRC32()

        ' The CRC of the current block as read from the block header
        Private ReadOnly blockCRC As UInteger

        ' true if the current block is randomised, otherwise false
        Private ReadOnly blockRandomised As Boolean

        ' Huffman Decoding stage 

        ' The end-of-block Huffman symbol. Decoding of the block ends when this is encountered
        Private huffmanEndOfBlockSymbol As Integer

        ' *
        '  A map from Huffman symbol index to output character. Some types of data (e.g. ASCII text)
        '  may contain only a limited number of byte values; Huffman symbols are only allocated to
        '  those values that actually occur in the uncompressed data.
        ' 

        Private ReadOnly huffmanSymbolMap As Byte() = New Byte(255) {}

        ' Move To Front stage 

        ' *
        '  Counts of each byte value within the bwtTransformedArray data. Collected at the Move
        '  To Front stage, consumed by the Inverse Burrows Wheeler Transform stage
        ' 

        Private ReadOnly bwtByteCounts As Integer() = New Integer(255) {}

        ' *
        '  The Burrows-Wheeler Transform processed data. Read at the Move To Front stage, consumed by the
        '  Inverse Burrows Wheeler Transform stage 
        ' 

        Private bwtBlock As Byte()

        ' Inverse Burrows-Wheeler Transform stage 

        ' *
        '  At each position contains the union of :-
        '    An output character (8 bits)
        '    A pointer from each position to its successor (24 bits, left shifted 8 bits)
        '  As the pointer cannot exceed the maximum block size of 900k, 24 bits is more than enough to
        '  hold it; Folding the character data into the spare bits while performing the inverse BWT,
        '  when both pieces of information are available, saves a large number of memory accesses in
        '  the final decoding stages.
        ' 

        Private bwtMergedPointers As Integer()

        ' The current merged pointer into the Burrow-Wheeler Transform array
        Private bwtCurrentMergedPointer As Integer

        ' *
        '  The actual length in bytes of the current block at the Inverse Burrows Wheeler Transform
        '  stage (before final Run-Length Decoding)
        ' 

        Private bwtBlockLength As Integer

        ' The number of output bytes that have been decoded up to the Inverse Burrows Wheeler Transform stage
        Private bwtBytesDecoded As Integer

        ' Run-Length Encoding and Random Perturbation stage 

        ' The most recently RLE decoded byte
        Private rleLastDecodedByte As Integer = -1

        ' *
        '  The number of previous identical output bytes decoded. After 4 identical bytes, the next byte
        '  decoded is an RLE repeat count
        ' 

        Private rleAccumulator As Integer

        ' The RLE repeat count of the current decoded byte. When this reaches zero, a new byte is decoded
        Private rleRepeat As Integer

        ' If the current block is randomised, the position within the RNUMS randomisation array
        Private randomIndex As Integer

        ' If the current block is randomised, the remaining count at the current RNUMS position
        Private randomCount As Integer = RNUMS(0) - 1
#End Region

#Region "Public fields"
        ' Minimum number of alternative Huffman tables
        Public Const HUFFMAN_MINIMUM_TABLES As Integer = 2

        ' Maximum number of alternative Huffman tables
        Public Const HUFFMAN_MAXIMUM_TABLES As Integer = 6
#End Region

#Region "Private methods"
        ' *
        '  Read and decode the block's Huffman tables
        '  @return A decoder for the Huffman stage that uses the decoded tables
        '  Exception if the input stream reaches EOF before all table data has been read
        ' 

        Private Function ReadHuffmanTables() As BZip2HuffmanStageDecoder
            Dim tableCodeLengths = New Byte(5, 257) {}

            ' Read Huffman symbol to output byte map 
            Dim huffmanUsedRanges = bitInputStream.ReadBits(16)
            Dim huffmanSymbolCount = 0

            For i = 0 To 16 - 1
                If (huffmanUsedRanges And 1 << 15 >> i) = 0 Then Continue For
                Dim j = 0, k = i << 4

                While j < 16

                    If bitInputStream.ReadBoolean() Then
                        huffmanSymbolMap(std.Min(Threading.Interlocked.Increment(huffmanSymbolCount), huffmanSymbolCount - 1)) = CByte(k)
                    End If

                    j += 1
                    k += 1
                End While
            Next

            Dim endOfBlockSymbol = huffmanSymbolCount + 1
            huffmanEndOfBlockSymbol = endOfBlockSymbol

            ' Read total number of tables and selectors
            Dim totalTables = bitInputStream.ReadBits(3)
            Dim totalSelectors = bitInputStream.ReadBits(15)

            If totalTables < HUFFMAN_MINIMUM_TABLES OrElse totalTables > HUFFMAN_MAXIMUM_TABLES OrElse totalSelectors < 1 OrElse totalSelectors > HUFFMAN_MAXIMUM_SELECTORS Then
                Throw New Exception("BZip2 block Huffman tables invalid")
            End If

            ' Read and decode MTFed Huffman selector list 
            Dim tableMTF = New MoveToFront()
            Dim selectors = New Byte(totalSelectors - 1) {}

            For selector = 0 To totalSelectors - 1
                selectors(selector) = tableMTF.IndexToFront(CInt(bitInputStream.ReadUnary()))
            Next

            ' Read the Canonical Huffman code lengths for each table 
            For table = 0 To totalTables - 1
                Dim currentLength As Integer = bitInputStream.ReadBits(5)

                For i = 0 To endOfBlockSymbol

                    While bitInputStream.ReadBoolean()
                        currentLength += If(bitInputStream.ReadBoolean(), -1, 1)
                    End While

                    tableCodeLengths(table, i) = CByte(currentLength)
                Next
            Next

            Return New BZip2HuffmanStageDecoder(bitInputStream, endOfBlockSymbol + 1, tableCodeLengths, selectors)
        End Function

        ' *
        '  Reads the Huffman encoded data from the input stream, performs Run-Length Decoding and
        '  applies the Move To Front transform to reconstruct the Burrows-Wheeler Transform array
        '  @param huffmanDecoder The Huffman decoder through which symbols are read
        '  Exception if an end-of-block symbol was not decoded within the declared block size
        ' 

        Private Sub DecodeHuffmanData(huffmanDecoder As BZip2HuffmanStageDecoder)
            Dim symbolMTF = New MoveToFront()
            Dim _bwtBlockLength = 0
            Dim repeatCount = 0
            Dim repeatIncrement = 1
            Dim mtfValue = 0

            While True
                Dim nextSymbol = huffmanDecoder.NextSymbol()

                If nextSymbol = BZip2MTFAndRLE2StageEncoder.RLE_SYMBOL_RUNA Then
                    repeatCount += repeatIncrement
                    repeatIncrement <<= 1
                ElseIf nextSymbol = BZip2MTFAndRLE2StageEncoder.RLE_SYMBOL_RUNB Then
                    repeatCount += repeatIncrement << 1
                    repeatIncrement <<= 1
                Else
                    Dim nextByte As Byte

                    If repeatCount > 0 Then
                        If _bwtBlockLength + repeatCount > bwtBlock.Length Then Throw New Exception("BZip2 block exceeds declared block size")
                        nextByte = huffmanSymbolMap(mtfValue)
                        bwtByteCounts(nextByte And &HFF) += repeatCount

                        While Threading.Interlocked.Decrement(repeatCount) >= 0
                            bwtBlock(std.Min(Threading.Interlocked.Increment(_bwtBlockLength), _bwtBlockLength - 1)) = nextByte
                        End While

                        repeatCount = 0
                        repeatIncrement = 1
                    End If

                    If nextSymbol = huffmanEndOfBlockSymbol Then Exit While
                    If _bwtBlockLength >= bwtBlock.Length Then Throw New Exception("BZip2 block exceeds declared block size")
                    mtfValue = symbolMTF.IndexToFront(nextSymbol - 1) And &HFF
                    nextByte = huffmanSymbolMap(mtfValue)
                    bwtByteCounts(nextByte And &HFF) += 1
                    bwtBlock(std.Min(Threading.Interlocked.Increment(_bwtBlockLength), _bwtBlockLength - 1)) = nextByte
                End If
            End While

            bwtBlockLength = _bwtBlockLength
        End Sub

        ' *
        '  Set up the Inverse Burrows-Wheeler Transform merged pointer array
        '  @param bwtStartPointer The start pointer into the BWT array
        '  Exception if the given start pointer is invalid
        ' 

        Private Sub InitialiseInverseBWT(bwtStartPointer As UInteger)
            Dim _bwtMergedPointers = New Integer(bwtBlockLength - 1) {}
            Dim characterBase = New Integer(255) {}
            If bwtStartPointer < 0 OrElse bwtStartPointer >= bwtBlockLength Then Throw New Exception("BZip2 start pointer invalid")

            ' Cumulatise character counts
            Array.ConstrainedCopy(bwtByteCounts, 0, characterBase, 1, 255)

            For i = 2 To 255
                characterBase(i) += characterBase(i - 1)
            Next

            ' Merged-Array Inverse Burrows-Wheeler Transform
            ' Combining the output characters and forward pointers into a single array here, where we
            ' have already read both of the corresponding values, cuts down on memory accesses in the
            ' final walk through the array
            For i = 0 To bwtBlockLength - 1
                Dim value = bwtBlock(i) And &HFF
                _bwtMergedPointers(std.Min(Threading.Interlocked.Increment(characterBase(value)), characterBase(value) - 1)) = (i << 8) + value
            Next

            bwtBlock = Nothing
            bwtMergedPointers = _bwtMergedPointers
            bwtCurrentMergedPointer = _bwtMergedPointers(bwtStartPointer)
        End Sub

        ' *
        '  Decodes a byte from the Burrows-Wheeler Transform stage. If the block has randomisation
        '  applied, reverses the randomisation
        '  @return The decoded byte
        ' 

        Private Function decodeNextBWTByte() As Integer
            Dim nextDecodedByte = bwtCurrentMergedPointer And &HFF
            bwtCurrentMergedPointer = bwtMergedPointers(bwtCurrentMergedPointer >> 8)

            If blockRandomised Then
                If Threading.Interlocked.Decrement(randomCount) = 0 Then
                    nextDecodedByte = nextDecodedByte Xor 1
                    randomIndex = (randomIndex + 1) Mod 512
                    randomCount = RNUMS(randomIndex)
                End If
            End If

            bwtBytesDecoded += 1
            Return nextDecodedByte
        End Function
#End Region

#Region "Public methods"
        ' *
        '  Public constructor
        '  @param bitInputStream The BZip2BitInputStream to read from
        '  @param blockSize The maximum decoded size of the block
        '  Exception If the block could not be decoded
        ' 

        Public Sub New(bitInputStream As BZip2BitInputStream, blockSize As UInteger)
            Me.bitInputStream = bitInputStream
            bwtBlock = New Byte(blockSize - 1) {}

            ' Read block header
            blockCRC = Me.bitInputStream.ReadInteger()  '.ReadBits(32);
            blockRandomised = Me.bitInputStream.ReadBoolean()
            Dim bwtStartPointer = Me.bitInputStream.ReadBits(24)

            ' Read block data and decode through to the Inverse Burrows Wheeler Transform stage
            Dim huffmanDecoder = ReadHuffmanTables()
            DecodeHuffmanData(huffmanDecoder)
            InitialiseInverseBWT(bwtStartPointer)
        End Sub

        ' *
        '  Decodes a byte from the final Run-Length Encoding stage, pulling a new byte from the
        '  Burrows-Wheeler Transform stage when required
        '  @return The decoded byte, or -1 if there are no more bytes
        ' 

        Public Function Read() As Integer
            While rleRepeat < 1
                If bwtBytesDecoded = bwtBlockLength Then Return -1
                Dim nextByte As Integer = decodeNextBWTByte()

                If nextByte <> rleLastDecodedByte Then
                    ' New byte, restart accumulation
                    rleLastDecodedByte = nextByte
                    rleRepeat = 1
                    rleAccumulator = 1
                    crc.UpdateCrc(nextByte)
                Else

                    If Threading.Interlocked.Increment(rleAccumulator) = 4 Then
                        ' Accumulation complete, start repetition
                        Dim _rleRepeat As Integer = decodeNextBWTByte() + 1
                        rleRepeat = _rleRepeat
                        rleAccumulator = 0
                        crc.UpdateCrc(nextByte, _rleRepeat)
                    Else
                        rleRepeat = 1
                        crc.UpdateCrc(nextByte)
                    End If
                End If
            End While

            rleRepeat -= 1
            Return rleLastDecodedByte
        End Function

        ' *
        '  Decodes multiple bytes from the final Run-Length Encoding stage, pulling new bytes from the
        '  Burrows-Wheeler Transform stage when required
        '  @param destination The array to write to
        '  @param offset The starting position within the array
        '  @param length The number of bytes to read
        '  @return The number of bytes actually read, or -1 if there are no bytes left in the block
        ' 

        Public Function Read(destination As Byte(), offset As Integer, length As Integer) As Integer
            Dim i As Integer
            i = 0

            While i < length
                Dim decoded = Read()
                If decoded = -1 Then Return If(i = 0, -1, i)
                destination(offset) = CByte(decoded)
                i += 1
                offset += 1
            End While

            Return i
        End Function

        ' *
        '  Verify and return the block CRC. This method may only be called after all of the block's bytes have been read
        '  @return The block CRC
        '  Exception if the CRC verification failed
        ' 

        Public Function CheckCrc() As UInteger
            If blockCRC <> crc.CRC Then Throw New Exception("BZip2 block CRC error")
            Return crc.CRC
        End Function
#End Region
    End Class
End Namespace
