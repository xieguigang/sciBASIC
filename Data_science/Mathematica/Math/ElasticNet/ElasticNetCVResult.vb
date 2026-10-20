''' <summary>
''' 弹性网络的交叉验证结果：在 ``l1Ratio × alpha`` 的二维网格上面
''' 逐一计算折外均方误差，并以误差最小的组合作为最优超参数。
''' 
''' 由于不同的 ``l1Ratio`` 具有不同的 alpha 上界（最大 alpha 与 L1 惩罚的
''' 强度直接相关），因此**每一个 `l1Ratio` 都有自己的一条 alpha 网格**，
''' <see cref="Alphas"/> 与 <see cref="MSE"/> 都是锯齿数组：
''' 第一个下标是 `l1Ratio`、第二个下标是该 `l1Ratio` 之下的 alpha 步骤。
''' </summary>
Public Class ElasticNetCVResult

    ''' <summary>
    ''' 交叉验证所遍历的 L1 混合比例
    ''' </summary>
    ''' <returns></returns>
    Public Property L1Ratios As Double()

    ''' <summary>
    ''' 每一个 l1Ratio 所对应的 alpha 网格（由大到小）
    ''' </summary>
    ''' <returns></returns>
    Public Property Alphas As Double()()

    ''' <summary>
    ''' 折外均方误差矩阵：行对应 <see cref="L1Ratios"/>、列对应该比例的
    ''' alpha 步骤，数值越小越好
    ''' </summary>
    ''' <returns></returns>
    Public Property MSE As Double()()

    ''' <summary>
    ''' 交叉验证所使用的折数
    ''' </summary>
    ''' <returns></returns>
    Public Property Folds As Integer

    ''' <summary>
    ''' 最优的 l1Ratio 在 <see cref="L1Ratios"/> 之中的下标
    ''' </summary>
    ''' <returns></returns>
    Public Property BestL1RatioIndex As Integer

    ''' <summary>
    ''' 最优的 alpha 在其所属网格之中的下标
    ''' </summary>
    ''' <returns></returns>
    Public Property BestAlphaIndex As Integer

    ''' <summary>
    ''' 用最优超参数在全量数据上面重新拟合得到的最终模型
    ''' </summary>
    ''' <returns></returns>
    Public Property Fit As ElasticNetFit

    ''' <summary>
    ''' 最优的 l1Ratio
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BestL1Ratio As Double
        Get
            If L1Ratios Is Nothing OrElse L1Ratios.Length = 0 Then
                Return 0.0
            End If

            Return L1Ratios(BestL1RatioIndex)
        End Get
    End Property

    ''' <summary>
    ''' 最优的 alpha
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BestAlpha As Double
        Get
            If Alphas Is Nothing OrElse Alphas.Length = 0 Then
                Return 0.0
            End If

            Dim grid As Double() = Alphas(BestL1RatioIndex)

            If grid Is Nothing OrElse grid.Length = 0 Then
                Return 0.0
            End If

            Return grid(BestAlphaIndex)
        End Get
    End Property

    ''' <summary>
    ''' 最优组合的折外均方误差
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BestMSE As Double
        Get
            If MSE Is Nothing OrElse MSE.Length = 0 Then
                Return Double.NaN
            End If

            Dim grid As Double() = MSE(BestL1RatioIndex)

            If grid Is Nothing OrElse grid.Length = 0 Then
                Return Double.NaN
            End If

            Return grid(BestAlphaIndex)
        End Get
    End Property

    ''' <summary>
    ''' 把交叉验证的误差曲面导出为长表：每一行是一个
    ''' ``(l1Ratio, alpha)`` 组合，特征列是这两个超参数、标签列是折外均方误差
    ''' </summary>
    ''' <returns></returns>
    Public Function toNumericTable() As NumericTable
        Dim rows As New List(Of Double())
        ' 注意：局部变量不能叫 mse —— VB 大小写不敏感，它会与属性 MSE 撞名
        Dim errors As New List(Of Double)

        If L1Ratios IsNot Nothing AndAlso MSE IsNot Nothing Then
            For i As Integer = 0 To L1Ratios.Length - 1
                Dim grid As Double() = MSE(i)
                Dim ag As Double() = If(Alphas IsNot Nothing AndAlso i < Alphas.Length, Alphas(i), DirectCast(Nothing, Double()))

                If grid Is Nothing Then
                    Continue For
                End If

                For k As Integer = 0 To grid.Length - 1
                    rows.Add(New Double() {L1Ratios(i), If(ag IsNot Nothing AndAlso k < ag.Length, ag(k), 0.0)})
                    errors.Add(grid(k))
                Next
            Next
        End If

        Dim matrix As Double()() = rows.ToArray
        Dim rowNames As String() = Enumerable.Range(1, matrix.Length).Select(Function(i) $"cv_{i}").ToArray
        Dim table As New NumericTable(matrix, rowNames, New String() {"l1_ratio", "alpha"})

        Call table.SetLabel("mse", errors.ToArray)

        Return table
    End Function

    Public Overrides Function ToString() As String
        Return $"elastic net CV: {Folds}-fold, {L1Ratios?.Length} l1_ratio(s), best l1_ratio = {BestL1Ratio:G4}, best alpha = {BestAlpha:G4}, cv mse = {BestMSE:G4}"
    End Function
End Class
