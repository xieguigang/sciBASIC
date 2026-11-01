Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train.kernels

Namespace train

    ''' <summary>
    ''' 训练期的特征列表。
    '''
    ''' 自本次重构起，实际的训练数据由 <see cref="Columns"/>（<see cref="ColumnBlock"/>，
    ''' 列主序 SoA：每特征「值数组 + 行号数组 + 区间边界」）承载。
    '''
    ''' 旧的 <c>Single()()()</c>（每 (样本,特征) 一个两元素 <c>Single()</c> 对象，约 32~40 字节）
    ''' 与逐区间物化的 <c>cutting_inds</c>（每区间一个 <c>Integer()</c> 对象）
    ''' 已被移除：10 万 × 100 的数据集上，仅此两项就分别占用约 400MB 与 40MB + 10 万个小数组。
    ''' </summary>
    Public Class AttributeList

        Public feature_dim As Integer
        Public missing_value_attribute_list As Integer()()
        ''' <summary>
        ''' 【已废弃】旧逐区间实现物化的区间行号。
        ''' 新内核使用 <see cref="Columns"/> 的区间视图，本字段保留为兼容占位（恒为空）。
        ''' </summary>
        Public cutting_inds As Integer()()()
        ''' <summary>
        ''' 【已废弃】旧逐区间实现物化的切分阈值，同上。
        ''' </summary>
        Public cutting_thresholds As Single()()
        Public origin_feature As Single()()
        Public cat_features_cols As List(Of Integer)

        ''' <summary>
        ''' 列块（列主序 SoA），跨树共享且只读
        ''' </summary>
        Public ReadOnly Property Columns As ColumnBlock

        Public Sub New(data As TrainData)
            missing_value_attribute_list = data.missing_index
            feature_dim = data.feature_dim
            origin_feature = data.origin_feature
            cat_features_cols = If(data.cat_features_cols, New List(Of Integer)())

            Columns = ColumnBlock.FromTrainData(data)

            cutting_inds = New Integer(feature_dim - 1)()() {}
            cutting_thresholds = New Single(feature_dim - 1)() {}

            ' 列块建好之后，解析阶段为「按值排序」而构造的
            ' Single()()() 已经没有任何消费者，立即释放以降低常驻内存。
            data.ReleaseRawColumns()
        End Sub
    End Class
End Namespace
