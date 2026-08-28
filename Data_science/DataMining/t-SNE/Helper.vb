#Region "Microsoft.VisualBasic::036d324dfaecf8bfb223698502597b78, Data_science\DataMining\t-SNE\Helper.vb"

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

    '   Total Lines: 164
    '    Code Lines: 97 (59.15%)
    ' Comment Lines: 36 (21.95%)
    '    - Xml Docs: 63.89%
    ' 
    '   Blank Lines: 31 (18.90%)
    '     File Size: 5.24 KB


    ' Module Helper
    ' 
    '     Function: d2p, L2, xtod, zeros
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Module Helper

    ''' <summary>
    ''' 依据给定的并行度创建一个 <see cref="System.Threading.Tasks.ParallelOptions"/> 对象
    ''' </summary>
    ''' <param name="nthreads">
    ''' 并行线程数，小于等于 0 时表示不限制（交由 TPL 的线程池自行调度）
    ''' </param>
    ''' <returns></returns>
    Friend Function ParallelOptions(nthreads As Integer) As System.Threading.Tasks.ParallelOptions
        Return New System.Threading.Tasks.ParallelOptions With {
            .MaxDegreeOfParallelism = If(nthreads > 0, nthreads, -1)
        }
    End Function

    ''' <summary>
    ''' utilitity that creates contiguous vector of zeros of size n
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Friend Function zeros(n As Integer) As Double()
        ' .NET 数组在分配之时即已经被清零，无需再做一次 O(n) 的写入扫描
        Return New Double(n - 1) {}
    End Function

    ''' <summary>
    ''' compute L2 distance between two vectors
    ''' </summary>
    ''' <param name="x1"></param>
    ''' <param name="x2"></param>
    ''' <returns></returns>
    Friend Function L2(x1 As Double(), x2 As Double()) As Double
        Dim N = x1.Length
        Dim d As Double = 0

        For i = 0 To N - 1
            Dim x1i = x1(i)
            Dim x2i = x2(i)
            d += (x1i - x2i) * (x1i - x2i)
        Next

        Return d
    End Function

    ''' <summary>
    ''' 并行计算两两距离矩阵
    ''' </summary>
    ''' <param name="X"></param>
    ''' <param name="nthreads">并行线程数，&lt;=0 表示不限制</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 按外层行 i 分派。第 i 个任务只写第 i 行与第 i 列（三角对称），
    ''' 不同的 i 之间所写入的单元格集合互不相交，因此无需加锁。
    ''' </remarks>
    Friend Function xtod(X As Double()(), Optional nthreads As Integer = 0) As Double()
        Dim N As Integer = X.Length
        Dim dist As Double() = zeros(N * N) ' allocate contiguous array
        Dim opts = ParallelOptions(nthreads)

        System.Threading.Tasks.Parallel.For(0, N, opts,
            Sub(i)
                Dim xi = X(i)
                Dim offset = i * N

                For j As Integer = i + 1 To N - 1
                    Dim d = L2(xi, X(j))

                    dist(offset + j) = d
                    dist(j * N + i) = d
                Next
            End Sub)

        Return dist
    End Function

    ''' <summary>
    ''' 每一行在进行 beta 二分搜索与 top-k 选择时所需要的线程私有工作区
    ''' </summary>
    Private Class RowWorkspace
        ''' <summary>第 i 行到所有点的距离（BH 模式下按行现算）</summary>
        Friend dist As Double()
        ''' <summary>条件概率 p_{j|i}</summary>
        Friend prow As Double()
        ''' <summary>quickselect 的牺牲缓冲，会被打乱顺序</summary>
        Friend work As Double()

        Sub New(n As Integer)
            dist = New Double(n - 1) {}
            prow = New Double(n - 1) {}
            work = New Double(n - 1) {}
        End Sub
    End Class

    ''' <summary>
    ''' 线程私有缓冲区的释放回调（<c>Parallel.For</c> 的 localFinally 需要一个非空委托）
    ''' </summary>
    Private Shared Sub ReleaseWorkspace(ws As RowWorkspace)
        ' 线程本地缓冲区交还给 GC，此处无需任何额外处理
    End Sub

    ''' <summary>
    ''' 在 <paramref name="arr"/> 的前 <paramref name="n"/> 个元素中找出第 <paramref name="k"/> 大的值
    ''' </summary>
    ''' <param name="arr">会被就地分区打乱，调用前请自行备份</param>
    ''' <param name="n"></param>
    ''' <param name="k">0-based，0 表示最大值</param>
    ''' <returns></returns>
    ''' <remarks>3-way 快排选择，平均 O(n)，用于避免对每一行做 O(n log n) 的全排序</remarks>
    Private Function QuickSelect(arr As Double(), n As Integer, k As Integer) As Double
        Dim left = 0
        Dim right = n - 1

        Do While left < right
            Dim pivotValue = arr((left + right) \ 2)
            Dim i = left, j = right, m = left

            Do While m <= j
                If arr(m) > pivotValue Then
                    Dim t = arr(m) : arr(m) = arr(i) : arr(i) = t
                    i += 1
                    m += 1
                ElseIf arr(m) < pivotValue Then
                    Dim t = arr(m) : arr(m) = arr(j) : arr(j) = t
                    j -= 1
                Else
                    m += 1
                End If
            Loop

            ' 此时 [left, i) 全部 &gt; pivot，[i, j] 全部 == pivot，(j, right] 全部 &lt; pivot
            If k < i Then
                right = i - 1
            ElseIf k > j Then
                left = j + 1
            Else
                Exit Do
            End If
        Loop

        Return arr(k)
    End Function

    ''' <summary>
    ''' 逐行现算距离 + 每行仅保留 top-k 近邻，构建稀疏的联合概率矩阵
    ''' </summary>
    ''' <param name="X">高维原始数据</param>
    ''' <param name="perplexity">困惑度</param>
    ''' <param name="tol">beta 二分搜索的收敛容差</param>
    ''' <param name="k">每一行保留的近邻数量</param>
    ''' <param name="nthreads">并行线程数</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 这个函数全程不物化 N×N 的距离矩阵与 N×N 的稠密概率矩阵，
    ''' 是 Barnes-Hut 模式得以突破 O(N²) 内存墙的关键。
    ''' 
    ''' 对称化采用「双向展开」而非字典合并：对于每一行的第 m 个近邻 j，
    ''' 分别写入 (i, j, p_{j|i}) 与 (j, i, p_{j|i})。
    ''' 于是对于无序对 {i, j}，行 i 上会同时收到 p_{j|i}（来自行 i 自身的近邻表）与
    ''' p_{i|j}（来自行 j 的近邻表所展开出的反向条目），两者相加恰好等于
    ''' 联合概率定义中的 (p_{j|i} + p_{i|j})，因此无需再做一次 O(nnz) 的合并去重。
    ''' </remarks>
    Friend Function d2pSparse(X As Double()(),
                             perplexity As Double,
                             tol As Double,
                             k As Integer,
                             Optional nthreads As Integer = 0) As SparseP

        Dim N As Integer = X.Length
        Dim Htarget = std.Log(perplexity) ' target entropy of distribution
        Dim opts = ParallelOptions(nthreads)
        Dim kk As Integer = std.Min(k, N - 1)
        Dim nnz0 As Integer = N * kk

        ' 双向展开之后的总条目数
        Dim keys = New Long(2 * nnz0 - 1) {}
        Dim vals = New Double(2 * nnz0 - 1) {}

        System.Threading.Tasks.Parallel.For(Of RowWorkspace)(
            0, N, opts,
            Function() New RowWorkspace(N),
            Function(i, loopState, ws) As RowWorkspace
                Dim xi = X(i)

                ' 现算第 i 行的距离，避免物化 N×N 距离矩阵
                For j = 0 To N - 1
                    ws.dist(j) = L2(xi, X(j))
                Next

                Dim betamin = Double.NegativeInfinity
                Dim betamax = Double.PositiveInfinity
                Dim beta As Double = 1 ' initial value of precision
                Dim done = False
                Dim maxtries = 50
                Dim num = 0

                ' perform binary search to find a suitable precision beta
                ' so that the entropy of the distribution is appropriate
                While Not done
                    Dim psum = 0.0

                    For j = 0 To N - 1
                        Dim pj = std.Exp(-ws.dist(j) * beta)

                        If i = j Then pj = 0 ' we dont care about diagonals
                        ws.prow(j) = pj
                        psum += pj
                    Next

                    ' normalize p and compute entropy
                    Dim Hhere = 0.0

                    For j = 0 To N - 1
                        Dim pj = ws.prow(j) / psum

                        ws.prow(j) = pj

                        If pj > 0.0000001 Then
                            Hhere -= pj * std.Log(pj)
                        End If
                    Next

                    If Hhere > Htarget Then
                        betamin = beta

                        If betamax = Double.PositiveInfinity Then
                            beta = beta * 2
                        Else
                            beta = (beta + betamax) / 2
                        End If
                    Else
                        betamax = beta

                        If betamin = Double.NegativeInfinity Then
                            beta = beta / 2
                        Else
                            beta = (beta + betamin) / 2
                        End If
                    End If

                    num += 1

                    If std.Abs(Hhere - Htarget) < tol Then done = True
                    If num >= maxtries Then done = True
                End While

                ' 排除自身之后再取 top-k
                ws.prow(i) = Double.NegativeInfinity
                Call Array.Copy(ws.prow, ws.work, N)

                Dim threshold = QuickSelect(ws.work, N, N - kk)
                Dim baseOffset = i * kk
                Dim t = 0

                For j = 0 To N - 1
                    If t >= kk Then Exit For
                    If ws.prow(j) > threshold Then
                        EmitPair(keys, vals, N, baseOffset + t, i, j, ws.prow(j))
                        t += 1
                    End If
                Next

                ' 补齐与阈值并列的元素
                If t < kk Then
                    For j = 0 To N - 1
                        If t >= kk Then Exit For
                        If ws.prow(j) = threshold Then
                            EmitPair(keys, vals, N, baseOffset + t, i, j, ws.prow(j))
                            t += 1
                        End If
                    Next
                End If

                ' 理论上不会发生（kk &lt;= N - 1），留作兜底以避免出现空洞
                While t < kk
                    EmitPair(keys, vals, N, baseOffset + t, i, i, 0.0)
                    t += 1
                End While

                Return ws
            End Function,
            AddressOf ReleaseWorkspace)

        Return SparseProbability.Build(keys, vals, N)
    End Function

    ''' <summary>
    ''' 把 (i, j, v) 双向写入键/值数组，同时完成 (p_{j|i} + p_{i|j}) 的对称化展开
    ''' </summary>
    Private Sub EmitPair(keys As Long(), vals As Double(), N As Integer,
                         slot As Integer, i As Integer, j As Integer, v As Double)
        Dim p = 2 * slot

        keys(p) = CLng(i) * N + j
        vals(p) = v
        keys(p + 1) = CLng(j) * N + i
        vals(p + 1) = v
    End Sub

    ''' <summary>
    ''' compute (p_{i|j} + p_{j|i})/(2n)
    ''' </summary>
    ''' <param name="D">distance matrix</param>
    ''' <param name="perplexity"></param>
    ''' <param name="tol"></param>
    ''' <returns></returns>
    Friend Function d2p(D As Double(),
                        perplexity As Double,
                        tol As Double,
                        Optional nthreads As Integer = 0) As Double()

        Dim Nf = std.Sqrt(D.Length) ' this better be an integer
        Dim N As Integer = std.Floor(Nf)
        Dim Htarget = std.Log(perplexity) ' target entropy of distribution
        Dim P = zeros(N * N) ' temporary probability matrix
        Dim opts = ParallelOptions(nthreads)

        ' 每一行 i 的二分搜索完全独立，仅写第 i 行，可以安全地并行分派；
        ' 唯一需要私有化的是临时存储 prow，这里通过 localInit 为每个线程各分配一份并复用，
        ' 避免在内层循环里做 N 次长度为 N 的数组分配。
        System.Threading.Tasks.Parallel.For(Of Double())(
            0, N, opts,
            Function() New Double(N - 1) {},
            Function(i, loopState, prow) As Double()
                Call SearchPrecision(D, P, N, i, Htarget, tol, prow)
                Return prow
            End Function,
            AddressOf ReleaseRowBuffer)

        Return Symmetrize(P, N, nthreads)
    End Function

    ''' <summary>
    ''' 线程私有缓冲区的释放回调
    ''' </summary>
    Private Shared Sub ReleaseRowBuffer(prow As Double())
        ' 线程本地缓冲区交还给 GC，此处无需任何额外处理
    End Sub

    ''' <summary>
    ''' 对第 i 行执行 beta 二分搜索，把最终的条件概率写入 <paramref name="P"/> 的第 i 行
    ''' </summary>
    Private Sub SearchPrecision(D As Double(), P As Double(), N As Integer, i As Integer,
                                Htarget As Double, tol As Double, prow As Double())
        Dim betamin = Double.NegativeInfinity
            Dim betamax = Double.PositiveInfinity
            Dim beta As Double = 1 ' initial value of precision
            Dim done = False
            Dim maxtries = 50

            ' perform binary search to find a suitable precision beta
            ' so that the entropy of the distribution is appropriate
            Dim num = 0

            While Not done
                'debugger;

                ' compute entropy and kernel row with beta precision
                Dim psum = 0.0

                For j = 0 To N - 1
                    Dim pj = std.Exp(-D(i * N + j) * beta)
                    If i = j Then pj = 0 ' we dont care about diagonals
                    prow(j) = pj
                    psum += pj
                Next

                ' normalize p and compute entropy
                Dim Hhere = 0.0

                For j = 0 To N - 1
                    Dim pj = prow(j) / psum

                    prow(j) = pj

                    If pj > 0.0000001 Then
                        Hhere -= pj * std.Log(pj)
                    End If
                Next

                ' adjust beta based on result
                If Hhere > Htarget Then
                    ' entropy was too high (distribution too diffuse)
                    ' so we need to increase the precision for more peaky distribution
                    betamin = beta ' move up the bounds

                    If betamax = Double.PositiveInfinity Then
                        beta = beta * 2
                    Else
                        beta = (beta + betamax) / 2
                    End If
                Else
                    ' converse case. make distrubtion less peaky
                    betamax = beta

                    If betamin = Double.NegativeInfinity Then
                        beta = beta / 2
                    Else
                        beta = (beta + betamin) / 2
                    End If
                End If

                ' stopping conditions: too many tries or got a good precision
                num += 1

                If std.Abs(Hhere - Htarget) < tol Then
                    done = True
                End If

                If num >= maxtries Then
                    done = True
                End If
            End While

            ' console.log('data point ' + i + ' gets precision ' + beta + ' after ' + num + ' binary search steps.');
            ' copy over the final prow to P at row i
            For j = 0 To N - 1
                P(i * N + j) = prow(j)
            Next
        ' copy over the final prow to P at row i
        Dim offset = i * N

        For j = 0 To N - 1
            P(offset + j) = prow(j)
        Next
    End Sub

    ''' <summary>
    ''' symmetrize P and normalize it to sum to 1 over all ij
    ''' </summary>
    ''' <param name="P">稠密的条件概率矩阵，行主序</param>
    ''' <param name="N"></param>
    ''' <param name="nthreads">并行线程数</param>
    ''' <returns>对称化之后的联合概率矩阵</returns>
    ''' <remarks>
    ''' 第 i 个任务只写 <paramref name="Pout"/> 的第 i 行，行与行之间互不相交，可安全并行。
    ''' </remarks>
    Private Function Symmetrize(P As Double(), N As Integer, nthreads As Integer) As Double()
        Dim Pout = zeros(N * N)
        Dim N2 = N * 2
        Dim opts = ParallelOptions(nthreads)

        System.Threading.Tasks.Parallel.For(0, N, opts,
            Sub(i)
                Dim offset = i * N

                For j = 0 To N - 1
                    Pout(offset + j) = std.Max((P(offset + j) + P(j * N + i)) / N2, 1.0E-100)
                Next
            End Sub)

        Return Pout
    End Function
End Module
