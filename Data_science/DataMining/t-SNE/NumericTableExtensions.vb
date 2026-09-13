#Region "Microsoft.VisualBasic::NumericTableExtensions, Data_science\DataMining\t-SNE\NumericTableExtensions.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data
Imports tSNEEstimator = Microsoft.VisualBasic.MachineLearning.tSNE.tSNE

''' <summary>
''' t-SNE 降维算法的统一二维表入口。
''' </summary>
Public Module NumericTableExtensions

    ''' <summary>
    ''' 使用 t-SNE 对统一二维表<see cref="NumericTable"/>之中的样本特征进行降维，
    ''' 并返回以嵌入坐标为特征的新二维表。
    ''' 
    ''' ```vb
    ''' Dim y = x.tsne(perplexity:=30, dim:=2)
    ''' Dim z = y.kmeans(k:=3)
    ''' ```
    ''' </summary>
    ''' <param name="source">经过预处理之后的纯数值二维表</param>
    ''' <param name="perplexity">困惑度，必须小于样本数量</param>
    ''' <param name="dims">嵌入维度，通常为 2 或 3</param>
    ''' <param name="epsilon">学习率</param>
    ''' <param name="iterations">梯度下降的迭代步数</param>
    ''' <param name="useBarnesHut">是否启用 Barnes-Hut 近似</param>
    ''' <param name="nthreads">并行线程数，&lt;= 0 时取当前主机的 CPU 核心数</param>
    ''' <returns>以嵌入坐标（``dim_1..dim_n``）为特征的新二维表</returns>
    <Extension>
    Public Function tsne(source As NumericTable,
                         Optional perplexity As Double = 30,
                         Optional dims As Integer = 2,
                         Optional epsilon As Double = 200,
                         Optional iterations As Integer = 1000,
                         Optional useBarnesHut As Boolean = False,
                         Optional nthreads As Integer = 0) As NumericTable

        If source Is Nothing Then
            Throw New ArgumentNullException(NameOf(source))
        End If
        If source.nsamples = 0 OrElse source.nfeatures = 0 Then
            Throw New InvalidConstraintException("the source table has no sample/feature data for t-SNE embedding!")
        End If
        If perplexity <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(perplexity), "the perplexity must be a positive value")
        End If
        If perplexity >= source.nsamples Then
            Throw New ArgumentException(
                $"the perplexity {perplexity} must be smaller than the sample size {source.nsamples}!",
                NameOf(perplexity)
            )
        End If
        If iterations <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(iterations), "the iterations must be a positive value")
        End If

        ' 注意：UseBarnesHut / nthreads 必须在 InitDataRaw 之前设置，
        ' 因为其决定了概率矩阵的构建方式；这里通过构造函数一次性传入。
        Dim estimator As New tSNEEstimator(perplexity, dims, epsilon, nthreads, useBarnesHut)

        Call estimator.InitDataRaw(source.NumericRows())

        For i As Integer = 1 To iterations
            Call estimator.Step()
        Next

        Return source.ToEmbeddingTable(estimator.GetEmbedding())
    End Function
End Module
