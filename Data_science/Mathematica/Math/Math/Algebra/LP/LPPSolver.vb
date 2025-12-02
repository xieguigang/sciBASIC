#Region "Microsoft.VisualBasic::ea31524756bca8aa776f8a8f83b11efe, Data_science\Mathematica\Math\Math\Algebra\LP\LPPSolver.vb"

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

    '   Total Lines: 403
    '    Code Lines: 259 (64.27%)
    ' Comment Lines: 65 (16.13%)
    '    - Xml Docs: 18.46%
    ' 
    '   Blank Lines: 79 (19.60%)
    '     File Size: 17.28 KB


    '     Class LPPSolver
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: choosePivotConstraint, choosePivotVar, findInitialBasicVariables, isFeasible, runIteration
    '                   Solve
    ' 
    '         Sub: pivot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar

Namespace LinearAlgebra.LinearProgramming

    ''' <summary>
    ''' 使用单纯形法进行线性规划问题的求解
    ''' </summary>
    Friend Class LPPSolver

        ReadOnly lpp As LPP

        Sub New(problem As LPP)
            lpp = problem
        End Sub

        Public Function Solve(Optional showProgress As Boolean = True) As LPPSolution
            ' Initialize Variables
            ' Point badness if we are going to be incrementing this later?
            Dim solutionLog As New StringBuilder
            Dim varNum As Integer = lpp.variableNames.Count

            Call lpp.makeStandardForm()

            Dim artificialVariables As List(Of Integer) = lpp.ArtificialVariableAssignments

            ' ArrayList<String> varNames = Input.VariableNames;
            ' String LaTeXString = latex.LPPtoLaTeX.displayLPP(Input)+'\n';
            If showProgress Then
                Call Console.WriteLine("Make Standard Form...")
            End If

            Call solutionLog.AppendLine("Make Standard Form")
            Call lpp.makeStandardForm(artificialVariables)

            Dim startTime As Long = App.ElapsedMilliseconds

            If showProgress Then
                Call Console.WriteLine("Add artificial variables to the LPP...")
            End If

            ' Add artificial variables to the LPP
            Call lpp.addArtificialVariables(artificialVariables)

            If showProgress Then
                Call Console.WriteLine("Search for Basic Feasible Solution...")
            End If

            ' Search for Basic Feasible Solution
            Dim basicVariables As List(Of Integer) = findInitialBasicVariables(artificialVariables)
            Dim feasibleSolutionTime As Long = App.ElapsedMilliseconds - startTime

            ' Return fail message if no feasible solution is found
            If basicVariables(0) = -1 Then
                Return New LPPSolution("Could not find a Basic Feasible Solution.", solutionLog.ToString, feasibleSolutionTime)
            End If

            solutionLog.AppendLine("Basic Variables: " & String.Join(", ", basicVariables.ToArray))

            Dim [error] As LPPSolution = runIteration(
                basicVariables, artificialVariables,
                solutionLog,
                feasibleSolutionTime,
                showProgress
            )

            If Not [error] Is Nothing Then
                Return [error]
            End If

            ' Close LaTeX and initialize sensitivity variables
            ' LaTeXString += latex.LPPtoLaTeX.endTableaus();
            Dim optimalSolution(lpp.variableNames.Count - 1) As Double
            Dim reducedCost(lpp.variableNames.Count - 1) As Double
            Dim shadowPrice(lpp.constraintTypes.Length - 1) As Double
            Dim slack(lpp.constraintTypes.Length - 1) As Double

            ' Collect optimal solution and reduced cost from final tableau
            For i As Integer = 0 To lpp.variableNames.Count - 1
                If basicVariables.Contains(i) Then
                    Dim basicVariableIndex As Integer = basicVariables.IndexOf(i)

                    ' Check for redundant constraints
                    If basicVariables.IndexOf(i) <> basicVariables.LastIndexOf(i) Then
                        For k As Integer = 0 To basicVariables.Count - 1
                            If basicVariables(k) = i Then
                                Dim constraint As List(Of Double) = lpp.constraintCoefficients(basicVariableIndex)

                                For m As Integer = 0 To constraint.Count - 1
                                    If constraint(m) <> 0.0 Then
                                        basicVariableIndex = k
                                        Exit For
                                    End If
                                Next
                            End If
                        Next
                    End If

                    ' Set values
                    optimalSolution(i) = lpp.constraintRightHandSides(basicVariableIndex)
                    reducedCost(i) = lpp.objectiveFunctionCoefficients(i)
                Else
                    optimalSolution(i) = 0
                    reducedCost(i) = lpp.objectiveFunctionCoefficients(i)
                End If
            Next

            ' Collect constraint sensitivity analysis data from final tableau
            For j As Integer = 0 To lpp.constraintTypes.Length - 1
                If lpp.constraintTypes(j) = "=" Then
                    slack(j) = 0
                    shadowPrice(j) = -1 * lpp.objectiveFunctionCoefficients(artificialVariables(j))
                    ' This had the double 0.0 or -0.0 check.
                ElseIf lpp.objectiveFunctionCoefficients(varNum) = 0.0 Then
                    slack(j) = lpp.constraintRightHandSides(j)
                    shadowPrice(j) = 0
                    varNum += 1
                Else
                    slack(j) = 0
                    shadowPrice(j) = -1 * lpp.objectiveFunctionCoefficients(varNum)
                    varNum += 1
                End If
            Next

            ' TODO: Undo makeStandardForm ... convert back or accept mutability of LPP?
            ' TODO: Dependency - print artificial variables?
            ' TODO: Create new LPPSolution objects during 

            ' Return the compiled solution.
            Return New LPPSolution(
                optimalSolution, lpp.objectiveFunctionValue, lpp.variableNames.ToArray, lpp.constraintTypes,
                slack,
                shadowPrice,
                reducedCost,
                App.ElapsedMilliseconds - startTime,
                feasibleSolutionTime,
                solutionLog.ToString, LPP.DecimalFormat
            )
        End Function

        Private Function runIteration(basicVariables As List(Of Integer),
                                      artificialVariables As List(Of Integer),
                                      solutionLog As StringBuilder,
                                      feasibleSolutionTime&,
                                      showProgress As Boolean) As LPPSolution

            ' Pivot until optimal solution
            Dim go As Boolean = True
            Dim limiter As Integer = 0

            Call "Run LPP Solution Iterations...".info

            'LaTeXString += latex.LPPtoLaTeX.beginTableaus(Input);

            'Create the ProgressBar
            ' Maximum: The Max value in ProgressBar (Default is 100)
            Using progBar = New ProgressBar() With {.Maximum = Nothing}
                Do While go
                    ' Get next variable to pivot on
                    Dim n As Integer = choosePivotVar(artificialVariables)
                    Dim [next] As Integer = choosePivotConstraint(n)

                    ' If optimal solution reached, end 'while' statement
                    'LaTeXString += latex.LPPtoLaTeX.makeTableau(Input, BasicVars, limiter);
                    limiter += 1

                    If n = -1 Then
                        ' Found a solution.  Stop pivoting.
                        go = False

                    ElseIf limiter = LPP.PIVOT_ITERATION_LIMIT Then
                        Call "Max iteration reached...".warning
                        ' Check iteration limit not exceeded.
                        Return New LPPSolution("The pivot max iteration cap was exceeded.", solutionLog.ToString, feasibleSolutionTime)
                    ElseIf [next] = -1 Then
                        Call "LPP is unbounded!".warning
                        ' Check for unboundedness.
                        Return New LPPSolution("The given LPP is unbounded.", solutionLog.ToString, feasibleSolutionTime)

                    Else
                        ' Get pivot constraint, continue.
                        Call pivot(n, [next])

                        basicVariables([next]) = n
                        solutionLog.AppendLine("Pivot at " & n & ", " & [next])
                    End If

                    Call progBar.PerformStep()
                Loop
            End Using

            Return Nothing
        End Function

        ''' <summary>
        ''' 判断当前的这个线性规划问题是否是可解的？ 
        ''' </summary>
        ''' <param name="possibleSolution"></param>
        ''' <returns></returns>
        Private Function isFeasible(possibleSolution As List(Of Integer)) As Boolean
            For j As Integer = 0 To lpp.constraintRightHandSides.Length - 1
                pivot(possibleSolution(j), j)
            Next

            For j As Integer = 0 To lpp.constraintRightHandSides.Length - 1
                Dim constraint As List(Of Double) = lpp.constraintCoefficients(j)

                ' Check all basic variables are non-negative
                If lpp.constraintRightHandSides(j) < 0 Then
                    Return False
                End If

                ' Ensure there are no unequal constraints
                Dim q As Double = 0
                For i As Integer = 0 To constraint.Count - 1
                    If possibleSolution.Contains(i) Then
                        q = q + constraint(i) * lpp.constraintRightHandSides(possibleSolution.IndexOf(i))
                    End If
                Next
                If q <> lpp.constraintRightHandSides(j) Then
                    Return False
                End If
            Next
            Return True
        End Function

        ' TODO: Review
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="varIndex"></param>
        ''' <param name="constIndex"></param>
        Public Sub pivot(varIndex As Integer, constIndex As Integer)
            Dim pivotConstraint As List(Of Double) = lpp.constraintCoefficients(constIndex)

            If pivotConstraint(varIndex) = 0.0 Then
                Return
            End If

            Dim pivotConstraintRHS As Double = lpp.constraintRightHandSides(constIndex)
            ' Divide the pivot constraint through by the pivot variable coefficient
            Dim pivotVarCoeff As Double = pivotConstraint(varIndex)
            Dim coeff As Double

            For i As Integer = 0 To pivotConstraint.Count - 1
                coeff = pivotConstraint(i)
                pivotConstraint(i) = coeff / pivotVarCoeff
            Next

            pivotConstraintRHS = pivotConstraintRHS / pivotVarCoeff

            lpp.constraintCoefficients(constIndex) = pivotConstraint
            lpp.constraintRightHandSides(constIndex) = pivotConstraintRHS

            ' eliminate the pivot variable from the other constraints
            For j As Integer = 0 To lpp.constraintCoefficients.Length - 1

                ' check constraint j != pivot constraint
                If j <> constIndex Then

                    ' make constraint local variables
                    Dim constraint As List(Of Double) = lpp.constraintCoefficients(j)
                    Dim constraintRHS As Double = lpp.constraintRightHandSides(j)

                    ' check the coefficient of the pivot variable in the non-pivot constraint != 0
                    If constraint(varIndex) <> 0 Then
                        Dim constraintPivotVarCoeff As Double = constraint(varIndex)

                        ' perform Elimination variable by variable
                        For i As Integer = 0 To constraint.Count - 1
                            constraint(i) = constraint(i) - (pivotConstraint(i) * constraintPivotVarCoeff)
                        Next

                        ' write new constraint to LPP
                        constraintRHS = (constraintRHS - (pivotConstraintRHS * constraintPivotVarCoeff))

                        lpp.constraintCoefficients(j) = constraint
                        lpp.constraintRightHandSides(j) = constraintRHS
                    End If
                End If
            Next

            ' substitute pivot variable into the objective function.
            pivotVarCoeff = lpp.objectiveFunctionCoefficients(varIndex)
            pivotConstraint = lpp.constraintCoefficients(constIndex)
            pivotConstraintRHS = lpp.constraintRightHandSides(constIndex)

            lpp.objectiveFunctionCoefficients(varIndex) = 0.0
            lpp.objectiveFunctionValue = lpp.objectiveFunctionValue + (pivotVarCoeff * pivotConstraintRHS)

            For i As Integer = 0 To lpp.objectiveFunctionCoefficients.Count - 1
                If i <> varIndex Then
                    lpp.objectiveFunctionCoefficients(i) = lpp.objectiveFunctionCoefficients(i) + ((-1) * pivotConstraint(i) * pivotVarCoeff)
                End If
            Next
        End Sub

        Private Function findInitialBasicVariables(artificialVariables As List(Of Integer)) As List(Of Integer)
            ' Declare Basic Variable array, boolean to indicate if a feasible solution has been found
            Dim alpha As New List(Of Integer)()
            Dim foundBasicFeasSol As Boolean = False

            ' Determine the number of regular variables
            Dim q As Integer = 0

            For j As Integer = 0 To artificialVariables.Count - 1
                If artificialVariables(j) <> -1 Then
                    q += 1
                End If
            Next

            ' Set up parameters for finding subsets
            Dim n As Integer = lpp.variableNames.Count - q
            Dim powerSetSize As Long = CLng(Fix(2 ^ n))

            For i As Long = powerSetSize To 0 Step -1
                '  Convert the binary number to a string containing n digits
                Dim binary As List(Of Byte) = intToBinary(i, n)

                If binary.Count < lpp.constraintRightHandSides.Length Then
                    Continue For
                End If

                ' Reinitialize potential basic feasible solution
                alpha.Clear()

                '  Create the corresponding subset
                For j As Integer = 0 To binary.Count - 1
                    If binary(j) = 1 Then
                        alpha.Add(j)
                    End If
                Next

                ' Check to see if the basic variable set alpha is feasible
                If alpha.Count = lpp.constraintRightHandSides.Length Then
                    If isFeasible(alpha) Then
                        foundBasicFeasSol = True
                        Exit For
                    End If
                End If
            Next

            '  No feasible solution is found, create dummy solution vector.
            If Not foundBasicFeasSol Then
                alpha = New List(Of Integer)()

                For j As Integer = 0 To lpp.constraintRightHandSides.Length - 1
                    alpha.Add(-1)
                Next
            End If

            Return alpha
        End Function

        Private Function choosePivotVar(artificialVariables As List(Of Integer)) As Integer
            Dim q As Double = 0
            Dim choice As Integer = -1
            Dim maxormin As Integer = If(lpp.objectiveFunctionType = OptimizationType.MAX, -1, 1)
            Dim coefficientTerm As Double

            For i As Integer = 0 To lpp.objectiveFunctionCoefficients.Count - 1
                coefficientTerm = maxormin * lpp.objectiveFunctionCoefficients(i)

                If Not artificialVariables.Contains(i) Then
                    If coefficientTerm < q Then
                        q = coefficientTerm
                        choice = i
                    End If
                End If
            Next

            Return choice
        End Function

        Private Function choosePivotConstraint(n As Integer) As Integer
            ' Short-circuit the procedure if choosePivotVar gives -1.
            If n = -1 Then
                Return 0
            End If

            ' Initialize variables
            Dim q As Double? = Double.PositiveInfinity
            Dim choice As Integer = -1

            ' Run down the column for the given variable, compare ratios of coefficient/RHS
            For j As Integer = 0 To lpp.constraintRightHandSides.Length - 1
                Dim constraint As List(Of Double) = lpp.constraintCoefficients(j)
                If constraint(n) > 0 Then
                    Dim ratio As Double = lpp.constraintRightHandSides(j) / constraint(n)

                    ' q holds the lowest ratio, if a lowest ratio is found, our choice is changed to corresponding constraint index
                    If j = 0 OrElse ratio.CompareTo(q) < 0 Then
                        q = ratio
                        choice = j
                    End If
                End If
            Next

            Return choice
        End Function
    End Class
End Namespace
