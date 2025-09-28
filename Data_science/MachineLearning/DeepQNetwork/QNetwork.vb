#Region "Microsoft.VisualBasic::9791e233fa9c26a52b72dc06d993d1ca, Data_science\MachineLearning\DeepQNetwork\QNetwork.vb"

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
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers

''' <summary>
''' Create Q-learning model based on a deep full connected network
''' </summary>
Public Class QNetwork

    ReadOnly DNN As ConvolutionalNN
    ReadOnly actionSet As Array

    Dim ada As TrainerAlgorithm

    ''' <summary>
    ''' Create a new Q-learning model
    ''' </summary>
    ''' <param name="statSize">
    ''' the vector size of the current world status
    ''' </param>
    ''' <param name="actions">
    ''' should be a <see cref="System.Enum"/> value type of the output actions
    ''' </param>
    ''' <param name="hiddens">
    ''' configs of the hidden layers, is a vector of the neuron nodes in each hidden layers
    ''' </param>
    Sub New(statSize As Integer, actions As Type, Optional hiddens As Integer() = Nothing)
        Call Me.New(actions)
        DNN = buildModel(statSize, defaultShape(hiddens, statSize, actionSet.Length), actionSet.Length)
        ada = New AdaGradTrainer(5, 0.001)
        ada.SetKernel(DNN)
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
        Call Me.New(actions)
        DNN = Q
        ada = New AdaGradTrainer(5, 0.001)
        ada.SetKernel(DNN)
    End Sub

End Class

