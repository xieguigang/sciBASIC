Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math

Namespace CNN.losslayers


    ''' <summary>
    ''' This layer will squash the result of the activations in the fully
    ''' connected layer and give you a value of 0 to 1 for all output activations.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class SoftMaxLayer
        Inherits LossLayer

        Private es As Double()

        Public Sub New(def As OutputDefinition)
            MyBase.New(def)
            ' computed
            num_inputs = def.OutY * def.OutX * def.Depth
            out_depth = num_inputs
            out_sx = 1
            out_sy = 1

            def.OutX = out_sx
            def.OutY = out_sy
            def.Depth = out_depth
        End Sub

        Public Overrides Function forward(db As DataBlock, training As Boolean) As DataBlock
            in_act = db

            Dim A As DataBlock = New DataBlock(1, 1, out_depth, 0.0)

            ' compute max activation
            Dim [as] = db.Weights
            Dim amax = db.getWeight(0)
            For i = 1 To out_depth - 1
                If [as](i) > amax Then
                    amax = [as](i)
                End If
            Next

            ' compute exponentials (carefully to not blow up)
            Dim es = New Double(out_depth - 1) {}
            es.fill(0)
            Dim esum = 0.0
            For i = 0 To out_depth - 1
                Dim e = std.Exp([as](i) - amax)
                esum += e
                es(i) = e
            Next

            ' normalize and output to sum to one
            For i = 0 To out_depth - 1
                es(i) /= esum
                A.setWeight(i, es(i))
            Next

            Me.es = es ' save these for backprop
            out_act = A
            Return out_act
        End Function

        Public Overrides Function backward(y As Integer) As Double

            ' compute and accumulate gradient wrt weights and bias of this layer
            Dim x = in_act
            x.clearGradient() ' zero out the gradient of input Vol

            For i = 0 To out_depth - 1
                Dim indicator = If(i = y, 1.0, 0.0)
                Dim mul = -(indicator - es(i))
                x.setGradient(i, mul)
            Next

            ' loss is the class negative log likelihood
            Return -std.Log(es(y))
        End Function
    End Class
End Namespace
