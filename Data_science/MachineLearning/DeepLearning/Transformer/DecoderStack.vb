#Region "Microsoft.VisualBasic::b771a8ec9d178789342203bf253eec7d, Data_science\MachineLearning\DeepLearning\Transformer\DecoderStack.vb"

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

    '   Total Lines: 118
    '    Code Lines: 51 (43.22%)
    ' Comment Lines: 48 (40.68%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 19 (16.10%)
    '     File Size: 5.58 KB


    '     Class DecoderStack
    ' 
    '         Properties: LastCaches
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Backward, Decode
    ' 
    '         Sub: MakeTrainingStep, SetDropoutNodes, ZeroGradients
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' DecoderStack —— 解码器堆叠（Nx 层）
'
' 解码按词逐步推进，每一步都会覆盖各层的前向缓存，因此 <see cref="Decode"/> 会把
' 当步每一层的缓存快照保存到 <see cref="LastCaches"/>，供随后的反向传播使用。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    ''' <summary>
    ''' A stack of <see cref="DecoderLayer"/> instances, applied one after another.
    ''' </summary>
    ''' <remarks>
    ''' Decoding proceeds token by token and every step overwrites the forward caches of the layers, so <see cref="Decode"/>
    ''' stores a snapshot of each layer's cache in <see cref="LastCaches"/> for the following backward pass.
    ''' </remarks>
    Public Class DecoderStack

        Private Nx As Integer
        Private decoderLayers As List(Of DecoderLayer) = New List(Of DecoderLayer)()

        Private _lastCaches As List(Of DecoderLayer.Cache)

        ''' <summary>Gets the forward cache snapshot of every layer for the most recent <see cref="Decode"/> step.</summary>
        Public ReadOnly Property LastCaches As List(Of DecoderLayer.Cache)
            Get
                Return _lastCaches
            End Get
        End Property

        ''' <summary>
        ''' Creates a decoder stack with the given number of identical layers.
        ''' </summary>
        ''' <param name="Nx">Number of decoder layers.</param>
        ''' <param name="embeddingSize">Width of the model.</param>
        ''' <param name="dk">Dimension of the query and key projections per head.</param>
        ''' <param name="dv">Dimension of the value projection per head.</param>
        ''' <param name="h">Number of attention heads.</param>
        ''' <param name="dff">Hidden width of the feed forward network.</param>
        Public Sub New(Nx As Integer, embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.Nx = Nx

            For i = 0 To Nx - 1
                decoderLayers.Add(New DecoderLayer(embeddingSize, dk, dv, h, dff))
            Next
        End Sub

        ''' <summary>
        ''' Runs the decoder input through all decoder layers and records the per layer forward cache snapshots.
        ''' </summary>
        ''' <param name="encoderOutput">The output of the encoder stack.</param>
        ''' <param name="word_embeddings">The embedded decoder input.</param>
        ''' <param name="isTraining">When <c>True</c> dropout is applied where configured.</param>
        ''' <returns>The output of the last decoder layer.</returns>
        Public Function Decode(encoderOutput As Tensor, word_embeddings As Tensor, isTraining As Boolean) As Tensor
            _lastCaches = New List(Of DecoderLayer.Cache)()

            Dim decoderOutput = decoderLayers(0).Decode(encoderOutput, word_embeddings, isTraining)
            _lastCaches.Add(decoderLayers(0).LastCache)

            For i = 1 To Nx - 1
                decoderOutput = decoderLayers(i).Decode(encoderOutput, decoderOutput, isTraining)
                _lastCaches.Add(decoderLayers(i).LastCache)
            Next

            Return decoderOutput
        End Function

        ''' <summary>
        ''' Backpropagates through all decoder layers and returns the gradient with respect to the decoder input, while
        ''' accumulating the gradient with respect to the encoder output.
        ''' </summary>
        ''' <param name="caches">The forward cache snapshots that belong to this decode step.</param>
        ''' <param name="dOut">Gradient with respect to the decoder output of this step.</param>
        ''' <param name="dEncoderOutput">Accumulator for the gradient with respect to the encoder output, shared across decode steps.</param>
        ''' <returns>The gradient with respect to the decoder input.</returns>
        Public Function Backward(caches As List(Of DecoderLayer.Cache), dOut As Tensor, ByRef dEncoderOutput As Tensor) As Tensor
            Dim d = dOut

            For i = caches.Count - 1 To 0 Step -1
                d = decoderLayers(i).Backward(caches(i), d, dEncoderOutput)
            Next

            Return d
        End Function

        ''' <summary>
        ''' Configures dropout on every decoder layer.
        ''' </summary>
        ''' <param name="dropout">Dropout rate in <c>[0, 1)</c>.</param>
        Public Sub SetDropoutNodes(dropout As Double)
            For i = 0 To Nx - 1
                decoderLayers(i).SetDropoutNodes(dropout)
            Next
        End Sub

        ''' <summary>Clears the gradient accumulators of every decoder layer.</summary>
        Public Sub ZeroGradients()
            For i = 0 To Nx - 1
                decoderLayers(i).ZeroGradients()
            Next
        End Sub

        ''' <summary>
        ''' Applies one optimizer step to every decoder layer.
        ''' </summary>
        ''' <param name="learningRate">The learning rate for this step.</param>
        ''' <param name="[step]">The current step index, used by the Adam bias correction.</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            For i = 0 To Nx - 1
                decoderLayers(i).MakeTrainingStep(learningRate, [step])
            Next
        End Sub

    End Class
End Namespace
