Namespace org.renjin.hdf5.vector




	Public Class ChunkedDoubleVector
		Inherits org.renjin.sexp.DoubleVector

		Private ReadOnly dataset As ChunkedDataset
		Private chunk As ChunkCursor = Nothing

		Public Sub New(ByVal dataset As ChunkedDataset)
			MyBase.New(dataset.buildAttributes())
			Me.dataset = dataset
		End Sub

		Public Sub New(ByVal dataset As ChunkedDataset, ByVal attributeMap As org.renjin.sexp.AttributeMap)
			MyBase.New(attributeMap)
			Me.dataset = dataset
		End Sub

		Protected Friend Overrides Function cloneWithNewAttributes(ByVal attributeMap As org.renjin.sexp.AttributeMap) As org.renjin.sexp.SEXP
			Return New ChunkedDoubleVector(Me.dataset, attributeMap)
		End Function

		Public Overrides Function getElementAsDouble(ByVal i As Integer) As Double

			If chunk Is Nothing OrElse (Not chunk.containsVectorIndex(i)) Then
				Try
					chunk = dataset.chunkAt(i)
				Catch e As java.io.IOException
					Throw New org.renjin.eval.EvalException("I/O Error while accessing HDF5 File: " & e.Message, e)
				End Try
			End If

			Return chunk.valueAt(i)
		End Function

		Public Property Overrides ConstantAccessTime As Boolean
			Get
				Return True
			End Get
		End Property

		Public Overrides Function length() As Integer
			Return dataset.VectorLength32
		End Function
	End Class

End Namespace