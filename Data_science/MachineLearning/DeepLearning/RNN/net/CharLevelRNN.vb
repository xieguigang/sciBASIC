#Region "Microsoft.VisualBasic::59d99f299ee33c9879cd970e39c22416, Data_science\MachineLearning\DeepLearning\RNN\net\CharLevelRNN.vb"

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

    '   Total Lines: 38
    '    Code Lines: 17 (44.74%)
    ' Comment Lines: 14 (36.84%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 7 (18.42%)
    '     File Size: 1.44 KB


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
		''' * Sample ** </summary>
		Public Overridable Function sampleString(length As Integer, seed As String, temp As Double) As String Implements CharacterSampleable.sampleString
			Return sampleString(length, seed, temp, True)
		End Function

		Public Overridable Function sampleString(length As Integer, seed As String, temp As Double, advance As Boolean) As String Implements CharacterSampleable.sampleString
			Dim seedIndices = Alphabet.charsToIndices(seed)
            Dim sampledIndices = sampleIndices(length, seedIndices, temp, advance)
            Dim sampledChars = Alphabet.indicesToChars(sampledIndices)

            Return New String(sampledChars)
		End Function
	End Class
End Namespace
