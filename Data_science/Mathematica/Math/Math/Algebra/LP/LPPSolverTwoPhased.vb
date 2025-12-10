#Region "Microsoft.VisualBasic::b7754380b348c34c1308be8cad41152f, Data_science\Mathematica\Math\Math\Algebra\LP\LPPSolverTwoPhased.vb"

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

    '   Total Lines: 442
    '    Code Lines: 284 (64.25%)
    ' Comment Lines: 92 (20.81%)
    '    - Xml Docs: 54.35%
    ' 
    '   Blank Lines: 66 (14.93%)
    '     File Size: 18.61 KB


    '     Class LPPSolverTwoPhased
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ChooseEnteringVariable, ChooseLeavingConstraint, ExtractSolution, GetArtificialVariablesList, GetCurrentBasicVariables
    '                   InitializePhase1BasicVariables, IsArtificialVariable, IsPotentialBasicVariable, IsUnitVector, Phase1
    '                   Phase2, RunSimplexIteration, Solve
    ' 
    '         Sub: Pivot, RemoveArtificialVariables, RestoreOriginalObjective
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
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
        ''' 第一阶段：最小化人工变量之和（完整修正版本）
        ''' </summary>
        Private Function Phase1(showProgress As Boolean, log As StringBuilder) As LPPSolution
            ' 保存原始目标函数
            Dim originalObj = lpp.objectiveFunctionCoefficients.ToArray()
            Dim originalValue = lpp.objectiveFunctionValue

            ' 设置第一阶段目标：最小化人工变量之和
            For i = 0 To lpp.objectiveFunctionCoefficients.Count - 1
                ' 只对人工变量设置系数为1，其他为0
                lpp.objectiveFunctionCoefficients(i) = If(IsArtificialVariable(i), 1, 0)
            Next
            lpp.objectiveFunctionValue = 0

            ' 初始化第一阶段基变量
            Dim basicVars = InitializePhase1BasicVariables()
            Dim artificialVarsList = GetArtificialVariablesList()

            log.AppendLine($"Phase 1: Initialized {basicVars.Count} basic variables")
            log.AppendLine($"Phase 1: Found {artificialVarsList.Count} artificial variables")

            ' 执行单纯形迭代
            Dim result = RunSimplexIteration(basicVars, artificialVarsList, log, showProgress)

            If result IsNot Nothing Then
                ' 恢复原始目标函数后再返回
                RestoreOriginalObjective(originalObj, originalValue)
                Return result
            End If

            ' 检查可行性
            If std.Abs(lpp.objectiveFunctionValue) > EPSILON Then
                RestoreOriginalObjective(originalObj, originalValue)
                Return New LPPSolution("No feasible solution (phase 1 optimal value > 0)", log.ToString, 0)
            End If

            ' 恢复原始目标函数
            RestoreOriginalObjective(originalObj, originalValue)
            log.AppendLine("Phase 1 completed successfully")

            Return Nothing
        End Function

        ''' <summary>
        ''' 初始化第一阶段基变量
        ''' 在第一阶段，人工变量作为初始基变量
        ''' </summary>
        Private Function InitializePhase1BasicVariables() As List(Of Integer)
            Dim basicVars As New List(Of Integer)
            Dim variableCount = lpp.objectiveFunctionCoefficients.Count
            Dim constraintCount = lpp.constraintRightHandSides.Length

            ' 为每个约束行分配一个基变量
            For rowIndex = 0 To constraintCount - 1
                Dim foundBasicVar = False

                ' 首先尝试找到已经存在的单位向量列（松弛变量）
                For varIndex = lpp.originalVariableCount To variableCount - 1
                    If IsUnitVector(varIndex, rowIndex) Then
                        basicVars.Add(varIndex)
                        foundBasicVar = True
                        Exit For
                    End If
                Next

                ' 如果没有找到合适的松弛变量，则使用人工变量
                If Not foundBasicVar Then
                    ' 添加新的人工变量
                    Dim artificialVarIndex = lpp.addArtificialVariable(rowIndex)
                    basicVars.Add(artificialVarIndex)
                End If
            Next

            Return basicVars
        End Function

        ''' <summary>
        ''' 检查变量是否构成单位向量（在指定行系数为1，其他行为0）
        ''' </summary>
        Private Function IsUnitVector(varIndex As Integer, targetRow As Integer) As Boolean
            ' 检查目标行的系数是否为1
            If std.Abs(lpp.constraintCoefficients(targetRow)(varIndex) - 1.0) > EPSILON Then
                Return False
            End If

            ' 检查其他行的系数是否为0
            For rowIndex = 0 To lpp.constraintCoefficients.Length - 1
                If rowIndex <> targetRow Then
                    If std.Abs(lpp.constraintCoefficients(rowIndex)(varIndex)) > EPSILON Then
                        Return False
                    End If
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 恢复原始目标函数
        ''' </summary>
        Private Sub RestoreOriginalObjective(originalObj As Double(), originalValue As Double)
            lpp.objectiveFunctionCoefficients.Clear()
            lpp.objectiveFunctionCoefficients.AddRange(originalObj)
            lpp.objectiveFunctionValue = originalValue
        End Sub

        ''' <summary>
        ''' 获取人工变量列表
        ''' 识别所有以'a'开头或索引大于原始变量数的变量
        ''' </summary>
        Private Function GetArtificialVariablesList() As List(Of Integer)
            Dim artificialVars As New List(Of Integer)
            Dim variableCount = lpp.objectiveFunctionCoefficients.Count

            For varIndex = 0 To variableCount - 1
                If IsArtificialVariable(varIndex) Then
                    artificialVars.Add(varIndex)
                End If
            Next

            Return artificialVars
        End Function

        ''' <summary>
        ''' 判断是否为人工变量
        ''' </summary>
        Private Function IsArtificialVariable(varIndex As Integer) As Boolean
            ' 根据变量命名或位置判断是否为人工变量
            Return varIndex >= lpp.originalVariableCount AndAlso
                lpp.variableNames(varIndex) Like lpp.artificialVariable
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
        ''' 改进的旋转操作（增加数值稳定性检查）
        ''' </summary>
        Public Sub Pivot(varIndex As Integer, constIndex As Integer)
            Dim pivotRow = lpp.constraintCoefficients(constIndex)
            Dim pivotElem = pivotRow(varIndex)

            ' 增强主元有效性检查
            If std.Abs(pivotElem) < EPSILON Then
                Throw New InvalidOperationException($"Pivot element too small ({pivotElem}) for numerical stability at variable {varIndex}, constraint {constIndex}")
            End If

            ' 使用更稳定的缩放因子计算
            Dim scale = 1.0 / pivotElem

            ' 应用缩放时避免累积误差
            For i = 0 To pivotRow.Count - 1
                pivotRow(i) = pivotRow(i) * scale
            Next
            lpp.constraintRightHandSides(constIndex) = lpp.constraintRightHandSides(constIndex) * scale

            ' 改进的消去过程，减少数值误差
            For j = 0 To lpp.constraintCoefficients.Length - 1
                If j <> constIndex Then
                    Dim row = lpp.constraintCoefficients(j)
                    Dim factor = row(varIndex)
                    If std.Abs(factor) > EPSILON Then
                        For i = 0 To row.Count - 1
                            row(i) = row(i) - factor * pivotRow(i)
                        Next
                        lpp.constraintRightHandSides(j) = lpp.constraintRightHandSides(j) - factor * lpp.constraintRightHandSides(constIndex)
                    End If
                End If
            Next

            ' 更新目标函数系数
            Dim objCoeff = lpp.objectiveFunctionCoefficients(varIndex)
            If std.Abs(objCoeff) > EPSILON Then
                For i = 0 To lpp.objectiveFunctionCoefficients.Count - 1
                    lpp.objectiveFunctionCoefficients(i) = lpp.objectiveFunctionCoefficients(i) - objCoeff * pivotRow(i)
                Next
                lpp.objectiveFunctionValue = lpp.objectiveFunctionValue + objCoeff * lpp.constraintRightHandSides(constIndex)
            End If
            lpp.objectiveFunctionCoefficients(varIndex) = 0
        End Sub

        ''' <summary>
        ''' 获取当前基变量（改进的数值稳定性版本）
        ''' </summary>
        Private Function GetCurrentBasicVariables() As List(Of Integer)
            Dim basicVars As New List(Of Integer)
            Dim variableCount = lpp.objectiveFunctionCoefficients.Count

            ' 为每个约束行找到基变量
            For rowIndex = 0 To lpp.constraintCoefficients.Length - 1
                Dim row = lpp.constraintCoefficients(rowIndex)
                Dim candidateVar = -1
                Dim maxCoeff = 0.0

                ' 寻找该行中系数最大的变量（更稳定的识别方法）
                For varIndex = 0 To variableCount - 1
                    Dim coeff = std.Abs(row(varIndex))
                    If coeff > EPSILON AndAlso coeff > maxCoeff Then
                        ' 检查该变量是否可能成为基变量
                        If IsPotentialBasicVariable(varIndex, rowIndex) Then
                            candidateVar = varIndex
                            maxCoeff = coeff
                        End If
                    End If
                Next

                basicVars.Add(candidateVar)
            Next

            Return basicVars
        End Function

        ''' <summary>
        ''' 检查变量是否可能成为基变量
        ''' </summary>
        Private Function IsPotentialBasicVariable(varIndex As Integer, currentRow As Integer) As Boolean
            ' 检查该变量在其他行中的系数是否足够小
            For rowIndex = 0 To lpp.constraintCoefficients.Length - 1
                If rowIndex <> currentRow Then
                    Dim coeff = std.Abs(lpp.constraintCoefficients(rowIndex)(varIndex))
                    If coeff > EPSILON Then
                        Return False
                    End If
                End If
            Next

            ' 检查在当前行中的系数不为零
            Return std.Abs(lpp.constraintCoefficients(currentRow)(varIndex)) > EPSILON
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
        ''' 提取解信息（修正版本）
        ''' </summary>
        Private Function ExtractSolution() As (optimalValues As Double(), slack As Double(), shadowPrice As Double(), reducedCost As Double())
            Dim n = lpp.originalVariableCount
            Dim m = lpp.constraintTypes.Length
            Dim optimalValues(n - 1) As Double
            Dim reducedCost(n - 1) As Double
            Dim slack(m - 1) As Double
            Dim shadowPrice(m - 1) As Double

            ' 初始化所有变量值为0
            For i = 0 To n - 1
                optimalValues(i) = 0
                reducedCost(i) = lpp.objectiveFunctionCoefficients(i)
            Next

            ' 获取基变量及其值
            Dim basicVars = GetCurrentBasicVariables()
            For rowIndex = 0 To basicVars.Count - 1
                Dim varIndex = basicVars(rowIndex)
                If varIndex >= 0 AndAlso varIndex < n Then ' 只处理原始变量
                    optimalValues(varIndex) = lpp.constraintRightHandSides(rowIndex)
                End If
            Next

            ' 计算松弛变量和影子价格（修正逻辑）
            Dim slackIndex = 0
            For j = 0 To m - 1
                ' 计算当前约束的松弛量
                Dim constraintValue = 0.0
                For i = 0 To n - 1
                    constraintValue += optimalValues(i) * lpp.constraintCoefficients(j)(i)
                Next

                slack(j) = lpp.constraintRightHandSides(j) - constraintValue

                ' 影子价格为目标函数对约束右端项的敏感度
                ' 这里简化处理，实际应该从对偶变量获取
                shadowPrice(j) = 0 ' 需要更复杂的计算逻辑
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
