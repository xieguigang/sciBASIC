Namespace org.renjin.hdf5.vector



	Public Class ChunkCursor


		Private ReadOnly vectorOffset As Long
		Private ReadOnly vectorLength As Long
		Private chunk As org.renjin.hdf5.chunked.Chunk

		Public Sub New(vectorOffset As Long, vectorLength As Long, chunk As org.renjin.hdf5.chunked.Chunk)

			Me.vectorOffset = vectorOffset
			Me.vectorLength = vectorLength
			Me.chunk = chunk
		End Sub

		Public Overridable Function containsVectorIndex(vectorIndex As Integer) As Boolean
			Return vectorIndex >= vectorOffset AndAlso vectorIndex < (vectorOffset + vectorLength)
		End Function

		Public Overridable Function valueAt(i As Integer) As Double
			Return chunk.getDoubleAt((CInt(i - vectorOffset)))
		End Function
	End Class

End Namespace