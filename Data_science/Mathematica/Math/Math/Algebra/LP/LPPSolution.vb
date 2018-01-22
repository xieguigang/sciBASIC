Namespace Algebra.LinearProgramming

    Public Class LPPSolution

        Dim solution() As Double
        Dim variableNames() As String
        Dim slack() As Double
        Dim shadowPrice() As Double
        Dim reducedCost() As Double

        Public ReadOnly Property failureMessage As String
        Public Property SolutionLog As String
        Public Property SolveTime As Long
        Public Property FeasibleSolutionTime As Long
        Public Property ConstraintTypes As String()
        Public Property ObjectiveFunctionValue As Double

        Public Sub New(failureMessage As String, solutionLog As String, feasibleSolutionTime As Long)
            Me.failureMessage = failureMessage
            Me.SolutionLog = solutionLog
            Me.FeasibleSolutionTime = feasibleSolutionTime
        End Sub

        Public Sub New(optimalSolution() As Double, objectiveFunctionValue As Double, variableNames() As String, constraintTypes() As String, slack() As Double, shadowPrice() As Double, reducedCost() As Double, solveTime As Long, feasibleSolutionTime As Long, solutionLog As String)
            Me.solution = optimalSolution
            Me.ObjectiveFunctionValue = objectiveFunctionValue
            Me.variableNames = variableNames
            Me.ConstraintTypes = constraintTypes
            Me.slack = slack
            Me.shadowPrice = shadowPrice
            Me.reducedCost = reducedCost
            Me.SolveTime = solveTime
            Me.FeasibleSolutionTime = feasibleSolutionTime
            Me.SolutionLog = solutionLog
        End Sub

        Public Overrides Function ToString() As String
            If failureMessage IsNot Nothing Then
                Return failureMessage
            End If

            Dim output As String = ""

            ' Optimal Solution
            For i As Integer = 0 To solution.Length - 1
                output += variableNames(i) & " = " & formatDecimals(solution(i)) & vbLf
            Next

            ' Objective Function
            output += ControlChars.Lf & "Objective Function Value = " & formatDecimals(ObjectiveFunctionValue) & ControlChars.Lf & ControlChars.Lf

            Return output
        End Function

        Public Function constraintSensitivityString() As String
            Dim output As String = ""

            For j As Integer = 0 To slack.Length - 1
                ' Double 0.0 or -0.0 condition was here.
                If slack(j) = 0 Then
                    output &= "Constraint " & (j + 1) & " is binding"

                    If shadowPrice(j) > Double.NegativeInfinity Then
                        output &= " with shadow price " & formatDecimals(shadowPrice(j)) & "." & vbLf
                    Else
                        output &= "." & vbLf
                    End If
                ElseIf slack(j).CompareTo(0.0) > 0 Then
                    output &= "Constraint " & (j + 1) & " is non-binding with " & formatDecimals(slack(j)) & " slack." & vbLf
                ElseIf slack(j).CompareTo(0.0) < 0 Then
                    output &= "Constraint " & (j + 1) & " is non-binding with " & formatDecimals(slack(j)) & " surplus." & vbLf
                End If
            Next

            Return output & ControlChars.Lf
        End Function

        Public Function coefficientSensitivityString() As String
            Dim output As String = ""

            For i As Integer = 0 To solution.Length - 1
                output += variableNames(i) & ": " & "Reduced Cost = " & formatDecimals(reducedCost(i)) & ControlChars.Lf
            Next

            Return output & ControlChars.Lf
        End Function
    End Class
End Namespace