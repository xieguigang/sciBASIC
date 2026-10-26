Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.MachineLearning.tSNE
Imports Microsoft.VisualBasic.DataMining.PaCMAP

''' <summary>
''' 降维模块：PCA / UMAP / t-SNE / PaCMAP。
''' <para>
''' 所有函数都直接接收特征表并返回**新的**坐标表（特征列名为 ``dim_1 .. dim_n``，
''' 行名与原表的标签列会被继承），因此结果可以直接接绘图或聚类。
''' </para>
''' </summary>
Public Module dimreduce

    ''' <summary>主成分分析，返回分析结果对象（需要坐标表时请用 <see cref="pcaScore"/>）。</summary>
    Public Function pca(source As NumericTable, Optional maxPC As Integer = 5) As MultivariateAnalysisResult
        Return source.pca(maxPC:=maxPC)
    End Function

    ''' <summary>主成分分析的得分矩阵（样本在 PC1..PCn 上的坐标）。</summary>
    Public Function pcaScore(source As NumericTable, Optional maxPC As Integer = 5) As NumericTable
        Return source.pca(maxPC:=maxPC).ScoreTable()
    End Function

    ''' <summary>主成分分析的载荷矩阵（特征在各主成分上的权重）。</summary>
    Public Function pcaLoading(source As NumericTable, Optional maxPC As Integer = 5) As NumericTable
        Return source.pca(maxPC:=maxPC).LoadingTable()
    End Function

    ''' <summary>UMAP 流形降维。</summary>
    Public Function umap(source As NumericTable, Optional dims As Integer = 2, Optional neighbors As Integer = 15) As NumericTable
        Return source.umap(dims:=dims, neighbors:=neighbors)
    End Function

    ''' <summary>t-SNE 降维。</summary>
    Public Function tsne(source As NumericTable, Optional dims As Integer = 2, Optional perplexity As Double = 30) As NumericTable
        Return source.tsne(perplexity:=perplexity, dims:=dims)
    End Function

    ''' <summary>PaCMAP 降维。</summary>
    Public Function pacmap(source As NumericTable, Optional dims As Integer = 2, Optional neighbors As Integer = 10) As NumericTable
        Return source.pacmap(dims:=dims, neighbors:=neighbors)
    End Function
End Module
