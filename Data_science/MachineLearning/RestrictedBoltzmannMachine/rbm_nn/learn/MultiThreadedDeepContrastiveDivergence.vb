#Region "Microsoft.VisualBasic::85019bbf96b8888e9cc90032e85c54b8, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\learn\MultiThreadedDeepContrastiveDivergence.vb"

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

    '   Total Lines: 127
    '    Code Lines: 74
    ' Comment Lines: 27
    '   Blank Lines: 26
    '     File Size: 5.82 KB


    '     Class MultiThreadedDeepContrastiveDivergence
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: buildSamplesFromActivatedHiddenLayers, runHidden, runVisible
    ' 
    '         Sub: learn
    '         Class ContrastiveDivergenceRunner
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.deep
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Parallel


Namespace nn.rbm.learn

    ''' <summary>
    ''' Created by kenny on 5/22/14.
    ''' 
    ''' </summary>
    Public Class MultiThreadedDeepContrastiveDivergence

        Private ReadOnly clock As Clock = New Clock()

        Private ReadOnly deepContrastiveDivergence As DeepContrastiveDivergence

        Private ReadOnly contrastiveDivergence As ContrastiveDivergence

        Private ReadOnly learningParameters As LearningParameters

        Public Sub New(learningParameters As LearningParameters)
            Me.New(learningParameters, 8)
        End Sub

        Public Sub New(learningParameters As LearningParameters, numberThreads As Integer)
            contrastiveDivergence = New ContrastiveDivergence(learningParameters)
            deepContrastiveDivergence = New DeepContrastiveDivergence(learningParameters)
            Me.learningParameters = learningParameters

            VectorTask.n_threads = numberThreads
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
                Dim threadPoolExecutor As ContrastiveDivergenceRunner = New ContrastiveDivergenceRunner(rbmLayer, samplePieces, learningParameters)
                threadPoolExecutor.Run()
            Next

            If learningParameters.Log Then
                ' LOGGER.info("All Layers finished Training in " + clock.elapsedSeconds() + "s");
            End If
        End Sub

        ' 
        ' 		    Assuming the RBM has been trained, run the network on a set of visible units to get a sample of the hidden units.
        ' 		    Parameters, A matrix where each row consists of the states of the visible units.
        ' 		    hidden_states, A matrix where each row consists of the hidden units activated from the visible
        ' 		    units in the data matrix passed in.
        ' 		 
        Public Function runVisible(deepRBM As DeepRBM, dataSet As DenseMatrix) As DenseMatrix
            Return deepContrastiveDivergence.runVisible(deepRBM, dataSet)
        End Function

        ' 
        ' 		    Assuming the RBM has been trained, run the network on a set of hidden units to get a sample of the visible units.
        ' 		    Parameters, A matrix where each row consists of the states of the hidden units.
        ' 		    visible_states, A matrix where each row consists of the visible units activated from the hidden
        ' 		    units in the data matrix passed in.
        ' 		 
        Public Function runHidden(deepRBM As DeepRBM, dataSet As DenseMatrix) As DenseMatrix
            Return deepContrastiveDivergence.runHidden(deepRBM, dataSet)
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

        Private Class ContrastiveDivergenceRunner
            Inherits VectorTask

            Friend ReadOnly contrastiveDivergence As ContrastiveDivergence
            Friend ReadOnly rbmLayer As RBMLayer
            Friend ReadOnly samplePieces As IList(Of DenseMatrix)

            Public Sub New(rbmLayer As RBMLayer, samplePieces As IList(Of DenseMatrix), learningParameters As LearningParameters)
                MyBase.New(rbmLayer.size())
                contrastiveDivergence = New ContrastiveDivergence(learningParameters)
                Me.rbmLayer = rbmLayer
                Me.samplePieces = samplePieces
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                For r = start To ends
                    Dim rbm = rbmLayer.getRBM(r)
                    Dim splitDataSet = samplePieces(r)

                    contrastiveDivergence.learn(rbm, splitDataSet)
                Next
            End Sub
        End Class

    End Class

End Namespace
