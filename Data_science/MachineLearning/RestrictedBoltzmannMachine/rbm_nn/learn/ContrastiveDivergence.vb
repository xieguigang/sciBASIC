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
    '    Code Lines: 79
    ' Comment Lines: 56
    '   Blank Lines: 36
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

        Private ReadOnly logisticsFunction As DoubleFunction

        Public Sub New(learningParameters As LearningParameters)
            Me.learningParameters = learningParameters
            logisticsFunction = New Sigmoid()
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
            Dim weights = rbm.Weights

            clock.start()
            For epoch = 0 To learningParameters.Epochs - 1

                Dim [error] As Double = 0
                For Each dataSet In dataSets
                    Dim numberSamples As Integer = dataSet.rows()

                    ' Read training data and sample from the hidden later, positive CD phase, (reality phase)
                    Dim positiveHiddenActivations = dataSet.dot(weights)
                    Dim positiveHiddenProbabilities As DenseMatrix = positiveHiddenActivations.copy().apply(logisticsFunction)
                    Dim positiveHiddenStates = positiveHiddenProbabilities.apply(DenseMatrix.random(numberSamples, rbm.HiddenSize), ACTIVATION_STATE)

                    ' Note that we're using the activation *probabilities* of the hidden states, not the hidden states themselves, when computing associations.
                    ' We could also use the states; see section 3 of Hinton's A Practical Guide to Training Restricted Boltzmann Machines" for more.
                    Dim positiveAssociations As DenseMatrix = dataSet.transpose().dot(positiveHiddenProbabilities)

                    ' Reconstruct the visible units and sample again from the hidden units. negative CD phase, aka the daydreaming phase.
                    Dim negativeVisibleActivations As DenseMatrix = positiveHiddenStates.dot(weights.transpose())
                    Dim negativeVisibleProbabilities = negativeVisibleActivations.apply(logisticsFunction)
                    Dim negativeHiddenActivations = negativeVisibleProbabilities.dot(weights)
                    Dim negativeHiddenProbabilities = negativeHiddenActivations.apply(logisticsFunction)

                    ' Note, again, that we're using the activation *probabilities* when computing associations, not the states themselves.
                    Dim negativeAssociations As DenseMatrix = negativeVisibleProbabilities.transpose().dot(negativeHiddenProbabilities)

                    ' Update weights.
                    Dim updates = positiveAssociations.subtract(negativeAssociations).divide(numberSamples).multiply(learningParameters.LearningRate)
                    weights = weights.add(updates)

                    [error] += dataSet.copy().subtract(negativeVisibleProbabilities).pow(2).sum()
                Next

                If learningParameters.Log AndAlso epoch Mod 10 = 0 And epoch > 0 Then
                    VBDebugger.WriteLine("Epoch: " & epoch & "/" & learningParameters.Epochs & ", error: " & [error] & ", time: " & clock.elapsedMillis() & "ms")
                End If
                clock.reset()
            Next

            rbm.Weights = weights
        End Sub

        ' 
        ' 		    Assuming the FastRBM has been trained, run the network on a set of visible units to get a sample of the hidden units.
        ' 		    Parameters, A matrix where each row consists of the states of the visible units.
        ' 		    hidden_states, A matrix where each row consists of the hidden units activated from the visible
        ' 		    units in the data matrix passed in.
        ' 		 
        Public Function runVisible(rbm As RBM, dataSet As DenseMatrix) As DenseMatrix
            Dim numberSamples As Integer = dataSet.rows()
            Dim weights = rbm.Weights

            ' Calculate the activations of the hidden units.
            Dim hiddenActivations = dataSet.dot(weights)
            ' Calculate the probabilities of turning the hidden units on.
            Dim hiddenProbabilities = hiddenActivations.apply(logisticsFunction)
            ' Turn the hidden units on with their specified probabilities.
            Dim hiddenStates = hiddenProbabilities.apply(DenseMatrix.random(numberSamples, rbm.HiddenSize), ACTIVATION_STATE)

            Return hiddenStates
        End Function

        ' 
        ' 		    Assuming the FastRBM has been trained, run the network on a set of hidden units to get a sample of the visible units.
        ' 		    Parameters, A matrix where each row consists of the states of the hidden units.
        ' 		    visible_states, A matrix where each row consists of the visible units activated from the hidden
        ' 		    units in the data matrix passed in.
        ' 		 
        Public Function runHidden(rbm As RBM, dataSet As DenseMatrix) As DenseMatrix
            Dim numberSamples As Integer = dataSet.rows()
            Dim weights = rbm.Weights

            ' Calculate the activations of the hidden units.
            Dim visibleActivations As DenseMatrix = dataSet.dot(weights.transpose())
            ' Calculate the probabilities of turning the visible units on.
            Dim visibleProbabilities = visibleActivations.apply(logisticsFunction)
            ' Turn the visible units on with their specified probabilities.
            Dim visibleStates = visibleProbabilities.apply(DenseMatrix.random(numberSamples, rbm.VisibleSize), ACTIVATION_STATE)

            Return visibleStates
        End Function

        ' 
        ' 		    Randomly initialize the visible units once, and start running alternating Gibbs sampling steps
        ' 		    (where each step consists of updating all the hidden units, and then updating all of the visible units),
        ' 		    taking a sample of the visible units at each step.
        ' 		    Note that we only initialize the network *once*, so these samples are correlated.
        ' 		    samples: A matrix, where each row is a sample of the visible units produced while the network was daydreaming.
        ' 		 
        Public Function dayDream(rbm As RBM, dataSet As DenseMatrix, dreamSamples As Integer) As ISet(Of DenseMatrix)
            Dim weights = rbm.Weights

            ' Take the first sample from a uniform distribution.
            '        Matrix sample = DenseMatrix.make(new double[][]{dataSet.row(RANDOM.nextInt(numberSamples)).toArray()});
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

                Dim hiddenActivations = visibleValues.dot(weights)
                ' Calculate the probabilities of turning the hidden units on.
                Dim hiddenProbabilities = hiddenActivations.apply(logisticsFunction)
                ' Turn the hidden units on with their specified probabilities.
                Dim hiddenStates As DenseMatrix = hiddenProbabilities.apply(DenseMatrix.random(sample.rows(), rbm.HiddenSize), ACTIVATION_STATE)

                ' Calculate the activations of the hidden units.
                Dim visibleActivations As DenseMatrix = hiddenStates.dot(weights.transpose())
                ' Calculate the probabilities of turning the visible units on.
                Dim visibleProbabilities = visibleActivations.apply(logisticsFunction)
                ' Turn the visible units on with their specified probabilities.
                Dim visibleStates As DenseMatrix = visibleProbabilities.apply(DenseMatrix.random(sample.rows(), sample.columns()), ACTIVATION_STATE)

                sample = visibleStates
            Next
            Return samples
        End Function

    End Class

End Namespace
