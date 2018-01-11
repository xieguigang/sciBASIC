Namespace Algebra.LinearProgramming

    ''' <summary>
    ''' Linear programming solver from: 
    ''' 
    ''' https://github.com/gthole/lpp
    ''' </summary>
    Public Class LPP

        Private objectiveFunctionType As OptimizationType
        Private variableNames() As String
        Private objectiveFunctionCoefficients() As Double
        Private constraintCoefficients()() As Double
        Private constraintTypes() As String
        Private constraintRightHandSides() As Double
        Private objectiveFunctionValue As Double

        Const PIVOT_ITERATION_LIMIT As Integer = 1000
        Const USE_SUBSCRIPT_UNICODE As Boolean = False

        Sub New(opt As OptimizationType, variableNames$(), objectiveFunctionCoefficients#(), constraintCoefficients#(,), constraintTypes$(), constraintRightHandSides#(), Optional objectiveFunctionValue# = 0)
            Call Me.New(opt.Description, variableNames, objectiveFunctionCoefficients, constraintCoefficients.ToVectorList, constraintTypes, constraintRightHandSides, objectiveFunctionValue)
        End Sub

        Public Sub New(objectiveFunctionType As String, variableNames() As String, objectiveFunctionCoefficients() As Double, constraintCoefficients()() As Double, constraintTypes() As String, constraintRightHandSides() As Double, objectiveFunctionValue As Double)

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
                    Throw New Exception("LPP constraint " & i & " is not of the same size length as the objective function.")
                End If
            Next

            Me.objectiveFunctionType = objectiveFunctionType.ParseType
            Me.variableNames = variableNames
            Me.objectiveFunctionCoefficients = objectiveFunctionCoefficients
            Me.constraintCoefficients = constraintCoefficients
            Me.constraintTypes = constraintTypes
            Me.constraintRightHandSides = constraintRightHandSides
            Me.objectiveFunctionValue = objectiveFunctionValue
        End Sub


        Public Overrides Function ToString() As String
            Dim output As String = objectiveFunctionType.Description

            output = output & "  " & displayEqLine(objectiveFunctionCoefficients, variableNames)
            output = output & ControlChars.Lf & "subject to the constraints:" & ControlChars.Lf

            For j As Integer = 0 To constraintRightHandSides.Length - 1
                Dim constraint() As Double = constraintCoefficients(j)
                output += displayEqLine(constraint, variableNames)
                output &= " " & constraintTypes(j)
                output &= " " & formatDecimals(constraintRightHandSides(j))
                output += ControlChars.Lf
            Next

            Return output & ControlChars.Lf
        End Function

        Private Shared Function displayEqLine(coefficients() As Double, variableNames() As String) As String
            Dim output As String = ""

            Dim startIndex As Integer = 1
            For i As Integer = 0 To variableNames.Length - 1
                If coefficients(i) <> 0 Then
                    output = output + formatDecimals(coefficients(i)) + variableNames(i)
                    Exit For
                Else
                    startIndex += 1
                End If
            Next

            For i As Integer = startIndex To variableNames.Length - 1
                Dim signString As String = " + "
                Dim sign As Double = 1.0

                If coefficients(i) < 0.0 Then
                    signString = " - "
                    sign = -1
                End If
                If coefficients(i) <> 0 Then
                    output = output + signString + formatDecimals(sign * coefficients(i)) + variableNames(i)
                End If
            Next

            Return output
        End Function

        ''' <summary>
        ''' Convert an integer into a multi-character subscript.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Private Function subscriptN(n As Integer) As String
            If Not USE_SUBSCRIPT_UNICODE Then
                Return "_" & n
            End If

            Dim index As String = "" & n
            Dim subscript As String = ""
            Dim c As Char

            For i As Integer = 0 To index.Length - 1
                Select Case n
                    Case 0
                        c = ChrW(&H2080)
                    Case 1
                        c = ChrW(&H2081)
                    Case 2
                        c = ChrW(&H2082)
                    Case 3
                        c = ChrW(&H2083)
                    Case 4
                        c = ChrW(&H2084)
                    Case 5
                        c = ChrW(&H2085)
                    Case 6
                        c = ChrW(&H2086)
                    Case 7
                        c = ChrW(&H2087)
                    Case 8
                        c = ChrW(&H2088)
                    Case Else
                        c = ChrW(&H2089)
                End Select

                subscript += c
            Next

            Return subscript
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
                        assignments.Add(objectiveFunctionCoefficients.Length + k)
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
            Dim pivotConstraint() As Double = constraintCoefficients(constIndex)
            Dim pivotConstraintRHS As Double = constraintRightHandSides(constIndex)

            If pivotConstraint(varIndex) <> 0 Then

                'Divide the pivot constraint through by the pivot variable coefficient
                Dim pivotVarCoeff As Double = pivotConstraint(varIndex)
                For i As Integer = 0 To pivotConstraint.Length - 1
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
                        Dim constraint() As Double = constraintCoefficients(j)
                        Dim constraintRHS As Double = constraintRightHandSides(j)

                        ' check the coefficient of the pivot variable in the non-pivot constraint != 0
                        If constraint(varIndex) <> 0 Then
                            Dim constraintPivotVarCoeff As Double = constraint(varIndex)

                            ' perform Elimination variable by variable
                            For i As Integer = 0 To constraint.Length - 1
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

                For i As Integer = 0 To objectiveFunctionCoefficients.Length - 1
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
            Dim newVariableNames() As String = copyOf(variableNames, variableNames.Length + 1)
            newVariableNames(variableNames.Length) = "v" & subscriptN(variableNames.Length + 1)
            variableNames = newVariableNames

            Dim newObjectiveFunctionCoefficients() As Double = copyOf(objectiveFunctionCoefficients, objectiveFunctionCoefficients.Length + 1)
            newObjectiveFunctionCoefficients(objectiveFunctionCoefficients.Length) = 0
            objectiveFunctionCoefficients = newObjectiveFunctionCoefficients

            For j As Integer = 0 To constraintCoefficients.Length - 1
                Dim constraint() As Double = copyOf(constraintCoefficients(j), constraintCoefficients(j).Length + 1)
                constraint(constraintCoefficients(j).Length) = If(j <> constraintIndex, 0, value)
                constraintCoefficients(j) = constraint
            Next
        End Sub

        Private Shared Function isFeasible(lpp As LPP, possibleSolution As List(Of Integer)) As Boolean
            For j As Integer = 0 To lpp.constraintRightHandSides.Length - 1
                lpp.pivot(possibleSolution(j), j)
            Next

            For j As Integer = 0 To lpp.constraintRightHandSides.Length - 1
                Dim constraint() As Double = lpp.constraintCoefficients(j)

                ' Check all basic variables are non-negative
                If lpp.constraintRightHandSides(j) < 0 Then
                    Return False
                End If

                ' Ensure there are no unequal constraints
                Dim q As Double = 0
                For i As Integer = 0 To constraint.Length - 1
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
            Dim n As Integer = variableNames.Length - q
            Dim powersetsize As Integer = CInt(Fix(Math.Pow(2, n)))

            For i As Integer = 0 To powersetsize - 1

                ' Reinitialize potential basic feasible solution
                alpha = New List(Of Integer)()

                '  Convert the binary number to a string containing n digits
                Dim binary As String = intToBinary(i, n)

                '  Create the corresponding subset
                For j As Integer = 0 To binary.Length - 1
                    If binary.Chars(j) = "1"c Then
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

        Private Shared Function intToBinary(binary As Integer, digits As Integer) As String

            Dim temp As String = Convert.ToString(binary, 2)
            Dim foundDigits As Integer = temp.Length
            Dim returner As String = temp

            For i As Integer = foundDigits To digits - 1
                returner = "0" & returner
            Next

            Return returner
        End Function

        Private Function choosePivotVar(artificialVariables As List(Of Integer)) As Integer
            Dim q As Double = 0
            Dim choice As Integer = -1
            Dim maxormin As Integer = If(objectiveFunctionType = OptimizationType.MAX, -1, 1)

            For i As Integer = 0 To objectiveFunctionCoefficients.Length - 1
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
                Dim constraint() As Double = constraintCoefficients(j)
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

        Public Function solve() As LPPSolution
            ' Initialize Variables
            Dim varNum As Integer = variableNames.Length ' Point badness if we are going to be incrementing this later?
            Me.makeStandardForm()
            Dim artificialVariables As List(Of Integer) = ArtificialVariableAssignments

            ' ArrayList<String> varNames = Input.VariableNames;
            ' String LaTeXString = latex.LPPtoLaTeX.displayLPP(Input)+'\n';

            Dim solutionLog As String = "Make Standard Form" & vbLf
            makeStandardForm(artificialVariables)
            Dim startTime As Long = App.ElapsedMilliseconds

            ' Add artificial variables to the LPP
            addArtificialVariables(artificialVariables)

            ' Search for Basic Feasible Solution
            Dim basicVariables As List(Of Integer) = findInitialBasicVariables(artificialVariables)
            Dim feasibleSolutionTime As Long = App.ElapsedMilliseconds - startTime

            ' Return fail message if no feasible solution is found
            If basicVariables(0) = -1 Then
                Return New LPPSolution("Could not find a Basic Feasible Solution.", solutionLog, feasibleSolutionTime)
            End If

            solutionLog &= "Basic Variables: " & String.Join(", ", basicVariables.ToArray) & ControlChars.Lf


            ' Pivot until optimal solution
            Dim go As Boolean = True
            Dim limiter As Integer = 1
            'LaTeXString += latex.LPPtoLaTeX.beginTableaus(Input);

            Do While go

                ' Get next variable to pivot on
                Dim n As Integer = choosePivotVar(artificialVariables)
                Dim [next] As Integer = choosePivotConstraint(n)

                ' If optimal solution reached, end 'while' statement
                'LaTeXString += latex.LPPtoLaTeX.makeTableau(Input, BasicVars, limiter);

                ' Found a solution.  Stop pivoting.
                If n = -1 Then
                    go = False

                    ' Check iteration limit not exceeded.
                ElseIf limiter = PIVOT_ITERATION_LIMIT Then
                    Return New LPPSolution("The pivot max iteration cap was exceeded.", solutionLog, feasibleSolutionTime)

                    ' Check for unboundedness.
                ElseIf [next] = -1 Then
                    Return New LPPSolution("The given LPP is unbounded.", solutionLog, feasibleSolutionTime)

                    ' Get pivot constraint, continue.
                Else
                    pivot(n, [next])
                    basicVariables([next]) = n
                    solutionLog &= "Pivot at " & n & ", " & [next] & vbLf
                End If

                limiter += 1
            Loop

            ' Close LaTeX and initialize sensitivity variables
            ' LaTeXString += latex.LPPtoLaTeX.endTableaus();
            Dim optimalSolution(variableNames.Length - 1) As Double
            Dim reducedCost(variableNames.Length - 1) As Double
            Dim shadowPrice(constraintTypes.Length - 1) As Double
            Dim slack(constraintTypes.Length - 1) As Double

            ' Collect optimal solution and reduced cost from final tableau
            For i As Integer = 0 To variableNames.Length - 1
                If basicVariables.Contains(i) Then
                    Dim basicVariableIndex As Integer = basicVariables.IndexOf(i)

                    ' Check for redundant constraints
                    If basicVariables.IndexOf(i) <> basicVariables.LastIndexOf(i) Then
                        For k As Integer = 0 To basicVariables.Count - 1
                            If basicVariables(k) = i Then
                                Dim constraint() As Double = constraintCoefficients(basicVariableIndex)

                                For m As Integer = 0 To constraint.Length - 1
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
            Return New LPPSolution(optimalSolution, objectiveFunctionValue, variableNames, constraintTypes, slack, shadowPrice, reducedCost, App.ElapsedMilliseconds - startTime, feasibleSolutionTime, solutionLog)
        End Function
    End Class
End Namespace