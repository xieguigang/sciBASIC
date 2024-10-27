Imports System.IO

Namespace RNN

	''' <summary>
	''' Application options.
	''' </summary>
	Public Class Options

		' * Model parameters ** 

		Public Property hiddenSize As Integer = 100 ' Size of a single RNN layer hidden state.

		Public Property layers As Integer = 2 ' How many layers in a net?


		' * Training parameters ** 

		Public Property sequenceLength As Integer = 50 ' How many steps to unroll during training?

		Public Property learningRate As Double = 0.1 ' The network learning rate.


		' * Sampling parameters **

		' Sampling temperature (0.0, 1.0]. Lower
		' temperature means more conservative
		' predictions.
		Public Property samplingTemp As Double = 1.0

		' * Other options ** 

		Public Property printOptions As Boolean = True ' Print options at the start.

		Public Property trainingSampleLength As Integer = 400 ' Length of a sample during training.

		Public Property snapshotEveryNSamples As Integer = 50 ' Take a network's snapshot every N samples.

		Public Property loopAroundTimes As Integer = 0 ' Loop around the training data this many times.


		Public Property sampleEveryNSteps As Integer = 100 ' Take a sample during training every N steps.

		Public Property inputFile As String = "input.txt" ' The training data.

		Public Property useSingleLayerNet As Boolean = False ' Use the simple, single layer net.

	End Class
End Namespace