#Region "Microsoft.VisualBasic::378e5ea80916f734b68fd38a9d3c8290, Data_science\DataMining\t-SNE\tSNE.vb"

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
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 165
    '    Code Lines: 97 (58.79%)
    ' Comment Lines: 41 (24.85%)
    '    - Xml Docs: 51.22%
    ' 
    '   Blank Lines: 27 (16.36%)
    '     File Size: 4.97 KB


    ' Class tSNE
    ' 
    '     Properties: dimension
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: [Step], GetEmbedding
    ' 
    '     Sub: InitDataDist, InitDataRaw, InitSolution
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Public Class tSNE : Inherits IDataEmbedding

    ''' <summary>
    ''' effective number of nearest neighbors
    ''' </summary>
    Friend mPerplexity As Double

    ''' <summary>
    ''' learning rate
    ''' </summary>
    Friend mEpsilon As Double

    Friend mIter As Double

    ''' <summary>
    ''' 联合概率矩阵（精确模式下的稠密 N×N，行主序）
    ''' </summary>
    Friend mP As Double()

    ''' <summary>
    ''' 联合概率矩阵（Barnes-Hut 模式下的稀疏 CSR 表示）
    ''' </summary>
    Friend mSparseP As SparseP

    ''' <summary>
    ''' Y is an array of 2-D points that you can plot
    ''' </summary>
    ''' <remarks>
    ''' 这是嵌入结果的权威存储，<see cref="GetEmbedding"/> 直接返回其引用，
    ''' 因此必须始终保持为锯齿数组形态以维持既有的公开语义。
    ''' 热循环则一律走 <see cref="mYFlat"/> 一维镜像以获得更好的缓存局部性。
    ''' </remarks>
    Friend mY As Double()()

    ''' <summary>
    ''' Y 的一维行主序镜像，索引为 <c>i * mDim + d</c>
    ''' </summary>
    Friend mYFlat As Double()

    ''' <summary>
    ''' step gains to accelerate progress in unchanging directions（行主序一维，长度 N*dim）
    ''' </summary>
    Friend mGains As Double()

    ''' <summary>
    ''' momentum accumulator（行主序一维，长度 N*dim）
    ''' </summary>
    Friend mYStep As Double()

    ''' <summary>
    ''' Barnes-Hut 模式下的斥力累加器
    ''' </summary>
    Friend bhNegF As Double()

    ''' <summary>
    ''' Barnes-Hut 模式下的引力累加器
    ''' </summary>
    Friend bhPosF As Double()

    Friend mN As Integer

    Friend mCost As Double

    ''' <summary>
    ''' 梯度，行主序一维数组，长度为 N*dim
    ''' </summary>
    Friend mGrad As Double()

    ''' <summary>
    ''' dimensionality of the embedding
    ''' </summary>
    Friend ReadOnly mDim As Integer

    Friend ReadOnly random As RandomHelper

    Friend ReadOnly cost As CostFunction

    ''' <summary>
    ''' 并行度封装，随 <see cref="nthreads"/> 一同更新
    ''' </summary>
    Friend opts As System.Threading.Tasks.ParallelOptions

    ''' <summary>
    ''' 实际的并行线程数
    ''' </summary>
    Friend mThreads As Integer

    ''' <summary>
    ''' 并行计算所使用的线程数量
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 默认取当前主机的 CPU 逻辑核心数（<c>App.CPUCoreNumbers</c>）。
    ''' 由于 t-SNE 的 O(N^2) 热循环是内存带宽受限而非算力受限的，
    ''' 实际的加速比会低于线程数，调低此值有时反而更快。
    ''' </remarks>
    Public Property nthreads As Integer
        Get
            Return mThreads
        End Get
        Set(value As Integer)
            mThreads = std.Max(1, value)
            opts.MaxDegreeOfParallelism = mThreads
        End Set
    End Property

    ''' <summary>
    ''' 是否启用 Barnes-Hut 近似
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 默认关闭，此时走与改造前完全一致的精确稠密路径。
    ''' 启用之后概率矩阵改为稀疏表示、梯度改用空间划分树做远场近似，
    ''' 时间与内存复杂度由 O(N^2) 降至 O(N log N) 与 O(N·k)，
    ''' 代价是结果为近似值（由 <see cref="theta"/> 与近邻数控制精度）。
    ''' 
    ''' 注意：必须在调用 <see cref="InitDataRaw"/> 或 <see cref="InitDataDist"/> 之前设置，
    ''' 因为它决定了概率矩阵的构建方式。
    ''' </remarks>
    Public Property UseBarnesHut As Boolean = False

    ''' <summary>
    ''' Barnes-Hut 近似阈值
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 当某个 cell 的角宽度（宽度 / 到质心的距离）小于该阈值时，
    ''' 就用其质心一次性近似整棵子树。取值越大越快越粗糙，取 0 退化为精确计算，
    ''' 参考实现中的经验取值为 0.5。仅当 <see cref="UseBarnesHut"/> 为 True 时生效。
    ''' </remarks>
    Public Property theta As Double = 0.5

    ''' <summary>
    ''' Barnes-Hut 模式下每一行保留的近邻数量
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 取 0（默认值）时自动取 3 * perplexity（参考实现的经验取值）。
    ''' 这个值直接决定稀疏概率矩阵的内存占用 O(N·k)，是精度与开销之间最主要的调节旋钮。
    ''' </remarks>
    Public Property KNN As Integer = 0

    Public Overrides ReadOnly Property dimension As Integer
        Get
            Return mDim
        End Get
    End Property

    ''' <summary>
    ''' 创建一个 t-SNE 降维器，并行度取当前主机的 CPU 核心数
    ''' </summary>
    ''' <param name="perplexity">困惑度</param>
    ''' <param name="[dim">嵌入维度，通常为 2 或 3</param>
    ''' <param name="epsilon">学习率</param>
    Public Sub New(perplexity As Double, [dim] As Integer, epsilon As Double)
        Me.New(perplexity, [dim], epsilon, Global.Microsoft.VisualBasic.App.CPUCoreNumbers)
    End Sub

    ''' <summary>
    ''' 创建一个 t-SNE 降维器
    ''' </summary>
    ''' <param name="perplexity">困惑度</param>
    ''' <param name="[dim">嵌入维度，通常为 2 或 3</param>
    ''' <param name="epsilon">学习率</param>
    ''' <param name="nthreads">并行线程数，&lt;= 0 时取当前主机的 CPU 核心数</param>
    ''' <param name="useBarnesHut">是否启用 Barnes-Hut 近似，默认关闭（走精确路径）</param>
    Public Sub New(perplexity As Double, [dim] As Integer, epsilon As Double,
                   nthreads As Integer,
                   Optional useBarnesHut As Boolean = False)

        mPerplexity = perplexity
        mDim = [dim]
        mEpsilon = epsilon
        mIter = 0
        Me.mThreads = If(nthreads > 0, nthreads, Global.Microsoft.VisualBasic.App.CPUCoreNumbers)
        Me.opts = New System.Threading.Tasks.ParallelOptions With {
            .MaxDegreeOfParallelism = mThreads
        }
        Me.UseBarnesHut = useBarnesHut
        random = New RandomHelper(Me)
        cost = New CostFunction(Me)
    End Sub

    ''' <summary>
    ''' Barnes-Hut 模式下每行实际保留的近邻数量
    ''' </summary>
    ''' <returns></returns>
    Friend Function EffectiveK() As Integer
        If KNN > 0 Then
            Return KNN
        End If

        Return SparseProbability.SuggestK(mPerplexity)
    End Function

    ''' <summary>
    ''' return current solution
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetEmbedding() As Double()()
        Return mY
    End Function

    ''' <summary>
    ''' this function takes a set of high-dimensional points
    ''' and creates matrix P from them using gaussian kernel
    ''' </summary>
    ''' <param name="X"></param>
    Public Sub InitDataRaw(X As IEnumerable(Of Double()))
        Dim data = X.ToArray
        Dim N = data.Length

        If UseBarnesHut Then
            ' 稀疏路径：不物化 N×N 的距离矩阵，也不物化 N×N 的稠密概率矩阵
            mP = Nothing
            mSparseP = d2pSparse(mPerplexity, 0.0001, EffectiveK(), mThreads, X:=data)
        Else
            Dim dists = xtod(data, mThreads)

            mP = d2p(dists, mPerplexity, 0.0001, mThreads)
            mSparseP = Nothing
        End If

        mN = N

        Call InitSolution()
    End Sub

    ' this function takes a given distance matrix and creates
    ' matrix P from them.
    ' D is assumed to be provided as a list of lists, and should be symmetric
    Public Sub InitDataDist(D As Double()())
        Dim N = D.Length

        ' convert D to a (fast) typed array version
        Dim dists = zeros(N * N) ' allocate contiguous array

        System.Threading.Tasks.Parallel.For(0, N, opts,
            Sub(i)
                Dim offset = i * N

                For j As Integer = i + 1 To N - 1
                    Dim lD = D(i)(j)

                    dists(offset + j) = lD
                    dists(j * N + i) = lD
                Next
            End Sub)

        If UseBarnesHut Then
            ' 距离矩阵由调用方提供，无法避免其 N×N 的开销，
            ' 但概率矩阵仍然可以建成稀疏的
            mP = Nothing
            mSparseP = d2pSparse(mPerplexity, 0.0001, EffectiveK(), mThreads, D:=dists)
        Else
            mP = d2p(dists, mPerplexity, 0.0001, mThreads)
            mSparseP = Nothing
        End If

        mN = N

        InitSolution() ' refresh this
    End Sub

    ' (re)initializes the solution to random
    Private Sub InitSolution()
        Dim N As Integer = mN
        Dim D As Integer = mDim

        ' generate random solution to t-SNE
        ' 初始化只占 O(N*dim)，相对 N^2 的热循环可以忽略，
        ' 因此这里保持串行执行，令随机序列可复现
        mYFlat = random.randn2d(N, D) ' the solution
        mGains = RandomHelper.randn2d(N, D, 1.0) ' step gains to accelerate progress in unchanging directions
        mYStep = RandomHelper.randn2d(N, D, 0.0) ' momentum accumulator

        ' 权威存储仍然保持锯齿数组形态，以维持 GetEmbedding 的引用语义
        mY = New Double(N - 1)() {}

        For i As Integer = 0 To N - 1
            mY(i) = New Double(D - 1) {}
        Next

        Call SyncYFromFlat()

        mGrad = New Double(N * D - 1) {}
        bhNegF = Nothing
        bhPosF = Nothing
        mIter = 0
    End Sub

    ''' <summary>
    ''' 把嵌入结果由锯齿数组同步到一维镜像
    ''' </summary>
    Private Sub SyncFlatFromY()
        Dim N = mN
        Dim D = mDim

        For i As Integer = 0 To N - 1
            Dim row = mY(i)
            Dim offset = i * D

            For d As Integer = 0 To D - 1
                mYFlat(offset + d) = row(d)
            Next
        Next
    End Sub

    ''' <summary>
    ''' 把嵌入结果由一维镜像同步回锯齿数组
    ''' </summary>
    Private Sub SyncYFromFlat()
        Dim N = mN
        Dim D = mDim

        For i As Integer = 0 To N - 1
            Dim row = mY(i)
            Dim offset = i * D

            For d As Integer = 0 To D - 1
                row(d) = mYFlat(offset + d)
            Next
        Next
    End Sub

    ' perform a single step of optimization to improve the embedding
    Public Function [Step]() As Double
        mIter += 1

        Dim N = mN
        Dim [dim] = mDim
        Dim Y = mYFlat
        Dim G = mGains
        Dim S = mYStep
        Dim grad = mGrad

        ' 外部可能通过 GetEmbedding() 直接改写了 mY，这里把它同步回一维镜像。
        ' 开销为 O(N*dim)，相对 O(N^2) 的热循环可以忽略。
        Call SyncFlatFromY()

        Me.cost.CostGrad(Y) ' evaluate gradient

        Dim ymean = zeros([dim])
        ' 这两个量在整个循环内为常量，提到循环外以避免 N*dim 次的字段读取与分支判断
        Dim momval = If(mIter < 250, 0.5, 0.8)
        Dim eps = mEpsilon

        ' perform gradient step
        ' 第 i 个任务独占第 i 行（G / S / Y 按行写入），行与行之间无冲突；
        ' ymean 按维度归约，用线程本地累加器 + 原子合并。
        System.Threading.Tasks.Parallel.For(Of Double())(
            0, N, opts,
            Function() New Double([dim] - 1) {},
            Function(i, loopState, localMean) As Double()
                Dim offset = i * [dim]

                For d = 0 To [dim] - 1
                    Dim gid = grad(offset + d)
                    Dim sid = S(offset + d)
                    Dim gainid = G(offset + d)

                    ' compute gain update
                    Dim newgain = If(std.Sign(gid) = std.Sign(sid), gainid * 0.8, gainid + 0.2)

                    If newgain < 0.01 Then
                        ' clamp
                        newgain = 0.01
                    End If

                    ' store for next turn
                    G(offset + d) = newgain
                    ' compute momentum step direction
                    Dim newsid = momval * sid - eps * newgain * gid
                    ' remember the step we took
                    S(offset + d) = newsid
                    ' step!
                    Y(offset + d) += newsid
                    ' accumulate mean so that we can center later
                    localMean(d) += Y(offset + d)
                Next

                Return localMean
            End Function,
            Sub(localMean)
                For d = 0 To [dim] - 1
                    System.Threading.Interlocked.Add(ymean(d), localMean(d))
                Next
            End Sub)

        ' reproject Y to be zero mean，同时把一维镜像回写到权威的锯齿数组
        System.Threading.Tasks.Parallel.For(0, N, opts,
            Sub(i)
                Dim row = mY(i)
                Dim offset = i * [dim]

                For d = 0 To [dim] - 1
                    Dim v = Y(offset + d) - ymean(d) / N

                    Y(offset + d) = v
                    row(d) = v
                Next
            End Sub)

        ' return current cost
        Return mCost
    End Function
End Class
