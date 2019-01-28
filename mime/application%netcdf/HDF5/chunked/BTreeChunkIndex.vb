Imports System
Imports System.Diagnostics
Imports System.Collections.Generic

Namespace org.renjin.hdf5.chunked




	Public Class BTreeChunkIndex
		Inherits ChunkIndex

		Private file As org.renjin.hdf5.Hdf5Data
		Private dataLayout As org.renjin.hdf5.message.DataLayoutMessage
		Private ReadOnly chunkDecoder As ChunkDecoder
		Private ReadOnly rootNode As ChunkNode

		Private nodes As IDictionary(Of Long?, ChunkNode) = New Dictionary(Of Long?, ChunkNode)()

		Private chunkCache As org.renjin.repackaged.guava.cache.LoadingCache(Of ChunkKey, Chunk)

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public BTreeChunkIndex(org.renjin.hdf5.Hdf5Data file, org.renjin.hdf5.message.DataLayoutMessage dataLayout, ChunkDecoder decoder) throws java.io.IOException
		Public Sub New(file As org.renjin.hdf5.Hdf5Data, dataLayout As org.renjin.hdf5.message.DataLayoutMessage, decoder As ChunkDecoder)
			Me.file = file
			Me.dataLayout = dataLayout
			Me.rootNode = readNode(dataLayout.ChunkIndexAddress, 4096)
			Me.chunkDecoder = decoder
'JAVA TO VB CONVERTER TODO TASK: Anonymous inner classes are not converted to VB if the base type is not defined in the code being converted:
'			Me.chunkCache = org.renjin.repackaged.guava.cache.CacheBuilder.newBuilder().softValues().build(New org.renjin.repackaged.guava.cache.CacheLoader<ChunkKey, Chunk>()
	'		{
	'				@Override public Chunk load(ChunkKey key) throws Exception
	'				{
	'					Return readChunkData(key);
	'				}
	'			});

		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private ChunkNode getNode(ChunkKey key) throws java.io.IOException
		Private Function getNode(key As ChunkKey) As ChunkNode
'JAVA TO VB CONVERTER NOTE: The variable node was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim node_Renamed As ChunkNode = nodes(key.ChildPointer)
			If node_Renamed Is Nothing Then
				node_Renamed = readNode(key.ChildPointer, key.ChunkSize)
				nodes(key.ChildPointer) = node_Renamed
			End If
			Return node_Renamed
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private ChunkNode readNode(long address, int size) throws java.io.IOException
		Private Function readNode(address As Long, size As Integer) As ChunkNode
			Return New ChunkNode(dataLayout, file.readerAt(address, size))
		End Function

		''' <summary>
		''' Returns the chunk containing the value at the given
		''' </summary>
'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public Chunk chunkAt(long[] arrayIndex) throws java.io.IOException
		Public Overrides Function chunkAt(arrayIndex() As Long) As Chunk
			Debug.Assert(dataLayout.Dimensionality = arrayIndex.Length, "Invalid dimensionality")
			Return getChunk(arrayIndex)
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private Chunk getChunk(long[] index) throws java.io.IOException
		Private Function getChunk(index() As Long) As Chunk
			Dim key As ChunkKey = findNode(index)
			Try
				Return chunkCache.get(key)
			Catch e As java.util.concurrent.ExecutionException
				Throw New Exception(e)
			End Try
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private Chunk readChunkData(ChunkKey key) throws java.io.IOException
		Private Function readChunkData(key As ChunkKey) As Chunk
			Return chunkDecoder.read(key.Offset, key.ChildPointer, key.ChunkSize)
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private ChunkKey findNode(long[] chunkCoordinates) throws java.io.IOException
		Private Function findNode(chunkCoordinates() As Long) As ChunkKey

'JAVA TO VB CONVERTER NOTE: The variable node was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim node_Renamed As ChunkNode = rootNode
			Do While Not node_Renamed.Leaf
				node_Renamed = getNode(node_Renamed.findChildAddress(chunkCoordinates))
			Loop

			Return node_Renamed.findChildAddress(chunkCoordinates)
		End Function
	End Class

End Namespace