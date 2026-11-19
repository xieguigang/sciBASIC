Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports std = System.Math

Namespace BooleanNetworks

    ''' <summary>
    ''' 从二值化表达矩阵推断 PBN：逐基因为每个候选父集计算 CoD，
    ''' 以 CoD 为权重构造该基因的布尔规则集合与选择概率。
    ''' </summary>
    ''' <remarks>
    ''' 缺失值采用 Best-Fit Extension：拟合规则时跳过父值或目标值缺失的样本。
    ''' 同一基因的规则必须具有相同父节点数（<see cref="BooleanNode"/> 约束），
    ''' 因此按父集大小分组，每组独立估计选择概率。
    ''' </remarks>
    Public NotInheritable Class PbnInference

        Private ReadOnly _maxParents As Integer
        Private ReadOnly _minCod As Double
        Private ReadOnly _maxRulesPerNode As Integer

        ''' <summary>构造 PBN 推断器。</summary>
        Public Sub New(Optional maxParents As Integer = 3,
                       Optional minCod As Double = 0.05,
                       Optional maxRulesPerNode As Integer = 4)
            If maxParents < 0 OrElse maxParents > BooleanTruthTable.MaxMaskParents Then
                Throw New ArgumentOutOfRangeException(NameOf(maxParents))
            End If
            If minCod < 0.0 OrElse minCod > 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(minCod), minCod, "CoD threshold must be in [0,1]")
            End If
            If maxRulesPerNode < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(maxRulesPerNode))
            End If

            _maxParents = maxParents
            _minCod = minCod
            _maxRulesPerNode = maxRulesPerNode
        End Sub

        ''' <summary>最小 CoD 阈值。</summary>
        Public ReadOnly Property MinCod As Double
            Get
                Return _minCod
            End Get
        End Property

        ''' <summary>
        ''' 拟合 PBN；返回节点数组、每个基因的候选函数、CoD 与选择概率。
        ''' </summary>
        Public Function Fit(data As BooleanExpressionData, Optional seed As Integer = 12345) As PbnInferenceResult
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If

            Dim nodes(data.GeneCount - 1) As BooleanNode
            Dim parents(data.GeneCount - 1)()() As Integer
            Dim probabilities(data.GeneCount - 1)() As Double
            Dim cods(data.GeneCount - 1)() As Double
            Dim tables(data.GeneCount - 1)()() As Boolean

            For gene As Integer = 0 To data.GeneCount - 1
                Dim baselineCount As Integer = 0
                Dim baselineErrors As Integer = 0

                Call InferenceHelper.Baseline(data, gene, baselineCount, baselineErrors)

                Dim candidates As New List(Of FittedRule)()
                Dim limit As Integer = std.Min(_maxParents, data.GeneCount - 1)

                For k As Integer = 0 To limit
                    For Each candidate As Integer() In InferenceHelper.Combinations(data.GeneCount, k, gene)
                        Dim fitted As FittedRule = InferenceHelper.FitRule(data, gene, candidate)

                        If fitted Is Nothing OrElse fitted.UsableSamples = 0 Then
                            Continue For
                        End If

                        Dim cod As Double = InferenceHelper.ComputeCod(baselineErrors, fitted.Errors, fitted.UsableSamples)

                        If cod >= _minCod Then
                            Call candidates.Add(fitted)
                        End If
                    Next
                Next

                If candidates.Count = 0 Then
                    Dim emptyParents(-1) As Integer
                    Dim fallback As FittedRule = InferenceHelper.FitRule(data, gene, emptyParents)

                    If fallback Is Nothing Then
                        Dim truth(0) As Boolean
                        truth(0) = baselineErrors < baselineCount - baselineErrors
                        fallback = New FittedRule(emptyParents, truth, baselineCount - baselineErrors, baselineErrors, baselineCount)
                    End If

                    Call candidates.Add(fallback)
                End If

                Dim chosen As List(Of FittedRule) = SelectByParentSize(candidates)
                Dim ruleCount As Integer = std.Min(chosen.Count, _maxRulesPerNode)
                Dim functions(ruleCount - 1) As IBooleanRule
                Dim probs(ruleCount - 1) As Double
                Dim codValues(ruleCount - 1) As Double
                Dim parentSets(ruleCount - 1)() As Integer
                Dim truthTables(ruleCount - 1)() As Boolean
                Dim total As Double = 0.0

                For i As Integer = 0 To ruleCount - 1
                    Dim rule As FittedRule = chosen(i)
                    Dim cod As Double = InferenceHelper.ComputeCod(baselineErrors, rule.Errors, rule.UsableSamples)

                    total += cod
                    functions(i) = New BooleanTruthTable(rule.Parents.Length, rule.TruthTable)
                    codValues(i) = cod
                    parentSets(i) = rule.Parents
                    truthTables(i) = rule.TruthTable
                Next

                For i As Integer = 0 To ruleCount - 1
                    probs(i) = If(total > 0.0, codValues(i) / total, 1.0 / ruleCount)
                Next

                ' 归一化抵消浮点误差，确保和为 1
                Dim sum As Double = 0.0

                For i As Integer = 0 To probs.Length - 1
                    sum += probs(i)
                Next

                For i As Integer = 0 To probs.Length - 1
                    probs(i) /= sum
                Next

                Dim lastParents As Integer() = parentSets(0)

                nodes(gene) = New BooleanNode(data.GeneNames(gene), lastParents, functions, probs)
                parents(gene) = parentSets
                probabilities(gene) = probs
                cods(gene) = codValues
                tables(gene) = truthTables
            Next

            Return New PbnInferenceResult(nodes, parents, probabilities, cods, tables, seed)
        End Function

        ''' <summary>优先选中规则数最多且 CoD 总量最高的父集大小分组。</summary>
        Private Shared Function SelectByParentSize(candidates As List(Of FittedRule)) As List(Of FittedRule)
            Dim groups As New Dictionary(Of Integer, List(Of FittedRule))()

            For Each rule As FittedRule In candidates
                Dim key As Integer = rule.Parents.Length

                If Not groups.ContainsKey(key) Then
                    groups(key) = New List(Of FittedRule)()
                End If

                Call groups(key).Add(rule)
            Next

            Dim best As List(Of FittedRule) = Nothing
            Dim bestScore As Double = -1.0

            For Each group As List(Of FittedRule) In groups.Values
                Dim score As Double = group.Count

                If score > bestScore Then
                    bestScore = score
                    best = group
                End If
            Next

            Return best
        End Function
    End Class

    ''' <summary>PBN 推断结果。</summary>
    Public NotInheritable Class PbnInferenceResult

        Private ReadOnly _nodes As BooleanNode()
        Private ReadOnly _parents As Integer()()()
        Private ReadOnly _probabilities As Double()()
        Private ReadOnly _cods As Double()()
        Private ReadOnly _truthTables As Boolean()()()
        Private ReadOnly _seed As Integer

        ''' <summary>构造推断结果。</summary>
        Public Sub New(nodes As BooleanNode(), parents As Integer()()(), probabilities As Double()(),
                       cods As Double()(), truthTables As Boolean()()(), seed As Integer)
            _nodes = nodes
            _parents = parents
            _probabilities = probabilities
            _cods = cods
            _truthTables = truthTables
            _seed = seed
        End Sub

        ''' <summary>拟合得到的节点数组。</summary>
        Public ReadOnly Property Nodes As BooleanNode()
            Get
                Return DirectCast(_nodes.Clone(), BooleanNode())
            End Get
        End Property

        ''' <summary>每个基因的候选父集（逐函数）。</summary>
        Public ReadOnly Property Parents As Integer()()()
            Get
                Return DirectCast(_parents.Clone(), Integer()()())
            End Get
        End Property

        ''' <summary>每个基因各函数的选择概率。</summary>
        Public ReadOnly Property Probabilities As Double()()
            Get
                Return DirectCast(_probabilities.Clone(), Double()())
            End Get
        End Property

        ''' <summary>每个基因各函数的 CoD。</summary>
        Public ReadOnly Property Cods As Double()()
            Get
                Return DirectCast(_cods.Clone(), Double()())
            End Get
        End Property

        ''' <summary>每个基因各函数的真值表。</summary>
        Public ReadOnly Property TruthTables As Boolean()()()
            Get
                Return DirectCast(_truthTables.Clone(), Boolean()()())
            End Get
        End Property

        ''' <summary>用于可复现实验的随机种子。</summary>
        Public ReadOnly Property Seed As Integer
            Get
                Return _seed
            End Get
        End Property
    End Class

End Namespace
