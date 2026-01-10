#Region "Microsoft.VisualBasic::27e4d64dbbbef61a33e1639125d46c0a, Data\BinaryData\BinaryData\Bzip2\Algorithm\BZip2HuffmanStageEncoder.vb"

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

    '   Total Lines: 331
    '    Code Lines: 181 (54.68%)
    ' Comment Lines: 82 (24.77%)
    '    - Xml Docs: 3.66%
    ' 
    '   Blank Lines: 68 (20.54%)
    '     File Size: 14.72 KB


    '     Class BZip2HuffmanStageEncoder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: selectTableCount
    ' 
    '         Sub: assignHuffmanCodeSymbols, Encode, generateHuffmanCodeLengths, generateHuffmanOptimisationSeeds, optimiseSelectorsAndHuffmanTables
    '              writeBlockData, writeSelectorsAndHuffmanTables
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
    ''' <summary>
    ''' An encoder for the BZip2 Huffman encoding stage
    ''' </summary>	 
    Friend Class BZip2HuffmanStageEncoder
#Region "Private fields"
        ' Used in initial Huffman table generation
        Private Const HUFFMAN_HIGH_SYMBOL_COST As Integer = 15

        ' The longest Huffman code length created by the encoder
        Private Const HUFFMAN_ENCODE_MAXIMUM_CODE_LENGTH As Integer = 20

        ' Number of symbols decoded after which a new Huffman table is selected
        Public Const HUFFMAN_GROUP_RUN_LENGTH As Integer = 50

        ' The BZip2BitOutputStream to which the Huffman tables and data is written
        Private ReadOnly bitOutputStream As BZip2BitOutputStream

        ' The output of the Move To Front Transform and Run Length Encoding[2] stages
        Private ReadOnly mtfBlock As UShort()

        ' The actual number of values contained in the mtfBlock array
        Private ReadOnly mtfLength As Integer

        ' The number of unique values in the mtfBlock array
        Private ReadOnly mtfAlphabetSize As Integer

        ' The global frequencies of values within the mtfBlock array
        Private ReadOnly mtfSymbolFrequencies As Integer()

        ' The Canonical Huffman code lengths for each table
        Private ReadOnly huffmanCodeLengths As Integer(,)

        ' Merged code symbols for each table. The value at each position is ((code length << 24) | code)
        Private ReadOnly huffmanMergedCodeSymbols As Integer(,)

        ' The selectors for each segment
        Private ReadOnly selectors As Byte()
#End Region

#Region "Public methods"
        ' *
        '  Public constructor
        ' 		 * @param bitOutputStream The BZip2BitOutputStream to write to
        ' 		 * @param mtfBlock The MTF block data
        ' 		 * @param mtfLength The actual length of the MTF block
        ' 		 * @param mtfAlphabetSize The size of the MTF block's alphabet
        ' 		 * @param mtfSymbolFrequencies The frequencies the MTF block's symbols
        ' 

        Public Sub New(bitOutputStream As BZip2BitOutputStream, mtfBlock As UShort(), mtfLength As Integer, mtfAlphabetSize As Integer, mtfSymbolFrequencies As Integer())
            Me.bitOutputStream = bitOutputStream
            Me.mtfBlock = mtfBlock
            Me.mtfSymbolFrequencies = mtfSymbolFrequencies
            Me.mtfAlphabetSize = mtfAlphabetSize
            Me.mtfLength = mtfLength
            Dim totalTables = selectTableCount(mtfLength)
            huffmanCodeLengths = New Integer(totalTables - 1, mtfAlphabetSize - 1) {}
            huffmanMergedCodeSymbols = New Integer(totalTables - 1, mtfAlphabetSize - 1) {}
            selectors = New Byte((mtfLength + HUFFMAN_GROUP_RUN_LENGTH - 1) / HUFFMAN_GROUP_RUN_LENGTH - 1) {}
        End Sub

        ' *
        ' 		 * Encodes and writes the block data
        ' 		 * @Exception on any I/O error writing the data
        ' 

        Public Sub Encode()
            ' Create optimised selector list and Huffman tables
            generateHuffmanOptimisationSeeds()

            For i = 3 To 0 Step -1
                optimiseSelectorsAndHuffmanTables(i = 0)
            Next

            assignHuffmanCodeSymbols()

            ' Write out the tables and the block data encoded with them
            writeSelectorsAndHuffmanTables()
            writeBlockData()
        End Sub
#End Region

#Region "Private methods"
        ' 
        ' 		 * Selects an appropriate table count for a given MTF length
        ' 		 * @param mtfLength The length to select a table count for
        ' 		 * @return The selected table count
        ' 		 
        Private Shared Function selectTableCount(mtfLength As Integer) As Integer
            If mtfLength >= 2400 Then Return 6
            If mtfLength >= 1200 Then Return 5
            If mtfLength >= 600 Then Return 4
            Return If(mtfLength >= 200, 3, 2)
        End Function

        ' 
        ' 		 * Generate a Huffman code length table for a given list of symbol frequencies
        ' 		 * @param alphabetSize The total number of symbols
        ' 		 * @param symbolFrequencies The frequencies of the symbols
        ' 		 * @param codeLengths The array to which the generated code lengths should be written
        ' 		 
        Private Shared Sub generateHuffmanCodeLengths(alphabetSize As Integer, symbolFrequencies As Integer(,), codeLengths As Integer(,), index As Integer)
            Dim mergedFrequenciesAndIndices = New Integer(alphabetSize - 1) {}
            Dim sortedFrequencies = New Integer(alphabetSize - 1) {}

            ' The Huffman allocator needs its input symbol frequencies to be sorted, but we need to return code lengths in the same order as the
            ' corresponding frequencies are passed in

            ' The symbol frequency and index are merged into a single array of integers - frequency in the high 23 bits, index in the low 9 bits.
            '     2^23 = 8,388,608 which is higher than the maximum possible frequency for one symbol in a block
            '     2^9 = 512 which is higher than the maximum possible alphabet size (== 258)
            ' Sorting this array simultaneously sorts the frequencies and leaves a lookup that can be used to cheaply invert the sort
            For i = 0 To alphabetSize - 1
                mergedFrequenciesAndIndices(i) = symbolFrequencies(index, i) << 9 Or i
            Next

            Array.Sort(mergedFrequenciesAndIndices)

            For i = 0 To alphabetSize - 1
                sortedFrequencies(i) = mergedFrequenciesAndIndices(i) >> 9
            Next

            ' Allocate code lengths - the allocation is in place, so the code lengths will be in the sortedFrequencies array afterwards
            AllocateHuffmanCodeLengths(sortedFrequencies, HUFFMAN_ENCODE_MAXIMUM_CODE_LENGTH)

            ' Reverse the sort to place the code lengths in the same order as the symbols whose frequencies were passed in
            For i = 0 To alphabetSize - 1
                codeLengths(index, mergedFrequenciesAndIndices(i) And &H1FF) = sortedFrequencies(i)
            Next
        End Sub

        ' 
        ' 		 * Generate initial Huffman code length tables, giving each table a different low cost section
        ' 		 * of the alphabet that is roughly equal in overall cumulative frequency. Note that the initial
        ' 		 * tables are invalid for actual Huffman code generation, and only serve as the seed for later
        ' 		 * iterative optimisation in optimiseSelectorsAndHuffmanTables(int)
        ' 		 
        Private Sub generateHuffmanOptimisationSeeds()
            Dim totalTables = huffmanCodeLengths.GetLength(0)
            Dim remainingLength = mtfLength
            Dim lowCostEnd = -1

            For i = 0 To totalTables - 1
                Dim targetCumulativeFrequency As Integer = remainingLength / (totalTables - i)
                Dim lowCostStart = lowCostEnd + 1
                Dim actualCumulativeFrequency = 0

                While actualCumulativeFrequency < targetCumulativeFrequency AndAlso lowCostEnd < mtfAlphabetSize - 1
                    actualCumulativeFrequency += mtfSymbolFrequencies(Threading.Interlocked.Increment(lowCostEnd))
                End While

                If lowCostEnd > lowCostStart AndAlso i <> 0 AndAlso i <> totalTables - 1 AndAlso (totalTables - i And 1) = 0 Then
                    actualCumulativeFrequency -= mtfSymbolFrequencies(std.Max(Threading.Interlocked.Decrement(lowCostEnd), lowCostEnd + 1))
                End If

                For j = 0 To mtfAlphabetSize - 1
                    If j < lowCostStart OrElse j > lowCostEnd Then huffmanCodeLengths(i, j) = HUFFMAN_HIGH_SYMBOL_COST
                Next

                remainingLength -= actualCumulativeFrequency
            Next
        End Sub

        ' 
        ' 		 * Co-optimise the selector list and the alternative Huffman table code lengths. This method is
        ' 		 * called repeatedly in the hope that the total encoded size of the selectors, the Huffman code
        ' 		 * lengths and the block data encoded with them will converge towards a minimum.<br>
        ' 		 * If the data is highly incompressible, it is possible that the total encoded size will
        ' 		 * instead diverge (increase) slightly.<br>
        ' 		 * @param storeSelectors If true, write out the (final) chosen selectors
        ' 		 
        Private Sub optimiseSelectorsAndHuffmanTables(storeSelectors As Boolean)
            Dim totalTables = huffmanCodeLengths.GetLength(0)
            Dim tableFrequencies = New Integer(totalTables - 1, mtfAlphabetSize - 1) {}
            Dim selectorIndex = 0

            ' Find the best table for each group of 50 block bytes based on the current Huffman code lengths
            Dim groupStart = 0

            While groupStart < mtfLength
                Dim groupEnd = std.Min(groupStart + HUFFMAN_GROUP_RUN_LENGTH, mtfLength) - 1

                ' Calculate the cost of this group when encoded by each table
                Dim cost = New Integer(totalTables - 1) {}

                For i = groupStart To groupEnd
                    Dim value As Integer = mtfBlock(i)

                    For j = 0 To totalTables - 1
                        cost(j) += huffmanCodeLengths(j, value)
                    Next
                Next

                ' Find the table with the least cost for this group
                Dim bestTable As Byte = 0
                Dim bestCost = cost(0)

                For i As Byte = 1 To totalTables - 1
                    Dim tableCost = cost(i)

                    If tableCost < bestCost Then
                        bestCost = tableCost
                        bestTable = i
                    End If
                Next

                ' Accumulate symbol frequencies for the table chosen for this block
                For i = groupStart To groupEnd
                    tableFrequencies(bestTable, mtfBlock(i)) += 1
                Next

                ' Store a selector indicating the table chosen for this block
                If storeSelectors Then
                    selectors(std.Min(Threading.Interlocked.Increment(selectorIndex), selectorIndex - 1)) = bestTable
                End If

                groupStart = groupEnd + 1
            End While

            ' Generate new Huffman code lengths based on the frequencies for each table accumulated in this iteration
            For i = 0 To totalTables - 1
                generateHuffmanCodeLengths(mtfAlphabetSize, tableFrequencies, huffmanCodeLengths, i)
            Next
        End Sub

        ' Assigns Canonical Huffman codes based on the calculated lengths
        Private Sub assignHuffmanCodeSymbols()
            Dim totalTables = huffmanCodeLengths.GetLength(0)

            For i = 0 To totalTables - 1
                Dim minimumLength = 32
                Dim maximumLength = 0

                For j = 0 To mtfAlphabetSize - 1
                    Dim length = huffmanCodeLengths(i, j)

                    If length > maximumLength Then
                        maximumLength = length
                    End If

                    If length < minimumLength Then
                        minimumLength = length
                    End If
                Next

                Dim code = 0

                For j = minimumLength To maximumLength

                    For k = 0 To mtfAlphabetSize - 1

                        If (huffmanCodeLengths(i, k) And &HFF) = j Then
                            huffmanMergedCodeSymbols(i, k) = j << 24 Or code
                            code += 1
                        End If
                    Next

                    code <<= 1
                Next
            Next
        End Sub

        ' *
        ' 		 * Write out the selector list and Huffman tables
        ' 		 * @Exception on any I/O error writing the data
        ' 

        Private Sub writeSelectorsAndHuffmanTables()
            Dim totalSelectors = selectors.Length
            Dim totalTables = huffmanCodeLengths.GetLength(0)
            bitOutputStream.WriteBits(3, totalTables)
            bitOutputStream.WriteBits(15, totalSelectors)

            ' Write the selectors
            Dim selectorMTF = New MoveToFront()

            For i = 0 To totalSelectors - 1
                bitOutputStream.WriteUnary(selectorMTF.ValueToFront(selectors(i)))
            Next

            ' Write the Huffman tables
            For i = 0 To totalTables - 1
                Dim currentLength = huffmanCodeLengths(i, 0)
                bitOutputStream.WriteBits(5, currentLength)

                For j = 0 To mtfAlphabetSize - 1
                    Dim codeLength = huffmanCodeLengths(i, j)
                    Dim value = If(currentLength < codeLength, 2UI, 3UI)
                    Dim delta = std.Abs(codeLength - currentLength)

                    While std.Max(Threading.Interlocked.Decrement(delta), delta + 1) > 0
                        bitOutputStream.WriteBits(2, value)
                    End While

                    bitOutputStream.WriteBoolean(False)
                    currentLength = codeLength
                Next
            Next
        End Sub

        ' *
        ' 		 * Writes out the encoded block data
        ' 		 * @Exception on any I/O error writing the data
        ' 

        Private Sub writeBlockData()
            Dim selectorIndex = 0
            Dim mtfIndex = 0

            While mtfIndex < mtfLength
                Dim groupEnd = std.Min(mtfIndex + HUFFMAN_GROUP_RUN_LENGTH, mtfLength) - 1
                Dim index As Integer = selectors(std.Min(Threading.Interlocked.Increment(selectorIndex), selectorIndex - 1))

                While mtfIndex <= groupEnd
                    Dim mergedCodeSymbol = huffmanMergedCodeSymbols(index, mtfBlock(std.Min(Threading.Interlocked.Increment(mtfIndex), mtfIndex - 1)))
                    bitOutputStream.WriteBits(mergedCodeSymbol >> 24, mergedCodeSymbol)
                End While
            End While
        End Sub
#End Region
    End Class
End Namespace
