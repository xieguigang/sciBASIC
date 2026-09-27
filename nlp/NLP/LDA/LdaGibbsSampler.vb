#Region "Microsoft.VisualBasic::3fd2267064f79504ca8ad25d0fa583a1, nlp\NLP\LDA\LdaGibbsSampler.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful, but
    ' WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 394
    '    Code Lines: 173 (43.91%)
    ' Comment Lines: 168 (42.64%)
    '    - Xml Docs: 79.76%
    ' 
    '   Blank Lines: 53 (13.45%)
    '     File Size: 15.20 KB


    '     Class LdaGibbsSampler
    ' 
    '         Properties: K, Phi, Theta
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: configure
    ' 
    '         Sub: (+2 Overloads) gibbs, initialState, sampling, update_params
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Concurrent
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Parallel

'  (C) Copyright 2005, Gregor Heinrich (gregor :: arbylon : net) (This file is
'  part of the org.knowceans experimental software packages.)
' 
'  LdaGibbsSampler is free software; you can redistribute it and/or modify it
'  under the terms of the GNU General Public License as published by the Free
'  Software Foundation; either version 2 of the License, or (at your option) any
'  later version.
' 
'  LdaGibbsSampler is distributed in the hope that it will be useful, but
'  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
'  FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
'  details.
' 
'  You should have received a copy of the GNU General Public License along with
'  this program; if not, write to the Free Software Foundation, Inc., 59 Temple
'  Place, Suite 330, Boston, MA 02111-1307 USA
' 
'  Created on Mar 6, 2005
' 
Namespace LDA

    ''' <summary>
    ''' Gibbs sampler for estimating the best assignments of topics for words and
    ''' documents in a corpus. The algorithm is introduced in Tom Griffiths' paper
    ''' "Gibbs sampling in the generative model of Latent Dirichlet Allocation"
    ''' (2002).
    ''' Gibbs sampler采样算法的实现
    ''' 
    ''' @author heinrich
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/hankcs/LDA4j
    ''' 
    ''' ### 并行化实现说明
    ''' 
    ''' collapsed gibbs sampling 是一条马尔可夫链，对第 i 个词语的采样依赖于
    ''' 除它以外所有词语的当前状态，所以严格的并行化在数学上是不可能的。这里
    ''' 采用的是与 PLDA / LightLDA / MALLET 相同的近似并行模型：
    ''' 
    ''' 1. 语料在训练开始之前被一次性划分为若干个连续文档块，之后所有迭代都
    '''    复用该划分，每个工作线程只处理自己块内的文档；
    ''' 2. 文档不跨块，于是文档独占的计数器 nd 完全不需要同步；
    ''' 3. 全局共享的 nw / nwsum 通过 System.Threading.Interlocked 原子更新，
    '''    计数不再丢失，也不再需要任何钳位修补；
    ''' 
    ''' 结果与串行训练在统计意义上是一致的（主题质量相同），但逐位并不严格
    ''' 相等。将 <see cref="Workers"/> 设置为 1 即可退回串行模式。
    ''' </remarks>
    Public Class LdaGibbsSampler

        ''' <summary>
        ''' document data (term lists)
        ''' 文档
        ''' </summary>  
        Friend documents As Integer()()

        ''' <summary>
        ''' 全部文档拼接之后的一维向量（CSR 布局），长度等于语料的词语总数
        ''' </summary>
        Friend docs As Integer()

        ''' <summary>
        ''' 文档 m 在 <see cref="docs"/> 之中的起始偏移，长度为 M + 1
        ''' </summary>
        Friend docOff As Integer()

        ''' <summary>
        ''' 文档 m 的词语数量。该值在整个采样过程中是常量，所以它取代了旧的
        ''' ndsum 计数器向量。
        ''' </summary>
        Friend docLen As Integer()

        ''' <summary>
        ''' vocabulary size
        ''' 词表大小
        ''' </summary> 
        Friend V As Integer

        ''' <summary>
        ''' number of topics
        ''' 主题数目
        ''' </summary> 
        Public ReadOnly Property K As Integer

        ''' <summary>
        ''' Dirichlet parameter (document--topic associations)
        ''' 文档——主题参数
        ''' </summary> 
        Friend alpha As Double = 2.0

        ''' <summary>
        ''' Dirichlet parameter (topic--term associations)
        ''' 主题——词语参数
        ''' </summary>
        Friend beta As Double = 0.5

        ''' <summary>
        ''' 每一个词语的主题分配，与 <see cref="docs"/> 等长对齐
        ''' </summary>
        Friend z As Integer()

        ''' <summary>
        ''' nw[w * K + k]：词语 w 归入主题 k 的次数
        ''' </summary>
        Friend nw As Integer()

        ''' <summary>
        ''' nd[m * K + k]：文档 m 之中归入主题 k 的词语的个数
        ''' </summary>
        Friend nd As Integer()

        ''' <summary>
        ''' nwsum[k * CACHE_LINE_INTS]：归入主题 k 的词语总数。
        ''' 每个主题都被填充到独占一条 cache line，以避免多线程下的 false sharing。
        ''' </summary>
        Friend nwsumP As Integer()

        ''' <summary>
        ''' cumulative statistics of theta
        ''' theta的累积量
        ''' </summary>
        Friend thetasum As Double()()

        ''' <summary>
        ''' cumulative statistics of phi
        ''' phi的累积量
        ''' </summary>
        Friend phisum As Double()()

        ''' <summary>
        ''' size of statistics
        ''' 样本容量
        ''' </summary>
        Friend numstats As Integer

        ''' <summary>
        ''' sampling lag (?)
        ''' 多久更新一次统计量
        ''' </summary> 
        Friend Shared THIN_INTERVAL As Integer = 20

        ''' <summary>
        ''' burn-in period
        ''' 收敛前的迭代次数
        ''' </summary>
        Private Shared BURN_IN As Integer = 100

        ''' <summary>
        ''' max iterations
        ''' 最大迭代次数
        ''' </summary>
        Friend Shared ITERATIONS As Integer = 1000

        ''' <summary>
        ''' sample lag (if -1 only one sample taken)
        ''' 最后的模型个数（取收敛后的n个迭代的参数做平均可以使得模型质量更高）
        ''' </summary>
        Private Shared SAMPLE_LAG As Integer = 10

        ''' <summary>
        ''' 实际参与本轮采样的工作线程数量
        ''' </summary>
        Friend sweepWorkers As Integer = 1

        ''' <summary>
        ''' 本次采样所使用的随机数种子
        ''' </summary>
        Friend seedValue As Integer = -1

        ReadOnly println As Action(Of Object)
        ReadOnly counter As New ApplicationServices.PerformanceCounter

        ''' <summary>
        ''' 并行采样所使用的工作线程数量。
        ''' </summary>
        ''' <returns>
        ''' 小于或者等于 0 表示沿用 <see cref="VectorTask.n_threads"/> 的全局设置；
        ''' 设置为 1 时采样器以串行模式运行。
        ''' </returns>
        Public Property Workers As Integer = 0

        ''' <summary>
        ''' 两次全局同步之间所运行的采样轮数。
        ''' </summary>
        ''' <returns>
        ''' 1 表示每一轮采样之后都与其它工作线程同步一次（最精确）。增大这个值可以
        ''' 减少 barrier 同步与私有行缓存重建的开销，从而进一步提高加速比，代价是
        ''' 每个工作线程看到的全局计数会滞后最多这么多个轮次。
        ''' </returns>
        Public Property SyncInterval As Integer = 1

        ''' <summary>
        ''' 采样所使用的随机数种子。
        ''' </summary>
        ''' <returns>
        ''' 负数表示使用基于时间的随机种子，此时采样结果不可复现；给定一个非负值
        ''' 则可以让采样结果可复现，便于做性能与一致性的基准测试。
        ''' </returns>
        Public Property RandomSeed As Integer = -1

        ''' <summary>
        ''' Retrieve estimated document--topic associations. If sample lag > 0 then
        ''' the mean value of all sampled statistics for theta[][] is taken.
        ''' 获取文档——主题矩阵
        ''' </summary>
        ''' <returns> theta multinomial mixture of document topics (M x K) </returns>
        Public Overridable ReadOnly Property Theta As Double()()
            Get
                Dim nDocs As Integer = documents.Length
                Dim nTopics As Integer = Me.K
                Dim lTheta = RectangularArray.Matrix(Of Double)(nDocs, nTopics)

                If SAMPLE_LAG > 0 Then
                    Call ForEachRow(nDocs, Sub(m As Integer)
                                               Dim sum As Double() = thetasum(m)
                                               Dim row As Double() = lTheta(m)

                                               For k As Integer = 0 To nTopics - 1
                                                   row(k) = sum(k) / numstats
                                               Next
                                           End Sub)
                Else
                    Dim alphaPrior As Double = Me.alpha
                    Dim nd As Integer() = Me.nd
                    Dim docLen As Integer() = Me.docLen

                    Call ForEachRow(nDocs, Sub(m As Integer)
                                               Dim denom As Double = docLen(m) + nTopics * alphaPrior
                                               Dim baseN As Integer = m * nTopics
                                               Dim row As Double() = lTheta(m)

                                               For k As Integer = 0 To nTopics - 1
                                                   row(k) = (nd(baseN + k) + alphaPrior) / denom
                                               Next
                                           End Sub)
                End If

                Return lTheta
            End Get
        End Property

        ''' <summary>
        ''' Retrieve estimated topic--word associations. If sample lag > 0 then the
        ''' mean value of all sampled statistics for phi[][] is taken.
        ''' 获取主题——词语矩阵
        ''' </summary>
        ''' <returns> phi multinomial mixture of topic words (K x V) </returns>
        Public Overridable ReadOnly Property Phi As Double()()
            Get
                Dim nTopics As Integer = Me.K
                Dim vSize As Integer = Me.V
                Dim lPhi = RectangularArray.Matrix(Of Double)(nTopics, vSize)

                If SAMPLE_LAG > 0 Then
                    Call ForEachRow(nTopics, Sub(k As Integer)
                                                 Dim sum As Double() = phisum(k)
                                                 Dim row As Double() = lPhi(k)

                                                 For w As Integer = 0 To vSize - 1
                                                     row(w) = sum(w) / numstats
                                                 Next
                                             End Sub)
                Else
                    Dim betaPrior As Double = Me.beta
                    Dim nw As Integer() = Me.nw
                    Dim nwsumP As Integer() = Me.nwsumP
                    Dim stride As Integer = SamplingState.CACHE_LINE_INTS

                    Call ForEachRow(nTopics, Sub(k As Integer)
                                                 Dim denom As Double = nwsumP(k * stride) + vSize * betaPrior
                                                 Dim row As Double() = lPhi(k)

                                                 For w As Integer = 0 To vSize - 1
                                                     row(w) = (nw(w * nTopics + k) + betaPrior) / denom
                                                 Next
                                             End Sub)
                End If

                Return lPhi
            End Get
        End Property

        ''' <summary>
        ''' 归入各个主题的词语总数
        ''' </summary>
        ''' <returns>
        ''' 该值恒等于语料的词语总数。并行采样一旦丢失了计数更新，这个值就会小于
        ''' 语料的词语总数，所以它可以作为并行实现的一项自检指标。
        ''' </returns>
        Public ReadOnly Property TopicTokenTotal As Long
            Get
                If nwsumP Is Nothing Then
                    Return 0
                End If

                ' note: do NOT name the loop variable "k" here, VB is case 
                ' insensitive so that it would shadow the K property and the 
                ' loop would end up as "For k = 0 To k - 1"
                Dim stride As Integer = SamplingState.CACHE_LINE_INTS
                Dim acc As Long = 0L

                For topic As Integer = 0 To K - 1
                    acc += nwsumP(topic * stride)
                Next

                Return acc
            End Get
        End Property

        ''' <summary>
        ''' 分配一个长度为 n 的整型向量，n 为 0 时返回空向量而不是抛出异常
        ''' </summary>
        Private Shared Function NewVector(n As Integer) As Integer()
            Return If(n > 0, New Integer(n - 1) {}, New Integer() {})
        End Function

        ''' <summary>
        ''' 对矩阵的每一行执行给定的操作，行数较多时自动并行化
        ''' </summary>
        Private Sub ForEachRow(rows As Integer, action As Action(Of Integer))
            If rows <= 0 Then
                Return
            End If

            If sweepWorkers > 1 AndAlso rows > 1 Then
                Call System.Threading.Tasks.Parallel.For(0, rows, action)
            Else
                For i As Integer = 0 To rows - 1
                    Call action(i)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Initialise the Gibbs sampler with data.
        ''' 用数据初始化采样器
        ''' </summary>
        ''' <param name="documents"> 文档 </param>
        ''' <param name="V">vocabulary size 词表大小 </param> 
        Public Sub New(documents As Integer()(), V As Integer, Optional log As Action(Of Object) = Nothing)
            Me.documents = documents
            Me.V = V
            Me.println = log

            If log Is Nothing Then
                Me.println = AddressOf VBDebugger.EchoLine
            End If
        End Sub

        ''' <summary>
        ''' Initialisation: Must start with an assignment of observations to topics ?
        ''' Many alternatives are possible, I chose to perform random assignments
        ''' with equal probabilities
        ''' 随机初始化状态
        ''' </summary>
        ''' <param name="K"> number of topics K个主题 </param>
        Private Sub initialState(K As Integer)
            Dim nDocs As Integer = documents.Length
            Dim t0 = Now

            Call println("allocating memory...")

            ' 将锯齿数组形式的语料扁平化为 CSR 布局：消除 M + 1 / V + 1 个
            ' 数组对象的分配与双重指针解引用，提升缓存命中率
            docOff = New Integer(nDocs) {}
            docLen = NewVector(nDocs)

            Dim total As Integer = 0

            For m As Integer = 0 To nDocs - 1
                docOff(m) = total
                docLen(m) = If(documents(m) Is Nothing, 0, documents(m).Length)
                total += docLen(m)
            Next

            docOff(nDocs) = total

            docs = NewVector(total)
            z = NewVector(total)

            For m As Integer = 0 To nDocs - 1
                If docLen(m) > 0 Then
                    Call Array.Copy(documents(m), Scan0, docs, docOff(m), docLen(m))
                End If
            Next

            ' initialise count variables. 初始化计数器
            nw = NewVector(CInt(CLng(V) * K))
            nd = NewVector(CInt(CLng(nDocs) * K))
            nwsumP = NewVector(K * SamplingState.CACHE_LINE_INTS)

            ' The z_i are are initialised to values in [1,K] to
            ' determine the initial state of the Markov chain.
            ' z_i := 1到K之间的值，表示马氏链的初始状态
            Dim rng As LdaRng = LdaRng.CreateWorkers(seedValue, 1)(Scan0)
            Dim stride As Integer = SamplingState.CACHE_LINE_INTS
            Dim topic As Integer

            For m As Integer = 0 To nDocs - 1
                Dim lN As Integer = docLen(m)
                Dim off As Integer = docOff(m)
                Dim ndBase As Integer = m * K

                For n As Integer = 0 To lN - 1
                    topic = rng.NextInteger(K)

                    ' number of instances of word i assigned to topic j
                    nw(docs(off + n) * K + topic) += 1
                    ' number of words in document i assigned to topic j.
                    nd(ndBase + topic) += 1
                    ' total number of words assigned to topic j.
                    nwsumP(topic * stride) += 1

                    z(off + n) = topic
                Next
            Next

            Call println($"initial_markov_state[{(Now - t0).FormatTime}]")
        End Sub

        ''' <summary>
        ''' with default cutoff alpha=2.0 and beta=0.5
        ''' </summary>
        ''' <param name="K"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub gibbs(K As Integer)
            Call gibbs(K, 2.0, 0.5)
        End Sub

        ''' <summary>
        ''' Main method: Select initial state ? Repeat a large number of times: 1.
        ''' Select an element 2. Update conditional on other elements. If
        ''' appropriate, output summary for each run.
        ''' 采样
        ''' </summary>
        ''' <param name="K">     number of topics 主题数 </param>
        ''' <param name="alpha"> symmetric prior parameter on document--topic associations 对称文档——主题先验概率？ </param>
        ''' <param name="beta">  symmetric prior parameter on topic--term associations 对称主题——词语先验概率？ </param> 
        Public Sub gibbs(K As Integer, alpha As Double, beta As Double)
            Me._K = K
            Me.alpha = alpha
            Me.beta = beta
            Me.seedValue = If(RandomSeed < 0, CInt(Date.Now.Ticks Mod Integer.MaxValue), RandomSeed)

            ' init sampler statistics  分配内存
            If SAMPLE_LAG > 0 Then
                thetasum = RectangularArray.Matrix(Of Double)(documents.Length, K)
                phisum = RectangularArray.Matrix(Of Double)(K, V)
                numstats = 0
            End If

            ' initial state of the Markov chain:
            Call initialState(K)

            Dim nDocs As Integer = documents.Length
            Dim nThreads As Integer = Me.Workers

            If nThreads <= 0 Then
                nThreads = VectorTask.n_threads
            End If
            If nThreads < 1 Then
                nThreads = 1
            End If

            ' 文档不跨块，所以块数最多等于文档数
            Dim maxWorkers As Integer = If(nDocs > 0, nDocs, 1)

            Me.sweepWorkers = If(nThreads < maxWorkers, nThreads, maxWorkers)

            ' 分区只计算一次，之后所有迭代复用该划分。旧实现在迭代内部为每篇
            ' 文档启动一次并行调度，调度次数为 M * ITERATIONS，这是其性能不
            ' 理想的首要原因。
            Dim blocks As DocBlock() = LdaPartition.Build(docLen, sweepWorkers)
            Dim rng As LdaRng() = LdaRng.CreateWorkers(seedValue, blocks.Length)
            Dim states As SamplingState() = New SamplingState(blocks.Length - 1) {}
            Dim parallelMode As Boolean = blocks.Length > 1

            For b As Integer = 0 To blocks.Length - 1
                states(b) = New SamplingState(K, alpha, beta, V, nw, nwsumP, nd, docs, docOff, docLen, z,
                                              useRowCache:=parallelMode)
            Next

            Call println($"* Sampling {ITERATIONS} iterations with burn-in of {BURN_IN} unique temp var.")
            Call println($"* gibbs run in {If(Not parallelMode, "sequential mode", $"parallel with {blocks.Length} CPU threads")}!")
            Call println($"* corpus: {nDocs} documents, {docs.Length} tokens, vocabulary={V}, topics={K}")
            Call VBDebugger.WaitOutput()

            Call RunSweeps(blocks, states, rng, parallelMode)
        End Sub

        ''' <summary>
        ''' 运行 <see cref="ITERATIONS"/> 轮采样迭代
        ''' </summary>
        ''' <remarks>
        ''' 并行模式下这里使用的是一组常驻工作线程加上一个 <see cref="Barrier"/>，
        ''' 而不是每一轮迭代都调用一次 Parallel.For：每轮迭代的工作量在多线程
        ''' 之下只有百微秒量级，而一次 Parallel.For 的派发与汇合开销与之相当，
        ''' 用常驻线程可以把这部分开销压到一次 barrier 同步的成本上。
        ''' </remarks>
        Private Sub RunSweeps(blocks As DocBlock(), states As SamplingState(), rng As LdaRng(), parallelMode As Boolean)
            ' z is initialized after initialState is called
            Dim t0 As Date = Now
            Dim t1 As Date = Now
            Dim bar As Tqdm.ProgressBar = Nothing

            If Not parallelMode Then
                For Each i As Integer In Tqdm.Range(0, ITERATIONS, bar:=bar, wrap_console:=App.EnableTqdm)
                    Call counter.Set()

                    For bi As Integer = 0 To blocks.Length - 1
                        Call SampleBlock(blocks(bi), states(bi), rng(bi))
                    Next

                    Call AfterSweep(i, t0, bar)
                Next

                Return
            End If

            Dim errors As New ConcurrentQueue(Of Exception)
            Dim iterDone As Integer = -1
            Dim interval As Integer = Me.SyncInterval

            If interval < 1 Then
                interval = 1
            End If

            ' 每 interval 轮采样才同步一次：barrier 与私有行缓存重建的开销被摊薄，
            ' 代价是各线程看到的全局计数最多滞后 interval 轮
            Dim rounds As Integer = (ITERATIONS + interval - 1) \ interval

            ' the post phase action runs after every worker has finished the 
            ' current sweep and before any of them is released, so that the 
            ' statistics can be collected without racing with the next sweep.
            Dim sync As New Barrier(blocks.Length + 1, Sub(b As Barrier)
                                                           iterDone += 1
                                                           Call AfterSweep(System.Math.Min((iterDone + 1) * interval, ITERATIONS) - 1, t0, bar)
                                                       End Sub)
            Dim threads As Thread() = New Thread(blocks.Length - 1) {}

            For b As Integer = 0 To blocks.Length - 1
                Dim bi As Integer = b

                threads(bi) = New Thread(Sub()
                                             Dim done As Integer = 0

                                             Try
                                                 For round As Integer = 0 To rounds - 1
                                                     Dim sweeps As Integer = interval

                                                     If round = rounds - 1 Then
                                                         sweeps = ITERATIONS - round * interval
                                                     End If

                                                     Call SampleBlock(blocks(bi), states(bi), rng(bi), sweeps)
                                                     Call sync.SignalAndWait()
                                                     done += 1
                                                 Next
                                             Catch ex As Exception
                                                 Call errors.Enqueue(ex)

                                                 ' keep the barrier balanced, otherwise 
                                                 ' the caller would dead lock
                                                 While done < rounds
                                                     Try
                                                         Call sync.SignalAndWait()
                                                     Catch ignore As Exception
                                                         Exit While
                                                     End Try

                                                     done += 1
                                                 End While
                                             End Try
                                         End Sub)

                threads(bi).IsBackground = True
                threads(bi).Name = $"lda-gibbs-{bi}"

                Call threads(bi).Start()
            Next

            For Each round As Integer In Tqdm.Range(0, rounds, bar:=bar, wrap_console:=App.EnableTqdm)
                Call sync.SignalAndWait()
            Next

            For Each t As Thread In threads
                Call t.Join()
            Next

            sync.Dispose()

            Dim failure As Exception = Nothing

            If errors.TryPeek(failure) Then
                Throw failure
            End If
        End Sub

        ''' <summary>
        ''' 一轮采样迭代结束之后的统计与进度更新
        ''' </summary>
        Private Sub AfterSweep(i As Integer, t0 As Date, bar As Tqdm.ProgressBar)
            Call counter.Mark("sampling")

            ' the progress bar is only created when wrap_console is TRUE,
            ' so that it can be Nothing here.
            If bar IsNot Nothing Then
                Dim t1 As Date = Now

                If i < BURN_IN Then
                    If i Mod THIN_INTERVAL = 0 Then
                        Call bar.SetLabel($"BURN_IN ... {StringFormats.ReadableElapsedTime((t1 - t0).TotalMilliseconds)}")
                    End If
                Else
                    If i Mod THIN_INTERVAL = 0 Then
                        Call bar.SetLabel($" ... {StringFormats.ReadableElapsedTime((t1 - t0).TotalMilliseconds)}")
                    End If
                End If
            End If

            ' get statistics after burn-in
            If i > BURN_IN AndAlso SAMPLE_LAG > 0 AndAlso i Mod SAMPLE_LAG = 0 Then
                Call counter.Mark("...")
                Call update_params()
                Call counter.Mark("update_pars")

                If bar IsNot Nothing Then
                    Dim t1 As Date = Now

                    Call bar.SetLabel($"get statistics after burn-in! {StringFormats.ReadableElapsedTime((t1 - t0).TotalMilliseconds)}")
                End If
            End If
        End Sub

        ''' <summary>
        ''' Add to the statistics the values of theta and phi for the current state.
        ''' 更新参数
        ''' </summary>
        Private Sub update_params()
            Dim nDocs As Integer = documents.Length
            Dim nTopics As Integer = Me.K
            Dim vSize As Integer = Me.V
            Dim alphaPrior As Double = Me.alpha
            Dim betaPrior As Double = Me.beta
            Dim stride As Integer = SamplingState.CACHE_LINE_INTS
            Dim nd As Integer() = Me.nd
            Dim nw As Integer() = Me.nw
            Dim nwsumP As Integer() = Me.nwsumP
            Dim docLen As Integer() = Me.docLen
            Dim thetasum As Double()() = Me.thetasum
            Dim phisum As Double()() = Me.phisum

            ' 行与行之间写入互不重叠，可以直接并行
            Call ForEachRow(nDocs, Sub(m As Integer)
                                       Dim denom As Double = docLen(m) + nTopics * alphaPrior
                                       Dim baseN As Integer = m * nTopics
                                       Dim sum As Double() = thetasum(m)

                                       For k As Integer = 0 To nTopics - 1
                                           sum(k) += (nd(baseN + k) + alphaPrior) / denom
                                       Next
                                   End Sub)

            Call ForEachRow(nTopics, Sub(k As Integer)
                                         Dim denom As Double = nwsumP(k * stride) + vSize * betaPrior
                                         Dim sum As Double() = phisum(k)

                                         For w As Integer = 0 To vSize - 1
                                             sum(w) += (nw(w * nTopics + k) + betaPrior) / denom
                                         Next
                                     End Sub)

            numstats += 1
        End Sub

        ''' <summary>
        ''' Configure the gibbs sampler
        ''' 配置采样器
        ''' </summary>
        ''' <param name="iterations">   number of total iterations </param>
        ''' <param name="burnIn">       number of burn-in iterations </param>
        ''' <param name="thinInterval"> update statistics interval </param>
        ''' <param name="sampleLag">    sample interval (-1 for just one sample at the end) </param>
        ''' <param name="workers">      the number of the worker threads of the parallel 
        '''                             sampling, zero or negative for use the global 
        '''                             <see cref="VectorTask.n_threads"/> setting, 
        '''                             one for run in sequential mode. </param>
        ''' <param name="seed">         the random seed of the sampler, a negative value 
        '''                             for use a time based random seed. </param>
        Public Function configure(iterations As Integer, burnIn As Integer, thinInterval As Integer, sampleLag As Integer,
                                  Optional workers As Integer = 0,
                                  Optional seed As Integer = -1,
                                  Optional syncInterval As Integer = 1) As LdaGibbsSampler

            LdaGibbsSampler.ITERATIONS = iterations
            BURN_IN = burnIn
            THIN_INTERVAL = thinInterval
            SAMPLE_LAG = sampleLag

            Me.Workers = workers
            Me.RandomSeed = seed
            Me.SyncInterval = syncInterval

            Return Me
        End Function
    End Class
End Namespace
