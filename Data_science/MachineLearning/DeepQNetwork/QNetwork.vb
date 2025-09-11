Imports Microsoft.VisualBasic.MachineLearning.CNN

''' <summary>
''' Create Q-learning model based on a deep full connected network
''' </summary>
Public Class QNetwork

    ReadOnly DNN As ConvolutionalNN
    ReadOnly actionSet As Array

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

        If hiddens.IsNullOrEmpty Then
            hiddens = {statSize * 8, statSize * 16, actionSet.Length * 2}
        End If

        DNN = buildModel(statSize, hiddens, actionSet.Length)
    End Sub

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
    End Sub

End Class
