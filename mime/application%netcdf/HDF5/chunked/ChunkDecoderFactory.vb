Imports System.Collections.Generic

Namespace org.renjin.hdf5.chunked



	Public Class ChunkDecoderFactory

	  Private file As org.renjin.hdf5.Hdf5Data

	  Public Sub New(file As org.renjin.hdf5.Hdf5Data)
		Me.file = file
	  End Sub

	  Public Overridable Function create(datatype As org.renjin.hdf5.message.DatatypeMessage, dataLayout As org.renjin.hdf5.message.DataLayoutMessage, dataStorage As org.renjin.repackaged.guava.base.Optional(Of org.renjin.hdf5.message.DataStorageMessage)) As ChunkDecoder

		Dim factory As ChunkFactory = createFactory(datatype)

		Dim filters As New List(Of org.renjin.hdf5.message.Filter)()
		If dataStorage.Present Then
		  filters.AddRange(dataStorage.get().Filters)
		End If

		If filters.Count = 0 Then
		  Return New UncompressedDecoder(file, factory)

		ElseIf filters.Count = 1 Then
		  Select Case filters(0).FilterId
			Case org.renjin.hdf5.message.Filter.FILTER_DEFLATE
			  Return New InflateDecoder(file, dataLayout, factory)
		  End Select
		  Throw New System.NotSupportedException("Filter: " & filters(0).FilterId)

		Else
		  Throw New System.NotSupportedException("Filters: " & filters)
		End If
	  End Function

'JAVA TO VB CONVERTER WARNING: 'final' parameters are not allowed in .NET:
'ORIGINAL LINE: public ChunkFactory createFactory(final org.renjin.hdf5.message.DatatypeMessage datatype)
	  Public Overridable Function createFactory(datatype As org.renjin.hdf5.message.DatatypeMessage) As ChunkFactory
		If datatype.DoubleIEE754 Then
		  Return New ChunkFactoryAnonymousInnerClassHelper()

		ElseIf datatype.SignedInteger32 Then
		  Return New ChunkFactoryAnonymousInnerClassHelper2()
		End If

		Throw New System.NotSupportedException("Datatype: " & datatype)
	  End Function

	  Private Class ChunkFactoryAnonymousInnerClassHelper
		  Implements ChunkFactory

		  Public Overrides Function wrap(chunkOffset() As Long, buffer As java.nio.ByteBuffer) As Chunk Implements ChunkFactory.wrap
			buffer.order(datatype.ByteOrder)
			Return New DoubleChunk(chunkOffset, buffer.asDoubleBuffer())
		  End Function
	  End Class

	  Private Class ChunkFactoryAnonymousInnerClassHelper2
		  Implements ChunkFactory

		  Public Overrides Function wrap(chunkOffset() As Long, buffer As java.nio.ByteBuffer) As Chunk Implements ChunkFactory.wrap
			buffer.order(datatype.ByteOrder)
			Return New Int32Chunk(chunkOffset, buffer.asIntBuffer())
		  End Function
	  End Class
	End Class

End Namespace