#include "Microsoft.VisualBasic.DataMining.Framework.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Math.Statistics.ANOVA.dll"
#include "Microsoft.VisualBasic.Drawing.dll"

imports microsoft.visualbasic.data
imports microsoft.visualbasic.data.framework
imports microsoft.visualbasic.datamining.kmeans
imports microsoft.visualbasic.datamining
imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

dim file = "G:\GCModeller\src\R-sharp\REnv\data\bezdekIris.csv"
dim k = 3

' ---------------------------------------------------------------------------
' 1. 从 csv 文件之中直接加载统一的二维表对象(NumericTable)
'
'    表的布局约定为「行名 + 特征列 + 带 label: 前缀的标签列」，所以这里通过
'    columns 白名单只加载 D1..D4 这四列数值特征列，而 csv 文件之中的第一列
'    (样本ID) 和文本类别列 class 都会被忽略掉
' ---------------------------------------------------------------------------
dim table = NumericTableIO.ReadCsv(file, columns := {"D1","D2","D3","D4"})

call console.WriteLine($"load table: {table.nsamples} samples x {table.nfeatures} features")

' ---------------------------------------------------------------------------
' 2. 执行 KMeans 聚类
'
'    聚类的结果会以标签列 cluster 的形式写入到表的标签矩阵之中，
'    因此后续的分析步骤可以直接共享同一张表
' ---------------------------------------------------------------------------
dim result = table.kmeans(k := k)

' ---------------------------------------------------------------------------
' 3. 对聚类之后的结果表执行 PCA 降维
'
'    pca 方法接收的就是统一二维表，分析结果仍然是 MultivariateAnalysisResult
'    对象，可以再通过 ScoreTable 扩展方法把降维投影结果(得分)抽取为一张新的表：
'
'    + 行    = 样本
'    + 特征  = PC1, PC2 主成分得分
' ---------------------------------------------------------------------------
dim score = result.pca(maxPC := 2).ScoreTable()

' ---------------------------------------------------------------------------
' 4. 从结果表之中取出绘图所需要的数据
' ---------------------------------------------------------------------------
dim pc1 = score.Feature("PC1")
dim pc2 = score.Feature("PC2")
dim classes = result.ClusterLabels()
dim class_id(classes.length - 1) as string
dim sizes(k - 1) as integer

for i = 0 to classes.length - 1
    class_id(i) = classes(i).ToString()
    sizes(classes(i) - 1) += 1
next

call console.WriteLine($"kmeans cluster sizes: {String.Join(", ", sizes)}")

Call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "PCA group of bezdek-Iris"
    plt.SubTitle = "PCA score scatter with 3 iris species colors"
    plt.XLabel = "PC1"
    plt.YLabel = "PC2"
    plt.Plot(DataSerials(x := pc1, y := pc2, class_id).tolist())
    plt.SavePng("Z:/bezdekIris-pca-groups.png", 300)
End Using

' ---------------------------------------------------------------------------
' 5. 把聚类结果表导出为 csv 文件
'
'    导出之后的文件布局仍然遵循「行名 + 特征列 + label: 前缀标签列」的约定，
'    因此可以再次通过 NumericTableIO.ReadCsv 无损地加载回来
' ---------------------------------------------------------------------------
call result.WriteCsv("Z:/bezdekIris-kmeans.csv")

call console.WriteLine("done: Z:/bezdekIris-pca-groups.png")
call console.WriteLine("done: Z:/bezdekIris-kmeans.csv")
