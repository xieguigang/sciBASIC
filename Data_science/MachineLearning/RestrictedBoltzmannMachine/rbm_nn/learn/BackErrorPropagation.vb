Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions

Namespace nn.rbm.learn

    ''' <summary>
    ''' Created by kenny on 5/23/14.
    ''' </summary>
    Public Class BackErrorPropagation

        Private Shared ReadOnly ONE_MINUS_X As DoubleFunction = New OneMinusX()

        Private Shared ReadOnly SIGMOID As DoubleFunction = New Sigmoid()

        Private ReadOnly learningParameters As LearningParameters

        Public Sub New(learningParameters As LearningParameters)
            Me.learningParameters = learningParameters
        End Sub

        Public Overridable Function learn(rbm As RBM, trainData As IList(Of DenseMatrix), teacherSignals As IList(Of DenseMatrix)) As Double
            Dim [error] As Double = 0
            For epoch = 0 To learningParameters.Epochs - 1
                [error] = 0
                For i = 0 To trainData.Count - 1

                    Dim input As DenseMatrix = trainData(i).copy()

                    Dim teacherSignal As DenseMatrix = teacherSignals(i).copy()

                    Dim output = feedFoward(rbm, input)
                    [error] += calculateAvgSquaredError(output, teacherSignal)
                    Dim errors = calculateErrors(output, teacherSignal)
                    adjustWeights(rbm, input, errors)

                Next
                If learningParameters.Log AndAlso epoch > 0 AndAlso epoch Mod 10 = 0 Then
                    ' LOGGER.info("Epoch: " + epoch + ", error: " + error / trainData.Count);
                End If
            Next
            Return [error]
        End Function

        Public Overridable Function backPropagate(rbm As RBM, output As DenseMatrix, teacherSignals As DenseMatrix) As Double
            Dim errors = calculateErrors(output, teacherSignals)
            adjustWeights(rbm, output, errors)
            Return calculateAvgSquaredError(output, teacherSignals)
        End Function

        ''' <summary>
        ''' feed forward
        ''' </summary>
        Public Overridable Function feedFoward(rbm As RBM, input As DenseMatrix) As DenseMatrix
            Dim output = DenseMatrix.make(1, rbm.HiddenSize)

            Dim weights = rbm.Weights

            For i = 0 To rbm.VisibleSize - 1
                For j = 0 To rbm.HiddenSize - 1
                    output.set(0, j, output.get(0, j) + input.get(0, i) * weights.get(i, j))
                Next
            Next
            Return output.apply(SIGMOID)
        End Function


        ''' <summary>
        ''' calculate the error
        ''' </summary>
        Private Function calculateErrors(output As DenseMatrix, teacherSignals As DenseMatrix) As DenseMatrix
            ' (teacher_i - output_i)  * output_i * (1 - output_i)
            Dim errors As DenseMatrix = teacherSignals.copy().subtract(output).multiply(output).multiply(output.apply(ONE_MINUS_X))
            Return errors
        End Function

        ''' <summary>
        ''' depending on the error, adjust the weights
        ''' </summary>
        Private Sub adjustWeights(rbm As RBM, input As DenseMatrix, errors As DenseMatrix)
            Dim weights = rbm.Weights
            '  adjust the weights
            For i = 0 To rbm.VisibleSize - 1
                For j = 0 To rbm.HiddenSize - 1
                    weights.set(i, j, weights.get(i, j) + learningParameters.LearningRate * errors.get(0, j) * input.get(0, i))
                Next
            Next
        End Sub

        ''' <summary>
        ''' calculate the average squared error between the
        ''' output layer and teacher signal
        ''' 
        ''' </summary>
        Private Function calculateAvgSquaredError(output As DenseMatrix, teacherSignals As DenseMatrix) As Double
            Return output.copy().subtract(teacherSignals).pow(2).sum() / output.columns()
        End Function
    End Class

End Namespace
