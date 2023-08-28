Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN.losslayers

    ''' <summary>
    ''' Regression layer is used when your output is an area of data.
    ''' When you don't have a single class that is the correct activation
    ''' but you try to find a result set near to your training area.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class RegressionLayer
        Inherits LossLayer
        Public Sub New(def As OutputDefinition)
            MyBase.New(def)
        End Sub

        Public Overrides Function forward(db As DataBlock, training As Boolean) As DataBlock
            in_act = db
            out_act = db ' nothing to do, output raw scores
            Return db
        End Function

        Public Overridable Overloads Function backward(y As Double()) As Double
            ' compute and accumulate gradient wrt weights and bias of this layer
            Dim x = in_act
            x.clearGradient() ' zero out the gradient of input Vol
            Dim loss = 0.0
            For i = 0 To out_depth - 1
                Dim dy = x.getWeight(i) - y(i)
                x.setGradient(i, dy)
                loss += 0.5 * dy * dy
            Next
            Return loss
        End Function

        Public Overrides Function backward(y As Integer) As Double
            ' compute and accumulate gradient wrt weights and bias of this layer
            Dim x = in_act
            x.clearGradient() ' zero out the gradient of input Vol
            Dim loss = 0.0

            ' lets hope that only one number is being regressed
            Dim dy = x.getWeight(0) - y
            x.setGradient(0, dy)
            loss += 0.5 * dy * dy

            Return loss
        End Function
    End Class

End Namespace
