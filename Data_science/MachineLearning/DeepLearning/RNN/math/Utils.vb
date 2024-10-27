Namespace RNN

	' Utility functions.
	Public Class Utils
		' Utilities for 2D arrays. 

		' Performs a deep copy of a 2D array of doubles.
		Public Shared Function deepCopyOf(src As Double()()) As Double()()
			If src Is Nothing Then
				Return Nothing
			End If

			Dim dst = New Double(src.Length - 1)() {}
			For i = 0 To src.Length - 1
				If src(i) IsNot Nothing Then
					dst(i) = src(i).CopyOf(src(i).Length)
				End If
			Next

			Return dst
		End Function

		' Returns the row count of a 2D array of doubles.
		' Treats null as size-0 array.
		Public Shared Function arrayRows(array As Double()()) As Integer
			If array Is Nothing Then
				Return 0
			End If
			Return array.Length
		End Function

		' Returns the col count of a 2D array of doubles.
		' Throws, if array is not rectangular. Treats null as size-0 array.
		Public Shared Function arrayCols(array As Double()()) As Integer
			If array Is Nothing Then
				Return 0
			End If

			' store expected length
			Dim length = 0
			If array(0) IsNot Nothing Then
				length = array(0).Length
			End If

			Return length
		End Function
	End Class

End Namespace