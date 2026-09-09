#include "Microsoft.VisualBasic.DataMining.UMAP.dll"
#include "Microsoft.VisualBasic.MachineLearning.DataStorage.dll"
#include "Microsoft.VisualBasic.DataMining.Framework.dll"
#include "Microsoft.VisualBasic.Drawing.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Math.Randomizer.dll"

imports Microsoft.VisualBasic.MachineLearning.DataStorage
imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
imports Microsoft.VisualBasic.DataMining.UMAP
imports Microsoft.VisualBasic.DataMining
imports Microsoft.VisualBasic.Scripting.Runtime
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

dim mnist as new MNIST(
    "G:\GCModeller\src\R-sharp\test\demo\machineLearning\umap\mnist_dataset\train-images-idx3-ubyte", 
    "G:\GCModeller\src\R-sharp\test\demo\machineLearning\umap\mnist_dataset\train-labels-idx1-ubyte")
dim dataset = mnist.ExtractDataSet(of ClusterEntity)().toarray()
dim umap as new umap(dimensions := 2,numberOfNeighbors := 128 )
dim n_epochs As Integer = umap.InitializeFit(dataset)
dim number = dataset.ClassId().ascharacter().toarray()

Call umap.Step(n_epochs)

dim manifold = umap.GetEmbedding()
dim x = from v as double() in manifold select v(0)
dim y = from v as double() in manifold select v(1)

call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "MNIST dataset UMAP embedding"
    plt.SubTitle = "UMAP manifold of 2 dimensions"
    plt.XLabel = "UMAP1"
    plt.YLabel = "UMAP2"
    plt.Plot(DataSerials(x:=x.toarray(),y:=y.toarray(), number).tolist())
    plt.SavePng("Z:/mnist-umap.png", 300)
End Using

