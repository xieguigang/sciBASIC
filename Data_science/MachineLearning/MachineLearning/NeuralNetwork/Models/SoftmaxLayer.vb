Imports stdNum = System.Math

Namespace NeuralNetwork

    ''' <summary>
    ''' softmax output layer of the classify result.
    ''' </summary>
    Public Class SoftmaxLayer

        Public Shared Function Softmax(V As Double()) As Double()
            Dim EVj As Double = Aggregate Vj As Double In V Into Sum(stdNum.Exp(Vj))
            Dim smax As Double() = V _
                .Select(Function(Vi) stdNum.Exp(Vi) / EVj) _
                .ToArray

            Return smax
        End Function
    End Class
End Namespace