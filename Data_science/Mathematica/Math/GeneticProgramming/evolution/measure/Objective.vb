Namespace evolution.measure
    Public Interface Objective

        Function getError(expected As Double, real As Double) As Double

        Function getOverallError(ParamArray errors As Double()) As Double

    End Interface

End Namespace
