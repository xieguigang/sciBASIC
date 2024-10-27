Namespace RNN

	' A recurrent neural network.
	<Serializable>
	Public MustInherit Class RNN
		Implements IntegerSampleable, Trainable
		Public MustOverride Function forwardBackward(ix As Integer(), iy As Integer()) As Double Implements Trainable.forwardBackward
		Public MustOverride Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer() Implements IntegerSampleable.sampleIndices
		Public MustOverride Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer() Implements IntegerSampleable.sampleIndices

		' * Get ** 

		' Returns true if the net was initialized.
		Public MustOverride ReadOnly Property Initialized As Boolean

		' Returns the vocabulary size (max index + 1), if initialized.
		Public MustOverride ReadOnly Property VocabularySize As Integer
	End Class
End Namespace