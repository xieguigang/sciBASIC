Namespace org.renjin.hdf5.chunked

	''' <summary>
	''' Chunk of data loaded into memory
	''' </summary>
	Public MustInherit Class Chunk

		Private ReadOnly chunkOffset() As Long

		Public Sub New(ByVal chunkOffset() As Long)
			Me.chunkOffset = chunkOffset
		End Sub

		Public Overridable Property ChunkOffset As Long()
			Get
				Return chunkOffset
			End Get
		End Property

		Public MustOverride Function getDoubleAt(ByVal i As Integer) As Double
	End Class

End Namespace