Option Strict On
Option Explicit On

Namespace BooleanNetworks

    ''' <summary>布尔规则类型，用于区分内置门电路与一般真值表。</summary>
    Public Enum BooleanRuleKind
        ''' <summary>任意真值表。</summary>
        TruthTable = 0
        ''' <summary>与门：全部输入为真时输出真。</summary>
        [And] = 1
        ''' <summary>或门：任一输入为真时输出真。</summary>
        [Or] = 2
        ''' <summary>非门：单输入取反。</summary>
        [Not] = 3
        ''' <summary>异或门：输入中真值为奇数个时输出真。</summary>
        [Xor] = 4
    End Enum

    ''' <summary>布尔规则求值接口，是 BN/PBN 与联合建模桥接层之间的扩展点。</summary>
    ''' <remarks>
    ''' 规则只负责「输入布尔向量 → 布尔输出」，不关心节点名称、父节点索引或连续值二值化。
    ''' </remarks>
    Public Interface IBooleanRule
        ''' <summary>规则接受的输入个数。</summary>
        ReadOnly Property InputCount As Integer
        ''' <summary>规则类型。</summary>
        ReadOnly Property Kind As BooleanRuleKind
        ''' <summary>对给定输入求值。</summary>
        Function Evaluate(inputs As Boolean()) As Boolean
        ''' <summary>导出长度为 <c>2^InputCount</c> 的真值表（索引即输入位模式）。</summary>
        Function ToTruthTable() As Boolean()
    End Interface

    ''' <summary>连续表达值到布尔状态的转换器。</summary>
    Public Interface IStateBinarizer
        ''' <summary>把单个连续值转为布尔状态。</summary>
        Function Binarize(value As Double) As Boolean
        ''' <summary>把一行连续表达值转为布尔状态向量。</summary>
        Function BinarizeRow(values As Double()) As Boolean()
    End Interface

End Namespace
