#Region "Microsoft.VisualBasic::8d00b240ee94b3e992f6a6c9fe0acbd9, Data_science\Mathematica\Math\Math\Algebra\LP\LPPSolution.vb"

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


    ' Code Statistics:

    '   Total Lines: 135
    '    Code Lines: 108 (80.00%)
    ' Comment Lines: 3 (2.22%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 24 (17.78%)
    '     File Size: 5.55 KB


    '     Class LPPSolution
    ' 
    '         Properties: ConstraintTypes, DecimalFormat, failureMessage, FeasibleSolutionTime, ObjectiveFunctionValue
    '                     SolutionLog, SolveTime
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: coefficientSensitivityString, constraintSensitivityString, GetReducedCost, (+3 Overloads) GetSolution, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

<Assembly: InternalsVisibleTo("Rlapack")>

Namespace LinearAlgebra.LinearProgramming

    Public Class LPPSolution

        Dim solution() As Double
        Dim variableNames() As String
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
            Me.variableNames = variableNames
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
            For i As Integer = 0 To variableNames.Length - 1
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

        Public Function GetReducedCost() As Dictionary(Of String, Double)
            Return variableNames _
                .Select(Function(name, i) (name, i)) _
                .ToDictionary(Function(i) i.name,
                              Function(i)
                                  Return reducedCost(i.i)
                              End Function)
        End Function

        Public Function coefficientSensitivityString() As String
            Dim output As String = ""

            For i As Integer = 0 To solution.Length - 1
                output += variableNames(i) & ": " & "Reduced Cost = " & reducedCost(i).ToString(DecimalFormat) & ControlChars.Lf
            Next

            Return output & ControlChars.Lf
        End Function
    End Class
End Namespace
