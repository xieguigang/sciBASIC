#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ---------------------------------------------------------------------------
' KMeans + PCA demo (standard library edition)
'
'   read.csv -> cluster.kmeans -> dimreduce.pcaScore
'    -> plot.scatter -> write.csv
'
' Both engine flavours share this exact api, see kmeans.js.
' ---------------------------------------------------------------------------

dim file = here("../../data/bezdekIris.csv")
dim k = 3

' ---------------------------------------------------------------------------
' 1. Load the unified 2D table object (NumericTable) directly from the csv file
'
'    The columns whitelist loads only the four numeric feature columns D1..D4;
'    the sample id column and the text class column are both ignored
' ---------------------------------------------------------------------------
dim dataset = read.csv(file, columns:={"D1", "D2", "D3", "D4"})

call console.WriteLine($"load table: {dataset.nsamples} samples x {dataset.nfeatures} features")

' ---------------------------------------------------------------------------
' 2. Run the KMeans clustering
'
'    The clustering result is written into the label matrix of the table as the
'    label column cluster, so every following step reuses the very same table
' ---------------------------------------------------------------------------
dim clustered = cluster.kmeans(dataset, k)
dim labels = table.classLabels(clustered)

call console.WriteLine($"kmeans cluster sizes: {String.Join(", ", cluster.clusterSizes(labels))}")

' ---------------------------------------------------------------------------
' 3. Run PCA on the clustered table and take the score matrix
'
'    + rows     = samples
'    + features = the PC1, PC2 principal component scores
' ---------------------------------------------------------------------------
dim score = dimreduce.pcaScore(clustered, 2)
dim pc1 = table.feature(score, "PC1")
dim pc2 = table.feature(score, "PC2")
dim class_id(labels.length - 1) as string

for i = 0 to labels.length - 1
    class_id(i) = labels(i).ToString()
next

' ---------------------------------------------------------------------------
' 4. Scatter the PCA scores, coloured by the kmeans cluster id
' ---------------------------------------------------------------------------
dim figure = plot.scatter(
    pc1, pc2,
    classId:=class_id,
    title:="PCA group of bezdek-Iris",
    subtitle:="PCA score scatter with 3 iris species colors",
    xLabel:="PC1", yLabel:="PC2", size:="1200,800")

call console.WriteLine("plot: " & plot.save(figure, here("bezdekIris-pca-groups.png")))

' ---------------------------------------------------------------------------
' 5. Export the clustering result table as a csv file
'
'    The layout still follows "row names + feature columns + label: prefixed
'    label columns", so it can be loaded back losslessly through read.csv
' ---------------------------------------------------------------------------
call console.WriteLine("csv : " & write.csv(clustered, here("bezdekIris-kmeans.csv")))

call console.WriteLine("done: bezdekIris-pca-groups.png")
call console.WriteLine("done: bezdekIris-kmeans.csv")
