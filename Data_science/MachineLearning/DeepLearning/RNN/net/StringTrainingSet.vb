#Region "Microsoft.VisualBasic::011791a1a401550a8f34b5f23f7c0123, Data_science\MachineLearning\DeepLearning\RNN\net\StringTrainingSet.vb"

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

    '   Total Lines: 84
    '    Code Lines: 51 (60.71%)
    ' Comment Lines: 17 (20.24%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (19.05%)
    '     File Size: 2.35 KB


    ' 	Class StringTrainingSet
    ' 
    ' 	    Properties: Alphabet, Data
    ' 
    ' 	    Constructor: (+1 Overloads) Sub New
    ' 
    ' 	    Function: fromFile, fromString, size, vocabularySize
    ' 
    ' 	    Sub: extract
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

	' Immutable training set for a character level RNN.
	Public Class StringTrainingSet
		Implements TrainingSet
		Private dataField As String ' Data from file.
		Private alphabetField As Alphabet ' Alphabet extracted from data.

		' Constructs from data. Treats null as an empty string.
		Private Sub New(data As String)
			If data Is Nothing Then
				data = ""
			End If

			dataField = data
			alphabetField = Alphabet.fromString(data)
		End Sub

		' Create 

		' Returns a training set with data from file (UTF-8).
		' Requires fileName != null.
		Public Shared Function fromFile(fileName As String) As StringTrainingSet
			Return New StringTrainingSet(fileName.ReadAllText)
		End Function

		' Returns a training set created from a string.
		Public Shared Function fromString(data As String) As StringTrainingSet
			Return New StringTrainingSet(data)
		End Function

		' Main functionality 

		' Extracts out.length indices starting at index.
		' ix - input sequence
		' iy - expected output sequence (shifted by 1)
		Public Overridable Sub extract(lowerBound As Integer, ix As Integer(), iy As Integer()) Implements TrainingSet.extract
			' fetch one more symbol than the length.
			Dim upperBound = lowerBound + iy.Length + 1

			' prepare the input/output arrays
			Dim firstCharI As Integer
			Dim secondCharI = alphabetField.charToIndex(dataField(lowerBound))
			Dim t = 0
			Dim j = lowerBound + t + 1

			While j < upperBound
				firstCharI = secondCharI
				secondCharI = alphabetField.charToIndex(dataField(j))
				ix(t) = firstCharI
				iy(t) = secondCharI
				j += 1
				t += 1
			End While
		End Sub

		' Getters 

		' Returns the loaded data.
		Public Overridable ReadOnly Property Data As String
			Get
				Return dataField
			End Get
		End Property

		' Returns the alphabet.
		Public Overridable ReadOnly Property Alphabet As Alphabet
			Get
				Return alphabetField
			End Get
		End Property

		' Returns data size.
		Public Overridable Function size() As Integer Implements TrainingSet.size
			Return dataField.Length
		End Function

		' Returns the alphabet size.
		Public Overridable Function vocabularySize() As Integer Implements TrainingSet.vocabularySize
			Return alphabetField.size()
		End Function
	End Class

End Namespace
