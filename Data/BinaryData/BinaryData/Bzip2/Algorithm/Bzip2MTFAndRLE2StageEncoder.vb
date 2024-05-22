#Region "Microsoft.VisualBasic::9f56cbd58ab573460eba7aadda0b0404, Data\BinaryData\BinaryData\Bzip2\Algorithm\Bzip2MTFAndRLE2StageEncoder.vb"

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

    '   Total Lines: 176
    '    Code Lines: 112 (63.64%)
    ' Comment Lines: 31 (17.61%)
    '    - Xml Docs: 16.13%
    ' 
    '   Blank Lines: 33 (18.75%)
    '     File Size: 6.87 KB


    '     Class BZip2MTFAndRLE2StageEncoder
    ' 
    '         Properties: MtfAlphabetSize, MtfBlock, MtfLength, MtfSymbolFrequencies
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Encode
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
    ''' <summary>An encoder for the BZip2 Move To Front Transform and Run-Length Encoding[2] stages</summary>
    ''' <remarks>
    ''' An encoder for the BZip2 Move To Front Transform and Run-Length Encoding[2] stages.
    ''' Although conceptually these two stages are separate, it is computationally efficient to perform them in one pass.
    ''' </remarks>
    Friend Class BZip2MTFAndRLE2StageEncoder

        ' Gets The actual length of the MTF block

        ' Gets the size of the MTF block's alphabet
        Private _MtfLength As Integer, _MtfAlphabetSize As Integer
#Region "Private fields"
        ' The Burrows-Wheeler transformed block
        Private ReadOnly bwtBlock As Integer()

        ' Actual length of the data in the bwtBlock array
        Private ReadOnly bwtLength As Integer

        ' At each position, true if the byte value with that index is present within the block, otherwise false 
        Private ReadOnly bwtValuesInUse As Boolean()

        ' The output of the Move To Front Transform and Run-Length Encoding[2] stages
        Private ReadOnly mtfBlockField As UShort()

        ' The global frequencies of values within the mtfBlock array
        Private ReadOnly mtfSymbolFrequenciesField As Integer() = New Integer(257) {}
#End Region

#Region "Public fields"
        ' Maximum possible Huffman alphabet size
        Public Const HUFFMAN_MAXIMUM_ALPHABET_SIZE As Integer = 258

        ' Huffman symbol used for run-length encoding
        Public Const RLE_SYMBOL_RUNA As UShort = 0

        ' Huffman symbol used for run-length encoding
        Public Const RLE_SYMBOL_RUNB As UShort = 1
#End Region

#Region "Public properties"
        ' Gets the encoded MTF block
        Public ReadOnly Property MtfBlock As UShort()
            Get
                Return mtfBlockField
            End Get
        End Property

        Public Property MtfLength As Integer
            Get
                Return _MtfLength
            End Get
            Private Set(value As Integer)
                _MtfLength = value
            End Set
        End Property

        Public Property MtfAlphabetSize As Integer
            Get
                Return _MtfAlphabetSize
            End Get
            Private Set(value As Integer)
                _MtfAlphabetSize = value
            End Set
        End Property

        ' Gets the frequencies of the MTF block's symbols
        Public ReadOnly Property MtfSymbolFrequencies As Integer()
            Get
                Return mtfSymbolFrequenciesField
            End Get
        End Property
#End Region

#Region "Public methods"
        ' *
        '  Public constructor
        ' 		 * @param bwtBlock The Burrows Wheeler Transformed block data
        ' 		 * @param bwtLength The actual length of the BWT data
        ' 		 * @param bwtValuesPresent The values that are present within the BWT data. For each index,
        ' 		 *            true if that value is present within the data, otherwise false
        ' 

        Public Sub New(bwtBlock As Integer(), bwtLength As Integer, bwtValuesPresent As Boolean())
            Me.bwtBlock = bwtBlock
            Me.bwtLength = bwtLength
            bwtValuesInUse = bwtValuesPresent
            mtfBlockField = New UShort(bwtLength + 1 - 1) {}
        End Sub

        ' Performs the Move To Front transform and Run Length Encoding[1] stages
        Public Sub Encode()
            Dim huffmanSymbolMap = New Byte(255) {}
            Dim symbolMTF = New MoveToFront()
            Dim totalUniqueValues = 0

            For i = 0 To 256 - 1
                If bwtValuesInUse(i) Then huffmanSymbolMap(i) = CByte(stdNum.Min(Threading.Interlocked.Increment(totalUniqueValues), totalUniqueValues - 1))
            Next

            Dim endOfBlockSymbol = totalUniqueValues + 1
            Dim mtfIndex = 0
            Dim repeatCount = 0
            Dim totalRunAs = 0
            Dim totalRunBs = 0

            For i = 0 To bwtLength - 1
                ' Move To Front
                Dim mtfPosition = symbolMTF.ValueToFront(huffmanSymbolMap(bwtBlock(i) And &HFF))

                ' Run Length Encode
                If mtfPosition = 0 Then
                    repeatCount += 1
                Else

                    If repeatCount > 0 Then
                        repeatCount -= 1

                        While True

                            If (repeatCount And 1) = 0 Then
                                mtfBlockField(stdNum.Min(Threading.Interlocked.Increment(mtfIndex), mtfIndex - 1)) = RLE_SYMBOL_RUNA
                                totalRunAs += 1
                            Else
                                mtfBlockField(stdNum.Min(Threading.Interlocked.Increment(mtfIndex), mtfIndex - 1)) = RLE_SYMBOL_RUNB
                                totalRunBs += 1
                            End If

                            If repeatCount <= 1 Then Exit While
                            repeatCount = repeatCount - 2 >> 1
                        End While

                        repeatCount = 0
                    End If

                    mtfBlockField(stdNum.Min(Threading.Interlocked.Increment(mtfIndex), mtfIndex - 1)) = (mtfPosition + 1)
                    mtfSymbolFrequenciesField(mtfPosition + 1) += 1
                End If
            Next

            If repeatCount > 0 Then
                repeatCount -= 1

                While True

                    If (repeatCount And 1) = 0 Then
                        mtfBlockField(stdNum.Min(Threading.Interlocked.Increment(mtfIndex), mtfIndex - 1)) = RLE_SYMBOL_RUNA
                        totalRunAs += 1
                    Else
                        mtfBlockField(stdNum.Min(Threading.Interlocked.Increment(mtfIndex), mtfIndex - 1)) = RLE_SYMBOL_RUNB
                        totalRunBs += 1
                    End If

                    If repeatCount <= 1 Then Exit While
                    repeatCount = repeatCount - 2 >> 1
                End While
            End If

            mtfBlockField(mtfIndex) = CUShort(endOfBlockSymbol)
            mtfSymbolFrequenciesField(endOfBlockSymbol) += 1
            mtfSymbolFrequenciesField(RLE_SYMBOL_RUNA) += totalRunAs
            mtfSymbolFrequenciesField(RLE_SYMBOL_RUNB) += totalRunBs
            MtfLength = mtfIndex + 1
            MtfAlphabetSize = endOfBlockSymbol + 1
        End Sub
#End Region
    End Class
End Namespace
