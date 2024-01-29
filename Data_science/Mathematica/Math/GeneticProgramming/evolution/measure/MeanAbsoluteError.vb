Namespace evolution.measure
    Public Class MeanAbsoluteError
        Inherits SumSquareError

        Public Overrides Function getOverallError(ParamArray errors As Double()) As Double
            Return MyBase.getOverallError(errors) / errors.Length
        End Function

    End Class

End Namespace
