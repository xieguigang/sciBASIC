' ============================================================================
' LPPSolution.vb — LP 求解结果类（用户提供，原样保留；仅补 Imports 以便独立编译）
' 语义约定（由 constraintSensitivityString 打印逻辑反推）：
'   slack(i)  = RHS_i − LHS_i：=0 → binding（配 shadowPrice）；>0 → slack；<0 → surplus
'   shadowPrice(i) = ∂目标/∂b_i（原始目标方向；非 binding 时自然为 0）
'   reducedCost(j) = c_j − Σ_i A_ij·shadowPrice_i（原始目标方向）
' 若整合进自带 IsNullOrEmpty/StringEmpty/NamedValue 扩展的框架（如 sciBASIC），
' 删除 CompatHelpers.vb 即可。
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Text

Public Class LPPSolution

    Public solution() As Double
    Dim variableNames As List(Of String)

    Friend slack() As Double
    Friend shadowPrice() As Double
    Friend reducedCost() As Double

    Public ReadOnly Property failureMessage As String

    Public Property SolutionLog As String
    Public Property SolveTime As Long
    Public Property FeasibleSolutionTime As Long
    Public Property ConstraintTypes As String()
    Public Property ObjectiveFunctionValue As Double
    Public Property DecimalFormat As String = "G5"

    Public ReadOnly Property SolverError As Boolean
        Get
            Return (variableNames Is Nothing OrElse variableNames.Count = 0) AndAlso Not failureMessage.StringEmpty(, True)
        End Get
    End Property

    Public Sub New(failureMessage As String, solutionLog As String, feasibleSolutionTime As Long)
        Me.failureMessage = failureMessage
        Me.SolutionLog = solutionLog
        Me.FeasibleSolutionTime = feasibleSolutionTime
    End Sub

    Public Sub New(optimalSolution() As Double,
                   objectiveFunctionValue As Double,
                   variableNames As String(),
                   ConstraintTypes() As String,
                   slack() As Double,
                   shadowPrice() As Double,
                   reducedCost() As Double,
                   SolveTime As Long,
                   FeasibleSolutionTime As Long,
                   SolutionLog As String,
                   DecimalFormat As String)

        Me.solution = optimalSolution
        Me.ObjectiveFunctionValue = objectiveFunctionValue
        Me.variableNames = New List(Of String)(variableNames)
        Me.ConstraintTypes = ConstraintTypes
        Me.slack = slack
        Me.shadowPrice = shadowPrice
        Me.reducedCost = reducedCost
        Me.SolveTime = SolveTime
        Me.FeasibleSolutionTime = FeasibleSolutionTime
        Me.SolutionLog = SolutionLog
        Me.DecimalFormat = DecimalFormat
    End Sub

    Public Function GetSolution(name As String) As Double
        Return solution(variableNames.IndexOf(name))
    End Function

    Public Iterator Function GetSolution(names As String()) As IEnumerable(Of Double)
        For Each name As String In names
            Yield solution(variableNames.IndexOf(name))
        Next
    End Function

    Public Iterator Function GetSolution() As IEnumerable(Of NamedValue(Of Double))
        For i As Integer = 0 To variableNames.Count - 1
            Yield New NamedValue(Of Double)(variableNames(i), solution(i))
        Next
    End Function

    Public Overrides Function ToString() As String
        If Not failureMessage.StringEmpty Then
            Return failureMessage
        Else
            Dim output As New StringBuilder

            ' Optimal Solution
            For i As Integer = 0 To solution.Length - 1
                output.AppendLine(variableNames(i) & " = " & solution(i).ToString(DecimalFormat))
            Next

            ' Objective Function
            output.AppendLine(ControlChars.Lf & "Objective Function Value = " & ObjectiveFunctionValue.ToString(DecimalFormat))
            output.AppendLine(constraintSensitivityString)
            output.AppendLine(coefficientSensitivityString)

            Return output.ToString
        End If
    End Function

    Public Function constraintSensitivityString() As String
        Dim output As String = ""

        For j As Integer = 0 To slack.Length - 1
            ' Double 0.0 or -0.0 condition was here.
            If slack(j) = 0 Then
                output &= "Constraint " & (j + 1) & " is binding"

                If shadowPrice(j) > Double.NegativeInfinity Then
                    output &= " with shadow price " & shadowPrice(j).ToString(DecimalFormat) & "." & vbLf
                Else
                    output &= "." & vbLf
                End If
            ElseIf slack(j).CompareTo(0.0) > 0 Then
                output &= "Constraint " & (j + 1) & " is non-binding with " & slack(j).ToString(DecimalFormat) & " slack." & vbLf
            ElseIf slack(j).CompareTo(0.0) < 0 Then
                output &= "Constraint " & (j + 1) & " is non-binding with " & slack(j).ToString(DecimalFormat) & " surplus." & vbLf
            End If
        Next

        Return output & ControlChars.Lf
    End Function

    Public Function coefficientSensitivityString() As String
        Dim output As String = ""

        For i As Integer = 0 To solution.Length - 1
            output += variableNames(i) & ": " & "Reduced Cost = " & reducedCost(i).ToString(DecimalFormat) & ControlChars.Lf
        Next

        Return output & ControlChars.Lf
    End Function
End Class
