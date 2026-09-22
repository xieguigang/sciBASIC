#Region "Microsoft.VisualBasic::d3a4907a75533904c9895608eccd55e1, Data_science\MachineLearning\DeepLearning\RNN\net\CharLevelRNN.vb"

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

    '   Total Lines: 51
    '    Code Lines: 17 (33.33%)
    ' Comment Lines: 27 (52.94%)
    '    - Xml Docs: 96.30%
    ' 
    '   Blank Lines: 7 (13.73%)
    '     File Size: 2.23 KB


    '     Class CharLevelRNN
    ' 
    '         Function: (+2 Overloads) sampleString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

    ''' <summary>
    ''' RNN that can use both indices, and characters as inputs/outputs.
    ''' </summary>
    <Serializable>
    Public MustInherit Class CharLevelRNN : Inherits RNN
        Implements CharacterSampleable

        ' * Initialize ** 

        ''' <summary>
        ''' Initializes the net. Requires that alphabet != null.
        ''' </summary>
        ''' <param name="alphabet"></param>
        Public MustOverride Sub initialize(alphabet As Alphabet)

        ''' <summary>
        ''' Returns the alphabet, if initialized.
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Alphabet As Alphabet

		''' <summary>
		''' Samples a string of the given length, advancing the hidden state.
		''' </summary>
		''' <param name="length">Number of characters to sample.</param>
		''' <param name="seed">The seed text used to warm up the hidden state.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>.</param>
		''' <returns>The sampled text.</returns>
		Public Overridable Function sampleString(length As Integer, seed As String, temp As Double) As String Implements CharacterSampleable.sampleString
			Return sampleString(length, seed, temp, True)
		End Function

		''' <summary>
		''' Samples a string of the given length.
		''' </summary>
		''' <param name="length">Number of characters to sample.</param>
		''' <param name="seed">The seed text used to warm up the hidden state.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>.</param>
		''' <param name="advance">When <c>True</c> the hidden state is advanced while consuming the seed.</param>
		''' <returns>The sampled text.</returns>
		Public Overridable Function sampleString(length As Integer, seed As String, temp As Double, advance As Boolean) As String Implements CharacterSampleable.sampleString
			Dim seedIndices = Alphabet.charsToIndices(seed)
            Dim sampledIndices = sampleIndices(length, seedIndices, temp, advance)
            Dim sampledChars = Alphabet.indicesToChars(sampledIndices)

            Return New String(sampledChars)
		End Function
	End Class
End Namespace
