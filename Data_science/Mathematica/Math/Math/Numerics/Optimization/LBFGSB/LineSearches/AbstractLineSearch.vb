Namespace Framework.Optimization.LBFGSB.LineSearches

    Public MustInherit Class AbstractLineSearch

        Protected _fx As Double
        Protected _step As Double
        Protected _dg As Double

        Public Overridable ReadOnly Property fx As Double
            Get
                Return _fx
            End Get
        End Property
        Public Overridable ReadOnly Property [step] As Double
            Get
                Return _step
            End Get
        End Property
        Public Overridable ReadOnly Property dg As Double
            Get
                Return _dg
            End Get
        End Property
    End Class

End Namespace
