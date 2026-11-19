Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Threading

Namespace BooleanNetworks

    ''' <summary>
    ''' BN 与 PBN 的公共基类：节点容器、状态读写、同步/异步步进与轨迹分析。
    ''' </summary>
    ''' <remarks>
    ''' 基类不决定节点如何求值——由派生类实现 <see cref="EvaluateNode"/>：
    ''' BN 使用确定性规则，PBN 按概率采样规则。其余调度逻辑完全共享。
    ''' </remarks>
    Public MustInherit Class BooleanNetworkBase

        Private ReadOnly _nodes As BooleanNode()
        Private ReadOnly _dependents As List(Of Integer)()
        Private _state As Boolean()
        Private _seed As Integer
        Private _random As Random
        Private _sampleCount As Long

        <ThreadStatic>
        Private Shared _nextBuffer As Boolean()

        ''' <summary>以节点集合和初始状态构造网络。</summary>
        Public Sub New(nodes As BooleanNode(), initialState As Boolean(), Optional seed As Integer = 12345)
            If nodes Is Nothing OrElse nodes.Length = 0 Then
                Throw New ArgumentException("network requires at least one node", NameOf(nodes))
            End If
            If initialState Is Nothing Then
                Throw New ArgumentNullException(NameOf(initialState))
            End If
            If initialState.Length <> nodes.Length Then
                Throw New ArgumentException($"initial state length {initialState.Length} does not match node count {nodes.Length}", NameOf(initialState))
            End If

            For i As Integer = 0 To nodes.Length - 1
                If nodes(i) Is Nothing Then
                    Throw New ArgumentException($"node [{i}] is empty", NameOf(nodes))
                End If
                For Each parent As Integer In nodes(i).Parents
                    If parent < 0 OrElse parent >= nodes.Length Then
                        Throw New ArgumentException($"node [{i}] has invalid parent index {parent}", NameOf(nodes))
                    End If
                Next
            Next

            _nodes = DirectCast(nodes.Clone(), BooleanNode())
            _state = StateSpace.Copy(initialState)
            _seed = seed
            _random = New Random(seed)
            _dependents = BuildDependents(_nodes)
        End Sub

        ''' <summary>网络节点（防御性副本）。</summary>
        Public ReadOnly Property Nodes As BooleanNode()
            Get
                Return DirectCast(_nodes.Clone(), BooleanNode())
            End Get
        End Property

        ''' <summary>基因个数。</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _nodes.Length
            End Get
        End Property

        ''' <summary>当前布尔状态（防御性副本）。</summary>
        Public Property State As Boolean()
            Get
                Return StateSpace.Copy(_state)
            End Get
            Set(value As Boolean())
                If value Is Nothing Then
                    Throw New ArgumentNullException(NameOf(value))
                End If
                If value.Length <> _nodes.Length Then
                    Throw New ArgumentException($"state length must be {_nodes.Length}", NameOf(value))
                End If

                _state = StateSpace.Copy(value)
            End Set
        End Property

        ''' <summary>按基因名读取当前状态。</summary>
        Public Function StateOf(name As String) As Boolean
            Dim index As Integer = IndexOf(name)

            If index < 0 Then
                Throw New ArgumentException($"unknown gene '{name}'", NameOf(name))
            End If

            Return _state(index)
        End Function

        ''' <summary>按名称查找基因索引；不存在返回 -1。</summary>
        Public Function IndexOf(name As String) As Integer
            For i As Integer = 0 To _nodes.Length - 1
                If String.Equals(_nodes(i).Name, name, StringComparison.Ordinal) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        ''' <summary>随机种子。</summary>
        Public Property Seed As Integer
            Get
                Return _seed
            End Get
            Set(value As Integer)
                _seed = value
                _random = New Random(value)
            End Set
        End Property

        ''' <summary>已完成的规则采样次数（BN 始终为 0）。</summary>
        Public ReadOnly Property SampleCount As Long
            Get
                Return _sampleCount
            End Get
        End Property

        Friend Sub IncrementSampleCount()
            _sampleCount += 1
        End Sub

        Friend ReadOnly Property SharedRandom As Random
            Get
                Return _random
            End Get
        End Property

        ''' <summary>派生类实现单节点求值语义。</summary>
        Protected MustOverride Function EvaluateNode(node As BooleanNode, inputs As Boolean()) As Boolean

        ''' <summary>从连续表达值设置状态。</summary>
        Public Sub SetContinuousState(values As Double(), binarizer As IStateBinarizer)
            If binarizer Is Nothing Then
                Throw New ArgumentNullException(NameOf(binarizer))
            End If

            State = binarizer.BinarizeRow(values)
        End Sub

        ''' <summary>为每个基因使用独立二值化器从连续表达值设置状态。</summary>
        Public Sub SetContinuousState(values As Double(), binarizers As IStateBinarizer())
            State = BinarizationHelper.BinarizeRow(binarizers, values)
        End Sub

        ''' <summary>同步步进：所有节点同时基于上一时刻状态求值。</summary>
        Public Function StepSynchronous() As Boolean()
            Dim nextState As Boolean() = NextBuffer(_nodes.Length)

            For i As Integer = 0 To _nodes.Length - 1
                Dim parents As Integer() = _nodes(i).Parents
                Dim inputs(parents.Length - 1) As Boolean

                For j As Integer = 0 To parents.Length - 1
                    inputs(j) = _state(parents(j))
                Next

                nextState(i) = EvaluateNode(_nodes(i), inputs)
            Next

            _state = StateSpace.Copy(nextState)

            Return State
        End Function

        ''' <summary>
        ''' 异步步进：随机选一个节点更新，并沿依赖图传播到所有受影响的节点。
        ''' </summary>
        ''' <remarks>
        ''' 只重算受影响基因；每个基因在一次异步步内最多更新一次，避免环上反复震荡。
        ''' </remarks>
        Public Function StepAsynchronous() As Boolean()
            Dim seedNode As Integer = _random.Next(_nodes.Length)
            Dim pending As New Queue(Of Integer)()
            Dim scheduled As New HashSet(Of Integer)()

            Call pending.Enqueue(seedNode)
            Call scheduled.Add(seedNode)

            While pending.Count > 0
                Dim index As Integer = pending.Dequeue()
                Dim oldValue As Boolean = _state(index)
                Dim parents As Integer() = _nodes(index).Parents
                Dim inputs(parents.Length - 1) As Boolean

                For j As Integer = 0 To parents.Length - 1
                    inputs(j) = _state(parents(j))
                Next

                _state(index) = EvaluateNode(_nodes(index), inputs)

                If _state(index) <> oldValue Then
                    For Each child As Integer In _dependents(index)
                        If scheduled.Add(child) Then
                            Call pending.Enqueue(child)
                        End If
                    Next
                End If
            End While

            Return State
        End Function

        ''' <summary>连续多步仿真并返回轨迹（含初始状态）。</summary>
        Public Function Run(steps As Integer, Optional asynchronous As Boolean = False) As StateTrajectory
            If steps < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(steps), steps, "step count cannot be negative")
            End If

            Dim trajectory As New StateTrajectory()

            Call trajectory.Append(_state)

            For i As Integer = 1 To steps
                If asynchronous Then
                    Call StepAsynchronous()
                Else
                    Call StepSynchronous()
                End If

                Call trajectory.Append(_state)
            Next

            Return trajectory
        End Function

        ''' <summary>仿真直到检测到吸引子或达到最大步数。</summary>
        Public Function RunToAttractor(Optional maxSteps As Integer = 10000,
                                       Optional asynchronous As Boolean = False) As AttractorResult
            If maxSteps <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(maxSteps), maxSteps, "max steps must be positive")
            End If

            Dim trajectory As New StateTrajectory()

            Call trajectory.Append(_state)

            For i As Integer = 1 To maxSteps
                If asynchronous Then
                    Call StepAsynchronous()
                Else
                    Call StepSynchronous()
                End If

                Call trajectory.Append(_state)

                Dim found As AttractorResult = trajectory.DetectAttractor()

                If found IsNot Nothing Then
                    Return found
                End If
            Next

            Return Nothing
        End Function

        Private Shared Function NextBuffer(length As Integer) As Boolean()
            If _nextBuffer Is Nothing OrElse _nextBuffer.Length <> length Then
                _nextBuffer = New Boolean(length - 1) {}
            End If

            Return _nextBuffer
        End Function

        Private Shared Function BuildDependents(nodes As BooleanNode()) As List(Of Integer)()
            Dim result(nodes.Length - 1) As List(Of Integer)

            For i As Integer = 0 To nodes.Length - 1
                result(i) = New List(Of Integer)()
            Next

            For i As Integer = 0 To nodes.Length - 1
                For Each parent As Integer In nodes(i).Parents
                    Call result(parent).Add(i)
                Next
            Next

            Return result
        End Function
    End Class

End Namespace
