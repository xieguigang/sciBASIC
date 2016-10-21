Namespace LP




    Public Class LinearSolver
        Implements Solver


        Private ReadOnly type As OptimizationType

        Public Sub New(ByVal type As OptimizationType)
            Me.type = type
        End Sub

        Public Function solve(ByVal ___tableau As Tableau) As Objective Implements Solver.solve
            If Not validate(___tableau) Then Throw New System.ArgumentException("Tableau is not in proper form")

            ' Check if the problem has a solution
            If ___tableau.Infeasible Then Throw New InfeasibleException("Problem is infeasible.")
            If ___tableau.Unbounded Then Throw New UnboundedException("Problem is unbounded.")

            ' Optimize
            Dim [loop] As Integer = 0
            Dim objectiveFunction As Double() = ___tableau.Matrix(0)
            Do While (Not isOptimal(objectiveFunction)) AndAlso [loop] < 100
                Dim pivotColumn As Integer = ___tableau.PivotColumn ' entering variable
                Dim pivotRow As Integer = ___tableau.getPivotRow(pivotColumn) ' leaving variable
                ___tableau.pivot(pivotRow, pivotColumn)
                [loop] += 1
            Loop

            Return New Objective(___tableau)
        End Function

        Public Function validate(ByVal ___tableau As Tableau) As Boolean Implements Solver.validate
            Return ___tableau.inProperForm()
        End Function

        ''' <summary>
        ''' Returns true if the current solution is optimal by verifying
        ''' if no entering basic variable is available, ie. there are no
        ''' negative values in the objective function 
        ''' </summary>
        Private Function isOptimal(ByVal objective As Double()) As Boolean
            For Each d As Double In objective
                If d < 0 Then Return False
            Next d
            Return True
        End Function



    End Class

End Namespace