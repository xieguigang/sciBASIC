Namespace org.renjin.hdf5.chunked




	Public Class InflateDecoder
		Implements ChunkDecoder

		Private inf As New java.util.zip.Inflater()

		Private file As org.renjin.hdf5.Hdf5Data

		''' <summary>
		''' The number of data *elements* (not bytes) in each chunk.
		''' </summary>
		Private chunkSize As Integer


		''' <summary>
		''' Array backing the buffer for compressed data read in from the file.
		''' </summary>
		Private deflatedBufferArray() As SByte

		''' <summary>
		''' The buffer for compressed data read in from the file.
		''' </summary>
		Private deflatedBuffer As java.nio.ByteBuffer

		Private ReadOnly chunkFactory As ChunkFactory

		Private ReadOnly chunkSizeBytes As Integer

		Public Sub New(file As org.renjin.hdf5.Hdf5Data, dataLayout As org.renjin.hdf5.message.DataLayoutMessage, chunkFactory As ChunkFactory)
			Me.file = file
			Me.chunkSize = org.renjin.repackaged.guava.primitives.Ints.checkedCast(dataLayout.ChunkElementCount)
			Me.chunkFactory = chunkFactory
			Me.chunkSizeBytes = Me.chunkSize * dataLayout.DatasetElementSize
		End Sub


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public Chunk read(long[] chunkOffset, long address, int size) throws java.io.IOException
		Public Overrides Function read(chunkOffset() As Long, address As Long, size As Integer) As Chunk Implements ChunkDecoder.read

			' Set up our buffer for reading in the compressed data
			If deflatedBufferArray Is Nothing OrElse deflatedBufferArray.Length < size Then
				deflatedBufferArray = New SByte(size - 1){}
				deflatedBuffer = java.nio.ByteBuffer.wrap(deflatedBufferArray)
			End If

			' Read the compressed chunk into our buffer
			deflatedBuffer.position(0)
			deflatedBuffer.limit(size)
			file.read(deflatedBuffer, address)

			' Deflate from the compressed buffer into the uncompressed buffer
			Dim buffer(chunkSizeBytes - 1) As SByte
			inf.reset()
			inf.setInput(deflatedBufferArray, 0, size)
			Dim [off] As Integer = 0
			Dim len As Integer = buffer.Length
			Dim n As Integer
            Try
                n = inf.inflate(buffer, [off], len)
                Do While n = 0
                    If inf.finished() OrElse inf.needsDictionary() Then
                        Exit Do
                    End If
                    If inf.needsInput() Then
                        Throw New Exception("Unexpected end of deflated chunk.")
                    End If
                    [off] += n
                    len -= n
                    n = inf.inflate(buffer, [off], len)
                Loop
            Catch e As Exception
                Throw New Exception(e)
            End Try

			Return chunkFactory.wrap(chunkOffset, java.nio.ByteBuffer.wrap(buffer))
	'
	'        // Allocate a new array of doubles and decode the uncompressed data
	'        // into floating point numbers
	'
	'        double values[] = new double[chunkSize];
	'        int bi = 0;
	'        for (int di = 0; di < chunkSize; di++) {
	'            long longValue =
	'                (buffer[bi+7] & 0xFFL) << 56
	'                | (buffer[bi+6] & 0xFFL) << 48
	'                | (buffer[bi+5] & 0xFFL) << 40
	'                | (buffer[bi+4] & 0xFFL) << 32
	'                | (buffer[bi+3] & 0xFFL) << 24
	'                | (buffer[bi+2] & 0xFFL) << 16
	'                | (buffer[bi+1] & 0xFFL) << 8
	'                | (buffer[bi] & 0xFFL);
	'
	'            values[di] = Double.longBitsToDouble(longValue);
	'            bi+=8;
	'        }
	'
	'        return new DoubleChunk()DoubleBuffer.wrap(values);
		End Function
	End Class

End Namespace