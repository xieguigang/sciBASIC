#Region "Microsoft.VisualBasic::51ff1bc643415d5120278fd16034f001, Data_science\MachineLearning\DeepLearning\Transformer\EncoderStack.vb"

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

    '   Total Lines: 100
    '    Code Lines: 42 (42.00%)
    ' Comment Lines: 43 (43.00%)
    '    - Xml Docs: 86.05%
    ' 
    '   Blank Lines: 15 (15.00%)
    '     File Size: 4.28 KB


    '     Class EncoderStack
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Backward, Encode
    ' 
    '         Sub: MakeTrainingStep, SetDropoutNodes, ZeroGradients
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' EncoderStack —— 编码器堆叠（Nx 层）
'
' 编码器在一次前向翻译中只执行一次，因此各层的前向缓存可以直接保存在层对象上，
' 反向传播时按层逆序回传即可。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    ''' <summary>
    ''' A stack of <see cref="EncoderLayer"/> instances, applied one after another.
    ''' </summary>
    ''' <remarks>
    ''' The encoder runs only once per translation, so the forward cache of every layer can be kept on the layer object and
    ''' the backward pass simply walks the layers in reverse order.
    ''' </remarks>
    Public Class EncoderStack

        Private Nx As Integer
        Private encoderLayers As New List(Of EncoderLayer)()

        ''' <summary>
        ''' Creates an encoder stack with the given number of identical layers.
        ''' </summary>
        ''' <param name="Nx">Number of encoder layers.</param>
        ''' <param name="embeddingSize">Width of the model.</param>
        ''' <param name="dk">Dimension of the query and key projections per head.</param>
        ''' <param name="dv">Dimension of the value projection per head.</param>
        ''' <param name="h">Number of attention heads.</param>
        ''' <param name="dff">Hidden width of the feed forward network.</param>
        Public Sub New(Nx As Integer, embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.Nx = Nx

            For i = 0 To Nx - 1
                encoderLayers.Add(New EncoderLayer(embeddingSize, dk, dv, h, dff))
            Next
        End Sub

        ''' <summary>
        ''' Runs the embedded input through all encoder layers.
        ''' </summary>
        ''' <param name="word_embeddings">The embedded input sequence.</param>
        ''' <param name="isTraining">When <c>True</c> dropout is applied where configured.</param>
        ''' <returns>The output of the last encoder layer.</returns>
        Public Function Encode(word_embeddings As Tensor, isTraining As Boolean) As Tensor
            Dim encoderOutput = encoderLayers(0).Encode(word_embeddings, isTraining)
            For i = 1 To Nx - 1
                encoderOutput = encoderLayers(i).Encode(encoderOutput, isTraining)
            Next

            Return encoderOutput
        End Function

        ''' <summary>
        ''' Backpropagates through all encoder layers.
        ''' </summary>
        ''' <param name="dOut">Gradient with respect to the encoder output.</param>
        ''' <returns>The gradient with respect to the encoder input (the word embeddings).</returns>
        Public Function Backward(dOut As Tensor) As Tensor
            Dim d = dOut

            For i = Nx - 1 To 0 Step -1
                d = encoderLayers(i).Backward(d, encoderLayers(i).LastCache)
            Next

            Return d
        End Function

        ''' <summary>
        ''' Configures dropout on every encoder layer.
        ''' </summary>
        ''' <param name="dropout">Dropout rate in <c>[0, 1)</c>.</param>
        Public Sub SetDropoutNodes(dropout As Double)
            For i = 0 To Nx - 1
                encoderLayers(i).SetDropoutNodes(dropout)
            Next
        End Sub

        ''' <summary>Clears the gradient accumulators of every encoder layer.</summary>
        Public Sub ZeroGradients()
            For i = 0 To Nx - 1
                encoderLayers(i).ZeroGradients()
            Next
        End Sub

        ''' <summary>
        ''' Applies one optimizer step to every encoder layer.
        ''' </summary>
        ''' <param name="learningRate">The learning rate for this step.</param>
        ''' <param name="[step]">The current step index, used by the Adam bias correction.</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            For i = 0 To Nx - 1
                encoderLayers(i).MakeTrainingStep(learningRate, [step])
            Next
        End Sub

    End Class
End Namespace
