Option Strict On

Imports System.Numerics

Namespace AprioriRules.Engine

    ''' <summary>
    ''' 事务数据集的索引表示层
    ''' </summary>
    ''' <remarks>
    ''' 这一层负责把外部表示（<see cref="Item"/> / <see cref="ItemSet"/>）转换成适合高频计数的内部表示：
    ''' 
    ''' 1. 外部 <see cref="Item.Code"/> 映射为紧凑的内部 id（0..d-1，按 Code 升序，
    '''    这样内部的字典序与 <see cref="Encoding"/> 的字符串序完全一致）；
    ''' 2. 每个事务转成一个升序去重的 <see cref="Integer"/> 数组；
    ''' 3. **事务按相异项数降序重排**——长度为 k 的项集只可能出现在前若干个长事务里，
    '''    于是第 k 层只需处理前 ``ceil(count_ge_k / 64)`` 个位图字，天然完成了长度剪枝；
    ''' 4. L1 支持度用一次横向扫描统计（取代原实现里「每个项各扫一遍全库」）；
    ''' 5. 只为 L1 频繁的项建立位图（稀有项根本不会出现在候选项里），
    '''    内存超预算时不建位图，退化为归并式横向计数。
    ''' </remarks>
    Friend Class TransactionIndex

        ''' <summary>
        ''' 允许为 Code 建立直接寻址表的最大编码跨度。
        ''' 超过这个跨度就退化成字典查找，避免为一个稀疏编码浪费大量内存。
        ''' </summary>
        Private Const DirectCodeTableLimit As Long = 4194304L

        ' 外部 Item.Code -> 内部 id
        ReadOnly _idOf As Dictionary(Of Integer, Integer)
        ' 内部 id -> 外部 Item
        ReadOnly _itemOf As Item()
        ' 事务（内部 id 升序去重），已按相异项数降序排列；位图模式下建完位图后释放
        Dim _tx As Integer()()
        ' 支持度分母：全部事务数（含空事务）
        ReadOnly _countN As Integer
        ReadOnly _maxLen As Integer
        ' 第 k 层需要处理的事务数 / 位图字数
        ReadOnly _geCount As Integer()
        ReadOnly _geWords As Integer()
        ReadOnly _wordsFull As Integer
        ' 内部 id -> 位图（非频繁项为 Nothing）
        Dim _masks As ULong()()
        Dim _bitset As Boolean
        ' 内部 id -> L1 支持度
        Dim _l1 As Integer()
        ' Code -> 内部 id 的直接寻址表（编码连续时启用），-1 表示未知 Code
        Dim _direct As Integer()
        Dim _minCode As Integer

        Sub New(items As IEnumerable(Of Item), transactions As ItemSet())
            Dim byCode As New Dictionary(Of Integer, Item)()

            For Each it As Item In items
                byCode(it.Code) = it
            Next

            Dim codes As Integer() = byCode.Keys.ToArray()

            Array.Sort(codes)

            _idOf = New Dictionary(Of Integer, Integer)(If(codes.Length > 0, codes.Length, 1))
            _itemOf = IntSetKeys.NewArray(Of Item)(codes.Length)

            For i As Integer = 0 To codes.Length - 1
                _idOf(codes(i)) = i
                _itemOf(i) = byCode(codes(i))
            Next

            ' <see cref="Encoding"/> 给出的 Code 是 0..d-1 的连续编号，
            ' 这种情况下用直接寻址的数组取代字典查找，逐事务转换能快一截
            If codes.Length > 0 Then
                Dim minCode As Integer = codes(0)
                Dim maxCode As Integer = codes(codes.Length - 1)

                Dim span As Long = CLng(maxCode) - CLng(minCode) + 1L

                ' 编码连续（或接近连续）时才值得换直接寻址，否则宁可用字典，
                ' 免得为一个稀疏编码白白开出一张大表
                If span <= DirectCodeTableLimit AndAlso span <= 8L * CLng(codes.Length) Then
                    _direct = New Integer(maxCode - minCode) {}
                    _minCode = minCode

                    For i As Integer = 0 To _direct.Length - 1
                        _direct(i) = -1
                    Next

                    For i As Integer = 0 To codes.Length - 1
                        _direct(codes(i) - minCode) = i
                    Next
                End If
            End If

            If transactions Is Nothing OrElse transactions.Length = 0 Then
                _countN = 0
                _maxLen = 0
                _wordsFull = 0
                _tx = New Integer()() {}
                _geCount = New Integer() {}
                _geWords = New Integer() {}
                _l1 = New Integer() {}
                Return
            End If

            Dim n As Integer = transactions.Length

            _countN = n
            _wordsFull = (n + 63) \ 64

            Dim raw As Integer()() = New Integer(n - 1)() {}
            Dim lens As Integer() = New Integer(n - 1) {}
            ' 复用的暂存缓冲，避免每个事务都经历 List 扩容 → ToArray → ReDim 这一串分配
            Dim buffer As Integer() = New Integer(31) {}
            Dim maxLen As Integer = 0
            Dim l1 As Integer() = IntSetKeys.NewArray(Of Integer)(codes.Length)

            For t As Integer = 0 To n - 1
                Dim cnt As Integer = 0
                Dim src As ItemSet = transactions(t)

                If Not src.IsNullOrEmpty() Then
                    Dim members As Item() = src.Items

                    If _direct Is Nothing Then
                        For Each it As Item In members
                            Dim id As Integer

                            If _idOf.TryGetValue(it.Code, id) Then
                                If cnt = buffer.Length Then
                                    ReDim Preserve buffer(buffer.Length * 2 - 1)
                                End If

                                buffer(cnt) = id
                                cnt += 1
                            End If
                        Next
                    Else
                        For i As Integer = 0 To members.Length - 1
                            Dim at As Integer = members(i).Code - _minCode

                            If CUInt(at) < CUInt(_direct.Length) Then
                                Dim id As Integer = _direct(at)

                                If id >= 0 Then
                                    If cnt = buffer.Length Then
                                        ReDim Preserve buffer(buffer.Length * 2 - 1)
                                    End If

                                    buffer(cnt) = id
                                    cnt += 1
                                End If
                            End If
                        Next
                    End If
                End If

                If cnt > 1 Then
                    If cnt <= 32 Then
                        ' 事务内项数一般很小，插入排序比 Array.Sort 的分派开销更划算
                        Call InsertionSort(buffer, cnt)
                    Else
                        Array.Sort(buffer, 0, cnt)
                    End If
                End If

                ' 事务内重复出现的项对子集判定没有影响，去重以减少后续工作量
                Dim w As Integer = 0

                For i As Integer = 0 To cnt - 1
                    If i = 0 OrElse buffer(i) <> buffer(i - 1) Then
                        buffer(w) = buffer(i)
                        w += 1
                        ' 顺手把 L1 支持度统计掉，省掉后面再遍历一遍全库
                        l1(buffer(w - 1)) += 1
                    End If
                    Next

                    Dim arr As Integer() = IntSetKeys.NewArray(Of Integer)(w)

                Array.Copy(buffer, arr, w)

                raw(t) = arr
                lens(t) = w

                If w > maxLen Then
                    maxLen = w
                End If
            Next

            _maxLen = maxLen

            ' 长度直方图 -> 每一层「长度 >= k」的事务数
            Dim hist As Integer() = New Integer(maxLen) {}

            For t As Integer = 0 To n - 1
                hist(lens(t)) += 1
            Next

            _geCount = New Integer(maxLen + 1) {}
            _geWords = New Integer(maxLen + 1) {}

            Dim ge As Integer = 0

            For k As Integer = maxLen To 0 Step -1
                ge += hist(k)
                _geCount(k) = ge
                _geWords(k) = (ge + 63) \ 64
            Next

            ' 按相异项数降序重排事务（事务顺序不影响任何支持度）
            Dim idxs As Integer() = New Integer(n - 1) {}

            For t As Integer = 0 To n - 1
                idxs(t) = t
            Next

            Array.Sort(lens, idxs)

            _tx = New Integer(n - 1)() {}

            For p As Integer = 0 To n - 1
                _tx(p) = raw(idxs(n - 1 - p))
            Next

            _l1 = l1
        End Sub

        ''' <summary>
        ''' 支持度分母：全部事务数（含空事务）
        ''' </summary>
        Friend ReadOnly Property TransactionCount As Integer
            Get
                Return _countN
            End Get
        End Property

        ''' <summary>
        ''' 最长事务的相异项数，也就是可能挖出的最大项集长度
        ''' </summary>
        Friend ReadOnly Property MaxLevel As Integer
            Get
                Return _maxLen
            End Get
        End Property

        ''' <summary>
        ''' 是否已经建立了位图索引（False 时走横向计数）
        ''' </summary>
        Friend ReadOnly Property UsesBitIndex As Boolean
            Get
                Return _bitset
            End Get
        End Property

        Friend Function ItemOf(id As Integer) As Item
            Return _itemOf(id)
        End Function

        Friend Function L1Support(id As Integer) As Integer
            Return _l1(id)
        End Function

        ''' <summary>
        ''' 第 <paramref name="level"/> 层需要处理的位图字数
        ''' </summary>
        Friend Function WordsForLevel(level As Integer) As Integer
            If level <= 0 OrElse level > _maxLen Then
                Return 0
            End If

            Return _geWords(level)
        End Function

        ''' <summary>
        ''' L1 支持度：在事务转换那一遍扫描里顺带统计完成，复杂度 O(N*L)。
        ''' </summary>
        ''' <remarks>
        ''' 原实现是「对每个项各扫一遍全库」的 O(d*N*L)，d 为相异项数。
        ''' </remarks>
        Friend Function GetL1() As Integer()
            Return _l1
        End Function

        ''' <summary>
        ''' 只为 L1 频繁的项建立位图。
        ''' </summary>
        ''' <returns>是否建立了位图；超内存预算时返回 False，后续走横向计数</returns>
        Friend Function BuildBitIndex(frequentIds As Integer(), budgetMB As Integer) As Boolean
            If _countN = 0 OrElse _itemOf.Length = 0 Then
                Return False
            End If

            Dim need As Long = CLng(frequentIds.Length) * CLng(_wordsFull) * 8L

            If budgetMB > 0 AndAlso need > CLng(budgetMB) * 1024L * 1024L Then
                _bitset = False
                Return False
            End If

            _masks = New ULong(_itemOf.Length - 1)() {}

            For Each id As Integer In frequentIds
                _masks(id) = New ULong(_wordsFull - 1) {}
            Next

            For t As Integer = 0 To _countN - 1
                Dim w As Integer = t >> 6
                Dim bit As ULong = 1UL << (t And 63)
                Dim arr As Integer() = _tx(t)

                For i As Integer = 0 To arr.Length - 1
                    Dim m As ULong() = _masks(arr(i))

                    If m IsNot Nothing Then
                        m(w) = m(w) Or bit
                    End If
                Next
            Next

            _bitset = True

            ' 位图模式下事务数组不再会被访问，尽早交还给 GC
            _tx = Nothing

            Return True
        End Function

        ''' <summary>
        ''' 把组内共享的前缀掩码算进 <paramref name="buf"/>
        ''' </summary>
        Friend Sub BuildPrefixMask(buf As ULong(), prefixIds As Integer(), words As Integer)
            If words <= 0 OrElse prefixIds.Length = 0 Then
                Return
            End If

            Array.Copy(_masks(prefixIds(0)), buf, words)

            For i As Integer = 1 To prefixIds.Length - 1
                Dim m As ULong() = _masks(prefixIds(i))

                For w As Integer = 0 To words - 1
                    ' 逐字提前退出：一旦前缀掩码的某个字归零，后面几项就不必再与了
                    If buf(w) <> 0UL Then
                        buf(w) = buf(w) And m(w)
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 候选 = 前缀 + 两个尾项 的支持度（位图实现）
        ''' </summary>
        Friend Function CountWithPrefix(prefix As ULong(), a As Integer, b As Integer, words As Integer) As Integer
            Dim ma As ULong() = _masks(a)
            Dim mb As ULong() = _masks(b)
            Dim cnt As Integer = 0

            For w As Integer = 0 To words - 1
                Dim v As ULong = prefix(w)

                If v = 0UL Then
                    Continue For
                End If

                v = v And ma(w)

                If v = 0UL Then
                    Continue For
                End If

                v = v And mb(w)

                If v <> 0UL Then
                    cnt += BitOperations.PopCount(v)
                End If
            Next

            Return cnt
        End Function

        ''' <summary>
        ''' 二元候选的支持度（位图实现），以支持度较小的一方作为稀疏基
        ''' </summary>
        Friend Function CountPair(baseId As Integer, otherId As Integer, words As Integer) As Integer
            Dim baseArr As ULong() = _masks(baseId)
            Dim other As ULong() = _masks(otherId)
            Dim cnt As Integer = 0

            For w As Integer = 0 To words - 1
                Dim v As ULong = baseArr(w)

                If v = 0UL Then
                    Continue For
                End If

                v = v And other(w)

                If v <> 0UL Then
                    cnt += BitOperations.PopCount(v)
                End If
            Next

            Return cnt
        End Function

        ''' <summary>
        ''' 候选的支持度（横向归并实现，位图超内存预算时的降级路径）
        ''' </summary>
        Friend Function CountHorizontal(cand As Integer(), level As Integer) As Integer
            Dim n As Integer = If(level > 0 AndAlso level <= _maxLen, _geCount(level), 0)
            Dim cnt As Integer = 0

            For t As Integer = 0 To n - 1
                If IsSubsetOf(cand, _tx(t)) Then
                    cnt += 1
                End If
            Next

            Return cnt
        End Function

        ''' <summary>
        ''' 小数组的插入排序（对事务这种十几到几十个元素的小数组比 Array.Sort 更快）
        ''' </summary>
        Private Shared Sub InsertionSort(a As Integer(), n As Integer)
            For i As Integer = 1 To n - 1
                Dim v As Integer = a(i)
                Dim j As Integer = i - 1

                While j >= 0 AndAlso a(j) > v
                    a(j + 1) = a(j)
                    j -= 1
                End While

                a(j + 1) = v
            Next
        End Sub

        ''' <summary>
        ''' 两个升序数组的包含判定，复杂度 O(|cand| + |trans|) 且可提前退出。
        ''' </summary>
        ''' <remarks>
        ''' 原实现是 O(|cand| * |trans|) 的线性 Contains 嵌套。
        ''' </remarks>
        Private Shared Function IsSubsetOf(cand As Integer(), trans As Integer()) As Boolean
            Dim i As Integer = 0
            Dim j As Integer = 0

            While i < cand.Length AndAlso j < trans.Length
                If trans(j) < cand(i) Then
                    j += 1
                ElseIf trans(j) > cand(i) Then
                    Return False
                Else
                    i += 1
                    j += 1
                End If
            End While

            Return i = cand.Length
        End Function
    End Class
End Namespace
