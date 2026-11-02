#Region "Microsoft.VisualBasic::36749daffc33f73c377165131ee82aa9, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\learn\ContrastiveDivergence.vb"

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

    '   Total Lines: 171
    '    Code Lines: 79 (46.20%)
    ' Comment Lines: 56 (32.75%)
    '    - Xml Docs: 21.43%
    ' 
    '   Blank Lines: 36 (21.05%)
    '     File Size: 9.70 KB


    '     Class ContrastiveDivergence
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: dayDream, runHidden, runVisible
    ' 
    '         Sub: (+2 Overloads) learn
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions.doubledouble.rbm
Imports Microsoft.VisualBasic.ApplicationServices

Namespace nn.rbm.learn


    ''' <summary>
    ''' Created by kenny on 5/15/14.
    ''' 
    '''  * http://blog.echen.me/2011/07/18/introduction-to-restricted-boltzmann-machines/
    ''' </summary>
    Public Class ContrastiveDivergence

        Private Shared ReadOnly ACTIVATION_STATE As DoubleDoubleFunction = New ActivationState()

        Private ReadOnly clock As Clock = New Clock()

        Private ReadOnly learningParameters As LearningParameters

        Public Sub New(learningParameters As LearningParameters)
            If learningParameters Is Nothing Then
                Throw New ArgumentNullException(NameOf(learningParameters))
            End If

            Me.learningParameters = learningParameters
        End Sub

        ''' <summary>
        ''' Learn a matrix of data. Each row represents a single training set.
        ''' For example t = [[1,0],[0,1]] will train the rbm to recognize [1,0] and [0,1]
        ''' If a Matrix is too large I recommend splitting it into smaller matrices, but if they are reasonably small, this
        ''' is a fast way to "simultaneously" train multiple inputs and should generable be used unless the matrices are just
        ''' too large </summary>
        ''' <param name="rbm"> </param>
        ''' <param name="dataSet"> </param>
        Public Sub learn(rbm As RBM, dataSet As DenseMatrix)
            learn(rbm, New List(Of DenseMatrix)() From {dataSet})
        End Sub

        Public Sub learn(rbm As RBM, dataSets As IReadOnlyCollection(Of DenseMatrix))
            clock.start()
            For epoch = 0 To learningParameters.Epochs - 1

                Dim [error] As Double = 0
                For Each dataSet In dataSets
                    [error] += trainMiniBatch(rbm, dataSet)
                Next

                If learningParameters.Log AndAlso epoch Mod 10 = 0 And epoch > 0 Then
                    VBDebugger.WriteLine("Epoch: " & epoch & "/" & learningParameters.Epochs & ", error: " & [error] & ", time: " & clock.elapsedMillis() & "ms")
                End If
                clock.reset()
            Next
        End Sub

        ''' <summary>
        ''' 使用一批数据完成一次CD-k更新，同时更新权重矩阵以及可见层、隐藏层的偏置项。
        ''' </summary>
        ''' <param name="rbm"></param>
        ''' <param name="dataSet"></param>
        ''' <returns>这一批数据的重构平方误差总和</returns>
        Private Function trainMiniBatch(rbm As RBM, dataSet As DenseMatrix) As Double
            Dim numberSamples As Integer = dataSet.rows()
            Dim learningRate As Double = learningParameters.LearningRate

            ' Read training data and sample from the hidden later, positive CD phase, (reality phase)
            Dim positiveHiddenProbabilities As DenseMatrix = rbm.HiddenProbabilities(dataSet)
            Dim positiveHiddenStates As DenseMatrix = positiveHiddenProbabilities.apply(DenseMatrix.random(numberSamples, rbm.HiddenSize), ACTIVATION_STATE)

            ' Note that we're using the activation *probabilities* of the hidden states, not the hidden states themselves, when computing associations.
            ' We could also use the states; see section 3 of Hinton's A Practical Guide to Training Restricted Boltzmann Machines" for more.
            Dim positiveAssociations As DenseMatrix = dataSet.transpose().dot(positiveHiddenProbabilities)
            Dim positiveVisibleActivation As Double() = dataSet.columnSum()
            Dim positiveHiddenActivation As Double() = positiveHiddenProbabilities.columnSum()

            ' Reconstruct the visible units and sample again from the hidden units. negative CD phase, aka the daydreaming phase.
            ' 
            ' 从这里开始进行k步交替Gibbs采样：k = 1的时候就是标准的CD-1
            Dim negativeHiddenStates As DenseMatrix = positiveHiddenStates
            Dim negativeVisibleProbabilities As DenseMatrix = Nothing
            Dim negativeHiddenProbabilities As DenseMatrix = Nothing
            Dim k As Integer = System.Math.Max(1, learningParameters.K)

            For step As Integer = 1 To k
                negativeVisibleProbabilities = rbm.VisibleProbabilities(negativeHiddenStates)

                Dim negativeVisibleStates As DenseMatrix = negativeVisibleProbabilities.apply(DenseMatrix.random(numberSamples, rbm.VisibleSize), ACTIVATION_STATE)

                negativeHiddenProbabilities = rbm.HiddenProbabilities(negativeVisibleStates)
                negativeHiddenStates = negativeHiddenProbabilities.apply(DenseMatrix.random(numberSamples, rbm.HiddenSize), ACTIVATION_STATE)
            Next

            ' Note, again, that we're using the activation *probabilities* when computing associations, not the states themselves.
            Dim negativeAssociations As DenseMatrix = negativeVisibleProbabilities.transpose().dot(negativeHiddenProbabilities)
            Dim negativeVisibleActivation As Double() = negativeVisibleProbabilities.columnSum()
            Dim negativeHiddenActivation As Double() = negativeHiddenProbabilities.columnSum()

            ' Update weights: W += lr * (E_data[vh^T] - E_model[vh^T]) / N
            Dim updates = positiveAssociations.subtract(negativeAssociations).divide(numberSamples).multiply(learningRate)

            rbm.Weights = rbm.Weights.add(updates)

            ' Update bias: a += lr * (E_data[v] - E_model[v]) / N, b += lr * (E_data[h] - E_model[h]) / N
            Dim vbias As Double() = rbm.VBias
            Dim hbias As Double() = rbm.HBias

            For i As Integer = 0 To rbm.VisibleSize - 1
                vbias(i) += learningRate * (positiveVisibleActivation(i) - negativeVisibleActivation(i)) / numberSamples
            Next

            For j As Integer = 0 To rbm.HiddenSize - 1
                hbias(j) += learningRate * (positiveHiddenActivation(j) - negativeHiddenActivation(j)) / numberSamples
            Next

            rbm.VBias = vbias
            rbm.HBias = hbias

            Return dataSet.copy().subtract(negativeVisibleProbabilities).pow(2).sum()
        End Function

        ' 
        ' 		    Assuming the FastRBM has been trained, run the network on a set of visible units to get a sample of the hidden units.
        ' 		    Parameters, A matrix where each row consists of the states of the visible units.
        ' 		    hidden_states, A matrix where each row consists of the hidden units activated from the visible
        ' 		    units in the data matrix passed in.
        ' 		 
        Public Function runVisible(rbm As RBM, dataSet As DenseMatrix) As DenseMatrix
            ' P(h=1|v) = σ(vW + b)，然后按照这个概率进行二值采样
            Return rbm.SampleHidden(dataSet)
        End Function

        ' 
        ' 		    Assuming the FastRBM has been trained, run the network on a set of hidden units to get a sample of the visible units.
        ' 		    Parameters, A matrix where each row consists of the states of the hidden units.
        ' 		    visible_states, A matrix where each row consists of the visible units activated from the hidden
        ' 		    units in the data matrix passed in.
        ' 		 
        Public Function runHidden(rbm As RBM, dataSet As DenseMatrix) As DenseMatrix
            ' P(v=1|h) = σ(hW^T + a)，然后按照这个概率进行二值采样
            Return rbm.SampleVisible(dataSet)
        End Function

        ' 
        ' 		    Randomly initialize the visible units once, and start running alternating Gibbs sampling steps
        ' 		    (where each step consists of updating all the hidden units, and then updating all of the visible units),
        ' 		    taking a sample of the visible units at each step.
        ' 		    Note that we only initialize the network *once*, so these samples are correlated.
        ' 		    samples: A matrix, where each row is a sample of the visible units produced while the network was daydreaming.
        ' 		 
        Public Function dayDream(rbm As RBM, dataSet As DenseMatrix, dreamSamples As Integer) As ISet(Of DenseMatrix)
            ' Take the first sample from a uniform distribution.
            Dim sample = dataSet

            ' store all samples history
            Dim samples As ISet(Of DenseMatrix) = New HashSet(Of DenseMatrix)()

            ' Start the alternating Gibbs sampling.
            ' Note that we keep the hidden units binary states, but leave the visible units as real probabilities.
            ' See section 3 of Hinton's "A Practical Guide to Training Restricted Boltzmann Machines" for more on why.
            For i As Integer = 0 To dreamSamples - 1

                ' Calculate the activations of the hidden units.
                Dim visibleValues = sample
                samples.Add(visibleValues)

                ' P(h=1|v)的伯努利采样
                Dim hiddenStates As DenseMatrix = rbm.SampleHidden(visibleValues)
                ' P(v=1|h)的伯努利采样
                Dim visibleStates As DenseMatrix = rbm.SampleVisible(hiddenStates)

                sample = visibleStates
            Next
            Return samples
        End Function

    End Class

End Namespace
