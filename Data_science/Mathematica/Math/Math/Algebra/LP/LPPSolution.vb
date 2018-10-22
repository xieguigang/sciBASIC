#Region "Microsoft.VisualBasic::633133b9b46f9bc194690cfac9808574, Data_science\Mathematica\Math\Math\Algebra\LP\LPPSolution.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class LPPSolution
    ' 
    '         Properties: ConstraintTypes, failureMessage, FeasibleSolutionTime, ObjectiveFunctionValue, SolutionLog
    '                     SolveTime
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: coefficientSensitivityString, constraintSensitivityString, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
