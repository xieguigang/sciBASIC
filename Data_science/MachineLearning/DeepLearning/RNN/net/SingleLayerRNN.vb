Namespace RNN

	' Single layer RNN.
	<Serializable>
	Public Class SingleLayerRNN
		Inherits BasicRNN
		' The layer

		Protected Friend layer As RNNLayer ' The single RNN layer

		Private initializedField As Boolean

		''' <summary>
		''' * Construct ** </summary>

		' Creates a net with default parameters.
		Public Sub New()
			layer = New RNNLayer()
		End Sub

		' Creates a net with default parameters and initializes immediately.
		Public Sub New(vocabularySize As Integer)
			Me.New()
			initialize(vocabularySize)
		End Sub

		''' <summary>
		''' * Hyperparameters ** </summary>

		' Sets the hidden layer size. Network must be initialized again.
		Public Overridable WriteOnly Property HiddenSize As Integer
			Set(value As Integer)
				layer.HiddenSize = value
				initializedField = False
			End Set
		End Property

		' Sets the learning rate.
		Public Overridable WriteOnly Property LearningRate As Double
			Set(value As Double)
				layer.LearningRate = value
			End Set
		End Property

		''' <summary>
		''' * Initialize ** </summary>

		' Initializes the net for this vocabulary size.
		' Requires vocabularySize > 0.
		Public Overrides Sub initialize(vocabularySize As Integer)
			' Set the layer parameters.

			layer.InputSize = vocabularySize
			layer.OutputSize = vocabularySize

			layer.initialize()

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
			layer.forward(layer.ixTox(ix))
			Dim loss = layer.getLoss(iy)
			layer.backward(layer.getdy(iy))

			Return loss
		End Function

		''' <summary>
		''' * Sample ** </summary>

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
				Return initializedField
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