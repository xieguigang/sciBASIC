Imports std = System.Math
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 离散贝叶斯网络的结构学习。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 提供两种互补的算法：
    ''' </para>
    ''' <list type="bullet">
    ''' <item>
    ''' <description>
    ''' <see cref="ChowLiu"/>：以两两互信息为边权求最大生成树，得到一个树形网络。
    ''' 这是速度最快的初始化方法，通常被用作爬山算法的起点。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <see cref="HillClimbing"/>：在加边 / 删边 / 反向边三种操作之上做贪心搜索，
    ''' 评分函数使用 BIC(等价于 MDL)，并且带有最大父节点数目约束与无环性检查。
    ''' </description>
    ''' </item>
    ''' </list>
    ''' <para>
    ''' BIC 评分 = 对数似然 − 0.5·ln(N)·(模型自由参数个数)，其中的惩罚项自动抑制过拟合，
    ''' 这一点对于互作数据这种小样本场景尤其重要。
    ''' </para>
    ''' </remarks>
    Public Class StructureLearner

        Private ReadOnly _data As Integer()()
        Private ReadOnly _ranges As Integer()
        Private ReadOnly _n As Integer
        Private ReadOnly _cache As New Dictionary(Of String, Double)

        Private _maxParents As Integer = 3

        ''' <summary>
        ''' 每个节点允许的最大父节点个数
        ''' </summary>
        Public Property MaxParents As Integer
            Get
                Return _maxParents
            End Get
            Set(value As Integer)
                _maxParents = value
            End Set
        End Property

        ''' <summary>
        ''' 由观测数据构造结构学习器
        ''' </summary>
        ''' <param name="data">观测数据，每行长度等于变量个数</param>
        ''' <param name="ranges">每个变量的取值个数</param>
        Public Sub New(data As Integer()(), ranges As Integer())
            If data Is Nothing OrElse data.Length = 0 Then
                Throw New ArgumentException("the observations can not be empty", NameOf(data))
            End If
            If ranges Is Nothing OrElse ranges.Length = 0 Then
                Throw New ArgumentException("the cardinalities can not be empty", NameOf(ranges))
            End If

            _data = data

            For Each row As Integer() In data
                If row Is Nothing OrElse row.Length <> ranges.Length Then
                    Throw New ArgumentException("all the observation rows must have the same length as the cardinalities array", NameOf(data))
                End If
            Next

            _ranges = ranges
            _n = ranges.Length
        End Sub

        ''' <summary>
        ''' 观测样本的个数
        ''' </summary>
        Public ReadOnly Property SampleCount As Integer
            Get
                Return _data.Length
            End Get
        End Property

        ''' <summary>
        ''' 单个节点在给定父节点集合之下的 BIC 局部评分(已包含惩罚项)
        ''' </summary>
        ''' <param name="node">节点下标</param>
        ''' <param name="parents">父节点下标集合</param>
        Public Function LocalScore(node As Integer, parents As Integer()) As Double
            Dim sorted As Integer() = If(parents Is Nothing, New Integer() {}, parents.Distinct().OrderBy(Function(v) v).ToArray())
            Dim key As String = node & ":" & String.Join(",", sorted)

            Dim cached As Double

            If _cache.TryGetValue(key, cached) Then
                Return cached
            End If

            Dim configCount As Integer = 1

            For Each p As Integer In sorted
                configCount *= _ranges(p)
            Next

            Dim counts As Integer(,) = New Integer(configCount - 1, _ranges(node) - 1) {}
            Dim totals As Integer() = New Integer(configCount - 1) {}
            Dim strides As Integer() = Factor.ComputeStrides(sorted.Select(Function(p) _ranges(p)).ToArray())
            Dim codes As Integer() = New Integer(sorted.Length - 1) {}

            For Each row As Integer() In _data
                If row(node) < 0 Then
                    Continue For
                End If

                Dim config As Integer = 0
                Dim complete As Boolean = True

                For k As Integer = 0 To sorted.Length - 1
                    Dim v As Integer = row(sorted(k))

                    If v < 0 Then
                        complete = False
                        Exit For
                    End If

                    codes(k) = v
                Next

                If Not complete Then
                    Continue For
                End If

                config = Factor.Encode(codes, strides)
                counts(config, row(node)) += 1
                totals(config) += 1
            Next

            Dim logLikelihood As Double = 0.0

            For c As Integer = 0 To configCount - 1
                If totals(c) = 0 Then
                    Continue For
                End If

                For v As Integer = 0 To _ranges(node) - 1
                    Dim n As Integer = counts(c, v)

                    If n > 0 Then
                        logLikelihood += n * std.Log(n / totals(c))
                    End If
                Next
            Next

            Dim freeParameters As Double = (_ranges(node) - 1) * configCount
            Dim penalty As Double = 0.5 * std.Log(_data.Length) * freeParameters
            Dim score As Double = logLikelihood - penalty

            _cache(key) = score

            Return score
        End Function

        ''' <summary>
        ''' 整体 BIC 评分
        ''' </summary>
        Public Function Bic(parents As Integer()()) As Double
            Dim total As Double = 0.0

            For i As Integer = 0 To _n - 1
                total += LocalScore(i, parents(i))
            Next

            Return total
        End Function

        ''' <summary>
        ''' 以两两互信息为边权求最大生成树，得到一个树形(DAG)结构。
        ''' </summary>
        Public Function ChowLiu() As Integer()()
            Dim mi As Double(,) = PairwiseMutualInformation()
            Dim parents As Integer()() = New Integer(_n - 1)() {}
            Dim inTree As Boolean() = New Boolean(_n - 1) {}
            Dim best As Integer() = New Integer(_n - 1) {}
            Dim bestWeight As Double() = New Double(_n - 1) {}

            For i As Integer = 0 To _n - 1
                parents(i) = New Integer() {}
                best(i) = -1
                bestWeight(i) = Double.NegativeInfinity
            Next

            inTree(0) = True

            For i As Integer = 1 To _n - 1
                bestWeight(i) = mi(0, i)
                best(i) = 0
            Next

            For iteration As Integer = 1 To _n - 1
                Dim pick As Integer = -1
                Dim weight As Double = Double.NegativeInfinity

                For i As Integer = 0 To _n - 1
                    If Not inTree(i) AndAlso bestWeight(i) > weight Then
                        weight = bestWeight(i)
                        pick = i
                    End If
                Next

                If pick < 0 Then
                    Exit For
                End If

                inTree(pick) = True
                parents(pick) = {best(pick)}

                For i As Integer = 0 To _n - 1
                    If inTree(i) Then
                        Continue For
                    End If

                    If mi(pick, i) > bestWeight(i) Then
                        bestWeight(i) = mi(pick, i)
                        best(i) = pick
                    End If
                Next
            Next

            Return parents
        End Function

        ''' <summary>
        ''' 爬山法结构学习：在加边 / 删边 / 反向边之上做贪心搜索直到没有改进。
        ''' </summary>
        ''' <param name="initial">初始结构，为 <c>Nothing</c> 的时候以 <see cref="ChowLiu"/> 的结果作为起点</param>
        ''' <param name="maxIterations">最大迭代次数</param>
        Public Function HillClimbing(Optional initial As Integer()() = Nothing,
                                     Optional maxIterations As Integer = 200) As Integer()()
            Dim parents As Integer()() = CloneStructure(If(initial, ChowLiu()))
            Dim current As Double = Bic(parents)

            For iteration As Integer = 1 To maxIterations
                Dim bestScore As Double = current
                Dim bestNode As Integer = -1
                Dim bestParents As Integer() = Nothing

                For i As Integer = 0 To _n - 1
                    ' 加边
                    If parents(i).Length < MaxParents Then
                        For p As Integer = 0 To _n - 1
                            If p = i OrElse parents(i).Contains(p) Then
                                Continue For
                            End If
                            If IsAncestor(parents, i, p) Then
                                ' 加 p -> i 会形成环
                                Continue For
                            End If

                            Dim candidate As Integer() = parents(i).Concat({p}).OrderBy(Function(v) v).ToArray()
                            Dim delta As Double = LocalScore(i, candidate) - LocalScore(i, parents(i))

                            If current + delta > bestScore Then
                                bestScore = current + delta
                                bestNode = i
                                bestParents = candidate
                            End If
                        Next
                    End If

                    ' 删边
                    For k As Integer = 0 To parents(i).Length - 1
                        Dim candidate As Integer() = parents(i).Where(Function(v, idx) idx <> k).ToArray()
                        Dim delta As Double = LocalScore(i, candidate) - LocalScore(i, parents(i))

                        If current + delta > bestScore Then
                            bestScore = current + delta
                            bestNode = i
                            bestParents = candidate
                        End If
                    Next
                Next

                If bestNode < 0 Then
                    Exit For
                End If

                parents(bestNode) = bestParents
                current = bestScore
            Next

            Return parents
        End Function

        ''' <summary>
        ''' 学习结构并返回装配好的(参数也一并学习的)贝叶斯网络
        ''' </summary>
        Public Function LearnNetwork(Optional maxIterations As Integer = 200) As BeliefNetwork
            Dim parents As Integer()() = HillClimbing(Nothing, maxIterations)
            Dim network As BeliefNetwork = CreateNetwork(_ranges, parents)
            Dim learner As New ParameterLearner()

            Call learner.LearnInPlace(network, _data)

            Return network
        End Function

        ''' <summary>
        ''' 由「变量取值个数 + 父节点集合」构造一个条件概率表为均匀分布的网络
        ''' </summary>
        Public Shared Function CreateNetwork(ranges As Integer(), parents As Integer()()) As BeliefNetwork
            If ranges Is Nothing OrElse ranges.Length = 0 Then
                Throw New ArgumentException("the cardinalities can not be empty", NameOf(ranges))
            End If
            If parents Is Nothing OrElse parents.Length <> ranges.Length Then
                Throw New ArgumentException("the parent sets must be aligned with the cardinalities", NameOf(parents))
            End If

            Dim nodes As BeliefNode() = New BeliefNode(ranges.Length - 1) {}

            For i As Integer = 0 To ranges.Length - 1
                nodes(i) = New BeliefNode("X" & i, i, ranges(i))
            Next

            For i As Integer = 0 To ranges.Length - 1
                Dim rows As Integer = 1

                For Each p As Integer In parents(i)
                    Call nodes(i).Parents.Add(nodes(p))
                    rows *= ranges(p)
                Next

                nodes(i).CP_Table = New Double(rows - 1, ranges(i) - 1) {}

                For r As Integer = 0 To rows - 1
                    For v As Integer = 0 To ranges(i) - 1
                        nodes(i).CP_Table(r, v) = 1.0 / ranges(i)
                    Next
                Next

                Call nodes(i).InvalidateCache()
            Next

            Return BeliefNetwork.CreateFrom(nodes)
        End Function

        ''' <summary>
        ''' 判断 <paramref name="ancestor"/> 是否是 <paramref name="node"/> 的祖先
        ''' </summary>
        Public Shared Function IsAncestor(parents As Integer()(), ancestor As Integer, node As Integer) As Boolean
            Dim stack As New Stack(Of Integer)
            Dim visited As New HashSet(Of Integer)

            For Each p As Integer In parents(node)
                Call stack.Push(p)
            Next

            While stack.Count > 0
                Dim current As Integer = stack.Pop()

                If current = ancestor Then
                    Return True
                End If
                If Not visited.Add(current) Then
                    Continue While
                End If

                For Each p As Integer In parents(current)
                    Call stack.Push(p)
                Next
            End While

            Return False
        End Function

        Private Function PairwiseMutualInformation() As Double(,)
            Dim mi As Double(,) = New Double(_n - 1, _n - 1) {}

            ' 为了节省内存，按变量对逐对统计
            For i As Integer = 0 To _n - 1
                For j As Integer = i + 1 To _n - 1
                    Dim counts As Integer(,) = New Integer(_ranges(i) - 1, _ranges(j) - 1) {}
                    Dim margI As Integer() = New Integer(_ranges(i) - 1) {}
                    Dim margJ As Integer() = New Integer(_ranges(j) - 1) {}
                    Dim total As Integer = 0

                    For Each row As Integer() In _data
                        If row(i) < 0 OrElse row(j) < 0 Then
                            Continue For
                        End If

                        counts(row(i), row(j)) += 1
                        margI(row(i)) += 1
                        margJ(row(j)) += 1
                        total += 1
                    Next

                    If total = 0 Then
                        Continue For
                    End If

                    Dim value As Double = 0.0

                    For a As Integer = 0 To _ranges(i) - 1
                        For b As Integer = 0 To _ranges(j) - 1
                            If counts(a, b) = 0 Then
                                Continue For
                            End If

                            Dim pab As Double = counts(a, b) / total
                            Dim pa As Double = margI(a) / total
                            Dim pb As Double = margJ(b) / total

                            value += pab * std.Log(pab / (pa * pb))
                        Next
                    Next

                    mi(i, j) = std.Max(value, 0.0)
                    mi(j, i) = mi(i, j)
                Next
            Next

            Return mi
        End Function

        Private Shared Function CloneStructure(parents As Integer()()) As Integer()()
            Dim clone As Integer()() = New Integer(parents.Length - 1)() {}

            For i As Integer = 0 To parents.Length - 1
                clone(i) = parents(i).ToArray()
            Next

            Return clone
        End Function
    End Class
End Namespace
