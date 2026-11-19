Option Strict On
Option Explicit On

Imports System
Imports std = System.Math

Namespace BooleanNetworks

    ''' <summary>
    ''' 布尔网络中的基因节点。
    ''' </summary>
    ''' <remarks>
    ''' 节点同时持有规则集合与选择概率：BN 只有一个概率为 1 的规则，
    ''' PBN 则持有多条规则及对应概率，每步按概率采样一条。
    ''' </remarks>
    Public NotInheritable Class BooleanNode

        Private ReadOnly _name As String
        Private ReadOnly _parents As Integer()
        Private ReadOnly _rules As IBooleanRule()
        Private ReadOnly _probabilities As Double()
        Private ReadOnly _cumulative As Double

        ''' <summary>创建确定性节点（BN 语义）。</summary>
        Public Sub New(name As String, parents As Integer(), rule As IBooleanRule)
            Call Me.New(name, parents, New IBooleanRule() {rule}, New Double() {1.0})
        End Sub

        ''' <summary>创建概率节点（PBN 语义）。</summary>
        Public Sub New(name As String, parents As Integer(), rules As IBooleanRule(), probabilities As Double())
            If String.IsNullOrWhiteSpace(name) Then
                Throw New ArgumentException("node name cannot be empty", NameOf(name))
            End If
            If parents Is Nothing Then
                Throw New ArgumentNullException(NameOf(parents))
            End If
            If rules Is Nothing OrElse rules.Length = 0 Then
                Throw New ArgumentException("at least one rule is required", NameOf(rules))
            End If
            If probabilities Is Nothing OrElse probabilities.Length <> rules.Length Then
                Throw New ArgumentException("probability count must match rule count", NameOf(probabilities))
            End If
            If parents.Length > BooleanTruthTable.MaxBitArrayParents Then
                Throw New ArgumentOutOfRangeException(NameOf(parents), parents.Length, "too many parents")
            End If

            Dim sum As Double = 0.0

            For i As Integer = 0 To rules.Length - 1
                If rules(i) Is Nothing Then
                    Throw New ArgumentException($"rule [{i}] is empty", NameOf(rules))
                End If
                If rules(i).InputCount <> parents.Length Then
                    Throw New ArgumentException($"rule [{i}] expects {rules(i).InputCount} inputs but node has {parents.Length} parents", NameOf(rules))
                End If
                If probabilities(i) < 0.0 OrElse Double.IsNaN(probabilities(i)) Then
                    Throw New ArgumentOutOfRangeException(NameOf(probabilities), probabilities(i), "probabilities must be non-negative")
                End If

                sum += probabilities(i)
            Next

            If std.Abs(sum - 1.0) > 1e-9 Then
                Throw New ArgumentException($"probabilities must sum to 1, got {sum}", NameOf(probabilities))
            End If

            _name = name
            _parents = DirectCast(parents.Clone(), Integer())
            _rules = DirectCast(rules.Clone(), IBooleanRule())
            _probabilities = DirectCast(probabilities.Clone(), Double())
            _cumulative = sum
        End Sub

        ''' <summary>节点名称（通常为基因名）。</summary>
        Public ReadOnly Property Name As String
            Get
                Return _name
            End Get
        End Property

        ''' <summary>父节点索引数组（防御性副本）。</summary>
        Public ReadOnly Property Parents As Integer()
            Get
                Return DirectCast(_parents.Clone(), Integer())
            End Get
        End Property

        ''' <summary>父节点个数。</summary>
        Public ReadOnly Property ParentCount As Integer
            Get
                Return _parents.Length
            End Get
        End Property

        ''' <summary>候选规则集合。</summary>
        Public ReadOnly Property Rules As IBooleanRule()
            Get
                Return DirectCast(_rules.Clone(), IBooleanRule())
            End Get
        End Property

        ''' <summary>候选规则的选择概率。</summary>
        Public ReadOnly Property Probabilities As Double()
            Get
                Return DirectCast(_probabilities.Clone(), Double())
            End Get
        End Property

        ''' <summary>是否只有一条确定性规则。</summary>
        Public ReadOnly Property IsDeterministic As Boolean
            Get
                Return _rules.Length = 1
            End Get
        End Property

        ''' <summary>用确定性规则求值（BN 语义；PBN 节点取第一条规则）。</summary>
        Public Function Evaluate(inputs As Boolean()) As Boolean
            Return _rules(0).Evaluate(inputs)
        End Function

        ''' <summary>按概率采样一条规则并求值（PBN 语义）。</summary>
        Public Function Sample(inputs As Boolean(), random As Random) As Boolean
            If random Is Nothing Then
                Throw New ArgumentNullException(NameOf(random))
            End If

            Dim ruleIndex As Integer = 0

            If _rules.Length > 1 Then
                Dim draw As Double = random.NextDouble() * _cumulative
                Dim acc As Double = 0.0

                For i As Integer = 0 To _probabilities.Length - 1
                    acc += _probabilities(i)

                    If draw < acc Then
                        ruleIndex = i
                        Exit For
                    End If
                Next
            End If

            Return _rules(ruleIndex).Evaluate(inputs)
        End Function

        ''' <summary>文本表示。</summary>
        Public Overrides Function ToString() As String
            Return $"{_name} <- [{String.Join(", ", _parents)}] ({_rules.Length} rule(s))"
        End Function
    End Class

End Namespace
