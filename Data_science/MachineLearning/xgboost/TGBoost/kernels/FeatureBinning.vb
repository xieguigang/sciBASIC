Imports std = System.Math

Namespace train.kernels

    ''' <summary>
    ''' 单个特征的分桶表（等频分桶）。
    '''
    ''' <see cref="Bins"/> 与列块中「按值升序排列的位置」一一对应，
    ''' <see cref="Thresholds"/> 给出每个桶内的最大特征值，从而在直方图上
    ''' 用 <c>value &lt;= Thresholds(bin)</c> 精确还原「左侧 = 前 b+1 个桶」的划分。
    ''' </summary>
    Public Class FeatureBinning

        ''' <summary>
        ''' 有序位置 -&gt; 桶号（非递减）
        ''' </summary>
        Public ReadOnly Bins As Integer()
        ''' <summary>
        ''' 桶号的窄整型视图（桶数 &lt; 65536 时非空）。
        '''
        ''' <c>hist</c> 的内层循环只读桶号，用 <c>UShort</c> 可把该列的访存带宽减半
        ''' （训练时这一项是每层每样本每特征一次读）。
        ''' </summary>
        Public ReadOnly Bins16 As UShort()
        ''' <summary>
        ''' 桶号 -&gt; 该桶内最大特征值
        ''' </summary>
        Public ReadOnly Thresholds As Single()
        ''' <summary>
        ''' 实际桶数
        ''' </summary>
        Public ReadOnly BucketCount As Integer

        Private Sub New(bins As Integer(), thresholds As Single(), bucketCount As Integer)
            Me.Bins = bins
            Me.Thresholds = thresholds
            Me.BucketCount = bucketCount

            If bucketCount > 0 AndAlso bucketCount <= UShort.MaxValue AndAlso bins IsNot Nothing Then
                Dim narrow As UShort() = New UShort(bins.Length - 1) {}

                For i As Integer = 0 To bins.Length - 1
                    narrow(i) = CUShort(bins(i))
                Next

                Me.Bins16 = narrow
            End If
        End Sub

        ''' <summary>
        ''' 构建分桶表：等频切分有序值序列，并保证相同取值落在同一桶
        ''' </summary>
        ''' <param name="keys">按值升序排列的特征值</param>
        ''' <param name="maxBin">最大桶数</param>
        ''' <returns></returns>
        Public Shared Function Build(keys As Single(), maxBin As Integer) As FeatureBinning
            Dim n As Integer = If(keys Is Nothing, 0, keys.Length)
            Dim bins As Integer() = New Integer(n - 1) {}

            If n = 0 Then
                Return New FeatureBinning(bins, New Single() {}, 0)
            End If

            Dim binCount As Integer = std.Min(std.Max(maxBin, 1), n)
            Dim stepSize As Double = n / CDbl(binCount)
            Dim current As Integer = 0
            Dim nextCut As Double = stepSize

            For i As Integer = 0 To n - 1

                ' 只在「值发生变化」的切点处推进桶号，保证相同取值落在同一桶
                If i > 0 AndAlso keys(i) <> keys(i - 1) Then

                    While i >= nextCut AndAlso current < binCount - 1
                        current += 1
                        nextCut += stepSize
                    End While
                End If

                bins(i) = current
            Next

            Dim actual As Integer = current + 1
            Dim thresholds As Single() = New Single(actual - 1) {}
            Dim seen As Boolean() = New Boolean(actual - 1) {}

            For i As Integer = 0 To n - 1
                ' keys 升序，因此后写覆盖前写即得到桶内最大值
                thresholds(bins(i)) = keys(i)
                seen(bins(i)) = True
            Next

            ' 空桶继承前一个桶的阈值（对应的分裂点与前一桶等价，不会产生额外候选）
            For i As Integer = 1 To actual - 1

                If Not seen(i) Then
                    thresholds(i) = thresholds(i - 1)
                End If
            Next

            Return New FeatureBinning(bins, thresholds, actual)
        End Function

        Public Overrides Function ToString() As String
            Return $"bins={BucketCount}"
        End Function
    End Class
End Namespace
