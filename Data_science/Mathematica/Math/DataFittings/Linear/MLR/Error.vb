Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports stdNum = System.Math

Namespace Multivariate

    Public Structure [Error] : Implements IFitError

        Public Property X As Vector
        Public Property Y As Double Implements IFitError.Y
        Public Property Yfit As Double Implements IFitError.Yfit

        Public Overrides Function ToString() As String
            Return $"{stdNum.Abs(Y - Yfit)} = |{Y} - {Yfit}|"
        End Function

        Public Shared Iterator Function RunTest(MLR As MLRFit, X As GeneralMatrix, Y As Vector) As IEnumerable(Of [Error])
            For Each xi In X.RowVectors.SeqIterator
                Dim yi = Y.Item(index:=xi)
                Dim yfit = MLR.Fx(xi)

                Yield New [Error] With {
                    .X = xi,
                    .Y = yi,
                    .Yfit = yfit
                }
            Next
        End Function
    End Structure

End Namespace