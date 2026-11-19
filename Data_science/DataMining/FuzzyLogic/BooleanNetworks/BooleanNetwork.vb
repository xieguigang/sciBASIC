Option Strict On
Option Explicit On

Imports System

Namespace BooleanNetworks

    ''' <summary>
    ''' 确定性布尔网络（BN）：每个基因持有唯一布尔规则。
    ''' </summary>
    Public NotInheritable Class BooleanNetwork
        Inherits BooleanNetworkBase

        ''' <summary>以节点和初始状态构造 BN。</summary>
        Public Sub New(nodes As BooleanNode(), initialState As Boolean())
            MyBase.New(nodes, initialState)
        End Sub

        ''' <summary>确定性求值，不做概率采样。</summary>
        Protected Overrides Function EvaluateNode(node As BooleanNode, inputs As Boolean()) As Boolean
            Return node.Evaluate(inputs)
        End Function

        ''' <summary>由「父节点索引 + 显式真值表 + 基因名」批量构造节点。</summary>
        Public Shared Function CreateNode(name As String, parents As Integer(), values As Boolean()) As BooleanNode
            Return New BooleanNode(name, parents, New BooleanTruthTable(parents.Length, values))
        End Function
    End Class

End Namespace
