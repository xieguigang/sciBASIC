#Region "Microsoft.VisualBasic::3ccaf28ae00c1f95e6d463a722238d7a, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\learn\RecurrentContrastiveDivergence.vb"

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

    '   Total Lines: 212
    '    Code Lines: 112 (52.83%)
    ' Comment Lines: 52 (24.53%)
    '    - Xml Docs: 23.08%
    ' 
    '   Blank Lines: 48 (22.64%)
    '     File Size: 10.95 KB


    '     Class RecurrentContrastiveDivergence
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: createTemporalInput, runHidden, runVisible, trainEvents, visualizeEvents
    ' 
    '         Sub: checkRBMConfigurations, learn, learnMany
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
    ''' </summary>
    Public Class RecurrentContrastiveDivergence

        Private Shared ReadOnly ACTIVATION_STATE As DoubleDoubleFunction = New ActivationState()

        Private Shared ReadOnly SIGMOID As DoubleFunction = New Sigmoid()

        Private Shared ReadOnly CLOCK As Clock = New Clock()

        Private ReadOnly learningParameters As LearningParameters

        Private ReadOnly memory As Integer

        Public Sub New(learningParameters As LearningParameters)
            Me.learningParameters = learningParameters
            memory = Me.learningParameters.Memory
        End Sub

        ''' 
        ''' <summary>
        ''' input memory will be of the form v_t-m, v_t-m-1, ..., v_t-1, v_t(current)
        ''' learn a sequence of events </summary>
        ''' <param name="rbm"> </param>
        ''' <param name="events"> </param>
        Public Sub learn(rbm As RBM, events As IList(Of DenseMatrix))
            checkRBMConfigurations(rbm, events)

            'LOGGER.info("Start Learning single recurrent event of (" + events.Count + " sequences)");
            Call CLOCK.start()
            For epoch = 0 To learningParameters.Epochs - 1
                Dim [error] = trainEvents(rbm, events)

                If epoch Mod 10 = 0 Then
                    'LOGGER.info("Epoch: " + epoch + "/" + learningParameters.Epochs + ", error: " + error + ", time: " + CLOCK.elapsedMillis() + "ms");
                    Call CLOCK.reset()
                End If
            Next
        End Sub

        ''' <summary>
        ''' learn many independent temporal events </summary>
        ''' <param name="rbm"> </param>
        ''' <param name="allEvents"> </param>
        Public Sub learnMany(rbm As RBM, allEvents As IList(Of IList(Of DenseMatrix)))
            checkRBMConfigurations(rbm, allEvents(0))

            For epoch = 0 To learningParameters.Epochs - 1
                Dim [error] = 0.0
                For Each events In allEvents
                    [error] += trainEvents(rbm, events)
                Next

                If epoch Mod 1 = 0 Then
                    'LOGGER.info("Epoch: " + epoch + "/" + learningParameters.Epochs + ", error: " + (error / allEvents.Count) + ", time: " + CLOCK.elapsedMillis() + "ms");
                    Call CLOCK.reset()
                End If
            Next
        End Sub

        Private Function trainEvents(rbm As RBM, events As IList(Of DenseMatrix)) As Double
            Dim numberEvents = events.Count
            Dim weights = rbm.Weights

            Dim [error] = 0.0
            For [event] = 0 To events.Count - memory - 1

                Dim currentAndNextEvent = createTemporalInput([event], events)

                ' Read training data and sample from the hidden later, positive CD phase, (reality phase)
                Dim positiveHiddenActivations = currentAndNextEvent.dot(weights)
                Dim positiveHiddenProbabilities = positiveHiddenActivations.apply(SIGMOID)
                Dim positiveHiddenStates As DenseMatrix = positiveHiddenProbabilities.copy().apply(DenseMatrix.random(currentAndNextEvent.rows(), rbm.HiddenSize), ACTIVATION_STATE)

                ' Note that we're using the activation *probabilities* of the hidden states, not the hidden states themselves, when computing associations.
                ' We could also use the states; see section 3 of Hinton's A Practical Guide to Training Restricted Boltzmann Machines" for more.
                Dim positiveAssociations As DenseMatrix = currentAndNextEvent.transpose().dot(positiveHiddenProbabilities)

                ' Reconstruct the visible units and sample again from the hidden units. negative CD phase, aka the daydreaming phase.
                Dim negativeVisibleActivations As DenseMatrix = positiveHiddenStates.dot(weights.transpose())
                Dim negativeVisibleProbabilities = negativeVisibleActivations.apply(SIGMOID)
                Dim negativeHiddenActivations = negativeVisibleProbabilities.dot(weights)
                Dim negativeHiddenProbabilities = negativeHiddenActivations.apply(SIGMOID)

                ' Note, again, that we're using the activation *probabilities* when computing associations, not the states themselves.
                Dim negativeAssociations As DenseMatrix = negativeVisibleProbabilities.transpose().dot(negativeHiddenProbabilities)

                ' Update weights.
                weights.add(positiveAssociations.subtract(negativeAssociations).divide(numberEvents).multiply(learningParameters.LearningRate))

                [error] += currentAndNextEvent.subtract(negativeVisibleProbabilities).pow(2).sum()
            Next
            Return [error] / events.Count
        End Function

        Private Sub checkRBMConfigurations(rbm As RBM, events As IList(Of DenseMatrix))
            Dim requiredSize As Integer = events(0).columns() + (events(0).columns() * memory)
            If rbm.VisibleSize <> requiredSize Then
                Throw New ArgumentException("RBM Input size must equal event.columns() + event.columns() * memory, required = " & requiredSize.ToString() & ", actual = " & rbm.VisibleSize.ToString())
            End If
        End Sub


        Private Function createTemporalInput([event] As Integer, events As IList(Of DenseMatrix)) As DenseMatrix
            Dim currentEvent = events([event])

            Dim temporalEvent = currentEvent
            Dim i = [event] + 1, t = 0

            While i < events.Count AndAlso t < memory
                temporalEvent = temporalEvent.addColumns(events(i))
                i += 1
                t += 1
            End While

            Dim temporalEventColumns As Integer = currentEvent.columns() + currentEvent.columns() * memory
            If temporalEvent.columns() < temporalEventColumns Then ' fill in blanks if there is not enough temporal data to train
                temporalEvent = temporalEvent.addColumns(DenseMatrix.make(currentEvent.rows(), temporalEventColumns - temporalEvent.columns()))
            End If
            'LOGGER.info("Dataset\n" + PrettyPrint.toPixelBox(temporalEvent.row(0).toArray(), 28, 0.5));
            Return temporalEvent
        End Function

        ' 
        ' 		    Assuming the RBM has been trained, run the network on a set of visible units to get a sample of the hidden units.
        ' 		    Parameters, A matrix where each row consists of the states of the visible units.
        ' 		    hidden_states, A matrix where each row consists of the hidden units activated from the visible
        ' 		    units in the data matrix passed in.
        ' 	
        ' 		    Recurrent version pass in an empty t-1 visible layer
        ' 		 
        Public Function runVisible(rbm As RBM, [event] As DenseMatrix) As DenseMatrix
            Dim weights = rbm.Weights

            Dim currentAndNoNextEvent As DenseMatrix = [event].addColumns(DenseMatrix.make([event].rows(), [event].columns() * memory)) ' append an empty visible layer for next guess

            ' Calculate the activations of the hidden units.
            Dim hiddenActivations = currentAndNoNextEvent.dot(weights)
            ' Calculate the probabilities of turning the hidden units on.
            Dim hiddenProbabilities = hiddenActivations.apply(SIGMOID)
            ' Turn the hidden units on with their specified probabilities.
            Dim hiddenStates As DenseMatrix = hiddenProbabilities.apply(DenseMatrix.random([event].rows(), rbm.HiddenSize), ACTIVATION_STATE)

            Return hiddenStates
        End Function


        Public Function visualizeEvents(rbm As RBM, events As IList(Of DenseMatrix)) As DenseMatrix
            Dim weights = rbm.Weights

            Dim lastVisibleStates As DenseMatrix

            Dim [event] = 0
            Do
                Dim currentAndNextEvent = createTemporalInput([event], events)

                ' run visible
                ' Calculate the activations of the hidden units.
                Dim hiddenActivations = currentAndNextEvent.dot(weights)
                ' Calculate the probabilities of turning the hidden units on.
                Dim hiddenProbabilities = hiddenActivations.apply(SIGMOID)
                ' Turn the hidden units on with their specified probabilities.
                Dim hiddenStates As DenseMatrix = hiddenProbabilities.apply(DenseMatrix.random(currentAndNextEvent.rows(), rbm.HiddenSize), ACTIVATION_STATE)

                ' run hidden
                ' Calculate the activations of the hidden units.
                Dim visibleActivations As DenseMatrix = hiddenStates.dot(weights.transpose())
                ' Calculate the probabilities of turning the visible units on.
                Dim visibleProbabilities = visibleActivations.apply(SIGMOID)
                ' Turn the visible units on with their specified probabilities.
                Dim visibleStates As DenseMatrix = visibleProbabilities.apply(DenseMatrix.random(hiddenStates.rows(), rbm.VisibleSize), ACTIVATION_STATE)

                lastVisibleStates = visibleStates

                [event] += 1
            Loop While [event] < events.Count - memory

            Return lastVisibleStates
        End Function

        ' 
        ' 		    Assuming the RBM has been trained, run the network on a set of hidden units to get a sample of the visible units.
        ' 		    Parameters, A matrix where each row consists of the states of the hidden units.
        ' 		    visible_states, A matrix where each row consists of the visible units activated from the hidden
        ' 		    units in the data matrix passed in.
        ' 		 
        Public Function runHidden(rbm As RBM, hidden As DenseMatrix) As DenseMatrix

            Dim weights = rbm.Weights

            ' Calculate the activations of the hidden units.
            Dim visibleActivations As DenseMatrix = hidden.dot(weights.transpose())
            ' Calculate the probabilities of turning the visible units on.
            Dim visibleProbabilities = visibleActivations.apply(SIGMOID)
            ' Turn the visible units on with their specified probabilities.
            Dim visibleStates As DenseMatrix = visibleProbabilities.apply(DenseMatrix.random(hidden.rows(), rbm.VisibleSize), ACTIVATION_STATE)

            Return visibleStates
        End Function

    End Class

End Namespace
