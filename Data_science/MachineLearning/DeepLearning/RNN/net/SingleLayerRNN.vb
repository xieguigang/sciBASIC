#Region "Microsoft.VisualBasic::4a23e4d2168b6168c6166fec187f469c, Data_science\MachineLearning\DeepLearning\RNN\net\SingleLayerRNN.vb"

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

    '   Total Lines: 136
    '    Code Lines: 67 (49.26%)
    ' Comment Lines: 43 (31.62%)
    '    - Xml Docs: 93.02%
    ' 
    '   Blank Lines: 26 (19.12%)
    '     File Size: 4.49 KB


    ' 	Class SingleLayerRNN
    ' 
    ' 	    Properties: HiddenSize, Initialized, LearningRate, VocabularySize
    ' 
    ' 	    Constructor: (+2 Overloads) Sub New
    ' 
    ' 	    Function: forwardBackward, (+2 Overloads) sampleIndices
    ' 
    ' 	    Sub: initialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

	''' <summary>
	''' Single layer RNN.
	''' </summary>
	<Serializable>
	Public Class SingleLayerRNN
		Inherits BasicRNN

		''' <summary>
		''' The single RNN layer
		''' </summary>
		Protected Friend layer As RNNLayer

		Private m_initialized As Boolean

		''' <summary>Creates a net with default parameters.</summary>
		Public Sub New()
			layer = New RNNLayer()
		End Sub

		''' <summary>
		''' Creates a net with default parameters and initializes it immediately.
		''' </summary>
		''' <param name="vocabularySize">The vocabulary size; it must be greater than zero.</param>
		Public Sub New(vocabularySize As Integer)
			Me.New()
			initialize(vocabularySize)
		End Sub

		' * Hyperparameters ** 

		''' <summary>Sets the hidden layer size. The network must be initialized again afterwards.</summary>
		Public Overridable WriteOnly Property HiddenSize As Integer
			Set(value As Integer)
				layer.HiddenSize = value
				m_initialized = False
			End Set
		End Property

		''' <summary>Sets the learning rate of the single layer.</summary>
		Public Overridable WriteOnly Property LearningRate As Double
			Set(value As Double)
				layer.LearningRate = value
			End Set
		End Property

		''' <summary>
		''' Initializes the network for the given vocabulary size.
		''' </summary>
		''' <param name="vocabularySize">The vocabulary size; it must be greater than zero.</param>
		Public Overrides Sub initialize(vocabularySize As Integer)
			' Set the layer parameters.

			layer.InputSize = vocabularySize
			layer.OutputSize = vocabularySize

			layer.initialize()

			m_initialized = True
		End Sub

		' * Train ** 

		''' <summary>
		''' Performs a forward-backward pass for the given token indices.
		''' </summary>
		''' <param name="ix">The input indices; its length must match <paramref name="iy"/> and must not be empty.</param>
		''' <param name="iy">The target indices; every index must be smaller than the vocabulary size.</param>
		''' <returns>The cross-entropy loss of this sequence.</returns>
		Public Overrides Function forwardBackward(ix As Integer(), iy As Integer()) As Double
			layer.forward(layer.ixTox(ix))
			Dim loss = layer.getLoss(iy)
			layer.backward(layer.getdy(iy))

			Return loss
		End Function

		''' <summary>
		''' Samples <paramref name="n"/> indices, advancing the hidden state.
		''' </summary>
		''' <param name="n">Number of indices to sample.</param>
		''' <param name="seed">The seed indices.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>.</param>
		''' <returns>The sampled indices.</returns>
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()
			Return sampleIndices(n, seed, temp, True)
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
			Dim savedState As Matrix = If(Not advance, layer.saveHiddenState(), Nothing)

			Dim sampled = New Integer(n - 1) {}

			layer.forward(layer.ixTox(seed))
			sampled(0) = Random.randomChoice(layer.getProbabilities(temp)) ' first choice given seed


			Dim seedVec = layer.ixTox(sampled(0))
			For t = 1 To n - 1
				layer.forward(seedVec)
				sampled(t) = Random.randomChoice(layer.getProbabilities(temp))
				seedVec = layer.ixTox(sampled(t))
			Next

			If Not advance Then
				layer.restoreHiddenState(savedState)
			End If

			Return sampled
		End Function

		''' <summary>Gets a value indicating whether the network has been initialized.</summary>
		Public Overrides ReadOnly Property Initialized As Boolean
			Get
				Return m_initialized
			End Get
		End Property

		''' <summary>Gets the vocabulary size, i.e. the maximum index plus one.</summary>
		Public Overrides ReadOnly Property VocabularySize As Integer
			Get
				Return layer.InputSize
			End Get
		End Property
	End Class

End Namespace
