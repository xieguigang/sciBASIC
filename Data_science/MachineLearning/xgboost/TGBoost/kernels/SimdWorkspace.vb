Imports sysmath = System.Math

Namespace train.kernels

    ''' <summary>
    ''' 训练期复用的预分配缓冲区。
    '''
    ''' 改造前每轮/每层都会新建若干 <c>Double()</c>（梯度、海森、叶分数、临时概率），
    ''' 在 128 线程下会成为 GC 压力的主要来源。本类按行数一次性分配并复用，
    ''' 只在行数增长时扩容。
    '''
    ''' 生命周期：每棵树一个实例（<c>Tree.fit</c> 内部持有），训练结束后随树一起释放。
    ''' </summary>
    Friend Class SimdWorkspace

        Private _prob As Double() = Array.Empty(Of Double)()
        Private _leafScores As Double() = Array.Empty(Of Double)()

        ''' <summary>
        ''' 当前已分配的容量（行数）
        ''' </summary>
        Public ReadOnly Property Capacity As Integer
            Get
                Return _prob.Length
            End Get
        End Property

        ''' <summary>
        ''' 确保缓冲区至少有 <paramref name="count"/> 个元素。
        ''' </summary>
        ''' <param name="count"></param>
        Public Sub Ensure(count As Integer)
            If _prob.Length >= count Then
                Return
            End If

            _prob = New Double(count - 1) {}
            _leafScores = New Double(count - 1) {}
        End Sub

        ''' <summary>
        ''' 概率/中间量的暂存缓冲（长度 &gt;= 行数）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Prob As Double()
            Get
                Return _prob
            End Get
        End Property

        ''' <summary>
        ''' 「行 -&gt; 叶分数」的连续缓冲（长度 &gt;= 行数）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LeafScores As Double()
            Get
                Return _leafScores
            End Get
        End Property

        ''' <summary>
        ''' 清空前 <paramref name="count"/> 项
        ''' </summary>
        ''' <param name="count"></param>
        Public Sub ClearLeafScores(count As Integer)
            If count > 0 AndAlso _leafScores.Length > 0 Then
                Array.Clear(_leafScores, 0, sysmath.Min(count, _leafScores.Length))
            End If
        End Sub
    End Class
End Namespace
