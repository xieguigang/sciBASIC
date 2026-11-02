Imports std = System.Math

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 基于变量消除(variable elimination)的精确推断引擎。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 与旧实现(<c>BElim</c> 内部手写的 bucket 递归)相比，这里做了三点修正：
    ''' </para>
    ''' <list type="number">
    ''' <item>
    ''' <description>
    ''' 支持任意多值节点：不再假设「一个父节点占用一个二进制位」，条件概率表的行索引由
    ''' <see cref="BeliefNode.RowIndex"/> 按混合进制展开。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' 证据以「合并」语义写入，查询变量与条件变量不会互相覆盖。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' 消除顺序由 moral graph 上的 min-fill 启发式决定，得到的 bucket 结构等价于一个正确的
    ''' junction tree，因此含 v-structure 的非树形网络也可以得到正确结果。
    ''' </description>
    ''' </item>
    ''' </list>
    ''' <para>
    ''' 全部运算都在对数空间之中进行，避免长链条相乘导致的下溢。
    ''' </para>
    ''' </remarks>
    Public Class VariableElimination

        Private ReadOnly _network As BeliefNetwork
        Private ReadOnly _nodes As BeliefNode()
        Private ReadOnly _factors As Factor()

        ''' <summary>
        ''' 为指定的网络创建推断引擎。网络的 CPT 快照在此刻被构建，之后对 CPT 的修改不会影响
        ''' 已经创建出来的引擎实例。
        ''' </summary>
        Public Sub New(network As BeliefNetwork)
            If network Is Nothing Then
                Throw New ArgumentNullException(NameOf(network))
            End If
            If network.Count = 0 Then
                Throw New ArgumentException("the network contains no node", NameOf(network))
            End If

            _network = network
            _nodes = network.Nodes
            _factors = New Factor(_nodes.Length - 1) {}

            For i As Integer = 0 To _nodes.Length - 1
                _factors(i) = BuildNodeFactor(_nodes(i), _nodes)
            Next
        End Sub

        ''' <summary>
        ''' 被推断的贝叶斯网络
        ''' </summary>
        Public ReadOnly Property Network As BeliefNetwork
            Get
                Return _network
            End Get
        End Property

        ''' <summary>
        ''' 计算 <c>P(targets | evidence)</c>。返回数组的下标是按照 <paramref name="targets"/>
        ''' 升序排列之后的混合进制展开。
        ''' </summary>
        ''' <param name="evidence">长度等于节点数的证据数组，-1 表示未观测</param>
        ''' <param name="targets">查询变量的下标集合</param>
        Public Function Query(evidence As Integer(), targets As Integer()) As Double()
            Return NumericLogSpace.NormalizeLog(LogQuery(evidence, targets))
        End Function

        ''' <summary>
        ''' 计算 <c>ln P(targets | evidence)</c>
        ''' </summary>
        Public Function LogQuery(evidence As Integer(), targets As Integer()) As Double()
            Dim sortedTargets As Integer() = targets.Distinct().OrderBy(Function(v) v).ToArray()
            Dim joint As Factor = Eliminate(evidence, sortedTargets, sum:=True)
            Dim logValues As Double() = NormalizeTargets(joint, evidence, sortedTargets)

            Return NumericLogSpace.NormalizeLogSpace(logValues)
        End Function

        ''' <summary>
        ''' 计算证据的边缘概率的对数值 <c>ln P(evidence)</c>
        ''' </summary>
        Public Function LogPartition(evidence As Integer()) As Double
            Dim remaining As Factor = Eliminate(evidence, New Integer() {}, sum:=True)
            Return remaining.Values(0)
        End Function

        ''' <summary>
        ''' 计算单个变量的边缘后验 <c>P(target = value | evidence)</c>
        ''' </summary>
        Public Function Probability(evidence As Integer(), target As Integer, value As Integer) As Double
            Dim probs As Double() = Query(evidence, {target})
            Return probs(value)
        End Function

        ''' <summary>
        ''' 计算单个变量的完整边缘后验分布
        ''' </summary>
        Public Function Marginal(evidence As Integer(), target As Integer) As Double()
            Return Query(evidence, {target})
        End Function

        ''' <summary>
        ''' 计算 MAP 赋值：给定证据，返回使后验概率最大的目标变量取值组合
        ''' </summary>
        Public Function MapAssignment(evidence As Integer(), targets As Integer()) As Integer()
            Dim sortedTargets As Integer() = targets.Distinct().OrderBy(Function(v) v).ToArray()
            Dim joint As Factor = Eliminate(evidence, sortedTargets, sum:=False)
            Dim cards As Integer() = sortedTargets.Select(Function(v) _nodes(v).Range).ToArray()
            Dim best As Integer = 0
            Dim bestValue As Double = Double.NegativeInfinity

            For i As Integer = 0 To joint.Size - 1
                If joint.Values(i) > bestValue Then
                    bestValue = joint.Values(i)
                    best = i
                End If
            Next

            Dim strides As Integer() = Factor.ComputeStrides(cards)
            Dim assignment As Integer() = New Integer(sortedTargets.Length - 1) {}
            Call Factor.Decode(best, cards, strides, assignment)

            For k As Integer = 0 To sortedTargets.Length - 1
                If evidence IsNot Nothing AndAlso evidence(sortedTargets(k)) <> BeliefNode.NotObserved Then
                    assignment(k) = evidence(sortedTargets(k))
                End If
            Next

            Return assignment
        End Function

        ''' <summary>
        ''' 在给定的证据之下消去除目标变量之外的所有变量，返回目标变量的联合因子(未归一化)
        ''' </summary>
        ''' <param name="sum">为 <c>True</c> 的时候求和消元，为 <c>False</c> 的时候取最大值消元</param>
        Public Function Eliminate(evidence As Integer(), targets As Integer(), sum As Boolean) As Factor
            Dim work As New List(Of Factor)(_factors.Length)
            Dim observed As New HashSet(Of Integer)

            If evidence IsNot Nothing Then
                For i As Integer = 0 To std.Min(evidence.Length, _nodes.Length) - 1
                    If evidence(i) <> BeliefNode.NotObserved Then
                        Call observed.Add(i)
                    End If
                Next
            End If

            Dim restricted As Factor() = New Factor(_factors.Length - 1) {}

            For i As Integer = 0 To _factors.Length - 1
                Dim f As Factor = _factors(i)

                For Each v As Integer In f.Variables
                    If observed.Contains(v) Then
                        f = f.Restrict(v, evidence(v))
                    End If
                Next

                restricted(i) = f
                Call work.Add(f)
            Next

            Dim eliminable As Integer() = Enumerable.Range(0, _nodes.Length) _
                .Where(Function(v) Array.IndexOf(targets, v) < 0 AndAlso Not observed.Contains(v)) _
                .ToArray()
            Dim order As Integer() = MinFillOrder(eliminable, restricted)

            For Each v As Integer In order
                Dim containing As New List(Of Factor)

                For Each f As Factor In work
                    If f.Contains(v) Then
                        Call containing.Add(f)
                    End If
                Next

                If containing.Count = 0 Then
                    Continue For
                End If

                For Each f As Factor In containing
                    Call work.Remove(f)
                Next

                Dim product As Factor = containing(0)

                For k As Integer = 1 To containing.Count - 1
                    product = product.Multiply(containing(k))
                Next

                If sum Then
                    Call work.Add(product.SumOut(v))
                Else
                    Call work.Add(product.MaxOut(v))
                End If
            Next

            Dim result As Factor = Nothing

            For Each f As Factor In work
                If result Is Nothing Then
                    result = f
                Else
                    result = result.Multiply(f)
                End If
            Next

            If result Is Nothing Then
                ' 没有任何因子留下来：只有一个变量且被直接观测的情形
                Dim constant As Double() = {0.0}
                Return New Factor(New Integer() {}, New Integer() {}, constant)
            End If

            Return result
        End Function

        ''' <summary>
        ''' 把只定义在目标变量子集之上的因子扩展为完整的 <paramref name="targets"/> 联合分布
        ''' (与证据冲突的组合取值为 0)
        ''' </summary>
        Private Function NormalizeTargets(joint As Factor, evidence As Integer(), targets As Integer()) As Double()
            Dim cards As Integer() = targets.Select(Function(v) _nodes(v).Range).ToArray()
            Dim strides As Integer() = Factor.ComputeStrides(cards)
            Dim size As Integer = If(cards.Length = 0, 1, strides(cards.Length - 1) * cards(cards.Length - 1))
            Dim values As Double() = New Double(size - 1) {}
            Dim codes As Integer() = New Integer(targets.Length - 1) {}
            Dim subset As Integer() = joint.Variables
            Dim subsetBuffer As Integer() = New Integer(subset.Length - 1) {}

            For i As Integer = 0 To size - 1
                Call Factor.Decode(i, cards, strides, codes)

                Dim conflict As Boolean = False

                For k As Integer = 0 To targets.Length - 1
                    If evidence IsNot Nothing AndAlso evidence(targets(k)) <> BeliefNode.NotObserved AndAlso evidence(targets(k)) <> codes(k) Then
                        conflict = True
                        Exit For
                    End If
                Next

                If conflict Then
                    values(i) = Double.NegativeInfinity
                    Continue For
                End If

                For k As Integer = 0 To subset.Length - 1
                    subsetBuffer(k) = codes(Array.IndexOf(targets, subset(k)))
                Next

                values(i) = joint.LogValue(subsetBuffer)
            Next

            Return values
        End Function

