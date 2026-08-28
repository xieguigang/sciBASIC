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
''' 稀疏联合概率矩阵（CSR 行压缩存储）
''' </summary>
''' <remarks>
''' 精确的 t-SNE 需要一个 N×N 的稠密联合概率矩阵，其内存占用为 8N² 字节，
''' 在 N 达到万级时就已经不可行。Barnes-Hut 近似只需要每一个点的 k 个近邻，
''' 因此这里改用 CSR 格式只保存 O(N·k) 个非零条目。
''' 
''' 由于采用了行压缩格式，第 i 行的全部条目在内存中是连续的，
''' 这使得梯度计算可以按行分派到多个线程之上而完全不需要加锁。
''' </remarks>
Friend Class SparseP

    ''' <summary>
    ''' 样本数量
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly N As Integer

    ''' <summary>
    ''' 非零条目总数（双向展开之后的长度）
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly nnz As Integer

    ''' <summary>
    ''' 行偏移表，长度为 N + 1；第 i 行的条目位于 <c>[rowPtr(i), rowPtr(i + 1))</c> 区间之内
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly rowPtr As Integer()

    ''' <summary>
    ''' 列索引，长度为 <see cref="nnz"/>
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly colP As Integer()

    ''' <summary>
    ''' 概率值，长度为 <see cref="nnz"/>；已经完成对称化与 1/(2N) 归一化
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly valP As Double()

    Friend Sub New(N As Integer, rowPtr As Integer(), colP As Integer(), valP As Double())
        Me.N = N
        Me.rowPtr = rowPtr
        Me.colP = colP
        Me.valP = valP
        Me.nnz = colP.Length
    End Sub

    ''' <summary>
    ''' 估算该稀疏矩阵所占用的字节数
    ''' </summary>
    ''' <returns></returns>
    Friend Function MemorySize() As Long
        Return CLng(rowPtr.Length) * 4 + CLng(colP.Length) * 4 + CLng(valP.Length) * 8
    End Function

    Public Overrides Function ToString() As String
        Return $"sparse P: N={N}, nnz={nnz}, ~{MemorySize() / 1024 / 1024} MB"
    End Function
End Class

''' <summary>
''' 稀疏联合概率矩阵的装配工具
''' </summary>
Friend Module SparseProbability

    ''' <summary>
    ''' 依据困惑度推荐每一行需要保留的近邻数量
    ''' </summary>
    ''' <param name="perplexity"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 沿用 L. van der Maaten 的 bh_tsne 参考实现中的经验取值 3 * perplexity。
    ''' 这个值直接决定了稀疏矩阵的内存占用（O(N·k)）与梯度计算量，
    ''' 是 Barnes-Hut 模式下精度与开销之间最主要的调节旋钮。
    ''' </remarks>
    Friend Function SuggestK(perplexity As Double) As Integer
        Return CInt(std.Floor(3 * perplexity)) + 1
    End Function

    ''' <summary>
    ''' 把 (i, j, v) 三元组排序之后装配为 CSR 稀疏矩阵
    ''' </summary>
    ''' <param name="keys">
    ''' 打包后的键，取值为 <c>i * N + j</c>；本函数会就地对其进行排序
    ''' </param>
    ''' <param name="vals">
    ''' 与 <paramref name="keys"/> 一一对应的概率值，排序过程中会被同步重排
    ''' </param>
    ''' <param name="N">样本数量</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 键 <c>i * N + j</c> 的升序排列天然就是行主序，因此一次排序即可同时完成
    ''' 「按行分组」与「行内按列有序」两件事，无需再借助哈希表做 O(nnz) 的合并去重：
    ''' 同一个无序对 {i, j} 在双向展开之后会在第 i 行出现两次（分别为 p_{j|i} 与 p_{i|j}），
    ''' 这两条条目在梯度循环中会被简单地累加起来，恰好等价于联合概率定义中的求和。
    ''' </remarks>
    Friend Function Build(keys As Long(), vals As Double(), N As Integer) As SparseP
        Call System.Array.Sort(keys, vals)

        Dim nnz As Integer = keys.Length
        Dim rowPtr = New Integer(N) {}
        Dim colP = New Integer(nnz - 1) {}
        Dim valP = New Double(nnz - 1) {}
        Dim scale As Double = 1.0 / (2.0 * N)
        Dim row As Integer = 0

        rowPtr(0) = 0

        For t As Integer = 0 To nnz - 1
            Dim key As Long = keys(t)
            Dim i As Integer = CInt(key \ N)
            Dim j As Integer = CInt(key - CLng(i) * N)

            ' 补齐当前行之前所有空行的行边界
            Do While row < i
                rowPtr(row + 1) = t
                row += 1
            Loop

            colP(t) = j
            valP(t) = vals(t) * scale
        Next

        ' 补齐末尾的若干个空行
        Do While row < N
            rowPtr(row + 1) = nnz
            row += 1
        Loop

        Return New SparseP(N, rowPtr, colP, valP)
    End Function
End Module
