#include "Microsoft.VisualBasic.DataMining.UMAP.dll"
#include "Microsoft.VisualBasic.MachineLearning.DataStorage.dll"
#include "Microsoft.VisualBasic.DataMining.Framework.dll"
#include "Microsoft.VisualBasic.Drawing.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Math.Randomizer.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"

imports Microsoft.VisualBasic.Data
imports Microsoft.VisualBasic.Data.Framework
imports Microsoft.VisualBasic.MachineLearning.DataStorage
imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
imports Microsoft.VisualBasic.DataMining.UMAP
imports Microsoft.VisualBasic.DataMining
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

' MNIST 训练集一共有 60000 个样本，对全量样本做 UMAP 的计算量非常大，
' 因此这个教程脚本只取前面的 limit 个样本进行演示(把 limit 调大即可处理更多样本)
dim limit = 5000

dim mnist as new MNIST(
    "G:\GCModeller\src\R-sharp\test\demo\machineLearning\umap\mnist_dataset\train-images-idx3-ubyte",
    "G:\GCModeller\src\R-sharp\test\demo\machineLearning\umap\mnist_dataset\train-labels-idx1-ubyte")

' ---------------------------------------------------------------------------
' 1. 把 MNIST 的手写数字样本构建为统一的二维表对象(NumericTable)
'
'    + 特征列 = p_1 .. p_784，即图像的像素灰度
'    + 标签列 = class，即每一个样本所对应的数字类别
'
'    后续的降维、聚类、导出操作都直接基于这一张表
' ---------------------------------------------------------------------------
dim rows As Double()() = New Double(limit - 1)() {}
dim classes(limit - 1) as double
dim i = 0

for each digit in mnist.ExtractDataSet(of ClusterEntity)()
    if i >= limit then exit for

    rows(i) = digit.entityVector
    classes(i) = CDbl(digit.cluster)
    i += 1
next

' 实际读取到的样本数量可能少于 limit
if i > 0 andalso i < limit then
    redim preserve rows(i - 1)
    redim preserve classes(i - 1)
end if

dim table = NumericTable.FromRows(
    fieldname("sample", n := rows.length).toarray(),
    rows,
    fieldname("p", n := rows(0).length).toarray())
call table.SetLabel("class", classes)

call console.WriteLine($"mnist table: {table.nsamples} samples x {table.nfeatures} pixels")

' ---------------------------------------------------------------------------
' 2. 使用 UMAP 把 784 维的像素数据降维到二维
'
'    umap 方法接收统一二维表，返回以嵌入坐标 dim_1..dim_2 为特征的新表，
'    行名以及原有的 class 标签列都会被继承下来
' ---------------------------------------------------------------------------
dim manifold = table.umap(dims := 2, neighbors := 128)

call console.WriteLine($"umap embedding: {manifold.nsamples} samples x {manifold.nfeatures} dims")

' ---------------------------------------------------------------------------
' 3. 绘图：UMAP1/UMAP2 散点图，按照手写数字的类别进行着色
' ---------------------------------------------------------------------------
dim x = manifold.Feature("dim_1")
dim y = manifold.Feature("dim_2")
dim number(classes.length - 1) as string

for k = 0 to classes.length - 1
    number(k) = classes(k).ToString()
next

call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "MNIST dataset UMAP embedding"
    plt.SubTitle = "UMAP manifold of 2 dimensions"
    plt.XLabel = "UMAP1"
    plt.YLabel = "UMAP2"
    plt.Plot(DataSerials(x := x, y := y, number).tolist())
    plt.SavePng("Z:/mnist-umap.png", 300)
End Using

' ---------------------------------------------------------------------------
' 4. 把降维结果表导出为 csv（dim_1, dim_2 特征列 + label:class 标签列）
' ---------------------------------------------------------------------------
call manifold.WriteCsv("Z:/mnist-umap.csv")

call console.WriteLine("done: Z:/mnist-umap.png")
call console.WriteLine("done: Z:/mnist-umap.csv")
