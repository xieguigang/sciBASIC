Imports System.Buffers
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Java
Imports par = System.Threading.Tasks.Parallel
Imports sysmath = System.Math
Imports taskParallel = System.Threading.Tasks.ParallelOptions

Namespace train.kernels

    ''' <summary>
    ''' 一列、一层、一组存活节点的扫描上下文。
    '''
    ''' 把「按节点分桶后的有序列位置」交给具体的分裂查找器实现，
    ''' 从而让 exact / histogram 两种算法共用同一套分桶与缺失值统计逻辑。
    ''' </summary>
    Friend Class ScanContext

        Public Nodes As List(Of BuildNode)
        ''' <summary>
        ''' 节点编号 -&gt; 本层存活下标，<c>-1</c> 表示该节点已不是存活节点
        ''' </summary>
        Public SlotIndex As Integer()
        Public Feature As Integer
        ''' <summary>
        ''' 按节点分桶后的「列有序位置」缓冲区，前 <c>total</c> 项有效
        ''' </summary>
        Public Positions As Integer()
        Public Starts As Integer()
        Public Counts As Integer()
        Public Grad As Double()
        Public Hess As Double()
        Public Mask As Boolean()
        Public NanG As Double()
        Public NanH As Double()
        Public NanCount As Integer()
        Public Best As SplitCandidate()
    End Class

    ''' <summary>
    ''' 分裂查找器基类。
    '''
    ''' 负责三件事：
    ''' 1. 把「哪些行属于哪些存活节点」按列分桶（一次 O(N) 计数排序，替代旧实现里
    '''    每层「全量 N 扫描 + 每个区间重建 HashSet」的做法）；
    ''' 2. 统计每个节点、每一列的缺失值梯度/海森；
    ''' 3. 并行调度（<c>Parallel.For</c> + <c>MaxDegreeOfParallelism</c>，让 <c>num_thread</c> 真正生效）。
    '''
    ''' 具体的「如何在节点的有序切片上搜索分裂点」由子类实现：
    ''' <see cref="ExactSplitFinder"/>（精确前缀扫描）/ <see cref="HistogramSplitFinder"/>（分桶直方图）。
    ''' </summary>
    Friend MustInherit Class SplitFinderBase

        Protected ReadOnly columns As ColumnBlock
        Protected ReadOnly lambda As Double
        Protected ReadOnly gamma As Double
        Protected ReadOnly minChildWeight As Double
        Protected ReadOnly minSamples As Integer
        Protected ReadOnly numThread As Integer

        Protected Sub New(columns As ColumnBlock, lambda As Double, gamma As Double,
                          minChildWeight As Double, minSamples As Integer, numThread As Integer)
            Me.columns = columns
            Me.lambda = lambda
            Me.gamma = gamma
            Me.minChildWeight = minChildWeight
            Me.minSamples = minSamples
            Me.numThread = numThread
        End Sub

        ''' <summary>
        ''' 子类的分裂扫描实现
        ''' </summary>
        Protected MustOverride Sub Scan(ctx As ScanContext)

        ''' <summary>
        ''' 为一层内的全部存活节点寻找最优分裂。
        ''' </summary>
        ''' <returns>
        ''' 与 <paramref name="nodes"/> 等长、索引一致的候选数组；
        ''' 无有效分裂的位置为 <see cref="SplitCandidate.NoSplit"/>。
        ''' </returns>
        Public Function FindAll(nodes As List(Of BuildNode), partitioner As RowPartitioner,
                                grad As Double(), hess As Double(), mask As Boolean(),
                                colSelected As List(Of Integer)) As SplitCandidate()

            Dim nodeCount As Integer = nodes.Count
            Dim best As SplitCandidate() = New SplitCandidate(nodeCount - 1) {}

            For i As Integer = 0 To nodeCount - 1
                best(i) = SplitCandidate.NoSplit
            Next

            If nodeCount = 0 OrElse colSelected Is Nothing OrElse colSelected.Count = 0 Then
                Return best
            End If

            Dim maxId As Integer = -1

            For i As Integer = 0 To nodeCount - 1

                If nodes(i).Id > maxId Then
                    maxId = nodes(i).Id
                End If
            Next

            ' 叶节点仍然持有行，因此 Slot 中的取值可能超出本层存活节点的最大编号
            maxId = sysmath.Max(maxId, partitioner.MaxNodeId)

            Dim slotIndex As Integer() = New Integer(maxId) {}

            For i As Integer = 0 To maxId
                slotIndex(i) = -1
            Next

            For i As Integer = 0 To nodeCount - 1
                slotIndex(nodes(i).Id) = i
            Next

            Dim numericCols As New List(Of Integer)()
            Dim catCols As New List(Of Integer)()

            For Each f As Integer In colSelected

                If f < 0 OrElse f >= columns.FeatureDim Then
                    Continue For
                End If

                If columns.IsCategorical(f) Then
                    Call catCols.Add(f)
                Else
                    Call numericCols.Add(f)
                End If
            Next

            Dim options As New taskParallel With {
                .MaxDegreeOfParallelism = If(numThread <= 0, -1, numThread)
            }
            Dim sync As New Object()

            If numericCols.Count > 0 Then
                Call par.For(0, numericCols.Count, options,
                    Sub(ci As Integer)
                        Dim local As SplitCandidate() = New SplitCandidate(nodeCount - 1) {}

                        For i As Integer = 0 To nodeCount - 1
                            local(i) = SplitCandidate.NoSplit
                        Next

                        Call ScanNumericColumn(nodes, slotIndex, partitioner, numericCols(ci), grad, hess, mask, local)

                        SyncLock sync

                            For i As Integer = 0 To nodeCount - 1

                                If local(i).HasSplit AndAlso
                                    (Not best(i).HasSplit OrElse local(i).Gain > best(i).Gain) Then

                                    best(i) = local(i)
                                End If
                            Next
                        End SyncLock
                    End Sub)
            End If

            ' 类别列通常很少，串行处理（内部按节点循环）
            For Each f As Integer In catCols
                Call ScanCategoricalColumn(nodes, slotIndex, partitioner, f, grad, hess, mask, best)
            Next

            Return best
        End Function

        ''' <summary>
        ''' 计算分裂增益（已扣除 <paramref name="gamma"/>）。
        '''
        ''' <c>gain = 0.5 * (GL²/(HL+λ) + GR²/(HR+λ) + GN²/(HN+λ) − G²/(H+λ)) − γ</c>；
        ''' 同时枚举「缺失值进独立子节点 / 进左 / 进右」三种方案。
        '''
        ''' 关键优化：当该列在该节点没有任何缺失值（<c>GN = HN = 0</c>）时三种方案恒等，
        ''' 直接返回，不再多算 2 次平方与 2 次除法。旧实现把该判断写在三次计算之后，
        ''' 而这是最内层热路径（每层 特征数 × 切分点数 次调用）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Shared Function SplitGain(gl As Double, hl As Double, gr As Double, hr As Double,
                                            gn As Double, hn As Double, totalG As Double, totalH As Double,
                                            lambda As Double, gamma As Double,
                                            ByRef nanDirection As Integer) As Double

            Dim parent As Double = totalG * totalG / (totalH + lambda)
            Dim g1 As Double = gl * gl / (hl + lambda) + gr * gr / (hr + lambda) + gn * gn / (hn + lambda) - parent

            If gn = 0 AndAlso hn = 0 Then
                nanDirection = SplitCandidate.NanDirectionNone
                Return 0.5 * g1 - gamma
            End If

            Dim g2 As Double = (gl + gn) * (gl + gn) / (hl + hn + lambda) + gr * gr / (hr + lambda) - parent
            Dim g3 As Double = gl * gl / (hl + lambda) + (gr + gn) * (gr + gn) / (hr + hn + lambda) - parent
            Dim bestGain As Double = g1

            nanDirection = SplitCandidate.NanDirectionChild

            If g2 > bestGain Then
                bestGain = g2
                nanDirection = SplitCandidate.NanDirectionLeft
            End If

            If g3 > bestGain Then
                bestGain = g3
                nanDirection = SplitCandidate.NanDirectionRight
            End If

            Return 0.5 * bestGain - gamma
        End Function

        ''' <summary>
        ''' 对一列数值特征做「按节点分桶」，然后交给 <see cref="Scan"/>
        ''' </summary>
        Private Sub ScanNumericColumn(nodes As List(Of BuildNode), slotIndex As Integer(),
                                      partitioner As RowPartitioner, feature As Integer,
                                      grad As Double(), hess As Double(), mask As Boolean(),
                                      best As SplitCandidate())

            Dim ids = columns.RowIds(feature)

            If ids Is Nothing OrElse ids.Length = 0 Then
                Return
            End If

            Dim n As Integer = ids.Length
            Dim nodeCount As Integer = nodes.Count
            Dim slotOf = partitioner.Slot

            Dim counts As Integer() = New Integer(nodeCount - 1) {}
            Dim starts As Integer() = New Integer(nodeCount - 1) {}
            Dim nanG As Double() = New Double(nodeCount - 1) {}
            Dim nanH As Double() = New Double(nodeCount - 1) {}
            Dim nanCount As Integer() = New Integer(nodeCount - 1) {}

            ' 1) 缺失值梯度统计
            Dim nanRows = columns.NanRows(feature)

            If nanRows IsNot Nothing Then

                For Each row As Integer In nanRows
                    Dim s As Integer = slotOf(row)

                    If s >= 0 Then
                        Dim ai As Integer = slotIndex(s)

                        If ai >= 0 AndAlso (mask Is Nothing OrElse mask(row)) Then
                            nanG(ai) += grad(row)
                            nanH(ai) += hess(row)
                            nanCount(ai) += 1
                        End If
                    End If
                Next
            End If

            ' 2) 每个节点在本列上的样本量
            For pos As Integer = 0 To n - 1
                Dim s As Integer = slotOf(ids(pos))

                If s >= 0 Then
                    Dim ai As Integer = slotIndex(s)

                    If ai >= 0 Then
                        counts(ai) += 1
                    End If
                End If
            Next

            Dim total As Integer = 0

            For i As Integer = 0 To nodeCount - 1
                starts(i) = total
                total += counts(i)
            Next

            If total = 0 Then
                Return
            End If

            ' 3) 计数排序：把「列有序位置」按节点聚成连续片段
            Dim pool = ArrayPool(Of Integer).Shared
            Dim positions = pool.Rent(total)

            Try
                Dim cursor As Integer() = New Integer(nodeCount - 1) {}

                For i As Integer = 0 To nodeCount - 1
                    cursor(i) = starts(i)
                Next

                For pos As Integer = 0 To n - 1
                    Dim s As Integer = slotOf(ids(pos))

                    If s >= 0 Then
                        Dim ai As Integer = slotIndex(s)

                        If ai >= 0 Then
                            positions(cursor(ai)) = pos
                            cursor(ai) += 1
                        End If
                    End If
                Next

                Dim ctx As New ScanContext With {
                    .Nodes = nodes,
                    .SlotIndex = slotIndex,
                    .Feature = feature,
                    .Positions = positions,
                    .Starts = starts,
                    .Counts = counts,
                    .Grad = grad,
                    .Hess = hess,
                    .Mask = mask,
                    .NanG = nanG,
                    .NanH = nanH,
                    .NanCount = nanCount,
                    .Best = best
                }

                Call Scan(ctx)
            Finally
                Call pool.Return(positions)
            End Try
        End Sub

        ''' <summary>
        ''' 类别特征：先按节点统计每个类别编码的 G/H，再按 <c>G/H</c> 排序做前缀扫描。
        ''' </summary>
        Private Sub ScanCategoricalColumn(nodes As List(Of BuildNode), slotIndex As Integer(),
                                          partitioner As RowPartitioner, feature As Integer,
                                          grad As Double(), hess As Double(), mask As Boolean(),
                                          best As SplitCandidate())

            Dim cats = columns.CategoryValues(feature)
            Dim codes = columns.CategoryCodes(feature)

            If cats Is Nothing OrElse cats.Length = 0 OrElse codes Is Nothing Then
                Return
            End If

            Dim catCount As Integer = cats.Length
            Dim nodeCount As Integer = nodes.Count
            Dim slotOf = partitioner.Slot
            Dim origin = columns.Origin
            Dim cg As Double() = New Double(nodeCount * catCount - 1) {}
            Dim ch As Double() = New Double(nodeCount * catCount - 1) {}
            Dim cn As Integer() = New Integer(nodeCount * catCount - 1) {}

            For ai As Integer = 0 To nodeCount - 1
                Dim node = nodes(ai)
                Dim rows = partitioner.RowsOf(node.Id)
                Dim offset As Integer = ai * catCount

                For Each row As Integer In rows
                    Dim v As Single = origin(row)(feature)

                    If ColumnBlock.IsMissing(v) Then
                        Continue For
                    End If

                    Dim code As Integer

                    If Not codes.TryGetValue(v, code) Then
                        Continue For
                    End If

                    If mask Is Nothing OrElse mask(row) Then
                        cg(offset + code) += grad(row)
                        ch(offset + code) += hess(row)
                        cn(offset + code) += 1
                    End If
                Next
            Next

            Dim nanRows = columns.NanRows(feature)
            Dim order As Integer() = New Integer(catCount - 1) {}

            For ai As Integer = 0 To nodeCount - 1
                Dim node = nodes(ai)
                Dim offset As Integer = ai * catCount
                Dim gn As Double = 0
                Dim hn As Double = 0
                Dim nn As Integer = 0

                If nanRows IsNot Nothing Then

                    For Each row As Integer In nanRows

                        If slotOf(row) = node.Id AndAlso (mask Is Nothing OrElse mask(row)) Then
                            gn += grad(row)
                            hn += hess(row)
                            nn += 1
                        End If
                    Next
                End If

                ' 只保留有样本的类别
                Dim m As Integer = 0

                For code As Integer = 0 To catCount - 1

                    If cn(offset + code) > 0 Then
                        order(m) = code
                        m += 1
                    End If
                Next

                If m < 2 Then
                    ' 少于两个类别，无法构造非空的两侧子集
                    Continue For
                End If

                ' 插入排序：按 G/H 升序（类别数通常很小）
                For idx As Integer = 1 To m - 1
                    Dim key As Integer = order(idx)
                    Dim keyRatio As Double = ratioG2H(cg(offset + key), ch(offset + key))
                    Dim j As Integer = idx - 1

                    While j >= 0 AndAlso ratioG2H(cg(offset + order(j)), ch(offset + order(j))) > keyRatio
                        order(j + 1) = order(j)
                        j -= 1
                    End While

                    order(j + 1) = key
                Next

                Dim gl As Double = 0
                Dim hl As Double = 0
                Dim picked As New List(Of Integer)(m)
                Dim bestGain As Double = If(best(ai).HasSplit, best(ai).Gain, 0.0)

                For idx As Integer = 0 To m - 2
                    Dim code As Integer = order(idx)

                    gl += cg(offset + code)
                    hl += ch(offset + code)
                    Call picked.Add(code)

                    Dim gr As Double = node.Grad - gl - gn
                    Dim hr As Double = node.Hess - hl - hn
                    Dim dir As Integer
                    Dim gain As Double = SplitGain(gl, hl, gr, hr, gn, hn, node.Grad, node.Hess, lambda, gamma, dir)

                    If gain > bestGain Then
                        bestGain = gain
                        best(ai) = New SplitCandidate With {
                            .Feature = feature,
                            .Gain = gain,
                            .LeftCategories = picked.ToArray,
                            .NanDirection = dir
                        }
                    End If
                Next
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function ratioG2H(g As Double, h As Double) As Double
            If h > 0 Then
                Return g / h
            Else
                Return Double.MaxValue
            End If
        End Function
    End Class
End Namespace
