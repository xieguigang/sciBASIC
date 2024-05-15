#Region "Microsoft.VisualBasic::fc200da578547e98495cc2abb9206f7d, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\learn\DeepContrastiveDivergence.vb"

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

    '   Total Lines: 176
    '    Code Lines: 109
    ' Comment Lines: 30
    '   Blank Lines: 37
    '     File Size: 8.46 KB


    '     Class DeepContrastiveDivergence
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: buildSampleData, buildSampleDataReverse, buildSamplesFromActivatedHiddenLayers, runHidden, runVisible
    ' 
    '         Sub: learn
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions.doubledouble.rbm
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.deep
Imports Microsoft.VisualBasic.ApplicationServices

Namespace nn.rbm.learn

    ''' <summary>
    ''' Created by kenny on 5/15/14.
    ''' 
    ''' </summary>
    Public Class DeepContrastiveDivergence

        Private Shared ReadOnly ACTIVATION__FUNCTION As DoubleDoubleFunction = New ActivationState()

        Private ReadOnly clock As Clock = New Clock()

        Private ReadOnly contrastiveDivergence As ContrastiveDivergence

        Private ReadOnly learningParameters As LearningParameters

        Public Sub New(learningParameters As LearningParameters)
            contrastiveDivergence = New ContrastiveDivergence(learningParameters)
            Me.learningParameters = learningParameters
        End Sub

        ' 
        ' 		   DBN Greedy Training
        ' 		   P(v,h1,h2,...hn) = P(v|h1)P(h1|h2)...P(hn-2|hn-1)P(hn-1|hn)
        ' 		   Train P(v|h1), use h1 for each v to train P(h1|h2), repeat until P(hn-1|hn) is trained
        ' 		 
        Public Sub learn(deepRBM As DeepRBM, dataSet As DenseMatrix)
            Dim rbmLayers = deepRBM.RbmLayers

            Dim trainingData As IList(Of DenseMatrix) = DenseMatrix.splitColumns(dataSet, rbmLayers(0).size()) ' split dataset across rbms

            Dim samplePieces = trainingData
            clock.reset()
            For layer = 0 To rbmLayers.Length - 1

                Dim rbmLayer = rbmLayers(layer)
                samplePieces = buildSamplesFromActivatedHiddenLayers(samplePieces, layer, rbmLayers)

                For r = 0 To rbmLayer.size() - 1
                    Dim rbm = rbmLayer.getRBM(r)
                    Dim splitDataSet = samplePieces(r)
                    contrastiveDivergence.learn(rbm, splitDataSet)
                Next

            Next

            If learningParameters.Log Then
                'LOGGER.info("All Layers finished Training in " + clock.elapsedSeconds() + "ms");
            End If
        End Sub

        ' 
        ' 		    Assuming the RBM has been trained, run the network on a set of visible units to get a sample of the hidden units.
        ' 		    Parameters, A matrix where each row consists of the states of the visible units.
        ' 		    hidden_states, A matrix where each row consists of the hidden units activated from the visible
        ' 		    units in the data matrix passed in.
        ' 		 
        Public Function runVisible(deepRBM As DeepRBM, dataSet As DenseMatrix) As DenseMatrix
            Dim rbmLayers = deepRBM.RbmLayers

            Dim trainingData As IList(Of DenseMatrix) = DenseMatrix.splitColumns(dataSet, rbmLayers(0).size()) ' split dataset across rbms

            Dim samplePieces = trainingData
            Dim hiddenStatesArray = New DenseMatrix(-1) {}

            For layer = 0 To rbmLayers.Length - 1
                Dim rbmLayer = rbmLayers(layer)
                hiddenStatesArray = New DenseMatrix(rbmLayer.size() - 1) {}

                samplePieces = buildSampleData(samplePieces, layer, rbmLayers)

                For r = 0 To rbmLayer.size() - 1
                    Dim rbm = rbmLayer.getRBM(r)
                    Dim splitDataSet = samplePieces(r)
                    Dim hiddenStates = contrastiveDivergence.runVisible(rbm, splitDataSet)
                    hiddenStatesArray(r) = hiddenStates
                Next
            Next

            Return DenseMatrix.make(DenseMatrix.concatColumns(hiddenStatesArray))

        End Function

        ' 
        ' 		    Assuming the RBM has been trained, run the network on a set of hidden units to get a sample of the visible units.
        ' 		    Parameters, A matrix where each row consists of the states of the hidden units.
        ' 		    visible_states, A matrix where each row consists of the visible units activated from the hidden
        ' 		    units in the data matrix passed in.
        ' 		 
        Public Function runHidden(deepRBM As DeepRBM, dataSet As DenseMatrix) As DenseMatrix
            Dim rbmLayers = deepRBM.RbmLayers

            Dim trainingData As IList(Of DenseMatrix) = DenseMatrix.splitColumns(dataSet, rbmLayers(rbmLayers.Length - 1).size()) ' split dataset across rbms

            Dim samplePieces = trainingData
            Dim visibleStatesArray = New DenseMatrix(-1) {}

            For layer = rbmLayers.Length - 1 To 0 Step -1
                Dim rbmLayer = rbmLayers(layer)
                visibleStatesArray = New DenseMatrix(rbmLayer.size() - 1) {}

                samplePieces = buildSampleDataReverse(samplePieces, layer, rbmLayers)

                For r = 0 To rbmLayer.size() - 1
                    Dim rbm = rbmLayer.getRBM(r)
                    Dim splitDataSet = samplePieces(r)

                    Dim visibleStates = contrastiveDivergence.runHidden(rbm, splitDataSet)
                    visibleStatesArray(r) = visibleStates
                Next
            Next
            Return DenseMatrix.make(DenseMatrix.concatColumns(visibleStatesArray))
        End Function

        ' 
        ' 		    Pass data into visible layers and activate hidden layers.
        ' 		    return hidden layers
        ' 		 
        Private Function buildSamplesFromActivatedHiddenLayers(sampleData As IList(Of DenseMatrix), layer As Integer, rbmLayers As RBMLayer()) As IList(Of DenseMatrix)
            Dim rbmLayer = rbmLayers(layer)

            If layer = 0 Then
                Return sampleData
            Else
                Dim previousLayer = rbmLayers(layer - 1)
                Dim previousLayerOutputs As DenseMatrix() = New DenseMatrix(previousLayer.size() - 1) {}
                For r = 0 To previousLayer.size() - 1
                    Dim rbm = previousLayer.getRBM(r)
                    previousLayerOutputs(r) = contrastiveDivergence.runVisible(rbm, sampleData(r))
                Next
                ' combine all outputs off hidden layer, then re-split them to input into the next visual layer
                Return DenseMatrix.splitColumns(DenseMatrix.make(DenseMatrix.concatColumns(previousLayerOutputs)), rbmLayer.size())
            End If
        End Function

        Private Function buildSampleData(trainingData As IList(Of DenseMatrix), layer As Integer, rbmLayers As RBMLayer()) As IList(Of DenseMatrix)
            Dim rbmLayer = rbmLayers(layer)

            If layer = 0 Then
                Return trainingData
            Else
                Dim previousLayer = rbmLayers(layer - 1)
                Dim previousLayerOutputs As DenseMatrix() = New DenseMatrix(previousLayer.size() - 1) {}
                For r = 0 To previousLayer.size() - 1
                    previousLayerOutputs(r) = contrastiveDivergence.runVisible(previousLayer.getRBM(r), trainingData(r))
                    ' previousLayer.getRBM(r).getHidden().getValues() };
                Next
                ' combine all outputs off hidden layer, then re-split them to input into the next visual layer
                Return DenseMatrix.splitColumns(DenseMatrix.make(DenseMatrix.concatColumns(previousLayerOutputs)), rbmLayer.size())
            End If
        End Function

        Private Function buildSampleDataReverse(trainingData As IList(Of DenseMatrix), layer As Integer, rbmLayers As RBMLayer()) As IList(Of DenseMatrix)
            Dim rbmLayer = rbmLayers(layer)

            If layer = rbmLayers.Length - 1 Then
                Return trainingData
            Else
                Dim previousLayer = rbmLayers(layer + 1)
                Dim previousLayerInputs As DenseMatrix() = New DenseMatrix(previousLayer.size() - 1) {}
                For r = 0 To previousLayer.size() - 1
                    previousLayerInputs(r) = contrastiveDivergence.runHidden(previousLayer.getRBM(r), trainingData(r))
                Next
                ' combine all outputs off hidden layer, then re-split them to input into the next visual layer
                Return DenseMatrix.splitColumns(DenseMatrix.make(DenseMatrix.concatColumns(previousLayerInputs)), rbmLayer.size())
            End If
        End Function

    End Class

End Namespace
