#Region "Microsoft.VisualBasic::0d8ac4a3883a1480a7670b8fc8110ac2, Data_science\MachineLearning\xgboost\util\ModelReader.vb"

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

    '   Total Lines: 283
    '    Code Lines: 200 (70.67%)
    ' Comment Lines: 9 (3.18%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 74 (26.15%)
    '     File Size: 10.45 KB


    '     Class ModelReader
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: asFloat, asUnsignedInt, fillBuffer, readByteArray, readByteAsInt
    '                   readDoubleArrayBE, readFloat, readFloatArray, (+2 Overloads) readInt, readIntArray
    '                   readIntBE, readLong, (+2 Overloads) readString, readUnsignedInt, (+2 Overloads) readUTF
    ' 
    '         Sub: Dispose, skip
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language

Namespace util

    ''' <summary>
    ''' Reads the Xgboost model from stream.
    ''' </summary>
    Public Class ModelReader : Implements IDisposable

        Private ReadOnly stream As Stream
        Private buffer As Byte()

        Public Sub New(filename As String)
            Me.New(New FileStream(filename, FileMode.Open, FileAccess.Read))
        End Sub

        Public Sub New([in] As Stream)
            stream = [in]
        End Sub

        Private Function fillBuffer(numBytes As Integer) As Integer
            If buffer Is Nothing OrElse buffer.Length < numBytes Then
                buffer = New Byte(numBytes - 1) {}
            End If

            Dim numBytesRead = 0

            While numBytesRead < numBytes
                Dim count = stream.Read(buffer, numBytesRead, numBytes - numBytesRead)

                If count < 0 Then
                    Return numBytesRead
                End If

                numBytesRead += count
            End While

            Return numBytesRead
        End Function

        Public Overridable Function readByteAsInt() As Integer
            Return stream.ReadByte
            ' Return stream.Read()
        End Function

        Public Overridable Function readByteArray(numBytes As Integer) As SByte()
            Dim numBytesRead = fillBuffer(numBytes)

            If numBytesRead < numBytes Then
                Throw New EOFException(String.Format("Cannot read byte array (shortage): expected = {0:D}, actual = {1:D}", numBytes, numBytesRead))
            End If

            Dim result = New SByte(numBytes - 1) {}
            Array.Copy(buffer, 0, result, 0, numBytes)
            Return result
        End Function

        Public Overridable Function readInt() As Integer
            Return readInt(ByteOrder.LittleEndian)
        End Function

        Public Overridable Function readIntBE() As Integer
            Return readInt(ByteOrder.BigEndian)
        End Function

        Private Function readInt(byteOrder As ByteOrder) As Integer
            Dim numBytesRead = fillBuffer(4)

            If numBytesRead < 4 Then
                Throw New EOFException("Cannot read int value (shortage): " & numBytesRead)
            End If

            Return ByteBuffer.wrap(buffer).order(byteOrder).int
        End Function

        Public Overridable Function readIntArray(numValues As Integer) As Integer()
            Dim numBytesRead = fillBuffer(numValues * 4)

            If numBytesRead < numValues * 4 Then
                Throw New EOFException(String.Format("Cannot read int array (shortage): expected = {0:D}, actual = {1:D}", numValues * 4, numBytesRead))
            End If

            Dim byteBuffer As ByteBuffer = ByteBuffer.wrap(buffer).order(ByteOrder.LittleEndian)
            Dim result = New Integer(numValues - 1) {}

            For i = 0 To numValues - 1
                result(i) = byteBuffer.int
            Next

            Return result
        End Function

        Public Overridable Function readUnsignedInt() As Integer
            Dim result As Integer = readInt()

            If result < 0 Then
                Throw New IOException("Cannot read unsigned int (overflow): " & result)
            End If

            Return result
        End Function

        Public Overridable Function readLong() As Long
            Dim numBytesRead = fillBuffer(8)

            If numBytesRead < 8 Then
                Throw New IOException("Cannot read long value (shortage): " & numBytesRead)
            End If

            Return ByteBuffer.wrap(buffer).order(ByteOrder.LittleEndian).[long]
        End Function

        Public Overridable Function asFloat(bytes As SByte()) As Single
            Return ByteBuffer.wrap(bytes).order(ByteOrder.LittleEndian).float
        End Function

        Public Overridable Function asUnsignedInt(bytes As SByte()) As Integer
            Dim result As Integer = ByteBuffer.wrap(bytes).order(ByteOrder.LittleEndian).int

            If result < 0 Then
                Throw New IOException("Cannot treat as unsigned int (overflow): " & result)
            End If

            Return result
        End Function

        Public Overridable Function readFloat() As Single
            Dim numBytesRead = fillBuffer(4)

            If numBytesRead < 4 Then
                Throw New IOException("Cannot read float value (shortage): " & numBytesRead)
            End If

            Return ByteBuffer.wrap(buffer).order(ByteOrder.LittleEndian).float
        End Function

        Public Overridable Function readFloatArray(numValues As Integer) As Single()
            Dim numBytesRead = fillBuffer(numValues * 4)

            If numBytesRead < numValues * 4 Then
                Throw New EOFException(String.Format("Cannot read float array (shortage): expected = {0:D}, actual = {1:D}", numValues * 4, numBytesRead))
            End If

            Dim byteBuffer As ByteBuffer = ByteBuffer.wrap(buffer).order(ByteOrder.LittleEndian)
            Dim result = New Single(numValues - 1) {}

            For i = 0 To numValues - 1
                result(i) = byteBuffer.float
            Next

            Return result
        End Function

        Public Overridable Function readDoubleArrayBE(numValues As Integer) As Double()
            Dim numBytesRead = fillBuffer(numValues * 8)

            If numBytesRead < numValues * 8 Then
                Throw New EOFException(String.Format("Cannot read double array (shortage): expected = {0:D}, actual = {1:D}", numValues * 8, numBytesRead))
            End If

            Dim byteBuffer As ByteBuffer = ByteBuffer.wrap(buffer).order(ByteOrder.BigEndian)
            Dim result = New Double(numValues - 1) {}

            For i = 0 To numValues - 1
                result(i) = byteBuffer.[double]
            Next

            Return result
        End Function

        Public Overridable Sub skip(numBytes As Long)
            Dim numBytesRead As Long = stream.skip(numBytes)

            If numBytesRead < numBytes Then
                Throw New IOException("Cannot skip bytes: " & numBytesRead)
            End If
        End Sub

        Public Overridable Function readString() As String
            Dim length As Long = readLong()

            If length > Integer.MaxValue Then
                Throw New IOException("Too long string: " & length)
            End If

            Return readString(length)
        End Function

        Public Overridable Function readString(numBytes As Integer) As String
            Dim numBytesRead = fillBuffer(numBytes)

            If numBytesRead < numBytes Then
                Throw New IOException(String.Format("Cannot read string({0:D}) (shortage): {1:D}", numBytes, numBytesRead))
            End If

            Return Encoding.UTF8.GetString(CType(CObj(buffer), Byte()), 0, numBytes)
        End Function

        Public Overridable Function readUTF() As String
            Dim utflen As Integer = readByteAsInt()
            utflen = CShort((utflen << 8 Or readByteAsInt()))
            Return readUTF(utflen)
        End Function

        Public Overridable Function readUTF(utflen As Integer) As String
            Dim numBytesRead = fillBuffer(utflen)

            If numBytesRead < utflen Then
                Throw New EOFException(String.Format("Cannot read UTF string bytes: expected = {0:D}, actual = {1:D}", utflen, numBytesRead))
            End If

            Dim chararr = New Char(utflen - 1) {}
            Dim c, char2, char3 As Integer
            Dim count = 0
            Dim chararr_count As i32 = 0

            While count < utflen
                c = buffer(count) And &HFF

                If c > 127 Then
                    Exit While
                End If

                count += 1
                chararr(++chararr_count) = ChrW(c)
            End While

            While count < utflen
                c = buffer(count) And &HFF

                Select Case c >> 4
                    Case 0, 1, 2, 3, 4, 5, 6, 7
                        ' 0xxxxxxx
                        count += 1
                        chararr(++chararr_count) = ChrW(c)
                    Case 12, 13
                        ' 110x xxxx   10xx xxxx
                        count += 2

                        If count > utflen Then
                            Throw New UTFDataFormatException("malformed input: partial character at end")
                        End If

                        char2 = buffer(count - 1)

                        If (char2 And &HC0) <> &H80 Then
                            Throw New UTFDataFormatException("malformed input around byte " & count)
                        End If

                        chararr(++chararr_count) = ChrW((c And &H1F) << 6 Or char2 And &H3F)
                    Case 14
                        ' 1110 xxxx  10xx xxxx  10xx xxxx 
                        count += 3

                        If count > utflen Then
                            Throw New UTFDataFormatException("malformed input: partial character at end")
                        End If

                        char2 = buffer(count - 2)
                        char3 = buffer(count - 1)

                        If (char2 And &HC0) <> &H80 OrElse (char3 And &HC0) <> &H80 Then
                            Throw New UTFDataFormatException("malformed input around byte " & count - 1)
                        End If

                        chararr(++chararr_count) = ChrW((c And &HF) << 12 Or (char2 And &H3F) << 6 Or (char3 And &H3F) << 0)
                    Case Else
                        ' 10xx xxxx,  1111 xxxx 
                        Throw New UTFDataFormatException("malformed input around byte " & count)
                End Select
            End While
            ' The number of chars produced may be less than utflen
            Return New String(chararr, 0, chararr_count)
        End Function

        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            stream.Close()
        End Sub
    End Class
End Namespace
