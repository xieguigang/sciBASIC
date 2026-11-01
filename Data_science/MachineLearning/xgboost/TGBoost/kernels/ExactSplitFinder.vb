Namespace train.kernels

    ''' <summary>
    ''' 精确（exact）分裂查找器。
    '''
    ''' 在节点自身的有序切片上做前缀扫描，于每个「特征值发生变化」的位置评估分裂增益。
    ''' 语义与旧的 SLIQ 实现等价（因此可用于新旧实现的结果对照），但：
    ''' + 只扫描本节点的行，而不是每层重建 <c>HashSet(Of TreeNode)</c> 遍历全部区间；
    ''' + 用 <c>G * G</c> 代替 <c>Math.Pow(G, 2)</c>；
    ''' + 无缺失值时走单分支快速路径（见 <see cref="SplitFinderBase.SplitGain"/>）。
    ''' </summary>
    Friend Class ExactSplitFinder
        Inherits SplitFinderBase

        Public Sub New(columns As ColumnBlock, lambda As Double, gamma As Double,
                       minChildWeight As Double, minSamples As Integer, numThread As Integer)
            MyBase.New(columns, lambda, gamma, minChildWeight, minSamples, numThread)
        End Sub

        Protected Overrides Sub Scan(ctx As ScanContext)
            Dim values = columns.Values(ctx.Feature)
            Dim ids = columns.RowIds(ctx.Feature)
            Dim positions = ctx.Positions
            Dim nodes = ctx.Nodes
            Dim grad = ctx.Grad
            Dim hess = ctx.Hess
            Dim mask = ctx.Mask

            For ai As Integer = 0 To nodes.Count - 1
                Dim count As Integer = ctx.Counts(ai)

                If count < 2 Then
                    Continue For
                End If

                Dim node = nodes(ai)
                Dim j0 As Integer = ctx.Starts(ai)
                Dim j1 As Integer = j0 + count - 1
                Dim gn As Double = ctx.NanG(ai)
                Dim hn As Double = ctx.NanH(ai)
                Dim gl As Double = 0
                Dim hl As Double = 0
                Dim bestGain As Double = If(ctx.Best(ai).HasSplit, ctx.Best(ai).Gain, 0.0)
                Dim improved As Boolean = False
                Dim bestThreshold As Single = 0
                Dim bestDirection As Integer = SplitCandidate.NanDirectionNone

                For t As Integer = j0 To j1 - 1
                    Dim k As Integer = positions(t)
                    Dim row As Integer = ids(k)

                    If mask Is Nothing OrElse mask(row) Then
                        gl += grad(row)
                        hl += hess(row)
                    End If

                    ' 下一个位置的取值相同 → 当前不是切点
                    If values(positions(t + 1)) = values(k) Then
                        Continue For
                    End If

                    Dim gr As Double = node.Grad - gl - gn
                    Dim hr As Double = node.Hess - hl - hn
                    Dim direction As Integer
                    Dim gain As Double = SplitGain(gl, hl, gr, hr, gn, hn, node.Grad, node.Hess, lambda, gamma, direction)

                    If gain > bestGain Then
                        bestGain = gain
                        bestThreshold = values(k)
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
