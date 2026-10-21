Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq

Namespace Hamming

    ''' <summary>
    ''' Hamming 聚类的统一二维表入口。
    ''' 
    ''' 与 <c>dbscan</c> / <c>hdbscan</c> / <c>bisectingKMeans</c> 等其它聚类算法保持一致：
    ''' 以 <see cref="NumericTable"/> 的特征矩阵作为输入，把簇编号写入 ``cluster`` 标签列
    ''' （1 基编号，与其它聚类结果可以直接互相比较）。
    ''' </summary>
    <HideModuleName>
    Public Module HammingTableExtensions

        ''' <summary>
        ''' 把二维表的特征矩阵二值化之后转换成位模式集合（供规则提取与预测复用）。
        ''' </summary>
        ''' <param name="source">经过预处理的数值二维表</param>
        ''' <param name="threshold">二值化阈值（严格大于阈值记为 1）</param>
        ''' <returns></returns>
        <Extension>
        Public Function hammingPatterns(source As NumericTable, Optional threshold As Double = 0.5) As BitPattern()
            If source Is Nothing Then
                Throw New ArgumentNullException(NameOf(source))
            End If

            Return source _
                .NumericRows() _
                .Select(Function(row) BitPattern.FromValues(row, threshold)) _
                .ToArray
        End Function

        ''' <summary>
        ''' 以 Hamming 距离为度量的 k-modes 聚类，结果写入 ``cluster`` 标签列（1 基编号）。
        ''' </summary>
        ''' <param name="source">经过预处理的数值二维表</param>
        ''' <param name="k">簇的数量</param>
        ''' <param name="binary">
        ''' 为 ``True``（缺省）时按阈值把特征二值化，走位运算快速路径；
        ''' 为 ``False`` 时把每一列的相异取值当作定类取值处理。
        ''' </param>
        ''' <param name="threshold">二值化阈值</param>
        ''' <param name="seed">随机种子，传入固定值可以复现结果</param>
        ''' <param name="initialization">初始质心的选取策略</param>
        ''' <param name="maxIterations">最大迭代次数</param>
        ''' <param name="parallel">是否在分配阶段并行计算</param>
        ''' <param name="labelName">标签列名称</param>
        ''' <returns>写入聚类标签之后的原表对象</returns>
        <Extension>
        Public Function hammingCluster(source As NumericTable,
                                       k As Integer,
                                       Optional binary As Boolean = True,
                                       Optional threshold As Double = 0.5,
                                       Optional seed As Integer? = Nothing,
                                       Optional initialization As KmodesInitialization = KmodesInitialization.PlusPlus,
                                       Optional maxIterations As Integer = 100,
                                       Optional parallel As Boolean = True,
                                       Optional labelName As String = "cluster") As NumericTable

            If source Is Nothing Then
                Throw New ArgumentNullException(NameOf(source))
            End If
            If String.IsNullOrEmpty(labelName) Then
                Throw New ArgumentNullException(NameOf(labelName))
            End If

            Dim data As HammingData = HammingEncoder.FromValues(source.NumericRows(), binary, threshold)
            Dim result As KmodesResult = New KmodesEngine().Run(data, k, seed, initialization, maxIterations, parallel)

            ' 算法层使用 0 基编号，写回表时统一加 1，与其它聚类算法保持一致
            Dim labels As Integer() = result.Labels.Select(Function(l) l + 1).ToArray

            Return source.SetLabel(labelName, labels)
        End Function

        ''' <summary>
        ''' 以指定标签列作为类别，提取 Hamming 聚类的布尔规则（Muselli–Liberati HC）。
        ''' </summary>
        ''' <param name="source">经过预处理的数值二维表</param>
        ''' <param name="labelColumn">作为类别的标签列名称，缺省为 ``cluster``</param>
        ''' <param name="threshold">特征二值化阈值</param>
        ''' <param name="theta">类内簇生成的距离阈值，``NaN`` 表示自动取 1</param>
        ''' <param name="prune">是否执行保持训练集一致的贪心剪枝</param>
        ''' <param name="minCoverage">一个簇至少需要包含的样本数量</param>
        ''' <returns>包含训练集精度报告的规则集合</returns>
        <Extension>
        Public Function hammingRuleExtract(source As NumericTable,
                                           Optional labelColumn As String = "cluster",
                                           Optional threshold As Double = 0.5,
                                           Optional theta As Double = Double.NaN,
                                           Optional prune As Boolean = True,
                                           Optional minCoverage As Integer = 1) As RuleSet

            If source Is Nothing Then
                Throw New ArgumentNullException(NameOf(source))
            End If

            Dim patterns As BitPattern() = source.hammingPatterns(threshold)
            Dim labels As Integer() = source.GetLabel(labelColumn).Select(Function(v) CInt(v)).ToArray

            Return New HammingClusteringEngine().Run(patterns, labels, theta, prune, minCoverage)
        End Function

        ''' <summary>
        ''' 把规则集合应用回二维表，预测结果写入 ``hamming_rule`` 标签列。
        ''' 
        ''' 没有任何规则命中的样本会被写成 ``-1``。
        ''' </summary>
        ''' <param name="source">经过预处理的数值二维表</param>
        ''' <param name="rules">由 <see cref="hammingRuleExtract"/> 得到的规则集合</param>
        ''' <param name="threshold">特征二值化阈值</param>
        ''' <param name="labelName">写入的标签列名称</param>
        ''' <returns>写入预测标签之后的原表对象</returns>
        <Extension>
        Public Function hammingRulePredict(source As NumericTable,
                                           rules As RuleSet,
                                           Optional threshold As Double = 0.5,
                                           Optional labelName As String = "hamming_rule") As NumericTable

            If source Is Nothing Then
                Throw New ArgumentNullException(NameOf(source))
            End If
            If rules Is Nothing Then
                Throw New ArgumentNullException(NameOf(rules))
            End If
            If String.IsNullOrEmpty(labelName) Then
                Throw New ArgumentNullException(NameOf(labelName))
            End If

            Dim predicted As Integer() = rules.PredictAll(source.hammingPatterns(threshold))

            Return source.SetLabel(labelName, predicted)
        End Function
    End Module
End Namespace
