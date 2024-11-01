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
