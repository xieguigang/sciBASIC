Namespace LP


    Public Interface Solver

        Function solve(ByVal equations As Tableau) As Objective

        Function validate(ByVal tableau As Tableau) As Boolean
    End Interface

End Namespace