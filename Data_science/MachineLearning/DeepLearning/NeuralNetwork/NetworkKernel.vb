#Region "Microsoft.VisualBasic::9798f53ed94de306a556e3e3c3f54a1f, Data_science\MachineLearning\DeepLearning\NeuralNetwork\NetworkKernel.vb"

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

    '   Total Lines: 87
    '    Code Lines: 43 (49.43%)
    ' Comment Lines: 33 (37.93%)
    '    - Xml Docs: 87.88%
    ' 
    '   Blank Lines: 11 (12.64%)
    '     File Size: 4.19 KB


    '     Module NetworkKernel
    ' 
    '         Function: BuildCNN, BuildDataBlock
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers

Namespace NeuralNetwork

    ''' <summary>
    ''' Uses a fully connected <see cref="ConvolutionalNN"/> as the single compute kernel of the
    ''' NeuralNetwork namespace.
    ''' </summary>
    ''' <remarks>
    ''' The module builds an equivalent fully connected CNN through <see cref="LayerBuilder"/> so that
    ''' <see cref="Netz"/> and <see cref="Network"/> share one numerical back end. The legacy
    ''' forward/backward/weight-update code (Layer/Neuron/Synapse) has been removed entirely; all numeric
    ''' work is performed by the CNN kernel while the public API stays unchanged.
    ''' </remarks>
    Public Module NetworkKernel

        ''' <summary>
        ''' Builds a CNN network made of fully connected layers, used as the unified compute kernel.
        ''' </summary>
        ''' <param name="inputSize">Number of input nodes.</param>
        ''' <param name="hiddenSize">Number of nodes of each hidden layer.</param>
        ''' <param name="outputSize">Number of output nodes.</param>
        ''' <param name="hiddenAct">The CNN activation layer applied in the hidden layers.</param>
        ''' <param name="outputAct">The CNN activation layer applied in the output layer.</param>
        ''' <param name="regression">
        ''' When <c>True</c> (the default) a <see cref="RegressionLayer"/> is appended for continuous
        ''' regression targets; when <c>False</c> a <see cref="SoftMaxLayer"/> is appended for classification.
        ''' </param>
        ''' <param name="dropOutRate">
        ''' Dropout rate in <c>[0, 1)</c>. When greater than zero a <see cref="DropoutLayer"/> is inserted
        ''' after every fully connected layer for regularization.
        ''' </param>
        ''' <returns>The constructed fully connected network.</returns>
        Public Function BuildCNN(inputSize%,
                               hiddenSize%(),
                               outputSize%,
                               hiddenAct As CNN.layers.Layer,
                               outputAct As CNN.layers.Layer,
                               Optional regression As Boolean = True,
                               Optional dropOutRate As Double = 0) As ConvolutionalNN
            Dim builder As New LayerBuilder

            Call builder.buildInputLayer(New Dimension(inputSize, 1), 1)

            For i As Integer = 0 To hiddenSize.Length - 1
                Call builder.buildFullyConnectedLayer(hiddenSize(i))
                Call builder.add(hiddenAct)

                If dropOutRate > 0 Then
                    Call builder.buildDropoutLayer(dropOutRate)
                End If
            Next

            Call builder.buildFullyConnectedLayer(outputSize)

            If dropOutRate > 0 Then
                Call builder.buildDropoutLayer(dropOutRate)
            End If

            If regression Then
                ' 回归任务的输出层应为线性（无激活），以允许输出取任意实数，
                ' 从而正确拟合连续数值目标（例如 a+b ∈ [0,2]）；
                ' 若在此处追加 Sigmoid 等激活，输出会被压到 (0,1)，
                ' 既无法表示大于 1 的目标，也会使训练退化为常数解。
                Call builder.buildRegressionLayer()
            Else
                Call builder.add(outputAct)
                Call builder.buildSoftmaxLayer()
            End If

            Return New ConvolutionalNN(builder)
        End Function

        ''' <summary>
        ''' Wraps a raw input sample into a <see cref="DataBlock"/> weight vector without image normalization.
        ''' </summary>
        ''' <param name="inputs">The raw input values of one sample.</param>
        ''' <returns>A data block whose weights hold <paramref name="inputs"/>.</returns>
        ''' <remarks>
        ''' No normalization is applied so the values keep the plain real-number semantics of the legacy
        ''' Netz/Network input, avoiding the <c>-0.5</c> offset introduced by image normalization.
        ''' </remarks>
        Public Function BuildDataBlock(inputs As Double()) As DataBlock
            Dim db As New DataBlock(inputs.Length, 1, 1, 0)
            Call Array.ConstrainedCopy(inputs, 0, db.w, 0, inputs.Length)
            Return db
        End Function
    End Module
End Namespace
