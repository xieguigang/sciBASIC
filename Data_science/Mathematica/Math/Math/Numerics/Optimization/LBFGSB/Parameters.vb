Namespace Framework.Optimization.LBFGSB

    Public NotInheritable Class Parameters

        Public m As Integer = 6
        Public epsilon As Double = 0.0000001
        Public epsilon_rel As Double = 0.0000001
        Public past As Integer = 3
        Public delta As Double = 0.0000000001
        Public max_iterations As Integer = 1000
        Public max_submin As Integer = 10
        Public max_linesearch As Integer = 20
        Public linesearch As LINESEARCH = LINESEARCH.MORETHUENTE_ORIG
        Public xtol As Double = 0.00000001 ' MoreThuente
        Public min_step As Double = 1.0E-20
        Public max_step As Double = 1.0E+20
        Public ftol As Double = 0.0001
        Public wolfe As Double = 0.9
        Public weak_wolfe As Boolean = True

    End Class

    Public Enum LINESEARCH
        MORETHUENTE_LBFGSPP
        MORETHUENTE_ORIG
        LEWISOVERTON
    End Enum

End Namespace
