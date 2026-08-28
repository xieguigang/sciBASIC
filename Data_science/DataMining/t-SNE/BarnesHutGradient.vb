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

Imports std = System.Math

''' <summary>
''' Barnes-Hut 模式下的梯度与成本（KL 散度）计算
''' </summary>
''' <remarks>
''' t-SNE 的梯度可以拆成吸引（正）与排斥（负）两个部分：
''' 
''' dC/dy_i = 4 * [ sum_j p_ij * qu_ij * (y_i - y_j)  -  (1/Z) * sum_j qu_ij^2 * (y_i - y_j) ]
''' 
''' 其中 qu_ij = 1 / (1 + ||y_i - y_j||^2)，Z = sum_{k != l} qu_kl。
''' 
''' 第一部分只涉及稀疏概率矩阵中的 O(N·k) 个近邻对，直接按行遍历即可；
''' 第二部分涉及全部 N² 个点对，改用 <see cref="SPTree"/> 做远场近似。
''' 
''' 并行策略：
''' 1) 远场斥力按点分派，每个点只写自己的 negF 行，无写冲突；配分函数 Z 用线程本地累加器归约。
''' 2) 近场引力按行分派，CSR 格式保证第 i 行的条目连续且与其他行不重叠，同样无写冲突；
'''    KL 成本用线程本地累加器归约。
''' </remarks>
Friend Module BarnesHutGradient

    ''' <summary>
    ''' 计算 Barnes-Hut 近似梯度与 KL 成本，结果写入 <see cref="tSNE.mGrad"/> 与 <see cref="tSNE.mCost"/>
    ''' </summary>
    ''' <param name="tSNE"></param>
    ''' <param name="Y">当前的低维嵌入，行主序一维数组，长度为 N * dim</param>
    Friend Sub Evaluate(tSNE As tSNE, Y As Double())
        Dim N As Integer = tSNE.mN
        Dim [dim] As Integer = tSNE.mDim
        Dim P As SparseP = tSNE.mSparseP
        Dim opts = tSNE.opts

        If P Is Nothing Then
            Throw New InvalidOperationException(
                "Barnes-Hut mode requires a sparse probability matrix. " &
                "Please set UseBarnesHut = True before calling InitDataRaw/InitDataDist.")
        End If

        ' 力累加器按需分配，精确路径下完全不占用这部分内存
        If tSNE.bhNegF Is Nothing OrElse tSNE.bhNegF.Length <> N * [dim] Then
            tSNE.bhNegF = New Double(N * [dim] - 1) {}
            tSNE.bhPosF = New Double(N * [dim] - 1) {}
        End If

        Dim grad As Double() = tSNE.mGrad
        Dim negF As Double() = tSNE.bhNegF
        Dim posF As Double() = tSNE.bhPosF
        Dim theta As Double = tSNE.theta
        ' trick that helps with local optima，仅作用于梯度，不进入成本项
        Dim pmul As Double = If(tSNE.mIter < 100, 4, 1)
        Dim blockSize As Integer = TaskBlockSize(N, tSNE.mThreads)
        Dim nBlocks As Integer = TaskBlockCount(N, blockSize)
        Dim zParts = New Double(nBlocks - 1) {}
        Dim costParts = New Double(nBlocks - 1) {}

        Call System.Array.Clear(negF, 0, negF.Length)
        Call System.Array.Clear(posF, 0, posF.Length)

        ' 每轮迭代都需要依据当前的嵌入重新建树，O(N log N)
        Dim tree As New SPTree([dim], Y, N)

        ' ---------- pass 1：远场斥力 + 配分函数 Z ----------
        ' 每个点只写自己的 negF 行，无写冲突；Z 每个任务块一份局部累加器
        System.Threading.Tasks.Parallel.For(0, nBlocks, opts,
            Sub(b)
                Dim from As Integer = b * blockSize
                Dim upto As Integer = std.Min(from + blockSize, N)
                Dim acc As Double = 0

                For i As Integer = from To upto - 1
                    Call tree.ComputeNonEdgeForces(i, theta, negF, acc)
                Next

                zParts(b) = acc
            End Sub)

        Dim Z As Double = SumParts(zParts)

        If Z <= 0 OrElse Double.IsNaN(Z) Then
            Z = 1
        End If

        Dim invZ As Double = 1.0 / Z

        ' ---------- pass 2：近场引力 + KL 成本 ----------
        ' CSR 之下第 i 行的条目连续，按行分块即天然无写冲突
        System.Threading.Tasks.Parallel.For(0, nBlocks, opts,
            Sub(b)
                Dim from As Integer = b * blockSize
                Dim upto As Integer = std.Min(from + blockSize, N)
                Dim acc As Double = 0

                For i As Integer = from To upto - 1
                    Dim iOffset As Integer = i * [dim]
                    Dim ends As Integer = P.rowPtr(i + 1)

                    For t As Integer = P.rowPtr(i) To ends - 1
                        Dim j As Integer = P.colP(t)
                        Dim jOffset As Integer = j * [dim]
                        ' 注意：不要命名为 D，VB 大小写不敏感会与循环变量 d 冲突
                        Dim d2sum As Double = 0

                        For d As Integer = 0 To [dim] - 1
                            Dim tmp As Double = Y(iOffset + d) - Y(jOffset + d)
                            d2sum += tmp * tmp
                        Next

                        Dim qu As Double = 1.0 / (1.0 + d2sum)
                        Dim v As Double = P.valP(t)
                        Dim mult As Double = v * pmul * qu

                        For d As Integer = 0 To [dim] - 1
                            posF(iOffset + d) += mult * (Y(iOffset + d) - Y(jOffset + d))
                        Next

                        ' q_ij = qu_ij / Z，夹一个下界避免 log(0)
                        acc += -v * std.Log(std.Max(qu * invZ, 1.0E-100))
                    Next
                Next

                costParts(b) = acc
            End Sub)

        ' ---------- pass 3：合成最终梯度 ----------
        Dim scale As Double = 4 * invZ

        System.Threading.Tasks.Parallel.For(0, nBlocks, opts,
            Sub(b)
                Dim from As Integer = b * blockSize
                Dim upto As Integer = std.Min(from + blockSize, N)

                For i As Integer = from To upto - 1
                    Dim offset As Integer = i * [dim]

                    For d As Integer = 0 To [dim] - 1
                        grad(offset + d) = 4 * posF(offset + d) - scale * negF(offset + d)
                    Next
                Next
            End Sub)

        tSNE.mCost = SumParts(costParts)
    End Sub
End Module
