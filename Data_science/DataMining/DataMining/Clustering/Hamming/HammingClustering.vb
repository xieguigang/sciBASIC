Imports System.Collections.Generic
Imports System.Linq

Namespace Hamming

    ''' <summary>
    ''' Muselli–Liberati 的 Hamming Clustering（HC）监督式规则提取。
    ''' 
    ''' 该算法面向二值输入的分类问题，目标是构建一个仅含 AND / OR / NOT 门的逻辑网络，
    ''' 既拟合训练集、又能还原潜在的布尔函数。核心步骤：
    ''' 
    ''' 1. **按类聚合**：把属于同一类别的输入模式归到一起；
    ''' 2. **簇生成**：在每个类别内部，把彼此 Hamming 距离不超过 ``theta``
    '''    （即在多数位上一致）的样本聚成一组；
    ''' 3. **合取规则提取**：对每个簇，把所有样本取值相同的位固定下来，
    '''    构成一条合取规则（AND 项）；
    ''' 4. **析取合并**：用 OR 把同类的多条合取规则组合成该类的判别函数；
    ''' 5. **剪枝**：在不破坏「训练集完全一致」的前提下逐位放开字面量，
    '''    降低规则复杂度并提升泛化能力；
    ''' 6. **逻辑网络**：输出仅由 AND / OR / NOT 构成的可解释判别网络
    '''    （见 <see cref="RuleSet"/>）。
    ''' </summary>
    Public Class HammingClusteringEngine

        ''' <summary>
        ''' 在二值模式集合上提取规则。
        ''' </summary>
        ''' <param name="patterns">训练集的位模式（等长）</param>
        ''' <param name="labels">训练集的类别标签，与 <paramref name="patterns"/> 一一对应</param>
        ''' <param name="theta">
        ''' 类内簇生成的距离阈值。默认值 ``NaN`` 表示自动取 1，
        ''' 即「只允许有一位不同」的样本聚成同一个簇。
        ''' </param>
        ''' <param name="prune">是否执行保持训练集一致的贪心剪枝</param>
        ''' <param name="minCoverage">一个簇至少需要包含的样本数量，缺省为 1（即不做过滤）</param>
        ''' <returns>包含训练集精度报告的规则集合</returns>
        Public Function Run(patterns As BitPattern(),
                            labels As Integer(),
                            Optional theta As Double = Double.NaN,
                            Optional prune As Boolean = True,
                            Optional minCoverage As Integer = 1) As RuleSet

            If patterns Is Nothing Then
                Throw New ArgumentNullException(NameOf(patterns))
            End If
            If labels Is Nothing Then
                Throw New ArgumentNullException(NameOf(labels))
            End If
            If patterns.Length = 0 Then
                Throw New ArgumentException("the input data set is empty!", NameOf(patterns))
            End If
            If patterns.Length <> labels.Length Then
                Throw New ArgumentException($"the patterns count {patterns.Length} is not equal to the labels count {labels.Length}!", NameOf(labels))
            End If
            If minCoverage < 1 Then
                Throw New ArgumentException($"the minimum coverage must be a positive integer, but {minCoverage}!", NameOf(minCoverage))
            End If

            Dim width As Integer = patterns(0).BitLength

            For i As Integer = 0 To patterns.Length - 1
                If patterns(i) Is Nothing Then
                    Throw New ArgumentException($"the pattern at index {i} is null!", NameOf(patterns))
                End If
                If patterns(i).BitLength <> width Then
                    Throw New ArgumentException($"the pattern at index {i} has {patterns(i).BitLength} bits, but the first pattern has {width} bits!", NameOf(patterns))
                End If
            Next

            Dim thetaValue As Double = If(Double.IsNaN(theta), 1.0, theta)

            If thetaValue < 0 Then
                Throw New ArgumentException($"the distance threshold cannot be negative, but {theta}!", NameOf(theta))
            End If

            Dim classLabels As Integer() = labels.Distinct().OrderBy(Function(l) l).ToArray()

            If classLabels.Length = 1 Then
                ' 单类别：任何位模式都归为该类，判别函数退化为恒真的合取项
                Dim all As Integer() = Enumerable.Range(0, patterns.Length).ToArray
                Dim constant As New ConjunctionRule(New Literal() {}, classLabels(0), all)
                Dim constantRules As New RuleSet({New ClassRule(classLabels(0), {constant})})

                constantRules.Report = RuleSetReport.Build(constantRules, patterns, labels)

                Return constantRules
            End If

            Dim classRules As New List(Of ClassRule)()

            For Each label As Integer In classLabels
                Dim members As Integer() = Enumerable _
                    .Range(0, patterns.Length) _
                    .Where(Function(i) labels(i) = label) _
                    .ToArray
                Dim generated As New List(Of ConjunctionRule)()
                Dim covered As New HashSet(Of Integer)()

                For Each seedIndex As Integer In members
                    If covered.Contains(seedIndex) Then
                        Continue For
                    End If

                    Dim cluster As New List(Of Integer)()
                    Call cluster.Add(seedIndex)

                    Dim literals As Literal() = FixedLiterals(patterns, cluster)

                    ' 种子自身就无法与其它类别区分（存在完全相同的异类模式）时跳过，
                    ' 交给一致性兜底流程处理
                    If Not IsPure(New ConjunctionRule(literals, label), patterns, labels, label) Then
                        Continue For
                    End If

                    ' 按照到种子的距离升序逐个吸纳样本：只要固定下来的合取项仍然纯净就接受
                    Dim candidates As Integer() = members _
                        .Where(Function(j) j <> seedIndex AndAlso Not covered.Contains(j)) _
                        .Where(Function(j) patterns(seedIndex).DistanceTo(patterns(j)) <= thetaValue) _
                        .OrderBy(Function(j) patterns(seedIndex).DistanceTo(patterns(j))) _
                        .ThenBy(Function(j) j) _
                        .ToArray

                    For Each j As Integer In candidates
                        Dim trial As New List(Of Integer)(cluster)

                        Call trial.Add(j)

                        Dim trialLiterals As Literal() = FixedLiterals(patterns, trial)

                        If IsPure(New ConjunctionRule(trialLiterals, label), patterns, labels, label) Then
                            cluster = trial
                            literals = trialLiterals
                        End If
                    Next

                    If cluster.Count < minCoverage Then
                        Continue For
                    End If

                    Dim rule As New ConjunctionRule(literals, label, cluster.ToArray)

                    If prune Then
                        rule = PruneLiterals(rule, patterns, labels, label)
                    End If

                    ' 剪枝会使规则泛化，因此按照剪枝之后的覆盖范围更新已覆盖集合
                    For Each j As Integer In members
                        If rule.Matches(patterns(j)) Then
                            Call covered.Add(j)
                        End If
                    Next

                    Call generated.Add(rule)
                Next

                Dim kept As List(Of ConjunctionRule) = SubsumptionPrune(DistinctRules(generated))

                ' 一致性兜底：仍然没有被任何规则覆盖的正例，生成一条只属于它自己的合取项
                Call EnsureCoverage(patterns, labels, label, members, kept, prune)

                Call classRules.Add(New ClassRule(label, kept.ToArray))
            Next

            Dim ruleset As New RuleSet(classRules.ToArray)

            ruleset.Report = RuleSetReport.Build(ruleset, patterns, labels)

            Return ruleset
        End Function

        ''' <summary>
        ''' 在统一编码之后的中间表示上提取规则（要求全部特征都是二值的）。
        ''' </summary>
        ''' <param name="data">统一编码之后的类别码矩阵</param>
        ''' <param name="labels">训练集的类别标签</param>
        ''' <param name="theta">类内簇生成的距离阈值</param>
        ''' <param name="prune">是否执行剪枝</param>
        ''' <param name="minCoverage">一个簇至少需要包含的样本数量</param>
        ''' <returns></returns>
        Public Function Run(data As HammingData,
                            labels As Integer(),
                            Optional theta As Double = Double.NaN,
                            Optional prune As Boolean = True,
                            Optional minCoverage As Integer = 1) As RuleSet

            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If
            If Not data.IsBinary Then
                Throw New InvalidOperationException("the supervised rule extraction requires binary features, please discretize the input data set first!")
            End If

            Return Run(data.ToPatterns(), labels, theta, prune, minCoverage)
        End Function

        ''' <summary>
        ''' 提取簇内所有成员取值都相同的那些位，作为合取项的字面量。
        ''' </summary>
        Private Shared Function FixedLiterals(patterns As BitPattern(), members As List(Of Integer)) As Literal()
            Dim width As Integer = patterns(0).BitLength
            Dim literals As New List(Of Literal)()

            For j As Integer = 0 To width - 1
                Dim value As Boolean = patterns(members(0)).BitAt(j)
                Dim same As Boolean = True

                For k As Integer = 1 To members.Count - 1
                    If patterns(members(k)).BitAt(j) <> value Then
                        same = False
                        Exit For
                    End If
                Next

                If same Then
                    Call literals.Add(New Literal(j, value))
                End If
            Next

            Return literals.ToArray
        End Function

        ''' <summary>
        ''' 生成把整个位模式完全固定下来的合取项（用于保证训练集完全一致）。
        ''' </summary>
        Private Shared Function ExactLiterals(pattern As BitPattern) As Literal()
            Dim literals As Literal() = New Literal(pattern.BitLength - 1) {}

            For j As Integer = 0 To pattern.BitLength - 1
                literals(j) = New Literal(j, pattern.BitAt(j))
            Next

            Return literals
        End Function

        ''' <summary>
        ''' 逐位尝试去掉字面量：只要去掉之后仍然不会覆盖到其它类别的训练样本，
        ''' 就接受这次泛化。多轮执行直到不能再化简。
        ''' </summary>
        Private Shared Function PruneLiterals(rule As ConjunctionRule,
                                             patterns As BitPattern(),
                                             labels As Integer(),
                                             label As Integer) As ConjunctionRule

            Dim current As New List(Of Literal)(rule.Literals)
            Dim changed As Boolean = True

            While changed
                changed = False

                For i As Integer = 0 To current.Count - 1
                    Dim trial As New List(Of Literal)(current)

                    Call trial.RemoveAt(i)

                    Dim candidate As New ConjunctionRule(trial.ToArray, label, rule.Covered)

                    If IsPure(candidate, patterns, labels, label) Then
                        current = trial
                        changed = True
                        Exit For
                    End If
                Next
            End While

            Return New ConjunctionRule(current.ToArray, label, rule.Covered)
        End Function

        ''' <summary>
        ''' 规则是否只命中本类别的训练样本（不破坏训练集一致性的判据）。
        ''' </summary>
        Private Shared Function IsPure(rule As ConjunctionRule,
                                       patterns As BitPattern(),
                                       labels As Integer(),
                                       label As Integer) As Boolean

            For i As Integer = 0 To patterns.Length - 1
                If labels(i) <> label AndAlso rule.Matches(patterns(i)) Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 去掉字面量集合完全相同的重复规则。
        ''' </summary>
        Private Shared Function DistinctRules(rules As List(Of ConjunctionRule)) As List(Of ConjunctionRule)
            Dim kept As New List(Of ConjunctionRule)()

            For Each rule As ConjunctionRule In rules
                Dim duplicate As Boolean = False

                For Each other As ConjunctionRule In kept
                    If SameLiterals(rule, other) Then
                        duplicate = True
                        Exit For
                    End If
                Next

                If Not duplicate Then
                    Call kept.Add(rule)
                End If
            Next

            Return kept
        End Function

        Private Shared Function SameLiterals(x As ConjunctionRule, y As ConjunctionRule) As Boolean
            If x.Literals.Length <> y.Literals.Length Then
                Return False
            End If

            For i As Integer = 0 To x.Literals.Length - 1
                If Not y.Contains(x.Literals(i)) Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 去掉被同类中更一般的规则完全覆盖的冗余规则。
        ''' </summary>
        Private Shared Function SubsumptionPrune(rules As List(Of ConjunctionRule)) As List(Of ConjunctionRule)
            Dim kept As New List(Of ConjunctionRule)()

            For i As Integer = 0 To rules.Count - 1
                Dim redundant As Boolean = False

                For j As Integer = 0 To rules.Count - 1
                    If i <> j AndAlso rules(j).Subsumes(rules(i)) Then
                        redundant = True
                        Exit For
                    End If
                Next

                If Not redundant Then
                    Call kept.Add(rules(i))
                End If
            Next

            Return kept
        End Function

        ''' <summary>
        ''' 保证训练集上的完全一致：对尚未被规则覆盖的正例补充专用合取项。
        ''' </summary>
        Private Shared Sub EnsureCoverage(patterns As BitPattern(),
                                          labels As Integer(),
                                          label As Integer,
                                          members As Integer(),
                                          ByRef classRules As List(Of ConjunctionRule),
                                          prune As Boolean)

            For Each index As Integer In members
                Dim matched As Boolean = False

                For Each rule As ConjunctionRule In classRules
                    If rule.Matches(patterns(index)) Then
                        matched = True
                        Exit For
                    End If
                Next

                If matched Then
                    Continue For
                End If

                ' 该样本与其它类别的样本位模式完全相同：任何规则都无法区分，跳过以免制造矛盾规则
                If HasConflict(patterns, labels, label, index) Then
                    Continue For
                End If

                Dim specific As New ConjunctionRule(ExactLiterals(patterns(index)), label, New Integer() {index})

                If prune Then
                    specific = PruneLiterals(specific, patterns, labels, label)
                End If

                Call classRules.Add(specific)

                ' 泛化之后的规则可能覆盖后面的正例，但逐个样本检查仍然正确
            Next
        End Sub

        ''' <summary>
        ''' 是否存在与本样本位模式完全相同、但类别不同的训练样本。
        ''' </summary>
        Private Shared Function HasConflict(patterns As BitPattern(),
                                            labels As Integer(),
                                            label As Integer,
                                            index As Integer) As Boolean

            For i As Integer = 0 To patterns.Length - 1
                If labels(i) <> label AndAlso patterns(i).DistanceTo(patterns(index)) = 0 Then
                    Return True
                End If
            Next

            Return False
        End Function
    End Class
End Namespace
