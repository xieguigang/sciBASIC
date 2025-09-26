#Region "Microsoft.VisualBasic::a8e7592b49be1f155b22e6fef38e0986, Data\BinaryData\BinaryData\LZW.vb"

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

    '   Total Lines: 229
    '    Code Lines: 139 (60.70%)
    ' Comment Lines: 43 (18.78%)
    '    - Xml Docs: 93.02%
    ' 
    '   Blank Lines: 47 (20.52%)
    '     File Size: 8.59 KB


    '     Class LZW
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Compress, Decompress
    ' 
    '     Class Encoder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Compress, FindMatch
    ' 
    '         Sub: WriteCode
    ' 
    '     Class Decoder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Decompress, ReadCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace LZW

    ''' <summary>
    ''' LZW Based Decompressor - basic algorithm used as described on Mark Nelson's website  http://marknelson.us
    ''' </summary>
    Public MustInherit Class LZW

        ''' <summary>
        ''' maimxum bits allowed to read
        ''' </summary>
        Public Const MAX_BITS As Integer = 14
        ''' <summary>
        ''' hash bit to use with the hasing algorithm to find correct index
        ''' </summary>
        Public Const HASH_BIT As Integer = MAX_BITS - 8
        ''' <summary>
        ''' max value allowed based on max bits
        ''' </summary>
        Public Const MAX_VALUE As Integer = (1 << MAX_BITS) - 1
        ''' <summary>
        ''' max code possible
        ''' </summary>
        Public Const MAX_CODE As Integer = MAX_VALUE - 1
        ''' <summary>
        ''' must be bigger than the maximum allowed by maxbits and prime
        ''' </summary>
        Public Const TABLE_SIZE As Integer = 18041

        ''' <summary>
        ''' code table
        ''' </summary>
        Protected codeTable As Integer() = New Integer(18040) {}
        ''' <summary>
        ''' prefix table
        ''' </summary>
        Protected prefixTable As Integer() = New Integer(18040) {}
        ''' <summary>
        ''' character table
        ''' </summary>
        Protected charTable As Integer() = New Integer(18040) {}

        ''' <summary>
        ''' bit buffer to temporarily store bytes read from the files
        ''' </summary>
        Protected bitBuffer As ULong = 0
        ''' <summary>
        ''' counter for knowing how many bits are in the bit buffer
        ''' </summary>
        Protected bitCounter As Integer = 0

        Protected ReadOnly input As Stream

        Friend Sub New(input As Stream)
            Me.input = input
        End Sub

        Public Shared Function Compress(input As Stream, out As Stream) As Boolean
            Return New Encoder(input).Compress(out)
        End Function

        Public Shared Function Decompress(input As Stream, out As Stream) As Boolean
            Return New Decoder(input).Decompress(out)
        End Function

    End Class

    Friend Class Encoder : Inherits LZW

        Friend Sub New(input As Stream)
            Call MyBase.New(input)
        End Sub

        Public Overloads Function Compress(out As Stream) As Boolean
            Dim iNextCode = 256
            Dim iChar = 0, iString = 0, iIndex = 0

            For i = 0 To TABLE_SIZE - 1 'blank out table
                codeTable(i) = -1
            Next

            iString = input.ReadByte() 'get first code, will be 0-255 ascii char
            iChar = input.ReadByte()
            While iChar <> -1 'read until we reach end of file
                iIndex = FindMatch(iString, iChar) 'get correct index for prefix+char

                If codeTable(iIndex) <> -1 Then 'set string if we have something at that index
                    iString = codeTable(iIndex) 'insert new entry
                Else
                    If iNextCode <= MAX_CODE Then 'otherwise we insert into the tables
                        iNextCode += 1
                        codeTable(iIndex) = iNextCode  'insert and increment next code to use
                        prefixTable(iIndex) = iString
                        charTable(iIndex) = CByte(iChar)
                    End If

                    WriteCode(out, iString) 'output the data in the string
                    iString = iChar
                End If

                iChar = input.ReadByte()
            End While

            WriteCode(out, iString) 'output last code
            WriteCode(out, MAX_VALUE) 'output end of buffer
            WriteCode(out, 0) 'flush

            Return True
        End Function

        ''' <summary>
        ''' hasing function, tries to find index of prefix+char, 
        ''' if not found returns -1 to signify space available
        ''' </summary>
        ''' <param name="pPrefix"></param>
        ''' <param name="pChar"></param>
        ''' <returns></returns>
        Private Function FindMatch(pPrefix As Integer, pChar As Integer) As Integer
            Dim index = 0, offset = 0

            index = pChar << HASH_BIT Xor pPrefix

            offset = If(index = 0, 1, TABLE_SIZE - index)

            While True
                If codeTable(index) = -1 Then Return index

                If prefixTable(index) = pPrefix AndAlso charTable(index) = pChar Then Return index

                index -= offset
                If index < 0 Then index += TABLE_SIZE
            End While

            Throw New Exception("This exception will never throw!")
        End Function

        Private Sub WriteCode(pWriter As Stream, pCode As Integer)
            bitBuffer = bitBuffer Or CULng(pCode) << 32 - MAX_BITS - bitCounter 'make space and insert new code in buffer
            bitCounter += MAX_BITS 'increment bit counter

            While bitCounter >= 8 'write all the bytes we can
                Dim temp As Integer = CByte((bitBuffer >> 24) And 255UL)
                pWriter.WriteByte(CByte((bitBuffer >> 24) And 255UL)) 'write byte from bit buffer
                bitBuffer <<= 8 'remove written byte from buffer
                bitCounter -= 8 'decrement counter
            End While
        End Sub

    End Class

    Friend Class Decoder : Inherits LZW

        Friend Sub New(input As Stream)
            Call MyBase.New(input)
        End Sub

        Public Overloads Function Decompress(out As Stream) As Boolean
            Dim iNextCode = 256
            Dim iNewCode, iOldCode As Integer
            Dim bChar As Byte
            Dim iCurrentCode, iCounter As Integer
            Dim baDecodeStack = New Byte(18040) {}

            iOldCode = ReadCode(input)
            bChar = CByte(iOldCode)
            out.WriteByte(CByte(iOldCode)) 'write first byte since it is plain ascii

            iNewCode = ReadCode(input)

            While iNewCode <> MAX_VALUE 'read file all file
                If iNewCode >= iNextCode Then 'fix for prefix+chr+prefix+char+prefx special case
                    baDecodeStack(0) = bChar
                    iCounter = 1
                    iCurrentCode = iOldCode
                Else
                    iCounter = 0
                    iCurrentCode = iNewCode
                End If

                While iCurrentCode > 255 'decode string by cycling back through the prefixes
                    'lstDecodeStack.Add((byte)_iaCharTable[iCurrentCode]);
                    'iCurrentCode = _iaPrefixTable[iCurrentCode];
                    baDecodeStack(iCounter) = CByte(charTable(iCurrentCode))
                    Threading.Interlocked.Increment(iCounter)
                    If iCounter >= MAX_CODE Then Throw New Exception("oh crap")
                    iCurrentCode = prefixTable(iCurrentCode)
                End While

                baDecodeStack(iCounter) = CByte(iCurrentCode)
                bChar = baDecodeStack(iCounter) 'set last char used

                While iCounter >= 0 'write out decodestack
                    out.WriteByte(baDecodeStack(iCounter))
                    Threading.Interlocked.Decrement(iCounter)
                End While

                If iNextCode <= MAX_CODE Then 'insert into tables
                    prefixTable(iNextCode) = iOldCode
                    charTable(iNextCode) = bChar
                    Threading.Interlocked.Increment(iNextCode)
                End If

                iOldCode = iNewCode

                'if (reader.PeekChar() != 0)
                iNewCode = ReadCode(input)
            End While

            Return True
        End Function

        Private Function ReadCode(pReader As Stream) As Integer
            Dim iReturnVal As UInteger

            While bitCounter <= 24 'fill up buffer
                bitBuffer = bitBuffer Or CULng(pReader.ReadByte()) << 24 - bitCounter 'insert byte into buffer
                bitCounter += 8 'increment counter
            End While

            iReturnVal = CUInt(bitBuffer) >> 32 - MAX_BITS 'get last byte from buffer so we can return it
            bitBuffer <<= MAX_BITS 'remove it from buffer
            bitCounter -= MAX_BITS 'decrement bit counter

            Dim temp As Integer = CInt(iReturnVal)
            Return temp
        End Function
    End Class
End Namespace
