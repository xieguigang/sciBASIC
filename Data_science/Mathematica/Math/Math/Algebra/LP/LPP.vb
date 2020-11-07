#Region "Microsoft.VisualBasic::7bbc0986bf76b0bd83085ecba9c3e2b2, Data_science\Mathematica\Math\Math\Algebra\LP\LPP.vb"

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

    '     Class LPP
    ' 
    '         Properties: ArtificialVariableAssignments, DecimalFormat, ObjectFunctionVariables, PIVOT_ITERATION_LIMIT, USE_SUBSCRIPT_UNICODE
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: choosePivotConstraint, choosePivotVar, displayEqLine, findInitialBasicVariables, increaseArtificialVariableIndices
    '                   isFeasible, runIteration, solve, ToString
    ' 
    '         Sub: addArtificialVariables, addVariableAt, (+2 Overloads) makeStandardForm, pivot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports stdNum = System.Math

Namespace LinearAlgebra.LinearProgramming

    ''' <summary>
    ''' Linear programming solver from: 
    ''' 
    ''' https://github.com/gthole/lpp
    ''' </summary>
    Public Class LPP

        Dim objectiveFunctionType As OptimizationType
        ''' <summary>
        ''' 这个变量名称列表之中会添加拓展的新的变量名称
        ''' 
        ''' 可以使用objectfunction的系数长度来取出原来的输入的变量名称的列表
        ''' </summary>
        Dim variableNames As List(Of String)
        Dim objectiveFunctionCoefficients As List(Of Double)
        Dim constraintCoefficients() As List(Of Double)
        Dim constraintTypes() As String
        Dim constraintRightHandSides() As Double
        Dim objectiveFunctionValue As Double

        Public Shared Property PIVOT_ITERATION_LIMIT As Integer = 1000
        Public Shared Property USE_SUBSCRIPT_UNICODE As Boolean = False

        Public Shared Property DecimalFormat As String = "G5"

        Public ReadOnly Property ObjectFunctionVariables As String()
            Get
                Return variableNames.Take(objectiveFunctionCoefficients.Count).ToArray
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="opt">目标函数的类型，是求取极大值还是极小值</param>
        ''' <param name="variableNames">方程之中的未知变量的名称，可以省略这个函数，程序会默认会自动使用x1, x2, x3...等来自动命名</param>
        ''' <param name="objectiveFunctionCoefficients">目标函数之中每一个未知变量所对应的系数</param>
        ''' <param name="constraintCoefficients">方程组的左边：系数矩阵</param>
        ''' <param name="constraintTypes">方程组之中的函数类型：大于，小于，等于</param>
        ''' <param name="constraintRightHandSides">方程组的右边：方程组之中每一个方程的结果值</param>
        ''' <param name="objectiveFunctionValue">目标方程的目标结果值</param>
        Sub New(opt As OptimizationType,
                variableNames$(),
                objectiveFunctionCoefficients#(),
                constraintCoefficients#(,),
                constraintTypes$(),
                constraintRightHandSides#(),
                Optional objectiveFunctionValue# = 0)

            Call Me.New(opt.Description, variableNames, objectiveFunctionCoefficients, constraintCoefficients.ToVectorList, constraintTypes, constraintRightHandSides, objectiveFunctionValue)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="objectiveFunctionType$">目标函数的类型，是求取极大值还是极小值</param>
        ''' <param name="variableNames">方程之中的未知变量的名称，可以省略这个函数，程序会默认会自动使用x1, x2, x3...等来自动命名</param>
        ''' <param name="objectiveFunctionCoefficients">目标函数之中每一个未知变量所对应的系数</param>
        ''' <param name="constraintCoefficients">方程组的左边：系数矩阵</param>
        ''' <param name="constraintTypes">方程组之中的函数类型：大于，小于，等于</param>
        ''' <param name="constraintRightHandSides">方程组的右边：方程组之中每一个方程的结果值</param>
        ''' <param name="objectiveFunctionValue">目标方程的目标结果值</param>
        Public Sub New(objectiveFunctionType$,
                       variableNames() As String,
                       objectiveFunctionCoefficients() As Double,
                       constraintCoefficients()() As Double,
                       constraintTypes() As String,
                       constraintRightHandSides() As Double,
                       objectiveFunctionValue As Double)

            ' Create default variable name array
            If variableNames Is Nothing OrElse variableNames.Length = 0 Then
                variableNames = New String(objectiveFunctionCoefficients.Length - 1) {}

                For i As Integer = 0 To variableNames.Length - 1
                    variableNames(i) = "x" & subscriptN(i)
                Next
            End If

            ' Validation
            If constraintTypes.Length <> constraintRightHandSides.Length OrElse constraintRightHandSides.Length <> constraintCoefficients.Length Then
                Throw New Exception("LPP constraints do not appear well-formed.")
            End If
            If variableNames.Length <> objectiveFunctionCoefficients.Length Then
                Throw New Exception("LPP objective function does not appear well-formed.")
            End If
            For i As Integer = 0 To constraintCoefficients.Length - 1
                If constraintCoefficients(i).Length <> variableNames.Length Then
                    Throw New Exception($"LPP constraint {i} is not of the same size length as the objective function.")
                End If
            Next

            Me.objectiveFunctionType = objectiveFunctionType.ParseType
            Me.variableNames = variableNames.ToList
            Me.objectiveFunctionCoefficients = objectiveFunctionCoefficients.ToList
            Me.constraintCoefficients = constraintCoefficients _
                .Select(Function(v) v.ToList) _
                .ToArray
            Me.constraintTypes = constraintTypes
            Me.constraintRightHandSides = constraintRightHandSides
            Me.objectiveFunctionValue = objectiveFunctionValue
        End Sub

        Public Overrides Function ToString() As String
            Dim output As String = objectiveFunctionType.Description

            output = output & "  " & displayEqLine(objectiveFunctionCoefficients.ToArray, variableNames)
            output = output & ControlChars.Lf & "subject to the constraints:" & ControlChars.Lf

            For j As Integer = 0 To constraintRightHandSides.Length - 1
                Dim constraint() As Double = constraintCoefficients(j).ToArray
                output += displayEqLine(constraint, variableNames)
                output &= " " & constraintTypes(j)
                output &= " " & constraintRightHandSides(j).ToString(DecimalFormat)
                output += ControlChars.Lf
            Next

            Return output & ControlChars.Lf
        End Function

        Private Shared Function displayEqLine(coefficients() As Double, variableNames As List(Of String)) As String
            Dim output As String = ""

            Dim startIndex As Integer = 1
            For i As Integer = 0 To variableNames.Count - 1
                If coefficients(i) <> 0 Then
                    output = output + coefficients(i).ToString(DecimalFormat) + variableNames(i)
                    Exit For
                Else
                    startIndex += 1
                End If
            Next

            For i As Integer = startIndex To variableNames.Count - 1
                Dim signString As String = " + "
                Dim sign As Double = 1.0

                If coefficients(i) < 0.0 Then
                    signString = " - "
                    sign = -1
                End If
                If coefficients(i) <> 0 Then
                    output = output + signString + (sign * coefficients(i)).ToString(DecimalFormat) + variableNames(i)
                End If
            Next

            Return output
        End Function

        ''' <summary>
        ''' Change Signs to = by adding variables
        ''' </summary>
        Public Sub makeStandardForm()
            For i As Integer = 0 To constraintTypes.Length - 1
                If constraintTypes(i) <> "=" Then
                    addVariableAt(i, If(constraintTypes(i) = "³", -1, 1))
                    constraintTypes(i) = "="
                End If
            Next
        End Sub

        ''' <summary>
        ''' Change signs to = by adding variables
        ''' </summary>
        ''' <param name="artificialVariables"></param>
        Private Sub makeStandardForm(artificialVariables As List(Of Integer))
            For i As Integer = 0 To constraintTypes.Length - 1
                If constraintTypes(i) <> "=" Then
                    addVariableAt(i, If(constraintTypes(i) = "³", -1, 1))
                    constraintTypes(i) = "="
                    artificialVariables = increaseArtificialVariableIndices(artificialVariables)
                End If
            Next
        End Sub

        Private Shared Function increaseArtificialVariableIndices(artificialVariables As List(Of Integer)) As List(Of Integer)
            For Each artificialVariable As Integer In artificialVariables
                If artificialVariable <> -1 Then
                    artificialVariable += 1
                End If
            Next

            Return artificialVariables
        End Function

        Private ReadOnly Property ArtificialVariableAssignments As List(Of Integer)
            Get
                Dim assignments As New List(Of Integer)()
                Dim k As Integer = 0

                For j As Integer = 0 To constraintTypes.Length - 1
                    If constraintTypes(j) = "=" Then
                        assignments.Add(objectiveFunctionCoefficients.Count + k)
                        k += 1
                    Else
                        assignments.Add(-1)
                    End If
                Next

                Return assignments
            End Get
        End Property

        Private Sub addArtificialVariables(artificialVariables As List(Of Integer))
            For j As Integer = 0 To constraintTypes.Length - 1
                If artificialVariables(j) <> -1 Then
                    Me.addVariableAt(j, 1)
                End If
            Next
        End Sub

        ' TODO: Review
        Public Sub pivot(varIndex As Integer, constIndex As Integer)
            Dim pivotConstraint As List(Of Double) = constraintCoefficients(constIndex)
            Dim pivotConstraintRHS As Double = constraintRightHandSides(constIndex)

            If pivotConstraint(varIndex) <> 0 Then

                'Divide the pivot constraint through by the pivot variable coefficient
                Dim pivotVarCoeff As Double = pivotConstraint(varIndex)
                For i As Integer = 0 To pivotConstraint.Count - 1
                    Dim coeff As Double = pivotConstraint(i)
                    pivotConstraint(i) = coeff / pivotVarCoeff
                Next
                pivotConstraintRHS = (pivotConstraintRHS / pivotVarCoeff)
                constraintCoefficients(constIndex) = pivotConstraint
                constraintRightHandSides(constIndex) = pivotConstraintRHS

                ' eliminate the pivot variable from the other constraints
                For j As Integer = 0 To constraintCoefficients.Length - 1

                    ' check constraint j != pivot constraint
                    If j <> constIndex Then

                        ' make constraint local variables
                        Dim constraint As List(Of Double) = constraintCoefficients(j)
                        Dim constraintRHS As Double = constraintRightHandSides(j)

                        ' check the coefficient of the pivot variable in the non-pivot constraint != 0
                        If constraint(varIndex) <> 0 Then
                            Dim constraintPivotVarCoeff As Double = constraint(varIndex)

                            ' perform Elimination variable by variable
                            For i As Integer = 0 To constraint.Count - 1
                                constraint(i) = constraint(i) - (pivotConstraint(i) * constraintPivotVarCoeff)
                            Next

                            ' write new constraint to LPP
                            constraintRHS = (constraintRHS - (pivotConstraintRHS * constraintPivotVarCoeff))
                            constraintCoefficients(j) = constraint
                            constraintRightHandSides(j) = constraintRHS
                        End If
                    End If
                Next

                ' substitute pivot variable into the objective function.
                pivotVarCoeff = objectiveFunctionCoefficients(varIndex)
                pivotConstraint = constraintCoefficients(constIndex)
                pivotConstraintRHS = constraintRightHandSides(constIndex)

                objectiveFunctionCoefficients(varIndex) = 0.0
                objectiveFunctionValue = objectiveFunctionValue + (pivotVarCoeff * pivotConstraintRHS)

                For i As Integer = 0 To objectiveFunctionCoefficients.Count - 1
                    If i <> varIndex Then
                        objectiveFunctionCoefficients(i) = objectiveFunctionCoefficients(i) + ((-1) * pivotConstraint(i) * pivotVarCoeff)
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' Unfortunate copy and pasting going on here.
        ''' </summary>
        ''' <param name="constraintIndex"></param>
        ''' <param name="value"></param>
        Private Sub addVariableAt(constraintIndex As Integer, value As Double)
            variableNames.Add("v" & subscriptN(variableNames.Count + 1))
            objectiveFunctionCoefficients.Add(0)

            For j As Integer = 0 To constraintCoefficients.Length - 1
                constraintCoefficients(j).Add(If(j <> constraintIndex, 0, value))
            Next
        End Sub

        Private Shared Function isFeasible(lpp As LPP, possibleSolution As List(Of Integer)) As Boolean
            For j As Integer = 0 To lpp.constraintRightHandSides.Length - 1
                lpp.pivot(possibleSolution(j), j)
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
            Dim n As Integer = variableNames.Count - q
            Dim powerSetSize As Integer = CInt(Fix(stdNum.Pow(2, n)))

            For i As Integer = 0 To powerSetSize - 1

                ' Reinitialize potential basic feasible solution
                alpha = New List(Of Integer)()

                '  Convert the binary number to a string containing n digits
                Dim binary As List(Of Char) = intToBinary(i, n)

                '  Create the corresponding subset
                For j As Integer = 0 To binary.Count - 1
                    If binary(j) = "1"c Then
                        alpha.Add(j)
                    End If
                Next

                ' Check to see if the basic variable set alpha is feasible
                If alpha.Count = constraintRightHandSides.Length AndAlso isFeasible(Me, alpha) Then
                    foundBasicFeasSol = True
                    Exit For
                End If
            Next

            '  No feasible solution is found, create dummy solution vector.
            If Not foundBasicFeasSol Then
                alpha = New List(Of Integer)()
                For j As Integer = 0 To constraintRightHandSides.Length - 1
                    alpha.Add(-1)
                Next
            End If

            Return alpha
        End Function

        Private Function choosePivotVar(artificialVariables As List(Of Integer)) As Integer
            Dim q As Double = 0
            Dim choice As Integer = -1
            Dim maxormin As Integer = If(objectiveFunctionType = OptimizationType.MAX, -1, 1)

            For i As Integer = 0 To objectiveFunctionCoefficients.Count - 1
                Dim coefficientTerm As Double = maxormin * objectiveFunctionCoefficients(i)

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
            For j As Integer = 0 To constraintRightHandSides.Length - 1
                Dim constraint As List(Of Double) = constraintCoefficients(j)
                If constraint(n) > 0 Then
                    Dim ratio As Double = constraintRightHandSides(j) / constraint(n)

                    ' q holds the lowest ratio, if a lowest ratio is found, our choice is changed to corresponding constraint index
                    If j = 0 OrElse ratio.CompareTo(q) < 0 Then
                        q = ratio
                        choice = j
                    End If
                End If
            Next

            Return choice
        End Function

        Private Function runIteration(basicVariables As List(Of Integer),
                                      artificialVariables As List(Of Integer),
                                      solutionLog As StringBuilder,
                                      feasibleSolutionTime&,
                                      showProgress As Boolean) As LPPSolution

            ' Pivot until optimal solution
            Dim go As Boolean = True
            Dim limiter As Integer = 1
            Dim tick As Action
            Dim progress As ProgressBar = Nothing

            If showProgress Then
                progress = New ProgressBar("Run LPP Solution Iterations...")

                With New ProgressProvider(progress, PIVOT_ITERATION_LIMIT)
                    Dim ETA$, msg$

                    tick = Sub()
                               limiter += 1
                               ETA = .ETA().FormatTime
                               msg = $"Iteration {limiter}/{PIVOT_ITERATION_LIMIT}, ETA={ETA}"
                               progress.SetProgress(.StepProgress, msg)
                           End Sub
                End With
            Else
                tick = Sub() limiter += 1

            End If

            'LaTeXString += latex.LPPtoLaTeX.beginTableaus(Input);

            Do While go

                ' Get next variable to pivot on
                Dim n As Integer = choosePivotVar(artificialVariables)
                Dim [next] As Integer = choosePivotConstraint(n)

                ' If optimal solution reached, end 'while' statement
                'LaTeXString += latex.LPPtoLaTeX.makeTableau(Input, BasicVars, limiter);

                If n = -1 Then
                    ' Found a solution.  Stop pivoting.
                    go = False

                ElseIf limiter = PIVOT_ITERATION_LIMIT Then
                    Call progress?.SetProgress(100, "Max iteration reached...")
                    Call progress?.Dispose()
                    ' Check iteration limit not exceeded.
                    Return New LPPSolution("The pivot max iteration cap was exceeded.", solutionLog.ToString, feasibleSolutionTime)

                ElseIf [next] = -1 Then
                    Call progress?.SetProgress(100, "LPP is unbounded!")
                    Call progress?.Dispose()
                    ' Check for unboundedness.
                    Return New LPPSolution("The given LPP is unbounded.", solutionLog.ToString, feasibleSolutionTime)

                Else
                    ' Get pivot constraint, continue.
                    pivot(n, [next])
                    basicVariables([next]) = n
                    solutionLog.AppendLine("Pivot at " & n & ", " & [next])
                End If

                Call tick()
            Loop

            Call progress?.SetProgress(100, "Complete!")
            Call progress?.Dispose()

            Return Nothing
        End Function

        Public Function solve(Optional showProgress As Boolean = True) As LPPSolution
            ' Initialize Variables
            ' Point badness if we are going to be incrementing this later?
            Dim solutionLog As New StringBuilder
            Dim varNum As Integer = variableNames.Count

            Call makeStandardForm()

            Dim artificialVariables As List(Of Integer) = ArtificialVariableAssignments

            ' ArrayList<String> varNames = Input.VariableNames;
            ' String LaTeXString = latex.LPPtoLaTeX.displayLPP(Input)+'\n';
            If showProgress Then
                Call Console.WriteLine("Make Standard Form...")
            End If

            Call solutionLog.AppendLine("Make Standard Form")
            Call makeStandardForm(artificialVariables)

            Dim startTime As Long = App.ElapsedMilliseconds

            If showProgress Then
                Call Console.WriteLine("Add artificial variables to the LPP...")
            End If

            ' Add artificial variables to the LPP
            Call addArtificialVariables(artificialVariables)

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
            Dim optimalSolution(variableNames.Count - 1) As Double
            Dim reducedCost(variableNames.Count - 1) As Double
            Dim shadowPrice(constraintTypes.Length - 1) As Double
            Dim slack(constraintTypes.Length - 1) As Double

            ' Collect optimal solution and reduced cost from final tableau
            For i As Integer = 0 To variableNames.Count - 1
                If basicVariables.Contains(i) Then
                    Dim basicVariableIndex As Integer = basicVariables.IndexOf(i)

                    ' Check for redundant constraints
                    If basicVariables.IndexOf(i) <> basicVariables.LastIndexOf(i) Then
                        For k As Integer = 0 To basicVariables.Count - 1
                            If basicVariables(k) = i Then
                                Dim constraint As List(Of Double) = constraintCoefficients(basicVariableIndex)

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
                    optimalSolution(i) = constraintRightHandSides(basicVariableIndex)
                    reducedCost(i) = objectiveFunctionCoefficients(i)
                Else
                    optimalSolution(i) = 0
                    reducedCost(i) = objectiveFunctionCoefficients(i)
                End If
            Next

            ' Collect constraint sensitivity analysis data from final tableau
            For j As Integer = 0 To constraintTypes.Length - 1
                If constraintTypes(j) = "=" Then
                    slack(j) = 0
                    shadowPrice(j) = -1 * objectiveFunctionCoefficients(artificialVariables(j))
                    ' This had the double 0.0 or -0.0 check.
                ElseIf objectiveFunctionCoefficients(varNum) = 0.0 Then
                    slack(j) = constraintRightHandSides(j)
                    shadowPrice(j) = 0
                    varNum += 1
                Else
                    slack(j) = 0
                    shadowPrice(j) = -1 * objectiveFunctionCoefficients(varNum)
                    varNum += 1
                End If
            Next

            ' TODO: Undo makeStandardForm ... convert back or accept mutability of LPP?
            ' TODO: Dependency - print artificial variables?
            ' TODO: Create new LPPSolution objects during 

            ' Return the compiled solution.
            Return New LPPSolution(
                optimalSolution, objectiveFunctionValue, variableNames.ToArray, constraintTypes,
                slack,
                shadowPrice,
                reducedCost,
                App.ElapsedMilliseconds - startTime,
                feasibleSolutionTime,
                solutionLog.ToString, DecimalFormat
            )
        End Function
    End Class
End Namespace
