Namespace org.renjin.hdf5.chunked


	Public Class DoubleChunk
		Inherits Chunk

	  Private ReadOnly buffer As java.nio.DoubleBuffer

	  Public Sub New(ByVal chunkOffset() As Long, ByVal buffer As java.nio.DoubleBuffer)
		MyBase.New(chunkOffset)
		Me.buffer = buffer
	  End Sub

	  Public Overrides Function getDoubleAt(ByVal i As Integer) As Double
		Return buffer.get(i)
	  End Function
	End Class

End Namespace