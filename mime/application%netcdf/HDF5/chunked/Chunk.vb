Namespace org.renjin.hdf5.chunked

	''' <summary>
	''' Chunk of data loaded into memory
	''' </summary>
	Public MustInherit Class Chunk

        Public Sub New(chunkOffset() As Long)
			Me.chunkOffset = chunkOffset
		End Sub

		Public Overridable Property ChunkOffset As Long()

        Public MustOverride Function getDoubleAt(i As Integer) As Double
	End Class

End Namespace