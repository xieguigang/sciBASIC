Namespace org.renjin.hdf5.chunked


	Public MustInherit Class ChunkIndex

		''' <summary>
		''' Retrieves the chunk that includes the element at the given {@code arrayIndex}
		''' </summary>
'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public abstract Chunk chunkAt(long[] arrayIndex) throws java.io.IOException;
		Public MustOverride Function chunkAt(ByVal arrayIndex() As Long) As Chunk
	End Class

End Namespace