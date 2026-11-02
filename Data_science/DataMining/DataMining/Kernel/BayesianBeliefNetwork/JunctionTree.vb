Imports std = System.Math

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 基于团树(clique tree)的精确推断引擎，与 <see cref="VariableElimination"/> 互为对照实现。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 构建流程遵循标准的 junction tree 构造法：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>由条件概率表构造 moral graph；</description></item>
    ''' <item><description>使用 min-fill 启发式求解弦化(chordal)的消元顺序，每个变量消元时产生的团即一个 clique；</description></item>
    ''' <item><description>丢弃被其它团包含的冗余团，并以「公共变量个数」为边权求最大生成树，得到 junction tree；</description></item>
    ''' <item><description>进行 collect / distribute 两趟 Shafer-Shenoy 消息传递完成校准，然后读取任意变量的边缘后验。</description></item>
    ''' </list>
    ''' <para>
    ''' 变量消除与团树在本项目中互为数值对照：测试工程会对同一个网络交叉校验两者的结果，
    ''' 这样任何一方的实现错误都会被立刻发现。
    ''' </para>
    ''' </remarks>
    Public Class JunctionTree

        Private ReadOnly _network As BeliefNetwork
        Private ReadOnly _nodes As BeliefNode()
        Private ReadOnly _cliques As New List(Of Clique)
        Private ReadOnly _factorOwner As Integer()
        Private ReadOnly _nodeFactors As Factor()

        ''' <summary>
        ''' 团树之中的一个团
        ''' </summary>
        Public Class Clique

            Friend ReadOnly Index As Integer
            Friend ReadOnly VariableSet As Integer()
            Friend ReadOnly Neighbors As New List(Of Integer)

            Public Sub New(index As Integer, variables As Integer())
                Me.Index = index
                VariableSet = variables
            End Sub

            ''' <summary>
            ''' 团之中包含的变量(升序)
            ''' </summary>
            Public ReadOnly Property Variables As Integer()
                Get
                    Return VariableSet
                End Get
            End Property

            ''' <summary>
            ''' 相邻团的编号
            ''' </summary>
            Public ReadOnly Property Adjacent As Integer()
                Get
                    Return Neighbors.ToArray()
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"Clique {Index}: {{{String.Join(", ", VariableSet)}}}"
            End Function
        End Class

        ''' <summary>
        ''' 为指定的网络构建团树。结构在构造时一次性确定，证据在每次查询时注入。
        ''' </summary>
        Public Sub New(network As BeliefNetwork)
            If network Is Nothing Then
                Throw New ArgumentNullException(NameOf(network))
            End If

            _network = network
            _nodes = network.Nodes
            _nodeFactors = New Factor(_nodes.Length - 1) {}

            For i As Integer = 0 To _nodes.Length - 1
                _nodeFactors(i) = VariableElimination.BuildNodeFactor(_nodes(i), _nodes)
            Next

            Call BuildCliques()
            Call BuildTree()

            _factorOwner = New Integer(_nodeFactors.Length - 1) {}
            Call AssignFactors()
        End Sub

        ''' <summary>
        ''' 团树之中的全部团
        ''' </summary>
        Public ReadOnly Property Cliques As Clique()
            Get
                Return _cliques.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' 计算 <c>P(targets | evidence)</c>
        ''' </summary>
        ''' <remarks>
        ''' 当目标变量落在同一个团之内时返回精确的联合后验；否则返回各自边缘概率的乘积
        ''' (团树之上的跨团联合查询需要额外构造子团，本模块的用途是做数值对照，因此不做该扩展)。
        ''' </remarks>
        Public Function Query(evidence As Integer(), targets As Integer()) As Double()
            Dim sortedTargets As Integer() = targets.Distinct().OrderBy(Function(v) v).ToArray()
            Dim calibrated As Factor() = Calibrate(evidence)
            Dim contains As Integer = FindCliqueContaining(sortedTargets)

            If contains >= 0 Then
                Return ExpandToTargets(calibrated(contains), evidence, sortedTargets)
            End If

            Dim marginals As Double()() = sortedTargets.Select(Function(v) Marginal(evidence, v)).ToArray()
            Dim cards As Integer() = sortedTargets.Select(Function(v) _nodes(v).Range).ToArray()
            Dim strides As Integer() = Factor.ComputeStrides(cards)
            Dim size As Integer = If(cards.Length = 0, 1, strides(cards.Length - 1) * cards(cards.Length - 1))
            Dim result As Double() = New Double(size - 1) {}
            Dim codes As Integer() = New Integer(sortedTargets.Length - 1) {}

            For i As Integer = 0 To size - 1
                Call Factor.Decode(i, cards, strides, codes)

                Dim p As Double = 1.0

                For k As Integer = 0 To sortedTargets.Length - 1
                    p *= marginals(k)(codes(k))
                Next

                result(i) = p
            Next

            Return result
        End Function

        ''' <summary>
        ''' 计算单个变量的边缘后验分布
        ''' </summary>
        Public Function Marginal(evidence As Integer(), target As Integer) As Double()
            Dim calibrated As Factor() = Calibrate(evidence)
            Dim index As Integer = FindCliqueContaining({target})

            If index < 0 Then
                Dim uniform As Double() = New Double(_nodes(target).Range - 1) {}

                For i As Integer = 0 To uniform.Length - 1
                    uniform(i) = 1.0 / uniform.Length
                Next

                Return uniform
            End If

            Return ExpandToTargets(calibrated(index), evidence, {target})
        End Function

        ''' <summary>
        ''' 把一个定义在目标变量子集之上的因子扩展为完整的目标变量分布。
        ''' </summary>
        ''' <remarks>
        ''' 证据变量在构造团潜势的时候已经被固定并从定义域之中移除，因此当查询变量恰好就是
        ''' 被观测的那个变量时，直接读出潜势只会得到一张秩为 0 的表。这里按照证据把它展开为
        ''' 一个确定性的分布，与变量消除引擎的行为保持一致。
        ''' </remarks>
        Private Function ExpandToTargets(factor As Factor, evidence As Integer(), targets As Integer()) As Double()
            ' 先把团潜势边缘化到目标变量集合之上，之后它的定义域一定是目标的子集
            Dim marginalFactor As Factor = factor.MarginalizeTo(targets)
            Dim cards As Integer() = targets.Select(Function(v) _nodes(v).Range).ToArray()
            Dim strides As Integer() = Factor.ComputeStrides(cards)
            Dim size As Integer = If(targets.Length = 0, 1, strides(targets.Length - 1) * cards(targets.Length - 1))
            Dim raw As Double() = New Double(size - 1) {}
            Dim codes As Integer() = New Integer(targets.Length - 1) {}
            Dim subset As Integer() = marginalFactor.Variables
            Dim buffer As Integer() = New Integer(subset.Length - 1) {}

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
                    raw(i) = Double.NegativeInfinity
                    Continue For
                End If

                For k As Integer = 0 To subset.Length - 1
                    Dim position As Integer = Array.IndexOf(targets, subset(k))
                    buffer(k) = If(position >= 0, codes(position), 0)
                Next

                raw(i) = If(subset.Length = 0, 0.0, marginalFactor.LogValue(buffer))
            Next

            Return NumericLogSpace.NormalizeLog(raw)
        End Function

        ''' <summary>
        ''' 计算证据的边缘概率的对数值 <c>ln P(evidence)</c>
        ''' </summary>
        Public Function LogPartition(evidence As Integer()) As Double
            Dim calibrated As Factor() = Calibrate(evidence)
            Return calibrated(0).MarginalizeTo(New Integer() {}).LogMass()
        End Function

#Region "Structure construction"

        Private Sub BuildCliques()
            Dim variables As Integer() = Enumerable.Range(0, _nodes.Length).ToArray()
            Dim neighbors As New Dictionary(Of Integer, HashSet(Of Integer))

            For Each v As Integer In variables
                neighbors(v) = New HashSet(Of Integer)
            Next

            For Each f As Factor In _nodeFactors
                Dim vars As Integer() = f.Variables

                For i As Integer = 0 To vars.Length - 1
                    For j As Integer = i + 1 To vars.Length - 1
                        Call neighbors(vars(i)).Add(vars(j))
                        Call neighbors(vars(j)).Add(vars(i))
                    Next
                Next
            Next

            Dim order As Integer() = VariableElimination.MinFillOrder(variables, _nodeFactors)
            Dim pending As New HashSet(Of Integer)(variables)
            Dim raw As New List(Of Integer())

            For Each v As Integer In order
                Dim clique As New List(Of Integer) From {v}
                Dim nb As Integer() = neighbors(v).Where(Function(x) pending.Contains(x)).ToArray()

                Call clique.AddRange(nb)
                Call raw.Add(clique.Distinct().OrderBy(Function(x) x).ToArray())

                For i As Integer = 0 To nb.Length - 1
                    For j As Integer = 0 To nb.Length - 1
                        If i <> j Then
                            Call neighbors(nb(i)).Add(nb(j))
                        End If
                    Next
                Next

                For Each a As Integer In nb
                    Call neighbors(a).Remove(v)
                Next

                Call pending.Remove(v)
            Next

            ' 丢弃被其它团包含的冗余团
            Dim kept As New List(Of Integer())

            For i As Integer = 0 To raw.Count - 1
                Dim redundant As Boolean = False

                For j As Integer = 0 To raw.Count - 1
                    If i = j OrElse raw(i).Length > raw(j).Length Then
                        Continue For
                    End If
                    If raw(i).All(Function(x) raw(j).Contains(x)) Then
                        If raw(i).Length = raw(j).Length Then
                            ' 大小相同的重复团只保留第一个
                            redundant = (j < i)
                        Else
                            redundant = True
                        End If

                        If redundant Then
                            Exit For
                        End If
                    End If
                Next

                If Not redundant Then
                    Call kept.Add(raw(i))
                End If
            Next

            If kept.Count = 0 Then
                Call kept.Add(variables)
            End If

            For i As Integer = 0 To kept.Count - 1
                Call _cliques.Add(New Clique(i, kept(i)))
            Next
        End Sub

        Private Sub BuildTree()
            If _cliques.Count <= 1 Then
                Return
            End If

            Dim count As Integer = _cliques.Count
            Dim inTree As Boolean() = New Boolean(count - 1) {}
            Dim best As Integer() = New Integer(count - 1) {}
            Dim bestWeight As Double() = New Double(count - 1) {}

            For i As Integer = 0 To count - 1
                best(i) = -1
                bestWeight(i) = Double.NegativeInfinity
            Next

            inTree(0) = True

            For i As Integer = 1 To count - 1
                bestWeight(i) = SeparatorSize(_cliques(0), _cliques(i))
                best(i) = 0
            Next

            For iteration As Integer = 1 To count - 1
                Dim nextClique As Integer = -1
                Dim weight As Double = Double.NegativeInfinity

                For i As Integer = 0 To count - 1
                    If Not inTree(i) AndAlso bestWeight(i) > weight Then
                        weight = bestWeight(i)
                        nextClique = i
                    End If
                Next

                If nextClique < 0 Then
                    Exit For
                End If

                inTree(nextClique) = True
                Call _cliques(nextClique).Neighbors.Add(best(nextClique))
                Call _cliques(best(nextClique)).Neighbors.Add(nextClique)

                For i As Integer = 0 To count - 1
                    If inTree(i) Then
                        Continue For
                    End If

                    Dim w As Double = SeparatorSize(_cliques(nextClique), _cliques(i))

                    If w > bestWeight(i) Then
                        bestWeight(i) = w
                        best(i) = nextClique
                    End If
                Next
            Next
        End Sub

        Private Shared Function SeparatorSize(a As Clique, b As Clique) As Double
            Return a.VariableSet.Count(Function(v) b.VariableSet.Contains(v))
        End Function

        Private Sub AssignFactors()
            For i As Integer = 0 To _nodeFactors.Length - 1
                Dim vars As Integer() = _nodeFactors(i).Variables
                Dim owner As Integer = -1

                For c As Integer = 0 To _cliques.Count - 1
                    If vars.All(Function(v) _cliques(c).VariableSet.Contains(v)) Then
                        owner = c
                        Exit For
                    End If
                Next

                _factorOwner(i) = owner
            Next
        End Sub

        Private Function FindCliqueContaining(variables As Integer()) As Integer
            For c As Integer = 0 To _cliques.Count - 1
                If variables.All(Function(v) _cliques(c).VariableSet.Contains(v)) Then
                    Return c
                End If
            Next

            Return -1
        End Function

        Private Function Separator(a As Integer, b As Integer) As Integer()
            Return _cliques(a).VariableSet _
                .Where(Function(v) _cliques(b).VariableSet.Contains(v)) _
                .ToArray()
        End Function

#End Region

#Region "Message passing (Shafer-Shenoy)"

        Private Function Calibrate(evidence As Integer()) As Factor()
            Dim count As Integer = _cliques.Count
            Dim potentials As Factor() = BuildPotentials(evidence)
            Dim messages(count - 1, count - 1) As Factor
            Dim parent As Integer() = New Integer(count - 1) {}
            Dim order As New List(Of Integer)

            For i As Integer = 0 To count - 1
                parent(i) = -1
            Next

            ' 以 0 号团为根做一次广度优先遍历，得到父指针与前序
            Dim visited As Boolean() = New Boolean(count - 1) {}
            Dim queue As New Queue(Of Integer)

            Call queue.Enqueue(0)
            visited(0) = True
            parent(0) = -1

            While queue.Count > 0
                Dim current As Integer = queue.Dequeue()
                Call order.Add(current)

                For Each neighbor As Integer In _cliques(current).Neighbors
                    If Not visited(neighbor) Then
                        visited(neighbor) = True
                        parent(neighbor) = current
                        Call queue.Enqueue(neighbor)
                    End If
                Next
            End While

            ' collect：自底向上
            For idx As Integer = order.Count - 1 To 0 Step -1
                Dim current As Integer = order(idx)

                If parent(current) < 0 Then
                    Continue For
                End If

                Dim message As Factor = potentials(current)

                For Each neighbor As Integer In _cliques(current).Neighbors
                    If neighbor <> parent(current) Then
                        message = message.Multiply(messages(neighbor, current))
                    End If
                Next

                messages(current, parent(current)) = message.MarginalizeTo(Separator(current, parent(current)))
            Next

            ' distribute：自顶向下
            For idx As Integer = 0 To order.Count - 1
                Dim current As Integer = order(idx)

                For Each child As Integer In _cliques(current).Neighbors
                    If parent(child) <> current Then
                        Continue For
                    End If

                    Dim message As Factor = potentials(current)

                    If parent(current) >= 0 Then
                        message = message.Multiply(messages(parent(current), current))
                    End If

                    For Each neighbor As Integer In _cliques(current).Neighbors
                        If neighbor <> child AndAlso neighbor <> parent(current) Then
                            message = message.Multiply(messages(neighbor, current))
                        End If
                    Next

                    messages(current, child) = message.MarginalizeTo(Separator(current, child))
                Next
            Next

            ' 校准之后每个团的潜势就是团变量上的联合后验(未归一化)
            Dim calibrated As Factor() = New Factor(count - 1) {}

            For c As Integer = 0 To count - 1
                Dim phi As Factor = potentials(c)

                For Each neighbor As Integer In _cliques(c).Neighbors
                    phi = phi.Multiply(messages(neighbor, c))
                Next

                calibrated(c) = phi
            Next

            Return calibrated
        End Function

        Private Function BuildPotentials(evidence As Integer()) As Factor()
            Dim count As Integer = _cliques.Count
            Dim potentials As Factor() = New Factor(count - 1) {}

            For c As Integer = 0 To count - 1
                potentials(c) = Identity
            Next

            For i As Integer = 0 To _nodeFactors.Length - 1
                Dim f As Factor = _nodeFactors(i)

                If evidence IsNot Nothing Then
                    For Each v As Integer In f.Variables
                        If evidence(v) <> BeliefNode.NotObserved Then
                            f = f.Restrict(v, evidence(v))
                        End If
                    Next
                End If

                Dim owner As Integer = _factorOwner(i)

                If owner < 0 Then
                    Continue For
                End If

                potentials(owner) = potentials(owner).Multiply(f)
            Next

            Return potentials
        End Function

        ''' <summary>
        ''' 秩为 0 的单位因子(对数潜势为 0，即概率 1)
        ''' </summary>
        Private Shared ReadOnly Property Identity As Factor
            Get
                Return New Factor(New Integer() {}, New Integer() {}, New Double() {0.0})
            End Get
        End Property

#End Region
    End Class
End Namespace
