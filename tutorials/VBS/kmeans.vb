#include "Microsoft.VisualBasic.DataMining.Framework.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Math.Statistics.ANOVA.dll"

imports microsoft.visualbasic.data.framework.storageprovider
imports microsoft.visualbasic.datamining.kmeans
imports microsoft.visualbasic.datamining
imports microsoft.visualbasic.data.framework
imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
imports microsoft.visualbasic.data.plots

dim file = "G:\GCModeller\src\R-sharp\REnv\data\bezdekIris.csv"
dim k = 3

dim dataset = DataFrameResolver.LoadDataSet(file, cols := {"D1","D2","D3","D4"}).ToKMeansModels()
dim result = dataset.kmeans( expected := k).toarray()
dim pca = result.CommonDataSet.PrincipalComponentAnalysis(maxPC := 2).GetPCAScore()
dim class_id = result.ClassId().ToArray()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "PCA group of bezdek-Iris"
    plt.SubTitle = "Two groups with linear trend"
    plt.XLabel = "PC1"
    plt.YLabel = "PC2"
    plt.Plot(DataSerials(x:=pca!PC1,y:=pca!PC2, class_id).tolist())
    plt.SavePng("Z:/bezdekIris-pca-groups.png", 300)
End Using

call result.saveto("Z:/test.csv")
