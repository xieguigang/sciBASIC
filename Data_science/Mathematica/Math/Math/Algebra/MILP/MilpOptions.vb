' ============================================================================
' MilpOptions.vb — MILP 求解控制选项与枚举
' ----------------------------------------------------------------------------
' 统一承载求解过程的终止条件、算法开关与数值容差，避免把大量参数散落到
' 分支定界 / 割平面 / 单纯形各层。所有时间单位为秒，所有"间隙"为相对量。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>
    ''' 分支变量选择规则。
    ''' </summary>
    Public Enum BranchRule

        ''' <summary>选择小数部分最接近 0.5 的整数变量（most-fractional）</summary>
        MostFractional = 0
        ''' <summary>按列顺序选择第一个分数变量（first-fractional）</summary>
        FirstFractional = 1
        ''' <summary>按伪成本（历史分支收益）选择</summary>
        PseudoCost = 2

    End Enum

    ''' <summary>
    ''' 节点选择规则。
    ''' </summary>
    Public Enum NodeRule

        ''' <summary>最佳界优先（best-bound）：优先扩展松弛界最好的节点</summary>
        BestBound = 0
        ''' <summary>深度优先：优先深入，快速获得整数可行解</summary>
        DepthFirst = 1

    End Enum

    ''' <summary>
    ''' MILP 求解选项。
    ''' </summary>
    Public Class MilpOptions

        ''' <summary>时间上限（秒）。默认 60。</summary>
        Public Property MaxSeconds As Double = 60.0

        ''' <summary>节点数量上限。默认 200000。</summary>
        Public Property MaxNodes As Integer = 200000

        ''' <summary>
        ''' 开集（未扩展节点）数量上限，用于内存保护（每个节点保存一份 l/u）。
        ''' 超过该值以 NodeLimit 终止。默认 20000。
        ''' </summary>
        Public Property MaxOpenNodes As Integer = 20000

        ''' <summary>相对间隙容差：|incumbent − bound| / max(1, |incumbent|) ≤ 该值即提前终止。默认 1e-4。</summary>
        Public Property RelativeGap As Double = 0.0001

        ''' <summary>绝对间隙容差。默认 1e-6。</summary>
        Public Property AbsoluteGap As Double = 0.000001

        ''' <summary>整数判定容差（用于分支变量挑选与整数可行性判定）。默认 1e-6。</summary>
        Public Property IntegerTolerance As Double = 0.000001

        ''' <summary>原始/对偶可行性容差（传给 BoundedSimplex）。默认 1e-7。</summary>
        Public Property FeasibilityTolerance As Double = 0.0000001

        ''' <summary>单次 LP 迭代上限。默认 20000。</summary>
        Public Property LpIterationLimit As Integer = 20000

        ' ---------------------------------------------------------------
        ' 算法开关
        ' ---------------------------------------------------------------

        ''' <summary>是否启用预处理（默认启用）。</summary>
        Public Property EnablePresolve As Boolean = True

        ''' <summary>是否启用 Gomory 割平面（默认启用）。</summary>
        Public Property EnableCuts As Boolean = True

        ''' <summary>是否启用整数启发式（舍入 / 潜水，默认启用）。</summary>
        Public Property EnableHeuristics As Boolean = True

        ''' <summary>根节点割平面轮数上限。默认 6。</summary>
        Public Property RootCutRounds As Integer = 6

        ''' <summary>每轮割平面最多新增的条数。默认 30。</summary>
        Public Property MaxCutsPerRound As Integer = 30

        ''' <summary>割平面系数尺度阈值：|系数| 超过该值视为数值不可靠而丢弃该割。默认 1e9。</summary>
        Public Property CutCoefficientLimit As Double = 1000000000.0

        ''' <summary>割平面密度上限：新割非零元比例超过该值则丢弃。默认 0.9。</summary>
        Public Property CutDensityLimit As Double = 0.9

        ''' <summary>潜水启发式最多执行的固定变量步数。默认 50。</summary>
        Public Property DivingDepthLimit As Integer = 50

        ''' <summary>分支变量选择规则。默认 most-fractional。</summary>
        Public Property Branch As BranchRule = BranchRule.MostFractional

        ''' <summary>节点选择规则。默认 best-bound。</summary>
        Public Property Node As NodeRule = NodeRule.BestBound

        ''' <summary>是否输出详细日志（求解进度）。默认关闭。</summary>
        Public Property Verbose As Boolean = False

        ''' <summary>数值输出格式。</summary>
        Public Property DecimalFormat As String = "G6"

        ''' <summary>
        ''' 复制一份选项（避免调用方后续修改影响进行中的求解）。
        ''' </summary>
        Public Function Clone() As MilpOptions
            Return New MilpOptions With {
                .MaxSeconds = MaxSeconds,
                .MaxNodes = MaxNodes,
                .MaxOpenNodes = MaxOpenNodes,
                .RelativeGap = RelativeGap,
                .AbsoluteGap = AbsoluteGap,
                .IntegerTolerance = IntegerTolerance,
                .FeasibilityTolerance = FeasibilityTolerance,
                .LpIterationLimit = LpIterationLimit,
                .EnablePresolve = EnablePresolve,
                .EnableCuts = EnableCuts,
                .EnableHeuristics = EnableHeuristics,
                .RootCutRounds = RootCutRounds,
                .MaxCutsPerRound = MaxCutsPerRound,
                .CutCoefficientLimit = CutCoefficientLimit,
                .CutDensityLimit = CutDensityLimit,
                .DivingDepthLimit = DivingDepthLimit,
                .Branch = Branch,
                .Node = Node,
                .Verbose = Verbose,
                .DecimalFormat = DecimalFormat
            }
        End Function

    End Class

End Namespace
