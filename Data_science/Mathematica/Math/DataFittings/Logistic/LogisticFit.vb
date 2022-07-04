Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class LogisticFit : Implements IFitted

    Public ReadOnly Property R2 As Double Implements IFitted.R2
    Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
    Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

    Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
        Throw New NotImplementedException()
    End Function

    Friend Shared Function CreateFit(log As Logistic, matrix As Instance()) As LogisticFit

    End Function
End Class
