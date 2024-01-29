Namespace evolution.measure
    Public Class SumSquareError
        Implements Objective

        Public Overridable Function getError(expected As Double, real As Double) As Double Implements Objective.getError
            Dim e = expected - real
            Return e * e
        End Function

        Public Overridable Function getOverallError(ParamArray errors As Double()) As Double Implements Objective.getOverallError
            Dim sum = 0.0
            For Each [error] In errors
                sum += [error]
            Next
            Return sum
        End Function

    End Class

End Namespace
