#Region "Microsoft.VisualBasic::3bc70e7ab4390cf25f6f94ceb7c787d9, Data_science\MachineLearning\MachineLearning\NeuralNetwork\Models\Synapse.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Synapse
    ' 
    '         Properties: Gradient, InputNeuron, OutputNeuron, Value, Weight
    '                     WeightDelta
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace NeuralNetwork

    ''' <summary>
    ''' （神经元的）突触 a connection between two nerve cells
    ''' </summary>
    ''' <remarks>
    ''' 可以将这个对象看作为网络节点之间的边链接
    ''' </remarks>
    Public Class Synapse

#Region "-- Properties --"
        Public Property InputNeuron As Neuron
        Public Property OutputNeuron As Neuron
        ''' <summary>
        ''' 两个神经元之间的连接强度
        ''' </summary>
        ''' <returns></returns>
        Public Property Weight As Double
        Public Property WeightDelta As Double
#End Region

        ''' <summary>
        ''' ``a.Weight * a.InputNeuron.Value``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As Double
            Get
                ' 在这里为了防止出现 0 * Inf = NaN 的情况出现
                If Weight = 0R OrElse InputNeuron.Value = 0 Then
                    Return 0
                Else
                    Return Weight * InputNeuron.Value
                End If
            End Get
        End Property

        ''' <summary>
        ''' ``a.OutputNeuron.Gradient * a.Weight``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Gradient As Double
            Get
                If OutputNeuron.Gradient = 0R OrElse Weight = 0R Then
                    Return 0
                Else
                    Return OutputNeuron.Gradient * Weight
                End If
            End Get
        End Property

        Public Sub New(inputNeuron As Neuron, outputNeuron As Neuron, weight As Func(Of Double))
            Call Me.New(inputNeuron, outputNeuron)

            ' 权重初始
            Me.Weight = weight()
            Me.WeightDelta = weight()
        End Sub

        ''' <summary>
        ''' Create from xml model
        ''' </summary>
        ''' <param name="inputNeuron"></param>
        ''' <param name="outputNeuron"></param>
        Sub New(inputNeuron As Neuron, outputNeuron As Neuron)
            Me.InputNeuron = inputNeuron
            Me.OutputNeuron = outputNeuron
        End Sub

        Public Overrides Function ToString() As String
            Return $"{InputNeuron.Guid}=>{OutputNeuron.Guid}"
        End Function
    End Class
End Namespace
