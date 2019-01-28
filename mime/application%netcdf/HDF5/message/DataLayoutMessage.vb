Namespace org.renjin.hdf5.message



	Public Class DataLayoutMessage
		Inherits Message

		Public Const MESSAGE_TYPE As Integer = &H8


		Public Enum LayoutClass
			COMPACT
			CONTIGUOUS
			CHUNKED
			VIRTUAL
		End Enum

		Public Enum ChunkIndexingType
			[SINGLE]
			IMPLICIT
			FIXED_ARRAY
			EXTENSIBLE_ARRAY
			BTREE
		End Enum

		Private version As SByte
		Private layoutClass As LayoutClass
		Private rawAddress As Long
		Private chunkIndexAddress As Long
		Private datasetElementSize As Integer
		Private dimensionSize() As Integer
		Private dimensionality As Integer

		Private chunkIndexingType As ChunkIndexingType = ChunkIndexingType.BTREE

		Private maxBits As Integer
		Private indexElements As Integer
		Private minPointers As Integer
		Private minElements As Integer

		''' <summary>
		''' the number of bits needed to store the maximum number of elements in a data block page.
		''' </summary>
		Private pageBits As Integer

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public DataLayoutMessage(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Public Sub New(reader As org.renjin.hdf5.HeaderReader)
			version = reader.readByte()
			If version = 3 Then
				readVersion3(reader)
			ElseIf version = 4 Then
				readVersion4(reader)
			Else
				Throw New System.NotSupportedException("Data layout: " & version)
			End If
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readVersion3(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Private Sub readVersion3(reader As org.renjin.hdf5.HeaderReader)
			layoutClass = System.Enum.GetValues(GetType(LayoutClass))(reader.readUInt8())
			Select Case layoutClass
				Case LayoutClass.CHUNKED
					readChunkedPropertiesV3(reader)
				Case Else
					Throw New System.NotSupportedException("Layout class: " & layoutClass)
			End Select
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readChunkedPropertiesV3(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Private Sub readChunkedPropertiesV3(reader As org.renjin.hdf5.HeaderReader)
			dimensionality = reader.readUInt8() - 1
			chunkIndexAddress = reader.readOffset()

			dimensionSize = New Integer(dimensionality - 1){}
			For i As Integer = 0 To dimensionality - 1
				dimensionSize(i) = reader.readUInt32AsInt()
			Next i
			datasetElementSize = reader.readUInt32AsInt()
		End Sub


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readVersion4(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Private Sub readVersion4(reader As org.renjin.hdf5.HeaderReader)
			layoutClass = System.Enum.GetValues(GetType(LayoutClass))(reader.readUInt8())
			Select Case layoutClass
				Case LayoutClass.CHUNKED
					readChunkedPropertiesV4(reader)
			End Select
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readChunkedPropertiesV4(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Private Sub readChunkedPropertiesV4(reader As org.renjin.hdf5.HeaderReader)
			Dim flags As org.renjin.hdf5.Flags = reader.readFlags()
			dimensionality = reader.readUInt8() - 1
			Dim dimensionSizeEncodedLength As Integer = reader.readUInt8()
			dimensionSize = New Integer(dimensionality - 1){}
			For i As Integer = 0 To dimensionality - 1
				dimensionSize(i) = CInt(reader.readUInt(dimensionSizeEncodedLength))
			Next i
			datasetElementSize = CInt(reader.readUInt(dimensionSizeEncodedLength))

			Dim chunkIndexingTypeIndex As Integer = reader.readUInt8()
			chunkIndexingType = System.Enum.GetValues(GetType(ChunkIndexingType))(chunkIndexingTypeIndex - 1)
			Select Case chunkIndexingType
				Case ChunkIndexingType.FIXED_ARRAY
					readFixedArrayProperties(reader)
				Case ChunkIndexingType.EXTENSIBLE_ARRAY
					readExtensibleArrayProperties(reader)
				Case Else
					Throw New System.NotSupportedException("chunkIndexingType: " & chunkIndexingType)
			End Select

			chunkIndexAddress = reader.readOffset()
		End Sub

		Private Sub readFixedArrayProperties(reader As org.renjin.hdf5.HeaderReader)
			pageBits = reader.readUInt8()
		End Sub

		Private Sub readExtensibleArrayProperties(reader As org.renjin.hdf5.HeaderReader)
			maxBits = reader.readUInt8()
			indexElements = reader.readUInt8()
			minPointers = reader.readUInt8()
			minElements = reader.readUInt8()
			pageBits = reader.readUInt8()

		End Sub

		Public Overridable Property Version As SByte
			Get
				Return version
			End Get
		End Property

		Public Overridable Property LayoutClass As LayoutClass
			Get
				Return layoutClass
			End Get
		End Property

		Public Overridable Property ChunkIndexAddress As Long
			Get
				Return chunkIndexAddress
			End Get
		End Property

		Public Overridable Property DatasetElementSize As Integer
			Get
				Return datasetElementSize
			End Get
		End Property

		Public Overridable Property Dimensionality As Integer
			Get
				Return dimensionality
			End Get
		End Property

		Public Overridable Function getChunkSize(dimensionIndex As Integer) As Integer
			Return dimensionSize(dimensionIndex)
		End Function

		Public Overridable Property ChunkIndexingType As ChunkIndexingType
			Get
				Return chunkIndexingType
			End Get
		End Property

		Public Overridable Property ChunkSize As Integer()
			Get
				Return dimensionSize
			End Get
		End Property

		Public Overridable Property ChunkCount As Long
			Get
				Dim count As Long = 1
				For i As Integer = 0 To dimensionality - 1
					count *= dimensionSize(i)
				Next i
				Return count
			End Get
		End Property

		Public Overridable Function getDimensionSize(i As Integer) As Integer
			Return dimensionSize(i)
		End Function

		Public Overridable Property ChunkElementCount As Long
			Get
				Dim count As Long = 1
				For i As Integer = 0 To dimensionality - 1
					count *= getChunkSize(i)
				Next i
				Return count
			End Get
		End Property


	End Class

End Namespace