#Region "Microsoft.VisualBasic::56c30593eb80c0924376c1dfad57b08c, Data_science\DataMining\t-SNE\CostFunction.vb"

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

    '   Total Lines: 88
    '    Code Lines: 59 (67.05%)
    ' Comment Lines: 8 (9.09%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 21 (23.86%)
    '     File Size: 2.36 KB


    ' Class CostFunction
    ' 
    '     Properties: mN
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: CostGrad
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Friend Class CostFunction

    ReadOnly tSNE As tSNE

    ''' <summary>
    ''' 未归一化的 Q 分布缓冲区，长度为 N*N，跨迭代复用
    ''' </summary>
    Private lQu As Double()

    ''' <summary>
    ''' 上一次分配缓冲区时所对应的样本数量
    ''' </summary>
    Private bufferedN As Integer = -1

    Public ReadOnly Property mN As Integer
        Get
            Return tSNE.mN
        End Get
    End Property

    Sub New(tSNE As tSNE)
        Me.tSNE = tSNE
    End Sub

    ''' <summary>
    ''' return cost and gradient, given an arrangement
    ''' </summary>
    ''' <param name="Y">
    ''' 当前的低维嵌入，行主序的一维数组（长度为 N * dim）
    ''' </param>
    ''' <remarks>
    ''' 这里依据 <see cref="tSNE.UseBarnesHut"/> 在精确路径与 Barnes-Hut 近似路径之间分派。
    ''' 默认走精确路径，行为与改造前完全一致。
    ''' </remarks>
    Public Sub CostGrad(Y As Double())
        If tSNE.UseBarnesHut Then
            Call BarnesHutGradient.Evaluate(tSNE, Y)
        Else
            Call CostGradExact(Y)
        End If
    End Sub

    ''' <summary>
    ''' 精确的稠密梯度计算，O(N^2) 时间、O(N^2) 内存
    ''' </summary>
    ''' <param name="Y">当前的低维嵌入，行主序一维数组</param>
    Private Sub CostGradExact(Y As Double())
        Dim N = mN
        Dim [dim] = tSNE.mDim ' dim of output space
        Dim P = tSNE.mP
        Dim opts = tSNE.opts
        Dim pmul = If(tSNE.mIter < 100, 4, 1) ' trick that helps with local optima

        Call EnsureBuffers(N, [dim])

        Dim Qu = lQu
        Dim grad = tSNE.mGrad

        ' compute current Q distribution, unnormalized first
        Dim qsum = 0.0
        Dim cost = 0.0

        ' ---------- pass 1：未归一化的 Q 分布 ----------
        ' 按外层行 i 分派。第 i 个任务只写第 i 行与第 i 列（三角对称），
        ' 不同的 i 之间所写入的单元格集合互不相交，因此无需加锁。
        ' qsum 用线程本地累加器归约，避免 N^2 次原子操作。
        System.Threading.Tasks.Parallel.For(Of Double)(
            0, N, opts,
            Function() 0.0,
            Function(i, loopState, acc) As Double
                Dim iOffset = i * [dim]
                Dim rowOffset = i * N

                For j As Integer = i + 1 To N - 1
                    Dim jOffset = j * [dim]
                    Dim dsum = 0.0

                    For d = 0 To [dim] - 1
                        Dim dhere = Y(iOffset + d) - Y(jOffset + d)
                        dsum += dhere * dhere
                    Next

                    ' Student t-distribution
                    Dim qu = 1.0 / (1.0 + dsum)

                    Qu(rowOffset + j) = qu
                    Qu(j * N + i) = qu

                    acc += 2 * qu
                Next

                Return acc
            End Function,
            Sub(acc) System.Threading.Interlocked.Add(qsum, acc))

        If qsum <= 0 OrElse Double.IsNaN(qsum) Then
            qsum = 1
        End If

        ' 归一化系数取倒数，把遍 2 中的 N^2 次除法降为乘法
        Dim invQsum = 1.0 / qsum

        ' ---------- pass 2：梯度与成本 ----------
        ' 第 i 个任务只写 grad 的第 i 行，行与行之间互不相交。
        ' 归一化之后的 Q 不再物化为一整份 N*N 的数组，而是按索引即时算出，
        ' 这样既省下 8N^2 字节内存，也省下了一整轮 N^2 的写 + 读内存扫描。
        System.Threading.Tasks.Parallel.For(Of Double)(
            0, N, opts,
            Function() 0.0,
            Function(i, loopState, acc) As Double
                Dim iOffset = i * [dim]
                Dim rowOffset = i * N

                For d = 0 To [dim] - 1
                    grad(iOffset + d) = 0.0
                Next

                For j = 0 To N - 1
                    Dim jOffset = j * [dim]
                    Dim pij = P(rowOffset + j)
                    Dim quij = Qu(rowOffset + j)
                    Dim qij = std.Max(quij * invQsum, 1.0E-100)

                    ' accumulate cost (the non-constant portion at least...)
                    acc += -pij * std.Log(qij)

                    Dim premult = 4 * (pmul * pij - qij) * quij

                    For d = 0 To [dim] - 1
                        grad(iOffset + d) += premult * (Y(iOffset + d) - Y(jOffset + d))
                    Next
                Next

                Return acc
            End Function,
            Sub(acc) System.Threading.Interlocked.Add(cost, acc))

        tSNE.mCost = cost
    End Sub

    ''' <summary>
    ''' 确保 Q 缓冲区与梯度缓冲区的尺寸与当前样本量匹配，并在尺寸未变时直接复用
    ''' </summary>
    ''' <param name="N"></param>
    ''' <param name="[dim]"></param>
    Private Sub EnsureBuffers(N As Integer, [dim] As Integer)
        If bufferedN = N AndAlso lQu IsNot Nothing AndAlso tSNE.mGrad IsNot Nothing Then
            Return
        End If

        Dim cells As Long = CLng(N) * N

        If cells > Integer.MaxValue Then
            Throw New InsufficientMemoryException(
                $"The dense exact t-SNE path requires {cells * 8 / 1024 / 1024 / 1024} GB for a {N} x {N} matrix. " &
                $"Please enable the Barnes-Hut approximation (UseBarnesHut = True) for datasets of this size.")
        End If

        lQu = New Double(CInt(cells) - 1) {}
        tSNE.mGrad = New Double(N * [dim] - 1) {}
        bufferedN = N
    End Sub

End Class
