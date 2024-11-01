#Region "Microsoft.VisualBasic::229f95da99534e81bcc700cdc71f361d, Data_science\MachineLearning\DeepLearning\RNN\math\Utils.vb"

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

    '   Total Lines: 49
    '    Code Lines: 32 (65.31%)
    ' Comment Lines: 8 (16.33%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (18.37%)
    '     File Size: 1.17 KB


    ' 	Class Utils
    ' 
    ' 	    Function: arrayCols, arrayRows, deepCopyOf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
