#Region "Microsoft.VisualBasic::484ce4a81453b700245577527a57eeb3, Data_science\MachineLearning\DeepLearning\RNN\net\MultiLayerRNN.vb"

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

    '   Total Lines: 200
    '    Code Lines: 113 (56.50%)
    ' Comment Lines: 45 (22.50%)
    '    - Xml Docs: 22.22%
    ' 
    '   Blank Lines: 42 (21.00%)
    '     File Size: 5.19 KB


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

	' Multi layer RNN.
	<Serializable>
	Public Class MultiLayerRNN
		Inherits BasicRNN
		' Layers

		Protected Friend layer As RNNLayer() ' All RNN layers

		' Init data

		Private hiddenSizeField As Integer()
		Private learningRateField As Double

		Protected Friend initializedField As Boolean

		''' <summary>
		''' * Construct ** </summary>

		' Creates a net with default parameters.
		Public Sub New()
			learningRateField = RNNLayer.defaultLearningRate
		End Sub

		' Creates a net with default parameters and initializes immediately.
		Public Sub New(vocabularySize As Integer)
			Me.New()
			initialize(vocabularySize)
		End Sub

		''' <summary>
		''' * Hyperparameters ** </summary>

		' 
		' 		    Sets the hidden layer sizes per RNN layer
		' 	
		' 		    hiddenSize.length > 0
		' 		    each size > 1
		' 	
		' 		    Network must be initialized again.
		' 		
		Public Overridable WriteOnly Property HiddenSize As Integer()
			Set(value As Integer())
				hiddenSizeField = value
				initializedField = False
			End Set
		End Property

		' Sets the learning rate for each layer.
		Public Overridable WriteOnly Property LearningRate As Double
			Set(value As Double)
				If layer Is Nothing Then
					learningRateField = value
				Else
					For Each layer As RNNLayer In Me.layer
						layer.LearningRate = value
					Next
				End If
			End Set
		End Property

		''' <summary>
		''' * Initialize ** </summary>

		' Initializes the net for this vocabulary size.
		' Requires vocabularySize > 0.
		Public Overrides Sub initialize(vocabularySize As Integer)
			' Create layers

			If hiddenSizeField Is Nothing Then ' default: single layer
				hiddenSizeField = New Integer(0) {}
				hiddenSizeField(0) = RNNLayer.defaultHiddenSize
			End If

			layer = New RNNLayer(hiddenSizeField.Length - 1) {}

			For i = 0 To layer.Length - 1
				layer(i) = New RNNLayer()

				If i = 0 Then
					layer(i).InputSize = vocabularySize
				Else
					layer(i).InputSize = hiddenSizeField(i - 1)
				End If

				layer(i).HiddenSize = hiddenSizeField(i)
				layer(i).LearningRate = learningRateField


				If i = layer.Length - 1 Then
					layer(i).OutputSize = vocabularySize
				Else
					layer(i).OutputSize = hiddenSizeField(i)
				End If

				layer(i).initialize()
			Next

			initializedField = True
		End Sub

		''' <summary>
		''' * Train ** </summary>

		' 
		' 		    Performs a forward-backward pass for the given indices.
		' 	
		' 		    ix.length and iy.length lengths must match, can't be empty.
		' 		    All indices must be less than the vocabulary size.
		' 	
		' 		    Returns the cross-entropy loss.
		' 		
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
		''' * Sample ** </summary>

		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()
			Return sampleIndices(n, seed, temp, True)
		End Function

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

		' Returns true if the net was initialized.
		Public Overrides ReadOnly Property Initialized As Boolean
			Get
				Return initializedField
			End Get
		End Property

		' Returns the vocabulary size - max index + 1.
		Public Overrides ReadOnly Property VocabularySize As Integer
			Get
				Return layer(0).InputSize
			End Get
		End Property
	End Class
End Namespace
