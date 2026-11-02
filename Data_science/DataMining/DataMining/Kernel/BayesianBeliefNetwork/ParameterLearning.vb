Imports std = System.Math
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 贝叶斯网络的参数(条件概率表)学习。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 采用 Dirichlet-多项共轭模型：先验 <c>Dirichlet(α)</c> 之下，最大后验估计为
    ''' </para>
    ''' <code>
    ''' θ(x | pa) = (N(x, pa) + α) / (N(pa) + α·|X|)
    ''' </code>
    ''' <para>
    ''' 这正是论文(readme 第 5.2 节)给出的 CPT 估计式。当某个父节点配置在数据之中从未出现过
    ''' (<c>N(pa) = 0</c>) 的时候，估计式退化为均匀分布 <c>1/|X|</c>，而不是像纯最大似然那样
    ''' 变成 0/0 —— 旧实现里「零计数配置直接退化为均匀分布」的分支正是缺少平滑的结果。
    ''' </para>
    ''' </remarks>
    Public Class ParameterLearner

        Private _alpha As Double = 1.0

        ''' <summary>
        ''' Dirichlet 先验的超参数，默认取 1(即加一平滑 / Laplace 平滑)
        ''' </summary>
        Public Property Alpha As Double
            Get
                Return _alpha
            End Get
            Set(value As Double)
                _alpha = value
            End Set
        End Property

        ''' <summary>
        ''' 为每个节点单独指定先验超参数；为 <c>Nothing</c> 的时候统一使用 <see cref="Alpha"/>
        ''' </summary>
        Public Property PerNodeAlpha As Double()

        Public Sub New()
        End Sub

        Public Sub New(alpha As Double)
            _alpha = alpha
        End Sub

        Private Function AlphaOf(nodeId As Integer) As Double
            If PerNodeAlpha IsNot Nothing AndAlso nodeId < PerNodeAlpha.Length Then
                Return PerNodeAlpha(nodeId)
            Else
                Return _alpha
            End If
        End Function

        ''' <summary>
        ''' 由观测数据学习条件概率表，返回一个新的网络(结构保持不变，原网络不会被修改)
        ''' </summary>
        ''' <param name="network">提供网络结构的贝叶斯网络</param>
        ''' <param name="data">
        ''' 观测数据，每行的长度必须等于节点数；取值 <c>-1</c> 表示该变量在该样本之中缺失
        ''' (缺失值不参与计数，含隐变量的情形请使用 <see cref="ExpectationMaximization"/>)
        ''' </param>
        Public Function Learn(network As BeliefNetwork, data As Integer()()) As BeliefNetwork
            Dim target As BeliefNetwork = network.Clone()
            Call LearnInPlace(target, data)
            Return target
        End Function

        ''' <summary>
        ''' 由观测数据就地更新网络的条件概率表
        ''' </summary>
        Public Sub LearnInPlace(network As BeliefNetwork, data As Integer()())
            Dim counts As Double()()() = CountData(network, data)

            For i As Integer = 0 To network.Count - 1
                Dim node As BeliefNode = network.Nodes(i)
                Call ApplyCounts(node, counts(i), AlphaOf(i))
            Next
        End Sub

        ''' <summary>
        ''' 按照 Dirichlet 平滑把计数转换为条件概率表
        ''' </summary>
        Public Shared Sub ApplyCounts(node As BeliefNode, counts As Double()(), alpha As Double)
            Dim rows As Integer = node.RowCount
            Dim range As Integer = node.Range

            For r As Integer = 0 To rows - 1
                Dim total As Double = 0.0

                For v As Integer = 0 To range - 1
                    total += counts(r)(v)
                Next

                Dim denominator As Double = total + alpha * range

                For v As Integer = 0 To range - 1
                    If denominator <= 0 Then
                        node.CP_Table(r, v) = 1.0 / range
                    Else
                        node.CP_Table(r, v) = (counts(r)(v) + alpha) / denominator
                    End If
                Next
            Next

            Call node.InvalidateCache()
        End Sub

        ''' <summary>
        ''' 统计每个节点的 <c>N(x, pa)</c> 计数
        ''' </summary>
        ''' <returns>返回 <c>counts(nodeId)(row)(value)</c> 的三维数组</returns>
        Public Shared Function CountData(network As BeliefNetwork, data As Integer()()) As Double()()()
            Dim nodes As BeliefNode() = network.Nodes
            Dim result As Double()()() = New Double(nodes.Length - 1)()() {}

            For i As Integer = 0 To nodes.Length - 1
                Dim table As Double()() = New Double(nodes(i).RowCount - 1)() {}

                For r As Integer = 0 To nodes(i).RowCount - 1
                    table(r) = New Double(nodes(i).Range - 1) {}
                Next

                result(i) = table
            Next

            Dim maxParents As Integer = 0

            For i As Integer = 0 To nodes.Length - 1
                If nodes(i).Parents.Count > maxParents Then
                    maxParents = nodes(i).Parents.Count
                End If
            Next

            Dim parentValues As Integer() = New Integer(maxParents - 1) {}

            For Each row As Integer() In data
                If row Is Nothing Then
                    Continue For
                End If

                For i As Integer = 0 To nodes.Length - 1
                    Dim node As BeliefNode = nodes(i)
                    Dim value As Integer = row(i)

                    If value < 0 Then
                        Continue For
                    End If

                    Dim complete As Boolean = True

                    For k As Integer = 0 To node.Parents.Count - 1
                        Dim pv As Integer = row(node.Parents(k).Id)

                        If pv < 0 Then
                            complete = False
                            Exit For
                        End If

                        parentValues(k) = pv
                    Next

                    If Not complete Then
                        Continue For
                    End If

                    Dim r As Integer = node.RowIndex(parentValues.Take(node.Parents.Count).ToArray())
                    result(i)(r)(value) += 1.0
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 计算数据在给定网络之下的对数似然(只统计完整观测的样本)
        ''' </summary>
        Public Shared Function LogLikelihood(network As BeliefNetwork, data As Integer()()) As Double
            Dim engine As New VariableElimination(network)
            Dim total As Double = 0.0

            For Each row As Integer() In data
                If row Is Nothing Then
                    Continue For
                End If

                Dim value As Double = engine.LogPartition(row)

                If Double.IsNegativeInfinity(value) Then
                    Return Double.NegativeInfinity
                End If

                total += value
            Next

            Return total
        End Function

        ''' <summary>
        ''' 计算单个样本在给定网络之下的对数概率
        ''' </summary>
        Public Shared Function LogProbability(network As BeliefNetwork, sample As Integer()) As Double
            Dim total As Double = 0.0
            Dim parentValues As Integer() = New Integer(0) {}

            For Each node As BeliefNode In network.Nodes
                Dim value As Integer = sample(node.Id)

                If value < 0 Then
                    Continue For
                End If

                If node.Parents.Count > 0 Then
                    parentValues = New Integer(node.Parents.Count - 1) {}

                    For k As Integer = 0 To node.Parents.Count - 1
                        parentValues(k) = sample(node.Parents(k).Id)
                    Next
                End If

                Dim p As Double = node.CP_Table(node.RowIndex(parentValues), value)

                DebugAudit.Check(p >= 0 AndAlso p <= 1, $"invalid conditional probability {p} on node '{node.Name}'")

                If p <= 0 Then
                    Return Double.NegativeInfinity
                End If

                total += std.Log(p)
            Next

            Return total
        End Function
    End Class
End Namespace
