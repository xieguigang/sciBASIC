Imports System

Namespace biz.k11i.xgboost.util

    ''' <summary>
    ''' Reads the Xgboost model from stream.
    ''' </summary>
    Public Class ModelReader
        Implements IDisposable

        Private ReadOnly stream As Stream
        Private buffer As SByte()

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: @Deprecated public ModelReader(String filename) throws java.io.IOException
        <Obsolete>
        Public Sub New(ByVal filename As String)
            Me.New(New FileStream(filename, FileMode.Open, FileAccess.Read))
        End Sub

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public ModelReader(java.io.InputStream in) throws java.io.IOException
        Public Sub New(ByVal [in] As Stream)
            stream = [in]
        End Sub

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: private int fillBuffer(int numBytes) throws java.io.IOException
        Private Function fillBuffer(ByVal numBytes As Integer) As Integer
            If buffer Is Nothing OrElse buffer.Length < numBytes Then
                buffer = New SByte(numBytes - 1) {}
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

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public int readByteAsInt() throws java.io.IOException
        Public Overridable Function readByteAsInt() As Integer
            Return stream.Read()
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public byte[] readByteArray(int numBytes) throws java.io.IOException
        Public Overridable Function readByteArray(ByVal numBytes As Integer) As SByte()
            Dim numBytesRead = fillBuffer(numBytes)

            If numBytesRead < numBytes Then
                Throw New EOFException(String.Format("Cannot read byte array (shortage): expected = {0:D}, actual = {1:D}", numBytes, numBytesRead))
            End If

            Dim result = New SByte(numBytes - 1) {}
            Array.Copy(buffer, 0, result, 0, numBytes)
            Return result
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public int readInt() throws java.io.IOException
        Public Overridable Function readInt() As Integer
            Return readInt(ByteOrder.LITTLE_ENDIAN)
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public int readIntBE() throws java.io.IOException
        Public Overridable Function readIntBE() As Integer
            Return readInt(ByteOrder.BIG_ENDIAN)
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: private int readInt(java.nio.ByteOrder byteOrder) throws java.io.IOException
        Private Function readInt(ByVal byteOrder As ByteOrder) As Integer
            Dim numBytesRead = fillBuffer(4)

            If numBytesRead < 4 Then
                Throw New EOFException("Cannot read int value (shortage): " & numBytesRead)
            End If

            Return ByteBuffer.wrap(buffer).order(byteOrder).int
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public int[] readIntArray(int numValues) throws java.io.IOException
        Public Overridable Function readIntArray(ByVal numValues As Integer) As Integer()
            Dim numBytesRead = fillBuffer(numValues * 4)

            If numBytesRead < numValues * 4 Then
                Throw New EOFException(String.Format("Cannot read int array (shortage): expected = {0:D}, actual = {1:D}", numValues * 4, numBytesRead))
            End If

            Dim byteBuffer As ByteBuffer = ByteBuffer.wrap(buffer).order(ByteOrder.LITTLE_ENDIAN)
            Dim result = New Integer(numValues - 1) {}

            For i = 0 To numValues - 1
                result(i) = byteBuffer.int
            Next

            Return result
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public int readUnsignedInt() throws java.io.IOException
        Public Overridable Function readUnsignedInt() As Integer
            Dim result As Integer = readInt()

            If result < 0 Then
                Throw New IOException("Cannot read unsigned int (overflow): " & result)
            End If

            Return result
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public long readLong() throws java.io.IOException
        Public Overridable Function readLong() As Long
            Dim numBytesRead = fillBuffer(8)

            If numBytesRead < 8 Then
                Throw New IOException("Cannot read long value (shortage): " & numBytesRead)
            End If

            Return ByteBuffer.wrap(buffer).order(ByteOrder.LITTLE_ENDIAN).[long]
        End Function

        Public Overridable Function asFloat(ByVal bytes As SByte()) As Single
            Return ByteBuffer.wrap(bytes).order(ByteOrder.LITTLE_ENDIAN).float
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public int asUnsignedInt(byte[] bytes) throws java.io.IOException
        Public Overridable Function asUnsignedInt(ByVal bytes As SByte()) As Integer
            Dim result As Integer = ByteBuffer.wrap(bytes).order(ByteOrder.LITTLE_ENDIAN).int

            If result < 0 Then
                Throw New IOException("Cannot treat as unsigned int (overflow): " & result)
            End If

            Return result
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public float readFloat() throws java.io.IOException
        Public Overridable Function readFloat() As Single
            Dim numBytesRead = fillBuffer(4)

            If numBytesRead < 4 Then
                Throw New IOException("Cannot read float value (shortage): " & numBytesRead)
            End If

            Return ByteBuffer.wrap(buffer).order(ByteOrder.LITTLE_ENDIAN).float
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public float[] readFloatArray(int numValues) throws java.io.IOException
        Public Overridable Function readFloatArray(ByVal numValues As Integer) As Single()
            Dim numBytesRead = fillBuffer(numValues * 4)

            If numBytesRead < numValues * 4 Then
                Throw New EOFException(String.Format("Cannot read float array (shortage): expected = {0:D}, actual = {1:D}", numValues * 4, numBytesRead))
            End If

            Dim byteBuffer As ByteBuffer = ByteBuffer.wrap(buffer).order(ByteOrder.LITTLE_ENDIAN)
            Dim result = New Single(numValues - 1) {}

            For i = 0 To numValues - 1
                result(i) = byteBuffer.float
            Next

            Return result
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public double[] readDoubleArrayBE(int numValues) throws java.io.IOException
        Public Overridable Function readDoubleArrayBE(ByVal numValues As Integer) As Double()
            Dim numBytesRead = fillBuffer(numValues * 8)

            If numBytesRead < numValues * 8 Then
                Throw New EOFException(String.Format("Cannot read double array (shortage): expected = {0:D}, actual = {1:D}", numValues * 8, numBytesRead))
            End If

            Dim byteBuffer As ByteBuffer = ByteBuffer.wrap(buffer).order(ByteOrder.BIG_ENDIAN)
            Dim result = New Double(numValues - 1) {}

            For i = 0 To numValues - 1
                result(i) = byteBuffer.[double]
            Next

            Return result
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public void skip(long numBytes) throws java.io.IOException
        Public Overridable Sub skip(ByVal numBytes As Long)
            Dim numBytesRead As Long = stream.skip(numBytes)

            If numBytesRead < numBytes Then
                Throw New IOException("Cannot skip bytes: " & numBytesRead)
            End If
        End Sub

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public String readString() throws java.io.IOException
        Public Overridable Function readString() As String
            Dim length As Long = readLong()

            If length > Integer.MaxValue Then
                Throw New IOException("Too long string: " & length)
            End If

            Return readString(length)
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public String readString(int numBytes) throws java.io.IOException
        Public Overridable Function readString(ByVal numBytes As Integer) As String
            Dim numBytesRead = fillBuffer(numBytes)

            If numBytesRead < numBytes Then
                Throw New IOException(String.Format("Cannot read string({0:D}) (shortage): {1:D}", numBytes, numBytesRead))
            End If

            Return StringHelperClass.NewString(buffer, 0, numBytes, Charset.forName("UTF-8"))
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public String readUTF() throws java.io.IOException
        Public Overridable Function readUTF() As String
            Dim utflen As Integer = readByteAsInt()
            utflen = CShort((utflen << 8 Or readByteAsInt()))
            Return readUTF(utflen)
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public String readUTF(int utflen) throws java.io.IOException
        Public Overridable Function readUTF(ByVal utflen As Integer) As String
            Dim numBytesRead = fillBuffer(utflen)

            If numBytesRead < utflen Then
                Throw New EOFException(String.Format("Cannot read UTF string bytes: expected = {0:D}, actual = {1:D}", utflen, numBytesRead))
            End If

            Dim chararr = New Char(utflen - 1) {}
            Dim c, char2, char3 As Integer
            Dim count = 0
            Dim chararr_count = 0

            While count < utflen
                c = buffer(count) And &HfF

                If c > 127 Then
                    Exit While
                End If

                count += 1
                chararr(Math.Min(Threading.Interlocked.Increment(chararr_count), chararr_count - 1)) = Microsoft.VisualBasic.ChrW(c)
            End While

            While count < utflen
                c = buffer(count) And &HfF

                Select Case c >> 4
                    Case 0, 1, 2, 3, 4, 5, 6, 7
                        ' 0xxxxxxx
                        count += 1
                        chararr(Math.Min(Threading.Interlocked.Increment(chararr_count), chararr_count - 1)) = Microsoft.VisualBasic.ChrW(c)
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

                        chararr(Math.Min(Threading.Interlocked.Increment(chararr_count), chararr_count - 1)) = Microsoft.VisualBasic.ChrW((c And &H1F) << 6 Or char2 And &H3F)
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

                        chararr(Math.Min(Threading.Interlocked.Increment(chararr_count), chararr_count - 1)) = Microsoft.VisualBasic.ChrW((c And &H0F) << 12 Or (char2 And &H3F) << 6 Or (char3 And &H3F) << 0)
                    Case Else
                        ' 10xx xxxx,  1111 xxxx 
                        Throw New UTFDataFormatException("malformed input around byte " & count)
                End Select
            End While
            ' The number of chars produced may be less than utflen
            Return New String(chararr, 0, chararr_count)
        End Function

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: @Override public void close() throws java.io.IOException
        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            stream.Close()
        End Sub
    End Class
End Namespace
