Namespace org.renjin.hdf5





	Public Class HeaderReader

		Private superblock As Superblock
		Private buffer As java.nio.ByteBuffer

		Public Sub New(superblock As Superblock, buffer As java.nio.ByteBuffer)
			org.renjin.repackaged.guava.base.Preconditions.checkNotNull(superblock, "superblock")
			Me.superblock = superblock
			Me.buffer = buffer
			Me.buffer.order(java.nio.ByteOrder.LITTLE_ENDIAN)
		End Sub

		Public Overridable Function readByte() As SByte
			Return buffer.get()
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public void checkSignature(String signature) throws java.io.IOException
		Public Overridable Sub checkSignature(signature As String)
			Dim expected() As SByte = signature.getBytes(org.renjin.repackaged.guava.base.Charsets.US_ASCII)
			Dim array() As SByte = readBytes(expected.Length)

			For i As Integer = 0 To expected.Length - 1
				If expected(i) <> array(i) Then
					Throw New java.io.IOException("Invalid signature. Expected: " & signature)
				End If
			Next i
		End Sub

		Public Overridable Function readFlags() As Flags
			Return New Flags(buffer.get())
		End Function

		Public Overridable Function readInt() As Integer
			Return buffer.Int
		End Function

		Public Overridable Function readUInt16() As Integer
			Return buffer.Char
		End Function

		Public Overridable Function readUInt8() As Integer
			Return org.renjin.repackaged.guava.primitives.UnsignedBytes.toInt(buffer.get())
		End Function

		Public Overridable Function readUInt32() As Long
			Return org.renjin.repackaged.guava.primitives.UnsignedInts.toLong(buffer.Int)
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public long readUInt64() throws java.io.IOException
		Public Overridable Function readUInt64() As Long
			Dim value As Long = buffer.Long
			If value < 0 Then
				Throw New java.io.IOException("Unsigned long overflow")
			End If
			Return value
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public int readVariableLengthSizeAsInt(Flags flags) throws java.io.IOException
		Public Overridable Function readVariableLengthSizeAsInt(flags As Flags) As Integer
			Dim length As Long = readVariableLengthSize(flags.value() And &H3)
			If length > Integer.MaxValue Then
				Throw New java.io.IOException("Overflow")
			End If
			Return CInt(length)
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public long readVariableLengthSize(int size) throws java.io.IOException
		Public Overridable Function readVariableLengthSize(size As Integer) As Long
			Select Case size
				Case 0
					Return readUInt8()
				Case 1
					Return readUInt16()
				Case 2
					Return readUInt32()
				Case 3
					Return readUInt64()

				Case Else
					Throw New System.ArgumentException("size: " & size)
			End Select
		End Function

		''' <summary>
		''' Sets the number of remaining bytes left in this header. </summary>
		''' <param name="length"> number of bytes </param>
		Public Overridable Sub updateLimit(length As Integer)
			buffer.limit(buffer.position() + length)
		End Sub

		Public Overridable Function remaining() As Integer
			Return buffer.remaining()
		End Function

		Public Overridable Function readBytes(size As Integer) As SByte()
			Dim bytes(size - 1) As SByte
			buffer.get(bytes)
			Return bytes
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public long readOffset() throws java.io.IOException
		Public Overridable Function readOffset() As Long
			If superblock.OffsetSize = 4 Then
				Return readUInt32()
			End If
			Dim value As Long = buffer.Long
			' -1 is the UNSPECIFIED value
			If value < -1 Then
				Throw New java.io.IOException("Long offset overflow")
			End If
			Return value
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public long readLength() throws java.io.IOException
		Public Overridable Function readLength() As Long
			If superblock.LengthSize = 4 Then
				Return readUInt32()
			End If
			Dim value As Long = buffer.Long
			' -1 is the UNSPECIFIED value
			If value < -1 Then
				Throw New java.io.IOException("Long length overflow")
			End If
			Return value
		End Function

		Public Overridable Function readString(length As Integer, charset As java.nio.charset.Charset) As String
			Dim bytes() As SByte = readBytes(length)
			Return New String(bytes, charset)
		End Function

		Public Overridable Function peekByte() As SByte
			Return buffer.get(0)
		End Function

		Public Overridable Sub readReserved(byteCount As Integer)
			For i As Integer = 0 To byteCount - 1
				Dim reserved As SByte = readByte()
			Next i
		End Sub

		Public Overridable Function position() As Integer
			Return buffer.position()
		End Function

		Public Overridable Function readUInt() As Integer
			Return buffer.Int
		End Function

		Public Overridable Function readIntArray(count As Integer) As Integer()
			Dim array(count - 1) As Integer
			For i As Integer = 0 To count - 1
				array(i) = readUInt()
			Next i
			Return array
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public int readUInt32AsInt() throws java.io.IOException
		Public Overridable Function readUInt32AsInt() As Integer
			Dim value As Long = readUInt32()
			If value > Integer.MaxValue Then
				Throw New java.io.IOException("Integer overflow")
			End If
			Return CInt(value)
		End Function

		Public Overridable Function readNullTerminatedAsciiString(nameLength As Integer) As String
			Dim bytes() As SByte = readBytes(nameLength)
			Dim len As Integer = 0
			Do While bytes(len) <> 0 AndAlso len < bytes.Length
				len += 1
			Loop
			Return New String(bytes, 0, len, org.renjin.repackaged.guava.base.Charsets.US_ASCII)
		End Function

		Public Overridable Function readNullTerminatedAsciiString() As String
			Dim bytes As New java.io.ByteArrayOutputStream()
			Dim b As Integer
			b=readUInt8()
			Do While b <> 0
				bytes.write(b)
				b=readUInt8()
			Loop
			Return New String(bytes.toByteArray(), org.renjin.repackaged.guava.base.Charsets.US_ASCII)
		End Function

		''' <summary>
		''' Reads an integer of the given size </summary>
		''' <param name="byteSize"> the size of the integer in bytes </param>
'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public long readUInt(int byteSize) throws java.io.IOException
		Public Overridable Function readUInt(byteSize As Integer) As Long
			Select Case byteSize
				Case 1
					Return readUInt8()
				Case 2
					Return readUInt16()
				Case 4
					Return readUInt32()
				Case 8
					Return readUInt64()
				Case Else
					Throw New System.ArgumentException("bytes: " & byteSize)
			End Select
		End Function

		''' <summary>
		''' If the reader is not currently aligned to the given multiple, advance the position until it is. </summary>
		''' <param name="multiple"> </param>
		Public Overridable Sub alignTo(multiple As Integer)
			Dim misalignment As Integer = (buffer.position() Mod multiple)
			If misalignment <> 0 Then
				buffer.position(buffer.position() + misalignment)
			End If
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public long[] readOffsets(int count) throws java.io.IOException
		Public Overridable Function readOffsets(count As Integer) As Long()
			Dim offsets(count - 1) As Long
			For i As Integer = 0 To count - 1
				offsets(i) = readOffset()
			Next i
			Return offsets
		End Function
	End Class

End Namespace