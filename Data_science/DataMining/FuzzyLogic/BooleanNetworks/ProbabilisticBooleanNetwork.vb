Option Strict On
Option Explicit On

Imports System

Namespace BooleanNetworks

    ''' <summary>
    ''' 概率布尔网络（PBN）：每个基因可持有多条布尔规则及选择概率，
    ''' 每次更新按概率采样一条规则。
    ''' </summary>
    Public NotInheritable Class ProbabilisticBooleanNetwork
        Inherits BooleanNetworkBase

        ''' <summary>以节点、初始状态和随机种子构造 PBN。</summary>
        Public Sub New(nodes As BooleanNode(), initialState As Boolean(), Optional seed As Integer = 12345)
            MyBase.New(nodes, initialState, seed)
        End Sub

        ''' <summary>按概率采样规则并求值。</summary>
        Protected Overrides Function EvaluateNode(node As BooleanNode, inputs As Boolean()) As Boolean
            If node.IsDeterministic Then
                Return node.Evaluate(inputs)
            End If

            Call IncrementSampleCount()

            Return node.Sample(inputs, SharedRandom)
        End Function

        ''' <summary>构造一条概率规则。</summary>
        Public Shared Function CreateRule(rule As IBooleanRule, probability As Double) As ProbabilityRule
            Return New ProbabilityRule(rule, probability)
        End Function

        ''' <summary>由概率规则列表构造节点。</summary>
        Public Shared Function CreateNode(name As String, parents As Integer(), ParamArray rules As ProbabilityRule()) As BooleanNode
            If rules Is Nothing OrElse rules.Length = 0 Then
                Throw New ArgumentException("at least one probability rule is required", NameOf(rules))
            End If

            Dim functions(rules.Length - 1) As IBooleanRule
            Dim probabilities(rules.Length - 1) As Double

            For i As Integer = 0 To rules.Length - 1
                functions(i) = rules(i).Rule
                probabilities(i) = rules(i).Probability
            Next

            Return New BooleanNode(name, parents, functions, probabilities)
        End Function
    End Class

    ''' <summary>一条带选择概率的布尔规则。</summary>
    Public NotInheritable Class ProbabilityRule

        Private ReadOnly _rule As IBooleanRule
        Private ReadOnly _probability As Double

        ''' <summary>以规则和概率构造条目。</summary>
        Public Sub New(rule As IBooleanRule, probability As Double)
            If rule Is Nothing Then
                Throw New ArgumentNullException(NameOf(rule))
            End If
            If probability < 0.0 OrElse probability > 1.0 OrElse Double.IsNaN(probability) Then
                Throw New ArgumentOutOfRangeException(NameOf(probability), probability, "probability must be in [0,1]")
            End If

            _rule = rule
            _probability = probability
        End Sub

        ''' <summary>布尔规则。</summary>
        Public ReadOnly Property Rule As IBooleanRule
            Get
                Return _rule
            End Get
        End Property

        ''' <summary>选择概率。</summary>
        Public ReadOnly Property Probability As Double
            Get
                Return _probability
            End Get
        End Property
    End Class

End Namespace
