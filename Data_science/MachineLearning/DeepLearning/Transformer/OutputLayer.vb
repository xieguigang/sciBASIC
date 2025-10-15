#Region "Microsoft.VisualBasic::0045ad6fa6ef7e7ba5261d3a7f86fee5, Data_science\MachineLearning\DeepLearning\Transformer\OutputLayer.vb"

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

    '   Total Lines: 32
    '    Code Lines: 21 (65.62%)
    ' Comment Lines: 3 (9.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (25.00%)
    '     File Size: 1.09 KB


    '     Class OutputLayer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Output
    ' 
    '         Sub: MakeTrainingStep
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer
    ''' <summary>
    ''' Produce a flat array with the same dimension as the number of words in the dictionary
    ''' </summary>
    Public Class OutputLayer
        Public Wo As Tensor

        Private WoOptimizer As Optimizer

        Public Sub New(sequenceLength As Integer, embeddingSize As Integer, dictionarySize As Integer)
            Wo = New Tensor(embeddingSize * sequenceLength, dictionarySize)
            Wo.GenerateNormalRandomValues()

            WoOptimizer = New Optimizer(Wo)
        End Sub

        Public Function Output(input As Tensor) As Tensor
            Dim flatInput = input.Flatten()
            Dim filteredOutput = Tensor.MatMul(flatInput, Wo)
            Dim softmaxOutput = filteredOutput.Softmax()

            Return softmaxOutput
        End Function

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            WoOptimizer.MakeTrainingStep(learningRate, [step], Wo)
        End Sub

    End Class
End Namespace
