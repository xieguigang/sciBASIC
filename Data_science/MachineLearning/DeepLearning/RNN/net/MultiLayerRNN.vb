#Region "Microsoft.VisualBasic::497ecc20e571fb0b8d38b2d52cfd187d, Data_science\MachineLearning\DeepLearning\RNN\net\MultiLayerRNN.vb"

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

    '   Total Lines: 213
    '    Code Lines: 113 (53.05%)
    ' Comment Lines: 61 (28.64%)
    '    - Xml Docs: 78.69%
    ' 
    '   Blank Lines: 39 (18.31%)
    '     File Size: 6.55 KB


    ' 	Class MultiLayerRNN
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
	''' Multi layer RNN: a stack of <see cref="RNNLayer"/> instances where the output of one layer feeds the next.
	''' </summary>
	<Serializable>
	Public Class MultiLayerRNN
		Inherits BasicRNN
		' Layers

		Protected Friend layer As RNNLayer() ' All RNN layers

		' Init data

		Private m_hiddenSize As Integer()
		Private m_learningRate As Double

		Protected Friend m_initialized As Boolean

		''' <summary>
		''' Creates a net with default parameters.
		''' </summary>
		Public Sub New()
			m_learningRate = RNNLayer.defaultLearningRate
		End Sub

		''' <summary>
		''' Creates a net with default parameters and initializes it immediately.
		''' </summary>
		''' <param name="vocabularySize">The vocabulary size; it must be greater than zero.</param>
		Public Sub New(vocabularySize As Integer)
			Me.New()
			Call initialize(vocabularySize)
		End Sub

		' * Hyperparameters ** 

		''' <summary>
		''' Sets the hidden layer size of every RNN layer. The network must be initialized again afterwards.
		''' </summary>
		''' <remarks>The array must not be empty and every size must be greater than one.</remarks>
		Public Overridable WriteOnly Property HiddenSize As Integer()
			Set(value As Integer())
				m_hiddenSize = value
				m_initialized = False
			End Set
		End Property

		''' <summary>Sets the learning rate of every layer.</summary>
		Public Overridable WriteOnly Property LearningRate As Double
			Set(value As Double)
				If layer Is Nothing Then
					m_learningRate = value
				Else
					For Each layer As RNNLayer In Me.layer
						layer.LearningRate = value
					Next
				End If
			End Set
		End Property

		''' <summary>
		''' Initializes the network and creates the stacked layers for the given vocabulary size.
		''' </summary>
		''' <param name="vocabularySize">The vocabulary size; it must be greater than zero.</param>
		Public Overrides Sub initialize(vocabularySize As Integer)
			' Create layers

			If m_hiddenSize Is Nothing Then ' default: single layer
				m_hiddenSize = New Integer(0) {}
				m_hiddenSize(0) = RNNLayer.defaultHiddenSize
			End If

			layer = New RNNLayer(m_hiddenSize.Length - 1) {}

			For i = 0 To layer.Length - 1
				layer(i) = New RNNLayer()

				If i = 0 Then
					layer(i).InputSize = vocabularySize
				Else
					layer(i).InputSize = m_hiddenSize(i - 1)
				End If

				layer(i).HiddenSize = m_hiddenSize(i)
				layer(i).LearningRate = m_learningRate


				If i = layer.Length - 1 Then
					layer(i).OutputSize = vocabularySize
				Else
					layer(i).OutputSize = m_hiddenSize(i)
				End If

				layer(i).initialize()
			Next

			m_initialized = True
		End Sub

		' * Train ** 

		''' <summary>
		''' Performs a forward-backward pass for the given token indices through all stacked layers.
		''' </summary>
		''' <param name="ix">The input indices; its length must match <paramref name="iy"/> and must not be empty.</param>
		''' <param name="iy">The target indices; every index must be smaller than the vocabulary size.</param>
		''' <returns>The cross-entropy loss of this sequence.</returns>
		Public Overrides Function forwardBackward(ix As Integer(), iy As Integer()) As Double
			' forward pass
			layer(0).forward(layer(0).ixTox(ix))
			For i = 1 To layer.Length - 1
				layer(i).forward(layer(i - 1).gety())
			Next

			' calculate loss and get dy
			Dim loss = layer(layer.Length - 1).getLoss(iy)

			' last layer backward pass
			layer(layer.Length - 1).backward(layer(layer.Length - 1).getdy(iy))

			' rest of the backward pass
			For i = layer.Length - 2 To 0 Step -1
				layer(i).backward(layer(i + 1).getdx())
			Next

			Return loss
		End Function

		''' <summary>
		''' Samples <paramref name="n"/> indices, advancing the hidden state of every layer.
		''' </summary>
		''' <param name="n">Number of indices to sample.</param>
		''' <param name="seed">The seed indices.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>.</param>
		''' <returns>The sampled indices.</returns>
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()
			Return sampleIndices(n, seed, temp, True)
		End Function

		''' <summary>
		''' Samples <paramref name="n"/> indices, optionally advancing the hidden state of every layer.
		''' </summary>
		''' <param name="n">Number of indices to sample.</param>
		''' <param name="seed">The seed indices.</param>
		''' <param name="temp">Sampling temperature in <c>(0.0, 1.0]</c>.</param>
		''' <param name="advance">When <c>True</c> the hidden states are advanced while consuming the seed.</param>
		''' <returns>The sampled indices.</returns>
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer()
			Dim savedState As Matrix() = Nothing

			If Not advance Then
				savedState = New Matrix(layer.Length - 1) {}
				For i = 0 To layer.Length - 1
					savedState(i) = layer(i).saveHiddenState()
				Next
			End If

			Dim sampled = New Integer(n - 1) {}

			' Seed forward pass.
			layer(0).forward(layer(0).ixTox(seed))
			For i = 1 To layer.Length - 1
				layer(i).forward(layer(i - 1).gety())
			Next

			' first choice given seed, use the temperature
			sampled(0) = Random.randomChoice(layer(layer.Length - 1).getProbabilities(temp))

			' Sample.

			Dim seedVec = layer(0).ixTox(sampled(0))
			For t = 1 To n - 1
				layer(0).forward(seedVec)
				For i = 1 To layer.Length - 1
					layer(i).forward(layer(i - 1).gety())
				Next

				' choose next, use the temperature
				sampled(t) = Random.randomChoice(layer(layer.Length - 1).getProbabilities(temp))
				seedVec = layer(0).ixTox(sampled(t))
			Next

			If Not advance Then
				For i = 0 To layer.Length - 1
					layer(i).restoreHiddenState(savedState(i))
				Next
			End If

			Return sampled
		End Function

		''' <summary>
		''' Returns true if the net was initialized.
		''' </summary>
		''' <returns></returns>
		Public Overrides ReadOnly Property Initialized As Boolean
			Get
				Return m_initialized
			End Get
		End Property

		''' <summary>
		''' Returns the vocabulary size - max index + 1.
		''' </summary>
		''' <returns></returns>
		Public Overrides ReadOnly Property VocabularySize As Integer
			Get
				Return layer(0).InputSize
			End Get
		End Property
	End Class
End Namespace
