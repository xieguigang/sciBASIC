Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

Namespace CNN.losslayers

    ''' <summary>
    ''' This layer uses the input area trying to find a line to
    ''' separate the correct activation from the incorrect ones.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class SVMLayer : Inherits LossLayer

        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.SVM
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

        Public Overrides Function backward(y As Integer) As Double
            ' compute and accumulate gradient wrt weights and bias of this layer
            Dim x = in_act.clearGradient()
            ' we're using structured loss here, which means that the score
            ' of the ground truth should be higher than the score of any other
            ' class, by a margin
            Dim yscore = x.getWeight(y) ' score of ground truth
            Dim margin = 1.0
            Dim loss = 0.0

            For i As Integer = 0 To out_depth - 1
                If y = i Then
                    Continue For
                End If

                Dim ydiff = -yscore + x.getWeight(i) + margin

                If ydiff > 0 Then
                    ' violating dimension, apply loss
                    x.addGradient(i, 1)
                    x.subGradient(y, 1)

                    loss += ydiff
                End If
            Next
            Return loss
        End Function

        Public Overrides Function ToString() As String
            Return "svm()"
        End Function

        Public Overrides Function backward(y() As Double) As Double()
            Throw New NotSupportedException("svm not supported")
        End Function
    End Class

End Namespace
