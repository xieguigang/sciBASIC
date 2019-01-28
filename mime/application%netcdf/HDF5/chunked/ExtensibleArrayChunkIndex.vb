Namespace org.renjin.hdf5.chunked



	''' <summary>
	''' The Extensible Array (EA) is a data structure that is used as a chunk index in datasets where the dataspace has a
	''' single unlimited dimension. In other words, one dimension is set to H5S_UNLIMITED, and the other dimensions are any
	''' number of fixed-size dimensions. The idea behind the extensible array is that a particular data object can be
	''' located via a lightweight indexing structure of fixed depth for a given address space. This indexing
	''' structure requires only a few (2-3) file operations per element lookup and gives good cache performance.
	''' Unlike the B-tree structure, the extensible array is optimized for appends.
	''' Where a B-tree would always add at the rightmost node under these circumstances,
	''' either creating a deep tree (version 1) or requiring expensive rebalances to correct (version 2),
	''' the extensible array has already mapped out a pre-balanced internal structure. This optimized internal
	''' structure is instantiated as needed when chunk records are inserted into the structure.
	''' 
	''' An Extensible Array consists of a header, an index block, secondary blocks, data blocks, and (optional)
	''' data block pages. The general scheme is that the index block is used to reference a secondary block,
	''' which is, in turn, used to reference the data block page where the chunk information is stored.
	''' The data blocks will be paged for efficiency when their size passes a threshold value.
	''' These pages are laid out contiguously on the disk after the data block,
	''' are initialized as needed, and are tracked via bitmaps stored in the secondary block.
	''' The number of secondary and data blocks/pages in a chunk index varies as they are allocated as needed
	''' and the first few are (conceptually) stored in parent elements as an optimization.
	''' </summary>
	Public Class ExtensibleArrayChunkIndex
		Inherits ChunkIndex

	  Private clientId As Integer

	  ''' <summary>
	  ''' The size in bytes of an element in the Extensible Array.
	  ''' </summary>
	  Private elementSize As Integer

	  ''' <summary>
	  ''' The number of bits needed to store the maximum number of elements in the Extensible Array.
	  ''' </summary>
	  Private maxNelementsBits As Integer

	  ''' <summary>
	  ''' The number of elements to store in the index block.
	  ''' </summary>
	  Private indexBlockElements As Integer

	  ''' <summary>
	  ''' The number of data block pointers to store in the index block.
	  ''' </summary>
	  Private indexBlockDataPointers As Integer

	  ''' <summary>
	  ''' The minimum number of elements per data block.
	  ''' </summary>
	  Private dataBlockMinElements As Integer

	  ''' <summary>
	  ''' The minimum number of data block pointers for a secondary block.
	  ''' </summary>
	  Private secondaryBlockMinDataPointers As Integer

	  ''' <summary>
	  ''' The number of bits needed to store the maximum number of elements in a data block page.
	  ''' </summary>
	  Private maxDataBlockPageNelmtsBits As Integer


	  ''' <summary>
	  ''' The number of secondary blocks created.
	  ''' </summary>
	  Private numSecondaryBlocks As Long

	  ''' <summary>
	  ''' The size of the secondary blocks created.
	  ''' </summary>
	  Private secondaryBlockSize As Long

	  ''' <summary>
	  ''' The number of data blocks created.
	  ''' </summary>
	  Private numDataBlocks As Long

	  ''' <summary>
	  ''' The size of the data blocks created.
	  ''' </summary>
	  Private dataBlockSize As Long

	  ''' <summary>
	  ''' The maximum index set.
	  ''' </summary>
	  Private maxIndexSet As Long

	  ''' <summary>
	  ''' The number of elements realized.
	  ''' </summary>
	  Private numElements As Long

	  ''' <summary>
	  ''' The address of the index block.
	  ''' </summary>
	  Private indexBlockAddress As Long

	  Private dimensions As Integer
	  Private chunkDecoder As ChunkFactory

	  ''' <summary>
	  ''' The checksum for the header.
	  ''' </summary>
	  Private checksum As Integer
	  Private dataBlockAddresses() As Long
	  Private secondaryBlockAddresses() As Long
	  Private file As org.renjin.hdf5.Hdf5Data
	  Private dataspace As org.renjin.hdf5.message.DataspaceMessage

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public ExtensibleArrayChunkIndex(org.renjin.hdf5.Hdf5Data file, org.renjin.hdf5.message.DataspaceMessage dataspace, org.renjin.hdf5.message.DataLayoutMessage layout, ChunkFactory chunkDecoder) throws java.io.IOException
	  Public Sub New(file As org.renjin.hdf5.Hdf5Data, dataspace As org.renjin.hdf5.message.DataspaceMessage, layout As org.renjin.hdf5.message.DataLayoutMessage, chunkDecoder As ChunkFactory)
		Me.file = file
		Me.dataspace = dataspace
		Me.dimensions = dataspace.Dimensionality
		Me.chunkDecoder = chunkDecoder
		readHeader(file, layout)
		readIndex(file)
	  End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readHeader(org.renjin.hdf5.Hdf5Data file, org.renjin.hdf5.message.DataLayoutMessage layout) throws java.io.IOException
	  Private Sub readHeader(file As org.renjin.hdf5.Hdf5Data, layout As org.renjin.hdf5.message.DataLayoutMessage)
		Dim headerSize As Integer = 12 + 6 * file.Superblock.LengthSize + file.Superblock.OffsetSize + 4
		Dim reader As org.renjin.hdf5.HeaderReader = file.readerAt(layout.ChunkIndexAddress, headerSize)
		reader.checkSignature("EAHD")

		Dim version As Integer = reader.readUInt8()
		If version <> 0 Then
		  Throw New System.NotSupportedException("Version: " & version)
		End If
		clientId = reader.readUInt8()
		elementSize = reader.readUInt8()
		maxNelementsBits = reader.readUInt8()

		indexBlockElements = reader.readUInt8()
		dataBlockMinElements = reader.readUInt8()
		secondaryBlockMinDataPointers = reader.readUInt8()
		indexBlockDataPointers = 2 * (secondaryBlockMinDataPointers - 1)
		maxDataBlockPageNelmtsBits = reader.readUInt8()

		numSecondaryBlocks = reader.readLength()
		secondaryBlockSize = reader.readLength()
		numDataBlocks = reader.readLength()
		dataBlockSize = reader.readLength()
		maxIndexSet = reader.readLength()
		numElements = reader.readLength()
		indexBlockAddress = reader.readLength()
		checksum = reader.readInt()
	  End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readIndex(org.renjin.hdf5.Hdf5Data file) throws java.io.IOException
	  Private Sub readIndex(file As org.renjin.hdf5.Hdf5Data)
		Dim indexSize As Long = 6 + file.Superblock.OffsetSize + indexBlockElements * elementSize + indexBlockDataPointers * file.Superblock.OffsetSize + numSecondaryBlocks * file.Superblock.OffsetSize + 4 ' checksum -  elements -  header address -  signature, version, client id

		Dim reader As org.renjin.hdf5.HeaderReader = file.readerAt(indexBlockAddress, indexSize)
		reader.checkSignature("EAIB")
		Dim version As Integer = reader.readUInt8()
		If version <> 0 Then
		  Throw New System.NotSupportedException("Version: " & version)
		End If
		Dim clientId As Integer = reader.readUInt8()
		Dim headerAddress As Long = reader.readOffset()

		Dim elements() As SByte = reader.readBytes(indexBlockElements * elementSize)
		dataBlockAddresses = reader.readOffsets(indexBlockDataPointers)
		secondaryBlockAddresses = reader.readOffsets(org.renjin.repackaged.guava.primitives.Ints.checkedCast(numSecondaryBlocks))
		Dim checksum As Integer = reader.readInt()

	  End Sub


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public Chunk chunkAt(long[] arrayIndex) throws java.io.IOException
	  Public Overrides Function chunkAt(arrayIndex() As Long) As Chunk
		Return readDataBlock(dataBlockAddresses(0))
	  End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private Chunk readDataBlock(long address) throws java.io.IOException
	  Private Function readDataBlock(address As Long) As Chunk
		Dim buffer As java.nio.ByteBuffer = file.bufferAt(address, dataBlockSize)

		Dim prefixSize As Integer = 6 + file.Superblock.OffsetSize + dimensions * file.Superblock.OffsetSize ' block offset -  header address -  signature, version, client id

		Dim reader As New org.renjin.hdf5.HeaderReader(file.Superblock, buffer)
		reader.checkSignature("EADB")
		Dim version As Integer = reader.readUInt8()
		If version <> 0 Then
		  Throw New System.NotSupportedException("Version: " & version)
		End If
		Dim clientId As Integer = reader.readUInt8()
		Dim headerAddress As Long = reader.readOffset()
		Dim blockOffset() As Integer = reader.readIntArray(dimensions)


		Throw New System.NotSupportedException("TODO")
	  End Function
	End Class

End Namespace