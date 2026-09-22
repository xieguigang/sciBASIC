#Region "Microsoft.VisualBasic::25169d509a4ed8bd65fab28918776e34, Data_science\MachineLearning\DeepLearning\CNN\ConvolutionalNN.vb"

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

    '   Total Lines: 222
    '    Code Lines: 125 (56.31%)
    ' Comment Lines: 58 (26.13%)
    '    - Xml Docs: 93.10%
    ' 
    '   Blank Lines: 39 (17.57%)
    '     File Size: 7.63 KB


    '     Class ConvolutionalNN
    ' 
    '         Properties: BackPropagationResult, input, LayerNum, output, Prediction
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) backward, forward, GetThreads, (+2 Overloads) predict, take
    '                   ToString
    ' 
    '         Sub: SetThreads
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers
Imports Microsoft.VisualBasic.Parallel

Namespace CNN

    ''' <summary>
    ''' A network class holding the layers and some helper functions
    ''' for training and validation.
    ''' 
    ''' Convolutional neural network (CNN) is a regularized type of feed-forward 
    ''' neural network that learns feature engineering by itself via filters
    ''' (or kernel) optimization. Vanishing gradients and exploding gradients, 
    ''' seen during backpropagation in earlier neural networks, are prevented by 
    ''' using regularized weights over fewer connections.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com) and s.chekanov 
    ''' </summary>
    ''' <remarks>
    ''' + https://github.com/kalaspuffar/JavaCNN
    ''' + https://github.com/karpathy/convnetjs
    ''' </remarks>
    Public Class ConvolutionalNN

        Dim m_layers As Layer()

        ''' <summary>Gets the number of layers.</summary>
        Public ReadOnly Property LayerNum As Integer
            Get
                Return m_layers.Length
            End Get
        End Property

        ''' <summary>Gets the input layer of the network.</summary>
        Public ReadOnly Property input As InputLayer
            Get
                Return m_layers(0)
            End Get
        End Property

        ''' <summary>Gets the output (loss) layer of the network.</summary>
        Public ReadOnly Property output As LossLayer
            Get
                Return m_layers(m_layers.Length - 1)
            End Get
        End Property

        ''' <summary>
        ''' Gets the layer at the given index, where index 0 is the input layer and the last index is the output layer.
        ''' </summary>
        ''' <param name="i">Zero based index of the layer.</param>
        ''' <returns>The layer at <paramref name="i"/>.</returns>
        Default Public ReadOnly Property Layer(i As Integer) As Layer
            Get
                Return m_layers(i)
            End Get
        End Property

        ''' <summary>Accumulates the parameters and gradients of every layer of the network.</summary>
        ''' <returns>The flattened parameter/gradient blocks of all layers.</returns>
        Public Overridable ReadOnly Property BackPropagationResult As BackPropResult()
            Get
                Return m_layers _
                    .Select(Function(l) l.BackPropagationResult) _
                    .IteratesALL _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' Convenience function that returns the argmax prediction, assuming the last layer of the network is a
        ''' softmax.
        ''' </summary>
        ''' <returns>The index of the most probable class.</returns>
        Public Overridable ReadOnly Property Prediction As Integer
            Get
                Dim S As LossLayer = output
                Dim p = S.OutAct.Weights
                Dim i As Integer = which.Max(p)

                Return i
            End Get
        End Property

        ''' <summary>
        ''' Creates a network from a fully built <see cref="LayerBuilder"/>.
        ''' </summary>
        ''' <param name="layers">The builder that holds the ordered layers of the network.</param>
        Public Sub New(layers As LayerBuilder)
            Me.m_layers = CType(layers, List(Of Layer)).ToArray
        End Sub

        ''' <summary>
        ''' Creates a network directly from an ordered layer sequence.
        ''' </summary>
        ''' <param name="layers">The layers of the network, from the input layer to the output layer.</param>
        Sub New(layers As IEnumerable(Of Layer))
            m_layers = layers.ToArray
        End Sub

        ''' <summary>
        ''' Runs a forward pass and returns the output activations of the network.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <returns>The output vector produced by the last layer.</returns>
        Public Function predict(db As DataBlock) As Double()
            Call forward(db, training:=Nothing)

            Dim S As LossLayer = output
            Dim p = S.OutAct.Weights

            Return p
        End Function

        ''' <summary>
        ''' Runs a forward pass for a flat input vector and returns the output activations.
        ''' </summary>
        ''' <param name="v">
        ''' The input values; they are written into a <see cref="DataBlock"/> built from the input layer shape.
        ''' </param>
        ''' <returns>The output vector produced by the last layer.</returns>
        Public Function predict(v As Double()) As Double()
            Dim input_shape = input
            Dim x As New DataBlock(input_shape.dims.x, input_shape.dims.y, input_shape.out_depth, 0)
            Call x.addImageData(v, 1.0)
            Return predict(x)
        End Function

        ''' <summary>
        ''' Builds a truncated copy of this network that keeps only the first <paramref name="n"/> layers; a helper for
        ''' VAE style implementations.
        ''' </summary>
        ''' <param name="n">Number of leading layers to keep.</param>
        ''' <returns>A network whose last layer is the embedding layer used to produce the outputs.</returns>
        Public Function take(n As Integer) As ConvolutionalNN
            Dim take_layers As New List(Of Layer)

            For i As Integer = 0 To n - 1
                Call take_layers.Add(m_layers(i))
            Next

            ' the last layer must be the loss layer for
            ' make outputs 
            Call take_layers.Add(output)

            Return New ConvolutionalNN(take_layers)
        End Function

        ''' <summary>
        ''' Runs a forward pass through all layers of the network.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">
        ''' Optional performance counter. When supplied the layers run in training mode and each layer is timed; when
        ''' omitted the network runs in prediction mode.
        ''' </param>
        ''' <returns>The output data block produced by the last layer.</returns>
        Public Overridable Function forward(db As DataBlock, Optional training As PerformanceCounter = Nothing) As DataBlock
            Dim flag As Boolean = Not training Is Nothing
            Dim act = m_layers(0).forward(db, training:=flag)

            If flag Then
                Call training.Mark("[forward]" & m_layers(0).ToString)
            End If

            For i As Integer = 1 To m_layers.Length - 1
                act = m_layers(i).forward(act, training:=flag)

                If flag Then
                    Call training.Mark("[forward]" & m_layers(i).ToString)
                End If
            Next

            Return act
        End Function

        ''' <summary>Backpropagates the loss and computes the gradients with respect to all parameters.</summary>
        ''' <param name="y">The target output vector.</param>
        ''' <param name="training">Optional performance counter used to time each layer backward pass.</param>
        ''' <returns>The loss value reported by the output layer.</returns>
        Public Overridable Function backward(y As Double(), Optional training As PerformanceCounter = Nothing) As Double()
            Dim flag As Boolean = Not training Is Nothing
            Dim N = m_layers.Length
            Dim loss = output.backward(y)

            If flag Then
                Call training.Mark("[backward]" & output.ToString)
            End If

            For i As Integer = N - 2 To 0 Step -1 ' first layer assumed input
                Call m_layers(i).backward()

                If flag Then
                    Call training.Mark("[backward]" & m_layers(i).ToString)
                End If
            Next

            Return loss
        End Function

        ''' <summary>Backpropagates a classification loss and computes the gradients with respect to all parameters.</summary>
        ''' <param name="y">Index of the target class.</param>
        ''' <param name="training">Optional performance counter used to time each layer backward pass.</param>
        ''' <returns>The loss value reported by the output layer.</returns>
        Public Overridable Function backward(y As Integer, training As PerformanceCounter) As Double
            Dim N = m_layers.Length
            Dim loss = output.backward(y)

            If Not training Is Nothing Then
                Call training.Mark("[backward]" & output.ToString)
            End If

            For i As Integer = N - 2 To 0 Step -1 ' first layer assumed input
                Call m_layers(i).backward()

                If Not training Is Nothing Then
                    Call training.Mark("[backward]" & m_layers(i).ToString)
                End If
            Next

            Return loss
        End Function

        ''' <summary>Sets the number of worker threads used by the vector task scheduler.</summary>
        ''' <param name="n">The number of threads.</param>
        Public Shared Sub SetThreads(n As Integer)
            VectorTask.n_threads = n
        End Sub

        ''' <summary>Gets the number of worker threads used by the vector task scheduler.</summary>
        ''' <returns>The configured thread count.</returns>
        Public Shared Function GetThreads() As Integer
            Return VectorTask.n_threads
        End Function

        ''' <summary>Returns a one line description of the layer chain.</summary>
        ''' <returns>A text that lists the layers joined by <c>-&gt;</c>.</returns>
        Public Overrides Function ToString() As String
            Return $"{m_layers.Count} CNN layers: {m_layers.JoinBy(" -> ")}"
        End Function
    End Class

End Namespace
