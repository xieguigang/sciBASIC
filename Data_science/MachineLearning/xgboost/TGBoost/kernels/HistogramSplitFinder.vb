Namespace train.kernels

    ''' <summary>
    ''' 直方图（hist）分裂查找器。
    '''
    ''' 先把节点的有序切片累积到 <c>max_bin</c> 个桶的直方图上，再在直方图上做前缀扫描。
    ''' 当特征取值基数很大（连续型、浮点测量值）时，exact 模式需要为每个不同取值评估一次分裂，
    ''' 本模式把评估次数压到桶数级别，是 <c>tree_method=hist</c> 的主要收益来源。
    '''
    ''' 注意：分桶会引入量化误差，因此默认的 <c>tree_method</c> 仍是 <c>exact</c>；
    ''' 需要显式开启本模式，并通过回归/基准报告评估指标偏差。
    ''' </summary>
    Friend Class HistogramSplitFinder
        Inherits SplitFinderBase

        Private ReadOnly maxBin As Integer

        Public Sub New(columns As ColumnBlock, lambda As Double, gamma As Double,
                       minChildWeight As Double, minSamples As Integer, numThread As Integer,
                       maxBin As Integer)
            MyBase.New(columns, lambda, gamma, minChildWeight, minSamples, numThread)
            Me.maxBin = If(maxBin <= 1, 256, maxBin)
        End Sub

        Protected Overrides Sub Scan(ctx As ScanContext)
            Dim binning = columns.GetBins(ctx.Feature, maxBin)
            Dim bucketCount As Integer = binning.BucketCount

            If bucketCount <= 1 Then
                Return
            End If

            Dim bins = binning.Bins
            ' 窄整型视图：桶数 <= 65535 时优先使用，减少内层循环的访存带宽
            Dim bins16 = binning.Bins16
            Dim thresholds = binning.Thresholds
            Dim ids = columns.RowIds(ctx.Feature)
            Dim positions = ctx.Positions
            Dim nodes = ctx.Nodes
            Dim grad = ctx.Grad
            Dim hess = ctx.Hess
            Dim mask = ctx.Mask
            Dim binG As Double() = New Double(bucketCount - 1) {}
            Dim binH As Double() = New Double(bucketCount - 1) {}

            For ai As Integer = 0 To nodes.Count - 1
                Dim count As Integer = ctx.Counts(ai)

                If count < 2 Then
                    Continue For
                End If

                Call Array.Clear(binG, 0, bucketCount)
                Call Array.Clear(binH, 0, bucketCount)

                Dim j0 As Integer = ctx.Starts(ai)
                Dim j1 As Integer = j0 + count - 1

                For t As Integer = j0 To j1
                    Dim pos As Integer = positions(t)
                    Dim row As Integer = ids(pos)

                    If mask Is Nothing OrElse mask(row) Then
                        Dim bin As Integer = If(bins16 IsNot Nothing, CInt(bins16(pos)), bins(pos))

                        binG(bin) += grad(row)
                        binH(bin) += hess(row)
                    End If
                Next

                Dim node = nodes(ai)
                Dim gn As Double = ctx.NanG(ai)
                Dim hn As Double = ctx.NanH(ai)
                Dim gl As Double = 0
                Dim hl As Double = 0
                Dim bestGain As Double = If(ctx.Best(ai).HasSplit, ctx.Best(ai).Gain, 0.0)
                Dim improved As Boolean = False
                Dim bestThreshold As Single = 0
                Dim bestDirection As Integer = SplitCandidate.NanDirectionNone

                For bin As Integer = 0 To bucketCount - 2
                    gl += binG(bin)
                    hl += binH(bin)

                    Dim gr As Double = node.Grad - gl - gn
                    Dim hr As Double = node.Hess - hl - hn
                    Dim direction As Integer
                    Dim gain As Double = SplitGain(gl, hl, gr, hr, gn, hn, node.Grad, node.Hess, lambda, gamma, direction)

                    If gain > bestGain Then
                        bestGain = gain
                        bestThreshold = thresholds(bin)
                        bestDirection = direction
                        improved = True
                    End If
                Next

                If improved Then
                    ctx.Best(ai) = New SplitCandidate With {
                        .Feature = ctx.Feature,
                        .Gain = bestGain,
                        .Threshold = bestThreshold,
                        .NanDirection = bestDirection
                    }
                End If
            Next
        End Sub
    End Class
End Namespace
