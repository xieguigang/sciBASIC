Imports Microsoft.VisualBasic.Language

Namespace LP

    Public Class LinearSolver

        Public Property Loops As Integer = 100

        ReadOnly type As OptimizationType

        Public Sub New(type As OptimizationType)
            Me.type = type
        End Sub

        Public Function solve(___tableau As Tableau) As Objective
            If Not ___tableau.inProperForm Then
                Throw New ArgumentException("Tableau is not in proper form")
            End If

            ' Check if the problem has a solution
            If ___tableau.Infeasible Then
                Throw New InfeasibleException("Problem is infeasible.")
            End If
            If ___tableau.Unbounded Then
                Throw New UnboundedException("Problem is unbounded.")
            End If

            ' Optimize
            Dim [loop] As int = 0
            Dim objectiveFunction As Double() = ___tableau.Matrix(0)

            Do While (Not isOptimal(objectiveFunction)) AndAlso ++[loop] < Loops
                Dim pivotColumn% = ___tableau.PivotColumn           ' entering variable
                Dim pivotRow% = ___tableau.getPivotRow(pivotColumn) ' leaving variable

                Call ___tableau.pivot(pivotRow, pivotColumn)
            Loop

            Return New Objective(___tableau)
        End Function

        ''' <summary>
        ''' Returns true if the current solution is optimal by verifying
        ''' if no entering basic variable is available, ie. there are no
        ''' negative values in the objective function 
        ''' </summary>
        Private Function isOptimal(objective As Double()) As Boolean
            For Each d As Double In objective
                If d < 0 Then Return False
            Next
            Return True
        End Function
    End Class
End Namespace