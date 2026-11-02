Imports std = System.Math

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 因子图之上的 loopy belief propagation 近似推断。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 对于树形(polytree)网络，本方法与 <see cref="VariableElimination"/> 给出完全一致的结果；
    ''' 对于含环的网络它给出的是 Bethe 近似，通常仍然足够准确，而且计算量与网络的规模成线性关系，
    ''' 因此在神经网络规模很大、精确推断不可行的时候可以退化为本方法。
    ''' </para>
    ''' <para>
    ''' 每个节点的条件概率表被当作因子图中的一个因子，因子与变量之间的消息都在对数空间之中传递，
    ''' 并且支持阻尼系数(damping)以抑制振荡、以及收敛判据与最大迭代次数。
    ''' </para>
    ''' </remarks>
    Public Class LoopyBeliefPropagation

        Private ReadOnly _network As BeliefNetwork
        Private ReadOnly _nodes As BeliefNode()
        Private ReadOnly _factors As Factor()
        Private ReadOnly _varFactors As List(Of Integer)()

        ''' <summary>
        ''' 一次 loopy BP 的运行结果
        ''' </summary>
        Public Class Result

            ''' <summary>
            ''' 每个节点的边缘后验分布
            ''' </summary>
            Public Property Marginals As Double()()
            ''' <summary>
            ''' 是否在最大迭代次数之内收敛
            ''' </summary>
            Public Property Converged As Boolean
            ''' <summary>
            ''' 实际执行的迭代次数
            ''' </summary>
            Public Property Iterations As Integer
            ''' <summary>
            ''' 最后一次迭代的最大消息变化量
            ''' </summary>
            Public Property FinalDelta As Double
        End Class

        ''' <summary>
        ''' 为指定的网络构建 BP 引擎
        ''' </summary>
        Public Sub New(network As BeliefNetwork)
            If network Is Nothing Then
                Throw New ArgumentNullException(NameOf(network))
            End If

            _network = network
            _nodes = network.Nodes
            _factors = New Factor(_nodes.Length - 1) {}
            _varFactors = Enumerable _
                .Range(0, _nodes.Length) _
                .Select(Function(i) New List(Of Integer)) _
                .ToArray()

            For i As Integer = 0 To _nodes.Length - 1
                _factors(i) = VariableElimination.BuildNodeFactor(_nodes(i), _nodes)

                For Each v As Integer In _factors(i).Variables
                    Call _varFactors(v).Add(i)
                Next
            Next
        End Sub

        ''' <summary>
        ''' 运行 loopy belief propagation
        ''' </summary>
        ''' <param name="evidence">证据数组，-1 表示未观测</param>
        ''' <param name="maxIterations">最大迭代次数</param>
        ''' <param name="tolerance">收敛判据，最大消息变化量小于该值即视为收敛</param>
        ''' <param name="damping">阻尼系数，1 表示不阻尼</param>
        Public Function Run(Optional evidence As Integer() = Nothing,
                            Optional maxIterations As Integer = 100,
                            Optional tolerance As Double = 1.0E-09,
                            Optional damping As Double = 0.5) As Result

            Dim count As Integer = _nodes.Length
            Dim factors As Factor() = New Factor(count - 1) {}

            For i As Integer = 0 To count - 1
                Dim f As Factor = _factors(i)

                If evidence IsNot Nothing Then
                    For Each v As Integer In f.Variables
                        If evidence(v) <> BeliefNode.NotObserved Then
                            f = f.Restrict(v, evidence(v))
                        End If
                    Next
                End If

                factors(i) = f
            Next

            ' 因子 -> 变量 的消息，按 (factor, variable) 索引
            Dim factorToVar As New Dictionary(Of Long, Double())
            Dim varToFactor As New Dictionary(Of Long, Double())

            For fi As Integer = 0 To count - 1
                For Each v As Integer In factors(fi).Variables
                    Dim slot As Long = MessageSlot(fi, v)
                    factorToVar(slot) = Uniform(factors(fi), v)
                    varToFactor(slot) = New Double(_nodes(v).Range - 1) {}
                Next
            Next

            Dim converged As Boolean = False
            Dim delta As Double = Double.MaxValue
            Dim iteration As Integer = 0

            For iteration = 1 To maxIterations
                delta = 0.0

                ' 因子 -> 变量
                For fi As Integer = 0 To count - 1
                    Dim f As Factor = factors(fi)

                    If f.Variables.Length = 0 Then
                        Continue For
                    End If

                    For Each v As Integer In f.Variables
                        Dim updated As Double() = ComputeFactorToVar(f, fi, v, varToFactor)
                        Dim slot As Long = MessageSlot(fi, v)
                        Dim previous As Double() = factorToVar(slot)
                        Dim blended As Double() = New Double(updated.Length - 1) {}

                        For k As Integer = 0 To updated.Length - 1
                            If Double.IsNegativeInfinity(updated(k)) OrElse Double.IsNegativeInfinity(previous(k)) Then
                                ' 对数负无穷无法参与线性插值，直接采用新消息
                                blended(k) = updated(k)
                            Else
                                blended(k) = damping * updated(k) + (1.0 - damping) * previous(k)
                            End If

                            delta = std.Max(delta, std.Abs(blended(k) - previous(k)))
                        Next

                        factorToVar(slot) = blended
                    Next
                Next

                ' 变量 -> 因子
                For v As Integer = 0 To count - 1
                    Dim containing As List(Of Integer) = _varFactors(v)

                    For Each target As Integer In containing
                        Dim updated As Double() = New Double(_nodes(v).Range - 1) {}

                        For Each other As Integer In containing
                            If other = target Then
                                Continue For
                            End If

                            Dim message As Double() = factorToVar(MessageSlot(other, v))

                            For k As Integer = 0 To updated.Length - 1
                                updated(k) += If(Double.IsNegativeInfinity(message(k)), 0.0, message(k))
                            Next
                        Next

                        Dim slot As Long = MessageSlot(target, v)
                        Dim previous As Double() = varToFactor(slot)

                        For k As Integer = 0 To updated.Length - 1
                            delta = std.Max(delta, std.Abs(updated(k) - previous(k)))
                        Next

                        varToFactor(slot) = updated
                    Next
                Next

                If delta < tolerance Then
                    converged = True
                    Exit For
                End If
            Next

            Dim marginals As Double()() = New Double(count - 1)() {}

            For v As Integer = 0 To count - 1
                Dim belief As Double() = New Double(_nodes(v).Range - 1) {}

                For Each fi As Integer In _varFactors(v)
                    Dim message As Double() = factorToVar(MessageSlot(fi, v))

                    For k As Integer = 0 To belief.Length - 1
                        belief(k) += If(Double.IsNegativeInfinity(message(k)), Double.NegativeInfinity, message(k))
                    Next
                Next

                marginals(v) = NumericLogSpace.NormalizeLog(belief)
            Next

            Return New Result With {
                .Marginals = marginals,
                .Converged = converged,
                .Iterations = std.Min(iteration, maxIterations),
                .FinalDelta = delta
            }
        End Function

        ''' <summary>
        ''' 计算单个变量的边缘后验。对于含环网络这是 Bethe 近似。
        ''' </summary>
        Public Function Marginal(evidence As Integer(), target As Integer) As Double()
            Return Run(evidence).Marginals(target)
        End Function

        Private Shared Function MessageSlot(factor As Integer, variable As Integer) As Long
            Return CLng(factor) * 1048576L + variable
        End Function

        Private Shared Function Uniform(f As Factor, v As Integer) As Double()
            Dim range As Integer = f.Cardinalities(Array.IndexOf(f.Variables, v))
            Return New Double(range - 1) {}
        End Function

        Private Function ComputeFactorToVar(f As Factor, factorIndex As Integer, v As Integer, varToFactor As Dictionary(Of Long, Double())) As Double()
            Dim variables As Integer() = f.Variables
            Dim cards As Integer() = f.Cardinalities
            Dim strides As Integer() = Factor.ComputeStrides(cards)
            Dim targetPosition As Integer = Array.IndexOf(variables, v)
            Dim range As Integer = cards(targetPosition)
            Dim result As Double() = New Double(range - 1) {}
            Dim codes As Integer() = New Integer(variables.Length - 1) {}
            Dim incoming As Double()() = New Double(variables.Length - 1)() {}

            For k As Integer = 0 To variables.Length - 1
                incoming(k) = varToFactor(MessageSlot(factorIndex, variables(k)))
            Next

            For i As Integer = 0 To f.Size - 1
                Dim logPotential As Double = f.Values(i)

                If Double.IsNegativeInfinity(logPotential) Then
                    Continue For
                End If

                Call Factor.Decode(i, cards, strides, codes)

                Dim total As Double = logPotential

                For k As Integer = 0 To variables.Length - 1
                    If k = targetPosition Then
                        Continue For
                    End If

                    total += incoming(k)(codes(k))
                Next

                result(codes(targetPosition)) = NumericLogSpace.LogSumExp(result(codes(targetPosition)), total)
            Next

            Return result
        End Function
    End Class
End Namespace
