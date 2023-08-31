Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

Namespace CNN.layers

    Public Class Conv2DTransposeLayer : Inherits DataLink
        Implements Layer

        Public ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                For i As Integer = 0 To out_depth - 1
                    Yield New BackPropResult(filters(i).Weights, filters(i).Gradients, l2_decay_mul, l1_decay_mul)
                Next
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Conv2DTranspose
            End Get
        End Property

        Friend out_depth, out_sx, out_sy As Integer
        Friend in_depth, in_sx, in_sy As Integer
        Friend sx, sy As Integer
        Friend stride, padding As Integer
        Friend filters As DataBlock()
        Friend l1_decay_mul As Double = 0.0
        Friend l2_decay_mul As Double = 1.0

        Public Sub New(def As OutputDefinition, sx As Integer, stride As Integer, padding As Integer)
            Me.out_depth = def.depth
            Me.in_depth = def.depth
            Me.in_sx = def.outX
            Me.in_sy = def.outY
            Me.sx = sx
            Me.sy = sx
            Me.stride = stride
            Me.padding = padding

            out_sx = (in_sx - 1) * stride + sx
            out_sy = (in_sy - 1) * stride + sy

            ' initializations
            Me.filters = New DataBlock(out_depth - 1) {}

            For i As Integer = 0 To out_depth - 1
                Me.filters(i) = New DataBlock(Me.sx, sy, in_depth)
            Next

            ' biases = New DataBlock(1, 1, out_depth, BIAS_PREF)

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        Public Sub backward() Implements Layer.backward
            Dim db = in_act.clearGradient
            Dim V_sx = db.SX
            Dim V_sy = db.SY

            For d As Integer = 0 To out_depth - 1
                Dim f = filters(d)

                For i As Integer = 0 To out_sx
                    Dim i_prime = i * stride
                    For j As Integer = 0 To out_sy
                        Dim j_prime = j * stride
                        Dim chain_grad = out_act.getGradient(i, j, d)

                        For k_row As Integer = 0 To f.SX
                            For k_col As Integer = 0 To f.SY
                                f.addGradient(k_row, k_col, d, in_act.getWeight(i, j, d) * chain_grad)
                                in_act.addGradient(i_prime + k_row, j_prime + k_col, d, f.getWeight(k_row, k_col, d) * chain_grad)
                            Next
                        Next
                    Next
                Next
            Next
        End Sub

        Public Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            out_act = New DataBlock(out_sx, out_sy, out_depth)
            in_act = db

            For d As Integer = 0 To out_depth - 1
                Dim f = filters(d)
                Dim wi As Double

                For i As Integer = 0 To out_sx
                    Dim i_prime = i * stride
                    For j As Integer = 0 To out_sy
                        Dim j_prime = j * stride
                        For k_row As Integer = 0 To f.SX - 1
                            For k_col As Integer = 0 To f.SY - 1
                                wi = out_act.getWeight(i, j, d) + f.getWeight(k_row, k_col, d) * in_act.getWeight(i, j, d)
                                out_act.setWeight(i_prime + k_row, j_prime + k_col, d, wi)
                            Next
                        Next
                    Next
                Next
            Next

            Return out_act
        End Function
    End Class
End Namespace