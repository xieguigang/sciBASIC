Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
Imports Microsoft.VisualBasic.Math

Namespace CNN.layers

    ''' <summary>
    ''' Implements Sigmoid nonlinearity elementwise x to 1/(1+e^(-x))
    ''' so the output is between 0 and 1.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    <Serializable>
    Public Class SigmoidLayer : Inherits DataLink
        Implements Layer

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Sigmoid
            End Get
        End Property

        Sub New()
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim V2 As DataBlock = db.cloneAndZero()
            Dim N = db.Weights.Length
            Dim V2_w = V2.Weights

            in_act = db
            ' exp(-v2)
            V2_w = SIMD.Exponent.f64_exp(SIMD.Subtract.f64_scalar_op_subtract_f64(0, V2_w))
            ' 1 + exp(-v2)
            V2_w = SIMD.Add.f64_op_add_f64_scalar(V2_w, 1)
            ' 1 / (1 + exp(-v2))
            V2_w = SIMD.Divide.f64_scalar_op_divide_f64(1, V2_w)

            'For i = 0 To N - 1
            '    V2.setWeight(i, 1.0 / (1.0 + std.Exp(-V2.getWeight(i))))
            'Next

            V2.w = V2_w
            out_act = V2

            Return out_act
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            ' we need to set dw of this
            ' zero out gradient wrt data
            Dim V = in_act.clearGradient()
            Dim V2 = out_act
            Dim N = V.Weights.Length
            Dim V2_w = V2.Weights
            Dim V2_dw = V2.Gradients
            Dim V_dw = SIMD.Multiply.f64_op_multiply_f64(
                SIMD.Multiply.f64_op_multiply_f64(V2_w, SIMD.Subtract.f64_scalar_op_subtract_f64(1, V2_w)), V2_dw)

            'For i = 0 To N - 1
            '    Dim v2wi = V2.getWeight(i)
            '    V.setGradient(i, v2wi * (1.0 - v2wi) * V2.getGradient(i))
            'Next
            Call V.setGradient(V_dw)
        End Sub

        Public Overrides Function ToString() As String
            Return "sigmoid()"
        End Function
    End Class

End Namespace
