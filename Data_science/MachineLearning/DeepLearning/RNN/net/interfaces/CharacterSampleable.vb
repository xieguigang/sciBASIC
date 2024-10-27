Namespace RNN
	Public Interface CharacterSampleable

		' Samples length characters, advances the state.
		' Seed must be at least one character.
		' temp is the must be in (0.0,1.0]. Lower temp means more conservative
		' predictions.
		' Throws, if any character in seed is not part of the alphabet.
		Function sampleString(length As Integer, seed As String, temp As Double) As String

		' Samples length characters, choose whether to advance the state.
		' Seed must be at least one character.
		' temp is the must be in (0.0,1.0]. Lower temp means more conservative
		' predictions.
		' Throws, if any character in seed is not part of the alphabet.
		Function sampleString(length As Integer, seed As String, temp As Double, advance As Boolean) As String
	End Interface
End Namespace