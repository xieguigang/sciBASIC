#Region "Microsoft.VisualBasic::d2af1668e5dd842b6f5d0453e2952d19, Data_science\Mathematica\Math\Math\Algebra\MILP\MilpOptions.vb"

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

    '   Total Lines: 140
    '    Code Lines: 57 (40.71%)
    ' Comment Lines: 51 (36.43%)
    '    - Xml Docs: 78.43%
    ' 
    '   Blank Lines: 32 (22.86%)
    '     File Size: 6.16 KB


    '     Enum BranchRule
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum NodeRule
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class MilpOptions
    ' 
    '         Properties: AbsoluteGap, Branch, CutCoefficientLimit, CutDensityLimit, DecimalFormat
    '                     DivingDepthLimit, EnableCuts, EnableHeuristics, EnablePresolve, FeasibilityTolerance
    '                     IntegerTolerance, LpIterationLimit, MaxCutsPerRound, MaxNodes, MaxOpenNodes
    '                     MaxSeconds, Node, RelativeGap, RootCutRounds, Verbose
    ' 
    '         Function: Clone
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' MilpOptions.vb — MILP 求解控制选项与枚举
' ----------------------------------------------------------------------------
' 统一承载求解过程的终止条件、算法开关与数值容差，避免把大量参数散落到
' 分支定界 / 割平面 / 单纯形各层。所有时间单位为秒，所有"间隙"为相对量。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover

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

        ''' <summary>
        ''' 是否启用多线程并行加速（割平面逐行生成、预处理行扫描、大矩阵 LU 消元、
        ''' 分支定界并行节点求解等）。SIMD 向量化不受该开关影响，始终启用。
        ''' 默认关闭以保持既有串行行为。
        ''' </summary>
        Public Property EnableParallel As Boolean = False

        ''' <summary>并行最大线程数；0 = 自动（CPU 逻辑核心数）。默认 0。</summary>
        Public Property MaxThreads As Integer = 0

        ''' <summary>
        ''' 是否启用基矩阵 LU 增量更新（产品形式 η 修正，换基 O(m²)）。
        ''' 关闭时退回「每次换基完整重构 LU」的既有路径 —— 既是 A/B benchmark
        ''' 的对照组，也是一键回退开关。默认开启。
        ''' </summary>
        Public Property EnableLuUpdate As Boolean = True

        ''' <summary>
        ''' 两轮完整重构之间允许的最大换基次数（η 条数上限），达到即强制完整重构。
        ''' 兼作周期性重构的节奏控制。默认 60。
        ''' </summary>
        Public Property LuMaxUpdates As Integer = 60

        ''' <summary>LU 更新主元分母 |1 + w_p| 的下限，低于该值拒绝本次更新并重构。默认 1e-12。</summary>
        Public Property LuPivotTolerance As Double = 0.000000000001

        ''' <summary>LU 更新 η 向量无穷范数上限，超过该值拒绝本次更新并重构。默认 1e6。</summary>
        Public Property LuMaxEtaNorm As Double = 1000000.0

        ''' <summary>把 LU 更新相关选项转换成共享层的 <see cref="LuUpdateOptions"/>。</summary>
        Friend Function ToLuUpdateOptions() As LuUpdateOptions
            Return New LuUpdateOptions With {
                .Enabled = EnableLuUpdate,
                .MaxUpdates = System.Math.Max(1, LuMaxUpdates),
                .PivotTolerance = LuPivotTolerance,
                .MaxEtaNorm = LuMaxEtaNorm
            }
        End Function

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
                .EnableParallel = EnableParallel,
                .MaxThreads = MaxThreads,
                .EnableLuUpdate = EnableLuUpdate,
                .LuMaxUpdates = LuMaxUpdates,
                .LuPivotTolerance = LuPivotTolerance,
                .LuMaxEtaNorm = LuMaxEtaNorm,
                .DecimalFormat = DecimalFormat
            }
        End Function

    End Class

End Namespace
