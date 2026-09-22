#Region "Microsoft.VisualBasic::3c32318ac6860bd46e238c2a9991aba2, Data_science\MachineLearning\DeepLearning\RNN\math\Utils.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 61
    '    Code Lines: 32 (52.46%)
    ' Comment Lines: 20 (32.79%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 9 (14.75%)
    '     File Size: 1.84 KB


    ' 	Class Utils
    ' 
    ' 	    Function: arrayCols, arrayRows, deepCopyOf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

	''' <summary>
	''' Utility functions for working with the jagged <c>Double()()</c> arrays used by the RNN matrix type.
	''' </summary>
	Public Class Utils
		' Utilities for 2D arrays. 

		''' <summary>
		''' Performs a deep copy of a 2D array of doubles.
		''' </summary>
		''' <param name="src">The source array; <c>Nothing</c> is returned unchanged.</param>
		''' <returns>A deep copy of <paramref name="src"/>.</returns>
		Public Shared Function deepCopyOf(src As Double()()) As Double()()
			If src Is Nothing Then
				Return Nothing
			End If

			Dim dst = New Double(src.Length - 1)() {}
            For i As Integer = 0 To src.Length - 1
                If src(i) IsNot Nothing Then
                    dst(i) = src(i).CopyOf(src(i).Length)
                End If
            Next

            Return dst
		End Function

		''' <summary>
		''' Returns the row count of a 2D array of doubles.
		''' </summary>
		''' <param name="array">The array to inspect; <c>Nothing</c> is treated as size zero.</param>
		''' <returns>The number of rows.</returns>
		Public Shared Function arrayRows(array As Double()()) As Integer
			If array Is Nothing Then
				Return 0
			End If
			Return array.Length
		End Function

		''' <summary>
		''' Returns the column count of a 2D array of doubles.
		''' </summary>
		''' <param name="array">The array to inspect; <c>Nothing</c> is treated as size zero.</param>
		''' <returns>The number of columns, taken from the first row.</returns>
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
