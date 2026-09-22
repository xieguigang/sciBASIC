#Region "Microsoft.VisualBasic::97f53272e58f1a2881a84bd30d751d92, Data_science\MachineLearning\DeepLearning\RNN\net\interfaces\IntegerSampleable.vb"

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
    '    Code Lines: 6 (35.29%)
    ' Comment Lines: 9 (52.94%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (11.76%)
    '     File Size: 706 B


    ' 	Interface IntegerSampleable
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN
	''' <summary>
	''' Network that can be sampled for a sequence of integer indices.
	''' </summary>
	Public Interface IntegerSampleable

		''' <summary>
		''' Samples <paramref name="n"/> indices, advancing the hidden state.
		''' </summary>
		''' <param name="n">Number of indices to sample.</param>
		''' <param name="seed">The seed indices; there must be at least one.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>; a lower temperature yields more conservative predictions.</param>
		''' <returns>The sampled indices.</returns>
		Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()

		''' <summary>
		''' Samples <paramref name="n"/> indices, optionally advancing the hidden state.
		''' </summary>
		''' <param name="n">Number of indices to sample.</param>
		''' <param name="seed">The seed indices; there must be at least one.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>; a lower temperature yields more conservative predictions.</param>
		''' <param name="advance">When <c>True</c> the hidden state is advanced while consuming the seed.</param>
		''' <returns>The sampled indices.</returns>
		Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer()
	End Interface
End Namespace
