#Region "Microsoft.VisualBasic::9361aeca5d03b8f144a21a49213446b4, Data_science\MachineLearning\DeepLearning\RNN\net\interfaces\TrainingSet.vb"

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

    '   Total Lines: 17
    '    Code Lines: 7 (41.18%)
    ' Comment Lines: 6 (35.29%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (23.53%)
    '     File Size: 444 B


    ' 	Interface TrainingSet
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN


	''' <summary>
	''' Training set for sequences.
	''' </summary>
	Public Interface TrainingSet
		''' <summary>
		''' Extracts a training pair starting at the given position.
		''' </summary>
		''' <param name="lowerBound">Index of the first symbol of the sequence.</param>
		''' <param name="ix">Receives the input sequence indices.</param>
		''' <param name="iy">Receives the expected output sequence indices, shifted by one symbol.</param>
		Sub extract(lowerBound As Integer, ix As Integer(), iy As Integer())

		''' <summary>Returns the data size.</summary>
		''' <returns>The number of symbols in the training data.</returns>
		Function size() As Integer

		''' <summary>Returns the vocabulary size.</summary>
		''' <returns>The maximum index plus one.</returns>
		Function vocabularySize() As Integer
	End Interface
End Namespace
