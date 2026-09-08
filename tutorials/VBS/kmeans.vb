#imports "G:\GCModeller\src\runtime\sciBASIC#\.nuget\net10.0\Microsoft.VisualBasic.DataMining.Framework.dll"
#imports "G:\GCModeller\src\runtime\sciBASIC#\.nuget\net10.0\Microsoft.VisualBasic.Data.Framework.dll"

imports microsoft.visualbasic.data.framework.storageprovider
imports microsoft.visualbasic.datamining.kmeans
imports system.collections.generic

dim file = "G:\GCModeller\src\R-sharp\REnv\data\bezdekIris.csv"
dim k = 3

dim dataset = DataFrameResolver.LoadDataSet(file).ToKMeansModels
dim result = dataset.kmeans( expected = k).toarray

call result.saveto("Z:/test.csv")
