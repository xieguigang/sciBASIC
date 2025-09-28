#Region "Microsoft.VisualBasic::b30139a34561afe7f31098843230298f, Data_science\MachineLearning\DeepLearning\Transformer\DecoderStack.vb"

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

    '   Total Lines: 38
    '    Code Lines: 30 (78.95%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (21.05%)
    '     File Size: 1.40 KB


    '     Class DecoderStack
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Decode
    ' 
    '         Sub: MakeTrainingStep, SetDropoutNodes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer
    Public Class DecoderStack
        Private Nx As Integer
        Private decoderLayers As List(Of DecoderLayer) = New List(Of DecoderLayer)()

        Public Sub New(Nx As Integer, embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.Nx = Nx

            For i = 0 To Nx - 1
                decoderLayers.Add(New DecoderLayer(embeddingSize, dk, dv, h, dff))
            Next
        End Sub

        Public Function Decode(encoderOutput As Tensor, word_embeddings As Tensor, isTraining As Boolean) As Tensor
            Dim decoderOutput = decoderLayers(0).Decode(encoderOutput, word_embeddings, isTraining)
            For i = 1 To Nx - 1
                decoderOutput = decoderLayers(i).Decode(encoderOutput, decoderOutput, isTraining)
            Next

            Return decoderOutput
        End Function

        Public Sub SetDropoutNodes(dropout As Double)
            For i = 0 To Nx - 1
                decoderLayers(i).SetDropoutNodes(dropout)
            Next
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            For i = 0 To Nx - 1
                decoderLayers(i).MakeTrainingStep(learningRate, [step])
            Next
        End Sub

    End Class
End Namespace

