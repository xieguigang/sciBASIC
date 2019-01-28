Namespace org.renjin.hdf5.chunked


	Public Class Int32Chunk
		Inherits Chunk

	  Private ReadOnly buffer As java.nio.IntBuffer
	  Private ReadOnly chunkOffset() As Long

	  Public Sub New(ByVal chunkOffset() As Long, ByVal buffer As java.nio.IntBuffer)
		MyBase.New(chunkOffset)
		Me.chunkOffset = chunkOffset
		Me.buffer = buffer
	  End Sub

	  Public Overrides Function getDoubleAt(ByVal i As Integer) As Double
		Return buffer.get(i)
	  End Function
	End Class

End Namespace