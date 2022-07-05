Imports Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class LogisticFit : Implements IFitted

    Public ReadOnly Property R2 As Double Implements IFitted.R2
        Get
            Return 1
        End Get
    End Property

    Public Property Polynomial As Formula Implements IFitted.Polynomial
    Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

    Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
        Dim logit As Double = Polynomial _
            .Factors _
            .Select(Function(wi, i) wi * x(i)) _
            .Sum
        Dim log As Double = Logistic.sigmoid(logit)

        Return log
    End Function

    Friend Shared Function CreateFit(log As Logistic, matrix As Instance()) As LogisticFit
        Dim weights As New Polynomial With {.Factors = log.theta.ToArray}
        Dim test As IFitError() = matrix _
            .Select(Function(i)
                        Return New [Error] With {
                            .X = i.x.AsVector,
                            .Y = i.label,
                            .Yfit = log.predict(i.x)
                        }
                    End Function) _
            .Select(Function(pi) DirectCast(pi, IFitError)) _
            .ToArray

        Return New LogisticFit With {
            .ErrorTest = test,
            .Polynomial = weights
        }
    End Function
End Class
