#Region "Microsoft.VisualBasic::c867ce4518b478f2bbea4925123cbe9e, Data_science\MachineLearning\DeepLearning\CNN\ConvolutionalNN.vb"

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

    '   Total Lines: 177
    '    Code Lines: 107 (60.45%)
    ' Comment Lines: 37 (20.90%)
    '    - Xml Docs: 94.59%
    ' 
    '   Blank Lines: 33 (18.64%)
    '     File Size: 6.14 KB


    '     Class ConvolutionalNN
    ' 
    '         Properties: BackPropagationResult, input, LayerNum, output, Prediction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) backward, forward, GetThreads, predict, ToString
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

        Public ReadOnly Property LayerNum As Integer
            Get
                Return m_layers.Length
            End Get
        End Property

        Public ReadOnly Property input As InputLayer
            Get
                Return m_layers(0)
            End Get
        End Property

        Public ReadOnly Property output As LossLayer
            Get
                Return m_layers(m_layers.Length - 1)
            End Get
        End Property

        Default Public ReadOnly Property Layer(i As Integer) As Layer
            Get
                Return m_layers(i)
            End Get
        End Property

        ''' <summary>
        ''' Accumulate parameters and gradients for the entire network
        ''' </summary>
        Public Overridable ReadOnly Property BackPropagationResult As BackPropResult()
            Get
                Return m_layers _
                    .Select(Function(l) l.BackPropagationResult) _
                    .IteratesALL _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' This is a convenience function for returning the argmax
        ''' prediction, assuming the last layer of the net is a softmax
        ''' </summary>
        Public Overridable ReadOnly Property Prediction As Integer
            Get
                Dim S As LossLayer = output
                Dim p = S.OutAct.Weights
                Dim i As Integer = which.Max(p)

                Return i
            End Get
        End Property

        Public Sub New(layers As LayerBuilder)
            Me.m_layers = CType(layers, List(Of Layer)).ToArray
        End Sub

        Sub New(layers As IEnumerable(Of Layer))
            m_layers = layers.ToArray
        End Sub

        Public Function predict(db As DataBlock) As Double()
            Call forward(db, training:=Nothing)

            Dim S As LossLayer = output
            Dim p = S.OutAct.Weights

            Return p
        End Function

        Public Function predict(v As Double()) As Double()

        End Function

        ''' <summary>
        ''' a helper function for VAE method implements
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns>
        ''' the last layer is the embedding layer for make outputs
        ''' </returns>
        Public Function take(n As Integer) As ConvolutionalNN
            Dim take_layers As New List(Of Layer)

            For i As Integer = 0 To n - 1
                take_layers.Add(m_layers(i))
            Next

            Return New ConvolutionalNN(take_layers)
        End Function

        ''' <summary>
        ''' Forward prop the network.
        ''' The trainer class passes is_training = true, but when this function is
        ''' called from outside (not from the trainer), it defaults to prediction mode
        ''' </summary>
        ''' <param name="db"></param>
        ''' <param name="training"></param>
        ''' <returns></returns>
        Public Overridable Function forward(db As DataBlock, training As PerformanceCounter) As DataBlock
            Dim flag As Boolean = Not training Is Nothing
            Dim act = m_layers(0).forward(db, training:=flag)

            If Not training Is Nothing Then
                Call training.Mark("[forward]" & m_layers(0).ToString)
            End If

            For i As Integer = 1 To m_layers.Length - 1
                act = m_layers(i).forward(act, training:=flag)

                If Not training Is Nothing Then
                    Call training.Mark("[forward]" & m_layers(i).ToString)
                End If
            Next

            Return act
        End Function

        ''' <summary>
        ''' Backprop: compute gradients wrt all parameters
        ''' </summary>
        Public Overridable Function backward(y As Double(), training As PerformanceCounter) As Double()
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

        ''' <summary>
        ''' Backprop: compute gradients wrt all parameters
        ''' </summary>
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

        Public Shared Sub SetThreads(n As Integer)
            VectorTask.n_threads = n
        End Sub

        Public Shared Function GetThreads() As Integer
            Return VectorTask.n_threads
        End Function

        Public Overrides Function ToString() As String
            Return $"{m_layers.Count} CNN layers: {m_layers.JoinBy(" -> ")}"
        End Function
    End Class

End Namespace
