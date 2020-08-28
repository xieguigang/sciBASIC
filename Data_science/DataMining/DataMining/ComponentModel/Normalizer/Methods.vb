
Namespace ComponentModel.Normalizer

    Public Enum Methods
        ''' <summary>
        ''' 归一化到[0, 1]区间内
        ''' </summary>
        NormalScaler
        ''' <summary>
        ''' 直接 x / max 进行归一化, 当出现极值的时候, 此方法无效, 根据数据分布,可能会归一化到[0, 1] 或者 [-1, 1]区间内
        ''' </summary>
        RelativeScaler
        ''' <summary>
        ''' 通过对数据进行区间离散化来完成归一化
        ''' </summary>
        RangeDiscretizer
    End Enum
End Namespace