#include "Microsoft.VisualBasic.Data.NLP.Word2Vec.dll"
#include "Microsoft.VisualBasic.Data.NLP.dll"
#include "Microsoft.VisualBasic.DataMining.Framework.dll"
#include "Microsoft.VisualBasic.Math.Statistics.ANOVA.dll"
#include "Microsoft.VisualBasic.Drawing.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"

Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.DataMining.Kmeans
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing
Imports Microsoft.VisualBasic.Data.Framework

dim textfile = "G:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Rapunzel.txt"
dim wv As Word2Vec = Word2VecFactory().setVectorSize(100).setMethod(TrainMethod.Skip_Gram).setNumOfThread(1).setFreqThresold(1).build()
Dim data As Paragraph() = Paragraph.Segmentation(textFile.ReadAllText).ToArray

For Each p As Paragraph In data
    For Each line In p.sentences
        Call wv.readTokens(line)
    Next
Next

call wv.training()

dim result = wv.outputVector.AsEnumerable().AsDataSet().CommonDataSet().PrincipalComponentAnalysis(maxPC:= 20).GetPCAScore()
dim clusters = result.Kmeans(k:=6).toarray()
dim class_id = clusters.ClassId().ascharacter().toarray()
dim x = clusters.feature(offset:= 0).toarray()
dim y = clusters.feature(offset:= 1).toarray()

' scatter plot with PC1 and PC2
call SkiaDriver.Register()
call result.add("class_id", class_id)

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "PCA group of Rapunzel"
    plt.SubTitle = "PCA score scatter of the 'Rapunzel' word2vector embedding result"
    plt.XLabel = "PC1"
    plt.YLabel = "PC2"
    plt.Plot(DataSerials(x,y, class_id).tolist())
    plt.SavePng("Z:/rapunzel-pca-groups.png", 300)
End Using

call result.WriteCsv("Z:/rapunzel-pca-groups.csv")