#Region "Microsoft.VisualBasic::cb2f420a4d5d6740e4f87388029535ba, Data\BinaryData\BinaryData\Bzip2\Algorithm\BZip2HuffmanStageDecoder.vb"

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

    '   Total Lines: 163
    '    Code Lines: 79
    ' Comment Lines: 49
    '   Blank Lines: 35
    '     File Size: 7.04 KB


    '     Class BZip2HuffmanStageDecoder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: NextSymbol
    ' 
    '         Sub: CreateHuffmanDecodingTables
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Bzip2 library for .net
' By Jaime Olivares
' Location: http://github.com/jaime-olivares/bzip2
' Ported from the Java implementation by Matthew Francis: https://github.com/MateuszBartosiewicz/bzip2

Imports stdNum = System.Math

Namespace Bzip2
    ''' <summary>
    ''' A decoder for the BZip2 Huffman coding stage
    ''' </summary>
    Friend Class BZip2HuffmanStageDecoder
#Region "Private fields"
        ' The BZip2BitInputStream from which Huffman codes are read
        Private ReadOnly bitInputStream As BZip2BitInputStream

        ' The longest Huffman code length accepted by the decoder
        Private Const HUFFMAN_DECODE_MAXIMUM_CODE_LENGTH As Integer = 23

        ' The Huffman table number to use for each group of 50 symbols
        Private ReadOnly selectors As Byte()

        ' The minimum code length for each Huffman table
        Private ReadOnly minimumLengths As Integer() = New Integer(5) {}

        ' *
        ' 	     * An array of values for each Huffman table that must be subtracted from the numerical value of
        ' 	     * a Huffman code of a given bit length to give its canonical code index
        ' 

        Private ReadOnly codeBases As Integer(,) = New Integer(5, 24) {}

        ' *
        ' 	     * An array of values for each Huffman table that gives the highest numerical value of a Huffman
        ' 	     * code of a given bit length
        ' 

        Private ReadOnly codeLimits As Integer(,) = New Integer(5, 23) {}

        ' A mapping for each Huffman table from canonical code index to output symbol
        Private ReadOnly codeSymbols As Integer(,) = New Integer(5, 257) {}

        ' The Huffman table for the current group
        Private currentTable As Integer

        ' The index of the current group within the selectors array
        Private groupIndex As Integer = -1

        ' The byte position within the current group. A new group is selected every 50 decoded bytes
        Private groupPosition As Integer = -1
#End Region

#Region "Public methods"
        ' *
        '  Public constructor
        ' 		 * @param bitInputStream The BZip2BitInputStream from which Huffman codes are read
        ' 		 * @param alphabetSize The total number of codes (uniform for each table)
        ' 		 * @param tableCodeLengths The Canonical Huffman code lengths for each table
        ' 		 * @param selectors The Huffman table number to use for each group of 50 symbols
        ' 

        Public Sub New(bitInputStream As BZip2BitInputStream, alphabetSize As Integer, tableCodeLengths As Byte(,), selectors As Byte())
            Me.bitInputStream = bitInputStream
            Me.selectors = selectors
            currentTable = Me.selectors(0)
            CreateHuffmanDecodingTables(alphabetSize, tableCodeLengths)
        End Sub

        ' *
        '  Decodes and returns the next symbol
        '  @return The decoded symbol
        '  Exception if the end of the input stream is reached while decoding
        ' 

        Public Function NextSymbol() As Integer
            ' Move to next group selector if required
            If Threading.Interlocked.Increment(groupPosition) Mod BZip2HuffmanStageEncoder.HUFFMAN_GROUP_RUN_LENGTH = 0 Then
                groupIndex += 1
                If groupIndex = selectors.Length Then Throw New Exception("Error decoding BZip2 block")
                currentTable = selectors(groupIndex) And &HfF
            End If

            Dim codeLength = minimumLengths(currentTable)
            Dim startBits As UInteger = bitInputStream.ReadBits(codeLength)
            Dim codeBits As UInteger = startBits

            ' Starting with the minimum bit length for the table, read additional bits one at a time
            ' until a complete code is recognised
            For codeLength = codeLength To HUFFMAN_DECODE_MAXIMUM_CODE_LENGTH
                If codeBits <= codeLimits(currentTable, codeLength) Then
                    ' Convert the code to a symbol index and return
                    Return codeSymbols(currentTable, codeBits - codeBases(currentTable, codeLength))
                End If

                codeBits = codeBits << 1 Or bitInputStream.ReadBits(1)
            Next

            ' A valid code was not recognised
            Throw New Exception("Error decoding BZip2 block")
        End Function
#End Region

#Region "Private methods"
        ' *
        ' 	     * Constructs Huffman decoding tables from lists of Canonical Huffman code lengths
        ' 	     * @param alphabetSize The total number of codes (uniform for each table)
        ' 	     * @param tableCodeLengths The Canonical Huffman code lengths for each table
        ' 

        Private Sub CreateHuffmanDecodingTables(alphabetSize As Integer, tableCodeLengths As Byte(,))
            Dim i As Integer

            For table = 0 To tableCodeLengths.GetLength(0) - 1
                Dim minimumLength = HUFFMAN_DECODE_MAXIMUM_CODE_LENGTH
                Dim maximumLength = 0

                ' Find the minimum and maximum code length for the table
                For i = 0 To alphabetSize - 1
                    maximumLength = stdNum.Max(tableCodeLengths(table, i), maximumLength)
                    minimumLength = stdNum.Min(tableCodeLengths(table, i), minimumLength)
                Next

                minimumLengths(table) = minimumLength

                ' Calculate the first output symbol for each code length
                For i = 0 To alphabetSize - 1
                    codeBases(table, tableCodeLengths(table, i) + 1) += 1
                Next

                For i = 1 To HUFFMAN_DECODE_MAXIMUM_CODE_LENGTH + 2 - 1
                    codeBases(table, i) += codeBases(table, i - 1)
                Next

                Dim code As Integer = 0

                ' Calculate the first and last Huffman code for each code length (codes at a given length are sequential in value)
                i = minimumLength

                While i <= maximumLength
                    Dim base1 = code
                    code += codeBases(table, i + 1) - codeBases(table, i)
                    codeBases(table, i) = base1 - codeBases(table, i)
                    codeLimits(table, i) = code - 1
                    code <<= 1
                    i += 1
                End While

                ' Populate the mapping from canonical code index to output symbol
                Dim bitLength = minimumLength, codeIndex = 0

                While bitLength <= maximumLength

                    For symbol = 0 To alphabetSize - 1
                        If tableCodeLengths(table, symbol) = bitLength Then codeSymbols(table, stdNum.Min(Threading.Interlocked.Increment(codeIndex), codeIndex - 1)) = symbol
                    Next

                    bitLength += 1
                End While
            Next
        End Sub
#End Region
    End Class
End Namespace
