#Region "Microsoft.VisualBasic::cf33a4c62745db835349f40f06005d24, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\LP\LinearSolver.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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
