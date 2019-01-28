Namespace org.renjin.hdf5.chunked



	Public Class UncompressedDecoder
		Implements ChunkDecoder

	  Private ReadOnly file As org.renjin.hdf5.Hdf5Data
	  Private ReadOnly factory As ChunkFactory

	  Public Sub New(ByVal file As org.renjin.hdf5.Hdf5Data, ByVal factory As ChunkFactory)
		Me.file = file
		Me.factory = factory
	  End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public Chunk read(long[] chunkOffset, long address, int size) throws java.io.IOException
	  Public Overrides Function read(ByVal chunkOffset() As Long, ByVal address As Long, ByVal size As Integer) As Chunk Implements ChunkDecoder.read
		Dim buffer As java.nio.MappedByteBuffer = file.map(java.nio.channels.FileChannel.MapMode.READ_ONLY, address, size)
		Return factory.wrap(chunkOffset, buffer)
	  End Function
	End Class

End Namespace