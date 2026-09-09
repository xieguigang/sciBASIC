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
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.DataMining.Kmeans
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing
Imports Microsoft.VisualBasic.Data.Framework
imports Microsoft.VisualBasic.linq
imports Microsoft.VisualBasic.scripting.runtime
imports Microsoft.VisualBasic.DataMining.UMAP

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

dim vector = wv.outputVector()
dim umap as new umap(dimensions := 9,numberOfNeighbors := 64 )
dim n_epochs = umap.InitializeFit( vector.AsEnumerable().asdataset().toarray())

Call umap.Step(n_epochs)

dim clusters = umap.AsDataSet(labels:=vector.tokens).Kmeans(k:=9).toarray()
dim class_id = clusters.ClassId().ascharacter().toarray()
dim x = clusters.feature(offset:= 0).toarray()
dim y = clusters.feature(offset:= 1).toarray()
dim result = clusters.as_dataframe(colnames:= fieldname("v", n:=vector.words).toarray())

' scatter plot with UMAP1 and UMAP2
call SkiaDriver.Register()
call result.add("class_id", class_id)

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "UMAP group of Rapunzel"
    plt.SubTitle = "UMAP scatter of the 'Rapunzel' word2vector embedding result"
    plt.XLabel = "UMAP1"
    plt.YLabel = "UMAP2"
    plt.Plot(DataSerials(x,y, class_id).tolist())
    plt.SavePng("Z:/rapunzel-umap-groups.png", 300)
End Using

call result.WriteCsv("Z:/rapunzel-umap-groups.csv")