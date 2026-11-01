Namespace train.kernels

    ''' <summary>
    ''' 一个分裂候选。
    '''
    ''' 旧实现把「最佳特征 / 最佳增益 / 最佳阈值 / 缺失方向 / 类别左子集」塞进
    ''' 一个 <c>List(Of Double)</c>，调用方靠 <c>ret.Count</c> 猜测返回值是数值阈值还是类别左子集，
    ''' 并且在没有任何列被更新时返回 <c>best_feature = 0</c> 的无效候选。
    ''' 本类型把候选显式化，<see cref="HasSplit"/> 为 <c>False</c> 时表示该节点无法继续分裂。
    ''' </summary>
    Public Class SplitCandidate

        ''' <summary>
        ''' 缺失值进入独立的第三个子节点
        ''' </summary>
        Public Const NanDirectionChild As Integer = 0
        ''' <summary>
        ''' 缺失值进入左子节点
        ''' </summary>
        Public Const NanDirectionLeft As Integer = 1
        ''' <summary>
        ''' 缺失值进入右子节点
        ''' </summary>
        Public Const NanDirectionRight As Integer = 2
        ''' <summary>
        ''' 训练集在该特征上不存在缺失值，预测期按样本数启发式选择方向
        ''' </summary>
        Public Const NanDirectionNone As Integer = 3

        ''' <summary>
        ''' 表示「没有可用的分裂」的共享实例
        ''' </summary>
        Public Shared ReadOnly NoSplit As New SplitCandidate()

        ''' <summary>
        ''' 分裂特征，<c>-1</c> 表示无效
        ''' </summary>
        Public Property Feature As Integer = -1
        ''' <summary>
        ''' 分裂增益（已扣除 <c>gamma</c>）
        ''' </summary>
        Public Property Gain As Double = Double.NegativeInfinity
        ''' <summary>
        ''' 数值特征的分裂阈值（<c>value &lt;= threshold</c> 进入左子节点）
        ''' </summary>
        Public Property Threshold As Single
        ''' <summary>
        ''' 类别特征进入左子节点的整数编码集合；数值特征为 <c>Nothing</c>
        ''' </summary>
        Public Property LeftCategories As Integer()
        ''' <summary>
        ''' 缺失值方向，取值为本类的 <c>NanDirection*</c> 常量
        ''' </summary>
        Public Property NanDirection As Integer = NanDirectionNone

        ''' <summary>
        ''' 是否存在有效分裂
        ''' </summary>
        Public ReadOnly Property HasSplit As Boolean
            Get
                Return Feature >= 0 AndAlso Gain > 0
            End Get
        End Property

        ''' <summary>
        ''' 是否为类别特征分裂
        ''' </summary>
        Public ReadOnly Property IsCategorical As Boolean
            Get
                Return LeftCategories IsNot Nothing
            End Get
        End Property

        Public Overrides Function ToString() As String
            If Not HasSplit Then
                Return "<no-split>"
            End If

            If IsCategorical Then
                Return $"feature={Feature}, cat, gain={Gain:F6}"
            Else
                Return $"feature={Feature}, <= {Threshold}, gain={Gain:F6}"
            End If
        End Function
    End Class
End Namespace
