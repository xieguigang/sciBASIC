Imports System.Collections.Concurrent
Imports System.Text
Imports System.Threading.Tasks
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports std = System.Math
' the root namespace of this project has its own Parallel type, so that the
' TPL Parallel type should be referenced via an alias to avoid the conflict.
Imports ParallelTask = System.Threading.Tasks.Parallel

Namespace LinearAlgebra.LinearProgramming

    ''' <summary>
    ''' A two-phased simplex solver which is implemented in the sparse matrix
    ''' format with the bounded variable support.
    ''' </summary>
    ''' <remarks>
    ''' ###### why this solver is re-written
    '''
    ''' the previous implementation of this solver class have some critical
    ''' bugs and performance problems:
    '''
    ''' 1. the basic variables was re-calculated from the tableau via a
    '''    "unit vector" scanning helper, which costs O(m^2 * n) and will
    '''    fail on the numerical noise, result in a full zero solution.
    ''' 2. the phase 1 objective includes the slack variables, and the
    '''    reduced cost row is never priced out against the initial basis.
    ''' 3. the tableau was stored in a dense format, which runs out of the
    '''    memory on the genome scale metabolic network problem.
    '''
    ''' this implementation maintains the basic variable list explicitly,
    ''' handles the variable bounds in the ratio test (bounded simplex), and
    ''' runs the pivot elimination in parallel.
    ''' </remarks>
    Public Class LPPSolverTwoPhased

        Const INF As Double = Double.PositiveInfinity

        ReadOnly lpp As LPP
        ReadOnly strict As Boolean

        ' ---------------------------------------------
        ' the working problem
        ' ---------------------------------------------
        Dim m As Integer
        Dim nStruct As Integer
        Dim nSlack As Integer
        Dim nArt As Integer
        Dim nWork As Integer

        Dim tableau() As SparseTableauRow
        ''' <summary>the current value of each basic variable, index by row</summary>
        Dim b() As Double
        ''' <summary>the upper bound of each variable, INF means no upper bound</summary>
        Dim hi() As Double
        ''' <summary>the objective coefficient of the current phase</summary>
        Dim c() As Double
        ''' <summary>the reduced cost of each variable, always in a dense format</summary>
        Dim d() As Double
        ''' <summary>basis(row) = the column index of the basic variable</summary>
        Dim basis() As Integer
        ''' <summary>0 = at lower bound(zero), 1 = basic, 2 = at upper bound</summary>
        Dim status() As Byte
        Dim isArt() As Boolean
        ''' <summary>false when the constraint row is a redundant row</summary>
        Dim rowActive() As Boolean
        ''' <summary>the cached pivot column</summary>
        Dim alpha() As Double
        Dim touched() As Integer
        Dim nTouched As Integer
        Dim maxAbsAlpha As Double
        ''' <summary>the ratio test buffer, a negative value means not a candidate</summary>
        Dim ratioBuf() As Double
        ''' <summary>1 = blocked by the lower bound, 2 = blocked by the upper bound</summary>
        Dim kindBuf() As Byte

        Dim objValue As Double
        Dim minSign As Double

        ' ---------------------------------------------
        ' mapping back to the original variables
        ' ---------------------------------------------
        Dim mapA() As Integer
        Dim mapB() As Integer
        Dim mapOffset() As Double
        Dim structC() As Double
        Dim objConst As Double

        Dim slackCol() As Integer
        Dim artCol() As Integer

        Dim nOrig As Integer
        Dim origTypes() As String
        Dim origRhs() As Double
        Dim origA As LpSparseMatrix

        ' numeric tolerances
        Dim dropTol As Double = 0.000000000001
        Dim pivotTol As Double = 0.000000001
        Dim zeroTol As Double = 0.000000001
        Dim pricingTol As Double = 0.000000001
        Dim feasTol As Double = 0.0000001

        Sub New(problem As LPP, strict As Boolean)
            Me.lpp = problem
            Me.strict = strict
        End Sub

        ''' <summary>
        ''' solve the linear programming problem
        ''' </summary>
        Public Function Solve(Optional showProgress As Boolean = True) As LPPSolution
            Dim solutionLog As New StringBuilder
            Dim startTime As Long = App.ElapsedMilliseconds
            Dim buildError As String = Nothing

            Try
                Call Build(solutionLog, showProgress)
            Catch ex As Exception
                buildError = ex.Message
            End Try

            If Not buildError.StringEmpty Then
                Return New LPPSolution("Invalid LPP model: " & buildError, solutionLog.ToString, 0)
            End If

            Dim feasibleSolutionTime As Long = 0

            ' the phase 1 is only required when the initial basis is not a
            ' feasible solution of the original problem: if all of the
            ' artificial variables are zero, then the start point (all of the
            ' structural variables at their lower bound) is already feasible,
            ' which is the common case of the FBA problem (the right hand side
            ' of the mass balance constraint is always zero).
            If nArt > 0 AndAlso NeedsPhase1() Then
                Dim phase1Error As LPPSolution = Phase1(solutionLog, showProgress)

                feasibleSolutionTime = App.ElapsedMilliseconds - startTime

                If phase1Error IsNot Nothing Then
                    Return phase1Error
                End If
            End If

            Dim phase2Error As LPPSolution = Phase2(solutionLog, showProgress)

            If phase2Error IsNot Nothing Then
                Return phase2Error
            End If

            Dim result As (values As Double(), slack As Double(), shadow As Double(), reduced As Double()) = ExtractSolution()
            Dim objective As Double = minSign * objValue + objConst

            Return New LPPSolution(
                result.values,
                objective,
                lpp.variableNames.Take(nOrig).ToArray,
                lpp.constraintTypes,
                result.slack,
                result.shadow,
                result.reduced,
                App.ElapsedMilliseconds - startTime,
                feasibleSolutionTime,
                solutionLog.ToString,
                LPP.DecimalFormat
            )
        End Function

        ''' <summary>
        ''' normalize the constraint type symbol
        ''' </summary>
        Private Shared Function normType(type As String) As String
            If type Is Nothing Then
                Return "="
            End If

            Select Case type.Trim
                Case "<=", "≤", "=<", "less", "le"
                    Return "<="
                Case ">=", "≥", "=>", "greater", "ge"
                    Return ">="
                Case Else
                    Return "="
            End Select
        End Function

        ''' <summary>
        ''' build the working tableau of the simplex method
        ''' </summary>
        Private Sub Build(log As StringBuilder, showProgress As Boolean)
            nOrig = lpp.originalVariableCount
            origTypes = lpp.constraintTypes.ToArray
            origRhs = lpp.constraintRightHandSides.ToArray

            If lpp.sparseConstraints IsNot Nothing Then
                origA = lpp.sparseConstraints
            Else
                Dim dense As Double()() = New Double(lpp.constraintCoefficients.Length - 1)() {}

                For i As Integer = 0 To dense.Length - 1
                    dense(i) = lpp.constraintCoefficients(i).ToArray
                Next

                origA = LpSparseMatrix.FromJagged(dense)
            End If

            m = origA.Rows

            If m <> origTypes.Length OrElse m <> origRhs.Length Then
                Throw New Exception("the constraint matrix size is not matched with the constraint type list")
            End If
            If origA.Columns <> nOrig Then
                Throw New Exception($"the constraint matrix column size ({origA.Columns}) is not matched with the variable size ({nOrig})")
            End If

            minSign = If(lpp.objectiveFunctionType = OptimizationType.MAX, -1.0, 1.0)

            Call BuildVariableMapping()
            Call BuildTableau(log)
        End Sub

        ''' <summary>
        ''' the variables with a negative lower bound will be splitted into
        ''' two non-negative variables, and the variables with a positive
        ''' lower bound will be shifted, so that all of the working variables
        ''' have a zero lower bound.
        ''' </summary>
        Private Sub BuildVariableMapping()
            Dim lo As Double() = lpp.lowerBounds
            Dim ub As Double() = lpp.upperBounds
            Dim cOrig As Double() = lpp.objectiveFunctionCoefficients.ToArray

            mapA = New Integer(nOrig - 1) {}
            mapB = New Integer(nOrig - 1) {}
            mapOffset = New Double(nOrig - 1) {}
            objConst = 0.0
            nStruct = 0

            Dim lj, uj As Double

            For j As Integer = 0 To nOrig - 1
                lj = If(lo Is Nothing OrElse j >= lo.Length, 0.0, lo(j))
                uj = If(ub Is Nothing OrElse j >= ub.Length, INF, ub(j))

                If lj < 0 AndAlso uj > 0 Then
                    ' v = x(+) - x(-), x(+) in [0, ub], x(-) in [0, -lb]
                    mapA(j) = nStruct
                    mapB(j) = nStruct + 1
                    mapOffset(j) = 0.0
                    nStruct += 2
                Else
                    ' v = w + lb, w in [0, ub - lb]
                    mapA(j) = nStruct
                    mapB(j) = -1
                    mapOffset(j) = lj
                    nStruct += 1
                End If
            Next

            structC = New Double(nStruct - 1) {}

            For j As Integer = 0 To nOrig - 1
                lj = If(lo Is Nothing OrElse j >= lo.Length, 0.0, lo(j))
                uj = If(ub Is Nothing OrElse j >= ub.Length, INF, ub(j))
                cOrig(j) = lpp.objectiveFunctionCoefficients(j)

                If mapB(j) >= 0 Then
                    structC(mapA(j)) = cOrig(j)
                    structC(mapB(j)) = -cOrig(j)
                Else
                    structC(mapA(j)) = cOrig(j)
                    objConst += cOrig(j) * lj
                End If
            Next
        End Sub

        ''' <summary>
        ''' build the sparse simplex tableau in the standard form
        ''' </summary>
        Private Sub BuildTableau(log As StringBuilder)
            Dim rhsW As Double() = New Double(m - 1) {}
            Dim flip As Double() = New Double(m - 1) {}
            Dim types As String() = New String(m - 1) {}

            Array.Copy(origRhs, rhsW, m)

            ' the shift of the variables with a non-zero lower bound
            For i As Integer = 0 To m - 1
                For p As Integer = origA.RowPtr(i) To origA.RowPtr(i + 1) - 1
                    Dim j As Integer = origA.ColIdx(p)

                    If j < nOrig AndAlso mapB(j) < 0 AndAlso mapOffset(j) <> 0.0 Then
                        rhsW(i) -= origA.Values(p) * mapOffset(j)
                    End If
                Next
            Next

            nSlack = 0
            nArt = 0

            For i As Integer = 0 To m - 1
                Dim ty As String = normType(origTypes(i))

                If rhsW(i) < 0 Then
                    rhsW(i) = -rhsW(i)
                    flip(i) = -1.0

                    If ty = "<=" Then
                        ty = ">="
                    ElseIf ty = ">=" Then
                        ty = "<="
                    End If
                Else
                    flip(i) = 1.0
                End If

                types(i) = ty

                If ty <> "=" Then nSlack += 1
                If ty <> "<=" Then nArt += 1
            Next

            Dim slackBase As Integer = nStruct
            Dim artBase As Integer = nStruct + nSlack

            nWork = nStruct + nSlack + nArt
            slackCol = New Integer(m - 1) {}
            artCol = New Integer(m - 1) {}

            Dim si As Integer = 0
            Dim ai As Integer = 0

            For i As Integer = 0 To m - 1
                If types(i) <> "=" Then
                    slackCol(i) = slackBase + si
                    si += 1
                Else
                    slackCol(i) = -1
                End If

                If types(i) <> "<=" Then
                    artCol(i) = artBase + ai
                    ai += 1
                Else
                    artCol(i) = -1
                End If
            Next

            ' collect the triplets of the working matrix
            Dim nnz As Integer = origA.NonZeros
            Dim tRow As New List(Of Integer)(nnz + m)
            Dim tCol As New List(Of Integer)(nnz + m)
            Dim tVal As New List(Of Double)(nnz + m)

            For i As Integer = 0 To m - 1
                For p As Integer = origA.RowPtr(i) To origA.RowPtr(i + 1) - 1
                    Dim j As Integer = origA.ColIdx(p)
                    Dim v As Double = origA.Values(p) * flip(i)

                    If j >= nOrig Then
                        Continue For
                    End If

                    tRow.Add(i)
                    tCol.Add(mapA(j))
                    tVal.Add(v)

                    If mapB(j) >= 0 Then
                        tRow.Add(i)
                        tCol.Add(mapB(j))
                        tVal.Add(-v)
                    End If
                Next

                If slackCol(i) >= 0 Then
                    tRow.Add(i)
                    tCol.Add(slackCol(i))
                    tVal.Add(If(types(i) = "<=", 1.0, -1.0))
                End If
                If artCol(i) >= 0 Then
                    tRow.Add(i)
                    tCol.Add(artCol(i))
                    tVal.Add(1.0)
                End If
            Next

            Dim work As LpSparseMatrix = LpSparseMatrix.FromTriplets(
                rows:=m, columns:=nWork,
                rowIdx:=tRow.ToArray, colIdx:=tCol.ToArray, vals:=tVal.ToArray
            )

            ' allocate the working buffer
            tableau = New SparseTableauRow(m - 1) {}
            b = New Double(m - 1) {}
            hi = New Double(nWork - 1) {}
            c = New Double(nWork - 1) {}
            d = New Double(nWork - 1) {}
            basis = New Integer(m - 1) {}
            status = New Byte(nWork - 1) {}
            isArt = New Boolean(nWork - 1) {}
            rowActive = New Boolean(m - 1) {}
            alpha = New Double(m - 1) {}
            touched = New Integer(m - 1) {}
            ratioBuf = New Double(m - 1) {}
            kindBuf = New Byte(m - 1) {}

            Array.Copy(rhsW, b, m)

            For i As Integer = 0 To m - 1
                Dim row As New SparseTableauRow(work.RowPtr(i + 1) - work.RowPtr(i) + 1)

                For p As Integer = work.RowPtr(i) To work.RowPtr(i + 1) - 1
                    row.Add(work.ColIdx(p), work.Values(p))
                Next

                tableau(i) = row
                rowActive(i) = True

                If types(i) = "<=" Then
                    basis(i) = slackCol(i)
                Else
                    basis(i) = artCol(i)
                End If

                status(basis(i)) = 1
                hi(basis(i)) = INF

                If artCol(i) >= 0 Then
                    isArt(artCol(i)) = True
                End If
            Next

            ' the upper bound of the structural variables
            Dim lo As Double() = lpp.lowerBounds
            Dim ub As Double() = lpp.upperBounds

            For j As Integer = 0 To nOrig - 1
                Dim lj As Double = If(lo Is Nothing OrElse j >= lo.Length, 0.0, lo(j))
                Dim uj As Double = If(ub Is Nothing OrElse j >= ub.Length, INF, ub(j))

                If mapB(j) >= 0 Then
                    hi(mapA(j)) = uj
                    hi(mapB(j)) = -lj
                Else
                    hi(mapA(j)) = If(uj >= INF, INF, uj - lj)
                End If
            Next

            ' the slack and the artificial variables have no upper bound in
            ' the phase 1, the artificial variables will be fixed at zero
            ' when the phase 1 is finished.
            For j As Integer = nStruct To nWork - 1
                If hi(j) = 0.0 Then
                    hi(j) = INF
                End If
            Next

            ' the numeric tolerance is scaled by the problem magnitude
            Dim maxAbs As Double = 1.0

            For i As Integer = 0 To m - 1
                If std.Abs(origRhs(i)) > maxAbs Then maxAbs = std.Abs(origRhs(i))
            Next
            For j As Integer = 0 To nStruct - 1
                If structC(j) <> 0 AndAlso std.Abs(structC(j)) > maxAbs Then maxAbs = std.Abs(structC(j))
            Next

            pricingTol = 0.000000001 * maxAbs
            feasTol = 0.0000001 * maxAbs

            log.AppendLine($"Build simplex tableau: {m} rows, {nStruct} structural variables, {nSlack} slack variables, {nArt} artificial variables")
        End Sub

        ''' <summary>
        ''' d(j) = c(j) - cB * B^-1 * A(j), the reduced cost row is priced out
        ''' against the current basis.
        ''' </summary>
        Private Sub PriceOut()
            Array.Copy(c, d, nWork)

            For r As Integer = 0 To m - 1
                Dim cb As Double = c(basis(r))

                If cb = 0.0 Then
                    Continue For
                End If

                Dim row As SparseTableauRow = tableau(r)

                For p As Integer = 0 To row.Count - 1
                    d(row.Idx(p)) -= cb * row.Val(p)
                Next
            Next
        End Sub

        ''' <summary>
        ''' re-calculate the objective function value from the scratch
        ''' </summary>
        Private Sub ResetObjective()
            objValue = 0.0

            For r As Integer = 0 To m - 1
                objValue += c(basis(r)) * b(r)
            Next
            For j As Integer = 0 To nWork - 1
                If status(j) = 2 Then
                    objValue += c(j) * hi(j)
                End If
            Next
        End Sub

        ''' <summary>
        ''' choose the entering variable via the Dantzig rule, the Bland rule
        ''' is applied when the degenerate iteration is detected.
        ''' </summary>
        Private Function ChooseEntering(useBland As Boolean) As Integer
            Dim best As Integer = -1
            Dim bestRate As Double = 0.0

            For j As Integer = 0 To nWork - 1
                Dim st As Byte = status(j)

                If st = 1 Then
                    Continue For
                End If
                If isArt(j) Then
                    Continue For
                End If
                If hi(j) = 0.0 Then
                    ' the variable is fixed at zero
                    Continue For
                End If

                ' at the lower bound the variable can be increased only, and
                ' at the upper bound the variable can be decreased only.
                Dim rate As Double = If(st = 2, -d(j), d(j))

                If rate < -pricingTol Then
                    If useBland Then
                        Return j
                    End If
                    If rate < bestRate Then
                        bestRate = rate
                        best = j
                    End If
                End If
            Next

            Return best
        End Function

        ''' <summary>
        ''' extract the pivot column from the sparse tableau
        ''' </summary>
        Private Sub ExtractColumn(q As Integer)
            If m >= 128 Then
                ' the range partitioner reduces the scheduling cost of the
                ' TPL task on a large amount of the rows
                ParallelTask.ForEach(Partitioner.Create(0, m),
                    Sub(range)
                        For r As Integer = range.Item1 To range.Item2 - 1
                            If rowActive(r) Then
                                alpha(r) = tableau(r).Item(q)
                            Else
                                alpha(r) = 0.0
                            End If
                        Next
                    End Sub)
            Else
                For r As Integer = 0 To m - 1
                    If rowActive(r) Then
                        alpha(r) = tableau(r).Item(q)
                    Else
                        alpha(r) = 0.0
                    End If
                Next
            End If

            nTouched = 0
            maxAbsAlpha = 0.0

            For r As Integer = 0 To m - 1
                Dim v As Double = std.Abs(alpha(r))

                If v > maxAbsAlpha Then
                    maxAbsAlpha = v
                End If
                If v <> 0.0 Then
                    touched(nTouched) = r
                    nTouched += 1
                End If
            Next
        End Sub

        ''' <summary>
        ''' do the pivot operation at tableau(r, q), note that the value of
        ''' the basic variables is updated by the caller.
        ''' </summary>
        Private Sub Pivot(r As Integer, q As Integer)
            Dim piv As Double = alpha(r)
            Dim row As SparseTableauRow = tableau(r)

            Call row.Scale(1.0 / piv, dropTol)

            If nTouched >= 16 Then
                ParallelTask.ForEach(Partitioner.Create(0, nTouched),
                    Sub(range)
                        For k As Integer = range.Item1 To range.Item2 - 1
                            Dim i As Integer = touched(k)

                            If i <> r Then
                                tableau(i).Axpy(row, alpha(i), dropTol)
                            End If
                        Next
                    End Sub)
            Else
                For k As Integer = 0 To nTouched - 1
                    Dim i As Integer = touched(k)

                    If i <> r Then
                        tableau(i).Axpy(row, alpha(i), dropTol)
                    End If
                Next
            End If

            ' update the reduced cost row
            Dim dq As Double = d(q)

            For p As Integer = 0 To row.Count - 1
                d(row.Idx(p)) -= dq * row.Val(p)
            Next

            d(q) = 0.0
            basis(r) = q
        End Sub

        ''' <summary>
        ''' run the bounded simplex iteration until the optimal solution is
        ''' reached.
        ''' </summary>
        ''' <returns>the error message, nothing when the iteration is converged</returns>
        Private Function RunSimplex(limit As Integer, log As StringBuilder,
                                    showProgress As Boolean, phaseName As String) As String

            Dim iteration As Integer = 0
            Dim degenerate As Integer = 0
            Dim useBland As Boolean = False
            Dim pivotFailure As Integer = 0
            Dim nextTick As Integer = 2000
            Dim clock As Stopwatch = Stopwatch.StartNew
            Dim costPrice As Long = 0, costColumn As Long = 0
            Dim costRatio As Long = 0, costPivot As Long = 0, costUpdate As Long = 0
            Dim mark As Long = clock.ElapsedTicks

            Do While iteration < limit
                Dim q As Integer = ChooseEntering(useBland)

                costPrice += clock.ElapsedTicks - mark
                mark = clock.ElapsedTicks

                If q < 0 Then
                    Exit Do
                End If

                Call ExtractColumn(q)

                costColumn += clock.ElapsedTicks - mark
                mark = clock.ElapsedTicks

                Dim dir As Integer = If(status(q) = 2, -1, 1)
                Dim t As Double = If(hi(q) >= INF, INF, hi(q))

                ' pass 1: the ratio test, both of the lower bound and the upper
                ' bound of the basic variables are checked here.
                For r As Integer = 0 To m - 1
                    ratioBuf(r) = -1.0

                    If Not rowActive(r) Then
                        Continue For
                    End If

                    Dim a As Double = alpha(r) * dir

                    If a > zeroTol Then
                        ratioBuf(r) = b(r) / a
                        kindBuf(r) = 1
                    ElseIf a < -zeroTol Then
                        Dim hb As Double = hi(basis(r))

                        If hb < INF Then
                            ratioBuf(r) = (hb - b(r)) / (-a)
                            kindBuf(r) = 2
                        End If
                    End If

                    If ratioBuf(r) >= 0.0 AndAlso ratioBuf(r) < t Then
                        t = ratioBuf(r)
                    End If
                Next

                ' the degenerated iteration produces a lot of the ratio ties,
                ' the tie is broken with the pivot magnitude at first (for the
                ' numerical stability) and then with the row sparsity (for 
                ' reducing the fill-in of the sparse tableau).
                Dim tieEps As Double = zeroTol
                Dim tieMax As Double = 0.0

                For r As Integer = 0 To m - 1
                    If ratioBuf(r) >= 0.0 AndAlso ratioBuf(r) <= t + tieEps Then
                        Dim absA As Double = std.Abs(alpha(r))

                        If absA > tieMax Then
                            tieMax = absA
                        End If
                    End If
                Next

                Dim minAbsA As Double = 0.1 * tieMax
                Dim leaveRow As Integer = -1
                Dim leaveUpper As Boolean = False
                Dim leaveNnz As Integer = Integer.MaxValue

                For r As Integer = 0 To m - 1
                    If ratioBuf(r) < 0.0 OrElse ratioBuf(r) > t + tieEps Then
                        Continue For
                    End If
                    If std.Abs(alpha(r)) < minAbsA Then
                        Continue For
                    End If
                    If tableau(r).Count < leaveNnz Then
                        leaveNnz = tableau(r).Count
                        leaveRow = r
                        leaveUpper = (kindBuf(r) = 2)
                    End If
                Next

                If leaveRow < 0 AndAlso tieMax > 0.0 Then
                    ' fall back to the largest pivot element in the tie set
                    Dim bestAbsA As Double = 0.0

                    For r As Integer = 0 To m - 1
                        If ratioBuf(r) < 0.0 OrElse ratioBuf(r) > t + tieEps Then
                            Continue For
                        End If

                        Dim absA As Double = std.Abs(alpha(r))

                        If absA > bestAbsA Then
                            bestAbsA = absA
                            leaveRow = r
                            leaveUpper = (kindBuf(r) = 2)
                        End If
                    Next
                End If

                If leaveRow >= 0 Then
                    t = ratioBuf(leaveRow)
                End If

                costRatio += clock.ElapsedTicks - mark
                mark = clock.ElapsedTicks

                If t >= INF Then
                    Return "The given LPP is unbounded."
                End If
                If t < 0.0 Then
                    t = 0.0
                End If

                objValue += d(q) * dir * t

                costUpdate += clock.ElapsedTicks - mark
                mark = clock.ElapsedTicks

                If t > 0.0 Then
                    Dim step_ As Double = dir * t

                    If m >= 128 Then
                        ParallelTask.ForEach(Partitioner.Create(0, m),
                            Sub(range)
                                For r As Integer = range.Item1 To range.Item2 - 1
                                    b(r) -= alpha(r) * step_
                                Next
                            End Sub)
                    Else
                        For r As Integer = 0 To m - 1
                            b(r) -= alpha(r) * step_
                        Next
                    End If
                End If

                If leaveRow < 0 Then
                    ' the entering variable hits its own upper bound, just
                    ' flip the variable to the other bound, no basis change
                    ' is required by this operation.
                    status(q) = If(status(q) = 2, CByte(0), CByte(2))
                ElseIf std.Abs(alpha(leaveRow)) < std.Max(pivotTol, 0.001 * tieMax) Then
                    pivotFailure += 1
                    useBland = True

                    If pivotFailure = 1 Then
                        Call $"[{phaseName}] pivot too small: q = {q}, |pivot| = {std.Abs(alpha(leaveRow)).ToString("G4")}, tie max = {tieMax.ToString("G4")}, non-zeros = {nTouched}, t = {t.ToString("G4")}".warning
                    End If

                    If pivotFailure > 32 Then
                        Return $"Numerical failure in the {phaseName}: the pivot element is too small."
                    End If
                Else
                    Dim leaving As Integer = basis(leaveRow)
                    Dim xq As Double = If(status(q) = 2, hi(q) - t, t)

                    Call Pivot(leaveRow, q)

                    status(q) = 1
                    status(leaving) = If(leaveUpper, CByte(2), CByte(0))
                    b(leaveRow) = xq
                    pivotFailure = 0
                End If

                costPivot += clock.ElapsedTicks - mark
                mark = clock.ElapsedTicks

                If t > 0.0 Then
                    degenerate = 0
                    useBland = False
                Else
                    degenerate += 1

                    If degenerate > std.Max(256, m \ 4) Then
                        useBland = True
                    End If
                End If

                iteration += 1

                If showProgress AndAlso iteration >= nextTick Then
                    Dim fillIn As Integer = 0

                    For r As Integer = 0 To m - 1
                        fillIn += tableau(r).Count
                    Next

                    nextTick = iteration + 2000
                    Call $"[{phaseName}] {iteration} iterations, objective = {objValue.ToString("G6")}, fill-in = {fillIn / std.Max(m, 1)} non-zeros/row".info
                    Call $"    cost: price = {costPrice * 1000.0 / Stopwatch.Frequency}ms, column = {costColumn * 1000.0 / Stopwatch.Frequency}ms, ratio = {costRatio * 1000.0 / Stopwatch.Frequency}ms, update = {costUpdate * 1000.0 / Stopwatch.Frequency}ms, pivot = {costPivot * 1000.0 / Stopwatch.Frequency}ms".info
                End If
            Loop

            If iteration >= limit Then
                Dim msg As String = $"Iteration limit exceeded upper bound {limit} in the {phaseName}"

                If strict Then
                    Return msg
                Else
                    Call ("[LPP_SOLVER] " & msg).warning
                End If
            End If

            log.AppendLine($"{phaseName}: {iteration} iterations, objective = {objValue}")

            Return Nothing
        End Function

        ''' <summary>
        ''' the iteration upper bound is scaled by the problem size
        ''' </summary>
        Private ReadOnly Property iterationLimit As Integer
            Get
                Return std.Max(LPP.PIVOT_ITERATION_LIMIT, 4 * (m + nWork) + 1000)
            End Get
        End Property

        ''' <summary>
        ''' checks whether the initial basis is already a feasible solution of
        ''' the original problem: all of the artificial variables are zero.
        ''' </summary>
        Private Function NeedsPhase1() As Boolean
            For r As Integer = 0 To m - 1
                If isArt(basis(r)) AndAlso b(r) > feasTol Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' phase 1: minimize the sum of the artificial variables for seeking
        ''' an initial basic feasible solution.
        ''' </summary>
        Private Function Phase1(log As StringBuilder, showProgress As Boolean) As LPPSolution
            For j As Integer = 0 To nWork - 1
                c(j) = If(isArt(j), 1.0, 0.0)
            Next

            Call PriceOut()
            Call ResetObjective()

            Dim msg As String = RunSimplex(iterationLimit, log, showProgress, "phase 1")

            If msg IsNot Nothing Then
                Return New LPPSolution(msg, log.ToString, 0)
            End If

            If objValue > feasTol Then
                Return New LPPSolution(
                    $"Could not find a Basic Feasible Solution (phase 1 objective = {objValue.ToString("G6")}).",
                    log.ToString, 0)
            End If

            Call DriveArtificialsOut()

            Return Nothing
        End Function

        ''' <summary>
        ''' phase 2: optimize the original objective function
        ''' </summary>
        Private Function Phase2(log As StringBuilder, showProgress As Boolean) As LPPSolution
            For j As Integer = 0 To nWork - 1
                If j < nStruct Then
                    c(j) = minSign * structC(j)
                Else
                    c(j) = 0.0
                End If
            Next

            ' the artificial variables are fixed at zero from now on
            For j As Integer = 0 To nWork - 1
                If isArt(j) Then
                    hi(j) = 0.0
                End If
            Next

            Call PriceOut()
            Call ResetObjective()

            Dim msg As String = RunSimplex(iterationLimit, log, showProgress, "phase 2")

            If msg IsNot Nothing Then
                Return New LPPSolution(msg, log.ToString, 0)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' drive the artificial variables out of the basis, the constraint
        ''' row will be marked as a redundant row when the artificial variable
        ''' can not be pivoted out.
        ''' </summary>
        Private Sub DriveArtificialsOut()
            For r As Integer = 0 To m - 1
                If Not rowActive(r) OrElse Not isArt(basis(r)) Then
                    Continue For
                End If

                Dim best As Integer = -1
                Dim bestVal As Double = 0.0
                Dim row As SparseTableauRow = tableau(r)

                For p As Integer = 0 To row.Count - 1
                    Dim j As Integer = row.Idx(p)

                    If isArt(j) OrElse status(j) = 1 Then
                        Continue For
                    End If

                    Dim av As Double = std.Abs(row.Val(p))

                    If av > bestVal Then
                        bestVal = av
                        best = j
                    End If
                Next

                If best >= 0 AndAlso bestVal > pivotTol Then
                    Dim leaving As Integer = basis(r)
                    Dim xq As Double = If(status(best) = 2, hi(best), 0.0)

                    Call ExtractColumn(best)

                    If std.Abs(alpha(r)) > pivotTol Then
                        Call Pivot(r, best)

                        status(best) = 1
                        status(leaving) = 0
                        b(r) = xq
                    End If
                Else
                    ' this constraint is a redundant constraint
                    rowActive(r) = False
                End If
            Next
        End Sub

        ''' <summary>
        ''' collect the solution of the original variables from the working
        ''' tableau.
        ''' </summary>
        Private Function ExtractSolution() As (values As Double(), slack As Double(), shadow As Double(), reduced As Double())
            Dim x As Double() = New Double(nWork - 1) {}

            For r As Integer = 0 To m - 1
                If rowActive(r) Then
                    x(basis(r)) = b(r)
                End If
            Next
            For j As Integer = 0 To nWork - 1
                If status(j) = 2 Then
                    x(j) = hi(j)
                End If
            Next

            Dim values As Double() = New Double(nOrig - 1) {}

            For j As Integer = 0 To nOrig - 1
                If mapB(j) >= 0 Then
                    values(j) = x(mapA(j)) - x(mapB(j))
                Else
                    values(j) = x(mapA(j)) + mapOffset(j)
                End If
            Next

            Dim slack As Double() = New Double(m - 1) {}
            Dim shadow As Double() = New Double(m - 1) {}

            For i As Integer = 0 To m - 1
                Dim s As Double = origRhs(i)

                For p As Integer = origA.RowPtr(i) To origA.RowPtr(i + 1) - 1
                    Dim j As Integer = origA.ColIdx(p)

                    If j < nOrig Then
                        s -= origA.Values(p) * values(j)
                    End If
                Next

                slack(i) = s

                If slackCol(i) >= 0 Then
                    shadow(i) = -d(slackCol(i)) * minSign
                ElseIf artCol(i) >= 0 Then
                    shadow(i) = -d(artCol(i)) * minSign
                Else
                    shadow(i) = 0.0
                End If
            Next

            Dim reduced As Double() = New Double(nOrig - 1) {}

            For j As Integer = 0 To nOrig - 1
                reduced(j) = d(mapA(j)) * minSign
            Next

            Return (values, slack, shadow, reduced)
        End Function

    End Class

End Namespace
