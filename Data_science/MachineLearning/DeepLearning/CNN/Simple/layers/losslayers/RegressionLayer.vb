Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

Namespace CNN.losslayers

    ''' <summary>
    ''' Regression layer is used when your output is an area of data.
    ''' When you don't have a single class that is the correct activation
    ''' but you try to find a result set near to your training area.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class RegressionLayer : Inherits LossLayer

        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.Regression
            End Get
        End Property

        Public Sub New(def As OutputDefinition)
            MyBase.New(def)
        End Sub

        Sub New()
        End Sub

        Public Overrides Function forward(db As DataBlock, training As Boolean) As DataBlock
            in_act = db
            out_act = db ' nothing to do, output raw scores
            Return db
        End Function

        Public Overrides Function backward(y As Double()) As Double()
            ' compute and accumulate gradient wrt weights and bias of this layer
            Dim x = in_act.clearGradient() ' zero out the gradient of input Vol
            Dim loss As Double() = New Double(y.Length - 1) {}

            For i As Integer = 0 To out_depth - 1
                Dim dy = x.getWeight(i) - y(i)

                x.setGradient(i, dy)
                loss(i) = 0.5 * dy * dy
            Next

            Return loss
        End Function

        Public Overrides Function backward(y As Integer) As Double
            ' compute and accumulate gradient wrt weights and bias of this layer
            Dim x = in_act.clearGradient() ' zero out the gradient of input Vol
            Dim loss = 0.0
            ' lets hope that only one number is being regressed
            Dim dy = x.getWeight(0) - y

            x.setGradient(0, dy)
            loss += 0.5 * dy * dy

            Return loss
        End Function

        Public Overrides Function ToString() As String
            Return "regression()"
        End Function
    End Class

End Namespace
