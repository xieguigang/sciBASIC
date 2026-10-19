Option Strict On

Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities

Namespace AprioriRules.Engine

    ''' <summary>
    ''' Apriori 高性能引擎：分层候选生成 + 位图支持度计数 + 反查式闭/极大项集
    ''' </summary>
    ''' <remarks>
    ''' 与原实现的结果完全一致（频繁项集及其支持度、闭项集、极大项集、强规则四个集合），
    ''' 差别只在于把每一处「逐事务线性 Contains」都换成了位运算或者归并联查。
    ''' </remarks>
    Friend Class AprioriEngine

        ''' <summary>
        ''' 一层挖掘的结果
        ''' </summary>
        Private Structure LevelResult
            Public Items As Integer()()
            Public Supports As Integer()
        End Structure

        ReadOnly _index As TransactionIndex
        ReadOnly _countN As Integer
        ''' <summary>
        ''' 事务总数（Double 形式），用于逐位复现原实现的阈值判定表达式
        ''' </summary>
        ReadOnly _countD As Double
        ReadOnly _minSupport As Double
        ReadOnly _minConfidence As Double
        ReadOnly _parallelism As Integer
        ReadOnly _bitBudget As Integer

        ''' <summary>
        ''' 项集（内部 id 数组） -&gt; 支持度
        ''' </summary>
        ReadOnly _supports As Dictionary(Of Integer(), Integer)
        ReadOnly _levels As New List(Of Integer()())
        ReadOnly _levelSupports As New List(Of Integer())

        ''' <summary>
        ''' 子项集 -&gt; 它的全部频繁父项集（长度 +1 的超集）
        ''' </summary>
        Dim _parentMap As Dictionary(Of Integer(), List(Of Integer()))
        ''' <summary>
        ''' 存在「支持度相同的父项集」的项集，即非闭项集
        ''' </summary>
        Dim _nonClosed As HashSet(Of Integer())

        Private Sub New(index As TransactionIndex,
                        minSupport As Double,
                        minConfidence As Double,
                        parallelism As Integer,
                        bitBudget As Integer)
            _index = index
            _countN = index.TransactionCount
            _countD = CDbl(index.TransactionCount)
            _minSupport = minSupport
            _minConfidence = minConfidence
            _parallelism = parallelism
            _bitBudget = bitBudget
            _supports = IntSetKeys.NewSupportTable()
        End Sub

        ''' <summary>
        ''' 建立挖掘引擎
        ''' </summary>
        Public Shared Function Create(items As IEnumerable(Of Item),
                                      transactions As ItemSet(),
                                      minSupport As Double,
                                      minConfidence As Double,
                                      Optional maxDegreeOfParallelism As Integer = -1,
                                      Optional bitIndexBudgetMB As Integer = 512) As AprioriEngine

            Return New AprioriEngine(
                New TransactionIndex(items, transactions),
                minSupport, minConfidence,
                maxDegreeOfParallelism, bitIndexBudgetMB)
        End Function

        ''' <summary>
        ''' 执行挖掘
        ''' </summary>
        Friend Function Run() As Output
            If _countN = 0 Then
                Return BuildOutput(New List(Of Rule)())
            End If

            Dim l1 As Integer() = _index.GetL1()
            Dim ids As New List(Of Integer)()

            ' 阈值判定沿用原实现的表达式（support / transactionsCount >= minSupport），保证浮点逐位一致
            For id As Integer = 0 To l1.Length - 1
                If CDbl(l1(id)) / _countD >= _minSupport Then
                    ids.Add(id)
                End If
            Next

            _index.BuildBitIndex(ids.ToArray(), _bitBudget)

            If ids.Count > 0 Then
                Dim set1 As Integer()() = New Integer(ids.Count - 1)() {}
                Dim sup1 As Integer() = New Integer(ids.Count - 1) {}

                For i As Integer = 0 To ids.Count - 1
                    set1(i) = New Integer() {ids(i)}
                    sup1(i) = l1(ids(i))
                Next

                AddLevel(set1, sup1)

                Dim level As Integer = 2

                ' 最长事务只有 maxLen 个不同项，包含 k 项的候选的支持度非零要求 k <= maxLen；
                ' 但 min_support 为 0 时支持度为 0 的候选也算频繁，因此再多放一层，
                ' 与原始实现在该参数取值下的行为保持一致（同时避免继续枚举到 2^d 的规模）
                Do While level <= _index.MaxLevel + 1
                    Dim prev As Integer()() = _levels(_levels.Count - 1)

                    If prev.Length < 2 Then
                        Exit Do
                    End If

                    Dim mined As LevelResult = MineLevel(prev, level)

                    If mined.Items.Length = 0 Then
                        Exit Do
                    End If

                    AddLevel(mined.Items, mined.Supports)

                    Call VBDebugger.EchoLine($" ..... {mined.Items.Length} / {_supports.Count}")

                    level += 1
                Loop
            End If

            Call VBDebugger.EchoLine($"generate rules based on all {_supports.Count} frequent items!")

            Dim rules As List(Of Rule) = New RuleMiner(_index, _supports, _minConfidence, _parallelism) _
                .Mine(_levels, _levelSupports)

            Return BuildOutput(rules)
        End Function

        Private Sub AddLevel(items As Integer()(), supports As Integer())
            _levels.Add(items)
            _levelSupports.Add(supports)

            For i As Integer = 0 To items.Length - 1
                _supports(items(i)) = supports(i)
            Next
        End Sub

        ''' <summary>
        ''' 由 L(k-1) 生成 L(k)：按前 k-2 项分组，组内两两拼接出候选，逐个计数后只保留频繁的
        ''' </summary>
        Private Function MineLevel(prev As Integer()(), level As Integer) As LevelResult
            Dim k As Integer = level
            Dim prefixLen As Integer = k - 2

            ' 1) 分组：prev 已按字典序排列，共享前 k-2 项的成员必然相邻
            Dim gPrefix As New List(Of Integer())()
            Dim gTails As New List(Of Integer())()
            Dim i As Integer = 0

            While i < prev.Length
                Dim j As Integer = i + 1

                While j < prev.Length AndAlso SamePrefix(prev(i), prev(j), prefixLen)
                    j += 1
                End While

                Dim prefix As Integer() = IntSetKeys.NewArray(Of Integer)(prefixLen)

                For p As Integer = 0 To prefixLen - 1
                    prefix(p) = prev(i)(p)
                Next

                Dim tails As Integer() = New Integer(j - i - 1) {}

                For p As Integer = 0 To j - i - 1
                    tails(p) = prev(i + p)(k - 2)
                Next

                gPrefix.Add(prefix)
                gTails.Add(tails)

                i = j
            End While

            ' 2) 行表：(组, 行)。组内最后一行没有后继，不入表
            Dim rowG As New List(Of Integer)()
            Dim rowI As New List(Of Integer)()
            Dim totalPairs As Long = 0

            For g As Integer = 0 To gTails.Count - 1
                Dim cnt As Integer = gTails(g).Length

                For r As Integer = 0 To cnt - 2
                    rowG.Add(g)
                    rowI.Add(r)
                Next

                totalPairs += CLng(cnt) * CLng(cnt - 1) \ 2L
            Next

            If rowG.Count = 0 Then
                Return New LevelResult With {
                    .Items = New Integer()() {},
                    .Supports = New Integer() {}
                }
            End If

            ' 3) 按累计 pair 数切成与处理器数相当的连续块：
            '    L2 只有一个组，只有按行切块才能让这一层也并行起来
            Dim procs As Integer = If(_parallelism > 0,
                                      System.Math.Min(_parallelism, Environment.ProcessorCount),
                                      Environment.ProcessorCount)
            Dim nChunks As Integer

            If totalPairs < 4096L Then
                nChunks = 1
            Else
                nChunks = CInt(System.Math.Min(CLng(procs), System.Math.Min(totalPairs \ 256L, CLng(rowG.Count))))
                nChunks = System.Math.Max(nChunks, 1)
            End If

            Dim bounds As Integer() = New Integer(nChunks) {}
            Dim target As Long = (totalPairs + nChunks - 1) \ nChunks
            Dim acc As Long = 0
            Dim c As Integer = 0

            For r As Integer = 0 To rowG.Count - 1
                acc += CLng(gTails(rowG(r)).Length - rowI(r) - 1)

                If acc >= target AndAlso c < nChunks - 1 Then
                    c += 1
                    bounds(c) = r + 1
                    acc = 0
                End If
            Next

            For c2 As Integer = c + 1 To nChunks
                bounds(c2) = rowG.Count
            Next

            bounds(0) = 0

            ' 4) 上一层的集合，用于候选剪枝（prune）
            Dim prevSet As HashSet(Of Integer()) = Nothing

            If level >= 3 Then
                prevSet = IntSetKeys.NewSetHash()

                For Each a As Integer() In prev
                    prevSet.Add(a)
                Next
            End If

            Dim rowGArr As Integer() = rowG.ToArray()
            Dim rowIArr As Integer() = rowI.ToArray()
            Dim gPrefixArr As Integer()() = gPrefix.ToArray()
            Dim gTailsArr As Integer()() = gTails.ToArray()
            Dim hitItems As List(Of Integer())() = New List(Of Integer())(nChunks - 1) {}
            Dim hitSups As List(Of Integer)() = New List(Of Integer)(nChunks - 1) {}

            If nChunks = 1 Then
                ProcessRows(0, level, rowGArr, rowIArr, gPrefixArr, gTailsArr, bounds, prevSet, hitItems, hitSups)
            Else
                Dim opts As New System.Threading.Tasks.ParallelOptions() With {
                    .MaxDegreeOfParallelism = procs
                }

                System.Threading.Tasks.Parallel.For(0, nChunks, opts,
                    Sub(chunk As Integer)
                        ProcessRows(chunk, level, rowGArr, rowIArr, gPrefixArr, gTailsArr, bounds, prevSet, hitItems, hitSups)
                    End Sub)
            End If

            ' 5) 按块序合并：块与行都是有序的，合并结果天然保持字典序
            Dim merged As New List(Of Integer())()
            Dim mergedSup As New List(Of Integer)()

            For ci As Integer = 0 To nChunks - 1
                merged.AddRange(hitItems(ci))
                mergedSup.AddRange(hitSups(ci))
            Next

            Return New LevelResult With {
                .Items = merged.ToArray(),
                .Supports = mergedSup.ToArray()
            }
        End Function

        ''' <summary>
        ''' 处理一个分块：逐行拼接候选并计数，只把跨过支持度阈值的候选物化成数组
        ''' </summary>
        Private Sub ProcessRows(chunk As Integer,
                                level As Integer,
                                rowG As Integer(),
                                rowI As Integer(),
                                gPrefix As Integer()(),
                                gTails As Integer()(),
                                bounds As Integer(),
                                prevSet As HashSet(Of Integer()),
                                hitItems As List(Of Integer())(),
                                hitSups As List(Of Integer)())

            Dim k As Integer = level
            Dim useBitset As Boolean = _index.UsesBitIndex
            Dim words As Integer = _index.WordsForLevel(k)
            Dim buf As ULong() = Nothing
            Dim scratch As Integer() = Nothing
            Dim cand As Integer() = New Integer(k - 1) {}
            Dim items As New List(Of Integer())()
            Dim sups As New List(Of Integer)()
            Dim lastG As Integer = -1

            If k >= 3 Then
                ' scratch 用来临时拼出待剪枝的 (k-1) 子集，长度必须恰好是 k-1
                scratch = New Integer(k - 2) {}

                If useBitset Then
                    buf = New ULong(System.Math.Max(words, 1) - 1) {}
                End If
            End If

            For r As Integer = bounds(chunk) To bounds(chunk + 1) - 1
                Dim g As Integer = rowG(r)

                If g <> lastG Then
                    If buf IsNot Nothing Then
                        _index.BuildPrefixMask(buf, gPrefix(g), words)
                    End If

                    lastG = g
                End If

                Dim tails As Integer() = gTails(g)
                Dim ti As Integer = tails(rowI(r))

                For j As Integer = rowI(r) + 1 To tails.Length - 1
                    Dim tj As Integer = tails(j)
                    Dim support As Integer

                    If useBitset Then
                        If k = 2 Then
                            ' 以支持度较小的一方作为稀疏基，能跳过更多空字
                            If _index.L1Support(ti) <= _index.L1Support(tj) Then
                                support = _index.CountPair(ti, tj, words)
                            Else
                                support = _index.CountPair(tj, ti, words)
                            End If
                        Else
                            If prevSet IsNot Nothing AndAlso Not AllSubsetsFrequent(scratch, gPrefix(g), ti, tj, prevSet) Then
                                Continue For
                            End If

                            support = _index.CountWithPrefix(buf, ti, tj, words)
                        End If
                    Else
                        If prevSet IsNot Nothing AndAlso Not AllSubsetsFrequent(scratch, gPrefix(g), ti, tj, prevSet) Then
                            Continue For
                        End If

                        For p As Integer = 0 To gPrefix(g).Length - 1
                            cand(p) = gPrefix(g)(p)
                        Next

                        cand(k - 2) = ti
                        cand(k - 1) = tj

                        support = _index.CountHorizontal(cand, k)
                    End If

                    If CDbl(support) / _countD >= _minSupport Then
                        Dim arr As Integer() = New Integer(k - 1) {}

                        For p As Integer = 0 To k - 3
                            arr(p) = gPrefix(g)(p)
                        Next

                        arr(k - 2) = ti
                        arr(k - 1) = tj

                        items.Add(arr)
                        sups.Add(support)
                    End If
                Next
            Next

            hitItems(chunk) = items
            hitSups(chunk) = sups
        End Sub

        ''' <summary>
        ''' 候选 {prefix, ti, tj} 的其余 k-2 个 (k-1) 子集是否全部频繁（Apriori 的 prune 步骤）
        ''' </summary>
        ''' <remarks>
        ''' 拼接所用的两个 (k-1) 子集 {prefix,ti} 与 {prefix,tj} 本身就是频繁的，无需再查。
        ''' </remarks>
        Private Shared Function AllSubsetsFrequent(scratch As Integer(),
                                                  prefix As Integer(),
                                                  ti As Integer,
                                                  tj As Integer,
                                                  prevSet As HashSet(Of Integer())) As Boolean
            For skip As Integer = 0 To prefix.Length - 1
                Dim w As Integer = 0

                For p As Integer = 0 To prefix.Length - 1
                    If p <> skip Then
                        scratch(w) = prefix(p)
                        w += 1
                    End If
                Next

                scratch(w) = ti
                scratch(w + 1) = tj

                If Not prevSet.Contains(scratch) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Private Shared Function SamePrefix(a As Integer(), b As Integer(), n As Integer) As Boolean
            For i As Integer = 0 To n - 1
                If a(i) <> b(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 由父项集反查子项集，一次性算出全部父子关系（用于闭项集与极大项集）
        ''' </summary>
        ''' <remarks>
        ''' 原实现是「对每个项集扫描全部频繁项集」的 O(F^2 * k)，
        ''' 并且每次都要先做一次 <see cref="Dictionary(Of TKey, TValue).Values"/> 的 ToArray 全量拷贝。
        ''' 这里反过来：遍历每个频繁项集 P，枚举它删掉一个元素得到的 |P|-1 子集 S，
        ''' 若 S 频繁就把 P 登记为 S 的父项集，并在支持度相同时把 S 标记为非闭。
        ''' </remarks>
        Private Sub ComputeLattice()
            _parentMap = IntSetKeys.NewSetTable(Of List(Of Integer()))()
            _nonClosed = IntSetKeys.NewSetHash()

            For li As Integer = 1 To _levels.Count - 1
                Dim items As Integer()() = _levels(li)
                Dim supps As Integer() = _levelSupports(li)

                For a As Integer = 0 To items.Length - 1
                    Dim parent As Integer() = items(a)
                    Dim psup As Integer = supps(a)
                    Dim k As Integer = parent.Length

                    For skip As Integer = 0 To k - 1
                        Dim subset As Integer() = New Integer(k - 2) {}
                        Dim w As Integer = 0

                        For p As Integer = 0 To k - 1
                            If p <> skip Then
                                subset(w) = parent(p)
                                w += 1
                            End If
                        Next

                        Dim ssup As Integer

                        If Not _supports.TryGetValue(subset, ssup) Then
                            Continue For
                        End If

                        Dim parents As List(Of Integer()) = Nothing

                        If Not _parentMap.TryGetValue(subset, parents) Then
                            parents = New List(Of Integer())()
                            _parentMap(subset) = parents
                        End If

                        parents.Add(parent)

                        If ssup = psup Then
                            _nonClosed.Add(subset)
                        End If
                    Next
                Next
            Next
        End Sub

        Private Function BuildOutput(rules As List(Of Rule)) As Output
            ComputeLattice()

            Dim frequent As New Dictionary(Of ItemSet, TransactionTokensItem)()
            Dim closed As New Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double))()
            Dim maximal As New List(Of ItemSet)()

            For li As Integer = 0 To _levels.Count - 1
                Dim items As Integer()() = _levels(li)
                Dim supps As Integer() = _levelSupports(li)

                For a As Integer = 0 To items.Length - 1
                    Dim arr As Integer() = items(a)
                    Dim key As ItemSet = ToItemSet(arr)
                    Dim token As New TransactionTokensItem() With {
                        .Name = key,
                        .Support = CDbl(supps(a))
                    }

                    frequent(key) = token

                    If _nonClosed.Contains(arr) Then
                        Continue For
                    End If

                    Dim parents As New Dictionary(Of ItemSet, Double)()
                    Dim parentList As List(Of Integer()) = Nothing

                    If _parentMap.TryGetValue(arr, parentList) Then
                        For Each p As Integer() In parentList
                            parents(ToItemSet(p)) = CDbl(_supports(p))
                        Next
                    Else
                        ' 没有任何频繁超集：既是闭项集也是极大项集
                        maximal.Add(key)
                    End If

                    closed(key) = parents
                Next
            Next

            Return New Output() With {
                .StrongRules = rules,
                .MaximalItemSets = maximal,
                .ClosedItemSets = closed,
                .FrequentItems = frequent,
                .TransactionSize = _countN
            }
        End Function

        Private Function ToItemSet(ids As Integer()) As ItemSet
            Dim items As Item() = IntSetKeys.NewArray(Of Item)(ids.Length)

            For i As Integer = 0 To ids.Length - 1
                items(i) = _index.ItemOf(ids(i))
            Next

            Return New ItemSet(items)
        End Function
    End Class
End Namespace
