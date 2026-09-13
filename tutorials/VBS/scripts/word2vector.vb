#include "Microsoft.VisualBasic.Data.NLP.Word2Vec.dll"
#include "Microsoft.VisualBasic.Data.NLP.dll"
#include "Microsoft.VisualBasic.DataMining.Framework.dll"
#include "Microsoft.VisualBasic.Drawing.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Math.Randomizer.dll"
#include "Microsoft.VisualBasic.DataMining.UMAP.dll"

Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.DataMining.Kmeans
Imports Microsoft.VisualBasic.DataMining.UMAP
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

dim textfile = "G:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Rapunzel.txt"
dim wv As Word2Vec = BuildWord2VecFactory() _
    .setVectorSize(30) _
    .setMethod(TrainMethod.CBow) _
    .setNumOfThread(1) _
    .setFreqThresold(1) _
    .build()
Dim data As Paragraph() = Paragraph.Segmentation(textFile.ReadAllText).ToArray

For Each p As Paragraph In data
    For Each line In p.sentences
        Call wv.readTokens(line)
    Next
Next

call wv.training()

' ---------------------------------------------------------------------------
' 1. 把训练出来的词向量集合构建为统一的二维表对象(NumericTable)
'
'    + 行名    = 词 token
'    + 特征列  = v_1 .. v_n，即每一个词的词向量
'
'    后续所有的降维、聚类、导出操作都直接基于这张表进行
' ---------------------------------------------------------------------------
dim vector = wv.outputVector()
dim tokens = vector.tokens
dim colnames = fieldname("v", n := vector.vectorSize).toarray()
dim rows As Double()() = New Double(tokens.length - 1)() {}

for i = 0 to tokens.length - 1
    dim raw = vector.wordMap(tokens(i))
    dim vals(raw.length - 1) as double

    for j = 0 to raw.length - 1
        vals(j) = CDbl(raw(j))
    next

    rows(i) = vals
next

dim table = NumericTable.FromRows(tokens, rows, colnames)

call console.WriteLine($"word vectors: {table.nsamples} tokens x {table.nfeatures} dims")

' ---------------------------------------------------------------------------
' 2. 使用 UMAP 把词向量降维到 9 维
'
'    umap 方法接收统一二维表，并且返回以嵌入坐标 dim_1..dim_n 为特征的新表，
'    行名以及原有的标签列都会被继承下来
' ---------------------------------------------------------------------------
dim manifold = table.umap(dims := 9, neighbors := 64)

call console.WriteLine($"umap embedding: {manifold.nsamples} samples x {manifold.nfeatures} dims")

' ---------------------------------------------------------------------------
' 3. 对降维之后的表执行 KMeans 聚类
'
'    聚类的结果会以标签列 cluster 的形式写入到表的标签矩阵之中
' ---------------------------------------------------------------------------
dim clusters = manifold.kmeans(k := 9)

' ---------------------------------------------------------------------------
' 4. 从结果表之中取出绘图所需要的数据
' ---------------------------------------------------------------------------
dim x = manifold.Feature("dim_1")
dim y = manifold.Feature("dim_2")
dim class_id = clusters.ClusterLabels()
dim classes(class_id.length - 1) as string
dim sizes(8) as integer

for i = 0 to class_id.length - 1
    classes(i) = class_id(i).ToString()
    sizes(class_id(i) - 1) += 1
next

call console.WriteLine($"kmeans cluster sizes: {String.Join(", ", sizes)}")

call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "UMAP group of Rapunzel"
    plt.SubTitle = "UMAP scatter of the 'Rapunzel' word2vector embedding result"
    plt.XLabel = "UMAP1"
    plt.YLabel = "UMAP2"
    plt.Plot(DataSerials(x, y, classes).tolist())
    plt.SavePng("Z:/rapunzel-umap-groups.png", 300)
End Using

' ---------------------------------------------------------------------------
' 5. 把聚类结果表导出为 csv
'
'    导出布局同样遵循「行名 + 特征列 + label: 前缀标签列」的约定，
'    可以再次通过 NumericTableIO.ReadCsv 无损加载回来
' ---------------------------------------------------------------------------
call clusters.WriteCsv("Z:/rapunzel-umap-groups.csv")

call console.WriteLine("done: Z:/rapunzel-umap-groups.png")
call console.WriteLine("done: Z:/rapunzel-umap-groups.csv")
