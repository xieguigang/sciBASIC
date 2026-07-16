#Region "Microsoft.VisualBasic::a9a46aba165865ebdf3898fcd7a46bcb, Data_science\MachineLearning\DeepQNetwork\QNetwork.vb"

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

    '   Total Lines: 72
    '    Code Lines: 38 (52.78%)
    ' Comment Lines: 20 (27.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (19.44%)
    '     File Size: 2.48 KB


    ' Class QNetwork
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: buildModel, defaultShape
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers

''' <summary>
''' Create Q-learning model based on a deep full connected network
''' </summary>
Public Class QNetwork

    ReadOnly DNN As ConvolutionalNN
    ReadOnly actionSet As Array

    Dim ada As TrainerAlgorithm

    ' architecture cache (for clone / rebuild / state-size inference)
    Private ReadOnly stateSize As Integer
    Private ReadOnly hiddenSizes As Integer()
    Private ReadOnly actionType As Type

    ''' <summary>
    ''' Create a new Q-learning model
    ''' </summary>
    ''' <param name="statSize">the vector size of the current world status</param>
    ''' <param name="actions">should be a <see cref="System.Enum"/> value type of the output actions</param>
    ''' <param name="hiddens">configs of the hidden layers, is a vector of the neuron nodes in each hidden layers</param>
    ''' <param name="learningRate">the optimizer step size for the online Q-learning update</param>
    Sub New(statSize As Integer, actions As Type, Optional hiddens As Integer() = Nothing, Optional learningRate As Double = 0.001)
        Me.stateSize = statSize
        Me.actionType = actions
        Call Me.New(actions)
        Dim shape = defaultShape(hiddens, statSize, actionSet.Length)
        Me.hiddenSizes = shape
        DNN = buildModel(statSize, shape, actionSet.Length)
        ada = New AdamTrainer(1, 0.0)
        ada.SetKernel(DNN)
        ada.learning_rate = learningRate
    End Sub

    Private Shared Function defaultShape(hiddens As Integer(), statSize As Integer, actionSize As Integer) As Integer()
        If hiddens.IsNullOrEmpty Then
            hiddens = {statSize * 8, statSize * 16, actionSize * 2}
        End If

        Return hiddens
    End Function

    Private Shared Function buildModel(statSize As Integer, hiddens As Integer(), output As Integer) As ConvolutionalNN
        Dim builder As New LayerBuilder

        Call builder.buildInputLayer(Dimension.One, depth:=statSize)

        For Each size As Integer In hiddens
            Call builder.buildFullyConnectedLayer(size).buildReLULayer()
        Next

        Call builder.buildFullyConnectedLayer(output).buildReLULayer()
        Call builder.buildRegressionLayer()

        Return New ConvolutionalNN(builder)
    End Function

    Private Sub New(actions As Type)
        actionSet = actions.GetEnumValues
    End Sub

    ''' <summary>
    ''' Create from an existed Q-learning model
    ''' </summary>
    ''' <param name="Q"></param>
    ''' <param name="actions">should be a <see cref="System.Enum"/> value type of the output actions</param>
    Sub New(Q As ConvolutionalNN, actions As Type)
        Me.stateSize = Q.input.out_depth
        Me.actionType = actions
        Call Me.New(actions)
        DNN = Q
        ada = New AdamTrainer(1, 0.0)
        ada.SetKernel(DNN)
        ada.learning_rate = 0.001
    End Sub

    ' ---------------- Q-learning runtime interface ----------------

    ''' <summary>dimension of the input world state vector</summary>
    Public ReadOnly Property StateSize As Integer
        Get
            Return stateSize
        End Get
    End Property

    ''' <summary>number of discrete actions</summary>
    Public ReadOnly Property ActionCount As Integer
        Get
            Return actionSet.Length
        End Get
    End Property

    ''' <summary>the enum values of the discrete action set</summary>
    Public ReadOnly Property Actions As Array
        Get
            Return actionSet
        End Get
    End Property

    ''' <summary>the optimizer learning rate (set at runtime)</summary>
    Public Property learningRate As Double
        Get
            Return ada.learning_rate
        End Get
        Set(value As Double)
            ada.learning_rate = value
        End Set
    End Property

    ''' <summary>
    ''' forward pass, returns the Q-value vector for every action.
    ''' The raw state vector is copied directly into the network input
    ''' (no image normalization), so predict and train share the same encoding.
    ''' </summary>
    Public Function predictQ(state As Double()) As Double()
        Dim db = New DataBlock(1, 1, stateSize, 0)
        For i As Integer = 0 To stateSize - 1
            db.setWeight(i, state(i))
        Next
        Dim act = DNN.forward(db, Nothing)
        Return act.Weights
    End Function

    ''' <summary>greedy action selection (argmax over Q-values)</summary>
    Public Function argmaxAction(state As Double()) As Integer
        Dim q = predictQ(state)
        Dim best = 0
        Dim bestv = q(0)
        For i As Integer = 1 To q.Length - 1
            If q(i) > bestv Then
                bestv = q(i)
                best = i
            End If
        Next
        Return best
    End Function

    ''' <summary>
    ''' one supervised regression step: fit the network output toward
    ''' <paramref name="targetQ"/> at the given <paramref name="state"/>.
    ''' With batch_size = 1 this performs an immediate online update.
    ''' </summary>
    Public Sub trainOnTargets(state As Double(), targetQ As Double())
        Dim db = New DataBlock(1, 1, stateSize, 0)
        For i As Integer = 0 To stateSize - 1
            db.setWeight(i, state(i))
        Next
        Call ada.train(db, targetQ, Nothing)
    End Sub

    ''' <summary>deep copy of this Q-network (independent weights)</summary>
    Public Function Clone() As QNetwork
        Dim q = New QNetwork(stateSize, actionType, hiddenSizes, ada.learning_rate)
        q.CopyWeightsFrom(Me)
        Return q
    End Function

    ''' <summary>
    ''' copy weight values from <paramref name="src"/> into this network.
    ''' Both networks must share the same architecture.
    ''' </summary>
    Public Sub CopyWeightsFrom(src As QNetwork)
        Dim mine = DNN.BackPropagationResult.ToArray()
        Dim theirs = src.DNN.BackPropagationResult.ToArray()
        For i As Integer = 0 To mine.Length - 1
            Array.Copy(theirs(i).Weights, 0, mine(i).Weights, 0, mine(i).Weights.Length)
        Next
    End Sub

End Class
