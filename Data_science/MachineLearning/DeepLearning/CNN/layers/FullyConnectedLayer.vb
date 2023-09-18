Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.Parallel

Namespace CNN.layers

    ''' <summary>
    ''' Neurons in a fully connected layer have full connections to all
    ''' activations in the previous layer, as seen in regular Neural Networks.
    ''' Their activations can hence be computed with a matrix multiplication
    ''' followed by a bias offset.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class FullyConnectedLayer : Inherits DataLink
        Implements Layer

        Private l1_decay_mul As Double = 0.0
        Private l2_decay_mul As Double = 1.0

        Private ReadOnly BIAS_PREF As Single = 0.1F

        Private out_depth, out_sx, out_sy As Integer
        Private num_inputs As Integer
        Private filters As DataBlock()
        Private biases As DataBlock

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                For i As Integer = 0 To out_depth - 1
                    Yield New BackPropResult(filters(i).Weights, filters(i).Gradients, l1_decay_mul, l2_decay_mul)
                Next

                Yield New BackPropResult(biases.Weights, biases.Gradients, 0.0, 0.0)
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.FullyConnected
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub New(def As OutputDefinition, num_neurons As Integer)
            out_depth = num_neurons

            ' computed
            num_inputs = def.outX * def.outY * def.depth
            out_sx = 1
            out_sy = 1

            ' initializations
            filters = New DataBlock(out_depth - 1) {}

            For i As Integer = 0 To out_depth - 1
                filters(i) = New DataBlock(1, 1, num_inputs)
            Next

            biases = New DataBlock(1, 1, out_depth, BIAS_PREF)

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As New DataBlock(1, 1, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db
            out_act = lA

            Call New ForwardTask(Me, lA, db).Run()

            Return out_act
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            Call New BackwardTask(Me, v:=in_act.clearGradient()).Run()
        End Sub

        Private Class ForwardTask : Inherits VectorTask

            Dim layer As FullyConnectedLayer
            Dim lA As DataBlock
            Dim db As DataBlock

            Public Sub New(layer As FullyConnectedLayer, lA As DataBlock, db As DataBlock)
                MyBase.New(layer.out_depth)
                Me.lA = lA
                Me.db = db
                Me.layer = layer
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer)
                Dim Vw As Double() = db.Weights

                For i As Integer = start To ends
                    Dim a = 0.0
                    Dim wi = layer.filters(i).Weights

                    For d As Integer = 0 To layer.num_inputs - 1
                        a += Vw(d) * wi(d) ' for efficiency use Vols directly for now
                    Next

                    a += layer.biases.getWeight(i)
                    lA.setWeight(i, a)
                Next
            End Sub
        End Class

        Private Class BackwardTask : Inherits VectorTask

            Dim v As DataBlock
            Dim layer As FullyConnectedLayer

            Public Sub New(layer As FullyConnectedLayer, v As DataBlock)
                MyBase.New(layer.out_depth)
                Me.v = v
                Me.layer = layer
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer)
                ' compute gradient wrt weights and data
                For i As Integer = start To ends
                    Dim tfi = layer.filters(i)
                    Dim chain_grad = layer.out_act.Gradients(i)

                    For d As Integer = 0 To layer.num_inputs - 1
                        Call v.addGradient(d, tfi.getWeight(d) * chain_grad) ' grad wrt input data
                        Call tfi.addGradient(d, v.getWeight(d) * chain_grad) ' grad wrt params
                    Next

                    Call layer.biases.addGradient(i, chain_grad)
                Next
            End Sub
        End Class

        Public Overrides Function ToString() As String
            Return $"full_connected({out_depth})"
        End Function
    End Class

End Namespace
