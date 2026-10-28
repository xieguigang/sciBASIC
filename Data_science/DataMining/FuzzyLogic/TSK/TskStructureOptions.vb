Option Strict On
Option Explicit On

Imports System

Namespace TSK

    ''' <summary>
    ''' TSK 网络的**结构**配置：输入/输出的命名、模糊划分方式与规模保护。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这里只放"决定网络拓扑"的参数。学习率、动量、优化器、早停、岭项等训练参数
    ''' 属于训练层，留在 ANFIS 工程的 <c>AnfisOptions</c>（它继承本类型）。
    ''' 这样任何工程只要引用 FuzzyLogic 就能构建与运行网络，而不需要引入训练栈。
    ''' </para>
    ''' <para>
    ''' VB 不支持协变返回类型，因此链式校验方法命名为 <see cref="ValidateStructure"/>，
    ''' 派生类型可以另外提供返回自身的 <c>Validate()</c>，不会产生重载冲突。
    ''' </para>
    ''' </remarks>
    Public Class TskStructureOptions

        ''' <summary>特征列名称；为空时自动生成 <c>x1..xn</c>。</summary>
        Public Property FeatureNames As String() = Nothing
        ''' <summary>输出名称；为空时自动生成 <c>y1..yK</c>。</summary>
        Public Property OutputNames As String() = Nothing

        ''' <summary>隶属函数类型（默认高斯）。</summary>
        Public Property MfKind As MembershipKind = MembershipKind.Gaussian
        ''' <summary>每个输入的隶属函数个数（默认 3）。</summary>
        Public Property MfCount As Integer = 3
        ''' <summary>
        ''' 逐输入的隶属函数个数；给定时覆盖 <see cref="MfCount"/>（长度必须等于输入维数）。
        ''' </summary>
        Public Property MfCounts As Integer() = Nothing
        ''' <summary>
        ''' 规则启用掩膜（长度 = <c>prod(m_i)</c>，编号顺序即全网格枚举顺序）。
        ''' 给定时只保留为 <c>True</c> 的规则，可用于注入专家规则或跳过不可能的规则组合。
        ''' </summary>
        Public Property RuleEnableMask As Boolean() = Nothing

        ''' <summary>分块处理的每块行数（控制临时内存与缓存命中率）。</summary>
        Public Property ChunkRows As Integer = 512

        ''' <summary>规则数上限，超过直接报错（默认 1024，对应 n=6、m=4 的量级）。</summary>
        Public Property MaxRules As Integer = 1024
        ''' <summary>结论参数总数上限 <c>R*(n+1)</c>，超过直接报错（默认 4096）。</summary>
        ''' <remarks>
        ''' 主要约束来自 <c>A'A</c> 的 <c>P^2</c> 内存：P=4096 时约 134 MB，
        ''' 再往上就应该改用递推最小二乘或先做规则剪枝。
        ''' </remarks>
        Public Property MaxConsequentParams As Integer = 4096

        ''' <summary>逐属性校验结构配置并返回自身。</summary>
        Public Function ValidateStructure() As TskStructureOptions
            If MfCount <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(MfCount), MfCount, "the membership function count must be positive")
            End If
            If MfCounts IsNot Nothing Then
                For i As Integer = 0 To MfCounts.Length - 1
                    If MfCounts(i) <= 0 Then
                        Throw New ArgumentOutOfRangeException(NameOf(MfCounts), $"input [{i}] has no membership function")
                    End If
                Next
            End If
            If ChunkRows <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(ChunkRows), ChunkRows, "the chunk size must be positive")
            End If
            If MaxRules <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(MaxRules), MaxRules, "the rule limit must be positive")
            End If
            If MaxConsequentParams <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(MaxConsequentParams), MaxConsequentParams, "the consequent parameter limit must be positive")
            End If

            Return Me
        End Function

        ''' <summary>浅拷贝（基类的数组字段单独复制）。</summary>
        Public Function CloneStructure() As TskStructureOptions
            Dim copy As TskStructureOptions = DirectCast(Me.MemberwiseClone(), TskStructureOptions)

            Call CopyStructureArraysTo(copy)

            Return copy
        End Function

        ''' <summary>
        ''' 把结构相关的数组字段深拷贝到目标实例（派生类型在 <c>Clone</c> 里复用）。
        ''' </summary>
        ''' <remarks>
        ''' <c>MemberwiseClone</c> 只做浅拷贝，数组会与源对象共享；派生类型的克隆必须调用本方法，
        ''' 否则对副本 <c>MfCounts</c> 的修改会串回原对象。
        ''' </remarks>
        Protected Sub CopyStructureArraysTo(target As TskStructureOptions)
            If target Is Nothing Then
                Throw New ArgumentNullException(NameOf(target))
            End If

            target.FeatureNames = CType(CloneArray(FeatureNames), String())
            target.OutputNames = CType(CloneArray(OutputNames), String())
            target.MfCounts = CType(CloneArray(MfCounts), Integer())
            target.RuleEnableMask = CType(CloneArray(RuleEnableMask), Boolean())
        End Sub

        ''' <summary>数组的安全克隆（<c>Nothing</c> 原样返回）。</summary>
        Protected Shared Function CloneArray(values As Array) As Array
            If values Is Nothing Then
                Return Nothing
            Else
                Return CType(values.Clone(), Array)
            End If
        End Function

    End Class

End Namespace
