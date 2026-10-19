Option Strict On

Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities

Namespace AprioriRules.Engine

    ''' <summary>
    ''' 强关联规则挖掘（ap-genrules）
    ''' </summary>
    ''' <remarks>
    ''' 原实现为每个频繁项集枚举全部「大小不超过 k/2 的前件」，为其中每一个都构造
    ''' <see cref="Rule"/>（连带两个 <see cref="ItemSet"/>）塞进 <see cref="HashSet(Of T)"/> 去重，
    ''' 最后才按 min_confidence 过滤掉绝大多数。这里改成：
    ''' 
    ''' 1. 项集的子集用 k 位 <see cref="ULong"/> 掩码表示，子集格上只做位运算，不分配对象；
    ''' 2. 后件从 1 项逐层长大，置信度不达标的后件连同它的所有超集一并剪掉
    '''    （h' ⊇ h ⇒ 前件 XY\h' ⊆ XY\h ⇒ 支持度更大 ⇒ 置信度更小，必然同样不达标）；
    ''' 3. 每个有序规则 (前件→后件) 恰好被生成一次，因此不再需要 HashSet 去重；
    ''' 4. 只有真正达标的规则才会物化成 <see cref="Rule"/>。
    ''' 
    ''' 与原实现产出的规则集合完全一致：原实现枚举 |X| &lt;= k/2 的拆分后再双向产出，
    ''' 合起来正是「所有满足 min_confidence 的有序规则」。
    ''' </remarks>
    Friend Class RuleMiner

        ReadOnly _index As TransactionIndex
        ReadOnly _supports As Dictionary(Of Integer(), Integer)
        ReadOnly _minConfidence As Double
        ReadOnly _parallelism As Integer

        Sub New(index As TransactionIndex,
                supports As Dictionary(Of Integer(), Integer),
                minConfidence As Double,
                parallelism As Integer)

            _index = index
            _supports = supports
            _minConfidence = minConfidence
            _parallelism = parallelism
        End Sub

        ''' <summary>
        ''' 挖掘全部频繁项集上的强规则
        ''' </summary>
        Friend Function Mine(levels As List(Of Integer()()), levelSupports As List(Of Integer())) As List(Of Rule)
            Dim all As New List(Of Integer())()
            Dim sups As New List(Of Integer)()

            For li As Integer = 1 To levels.Count - 1
                Dim items As Integer()() = levels(li)
                Dim ss As Integer() = levelSupports(li)

                For a As Integer = 0 To items.Length - 1
                    all.Add(items(a))
                    sups.Add(ss(a))
                Next
            Next

            If all.Count = 0 Then
                Return New List(Of Rule)()
            End If

            Dim procs As Integer = If(_parallelism > 0,
                                      System.Math.Min(_parallelism, Environment.ProcessorCount),
                                      Environment.ProcessorCount)
            Dim nChunks As Integer

            If all.Count < 64 Then
                nChunks = 1
            Else
                nChunks = System.Math.Max(1, System.Math.Min(procs, (all.Count + 31) \ 32))
            End If

            Dim bounds As Integer() = New Integer(nChunks) {}

            For c As Integer = 0 To nChunks
                bounds(c) = CInt(System.Math.Floor(CDbl(all.Count) * c / nChunks))
            Next

            bounds(nChunks) = all.Count

            Dim parts As List(Of Rule)() = New List(Of Rule)(nChunks - 1) {}

            If nChunks = 1 Then
                parts(0) = MineRange(all, sups, 0, all.Count)
            Else
                Dim opts As New System.Threading.Tasks.ParallelOptions() With {
                    .MaxDegreeOfParallelism = procs
                }

                System.Threading.Tasks.Parallel.For(0, nChunks, opts,
                    Sub(ci As Integer)
                        parts(ci) = MineRange(all, sups, bounds(ci), bounds(ci + 1))
                    End Sub)
            End If

            Dim result As New List(Of Rule)()

            For ci As Integer = 0 To nChunks - 1
                result.AddRange(parts(ci))
            Next

            result.Sort()

            Return result
        End Function

        Private Function MineRange(all As List(Of Integer()),
                                   sups As List(Of Integer),
                                   start As Integer,
                                   upTo As Integer) As List(Of Rule)

            Dim out As New List(Of Rule)()
            ' 复用的临时缓冲区：pool(n) 是长度恰好为 n 的项集缓冲，bits 是位置的位掩码
            Dim pool As Integer()() = New Integer(64)() {}
            Dim bits As ULong() = New ULong(63) {}
            Dim cur As New List(Of ULong)()
            Dim alive As New List(Of ULong)()
            Dim nextList As New List(Of ULong)()
            Dim seen As New HashSet(Of ULong)()

            For a As Integer = start To upTo - 1
                MineOne(all(a), sups(a), bits, pool, cur, alive, nextList, seen, out)
            Next

            Return out
        End Function

        ''' <summary>
        ''' 在单个频繁项集 XY 上按后件大小逐层推进
        ''' </summary>
        Private Sub MineOne(XY As Integer(),
                            supXY As Integer,
                            bits As ULong(),
                            pool As Integer()(),
                            cur As List(Of ULong),
                            alive As List(Of ULong),
                            nextList As List(Of ULong),
                            seen As HashSet(Of ULong),
                            out As List(Of Rule))

            Dim k As Integer = XY.Length

            If k > 63 Then
                ' 超过 63 项时无法用单个 ULong 表示子集，退回逐个组合枚举
                Call MineLarge(XY, supXY, out)
                Return
            End If

            For i As Integer = 0 To k - 1
                bits(i) = 1UL << i
            Next

            Dim full As ULong = (1UL << k) - 1UL

            cur.Clear()

            For i As Integer = 0 To k - 1
                cur.Add(bits(i))
            Next

            For m As Integer = 1 To k - 1
                alive.Clear()

                Dim anteSize As Integer = k - m

                For Each h As ULong In cur
                    Dim ante As ULong = full Xor h
                    Dim supA As Integer = SupportOfMask(XY, ante, anteSize, bits, pool)

                    If supA <= 0 Then
                        Continue For
                    End If

                    Dim conf As Double = CDbl(supXY) / CDbl(supA)

                    If conf >= _minConfidence Then
                        Call out.Add(MakeRule(XY, ante, anteSize, m, bits, supXY, supA, conf))
                        alive.Add(h)
                    End If
                Next

                If m = k - 1 Then
                    Exit For
                End If

                ' 只从存活的后件扩展：不达标者的超集必然同样不达标
                seen.Clear()
                nextList.Clear()

                For Each h As ULong In alive
                    For p As Integer = 0 To k - 1
                        If (h And bits(p)) = 0UL Then
                            Dim u As ULong = h Or bits(p)

                            If seen.Add(u) Then
                                nextList.Add(u)
                            End If
                        End If
                    Next
                Next

                If nextList.Count = 0 Then
                    Exit For
                End If

                cur.Clear()
                cur.AddRange(nextList)
            Next
        End Sub

        Private Function SupportOfMask(XY As Integer(),
                                       mask As ULong,
                                       size As Integer,
                                       bits As ULong(),
                                       pool As Integer()()) As Integer
            Dim buf As Integer() = pool(size)

            If buf Is Nothing Then
                buf = New Integer(size - 1) {}
                pool(size) = buf
            End If

            Dim w As Integer = 0

            For i As Integer = 0 To XY.Length - 1
                If (mask And bits(i)) <> 0UL Then
                    buf(w) = XY(i)
                    w += 1
                End If
            Next

            Dim s As Integer

            If _supports.TryGetValue(buf, s) Then
                Return s
            End If

            Return 0
        End Function

        Private Function MakeRule(XY As Integer(),
                                  ante As ULong,
                                  anteSize As Integer,
                                  consSize As Integer,
                                  bits As ULong(),
                                  supXY As Integer,
                                  supA As Integer,
                                  conf As Double) As Rule

            Dim xItems As Item() = New Item(anteSize - 1) {}
            Dim yItems As Item() = New Item(consSize - 1) {}
            Dim xi As Integer = 0
            Dim yi As Integer = 0

            For i As Integer = 0 To XY.Length - 1
                If (ante And bits(i)) <> 0UL Then
                    xItems(xi) = _index.ItemOf(XY(i))
                    xi += 1
                Else
                    yItems(yi) = _index.ItemOf(XY(i))
                    yi += 1
                End If
            Next

            Return New Rule(New ItemSet(xItems), New ItemSet(yItems), conf, (CDbl(supXY), CDbl(supA)))
        End Function

        ''' <summary>
        ''' 项集长度超过 63 时的回退路径：枚举 |X| &lt;= k/2 的前件（与原实现一致），再双向产出
        ''' </summary>
        Private Sub MineLarge(XY As Integer(), supXY As Integer, out As List(Of Rule))
            Dim k As Integer = XY.Length
            Dim pool As Integer()() = New Integer(k)() {}
            Dim stack As Integer() = New Integer(k - 1) {}

            For m As Integer = 1 To k \ 2
                Call EnumerateLarge(XY, supXY, k, m, 0, 0, stack, pool, out)
            Next
        End Sub

        Private Sub EnumerateLarge(XY As Integer(),
                                   supXY As Integer,
                                   k As Integer,
                                   m As Integer,
                                   depth As Integer,
                                   position As Integer,
                                   stack As Integer(),
                                   pool As Integer()(),
                                   out As List(Of Rule))

            If depth = m Then
                ' 偶数长度项集的对半拆分只保留包含首个元素的那一半，避免同一拆分产出两次
                If m * 2 = k AndAlso stack(0) <> 0 Then
                    Return
                End If

                Dim xIds As Integer() = New Integer(m - 1) {}
                Dim yIds As Integer() = New Integer(k - m - 1) {}
                Dim yi As Integer = 0
                Dim xi As Integer = 0

                For p As Integer = 0 To k - 1
                    If xi < m AndAlso stack(xi) = p Then
                        xIds(xi) = XY(p)
                        xi += 1
                    Else
                        yIds(yi) = XY(p)
                        yi += 1
                    End If
                Next

                Dim supX As Integer
                Dim supY As Integer

                _supports.TryGetValue(xIds, supX)
                _supports.TryGetValue(yIds, supY)

                If supX > 0 AndAlso CDbl(supXY) / CDbl(supX) >= _minConfidence Then
                    Call out.Add(New Rule(New ItemSet(ToItems(xIds)), New ItemSet(ToItems(yIds)),
                                          CDbl(supXY) / CDbl(supX), (CDbl(supXY), CDbl(supX))))
                End If
                If supY > 0 AndAlso CDbl(supXY) / CDbl(supY) >= _minConfidence Then
                    Call out.Add(New Rule(New ItemSet(ToItems(yIds)), New ItemSet(ToItems(xIds)),
                                          CDbl(supXY) / CDbl(supY), (CDbl(supXY), CDbl(supY))))
                End If

                Return
            End If

            For p As Integer = position To k - (m - depth)
                stack(depth) = p
                Call EnumerateLarge(XY, supXY, k, m, depth + 1, p + 1, stack, pool, out)
            Next
        End Sub

        Private Function ToItems(ids As Integer()) As Item()
            Dim items As Item() = New Item(ids.Length - 1) {}

            For i As Integer = 0 To ids.Length - 1
                items(i) = _index.ItemOf(ids(i))
            Next

            Return items
        End Function
    End Class
End Namespace
