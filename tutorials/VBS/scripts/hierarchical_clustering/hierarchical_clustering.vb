#include "Microsoft.VisualBasic.DataMining.HierarchicalClustering.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Math.Statistics.ANOVA.dll"
#include "Microsoft.VisualBasic.Drawing.dll"

imports Microsoft.VisualBasic.Data
imports Microsoft.VisualBasic.Data.Framework
imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

' ---------------------------------------------------------------------------
' Hierarchical clustering demo
'
'   feature table -> distanceMatrix() -> distance matrix table
'    -> hca() dendrogram / hcut() cut into clusters
'    -> compare with the real species -> PCA scatter plot -> export csv
' ---------------------------------------------------------------------------
dim file = here("../../data/bezdekIris.csv")
dim k = 3

' ---------------------------------------------------------------------------
' 1. Load the iris dataset as the unified 2D table format
'    (first column = row names, then the D1..D4 feature columns)
' ---------------------------------------------------------------------------
dim table = NumericTableIO.ReadCsv(file, columns := {"D1","D2","D3","D4"})

' The first column of the iris file is the species name (with duplicates), but
' hierarchical clustering requires unique sample names. So the row names are
' rewritten to unique sample ids here, while the real species is mapped to a
' numeric label column named species and kept on the table.
dim species = table.rowNames
dim spNames as new List(Of String)
dim sp(table.nsamples - 1) as double
dim ids(table.nsamples - 1) as string

for i = 0 to species.length - 1
    if not spNames.Contains(species(i)) then call spNames.Add(species(i))

    sp(i) = spNames.IndexOf(species(i)) + 1
    ids(i) = "sample_" & (i + 1)
next

table.rowNames = ids

call table.SetLabel("species", sp)

call console.WriteLine($"dataset: {table.nsamples} samples x {table.nfeatures} features, {spNames.Count} species")

' ---------------------------------------------------------------------------
' 2. Feature table -> symmetric distance matrix table (Euclidean by default)
' ---------------------------------------------------------------------------
dim dist = table.distanceMatrix()

call console.WriteLine($"distance matrix: {dist.nsamples} x {dist.nfeatures} (square matrix)")

' ---------------------------------------------------------------------------
' 3. Hierarchical clustering
'
'    + hca()  returns the root node of the dendrogram
'    + hcut() cuts the dendrogram into the requested number of clusters and
'      writes the cluster id into the cluster label column
' ---------------------------------------------------------------------------
dim tree = dist.hca()
dim flat = dist.hcut(k := k)
dim labels = flat.ClusterLabels()
dim sizes(k - 1) as integer

for i = 0 to labels.length - 1
    sizes(labels(i) - 1) += 1
next

dim leafs = tree.OrderLeafs()
dim leafHead(4) as string

for i = 0 to 4
    leafHead(i) = leafs(i)
next

call console.WriteLine($"dendrogram: leafs = {tree.Leafs}, leaf order head = {String.Join(", ", leafHead)}")
call console.WriteLine($"hcut(k := {k}) cluster sizes: {String.Join(", ", sizes)}")

' Cut the dendrogram by a distance threshold (clusters are merged while the
' linkage distance is below the threshold)
dim byThreshold = dist.hcut(threshold := 3.0)
dim thresholdLabels = byThreshold.ClusterLabels()
dim clusterNumber = 0

for i = 0 to thresholdLabels.length - 1
    if thresholdLabels(i) > clusterNumber then clusterNumber = thresholdLabels(i)
next

call console.WriteLine($"hcut(threshold := 3.0) -> {clusterNumber} clusters")

' ---------------------------------------------------------------------------
' 4. Compare the clusters with the real species (cross tabulation)
' ---------------------------------------------------------------------------
for each name in spNames
    dim counts(k - 1) as integer
    dim spId = spNames.IndexOf(name) + 1

    for i = 0 to labels.length - 1
        if sp(i) = spId then counts(labels(i) - 1) += 1
    next

    call console.WriteLine($"  {name}: {String.Join(", ", counts)}")
next

' ---------------------------------------------------------------------------
' 5. Run PCA on the feature table and colour the scatter plot by the
'    hierarchical cluster id
' ---------------------------------------------------------------------------
dim score = table.pca(maxPC := 2).ScoreTable()
dim class_id(labels.length - 1) as string

for i = 0 to labels.length - 1
    class_id(i) = "cluster " & labels(i)
next

call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "Hierarchical clustering of bezdek-Iris"
    plt.SubTitle = $"average linkage + hcut(k := {k})"
    plt.XLabel = "PC1"
    plt.YLabel = "PC2"
    plt.Plot(DataSerials(score.Feature("PC1"), score.Feature("PC2"), class_id).tolist())
    plt.SavePng(here("bezdekIris-hclust.png"), 300)
End Using

' ---------------------------------------------------------------------------
' 6. Write the hierarchical cluster ids back onto the original feature table
'    and export it as csv
'
'    The exported layout still follows the convention of
'    "row names + feature columns + label: prefixed label columns"
' ---------------------------------------------------------------------------
dim result = table.SetLabel("cluster", labels)

call result.WriteCsv(here("bezdekIris-hclust.csv"))

call console.WriteLine("done: bezdekIris-hclust.png")
call console.WriteLine("done: bezdekIris-hclust.csv")
