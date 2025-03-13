Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Framework.Optimization
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module3

    Private Class fitFunction : Inherits OptimizationObject

        Public ab As Double() = New Double() {3, 9}
        Dim loss As New List(Of Double())

        Public Overrides ReadOnly Property GradientDims As Integer
            Get
                Return 2
            End Get
        End Property

        Public Overrides Sub Update(gradient() As Double)
            ab = Subtract.f64_op_subtract_f64(ab, gradient)
        End Sub

        Public Overrides Sub AddLoss(errors() As Double)
            loss.Add(errors)
        End Sub

        Public Overrides Function PartialDerivative(x() As Double, err As Double) As Double()
            Return {
                -2 * err * x(0),    ' a 
                -2 * err * x(0) ^ 2 ' b
            }
        End Function

        Public Overrides Function Predict(x() As Double) As Double
            Dim x0 As Double = x(0)
            Return ab(0) * x0 + ab(1) * x0 ^ 2
        End Function
    End Class

    Sub Main()
        Dim xValues As Double()() = 100000.Sequence.Select(Function(i) New Double() {i}).ToArray
        Dim yValues As Double() = xValues.Select(Function(xi)
                                                     Dim x As Double = xi(0)
                                                     Dim y = -1.257 * x + 6.83 * x ^ 2
                                                     Return y
                                                 End Function).ToArray

        Dim fit_result = SteepestDescentFit(Of fitFunction).SteepestDescent(xValues, yValues,
                                                                            iterations:=1000,
                                                                            learningRate:=0.01,
                                                                            maxNorm:=10000,
                                                                            progress:=True)

        Call Console.WriteLine(fit_result.ab.GetJson)

        Pause()
    End Sub

End Module
