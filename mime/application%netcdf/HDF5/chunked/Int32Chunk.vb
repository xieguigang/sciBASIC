Namespace org.renjin.hdf5.chunked


	Public Class Int32Chunk
		Inherits Chunk

	  Private ReadOnly buffer As java.nio.IntBuffer
        Private ReadOnly Property GetChunkOffset() As Long

        Public Sub New(chunkOffset() As Long, buffer As java.nio.IntBuffer)
		MyBase.New(chunkOffset)
		Me.chunkOffset = chunkOffset
		Me.buffer = buffer
	  End Sub

	  Public Overrides Function getDoubleAt(i As Integer) As Double
		Return buffer.get(i)
	  End Function
	End Class

End Namespace