#Region "Elimination order heuristics"

        ''' <summary>
        ''' 在 moral graph 之上使用 min-fill 启发式计算变量消除顺序。
        ''' </summary>
        ''' <param name="variables">需要被消去的变量</param>
        ''' <param name="factors">当前的因子集合</param>
        ''' <remarks>
        ''' min-fill 启发式每一步都选择「消除之后需要新增的团(clique)边数最少」的变量，
        ''' 这样可以显著降低中间因子的规模，并且保证最终得到的团序列构成一个弦图(chordal graph)，
        ''' 从而对应一个合法的 junction tree。
        ''' </remarks>
        Public Shared Function MinFillOrder(variables As Integer(), factors As Factor()) As Integer()
            Dim order As New List(Of Integer)
            Dim neighbors As New Dictionary(Of Integer, HashSet(Of Integer))

            For Each v As Integer In variables
                neighbors(v) = New HashSet(Of Integer)
            Next

            ' 由每个因子的定义域诱导出 mora graph
            For Each f As Factor In factors
                Dim vars As Integer() = f.Variables

                For i As Integer = 0 To vars.Length - 1
                    For j As Integer = i + 1 To vars.Length - 1
                        If neighbors.ContainsKey(vars(i)) AndAlso neighbors.ContainsKey(vars(j)) Then
                            Call neighbors(vars(i)).Add(vars(j))
                            Call neighbors(vars(j)).Add(vars(i))
                        End If
                    Next
                Next
            Next

            Dim pending As New HashSet(Of Integer)(variables)

            While pending.Count > 0
                Dim best As Integer = -1
                Dim bestFill As Integer = Integer.MaxValue
                Dim bestDegree As Integer = Integer.MaxValue

                For Each v As Integer In pending
                    Dim nb As HashSet(Of Integer) = neighbors(v)
                    Dim nodes As Integer() = nb.ToArray()
                    Dim fill As Integer = 0

                    For i As Integer = 0 To nodes.Length - 1
                        For j As Integer = i + 1 To nodes.Length - 1
                            If Not IsAdjacent(neighbors, nodes(i), nodes(j)) Then
                                fill += 1
                            End If
                        Next
                    Next

                    If fill < bestFill OrElse (fill = bestFill AndAlso nb.Count < bestDegree) Then
                        bestFill = fill
                        bestDegree = nb.Count
                        best = v
                    End If
                Next

                Call order.Add(best)
                Call pending.Remove(best)

                Dim clique As Integer() = neighbors(best).ToArray()

                For i As Integer = 0 To clique.Length - 1
                    For j As Integer = 0 To clique.Length - 1
                        If i <> j Then
                            Call neighbors(clique(i)).Add(clique(j))
                        End If
                    Next
                Next

                For Each a As Integer In clique
                    Call neighbors(a).Remove(best)
                Next

                Call neighbors.Remove(best)
            End While

            Return order.ToArray()
        End Function

        ''' <summary>
        ''' 判断两个变量在当前 moral graph 之上是否相邻
        ''' </summary>
        Private Shared Function IsAdjacent(neighbors As Dictionary(Of Integer, HashSet(Of Integer)), a As Integer, b As Integer) As Boolean
            Dim bucket As HashSet(Of Integer) = Nothing

            If neighbors.TryGetValue(a, bucket) Then
                Return bucket.Contains(b)
            End If

            Return False
        End Function

#End Region

        ''' <summary>
        ''' 由节点的条件概率表构造一个因子
        ''' </summary>
        Public Shared Function BuildNodeFactor(node As BeliefNode, nodes As BeliefNode()) As Factor
            Dim parentIds As Integer() = node.Parents.Select(Function(p) p.Id).ToArray()
            Dim vars As Integer() = parentIds.Concat({node.Id}).Distinct().OrderBy(Function(v) v).ToArray()
            Dim cards As Integer() = vars.Select(Function(v) nodes(v).Range).ToArray()
            Dim strides As Integer() = Factor.ComputeStrides(cards)
            Dim size As Integer = strides(cards.Length - 1) * cards(cards.Length - 1)
            Dim values As Double() = New Double(size - 1) {}
            Dim codes As Integer() = New Integer(vars.Length - 1) {}
            Dim parentValues As Integer() = New Integer(parentIds.Length - 1) {}

            For i As Integer = 0 To size - 1
                Call Factor.Decode(i, cards, strides, codes)

                For k As Integer = 0 To parentIds.Length - 1
                    parentValues(k) = codes(Array.IndexOf(vars, parentIds(k)))
                Next

                Dim row As Integer = node.RowIndex(parentValues)
                Dim value As Integer = codes(Array.IndexOf(vars, node.Id))
                Dim p As Double = node.CP_Table(row, value)

                values(i) = If(p <= 0, Double.NegativeInfinity, std.Log(p))
            Next

            Return New Factor(vars, cards, values)
        End Function
    End Class
End Namespace
