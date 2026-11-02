Imports std = System.Math
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 贝叶斯网络推断器的抽象基类
    ''' </summary>
    Public MustInherit Class BNInfer

        ''' <summary>
        ''' 被推断的贝叶斯网络
        ''' </summary>
        Public BeliefNetwork As BeliefNetwork

        Protected Sub New(net As BeliefNetwork)
            If net Is Nothing Then
                Throw New ArgumentNullException(NameOf(net))
            End If

            BeliefNetwork = net
        End Sub

        ''' <summary>
        ''' 计算条件概率 <c>P(x | conditions)</c>
        ''' </summary>
        ''' <param name="x">
        ''' 查询变量的取值数组，长度等于节点数；不属于查询集合的位置请填 -1
        ''' </param>
        ''' <param name="conditions">
        ''' 条件变量的取值数组，长度等于节点数；不属于条件集合的位置请填 -1。
        ''' 两个数组之中「有值」的位置必须互不相交。
        ''' </param>
        Public MustOverride Function GetBelief(x As Integer(), conditions As Integer()) As Double

        Public Overrides Function ToString() As String
            Return BeliefNetwork.ToString
        End Function
    End Class

    ''' <summary>
    ''' 基于变量消除的精确推断器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 类名保留 <c>BElim</c> 是为了与旧版本以及历史调用代码保持兼容；内部实现已经由
    ''' 「手写 bucket 递归」替换为 <see cref="VariableElimination"/>，后者支持任意多值节点、
    ''' 正确的证据合并语义以及 min-fill 消除序。
    ''' </para>
    ''' <para>
    ''' <b>旧版本的两个致命缺陷</b>在这里被修正：(1) 求和循环被硬编码为 <c>For e = 0 To 1</c>，
    ''' 节点取值超过 2 个时结果完全错误；(2) <c>GetBelief</c> 先用条件写入证据、再用查询数组
    ''' 整体覆盖，而查询数组里「错开」的位置是 -1，于是条件证据被清空，结果没有意义。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class BElim : Inherits BNInfer

        Private ReadOnly _engine As VariableElimination

        ''' <summary>
        ''' 为指定的网络构造推断器
        ''' </summary>
        Public Sub New(net As BeliefNetwork)
            Call MyBase.New(net)
            _engine = New VariableElimination(net)
        End Sub

        ''' <summary>
        ''' 内部的变量消除引擎
        ''' </summary>
        Public ReadOnly Property Engine As VariableElimination
            Get
                Return _engine
            End Get
        End Property

        ''' <summary>
        ''' 计算条件概率 <c>P(x | conditions)</c>
        ''' </summary>
        ''' <param name="x">查询变量的取值(其余位置填 -1)</param>
        ''' <param name="conditions">条件变量的取值(其余位置填 -1)</param>
        ''' <returns>条件概率；当证据与查询相互矛盾的时候返回 0</returns>
        ''' <remarks>
        ''' 计算方式为 <c>P(x ∪ conditions) / P(conditions)</c>，两者都通过对数空间的
        ''' 配分函数(<see cref="VariableElimination.LogPartition"/>)相减得到，这样可以避免
        ''' 「先整体覆盖证据再求边缘」这一类语义陷阱。
        ''' </remarks>
        Public Overrides Function GetBelief(x As Integer(), conditions As Integer()) As Double
            Dim count As Integer = BeliefNetwork.Count
            Dim combined As Integer() = New Integer(count - 1) {}

            For i As Integer = 0 To count - 1
                combined(i) = BeliefNode.NotObserved
            Next

            If conditions IsNot Nothing Then
                For i As Integer = 0 To std.Min(conditions.Length, count) - 1
                    If conditions(i) <> BeliefNode.NotObserved Then
                        combined(i) = conditions(i)
                    End If
                Next
            End If

            Dim hasQuery As Boolean = False

            If x IsNot Nothing Then
                For i As Integer = 0 To std.Min(x.Length, count) - 1
                    If x(i) = BeliefNode.NotObserved Then
                        Continue For
                    End If

                    DebugAudit.Check(combined(i) = BeliefNode.NotObserved OrElse combined(i) = x(i),
                                     $"the node '{BeliefNetwork.Nodes(i).Name}' is assigned to both {combined(i)} and {x(i)}")

                    combined(i) = x(i)
                    hasQuery = True
                Next
            End If

            If Not hasQuery Then
                ' 没有查询变量：返回证据的归一化常数，按照定义就是 1
                Return 1.0
            End If

            Dim logJoint As Double = _engine.LogPartition(combined)
            Dim logCondition As Double

            If conditions Is Nothing Then
                logCondition = 0.0
            Else
                logCondition = _engine.LogPartition(conditions)
            End If

            If Double.IsNegativeInfinity(logJoint) Then
                Return 0.0
            End If
            If Double.IsNegativeInfinity(logCondition) Then
                DebugAudit.Check(False, "the conditional evidence has zero probability")
                Return 0.0
            End If

            Return std.Exp(logJoint - logCondition)
        End Function

        ''' <summary>
        ''' 计算单个变量的边缘后验分布
        ''' </summary>
        Public Function Marginal(conditions As Integer(), target As Integer) As Double()
            Return _engine.Marginal(conditions, target)
        End Function

        ''' <summary>
        ''' 计算一组变量的联合后验分布
        ''' </summary>
        Public Function Query(conditions As Integer(), targets As Integer()) As Double()
            Return _engine.Query(conditions, targets)
        End Function

        ''' <summary>
        ''' 计算 MAP 赋值
        ''' </summary>
        Public Function MostProbable(conditions As Integer(), targets As Integer()) As Integer()
            Return _engine.MapAssignment(conditions, targets)
        End Function
    End Class
End Namespace
