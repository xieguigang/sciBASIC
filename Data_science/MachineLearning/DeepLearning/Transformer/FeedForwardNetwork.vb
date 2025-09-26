#Region "Microsoft.VisualBasic::49d6844ce78ba16bb7b9d0182f5c62f4, Data_science\MachineLearning\DeepLearning\Transformer\FeedForwardNetwork.vb"

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

    '   Total Lines: 47
    '    Code Lines: 35 (74.47%)
    ' Comment Lines: 2 (4.26%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (21.28%)
    '     File Size: 1.61 KB


    '     Class FeedForwardNetwork
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FeedForward
    ' 
    '         Sub: GenerateRandomLayers, MakeTrainingStep
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer
    Public Class FeedForwardNetwork
        Private W1, W2 As Tensor
        Private b1, b2 As Tensor

        Private W1Optimizer, W2Optimizer, b1Optimizer, b2Optimizer As Optimizer

        Public Sub New(dff As Integer, embeddingSize As Integer)
            W1 = New Tensor(embeddingSize, dff)
            W2 = New Tensor(dff, embeddingSize)
            b1 = New Tensor(dff)
            b2 = New Tensor(embeddingSize)
            GenerateRandomLayers()

            W1Optimizer = New Optimizer(W1)
            W2Optimizer = New Optimizer(W2)
            b1Optimizer = New Optimizer(b1)
            b2Optimizer = New Optimizer(b2)
        End Sub

        Public Function FeedForward(G As Tensor) As Tensor
            ' First layer
            Dim FFN1 = Tensor.MatMul(G, W1).VecAdd(b1)
            FFN1.ReLU()

            ' Second layer
            Dim FFN2 = Tensor.MatMul(FFN1, W2).VecAdd(b2)

            Return FFN2
        End Function

        Private Sub GenerateRandomLayers()
            W1.GenerateNormalRandomValues()
            W2.GenerateNormalRandomValues()
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            W1Optimizer.MakeTrainingStep(learningRate, [step], W1)
            W2Optimizer.MakeTrainingStep(learningRate, [step], W2)
            b1Optimizer.MakeTrainingStep(learningRate, [step], b1)
            b2Optimizer.MakeTrainingStep(learningRate, [step], b2)
        End Sub

    End Class
End Namespace

