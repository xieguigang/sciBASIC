Imports std = System.Math
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 含隐变量(缺失观测)的贝叶斯网络参数学习：期望最大化(EM)算法。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' E 步使用 <see cref="VariableElimination"/> 计算隐变量在给定观测之下的后验，
    ''' 并据此累加期望充分统计量 <c>E[N(x, pa)]</c>；M 步用 Dirichlet 平滑的最大后验估计
    ''' 更新条件概率表(见 <see cref="ParameterLearner.ApplyCounts"/>)。
    ''' </para>
    ''' <para>
    ''' 每一轮的对数似然都会被记录下来，理论上它必须单调不减；测试工程正是用这一性质来做
    ''' EM 实现的回归检验。
    ''' </para>
    ''' </remarks>
    Public Class ExpectationMaximization

        Private _alpha As Double = 1.0
        Private _maxIterations As Integer = 100
        Private _tolerance As Double = 1.0E-06

        ''' <summary>
        ''' Dirichlet 先验超参数
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
        ''' 最大迭代轮数
        ''' </summary>
        Public Property MaxIterations As Integer
            Get
                Return _maxIterations
            End Get
            Set(value As Integer)
                _maxIterations = value
            End Set
        End Property

        ''' <summary>
        ''' 对数似然相对变化量小于该值即视为收敛
        ''' </summary>
        Public Property Tolerance As Double
            Get
                Return _tolerance
            End Get
            Set(value As Double)
                _tolerance = value
            End Set
        End Property

        Private ReadOnly _network As BeliefNetwork

        ''' <summary>
        ''' EM 的运行结果
        ''' </summary>
        Public Class Result

            ''' <summary>
            ''' 学习之后得到的网络(条件概率表已被更新)
            ''' </summary>
            Public Property Network As BeliefNetwork
            ''' <summary>
            ''' 每一轮迭代结束之后的对数似然
            ''' </summary>
            Public Property LogLikelihood As Double()
            ''' <summary>
            ''' 实际执行的迭代轮数
            ''' </summary>
            Public Property Iterations As Integer
            ''' <summary>
            ''' 是否在最大迭代轮数之内收敛
            ''' </summary>
            Public Property Converged As Boolean
        End Class

        Public Sub New(network As BeliefNetwork)
            If network Is Nothing Then
                Throw New ArgumentNullException(NameOf(network))
            End If

            _network = network
        End Sub

        ''' <summary>
        ''' 在含有缺失值的数据上运行 EM
        ''' </summary>
        ''' <param name="data">观测数据，每行长度等于节点数，取值 -1 表示该变量缺失</param>
        Public Function Run(data As Integer()()) As Result
            Dim network As BeliefNetwork = _network.Clone()
            Dim nodes As BeliefNode() = network.Nodes
            Dim history As New List(Of Double)
            Dim previous As Double = Double.NegativeInfinity
            Dim converged As Boolean = False
            Dim iteration As Integer = 0

            For iteration = 1 To MaxIterations
                Dim engine As New VariableElimination(network)
                Dim likelihood As Double = 0.0

                For Each row As Integer() In data
                    Dim value As Double = engine.LogPartition(row)

                    If Double.IsNegativeInfinity(value) Then
                        DebugAudit.Check(False, "a sample has zero probability under the current parameters")
                    Else
                        likelihood += value
                    End If
                Next

                Call history.Add(likelihood)

                ' E 步：累加期望充分统计量
                Dim counts As Double()()() = ExpectedCounts(engine, nodes, data)

                ' M 步：Dirichlet 平滑的最大后验估计
                For i As Integer = 0 To nodes.Length - 1
                    Call ParameterLearner.ApplyCounts(nodes(i), counts(i), Alpha)
                Next

                If iteration > 1 Then
                    Dim change As Double = std.Abs(likelihood - previous)

                    If change <= Tolerance * std.Max(1.0, std.Abs(previous)) Then
                        converged = True
                        Exit For
                    End If
                End If

                previous = likelihood
            Next

            Return New Result With {
                .Network = network,
                .LogLikelihood = history.ToArray(),
                .Iterations = std.Min(iteration, MaxIterations),
                .Converged = converged
            }
        End Function

        ''' <summary>
        ''' 计算期望充分统计量 <c>E[N(x, pa)]</c>
        ''' </summary>
        Public Shared Function ExpectedCounts(engine As VariableElimination, nodes As BeliefNode(), data As Integer()()) As Double()()()
            Dim counts As Double()()() = New Double(nodes.Length - 1)()() {}

            For i As Integer = 0 To nodes.Length - 1
                Dim table As Double()() = New Double(nodes(i).RowCount - 1)() {}

                For r As Integer = 0 To nodes(i).RowCount - 1
                    table(r) = New Double(nodes(i).Range - 1) {}
                Next

                counts(i) = table
            Next

            For Each row As Integer() In data
                If row Is Nothing Then
                    Continue For
                End If

                Dim probabilities As Double()() = New Double(nodes.Length - 1)() {}
                Dim scopes As Integer()() = New Integer(nodes.Length - 1)() {}

                For i As Integer = 0 To nodes.Length - 1
                    Dim node As BeliefNode = nodes(i)
                    Dim scope As Integer() = node.Parents.Select(Function(p) p.Id).Concat({node.Id}).Distinct().OrderBy(Function(v) v).ToArray()
                    scopes(i) = scope

                    Dim hidden As Boolean = scope.Any(Function(v) row(v) < 0)

                    If Not hidden Then
                        ' 全部观测到：可以确定地累加 1
                        probabilities(i) = Nothing
                    Else
                        probabilities(i) = engine.Query(row, scope)
                    End If
                Next

                For i As Integer = 0 To nodes.Length - 1
                    Dim node As BeliefNode = nodes(i)
                    Dim scope As Integer() = scopes(i)
                    Dim parentValues As Integer() = New Integer(node.Parents.Count - 1) {}

                    If probabilities(i) Is Nothing Then
                        For k As Integer = 0 To node.Parents.Count - 1
                            parentValues(k) = row(node.Parents(k).Id)
                        Next

                        Dim r As Integer = node.RowIndex(parentValues)
                        counts(i)(r)(row(node.Id)) += 1.0
                        Continue For
                    End If

                    Dim cards As Integer() = scope.Select(Function(v) nodes(v).Range).ToArray()
                    Dim strides As Integer() = Factor.ComputeStrides(cards)
                    Dim codes As Integer() = New Integer(scope.Length - 1) {}

                    For cell As Integer = 0 To probabilities(i).Length - 1
                        Dim p As Double = probabilities(i)(cell)

                        If p <= 0 Then
                            Continue For
                        End If

                        Call Factor.Decode(cell, cards, strides, codes)

                        For k As Integer = 0 To node.Parents.Count - 1
                            parentValues(k) = codes(Array.IndexOf(scope, node.Parents(k).Id))
                        Next

                        Dim r As Integer = node.RowIndex(parentValues)
                        Dim value As Integer = codes(Array.IndexOf(scope, node.Id))
                        counts(i)(r)(value) += p
                    Next
                Next
            Next

            Return counts
        End Function
    End Class
End Namespace
