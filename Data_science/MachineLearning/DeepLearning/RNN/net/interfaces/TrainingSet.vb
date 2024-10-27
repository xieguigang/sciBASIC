Namespace RNN


	' Training set for sequences.
	Public Interface TrainingSet
		' Extracts out.length indices starting at index.
		' ix - input sequence
		' iy - expected output sequence (shifted by 1)
		Sub extract(lowerBound As Integer, ix As Integer(), iy As Integer())

		' Returns the data size.
		Function size() As Integer

		' Returns the max index + 1.
		Function vocabularySize() As Integer
	End Interface
End Namespace