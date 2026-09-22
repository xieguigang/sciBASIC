#Region "Microsoft.VisualBasic::7a5d6cf02489deba5e2ff402853d2718, Data_science\MachineLearning\DeepLearning\RNN\net\interfaces\CharacterSampleable.vb"

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

    '   Total Lines: 28
    '    Code Lines: 6 (21.43%)
    ' Comment Lines: 20 (71.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (7.14%)
    '     File Size: 1.65 KB


    ' 	Interface CharacterSampleable
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN
	''' <summary>
	''' Network that can be sampled for a sequence of characters.
	''' </summary>
	Public Interface CharacterSampleable

		''' <summary>
		''' Samples <paramref name="length"/> characters, advancing the hidden state.
		''' </summary>
		''' <param name="length">Number of characters to sample.</param>
		''' <param name="seed">The seed text; it must contain at least one character.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>; a lower temperature yields more conservative predictions.</param>
		''' <returns>The sampled text.</returns>
		''' <exception cref="Exception">Thrown when a character of <paramref name="seed"/> is not part of the alphabet.</exception>
		Function sampleString(length As Integer, seed As String, temp As Double) As String

		''' <summary>
		''' Samples <paramref name="length"/> characters, optionally advancing the hidden state.
		''' </summary>
		''' <param name="length">Number of characters to sample.</param>
		''' <param name="seed">The seed text; it must contain at least one character.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>; a lower temperature yields more conservative predictions.</param>
		''' <param name="advance">When <c>True</c> the hidden state is advanced while consuming the seed.</param>
		''' <returns>The sampled text.</returns>
		''' <exception cref="Exception">Thrown when a character of <paramref name="seed"/> is not part of the alphabet.</exception>
		Function sampleString(length As Integer, seed As String, temp As Double, advance As Boolean) As String
	End Interface
End Namespace
