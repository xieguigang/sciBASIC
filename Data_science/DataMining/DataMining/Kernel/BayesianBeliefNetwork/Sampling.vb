Imports std = System.Math

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 贝叶斯网络之上的三类采样算法：祖先采样(ancestral sampling)、
    ''' Gibbs 采样以及似然加权(likelihood weighting)。
    ''' </summary>
    ''' <remarks>
    ''' 采样是精确推断不可行时的近似手段；在本项目之中它主要用于
    ''' (1) 生成结构学习 / EM 的合成测试数据，
    ''' (2) 对互作分配的後验进行随机搜索(Burger &amp; van Nimwegen 的 Gibbs 分配采样器)。
    ''' 所有函数都接受外部注入的 <see cref="Random"/> 实例，因此只要固定种子就可以得到
    ''' 完全可复现的结果。
    ''' </remarks>
    Public Module Sampling

        ''' <summary>
        ''' Gibbs 采样的运行结果
        ''' </summary>
        Public Class GibbsResult

            ''' <summary>
            ''' 每个变量的边缘频率分布
            ''' </summary>
            Public Property Marginals As Double()()
            ''' <summary>
            ''' 实际收集到的样本个数
            ''' </summary>
            Public Property Samples As Integer
            ''' <summary>
            ''' 最后一次采样的完整赋值
            ''' </summary>
            Public Property LastState As Integer()
        End Class

        ''' <summary>
        ''' 计算每个节点的子节点列表(下标已经按照拓扑序排列)
        ''' </summary>
        Public Function Children(network As BeliefNetwork) As List(Of Integer)()
            Dim nodes As BeliefNode() = network.Nodes
            Dim childList As List(Of Integer)() = Enumerable _
                .Range(0, nodes.Length) _
                .Select(Function(i) New List(Of Integer)) _
                .ToArray()

            For i As Integer = 0 To nodes.Length - 1
                For Each parent As BeliefNode In nodes(i).Parents
                    Call childList(parent.Id).Add(i)
                Next
            Next

            Return childList
        End Function

        ''' <summary>
        ''' 从条件概率表的某一行采样一个取值
        ''' </summary>
        Public Function SampleFromRow(node As BeliefNode, parentValues As Integer(), rng As Random) As Integer
            Dim row As Integer = node.RowIndex(parentValues)
            Dim threshold As Double = rng.NextDouble()
            Dim cumulative As Double = 0.0

            For v As Integer = 0 To node.Range - 2
                cumulative += node.CP_Table(row, v)

                If threshold <= cumulative Then
                    Return v
                End If
            Next

            Return node.Range - 1
        End Function

        ''' <summary>
        ''' 祖先采样：按照拓扑序依次采样每个节点。被观测的节点直接采用证据值。
        ''' </summary>
        Public Function AncestralSample(network As BeliefNetwork, rng As Random, Optional evidence As Integer() = Nothing) As Integer()
            Dim nodes As BeliefNode() = network.Nodes
            Dim sample As Integer() = New Integer(nodes.Length - 1) {}
            Dim parentValues As Integer() = New Integer() {}

            For i As Integer = 0 To nodes.Length - 1
                Dim node As BeliefNode = nodes(i)

                If evidence IsNot Nothing AndAlso i < evidence.Length AndAlso evidence(i) <> BeliefNode.NotObserved Then
                    sample(i) = evidence(i)
                    Continue For
                End If

                If node.Parents.Count > 0 Then
                    parentValues = New Integer(node.Parents.Count - 1) {}

                    For k As Integer = 0 To node.Parents.Count - 1
                        parentValues(k) = sample(node.Parents(k).Id)
                    Next
                End If

                sample(i) = SampleFromRow(node, parentValues, rng)
            Next

            Return sample
        End Function

        ''' <summary>
        ''' 似然加权采样：非证据节点按照先验采样，证据节点不采样而是把它的条件概率乘进权重。
        ''' </summary>
        Public Function LikelihoodWeightedSample(network As BeliefNetwork, rng As Random, evidence As Integer(), ByRef weight As Double) As Integer()
            Dim nodes As BeliefNode() = network.Nodes
            Dim sample As Integer() = New Integer(nodes.Length - 1) {}
            Dim parentValues As Integer() = New Integer() {}

            weight = 1.0

            For i As Integer = 0 To nodes.Length - 1
                Dim node As BeliefNode = nodes(i)

                If node.Parents.Count > 0 Then
                    parentValues = New Integer(node.Parents.Count - 1) {}

                    For k As Integer = 0 To node.Parents.Count - 1
                        parentValues(k) = sample(node.Parents(k).Id)
                    Next
                End If

                If evidence IsNot Nothing AndAlso i < evidence.Length AndAlso evidence(i) <> BeliefNode.NotObserved Then
                    sample(i) = evidence(i)
                    weight *= node.CP_Table(node.RowIndex(parentValues), sample(i))
                Else
                    sample(i) = SampleFromRow(node, parentValues, rng)
                End If
            Next

            Return sample
        End Function

        ''' <summary>
        ''' 用似然加权估计单个变量的边缘后验分布
        ''' </summary>
        Public Function LikelihoodWeighting(network As BeliefNetwork,
                                            rng As Random,
                                            samples As Integer,
                                            evidence As Integer(),
                                            target As Integer) As Double()

            Dim range As Integer = network.Nodes(target).Range
            Dim weights As Double() = New Double(range - 1) {}
            Dim total As Double = 0.0

            For i As Integer = 1 To samples
                Dim w As Double = 0.0
                Dim sample As Integer() = LikelihoodWeightedSample(network, rng, evidence, w)

                weights(sample(target)) += w
                total += w
            Next

            If total <= 0 Then
                Return New Double(range - 1) {}
            End If

            For k As Integer = 0 To range - 1
                weights(k) /= total
            Next

            Return weights
        End Function

        ''' <summary>
        ''' 在贝叶斯网络之上运行 Gibbs 采样，返回各变量的边缘频率分布。
        ''' </summary>
        ''' <param name="network">目标网络</param>
        ''' <param name="rng">随机数发生器</param>
        ''' <param name="iterations">总的扫描次数(每一轮会依次更新全部未观测节点)</param>
        ''' <param name="evidence">证据数组，-1 表示未观测</param>
        ''' <param name="burnIn">前若干轮扫描丢弃不计入频率统计</param>
        Public Function GibbsSampler(network As BeliefNetwork,
                                     rng As Random,
                                     iterations As Integer,
                                     Optional evidence As Integer() = Nothing,
                                     Optional burnIn As Integer = 0) As GibbsResult

            Dim nodes As BeliefNode() = network.Nodes
            Dim childList As List(Of Integer)() = Sampling.Children(network)
            Dim state As Integer() = New Integer(nodes.Length - 1) {}
            Dim observed As Boolean() = New Boolean(nodes.Length - 1) {}
            Dim counts As Integer()() = New Integer(nodes.Length - 1)() {}
            Dim collected As Integer = 0

            For i As Integer = 0 To nodes.Length - 1
                counts(i) = New Integer(nodes(i).Range - 1) {}
            Next

            ' 初始化：从先验采样，然后把观测值覆盖回去
            Dim initial As Integer() = AncestralSample(network, rng)

            For i As Integer = 0 To nodes.Length - 1
                If evidence IsNot Nothing AndAlso i < evidence.Length AndAlso evidence(i) <> BeliefNode.NotObserved Then
                    state(i) = evidence(i)
                    observed(i) = True
                Else
                    state(i) = initial(i)
                    observed(i) = False
                End If
            Next

            Dim parentValues As Integer() = New Integer() {}

            For iteration As Integer = 1 To iterations
                For i As Integer = 0 To nodes.Length - 1
                    If observed(i) Then
                        Continue For
                    End If

                    Dim node As BeliefNode = nodes(i)

                    If node.Parents.Count > 0 Then
                        parentValues = New Integer(node.Parents.Count - 1) {}

                        For k As Integer = 0 To node.Parents.Count - 1
                            parentValues(k) = state(node.Parents(k).Id)
                        Next
                    End If

                    Dim row As Integer = node.RowIndex(parentValues)
                    Dim weights As Double() = New Double(node.Range - 1) {}

                    For v As Integer = 0 To node.Range - 1
                        Dim p As Double = node.CP_Table(row, v)

                        For Each childId As Integer In childList(i)
                            Dim child As BeliefNode = nodes(childId)
                            Dim childParents As Integer() = New Integer(child.Parents.Count - 1) {}

                            For k As Integer = 0 To child.Parents.Count - 1
                                Dim pid As Integer = child.Parents(k).Id
                                childParents(k) = If(pid = i, v, state(pid))
                            Next

                            p *= child.CP_Table(child.RowIndex(childParents), state(childId))
                        Next

                        weights(v) = p
                    Next

                    state(i) = SampleCategorical(weights, rng)
                Next

                If iteration > burnIn Then
                    For i As Integer = 0 To nodes.Length - 1
                        counts(i)(state(i)) += 1
                    Next

                    collected += 1
                End If
            Next

            Dim marginals As Double()() = New Double(nodes.Length - 1)() {}

            For i As Integer = 0 To nodes.Length - 1
                Dim distribution As Double() = New Double(nodes(i).Range - 1) {}

                If collected > 0 Then
                    For v As Integer = 0 To nodes(i).Range - 1
                        distribution(v) = counts(i)(v) / collected
                    Next
                End If

                marginals(i) = distribution
            Next

            Return New GibbsResult With {
                .Marginals = marginals,
                .Samples = collected,
                .LastState = state
            }
        End Function

        ''' <summary>
        ''' 从一个未归一化的权重向量采样一个下标
        ''' </summary>
        Public Function SampleCategorical(weights As Double(), rng As Random) As Integer
            Dim total As Double = 0.0

            For Each w As Double In weights
                If w > 0 Then
                    total += w
                End If
            Next

            If total <= 0 Then
                Return 0
            End If

            Dim threshold As Double = rng.NextDouble() * total
            Dim cumulative As Double = 0.0

            For v As Integer = 0 To weights.Length - 1
                If weights(v) > 0 Then
                    cumulative += weights(v)

                    If threshold <= cumulative Then
                        Return v
                    End If
                End If
            Next

            Return weights.Length - 1
        End Function
    End Module
End Namespace
