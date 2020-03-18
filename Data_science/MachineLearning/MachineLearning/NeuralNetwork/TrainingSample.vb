Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure

Namespace NeuralNetwork

    Public Structure TrainingSample

        Dim sampleID As String
        Dim sample As Double()

        ''' <summary>
        ''' The output result.
        ''' </summary>
        Dim classify As Double()

        Public ReadOnly Property isEmpty As Boolean
            Get
                Return sample.IsNullOrEmpty OrElse classify.IsNullOrEmpty
            End Get
        End Property

        Sub New(sample As Sample)
            Me.sampleID = sample.ID
            Me.sample = sample.vector
            Me.classify = sample.target
        End Sub

    End Structure
End Namespace