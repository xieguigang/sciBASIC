Imports System.Threading

Namespace LDA

    ''' <summary>
    ''' The shared state of the gibbs sampling kernel.
    ''' </summary>
    ''' <remarks>
    ''' All of the worker threads share the same count vectors, but each of them 
    ''' owns one instance of this state object, so that the scratch buffers are 
    ''' thread local and are reused by every token.
    ''' 
    ''' The ownership of the count vectors is:
    ''' 
    ''' + ``nd`` (M * K) is owned by the worker thread that owns the document, 
    '''   a document is never split across two blocks, so that no synchronization 
    '''   is required here.
    ''' + ``ndsum`` does not exist at all: the very same topic is removed and 
    '''   then added back for one token, so the token count of a document is a 
    '''   constant during the whole sampling.
    ''' + ``nw`` (V * K) and ``nwsum`` are global. They are read during the sweep 
    '''   and they are only written at the end of the sweep, through the private 
    '''   delta buffers "rowDelta" and <see cref="nwsumDelta"/>.
    ''' 
    ''' Reading a cache line that nobody writes is cheap on every core, whilst 
    ''' writing it makes the line bounce between the cores. This is why the 
    ''' global counters must never be written from inside the sweep: once the 
    ''' model starts to converge, the writes concentrate on a few hot (word, topic) 
    ''' pairs and the bouncing makes the parallel sampler *slower* than the 
    ''' sequential one.
    ''' </remarks>
    Friend NotInheritable Class SamplingState

        ''' <summary>
        ''' the padding width of the <see cref="nwsumP"/> vector, given in the 
        ''' number of the Int32 elements: 16 * 4 bytes = 64 bytes = one cache 
        ''' line on x86 and x64.
        ''' </summary>
        Friend Const CACHE_LINE_INTS As Integer = 16

        ''' <summary>
        ''' the initial capacity of the word slot buffer, given in the number of 
        ''' the words
        ''' </summary>
        Private Const INIT_SLOTS As Integer = 256

        Friend ReadOnly K As Integer
        Friend ReadOnly alpha As Double
        Friend ReadOnly beta As Double
        Friend ReadOnly Vbeta As Double

        ''' <summary>
        ''' number of instances of the word w assigned to the topic k, indexed 
        ''' by ``w * K + k``. Shared by all of the worker threads, only written 
        ''' at the end of a sweep.
        ''' </summary>
        Friend ReadOnly nw As Integer()

        ''' <summary>
        ''' total number of the words assigned to the topic k, indexed by 
        ''' ``k * CACHE_LINE_INTS``. Shared by all of the worker threads, only 
        ''' written at the end of a sweep.
        ''' </summary>
        Friend ReadOnly nwsumP As Integer()

        ''' <summary>
        ''' number of the words of the document m assigned to the topic k, 
        ''' indexed by ``m * K + k``. Exclusive to the owner worker thread.
        ''' </summary>
        Friend ReadOnly nd As Integer()

        ''' <summary>
        ''' all of the documents of the corpus, concatenated into one single vector
        ''' </summary>
        Friend ReadOnly docs As Integer()

        ''' <summary>
        ''' the offset of the document m inside <see cref="docs"/>, the vector 
        ''' has M + 1 elements so that ``docOff(m + 1) - docOff(m)`` is the 
        ''' token count of the document m.
        ''' </summary>
        Friend ReadOnly docOff As Integer()

        ''' <summary>
        ''' the token count of the document m
        ''' </summary>
        Friend ReadOnly docLen As Integer()

        ''' <summary>
        ''' the topic assignment of every token, aligned with <see cref="docs"/>
        ''' </summary>
        Friend ReadOnly z As Integer()

        ''' <summary>
        ''' a scratch buffer of the topic probability, one buffer per worker thread
        ''' </summary>
        Friend ReadOnly pBuf As Double()

        ''' <summary>
        ''' 每个工作线程私有的主题计数视图，其值为 ``nwsum(k) + V * beta``
        ''' </summary>
        Friend ReadOnly nwsumLocal As Double()

        ''' <summary>
        ''' 本轮 sweep 之中当前线程对全局主题计数的累计增量
        ''' </summary>
        Friend ReadOnly nwsumDelta As Integer()

        ''' <summary>
        ''' 词 w 在本轮 sweep 之中所占用的增量槽位（槽位下标 + 1），0 表示尚未分配。
        ''' 长度为 V，每个工作线程一份。
        ''' </summary>
        Friend ReadOnly rowSlot As Integer()

        ''' <summary>
        ''' 槽位下标到词 id 的映射
        ''' </summary>
        Friend slotWord As Integer()

        ''' <summary>
        ''' 词槽位分配时从全局 ``nw`` 拷贝而来的私有快照，索引为 ``slot * K + k``。
        ''' 用于在 sweep 结束时计算净增量。
        ''' </summary>
        Friend rowSnap As Integer()

        ''' <summary>
        ''' 本轮 sweep 之中当前线程私有的 ``nw`` 行副本，索引为 ``slot * K + k``。
        ''' 采样内层循环只读这个副本，因此不会触碰任何被其它线程写入的 cache line。
        ''' </summary>
        Friend rowCur As Integer()

        ''' <summary>
        ''' 已经分配出去的槽位数量
        ''' </summary>
        Friend slotCount As Integer

        ''' <summary>
        ''' 当前 <see cref="slotWord"/> / <see cref="rowSnap"/> / <see cref="rowCur"/> 
        ''' 的容量（以词为单位）
        ''' </summary>
        Friend slotCapacity As Integer

        ''' <summary>
        ''' 是否启用私有行缓存。串行模式下没有其它线程会写 nw，此时绕过缓存可以
        ''' 省掉每轮 sweep 重建缓存的开销。
        ''' </summary>
        Friend ReadOnly useRowCache As Boolean

        Friend Sub New(K As Integer, alpha As Double, beta As Double, V As Integer,
                       nw As Integer(), nwsumP As Integer(), nd As Integer(),
                       docs As Integer(), docOff As Integer(), docLen As Integer(),
                       z As Integer(), useRowCache As Boolean)

            Me.K = K
            Me.useRowCache = useRowCache
            Me.alpha = alpha
            Me.beta = beta
            Me.Vbeta = V * beta
            Me.nw = nw
            Me.nwsumP = nwsumP
            Me.nd = nd
            Me.docs = docs
            Me.docOff = docOff
            Me.docLen = docLen
            Me.z = z
            Me.pBuf = If(K > 0, New Double(K - 1) {}, New Double() {})
            Me.nwsumLocal = If(K > 0, New Double(K - 1) {}, New Double() {})
            Me.nwsumDelta = If(K > 0, New Integer(K - 1) {}, New Integer() {})
            Me.rowSlot = If(useRowCache AndAlso V > 0, New Integer(V - 1) {}, New Integer() {})
            Me.slotCapacity = If(useRowCache AndAlso V > 0, System.Math.Min(V, INIT_SLOTS), 0)
            Me.slotWord = If(slotCapacity > 0, New Integer(slotCapacity - 1) {}, New Integer() {})
            Me.rowSnap = If(slotCapacity > 0, New Integer(slotCapacity * K - 1) {}, New Integer() {})
            Me.rowCur = If(slotCapacity > 0, New Integer(slotCapacity * K - 1) {}, New Integer() {})
            Me.slotCount = 0
        End Sub

        ''' <summary>
        ''' 为词 w 分配（或者取得已有的）增量槽位
        ''' </summary>
        ''' <returns>槽位下标，该值乘以 K 即为 "rowDelta" 的起始下标</returns>
        Friend Function GetSlot(w As Integer) As Integer
            Dim slot As Integer = rowSlot(w) - 1

            If slot >= 0 Then
                Return slot
            End If

            slot = slotCount

            If slot >= slotCapacity Then
                ' grow the slot buffers
                Dim capacity As Integer = If(slotCapacity > 0, slotCapacity * 2, INIT_SLOTS)
                Dim words As Integer() = New Integer(capacity - 1) {}
                Dim snap As Integer() = New Integer(capacity * K - 1) {}
                Dim cur As Integer() = New Integer(capacity * K - 1) {}
                Dim used As Integer = slotCount * K

                Call Array.Copy(slotWord, Scan0, words, Scan0, slotCount)
                Call Array.Copy(rowSnap, Scan0, snap, Scan0, used)
                Call Array.Copy(rowCur, Scan0, cur, Scan0, used)

                slotWord = words
                rowSnap = snap
                rowCur = cur
                slotCapacity = capacity
            End If

            ' take a private snapshot of the global nw row of this word; the slot 
            ' is recycled between two sweeps, so the row has to be re-read here
            Call Array.Copy(nw, w * K, rowSnap, slot * K, K)
            Call Array.Copy(nw, w * K, rowCur, slot * K, K)

            slotWord(slot) = w
            rowSlot(w) = slot + 1
            slotCount = slot + 1

            Return slot
        End Function

        ''' <summary>
        ''' 释放本轮 sweep 分配出去的全部槽位
        ''' </summary>
        Friend Sub ResetSlots()
            For s As Integer = 0 To slotCount - 1
                rowSlot(slotWord(s)) = 0
            Next

            slotCount = 0
        End Sub
    End Class

    ''' <summary>
    ''' The gibbs sampling kernel that runs on the flattened (CSR) corpus layout.
    ''' </summary>
    ''' <remarks>
    ''' Replaces the old <c>GibbsSamplingTask</c>, which was creating a parallel 
    ''' schedule for each single document and was copying the whole ``ndsum`` 
    ''' vector (the length of that vector is the document count of the corpus!) 
    ''' for every single token.
    ''' </remarks>
    Friend Module LdaSamplingKernel

        ''' <summary>
        ''' 在 sweep 开始时刷新本线程私有的计数视图
        ''' </summary>
        Friend Sub BeginSweep(st As SamplingState)
            Dim stride As Integer = SamplingState.CACHE_LINE_INTS
            Dim nTopics As Integer = st.K
            Dim Vbeta As Double = st.Vbeta

            Call st.ResetSlots()

            For k As Integer = 0 To nTopics - 1
                st.nwsumLocal(k) = st.nwsumP(k * stride) + Vbeta
                st.nwsumDelta(k) = 0
            Next
        End Sub

        ''' <summary>
        ''' 在 sweep 结束时将本线程累计的计数增量一次性合并回全局计数
        ''' </summary>
        Friend Sub EndSweep(st As SamplingState)
            Dim nTopics As Integer = st.K
            Dim nw As Integer() = st.nw

            If Not st.useRowCache Then
                ' the kernel has been writing into nw directly
                Call EndSweepNwSum(st)
                Return
            End If

            Dim rowSnap As Integer() = st.rowSnap
            Dim rowCur As Integer() = st.rowCur
            Dim slotWord As Integer() = st.slotWord

            ' 合并主题词计数：只有净增量不为零的 (词, 主题) 对才需要写回，
            ' 同一个词在同一轮 sweep 之内反复来回切换主题时其净增量为零，
            ' 于是这部分原子写操作被自然地消除掉了。
            For s As Integer = 0 To st.slotCount - 1
                Dim w As Integer = slotWord(s)
                Dim baseNw As Integer = w * nTopics
                Dim baseRow As Integer = s * nTopics

                For k As Integer = 0 To nTopics - 1
                    Dim delta As Integer = rowCur(baseRow + k) - rowSnap(baseRow + k)

                    If delta <> 0 Then
                        Call Interlocked.Add(nw(baseNw + k), delta)
                    End If
                Next
            Next

            Call EndSweepNwSum(st)
        End Sub

        ''' <summary>
        ''' 将本线程累计的主题计数增量合并回全局计数
        ''' </summary>
        Private Sub EndSweepNwSum(st As SamplingState)
            Dim stride As Integer = SamplingState.CACHE_LINE_INTS
            Dim nTopics As Integer = st.K

            For k As Integer = 0 To nTopics - 1
                Dim delta As Integer = st.nwsumDelta(k)

                If delta <> 0 Then
                    Call Interlocked.Add(st.nwsumP(k * stride), delta)

                    st.nwsumDelta(k) = 0
                End If
            Next
        End Sub

        ''' <summary>
        ''' Run one gibbs sampling sweep over all of the documents of the block.
        ''' </summary>
        ''' <param name="block">the continuous block of documents that is owned 
        ''' by the current worker thread.</param>
        ''' <param name="st">the shared sampling state.</param>
        ''' <param name="rng">the thread local random generator.</param>
        ''' <param name="sweeps">
        ''' the number of the sweeps that is run before the private state is 
        ''' synchronized with the other worker threads.
        ''' </param>
        Friend Sub SampleBlock(block As DocBlock, st As SamplingState, rng As LdaRng, Optional sweeps As Integer = 1)
            Dim nTopics As Integer = st.K
            Dim alpha As Double = st.alpha
            Dim beta As Double = st.beta
            Dim nwsumLocal As Double() = st.nwsumLocal
            Dim nwsumDelta As Integer() = st.nwsumDelta

            If sweeps < 1 Then
                sweeps = 1
            End If

            Call BeginSweep(st)

            Try
                Dim nd As Integer() = st.nd
                Dim docs As Integer() = st.docs
                Dim docOff As Integer() = st.docOff
                Dim docLen As Integer() = st.docLen
                Dim z As Integer() = st.z
                Dim p As Double() = st.pBuf
                Dim useCache As Boolean = st.useRowCache
                Dim nw As Integer() = st.nw
                Dim k As Integer

                If nTopics <= 0 OrElse docs Is Nothing Then
                    Return
                End If

                For sweep As Integer = 1 To sweeps
                    For m As Integer = block.docStart To block.docEnd
                        Dim len As Integer = docLen(m)

                        If len <= 0 Then
                            ' an empty document has nothing to sample
                            Continue For
                        End If

                        Dim off As Integer = docOff(m)
                        Dim ndBase As Integer = m * nTopics

                        For n As Integer = 0 To len - 1
                            Dim w As Integer = docs(off + n)
                            Dim row As Integer()
                            Dim rowBase As Integer
                            Dim oldTopic As Integer = z(off + n)

                            If useCache Then
                                ' the private copy of the nw row of this word, so that 
                                ' no cache line that is used by another thread is ever 
                                ' written from inside the sweep
                                rowBase = st.GetSlot(w) * nTopics
                                row = st.rowCur
                            Else
                                ' sequential mode: nobody else touches nw
                                rowBase = w * nTopics
                                row = nw
                            End If

                            ' remove z_i from the count variables
                            nd(ndBase + oldTopic) -= 1
                            row(rowBase + oldTopic) -= 1
                            nwsumLocal(oldTopic) -= 1
                            nwsumDelta(oldTopic) -= 1

                            ' sample from p(z_i | z_-i, w):
                            '
                            '   p(k) = (nw(w,k) + beta) / (nwsum(k) + V * beta)
                            '        * (nd(m,k) + alpha) / (ndsum(m) + K * alpha)
                            '
                            ' the factor 1 / (ndsum(m) + K * alpha) does not depend 
                            ' on the topic, so that it cancels out in the comparison 
                            ' of the cumulative distribution below and is skipped 
                            ' here.
                            '
                            ' nwsumLocal already contains the V * beta offset and it 
                            ' is private to this thread, so that reading it here 
                            ' never touches a cache line that is owned by another 
                            ' thread.
                            Dim total As Double = 0.0R

                            For k = 0 To nTopics - 1
                                Dim q As Double = (row(rowBase + k) + beta) *
                                                  (nd(ndBase + k) + alpha) / nwsumLocal(k)

                                p(k) = q
                                total += q
                            Next

                            ' do the multinomial sampling via the cumulative method
                            Dim u As Double = rng.NextDouble() * total
                            Dim acc As Double = 0.0R
                            Dim newTopic As Integer = nTopics - 1

                            For k = 0 To nTopics - 1
                                acc += p(k)

                                If u < acc Then
                                    newTopic = k
                                    Exit For
                                End If
                            Next

                            z(off + n) = newTopic

                            ' add the newly estimated z_i to the count variables
                            nd(ndBase + newTopic) += 1
                            row(rowBase + newTopic) += 1
                            nwsumLocal(newTopic) += 1
                            nwsumDelta(newTopic) += 1
                        Next
                    Next
                Next
            Finally
                ' merge the accumulated deltas back, so that the other worker 
                ' threads can see them on the next sweep
                Call EndSweep(st)
            End Try
        End Sub
    End Module
End Namespace
