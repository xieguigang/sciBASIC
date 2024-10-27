Imports std = System.Math

Namespace RNN

	''' <summary>
	''' Math helper functions
	''' </summary>
	Public Class Math
		Public Const compareEpsilon As Double = 0.000001

		''' <summary>
		''' Double epsilon compare 
		''' </summary>
		''' <param name="a"></param>
		''' <param name="b"></param>
		''' <returns></returns>
		Public Shared Function close(a As Double, b As Double) As Boolean
			Return std.Abs(a - b) <= compareEpsilon
		End Function

		Public Shared Function close(a As Double, b As Double, eps As Double) As Boolean
			Return std.Abs(a - b) <= eps
		End Function

		' return the comparison epsilon
		Public Shared Function eps() As Double
			Return compareEpsilon
		End Function

		' Useful Matrix functions 

		' Applies the softmax function with temperature = 1.0
		Public Shared Function softmax(yAtt As Matrix) As Matrix
			Dim e_to_x As Matrix = (New Matrix(yAtt)).exp()
			e_to_x = e_to_x.div(e_to_x.sum())
			Return e_to_x
		End Function

		' Applies the softmax function with the given temperature.
		' Temperature can't be close to 0.
		Public Shared Function softmax(yAtt As Matrix, temperature As Double) As Matrix
			Dim e_to_x As Matrix = (New Matrix(yAtt)).div(temperature).exp()
			e_to_x = e_to_x.div(e_to_x.sum())
			Return e_to_x
		End Function
	End Class

End Namespace