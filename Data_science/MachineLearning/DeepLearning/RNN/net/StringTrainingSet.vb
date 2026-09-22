#Region "Microsoft.VisualBasic::56d920974f5052252f933dff3d333599, Data_science\MachineLearning\DeepLearning\RNN\net\StringTrainingSet.vb"

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

    '   Total Lines: 85
    '    Code Lines: 51 (60.00%)
    ' Comment Lines: 17 (20.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (20.00%)
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

	''' <summary>
	''' Immutable training set for a character level RNN: a block of text together with the alphabet extracted from it.
	''' </summary>
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

		''' <summary>
		''' Creates a training set by reading a UTF-8 text file.
		''' </summary>
		''' <param name="fileName">Path of the training data file; it must not be <c>Nothing</c>.</param>
		''' <returns>The training set built from the file contents.</returns>
		Public Shared Function fromFile(fileName As String) As StringTrainingSet
			Return New StringTrainingSet(fileName.ReadAllText)
		End Function

		''' <summary>
		''' Creates a training set from a string.
		''' </summary>
		''' <param name="data">The training text.</param>
		''' <returns>The training set built from the text.</returns>
		Public Shared Function fromString(data As String) As StringTrainingSet
			Return New StringTrainingSet(data)
		End Function

		' Main functionality 

		''' <summary>
		''' Extracts a training pair starting at the given position.
		''' </summary>
		''' <param name="lowerBound">Index of the first character of the sequence.</param>
		''' <param name="ix">Receives the input sequence indices.</param>
		''' <param name="iy">Receives the expected output sequence indices, shifted by one character.</param>
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

		''' <summary>Gets the loaded training text.</summary>
		Public Overridable ReadOnly Property Data As String
			Get
				Return dataField
			End Get
		End Property

		''' <summary>Gets the alphabet extracted from the training text.</summary>
		Public Overridable ReadOnly Property Alphabet As Alphabet
			Get
				Return alphabetField
			End Get
		End Property

		''' <summary>Returns the number of characters in the training text.</summary>
		''' <returns>The data size.</returns>
		Public Overridable Function size() As Integer Implements TrainingSet.size
			Return dataField.Length
		End Function

		''' <summary>Returns the alphabet size, i.e. the vocabulary size.</summary>
		''' <returns>The number of distinct symbols.</returns>
		Public Overridable Function vocabularySize() As Integer Implements TrainingSet.vocabularySize
			Return alphabetField.size()
		End Function
	End Class

End Namespace
