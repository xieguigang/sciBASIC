#Region "Microsoft.VisualBasic::1481c0f9fce493a753ca9fbef54355c2, Data_science\MachineLearning\DeepLearning\RNN\net\SingleLayerRNN.vb"

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

    '   Total Lines: 127
    '    Code Lines: 67 (52.76%)
    ' Comment Lines: 31 (24.41%)
    '    - Xml Docs: 32.26%
    ' 
    '   Blank Lines: 29 (22.83%)
    '     File Size: 3.43 KB


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

		' Creates a net with default parameters.
		Public Sub New()
			layer = New RNNLayer()
		End Sub

		' Creates a net with default parameters and initializes immediately.
		Public Sub New(vocabularySize As Integer)
			Me.New()
			initialize(vocabularySize)
		End Sub

		' * Hyperparameters ** 

		' Sets the hidden layer size. Network must be initialized again.
		Public Overridable WriteOnly Property HiddenSize As Integer
			Set(value As Integer)
				layer.HiddenSize = value
				m_initialized = False
			End Set
		End Property

		' Sets the learning rate.
		Public Overridable WriteOnly Property LearningRate As Double
			Set(value As Double)
				layer.LearningRate = value
			End Set
		End Property

		' Initializes the net for this vocabulary size.
		' Requires vocabularySize > 0.
		Public Overrides Sub initialize(vocabularySize As Integer)
			' Set the layer parameters.

			layer.InputSize = vocabularySize
			layer.OutputSize = vocabularySize

			layer.initialize()

			m_initialized = True
		End Sub

		' * Train ** 

		' 
		' 		    Performs a forward-backward pass for the given indices.
		' 	
		' 		    ix.length and iy.length lengths must match, can't be empty.
		' 		    All indices must be less than the vocabulary size.
		' 	
		' 		    Returns the cross-entropy loss.
		' 		
		Public Overrides Function forwardBackward(ix As Integer(), iy As Integer()) As Double
			layer.forward(layer.ixTox(ix))
			Dim loss = layer.getLoss(iy)
			layer.backward(layer.getdy(iy))

			Return loss
		End Function

		' Samples n indices, sequence seed, advance the state.
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()
			Return sampleIndices(n, seed, temp, True)
		End Function

		' Samples n indices, sequence seed, choose whether to advance the state.
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

		' Returns true if the net was initialized.
		Public Overrides ReadOnly Property Initialized As Boolean
			Get
				Return m_initialized
			End Get
		End Property

		' Returns the vocabulary size - max index + 1.
		Public Overrides ReadOnly Property VocabularySize As Integer
			Get
				Return layer.InputSize
			End Get
		End Property
	End Class

End Namespace
