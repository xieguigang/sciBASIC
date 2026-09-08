#include "Microsoft.VisualBasic.DataMining.Framework.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Math.Statistics.ANOVA.dll"

imports microsoft.visualbasic.data.framework.storageprovider
imports microsoft.visualbasic.datamining.kmeans
imports microsoft.visualbasic.data.framework
imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA

dim file = "G:\GCModeller\src\R-sharp\REnv\data\bezdekIris.csv"
dim k = 3

dim dataset = DataFrameResolver.LoadDataSet(file, cols := {"D1","D2","D3","D4"}).ToKMeansModels
dim result = dataset.kmeans( expected := k).toarray
dim pca = result.CommonDataSet

call result.saveto("Z:/test.csv")
