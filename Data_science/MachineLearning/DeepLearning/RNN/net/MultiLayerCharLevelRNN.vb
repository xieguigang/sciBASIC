Namespace RNN

	' Single layer character level RNN.
	<Serializable>
	Public Class MultiLayerCharLevelRNN
		Inherits CharLevelRNN
		Protected Friend alphabetField As Alphabet ' The alphabet for sampling.

		Protected Friend internal As MultiLayerRNN ' Basic network.

		''' <summary>
		''' * Construct ** </summary>

		' Constructs without initialization.
		Public Sub New()
			internal = New MultiLayerRNN()
		End Sub

		' Constructs and initializes immediately.
		' Requires that alphabet != null.
		Public Sub New(alphabet As Alphabet)
			Me.New()
			initialize(alphabet)
		End Sub

		''' <summary>
		''' * Hyperparameters ** </summary>

		' Sets the hidden layer size. Network must be initialized again.
		Public Sub SetHiddenSize(value As Integer())
			internal.HiddenSize = value
		End Sub

		' Sets the learning rate.
		Public Sub SetLearningRate(value As Double)
			internal.LearningRate = value
		End Sub

		''' <summary>
		''' * Initialize ** </summary>

		' Initializes the net. alphabet != null.
		Public Overrides Sub initialize(alphabet As Alphabet)
			alphabetField = alphabet
			internal.initialize(alphabet.size())
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
			Return internal.forwardBackward(ix, iy)
		End Function

		''' <summary>
		''' * Sample ** </summary>

		' Samples n indices, sequence seed, advance the state.
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()
			Return internal.sampleIndices(n, seed, temp)
		End Function

		' Samples n indices, sequence seed, choose whether to advance the state.
		Public Overloads Overrides Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer()
			Return internal.sampleIndices(n, seed, temp, advance)
		End Function

		''' <summary>
		''' * Get ** </summary>

		' Returns the alphabet, if initialized.
		Public Overrides ReadOnly Property Alphabet As Alphabet
			Get
				Return alphabetField
			End Get
		End Property

		' Returns true if the net was initialized.
		Public Overrides ReadOnly Property Initialized As Boolean
			Get
				Return internal.Initialized
			End Get
		End Property

		' Returns the vocabulary size (the alphabet size), if initialized.
		Public Overrides ReadOnly Property VocabularySize As Integer
			Get
				Return internal.VocabularySize
			End Get
		End Property
	End Class
End Namespace