' ============================================================================
' MilpSolution.vb — MILP 求解结果
' ----------------------------------------------------------------------------
' 统一承载：
'   · 状态（最优/不可行/无界/达到时间·节点·间隙上限/数值错误）
'   · 原始问题空间的解向量与目标值
'   · 最优界与相对间隙（B&B 的收敛性证据）
'   · 求解统计（节点数、LP 次数、割平面数、启发式命中次数、耗时）
'   · 逐阶段日志与失败说明
'
' 报告量约定（与 LPPSolution 语义一致）：
'   ObjectiveValue 始终是"原始方向"的目标值（max 问题报 max 值）；
'   BestBound 是原始方向下的最优界（min 问题为下界，max 问题为上界）。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports System.Text
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>
    ''' MILP 求解状态。
    ''' </summary>
    Public Enum MilpStatus

        ''' <summary>已证明最优</summary>
        Optimal = 0
        ''' <summary>不可行</summary>
        Infeasible = 1
        ''' <summary>无界</summary>
        Unbounded = 2
        ''' <summary>达到时间上限（返回当前最优可行解与界）</summary>
        TimeLimit = 3
        ''' <summary>达到节点上限</summary>
        NodeLimit = 4
        ''' <summary>达到间隙上限（解与界都可用）</summary>
        GapLimit = 5
        ''' <summary>数值错误</summary>
        [Error] = 6

    End Enum

    ''' <summary>
    ''' MILP 求解结果。
    ''' </summary>
    Public Class MilpSolution

        Public ReadOnly Property Status As MilpStatus
        ''' <summary>原始问题空间的解（整数变量已取整）；无可行解时为 Nothing</summary>
        Public ReadOnly Property Solution As Double()
        Public ReadOnly Property VariableNames As String()
        ''' <summary>原始方向的目标值；无可行解时无意义</summary>
        Public ReadOnly Property ObjectiveValue As Double
        ''' <summary>原始方向的最优界（分支定界全局界）；不可行/无界时无意义</summary>
        Public ReadOnly Property BestBound As Double
        ''' <summary>相对间隙 |incumbent − bound| / max(1, |incumbent|)</summary>
        Public ReadOnly Property RelativeGap As Double
        ''' <summary>是否已经找到至少一个整数可行解</summary>
        Public ReadOnly Property HasIncumbent As Boolean

        Public Property NodesExplored As Integer
        Public Property LpSolves As Integer
        Public Property CutsAdded As Integer
        Public Property HeuristicSolutions As Integer
        ''' <summary>因 LP 数值失败（冷启动也无法求解）而被丢弃的子树数量；&gt; 0 时最优性未被证明</summary>
        Public Property DroppedNodes As Integer = 0
        ''' <summary>根节点 LP 松弛在原始方向下的目标值（Nothing 表示未求解）</summary>
        Public Property RootRelaxation As Double? = Nothing
        ''' <summary>是否在根节点即得到整数可行解</summary>
        Public Property IncumbentAtRoot As Boolean = False

        Public Property ElapsedMilliseconds As Long = 0
        Public Property Log As String = ""
        Public Property FailureMessage As String = ""

        ''' <summary>
        ''' 底层 LP 结果（用于打印影子价 / reduced cost 等敏感性信息）；可能为 Nothing。
        ''' </summary>
        Public Property Lp As LPPSolution = Nothing

        Public Sub New(status As MilpStatus,
                       solution As Double(),
                       variableNames As String(),
                       objectiveValue As Double,
                       bestBound As Double,
                       relativeGap As Double)

            Me.Status = status
            Me.Solution = solution
            Me.VariableNames = variableNames
            Me.ObjectiveValue = objectiveValue
            Me.BestBound = bestBound
            Me.RelativeGap = relativeGap
            Me.HasIncumbent = solution IsNot Nothing
        End Sub

        ''' <summary>
        ''' 求解是否失败（没有可用的解，且状态不是"最优"）。
        ''' </summary>
        Public ReadOnly Property SolverError As Boolean
            Get
                If Status = MilpStatus.Optimal Then Return False
                If Status = MilpStatus.GapLimit Then Return Not HasIncumbent
                If Status = MilpStatus.TimeLimit OrElse Status = MilpStatus.NodeLimit Then Return Not HasIncumbent
                Return True
            End Get
        End Property

        ''' <summary>按变量名取解分量。</summary>
        Public Function GetSolution(name As String) As Double
            If Solution Is Nothing Then Return Double.NaN

            For i As Integer = 0 To VariableNames.Length - 1
                If VariableNames(i) = name Then Return Solution(i)
            Next

            Return Double.NaN
        End Function

        Public Function StatusText() As String
            Select Case Status
                Case MilpStatus.Optimal : Return "最优（已证明）"
                Case MilpStatus.Infeasible : Return "不可行（无整数可行解）"
                Case MilpStatus.Unbounded : Return "无界"
                Case MilpStatus.TimeLimit : Return "达到时间上限"
                Case MilpStatus.NodeLimit : Return "达到节点上限"
                Case MilpStatus.GapLimit : Return "达到间隙上限"
                Case Else : Return "数值错误"
            End Select
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder()
            Dim fmt As String = "G6"

            sb.AppendLine($"MILP 状态: {StatusText()}")

            If Not FailureMessage.StringEmpty Then
                sb.AppendLine($"说明: {FailureMessage}")
            End If

            If Solution IsNot Nothing Then
                sb.AppendLine($"目标值: {ObjectiveValue.ToString(fmt)}")
                sb.AppendLine($"最优界: {BestBound.ToString(fmt)}   相对间隙: {RelativeGap.ToString("E3")}")

                If RootRelaxation.HasValue Then
                    sb.AppendLine($"根松弛值: {RootRelaxation.Value.ToString(fmt)}")
                End If

                sb.AppendLine("变量取值:")
                For i As Integer = 0 To Solution.Length - 1
                    sb.AppendLine($"  {VariableNames(i)} = {Solution(i).ToString(fmt)}")
                Next
            End If

            sb.AppendLine($"统计: 节点 {NodesExplored}，LP 求解 {LpSolves}，割平面 {CutsAdded}，" &
                          $"启发式可行解 {HeuristicSolutions}，耗时 {ElapsedMilliseconds} ms")

            If DroppedNodes > 0 Then
                sb.AppendLine($"警告: {DroppedNodes} 个节点因 LP 数值失败被丢弃，最优性未被证明。")
            End If

            Return sb.ToString()
        End Function

    End Class

End Namespace
