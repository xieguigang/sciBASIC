Namespace RNN

	' RNN that can use both indices, and characters as inputs/outputs.
	<Serializable>
	Public MustInherit Class CharLevelRNN
		Inherits RNN
		Implements CharacterSampleable
		''' <summary>
		''' * Initialize ** </summary>

		' Initializes the net. Requires that alphabet != null.
		Public MustOverride Sub initialize(alphabet As Alphabet)

		''' <summary>
		''' * Get ** </summary>

		' Returns the alphabet, if initialized.
		Public MustOverride ReadOnly Property Alphabet As Alphabet

		''' <summary>
		''' * Sample ** </summary>
		Public Overridable Function sampleString(length As Integer, seed As String, temp As Double) As String Implements CharacterSampleable.sampleString
			Return sampleString(length, seed, temp, True)
		End Function

		Public Overridable Function sampleString(length As Integer, seed As String, temp As Double, advance As Boolean) As String Implements CharacterSampleable.sampleString
			Dim seedIndices = Alphabet.charsToIndices(seed)

			Dim sampledIndices = sampleIndices(length, seedIndices, temp, advance)

			Dim sampledChars = Alphabet.indicesToChars(sampledIndices)

			Return New String(sampledChars)
		End Function
	End Class
End Namespace