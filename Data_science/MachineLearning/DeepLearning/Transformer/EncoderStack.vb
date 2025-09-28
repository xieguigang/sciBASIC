#Region "Microsoft.VisualBasic::b14f07c4eb436e4e474ed512fcfe4bba, Data_science\MachineLearning\DeepLearning\Transformer\EncoderStack.vb"

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

    '   Total Lines: 40
    '    Code Lines: 30 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (25.00%)
    '     File Size: 1.33 KB


    '     Class EncoderStack
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Encode
    ' 
    '         Sub: MakeTrainingStep, SetDropoutNodes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    Public Class EncoderStack

        Private Nx As Integer
        Private encoderLayers As New List(Of EncoderLayer)()

        Public Sub New(Nx As Integer, embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.Nx = Nx

            For i = 0 To Nx - 1
                encoderLayers.Add(New EncoderLayer(embeddingSize, dk, dv, h, dff))
            Next
        End Sub

        Public Function Encode(word_embeddings As Tensor, isTraining As Boolean) As Tensor
            Dim encoderOutput = encoderLayers(0).Encode(word_embeddings, isTraining)
            For i = 1 To Nx - 1
                encoderOutput = encoderLayers(i).Encode(encoderOutput, isTraining)
            Next

            Return encoderOutput
        End Function

        Public Sub SetDropoutNodes(dropout As Double)
            For i = 0 To Nx - 1
                encoderLayers(i).SetDropoutNodes(dropout)
            Next
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            For i = 0 To Nx - 1
                encoderLayers(i).MakeTrainingStep(learningRate, [step])
            Next
        End Sub

    End Class
End Namespace

