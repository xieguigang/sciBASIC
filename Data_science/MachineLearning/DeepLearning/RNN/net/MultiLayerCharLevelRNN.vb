#Region "Microsoft.VisualBasic::d82fbe50cd35a5640c595cd7e174eb70, Data_science\MachineLearning\DeepLearning\RNN\net\MultiLayerCharLevelRNN.vb"

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

    '   Total Lines: 128
    '    Code Lines: 48 (37.50%)
    ' Comment Lines: 64 (50.00%)
    '    - Xml Docs: 95.31%
    ' 
    '   Blank Lines: 16 (12.50%)
    '     File Size: 4.29 KB


    ' 	Class MultiLayerCharLevelRNN
    ' 
    ' 	    Properties: Alphabet, Initialized, VocabularySize
    ' 
    ' 	    Constructor: (+2 Overloads) Sub New
    ' 
    ' 	    Function: forwardBackward, (+2 Overloads) sampleIndices
    ' 
    ' 	    Sub: initialize, SetHiddenSize, SetLearningRate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

	''' <summary>
	''' Multi layer character level RNN: a character level network backed by a stack of RNN layers.
	''' </summary>
	<Serializable>
	Public Class MultiLayerCharLevelRNN : Inherits CharLevelRNN

		''' <summary>
		''' The alphabet for sampling.
		''' </summary>
		Protected Friend m_alphabet As Alphabet
		''' <summary>
		''' Basic network.
		''' </summary>
		Protected Friend internal As MultiLayerRNN

		''' <summary>
		''' Constructs without initialization.
		''' </summary>
		Public Sub New()
			internal = New MultiLayerRNN()
		End Sub

		''' <summary>
		''' Creates the network and initializes it immediately.
		''' </summary>
		''' <param name="alphabet">The alphabet; it must not be <c>Nothing</c>.</param>
		Public Sub New(alphabet As Alphabet)
			Me.New()
			initialize(alphabet)
		End Sub

		' * Hyperparameters ** 

		''' <summary>
		''' Sets the hidden layer sizes of the wrapped network. The network must be initialized again afterwards.
		''' </summary>
		''' <param name="value">The hidden size of every layer.</param>
		Public Sub SetHiddenSize(value As Integer())
			internal.HiddenSize = value
		End Sub

		''' <summary>
		''' Sets the learning rate of the wrapped network.
		''' </summary>
		''' <param name="value">The learning rate.</param>
		Public Sub SetLearningRate(value As Double)
			internal.LearningRate = value
		End Sub

		' * Initialize ** 

		''' <summary>
		''' Initializes the network for the given alphabet.
		''' </summary>
		''' <param name="alphabet">The alphabet; it must not be <c>Nothing</c>.</param>
		Public Overrides Sub initialize(alphabet As Alphabet)
			m_alphabet = alphabet
			internal.initialize(alphabet.size())
		End Sub

		' * Train ** 

		''' <summary>
		''' Performs a forward-backward pass for the given token indices.
		''' </summary>
		''' <param name="ix">The input indices; its length must match <paramref name="iy"/> and must not be empty.</param>
		''' <param name="iy">The target indices; every index must be smaller than the vocabulary size.</param>
		''' <returns>The cross-entropy loss of this sequence.</returns>
		Public Overrides Function forwardBackward(ix As Integer(), iy As Integer()) As Double
			Return internal.forwardBackward(ix, iy)
		End Function

		''' <summary>
		''' Samples <paramref name="n"/> indices, advancing the hidden state.
		''' </summary>
		''' <param name="n">Number of indices to sample.</param>
		''' <param name="seed">The seed indices.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>.</param>
		''' <returns>The sampled indices.</returns>
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()
			Return internal.sampleIndices(n, seed, temp)
		End Function

		''' <summary>
		''' Samples <paramref name="n"/> indices, optionally advancing the hidden state.
		''' </summary>
		''' <param name="n">Number of indices to sample.</param>
		''' <param name="seed">The seed indices.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>.</param>
		''' <param name="advance">When <c>True</c> the hidden state is advanced while consuming the seed.</param>
		''' <returns>The sampled indices.</returns>
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer()
			Return internal.sampleIndices(n, seed, temp, advance)
		End Function

		''' <summary>
		''' Returns the alphabet, if initialized.
		''' </summary>
		''' <returns></returns>
		Public Overrides ReadOnly Property Alphabet As Alphabet
			Get
				Return m_alphabet
			End Get
		End Property

		''' <summary>
		''' Returns true if the net was initialized.
		''' </summary>
		''' <returns></returns>
		Public Overrides ReadOnly Property Initialized As Boolean
			Get
				Return internal.Initialized
			End Get
		End Property

		''' <summary>
		''' Returns the vocabulary size (the alphabet size), if initialized.
		''' </summary>
		''' <returns></returns>
		Public Overrides ReadOnly Property VocabularySize As Integer
			Get
				Return internal.VocabularySize
			End Get
		End Property
	End Class
End Namespace
