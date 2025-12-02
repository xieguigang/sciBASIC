Imports System.Text
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming

    ''' <summary>
    ''' 使用两阶段法改进的单纯形法求解器
    ''' </summary>
    Public Class LPPSolverTwoPhased
        ReadOnly lpp As LPP
        Const EPSILON As Double = 0.0000000001

        Sub New(problem As LPP)
            lpp = problem
        End Sub

        Public Function Solve(Optional showProgress As Boolean = True) As LPPSolution
            Dim solutionLog As New StringBuilder
            Dim startTime As Long = App.ElapsedMilliseconds

            ' 标准化问题
            Call lpp.makeStandardForm()
            solutionLog.AppendLine("Converted to standard form")

            ' 添加人工变量
            Dim artificialVars = lpp.ArtificialVariableAssignments()
            Call lpp.addArtificialVariables(artificialVars)
            solutionLog.AppendLine($"Added {artificialVars.AsEnumerable.Count(Function(v) v <> -1)} artificial variables")

            ' 第一阶段：寻找初始可行解
            Dim phase1Result = Phase1(showProgress, solutionLog)
            If phase1Result IsNot Nothing Then
                Return phase1Result
            End If

            ' 移除人工变量
            RemoveArtificialVariables(artificialVars)
            solutionLog.AppendLine("Removed artificial variables")

            ' 第二阶段：求解原问题
            Dim phase2Result = Phase2(showProgress, solutionLog)
            If phase2Result IsNot Nothing Then
                Return phase2Result
            End If

            ' 提取解信息
            Dim solution = ExtractSolution()
            Return New LPPSolution(
                solution.optimalValues,
                lpp.objectiveFunctionValue,
                lpp.variableNames.Take(lpp.originalVariableCount).ToArray,
                lpp.constraintTypes,
                solution.slack,
                solution.shadowPrice,
                solution.reducedCost,
                App.ElapsedMilliseconds - startTime,
                0, ' 可行解时间在Phase1中计算
                solutionLog.ToString,
                LPP.DecimalFormat
            )
        End Function

        ''' <summary>
        ''' 第一阶段：最小化人工变量之和
        ''' </summary>
        Private Function Phase1(showProgress As Boolean, log As StringBuilder) As LPPSolution
            ' 保存原始目标函数
            Dim originalObj = lpp.objectiveFunctionCoefficients.ToArray()
            Dim originalValue = lpp.objectiveFunctionValue

            ' 设置第一阶段目标：最小化人工变量之和
            For i = 0 To lpp.objectiveFunctionCoefficients.Count - 1
                lpp.objectiveFunctionCoefficients(i) = If(i >= lpp.originalVariableCount, 1, 0)
            Next
            lpp.objectiveFunctionValue = 0

            ' 初始基变量 = 人工变量
            Dim basicVars = Enumerable.Range(0, lpp.constraintRightHandSides.Length).ToList()

            ' 执行单纯形迭代
            Dim result = RunSimplexIteration(basicVars, New List(Of Integer), log, showProgress)
            If result IsNot Nothing Then Return result

            ' 检查可行性
            If std.Abs(lpp.objectiveFunctionValue) > EPSILON Then
                Return New LPPSolution("No feasible solution (phase 1 optimal value > 0)", log.ToString, 0)
            End If

            ' 恢复原始目标函数
            lpp.objectiveFunctionCoefficients.Clear()
            lpp.objectiveFunctionCoefficients.AddRange(originalObj)
            lpp.objectiveFunctionValue = originalValue

            Return Nothing
        End Function

        ''' <summary>
        ''' 第二阶段：求解原问题
        ''' </summary>
        Private Function Phase2(showProgress As Boolean, log As StringBuilder) As LPPSolution
            ' 重新计算目标函数行
            Dim basicVars = GetCurrentBasicVariables()
            For Each row In basicVars.Select(Function(v, i) New With {.var = v, .row = i})
                If row.var >= 0 Then
                    Pivot(row.var, row.row)
                End If
            Next

            ' 执行单纯形迭代
            Return RunSimplexIteration(basicVars, New List(Of Integer), log, showProgress)
        End Function

        ''' <summary>
        ''' 核心单纯形迭代逻辑
        ''' </summary>
        Private Function RunSimplexIteration(basicVars As List(Of Integer),
                                            artificialVars As List(Of Integer),
                                            log As StringBuilder,
                                            showProgress As Boolean) As LPPSolution
            Dim iteration = 0
            Do While iteration < LPP.PIVOT_ITERATION_LIMIT
                ' 选择入基变量
                Dim enterVar = ChooseEnteringVariable(artificialVars)
                If enterVar = -1 Then Exit Do ' 最优解

                ' 选择出基约束
                Dim leaveRow = ChooseLeavingConstraint(enterVar)
                If leaveRow = -1 Then
                    Return New LPPSolution("Unbounded solution detected", log.ToString, 0)
                End If

                ' 执行旋转
                Pivot(enterVar, leaveRow)
                basicVars(leaveRow) = enterVar
                log.AppendLine($"Pivot: x{enterVar + 1} in, row {leaveRow + 1} out")
                iteration += 1
            Loop

            If iteration = LPP.PIVOT_ITERATION_LIMIT Then
                Return New LPPSolution("Iteration limit exceeded", log.ToString, 0)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 改进的旋转操作（增加数值稳定性）
        ''' </summary>
        Public Sub Pivot(varIndex As Integer, constIndex As Integer)
            Dim pivotRow = lpp.constraintCoefficients(constIndex)
            Dim pivotElem = pivotRow(varIndex)

            ' 检查主元有效性
            If std.Abs(pivotElem) < EPSILON Then
                Throw New InvalidOperationException("Pivot element too small for numerical stability")
            End If

            ' 缩放主元行
            Dim scale = 1.0 / pivotElem
            For i = 0 To pivotRow.Count - 1
                pivotRow(i) *= scale
            Next
            lpp.constraintRightHandSides(constIndex) *= scale

            ' 消去其他行
            For j = 0 To lpp.constraintCoefficients.Length - 1
                If j <> constIndex Then
                    Dim row = lpp.constraintCoefficients(j)
                    Dim factor = row(varIndex)
                    If std.Abs(factor) > EPSILON Then
                        For i = 0 To row.Count - 1
                            row(i) -= factor * pivotRow(i)
                        Next
                        lpp.constraintRightHandSides(j) -= factor * lpp.constraintRightHandSides(constIndex)
                    End If
                End If
            Next

            ' 更新目标函数
            Dim objCoeff = lpp.objectiveFunctionCoefficients(varIndex)
            lpp.objectiveFunctionCoefficients(varIndex) = 0
            lpp.objectiveFunctionValue += objCoeff * lpp.constraintRightHandSides(constIndex)

            For i = 0 To lpp.objectiveFunctionCoefficients.Count - 1
                If i <> varIndex Then
                    lpp.objectiveFunctionCoefficients(i) -= objCoeff * pivotRow(i)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 获取当前基变量
        ''' </summary>
        Private Function GetCurrentBasicVariables() As List(Of Integer)
            Dim basicVars As New List(Of Integer)
            For j = 0 To lpp.constraintCoefficients.Length - 1
                Dim row = lpp.constraintCoefficients(j)
                Dim basicVar = -1
                For i = 0 To row.Count - 1
                    If std.Abs(row(i) - 1) < EPSILON Then
                        ' 验证单位列向量
                        Dim isUnit = True
                        For k = 0 To lpp.constraintCoefficients.Length - 1
                            If k <> j AndAlso std.Abs(lpp.constraintCoefficients(k)(i)) > EPSILON Then
                                isUnit = False
                                Exit For
                            End If
                        Next
                        If isUnit Then
                            basicVar = i
                            Exit For
                        End If
                    End If
                Next
                basicVars.Add(basicVar)
            Next
            Return basicVars
        End Function

        ''' <summary>
        ''' 移除人工变量
        ''' </summary>
        Private Sub RemoveArtificialVariables(artificialVars As List(Of Integer))
            Dim sortedVars = artificialVars.Where(Function(v) v >= 0).OrderByDescending(Function(v) v)
            For Each var In sortedVars
                lpp.variableNames.RemoveAt(var)
                lpp.objectiveFunctionCoefficients.RemoveAt(var)
                For j = 0 To lpp.constraintCoefficients.Length - 1
                    lpp.constraintCoefficients(j).RemoveAt(var)
                Next
            Next
        End Sub

        ''' <summary>
        ''' 提取解信息
        ''' </summary>
        Private Function ExtractSolution() As (optimalValues As Double(), slack As Double(), shadowPrice As Double(), reducedCost As Double())
            Dim n = lpp.originalVariableCount
            Dim m = lpp.constraintTypes.Length
            Dim optimalValues(n - 1) As Double
            Dim reducedCost(n - 1) As Double
            Dim slack(m - 1) As Double
            Dim shadowPrice(m - 1) As Double

            ' 获取基变量值
            Dim basicVars = GetCurrentBasicVariables()
            For i = 0 To n - 1
                If i < basicVars.Count AndAlso basicVars(i) >= 0 Then
                    optimalValues(i) = lpp.constraintRightHandSides(i)
                Else
                    optimalValues(i) = 0
                End If
                reducedCost(i) = lpp.objectiveFunctionCoefficients(i)
            Next

            ' 计算松弛变量和影子价格
            Dim varIndex = n
            For j = 0 To m - 1
                If lpp.constraintTypes(j) = "=" Then
                    slack(j) = 0
                    shadowPrice(j) = -lpp.objectiveFunctionCoefficients(varIndex)
                Else
                    slack(j) = lpp.constraintRightHandSides(j)
                    shadowPrice(j) = 0
                End If
                varIndex += 1
            Next

            Return (optimalValues, slack, shadowPrice, reducedCost)
        End Function

        ''' <summary>
        ''' 选择入基变量（改进检验数计算）
        ''' </summary>
        Private Function ChooseEnteringVariable(artificialVars As List(Of Integer)) As Integer
            Dim direction = If(lpp.objectiveFunctionType = OptimizationType.MAX, -1, 1)
            Dim bestVar = -1
            Dim bestValue = 0.0

            For i = 0 To lpp.objectiveFunctionCoefficients.Count - 1
                If Not artificialVars.Contains(i) Then
                    Dim rc = direction * lpp.objectiveFunctionCoefficients(i)
                    If rc < bestValue - EPSILON Then
                        bestValue = rc
                        bestVar = i
                    End If
                End If
            Next

            Return bestVar
        End Function

        ''' <summary>
        ''' 选择出基约束（改进最小比值规则）
        ''' </summary>
        Private Function ChooseLeavingConstraint(enterVar As Integer) As Integer
            Dim minRatio = Double.PositiveInfinity
            Dim minRow = -1

            For j = 0 To lpp.constraintRightHandSides.Length - 1
                Dim coeff = lpp.constraintCoefficients(j)(enterVar)
                If coeff > EPSILON Then
                    Dim ratio = lpp.constraintRightHandSides(j) / coeff
                    If ratio < minRatio - EPSILON OrElse (std.Abs(ratio - minRatio) < EPSILON AndAlso j < minRow) Then
                        minRatio = ratio
                        minRow = j
                    End If
                End If
            Next

            Return minRow
        End Function
    End Class
End Namespace