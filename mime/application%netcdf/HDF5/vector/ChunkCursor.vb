Namespace org.renjin.hdf5.vector



	Public Class ChunkCursor


		Private ReadOnly vectorOffset As Long
		Private ReadOnly vectorLength As Long
		Private chunk As org.renjin.hdf5.chunked.Chunk

		Public Sub New(ByVal vectorOffset As Long, ByVal vectorLength As Long, ByVal chunk As org.renjin.hdf5.chunked.Chunk)

			Me.vectorOffset = vectorOffset
			Me.vectorLength = vectorLength
			Me.chunk = chunk
		End Sub

		Public Overridable Function containsVectorIndex(ByVal vectorIndex As Integer) As Boolean
			Return vectorIndex >= vectorOffset AndAlso vectorIndex < (vectorOffset + vectorLength)
		End Function

		Public Overridable Function valueAt(ByVal i As Integer) As Double
			Return chunk.getDoubleAt((CInt(i - vectorOffset)))
		End Function
	End Class

End Namespace