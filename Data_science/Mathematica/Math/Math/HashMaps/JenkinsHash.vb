#Region "Microsoft.VisualBasic::0882f4be53d688f86599a2f6344b3445, Data_science\Mathematica\Math\Math\HashMaps\JenkinsHash.vb"

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

    '   Total Lines: 212
    '    Code Lines: 109 (51.42%)
    ' Comment Lines: 74 (34.91%)
    '    - Xml Docs: 86.49%
    ' 
    '   Blank Lines: 29 (13.68%)
    '     File Size: 7.92 KB


    '     Module JenkinsHash
    ' 
    '         Function: [xor], add, byteToLong, fourByteToLong, (+3 Overloads) hash
    '                   leftShift, subtract
    ' 
    '         Sub: hashMix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace HashMaps

    ''' <summary>
    ''' Original source from http://256.com/sources/jenkins_hash_java/
    ''' 
    ''' <b>This is a Bob Jenkins hashing algorithm implementation</b>
    ''' &lt;br> 
    ''' These are functions for producing 32-bit hashes for hash table lookup.
    ''' hashword(), hashlittle(), hashlittle2(), hashbig(), mix(), and final()
    ''' are externally useful functions.  Routines to test the hash are included
    ''' if SELF_TEST is defined.  You can use this free for any purpose.  It's in
    ''' the public domain.  It has no warranty.
    ''' </summary>
    Public Module JenkinsHash

        ''' <summary>
        ''' max value to limit it to 4 bytes
        ''' </summary>
        Const MAX_VALUE As Long = &HFFFFFFFFL

        ''' <summary>
        ''' Convert a byte into a long value without making it negative. </summary>
        ''' <param name="b">
        ''' @return </param>
        Private Function byteToLong(b As SByte) As Long
            Dim val As Long = b And &H7F
            If (b And &H80) <> 0 Then val += 128
            Return val
        End Function

        ''' <summary>
        ''' Do addition and turn into 4 bytes. </summary>
        ''' <param name="val"> </param>
        ''' <param name="___add">
        ''' @return </param>
        Private Function add(val As Long, ___add As Long) As Long
            Return (val + ___add) And MAX_VALUE
        End Function

        ''' <summary>
        ''' Do subtraction and turn into 4 bytes. </summary>
        ''' <param name="val"> </param>
        ''' <param name="___subtract">
        ''' @return </param>
        Private Function subtract(val As Long, ___subtract As Long) As Long
            Return (val - ___subtract) And MAX_VALUE
        End Function

        ''' <summary>
        ''' Left shift val by shift bits and turn in 4 bytes. </summary>
        ''' <param name="val"> </param>
        ''' <param name="___xor">
        ''' @return </param>
        Private Function [xor](val As Long, [___xor] As Long) As Long
            Return (val Xor ___xor) And MAX_VALUE
        End Function

        ''' <summary>
        ''' Left shift val by shift bits.  Cut down to 4 bytes. </summary>
        ''' <param name="val"> </param>
        ''' <param name="shift">
        ''' @return </param>
        Private Function leftShift(val As Long, shift As Integer) As Long
            Return (val << shift) And MAX_VALUE
        End Function

        ''' <summary>
        ''' Convert 4 bytes from the buffer at offset into a long value. </summary>
        ''' <param name="bytes"> </param>
        ''' <param name="offset">
        ''' @return </param>
        Private Function fourByteToLong(bytes As SByte(), offset As Integer) As Long
            Return (byteToLong(bytes(offset + 0)) + (byteToLong(bytes(offset + 1)) << 8) + (byteToLong(bytes(offset + 2)) << 16) + (byteToLong(bytes(offset + 3)) << 24))
        End Function

        ''' <summary>
        ''' Mix up the values in the hash function.
        ''' </summary>
        Private Sub hashMix(ByRef a&, ByRef b&, ByRef c&)
            a = subtract(a, b)
            a = subtract(a, c)
            a = [xor](a, c >> 13)
            b = subtract(b, c)
            b = subtract(b, a)
            b = [xor](b, leftShift(a, 8))
            c = subtract(c, a)
            c = subtract(c, b)
            c = [xor](c, (b >> 13))
            a = subtract(a, b)
            a = subtract(a, c)
            a = [xor](a, (c >> 12))
            b = subtract(b, c)
            b = subtract(b, a)
            b = [xor](b, leftShift(a, 16))
            c = subtract(c, a)
            c = subtract(c, b)
            c = [xor](c, (b >> 5))
            a = subtract(a, b)
            a = subtract(a, c)
            a = [xor](a, (c >> 3))
            b = subtract(b, c)
            b = subtract(b, a)
            b = [xor](b, leftShift(a, 10))
            c = subtract(c, a)
            c = subtract(c, b)
            c = [xor](c, (b >> 15))
        End Sub

        ''' <summary>
        ''' Hash a variable-length key into a 32-bit value.  Every bit of the
        ''' key affects every bit of the return value.  Every 1-bit and 2-bit
        ''' delta achieves avalanche.  The best hash table sizes are powers of 2.
        ''' </summary>
        ''' <param name="buffer">       Byte array that we are hashing on. </param>
        ''' <param name="initialValue"> Initial value of the hash if we are continuing from
        '''                     a previous run.  0 if none. </param>
        ''' <returns> Hash value for the buffer. </returns>
        Public Function hash(buffer As SByte(), initialValue As Long) As Long
            Dim len, pos As Integer
            ' internal variables used in the various calculations
            Dim a As Long
            Dim b As Long
            Dim c As Long

            ' set up the internal state
            ' the golden ratio; an arbitrary value
            a = &H9E3779B9L
            ' the golden ratio; an arbitrary value
            b = &H9E3779B9L
            ' the previous hash value
            c = &HE6359A60L

            ' handle most of the key
            pos = 0
            For len = buffer.Length To 12 Step -12
                a = add(a, fourByteToLong(buffer, pos))
                b = add(b, fourByteToLong(buffer, pos + 4))
                c = add(c, fourByteToLong(buffer, pos + 8))
                hashMix(a, b, c)
                pos += 12
            Next len

            c += buffer.Length

            ' all the case statements fall through to the next on purpose
            Select Case len
                Case 11
                    c = add(c, leftShift(byteToLong(buffer(pos + 10)), 24))

                Case 10
                    c = add(c, leftShift(byteToLong(buffer(pos + 9)), 16))

                Case 9
                    c = add(c, leftShift(byteToLong(buffer(pos + 8)), 8))
                    ' the first byte of c is reserved for the length

                Case 8
                    b = add(b, leftShift(byteToLong(buffer(pos + 7)), 24))

                Case 7
                    b = add(b, leftShift(byteToLong(buffer(pos + 6)), 16))

                Case 6
                    b = add(b, leftShift(byteToLong(buffer(pos + 5)), 8))

                Case 5
                    b = add(b, byteToLong(buffer(pos + 4)))

                Case 4
                    a = add(a, leftShift(byteToLong(buffer(pos + 3)), 24))

                Case 3
                    a = add(a, leftShift(byteToLong(buffer(pos + 2)), 16))

                Case 2
                    a = add(a, leftShift(byteToLong(buffer(pos + 1)), 8))

                Case 1
                    a = add(a, byteToLong(buffer(pos + 0)))
                    ' case 0: nothing left to add
            End Select

            hashMix(a, b, c)

            Return c
        End Function

        ''' <summary>
        ''' See hash(byte[] buffer, long initialValue)
        ''' </summary>
        ''' <param name="buffer"> Byte array that we are hashing on. </param>
        ''' <returns> Hash value for the buffer. </returns>
        Public Function hash(buffer As SByte()) As Long
            Return hash(buffer, 0)
        End Function

        ''' <summary>
        ''' 只允许ASCII字符串
        ''' </summary>
        ''' <param name="key$"></param>
        ''' <returns></returns>
        Public Function hash(key$) As Long
            Dim bytes As SByte() = Encoding.ASCII _
                .GetBytes(key) _
                .Select(Function(b) CSByte(b)) _
                .ToArray
            Return hash(bytes)
        End Function
    End Module
End Namespace
