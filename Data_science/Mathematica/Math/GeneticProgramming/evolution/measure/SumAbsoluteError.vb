Imports std = System.Math

Namespace evolution.measure
    Public Class SumAbsoluteError
        Implements Objective

        Public Overridable Function getError(expected As Double, real As Double) As Double Implements Objective.getError
            Return std.Abs(expected - real)
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
