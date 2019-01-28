Namespace org.renjin.hdf5.vector



	''' <summary>
	''' Manages access to a chunked HDF5 dataset.
	''' </summary>
	Public Class ChunkedDataset

		Private ReadOnly dataspace As org.renjin.hdf5.message.DataspaceMessage
		Private ReadOnly datatype As org.renjin.hdf5.message.DatatypeMessage
		Private ReadOnly layout As org.renjin.hdf5.message.DataLayoutMessage
		Private ReadOnly chunkIndex As org.renjin.hdf5.chunked.ChunkIndex

		Private ReadOnly nDim As Integer
		Private dimensionSize() As Long
		Private chunkSize() As Long
		Private vectorLength As Long

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public ChunkedDataset(org.renjin.hdf5.Hdf5File file, org.renjin.hdf5.DataObject object) throws java.io.IOException
		Public Sub New(file As org.renjin.hdf5.Hdf5File, [object] As org.renjin.hdf5.DataObject)
			dataspace = [object].getMessage(GetType(org.renjin.hdf5.message.DataspaceMessage))
			datatype = [object].getMessage(GetType(org.renjin.hdf5.message.DatatypeMessage))
			layout = [object].getMessage(GetType(org.renjin.hdf5.message.DataLayoutMessage))

	'        
	'         * HDF5 lays out data in row-major order while R uses column-major order.
	'         * Following the convention of HDFArray, we will preserve the layout but transpose
	'         * the dimensions, so that will treat columns as rows and vice-versa.
	'         
			nDim = dataspace.Dimensionality
			vectorLength = 1
			dimensionSize = New Long(nDim - 1){}
			chunkSize = New Long(nDim - 1){}
			For i As Integer = 0 To nDim - 1
				dimensionSize(nDim - i - 1) = dataspace.getDimensionSize(i)
				chunkSize(nDim - i -1) = layout.getChunkSize(i)
				vectorLength *= dataspace.getDimensionSize(i)
			Next i

			chunkIndex = file.openChunkIndex([object])
		End Sub

		Private Function checkedIntCast(size As Long) As Integer
			If size > Integer.MaxValue Then
				Throw New org.renjin.eval.EvalException("Size too large: " & size)
			End If
			Return CInt(size)
		End Function


		''' <summary>
		''' Builds an R attribute list that includes this dataset's dimension.
		''' </summary>
		Public Overridable Function buildAttributes() As org.renjin.sexp.AttributeMap

			Dim intDims(nDim - 1) As Integer
			For i As Integer = 0 To nDim - 1
				intDims(i) = checkedIntCast(dimensionSize(i))
			Next i

			Return (New org.renjin.sexp.AttributeMap.Builder()).DimDim(New org.renjin.sexp.IntArrayVector(intDims)).build()
		End Function

		Public Overridable Property VectorLength32 As Integer
			Get
				Return checkedIntCast(vectorLength)
			End Get
		End Property

		Public Overridable Function vectorIndexToHdfsArrayIndex(vectorIndex As Long) As Long()
			Dim arrayIndex(nDim - 1) As Long
			Dim i As Integer = 0
			Do While i <> nDim
				arrayIndex(nDim - i - 1) = vectorIndex Mod dimensionSize(i)
				vectorIndex = (vectorIndex - arrayIndex(nDim - i - 1)) \ dimensionSize(i)
				i += 1
			Loop
			Return arrayIndex
		End Function

		Public Overridable Function hdfsArrayIndexToVectorIndex(arrayIndex() As Long) As Long
			Dim vectorIndex As Long = 0
			Dim offset As Long = 1

			Dim i As Integer = 0
			Do While i <> nDim
				vectorIndex += arrayIndex(nDim - i - 1) * offset
				offset *= dimensionSize(i)
				i += 1
			Loop

			Return vectorIndex
		End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public ChunkCursor chunkAt(int vectorIndex) throws java.io.IOException
		Public Overridable Function chunkAt(vectorIndex As Integer) As ChunkCursor
			Dim arrayIndex() As Long = vectorIndexToHdfsArrayIndex(vectorIndex)
			Dim chunk As org.renjin.hdf5.chunked.Chunk = chunkIndex.chunkAt(arrayIndex)

			Dim vectorStart As Long = hdfsArrayIndexToVectorIndex(chunk.ChunkOffset)
			Dim vectorLength As Long = chunkSize(0)

			Return New ChunkCursor(vectorStart, vectorLength, chunk)
		End Function
	End Class

End Namespace