Namespace org.renjin.hdf5.chunked



	''' <summary>
	''' The Fixed Array index can be used when the dataset has fixed maximum dimension sizes.
	''' 
	''' <p>Since the maximum number of chunks is known, an array of in-file-on-disk addresses based on the maximum number of
	''' chunks is allocated when data is written to the dataset. To access a dataset chunk with a specified offset,
	''' the chunk index associated with the offset is calculated. The index is mapped into the array to locate the disk
	''' address for the chunk.
	''' 
	''' <p>The Fixed Array (FA) index structure provides space and speed improvements in locating chunks over
	''' index structures that handle more dynamic data accesses like a Version 2 B-tree index. The entry into the Fixed Array
	''' is the Fixed Array header which contains metadata about the entries stored in the array. The header contains a
	''' pointer to a data block which stores the array of entries that describe the dataset chunks. For greater efficiency,
	''' the array will be divided into multiple pages if the number of entries exceeds a threshold value.
	''' The space for the data block and possibly data block pages are allocated as a single contiguous block of space.
	''' 
	''' The content of the data block depends on whether paging is activated or not. When paging is not used,
	''' elements that describe the chunks are stored in the data block. If paging is turned on, the data block
	''' contains a bitmap indicating which pages are initialized. Then subsequent data block pages will contain
	''' the entries that describe the chunks.
	''' </summary>
	Public Class FixedArrayChunkIndex
		Inherits ChunkIndex

		Private Const CHECKSUM_SIZE As Integer = 4

		Private file As org.renjin.hdf5.Hdf5Data
		Private ReadOnly layout As org.renjin.hdf5.message.DataLayoutMessage
		Private ReadOnly maxNumEntries As Long
		Private ReadOnly dataBlockHeaderSize As Integer
		Private dataBlockBuffer As java.nio.MappedByteBuffer
		Private ReadOnly entrySize As SByte

		Private chunkSize() As Integer
		Private chunkDims() As Integer
		Private ReadOnly numberElementsPerDataBlockPage As Integer
		Private ReadOnly numberOfPages As Integer
		Private ReadOnly dataBlockPageSize As Integer

		Private ReadOnly chunkCache As org.renjin.repackaged.guava.cache.Cache(Of Integer?, Chunk)

		Private ReadOnly decoder As ChunkDecoder

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public FixedArrayChunkIndex(org.renjin.hdf5.Hdf5Data file, org.renjin.hdf5.message.DataspaceMessage dataspace, org.renjin.hdf5.message.DataLayoutMessage layout, ChunkDecoder decoder) throws java.io.IOException
		Public Sub New(file As org.renjin.hdf5.Hdf5Data, dataspace As org.renjin.hdf5.message.DataspaceMessage, layout As org.renjin.hdf5.message.DataLayoutMessage, decoder As ChunkDecoder)
			Me.file = file
			Me.layout = layout
			Me.decoder = decoder

			Dim reader As org.renjin.hdf5.HeaderReader = file.readerAt(layout.ChunkIndexAddress, headerSize(file.Superblock))

			reader.checkSignature("FAHD")
			Dim version As SByte = reader.readByte()
			If version <> 0 Then
				Throw New System.NotSupportedException("FAHD version: " & version)
			End If
			Dim clientId As SByte = reader.readByte()
			If clientId <> 0 Then
				Throw New System.NotSupportedException("client id: " & clientId)
			End If
			entrySize = reader.readByte()
			Dim pageBits As SByte = reader.readByte()
			maxNumEntries = reader.readLength()

			numberElementsPerDataBlockPage = (1 << pageBits)
			numberOfPages = ceilDiv(maxNumEntries, numberElementsPerDataBlockPage)
			dataBlockPageSize = numberElementsPerDataBlockPage * entrySize + CHECKSUM_SIZE

			Dim dataBlockAddress As Long = reader.readOffset()
			Dim checkSum As Integer = reader.readInt()

			Dim pagingBitMapSize As Integer = ceilDiv(numberOfPages, 8)
			dataBlockHeaderSize = 6 + file.Superblock.OffsetSize + pagingBitMapSize + CHECKSUM_SIZE

			Dim datablockSize As Long = dataBlockHeaderSize + dataBlockPageSize * numberOfPages

			dataBlockBuffer = file.map(java.nio.channels.FileChannel.MapMode.READ_ONLY, dataBlockAddress, datablockSize)
			dataBlockBuffer.order(java.nio.ByteOrder.LITTLE_ENDIAN)

			Dim dataBlockHeaderReader As New org.renjin.hdf5.HeaderReader(file.Superblock, dataBlockBuffer)
			dataBlockHeaderReader.checkSignature("FADB")
			Dim dataBlockVersion As SByte = dataBlockHeaderReader.readByte()
			Dim dataBlockClientId As SByte = dataBlockHeaderReader.readByte()
			Dim dataBlockHeaderAddress As Long = dataBlockHeaderReader.readOffset()
			Dim pagingBitmask() As SByte = dataBlockHeaderReader.readBytes(pagingBitMapSize)
			Dim dataBlockHeaderChecksum As Integer = dataBlockHeaderReader.readInt()

			chunkSize = layout.ChunkSize
			chunkDims = New Integer(layout.Dimensionality - 1){}
			For i As Integer = 0 To layout.Dimensionality - 1
				chunkDims(i) = ceilDiv(dataspace.getDimensionSize(i), layout.getChunkSize(i))
			Next i

			Me.chunkCache = org.renjin.repackaged.guava.cache.CacheBuilder.newBuilder().softValues().build()
		End Sub

		Private Function ceilDiv(dimensionSize As Long, chunkSize As Integer) As Integer
			Dim count As Integer = CInt(dimensionSize \ chunkSize)
			If dimensionSize Mod chunkSize <> 0 Then
				count = count + 1
			End If
			Return count
		End Function

		Private Shared Function headerSize(superblock As org.renjin.hdf5.Superblock) As Integer
			Return 4 + 4 + superblock.LengthSize + superblock.OffsetSize + 4 ' checksum -  data block adddres -  max num entries -  version + client id ... -  signature
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public Chunk chunkAt(long[] arrayIndex) throws java.io.IOException
		Public Overrides Function chunkAt(arrayIndex() As Long) As Chunk
			Dim chunkIndex As Integer = arrayIndexToChunkIndex(arrayIndex)

			Dim chunk As Chunk = chunkCache.getIfPresent(chunkIndex)
			If chunk Is Nothing Then
				Dim pageIndex As Integer = chunkIndex \ numberElementsPerDataBlockPage
				Dim addressOffset As Integer = dataBlockHeaderSize + pageIndex * dataBlockPageSize + (chunkIndex Mod numberElementsPerDataBlockPage) * entrySize

				Dim chunkAddress As Long
				If entrySize = 8 Then
					chunkAddress = dataBlockBuffer.getLong(addressOffset)
				Else
					Throw New System.NotSupportedException("entrySize: " & entrySize)
				End If

				chunk = readChunk(arrayIndex, chunkAddress)
				chunkCache.put(chunkIndex, chunk)
			End If
			Return chunk
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private Chunk readChunk(long[] arrayIndex, long chunkAddress) throws java.io.IOException
		Private Function readChunk(arrayIndex() As Long, chunkAddress As Long) As Chunk

			Dim chunkOffset() As Long = elementToChunk(arrayIndex)
			Dim chunkSize As Long = layout.ChunkElementCount * layout.DatasetElementSize

			Return decoder.read(chunkOffset, chunkAddress, org.renjin.repackaged.guava.primitives.Ints.checkedCast(chunkSize))
		End Function

		Private Function elementToChunk(arrayIndex() As Long) As Long()
			Dim chunkOffset() As Long = java.util.Arrays.copyOf(arrayIndex, arrayIndex.Length)
			For i As Integer = 0 To chunkOffset.Length - 1
				chunkOffset(i) = (chunkOffset(i) \ chunkSize(i)) * chunkSize(i)
			Next i
			Return chunkOffset
		End Function

		Public Overridable Function arrayIndexToChunkIndex(arrayIndex() As Long) As Integer
			Dim chunkIndex As Long = 0
			Dim offset As Long = 1
			For i As Integer = chunkDims.Length-1 To 0 Step -1
				Dim chunkOffset As Long = arrayIndex(i) \ chunkSize(i)
				chunkIndex += chunkOffset * offset
				offset *= chunkDims(i)
			Next i
			Return CInt(chunkIndex)
		End Function
	End Class

End Namespace