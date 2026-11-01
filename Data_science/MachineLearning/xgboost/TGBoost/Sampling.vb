Namespace train

    ''' <summary>
    ''' 行（样本）子采样器。
    '''
    ''' 旧实现只在构造时随机一次 0/1 掩码，之后每轮只是 <c>Shuffle</c> 同一组掩码——
    ''' 每轮被选中的样本数恒定、且是「同一多重集合的置换」，与 xgboost 的
    ''' <c>subsample</c>（每轮独立抽样）语义不同，也无法保证每轮参与的训练子集不同。
    '''
    ''' 现在 <see cref="Resample"/> 每轮重新做伯努利抽样，并支持注入随机种子以便复现。
    ''' </summary>
    Public Class RowSampler

        ''' <summary>
        ''' 最近一次的 0/1 掩码（保留该字段以兼容既有调用方）
        ''' </summary>
        Public row_mask As New List(Of Double)()

        Private ReadOnly rowCount As Integer
        Private ReadOnly rate As Double
        Private ReadOnly rand As Random

        Public Sub New(n As Integer, sampling_rate As Double)
            Me.New(n, sampling_rate, Nothing)
        End Sub

        ''' <param name="n">样本数</param>
        ''' <param name="sampling_rate">采样率，会被裁剪到 [0, 1]</param>
        ''' <param name="seed">随机种子；<c>Nothing</c> 表示使用时间种子</param>
        Public Sub New(n As Integer, sampling_rate As Double, seed As Integer?)
            Me.rowCount = Math.Max(n, 0)
            Me.rate = If(Double.IsNaN(sampling_rate), 1.0, Math.Min(Math.Max(sampling_rate, 0.0), 1.0))
            Me.rand = If(seed.HasValue, New Random(seed.Value), New Random())

            Call buildMask()
        End Sub

        Private Sub buildMask()
            row_mask.Clear()

            For i As Integer = 0 To rowCount - 1
                row_mask.Add(If(rand.NextDouble() < rate, 1.0, 0.0))
            Next
        End Sub

        ''' <summary>
        ''' 重新抽样一轮
        ''' </summary>
        Public Overridable Sub shuffle()
            SyncLock rand
                Call buildMask()
            End SyncLock
        End Sub

        ''' <summary>
        ''' 返回本轮的布尔掩码（<c>Nothing</c> 表示全量参与）
        ''' </summary>
        ''' <returns></returns>
        Public Function Resample() As Boolean()
            If rate >= 1.0 Then
                Return Nothing
            End If

            Call shuffle()

            Dim mask As Boolean() = New Boolean(rowCount - 1) {}

            For i As Integer = 0 To rowCount - 1
                mask(i) = row_mask(i) > 0.5
            Next

            Return mask
        End Function

        Public ReadOnly Property Rate As Double
            Get
                Return rate
            End Get
        End Property
    End Class

    ''' <summary>
    ''' 列（特征）子采样器。
    '''
    ''' 旧实现用 <c>CInt(n * sampling_rate)</c> 计算列数，在特征少或采样率低时可能得到 0，
    ''' 于是「一棵树没有任何候选特征」——树无法分裂且没有任何提示。现在至少保留 1 列。
    ''' </summary>
    Public Class ColumnSampler

        Private ReadOnly cols As New List(Of Integer)()
        Private ReadOnly rand As Random

        ''' <summary>
        ''' 当前轮次被选中的特征列
        ''' </summary>
        Public col_selected As List(Of Integer)
        Private ReadOnly n_selected As Integer

        Public Sub New(n As Integer, sampling_rate As Double)
            Me.New(n, sampling_rate, Nothing)
        End Sub

        Public Sub New(n As Integer, sampling_rate As Double, seed As Integer?)
            Dim total As Integer = Math.Max(n, 1)
            Dim rate As Double = If(Double.IsNaN(sampling_rate), 1.0, Math.Min(Math.Max(sampling_rate, 0.0), 1.0))

            For i As Integer = 0 To total - 1
                Call cols.Add(i)
            Next

            Dim k As Integer = CInt(Math.Round(total * rate))

            If k < 1 Then
                k = 1
            ElseIf k > total Then
                k = total
            End If

            Me.n_selected = k
            Me.rand = If(seed.HasValue, New Random(seed.Value), New Random())
            Me.col_selected = cols.GetRange(0, n_selected)
        End Sub

        ''' <summary>
        ''' 重新抽样一轮特征列
        ''' </summary>
        Public Overridable Sub shuffle()
            SyncLock rand

                For i As Integer = cols.Count - 1 To 1 Step -1
                    Dim j As Integer = rand.Next(i + 1)
                    Dim tmp As Integer = cols(i)
                    cols(i) = cols(j)
                    cols(j) = tmp
                Next
            End SyncLock

            col_selected = cols.GetRange(0, n_selected)
        End Sub

        ''' <summary>
        ''' 当前轮次选中的特征列（调用前请先 <see cref="shuffle"/>）
        ''' </summary>
        ''' <returns></returns>
        Public Function SelectedColumns() As List(Of Integer)
            Return col_selected
        End Function
    End Class
End Namespace
