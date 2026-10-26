#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ---------------------------------------------------------------------------
' Hierarchical clustering demo (standard library edition)
'
'   feature table -> cluster.distanceMatrix -> cluster.hca / cluster.hcut
'    -> compare with the real species -> PCA scatter -> export csv
' ---------------------------------------------------------------------------

dim file = here("../../data/bezdekIris.csv")
dim k = 3

' ---------------------------------------------------------------------------
' 1. Load the iris dataset as the unified 2D table format
' ---------------------------------------------------------------------------
dim dataset = read.csv(file, columns:={"D1", "D2", "D3", "D4"})

' The first column of the iris file is the species name (with duplicates), but
' hierarchical clustering requires unique sample names. The real species is
' therefore mapped to a numeric label column named species, and the row names
' are rewritten to unique sample ids.
dim species = table.rowNames(dataset)
dim sp_names As New List(Of String)
dim sp(dataset.nsamples - 1) as double

for i = 0 to species.length - 1
    if not sp_names.Contains(species(i)) then call sp_names.Add(species(i))

    sp(i) = sp_names.IndexOf(species(i)) + 1
next

call table.setLabel(dataset, "species", sp)
call table.uniqueRowNames(dataset, "sample_")

call console.WriteLine($"dataset: {dataset.nsamples} samples x {dataset.nfeatures} features, {sp_names.Count} species")

' ---------------------------------------------------------------------------
' 2. Feature table -> symmetric distance matrix table (Euclidean by default)
' ---------------------------------------------------------------------------
dim dist = cluster.distanceMatrix(dataset)

call console.WriteLine($"distance matrix: {dist.nsamples} x {dist.nfeatures} (square matrix)")

' ---------------------------------------------------------------------------
' 3. Hierarchical clustering
'
'    + hca()  returns the root node of the dendrogram
'    + hcut() cuts the dendrogram into the requested number of clusters and
'      writes the cluster id into the cluster label column
' ---------------------------------------------------------------------------
dim tree = cluster.hca(dist)
dim flat = cluster.hcut(dist, k)
dim labels = table.classLabels(flat)

call console.WriteLine($"dendrogram: leafs = {cluster.leafCount(tree)}")
call console.WriteLine($"hcut(k := {k}) cluster sizes: {String.Join(", ", cluster.clusterSizes(labels))}")

' Cut the dendrogram by a distance threshold (clusters are merged while the
' linkage distance is below the threshold)
dim by_threshold = cluster.hcutByDistance(dist, 3.0)
dim threshold_number = cluster.clusterSizes(table.classLabels(by_threshold)).length

call console.WriteLine($"hcut(threshold := 3.0) -> {threshold_number} clusters")

' ---------------------------------------------------------------------------
' 4. Compare the clusters with the real species (cross tabulation)
' ---------------------------------------------------------------------------
dim cross = cluster.crossTable(table.classLabelAt(dataset, 0), labels)
dim cross_values = table.matrix(cross)

for i = 0 to cross.nsamples - 1
    call console.WriteLine($"  {sp_names(i)}: {String.Join(", ", cross_values(i))}")
next

' ---------------------------------------------------------------------------
' 5. Run PCA on the feature table and colour the scatter plot by the
'    hierarchical cluster id
' ---------------------------------------------------------------------------
dim score = dimreduce.pcaScore(dataset, 2)
dim class_id(labels.length - 1) as string

for i = 0 to labels.length - 1
    class_id(i) = "cluster " & labels(i)
next

dim figure = plot.scatter(
    table.feature(score, "PC1"), table.feature(score, "PC2"),
    classId:=class_id,
    title:="Hierarchical clustering of bezdek-Iris",
    subtitle:=$"average linkage + hcut(k := {k})",
    xLabel:="PC1", yLabel:="PC2", size:="1200,800")

call console.WriteLine("plot: " & plot.save(figure, here("bezdekIris-hclust.png")))

' ---------------------------------------------------------------------------
' 6. Export the hierarchical cluster ids together with the original table
' ---------------------------------------------------------------------------
call table.setClassLabel(dataset, "cluster", labels)
call console.WriteLine("csv : " & write.csv(dataset, here("bezdekIris-hclust.csv")))

call console.WriteLine("done: bezdekIris-hclust.png")
call console.WriteLine("done: bezdekIris-hclust.csv")
