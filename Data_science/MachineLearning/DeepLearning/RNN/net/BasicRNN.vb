Namespace RNN

	' RNN that uses integer indices as inputs and outputs.
	<Serializable>
	Public MustInherit Class BasicRNN
		Inherits RNN
		' Initializes the net for this vocabulary size.
		' Requires vocabularySize > 0.
		Public MustOverride Sub initialize(vocabularySize As Integer)
	End Class
End Namespace