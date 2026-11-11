Namespace Ensemble

    ''' <summary>Voting 集成器的聚合方式</summary>
    Public Enum VotingMode

        ''' <summary>硬投票：每个基模型先取 argmax，再按「一模型一票」（或加权）多数决</summary>
        Hard = 0

        ''' <summary>软投票：对各基模型的概率向量做加权平均后取 argmax，要求基模型都能给出概率</summary>
        Soft = 1

    End Enum

    ''' <summary>Bagging 回归器的聚合方式</summary>
    Public Enum BaggingAggregation

        ''' <summary>均值（对异常值敏感，但方差最小）</summary>
        Mean = 0

        ''' <summary>中位数（对异常值稳健，适合长尾分布的基模型误差）</summary>
        Median = 1

    End Enum

End Namespace
