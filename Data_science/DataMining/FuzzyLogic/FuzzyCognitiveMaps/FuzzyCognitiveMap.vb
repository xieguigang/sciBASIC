Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports std = System.Math
Imports BN = Microsoft.VisualBasic.DataMining.FuzzyLogic.BooleanNetworks

Namespace FuzzyCognitiveMaps

    ''' <summary>
    ''' 模糊认知图内核：加权邻接矩阵上的同步激活传播。
    ''' </summary>
    ''' <remarks>
    ''' 更新公式为 <c>c_i(t+1) = f( sum_j w_ji * c_j(t) )</c>，其中 <c>w_ji</c> 是概念 j 到概念 i 的因果权重。
    ''' 权重与激活值均可正可负（或落在 <c>[0,1]</c>，取决于所选激活函数）。
    ''' </remarks>
    Public NotInheritable Class FuzzyCognitiveMap

        Private ReadOnly _concepts As FcmConcept()
        Private ReadOnly _weights As Double(,)
        Private _activation As Double()
        Private _function As IActivationFunction
        Private _includeSelfLoop As Boolean

        <ThreadStatic>
        Private Shared _sumBuffer As Double()

        <ThreadStatic>
        Private Shared _nextBuffer As Double()

        ''' <summary>以概念集合和权重矩阵构造 FCM。</summary>
        Public Sub New(concepts As FcmConcept(), weights As Double(,),
                       Optional activationFunction As IActivationFunction = Nothing,
                       Optional includeSelfLoop As Boolean = False)
            If concepts Is Nothing OrElse concepts.Length = 0 Then
                Throw New ArgumentException("FCM requires at least one concept", NameOf(concepts))
            End If
            If weights Is Nothing Then
                Throw New ArgumentNullException(NameOf(weights))
            End If
            Dim n As Integer = concepts.Length

            If weights.GetLength(0) <> n OrElse weights.GetLength(1) <> n Then
                Throw New ArgumentException($"weight matrix must be {n}x{n}", NameOf(weights))
            End If
            For i As Integer = 0 To concepts.Length - 1
                If concepts(i) Is Nothing Then
                    Throw New ArgumentException($"concept [{i}] is empty", NameOf(concepts))
                End If
                For j As Integer = 0 To n - 1
                    If Double.IsNaN(weights(i, j)) OrElse Double.IsInfinity(weights(i, j)) Then
                        Throw New ArgumentException($"weight [{i},{j}] must be finite", NameOf(weights))
                    End If
                Next
            Next

            _concepts = DirectCast(concepts.Clone(), FcmConcept())
            _weights = DirectCast(weights.Clone(), Double(,))
            _function = If(activationFunction, New SigmoidActivation(1.0))
            _includeSelfLoop = includeSelfLoop
            _activation = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                _activation(i) = _concepts(i).InitialValue
                Call RefreshIncoming(i)
            Next
        End Sub

        ''' <summary>概念节点（防御性副本）。</summary>
        Public ReadOnly Property Concepts As FcmConcept()
            Get
                Return DirectCast(_concepts.Clone(), FcmConcept())
            End Get
        End Property

        ''' <summary>概念个数。</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _concepts.Length
            End Get
        End Property

        ''' <summary>当前激活向量（防御性副本）。</summary>
        Public Property Activation As Double()
            Get
                Return DirectCast(_activation.Clone(), Double())
            End Get
            Set(value As Double())
                If value Is Nothing Then
                    Throw New ArgumentNullException(NameOf(value))
                End If
                If value.Length <> _concepts.Length Then
                    Throw New ArgumentException($"activation length must be {_concepts.Length}", NameOf(value))
                End If

                _activation = DirectCast(value.Clone(), Double())
            End Set
        End Property

        ''' <summary>激活函数。</summary>
        Public Property ActivationFunction As IActivationFunction
            Get
                Return _function
            End Get
            Set(value As IActivationFunction)
                If value Is Nothing Then
                    Throw New ArgumentNullException(NameOf(value))
                End If

                _function = value
            End Set
        End Property

        ''' <summary>是否允许节点自身权重参与传播。</summary>
        Public Property IncludeSelfLoop As Boolean
            Get
                Return _includeSelfLoop
            End Get
            Set(value As Boolean)
                If _includeSelfLoop <> value Then
                    _includeSelfLoop = value

                    For i As Integer = 0 To _concepts.Length - 1
                        Call RefreshIncoming(i)
                    Next
                End If
            End Set
        End Property

        ''' <summary>读取权重 <c>w(source, target)</c>。</summary>
        Public Function GetWeight(source As Integer, target As Integer) As Double
            Call ValidateIndex(source, NameOf(source))
            Call ValidateIndex(target, NameOf(target))

            Return _weights(source, target)
        End Function

        ''' <summary>设置权重 <c>w(source, target)</c> 并同步 concepts 的入边视图。</summary>
        Public Sub SetWeight(source As Integer, target As Integer, value As Double)
            Call ValidateIndex(source, NameOf(source))
            Call ValidateIndex(target, NameOf(target))
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentException("weight must be finite", NameOf(value))
            End If

            _weights(source, target) = value
            Call RefreshIncoming(target)
        End Sub

        ''' <summary>按名称查找概念索引；不存在返回 -1。</summary>
        Public Function IndexOf(name As String) As Integer
            For i As Integer = 0 To _concepts.Length - 1
                If String.Equals(_concepts(i).Name, name, StringComparison.Ordinal) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        ''' <summary>按名称读取当前激活值。</summary>
        Public Function ActivationOf(name As String) As Double
            Dim index As Integer = IndexOf(name)

            If index < 0 Then
                Throw New ArgumentException($"unknown concept '{name}'", NameOf(name))
            End If

            Return _activation(index)
        End Function

        ''' <summary>同步传播一步。</summary>
        Public Function Propagate() As Double()
            Dim n As Integer = _concepts.Length
            Dim sums As Double() = SumBuffer(n)
            Dim nextState As Double() = NextBuffer(n)

            For i As Integer = 0 To n - 1
                sums(i) = 0.0
            Next

            For source As Integer = 0 To n - 1
                Dim value As Double = _activation(source)

                If value = 0.0 Then
                    Continue For
                End If

                For target As Integer = 0 To n - 1
                    If Not _includeSelfLoop AndAlso source = target Then
                        Continue For
                    End If

                    sums(target) += _weights(source, target) * value
                Next
            Next

            For i As Integer = 0 To n - 1
                nextState(i) = ClampToFunction(_function.Evaluate(sums(i)))
            Next

            _activation = DirectCast(nextState.Clone(), Double())

            Return Activation
        End Function

        ''' <summary>连续多步传播，返回每步的激活向量（含初始状态）。</summary>
        Public Function Run(steps As Integer) As List(Of Double())
            If steps < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(steps), steps, "step count cannot be negative")
            End If

            Dim history As New List(Of Double())()

            Call history.Add(Activation)

            For i As Integer = 1 To steps
                Call Propagate()
                Call history.Add(Activation)
            Next

            Return history
        End Function

        ''' <summary>
        ''' 传播直到前后两步的最大激活差小于阈值。
        ''' </summary>
        ''' <returns>收敛时返回步数；达到上限仍未收敛返回 -1。</returns>
        Public Function RunToConvergence(Optional tolerance As Double = 1e-6,
                                         Optional maxSteps As Integer = 1000) As Integer
            If tolerance <= 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(tolerance), tolerance, "tolerance must be positive")
            End If
            If maxSteps <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(maxSteps), maxSteps, "max steps must be positive")
            End If

            For stepIndex As Integer = 1 To maxSteps
                Dim previous As Double() = Activation

                Call Propagate()

                If MaxDifference(previous, _activation) < tolerance Then
                    Return stepIndex
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' 检测传播轨迹中的极限环（含不动点）。
        ''' </summary>
        Public Function DetectCycle(Optional maxSteps As Integer = 1000,
                                    Optional tolerance As Double = 1e-6) As BN.AttractorResult
            Dim seen As New Dictionary(Of String, Integer)()
            Dim trajectory As New List(Of Double())

            Call trajectory.Add(Activation)

            For stepIndex As Integer = 0 To maxSteps
                Dim key As String = ActivationKey(_activation)

                If stepIndex > 0 AndAlso seen.ContainsKey(key) Then
                    Dim first As Integer = seen(key)
                    Dim period As Integer = stepIndex - first
                    Dim cycle(period - 1)() As Boolean

                    For j As Integer = 0 To period - 1
                        Dim state As Double() = trajectory(first + j)
                        Dim bits(state.Length - 1) As Boolean

                        For k As Integer = 0 To state.Length - 1
                            bits(k) = state(k) >= (_function.LowerBound + _function.UpperBound) / 2.0
                        Next

                        cycle(j) = bits
                    Next

                    Return New BN.AttractorResult(first, period, cycle)
                End If

                seen(key) = stepIndex

                Call Propagate()
                Call trajectory.Add(Activation)
            Next

            Return Nothing
        End Function

        Private Sub RefreshIncoming(target As Integer)
            Call _concepts(target).ClearIncoming()

            For source As Integer = 0 To _concepts.Length - 1
                If Not _includeSelfLoop AndAlso source = target Then
                    Continue For
                End If
                If _weights(source, target) <> 0.0 Then
                    Call _concepts(target).SetIncoming(source, _weights(source, target))
                End If
            Next
        End Sub

        Private Function ClampToFunction(value As Double) As Double
            If value < _function.LowerBound Then
                Return _function.LowerBound
            ElseIf value > _function.UpperBound Then
                Return _function.UpperBound
            Else
                Return value
            End If
        End Function

        Private Sub ValidateIndex(index As Integer, name As String)
            If index < 0 OrElse index >= _concepts.Length Then
                Throw New ArgumentOutOfRangeException(name, index, "index outside concept range")
            End If
        End Sub

        Private Shared Function MaxDifference(a As Double(), b As Double()) As Double
            Dim max As Double = 0.0

            For i As Integer = 0 To a.Length - 1
                max = std.Max(max, std.Abs(a(i) - b(i)))
            Next

            Return max
        End Function

        Private Shared Function ActivationKey(state As Double()) As String
            Dim parts(state.Length - 1) As String

            For i As Integer = 0 To state.Length - 1
                parts(i) = state(i).ToString("G6", Globalization.CultureInfo.InvariantCulture)
            Next

            Return String.Join("|", parts)
        End Function

        Private Shared Function SumBuffer(length As Integer) As Double()
            If _sumBuffer Is Nothing OrElse _sumBuffer.Length <> length Then
                _sumBuffer = New Double(length - 1) {}
            End If

            Return _sumBuffer
        End Function

        Private Shared Function NextBuffer(length As Integer) As Double()
            If _nextBuffer Is Nothing OrElse _nextBuffer.Length <> length Then
                _nextBuffer = New Double(length - 1) {}
            End If

            Return _nextBuffer
        End Function
    End Class

End Namespace
