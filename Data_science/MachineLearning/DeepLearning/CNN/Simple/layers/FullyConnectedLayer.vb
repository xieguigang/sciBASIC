Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN.layers

    ''' <summary>
    ''' Neurons in a fully connected layer have full connections to all
    ''' activations in the previous layer, as seen in regular Neural Networks.
    ''' Their activations can hence be computed with a matrix multiplication
    ''' followed by a bias offset.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class FullyConnectedLayer : Implements Layer

        Private l1_decay_mul As Double = 0.0
        Private l2_decay_mul As Double = 1.0

        Private in_act As DataBlock
        Private out_act As DataBlock

        Private ReadOnly BIAS_PREF As Single = 0.0F

        Private out_depth, out_sx, out_sy As Integer
        Private num_inputs As Integer
        Private filters As IList(Of DataBlock)
        Private biases As DataBlock

        Public Overridable ReadOnly Property BackPropagationResult As IList(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                Dim results As IList(Of BackPropResult) = New List(Of BackPropResult)()
                For i = 0 To out_depth - 1
                    results.Add(New BackPropResult(filters(i).Weights, filters(i).Gradients, l1_decay_mul, l2_decay_mul))
                Next
                results.Add(New BackPropResult(biases.Weights, biases.Gradients, 0.0, 0.0))

                Return results
            End Get
        End Property

        Public Sub New(def As OutputDefinition, num_neurons As Integer)
            out_depth = num_neurons

            ' computed
            num_inputs = def.outX * def.outY * def.depth
            out_sx = 1
            out_sy = 1

            ' initializations
            Dim bias = BIAS_PREF
            filters = New List(Of DataBlock)()
            For i = 0 To out_depth - 1
                filters.Add(New DataBlock(1, 1, num_inputs))
            Next
            biases = New DataBlock(1, 1, out_depth, bias)

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As DataBlock = New DataBlock(1, 1, out_depth, 0.0)
            Dim Vw = db.Weights

            in_act = db

            For i = 0 To out_depth - 1
                Dim a = 0.0
                Dim wi = filters(i).Weights
                For d = 0 To num_inputs - 1
                    a += Vw(d) * wi(d) ' for efficiency use Vols directly for now
                Next
                a += biases.getWeight(i)
                lA.setWeight(i, a)
            Next
            out_act = lA
            Return out_act
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            Dim V = in_act
            V.clearGradient()

            ' compute gradient wrt weights and data
            For i = 0 To out_depth - 1
                Dim tfi = filters(i)
                Dim chain_grad = out_act.Gradients(i)
                For d = 0 To num_inputs - 1
                    V.addGradient(d, tfi.getWeight(d) * chain_grad) ' grad wrt input data
                    tfi.addGradient(d, V.getWeight(d) * chain_grad) ' grad wrt params
                Next
                biases.addGradient(i, chain_grad)
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return $"full_connected({out_depth})"
        End Function
    End Class

End Namespace
