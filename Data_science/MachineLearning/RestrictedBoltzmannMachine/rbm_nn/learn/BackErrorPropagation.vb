#Region "Microsoft.VisualBasic::82bf4452f78ec43a77b4048934b42c52, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\learn\BackErrorPropagation.vb"

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

    '   Total Lines: 99
    '    Code Lines: 59 (59.60%)
    ' Comment Lines: 20 (20.20%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 20 (20.20%)
    '     File Size: 4.02 KB


    '     Class BackErrorPropagation
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: backPropagate, calculateAvgSquaredError, calculateErrors, feedFoward, learn
    ' 
    '         Sub: adjustWeights
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

        Public Function learn(rbm As RBM, trainData As IList(Of DenseMatrix), teacherSignals As IList(Of DenseMatrix)) As Double
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

        Public Function backPropagate(rbm As RBM, output As DenseMatrix, teacherSignals As DenseMatrix) As Double
            Dim errors = calculateErrors(output, teacherSignals)
            adjustWeights(rbm, output, errors)
            Return calculateAvgSquaredError(output, teacherSignals)
        End Function

        ''' <summary>
        ''' feed forward
        ''' </summary>
        Public Function feedFoward(rbm As RBM, input As DenseMatrix) As DenseMatrix
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
