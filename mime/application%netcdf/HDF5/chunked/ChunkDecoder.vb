Namespace org.renjin.hdf5.chunked


	''' <summary>
	''' Responsible for decoding chunks, decompressing, etc as necessary
	''' </summary>
	Public Interface ChunkDecoder

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: Chunk read(long[] chunkOffset, long address, int size) throws java.io.IOException;
	  Function read(chunkOffset() As Long, address As Long, size As Integer) As Chunk

	End Interface

End Namespace