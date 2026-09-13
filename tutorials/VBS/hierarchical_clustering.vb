#include "Microsoft.VisualBasic.DataMining.HierarchicalClustering.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Math.Statistics.ANOVA.dll"
#include "Microsoft.VisualBasic.Drawing.dll"

imports Microsoft.VisualBasic.Data
imports Microsoft.VisualBasic.Data.Framework
imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

' ---------------------------------------------------------------------------
' 层次聚类的 demo
'
'   特征表 → distanceMatrix() → 距离矩阵表 → hca() 聚类树 / hcut() 切分成簇
'     → 与真实物种对比 → PCA 降维绘图 → 导出 csv
' ---------------------------------------------------------------------------
dim file = "G:\GCModeller\src\R-sharp\REnv\data\bezdekIris.csv"
dim k = 3

' ---------------------------------------------------------------------------
' 1. 加载 iris 数据集为统一二维表（首列行名 + D1..D4 特征列）
' ---------------------------------------------------------------------------
dim table = NumericTableIO.ReadCsv(file, columns := {"D1","D2","D3","D4"})

' iris 文件的第一列是物种名(有重复)，而层次聚类要求样本名唯一，
' 因此这里把行名改写为唯一的样本编号，同时把真实物种映射为数值标签列 species 保留下来
dim species = table.rowNames
dim spNames as new List(Of String)
dim sp(table.nsamples - 1) as double
dim ids(table.nsamples - 1) as string

for i = 0 to species.length - 1
    if not spNames.Contains(species(i)) then call spNames.Add(species(i))

    sp(i) = spNames.IndexOf(species(i)) + 1
    ids(i) = "sample_" & (i + 1)
next

table.rowNames = ids

call table.SetLabel("species", sp)

call console.WriteLine($"dataset: {table.nsamples} samples x {table.nfeatures} features, {spNames.Count} species")

' ---------------------------------------------------------------------------
' 2. 特征表 → 对称距离矩阵表（默认欧氏距离）
' ---------------------------------------------------------------------------
dim dist = table.distanceMatrix()

call console.WriteLine($"distance matrix: {dist.nsamples} x {dist.nfeatures} (square matrix)")

' ---------------------------------------------------------------------------
' 3. 层次聚类
'
'    + hca()  返回聚类树(dendrogram)的根节点
'    + hcut() 把聚类树切分成目标数量的簇，并把类编号写入 cluster 标签列
' ---------------------------------------------------------------------------
dim tree = dist.hca()
dim flat = dist.hcut(k := k)
dim labels = flat.ClusterLabels()
dim sizes(k - 1) as integer

for i = 0 to labels.length - 1
    sizes(labels(i) - 1) += 1
next

dim leafs = tree.OrderLeafs()
dim leafHead(4) as string

for i = 0 to 4
    leafHead(i) = leafs(i)
next

call console.WriteLine($"dendrogram: leafs = {tree.Leafs}, leaf order head = {String.Join(", ", leafHead)}")
call console.WriteLine($"hcut(k := {k}) cluster sizes: {String.Join(", ", sizes)}")

' 按照距离阈值切分聚类树（阈值小于该值的时候簇会被合并）
dim byThreshold = dist.hcut(threshold := 3.0)
dim thresholdLabels = byThreshold.ClusterLabels()
dim clusterNumber = 0

for i = 0 to thresholdLabels.length - 1
    if thresholdLabels(i) > clusterNumber then clusterNumber = thresholdLabels(i)
next

call console.WriteLine($"hcut(threshold := 3.0) -> {clusterNumber} clusters")

' ---------------------------------------------------------------------------
' 4. 与真实物种做对比（交叉表）
' ---------------------------------------------------------------------------
for each name in spNames
    dim counts(k - 1) as integer
    dim spId = spNames.IndexOf(name) + 1

    for i = 0 to labels.length - 1
        if sp(i) = spId then counts(labels(i) - 1) += 1
    next

    call console.WriteLine($"  {name}: {String.Join(", ", counts)}")
next

' ---------------------------------------------------------------------------
' 5. 在特征表上做 PCA 降维，并且按照层次聚类的簇编号着色
' ---------------------------------------------------------------------------
dim score = table.pca(maxPC := 2).ScoreTable()
dim class_id(labels.length - 1) as string

for i = 0 to labels.length - 1
    class_id(i) = "cluster " & labels(i)
next

call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "Hierarchical clustering of bezdek-Iris"
    plt.SubTitle = $"average linkage + hcut(k := {k})"
    plt.XLabel = "PC1"
    plt.YLabel = "PC2"
    plt.Plot(DataSerials(score.Feature("PC1"), score.Feature("PC2"), class_id).tolist())
    plt.SavePng("Z:/bezdekIris-hclust.png", 300)
End Using

' ---------------------------------------------------------------------------
' 6. 把层次聚类的簇编号写回到原始特征表之后导出为 csv
'
'    导出布局仍然遵循「行名 + 特征列 + label: 前缀标签列」的约定
' ---------------------------------------------------------------------------
dim result = table.SetLabel("cluster", labels)

call result.WriteCsv("Z:/bezdekIris-hclust.csv")

call console.WriteLine("done: Z:/bezdekIris-hclust.png")
call console.WriteLine("done: Z:/bezdekIris-hclust.csv")
