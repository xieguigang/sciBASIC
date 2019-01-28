Namespace org.renjin.hdf5.vector




	Public Class ChunkedDoubleVector
        Inherits DoubleVector

        Private ReadOnly dataset As ChunkedDataset
		Private chunk As ChunkCursor = Nothing

		Public Sub New(dataset As ChunkedDataset)
			MyBase.New(dataset.buildAttributes())
			Me.dataset = dataset
		End Sub

		Public Sub New(dataset As ChunkedDataset, attributeMap As org.renjin.sexp.AttributeMap)
			MyBase.New(attributeMap)
			Me.dataset = dataset
		End Sub

		Protected Friend Overrides Function cloneWithNewAttributes(attributeMap As org.renjin.sexp.AttributeMap) As org.renjin.sexp.SEXP
			Return New ChunkedDoubleVector(Me.dataset, attributeMap)
		End Function

		Public Overrides Function getElementAsDouble(i As Integer) As Double

			If chunk Is Nothing OrElse (Not chunk.containsVectorIndex(i)) Then
                Try
                    chunk = dataset.chunkAt(i)
                Catch e As Exception
                    Throw New Exception("I/O Error while accessing HDF5 File: " & e.Message, e)
                End Try
			End If

			Return chunk.valueAt(i)
		End Function

        Public ReadOnly Property ConstantAccessTime As Boolean
            Get
                Return True
            End Get
        End Property

        Public ReadOnly Property length() As Integer
            Get
                Return dataset.VectorLength32
            End Get
        End Property
    End Class

End Namespace