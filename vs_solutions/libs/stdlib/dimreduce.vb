Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.PaCMAP
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.MachineLearning.tSNE
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA

''' <summary>
''' 降维模块：PCA / UMAP / t-SNE / PaCMAP。
''' <para>
''' 所有函数都直接接收特征表并返回**新的**坐标表（特征列名为 ``dim_1 .. dim_n``，
''' 行名与原表的标签列会被继承），因此结果可以直接接绘图或聚类。
''' </para>
''' </summary>
Public Module dimreduce

    ''' <summary>主成分分析的得分矩阵（样本在 PC1..PCn 上的坐标）。</summary>
    ''' 
    <Extension>
    Public Function pcaScore(source As NumericTable, Optional maxPC As Integer = 5) As NumericTable
        Return ScoreOf(source, maxPC)
    End Function

    ''' <summary>主成分分析的载荷矩阵（特征在各主成分上的权重）。</summary>
    ''' 
    <Extension>
    Public Function pcaLoading(source As NumericTable, Optional maxPC As Integer = 5) As NumericTable
        Return source.pca(maxPC:=maxPC).LoadingTable()
    End Function

    ''' <summary>UMAP 流形降维。</summary>
    ''' 
    <Extension>
    Public Function umap(source As NumericTable, Optional dims As Integer = 2, Optional neighbors As Integer = 15) As NumericTable
        Return source.UMAPFit(dims:=dims, neighbors:=neighbors)
    End Function

    ''' <summary>t-SNE 降维。</summary>
    ''' 
    <Extension>
    Public Function tsne(source As NumericTable, Optional dims As Integer = 2, Optional perplexity As Double = 30) As NumericTable
        Return source.TSneFit(perplexity:=perplexity, dims:=dims)
    End Function

    ''' <summary>PaCMAP 降维。</summary>
    ''' 
    <Extension>
    Public Function pacmap(source As NumericTable, Optional dims As Integer = 2, Optional neighbors As Integer = 10) As NumericTable
        Return source.PacMapFit(dims:=dims, neighbors:=neighbors)
    End Function

    Private Function ScoreOf(source As NumericTable, maxPC As Integer) As NumericTable
        Return source.pca(maxPC:=maxPC).ScoreTable()
    End Function
End Module
