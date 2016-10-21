Namespace LP

    Public Interface Solver

        Function solve(equations As Tableau) As Objective

        Function validate(tableau As Tableau) As Boolean
    End Interface

End Namespace