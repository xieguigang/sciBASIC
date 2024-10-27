Namespace RNN
	' Network that can be sampled for a sequence of integers.
	Public Interface IntegerSampleable

		' Samples n indices, advances the state.
		' Seed must be at least one index.
		' temp is the must be in (0.0,1.0]. Lower temp means more conservative
		' predictions.
		Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()

		' Samples n indices, choose whether to advance the state.
		' Seed must be at least one index.
		' temp is the must be in (0.0,1.0]. Lower temp means more conservative
		' predictions.
		Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer()
	End Interface
End Namespace