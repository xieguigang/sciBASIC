#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ---------------------------------------------------------------------------
' Word2Vec + UMAP + KMeans demo (standard library edition)
'
'   Rapunzel text -> nlp.word2vec -> word vector table
'    -> dimreduce.umap -> cluster.kmeans -> plot.scatter -> write.csv
' ---------------------------------------------------------------------------

dim textfile = here("../../data/Rapunzel.txt")
dim text = read.text(textfile)

' ---------------------------------------------------------------------------
' 1. Train the word vectors and build them into a unified 2D table object
'
'    + row names = the word token
'    + features  = v_1 .. v_n, i.e. the word vector of each token
' ---------------------------------------------------------------------------
dim vectors = nlp.word2vec(text, vectorSize:=30, method:="cbow", freqThreshold:=1, window:=5, threads:=1)

call console.WriteLine($"word vectors: {vectors.nsamples} tokens x {vectors.nfeatures} dims")

' ---------------------------------------------------------------------------
' 2. Use UMAP to reduce the word vectors to 9 dimensions
' ---------------------------------------------------------------------------
dim manifold = vectors.umap(dims:=9, neighbors:=64)

call console.WriteLine($"umap embedding: {manifold.nsamples} samples x {manifold.nfeatures} dims")

' ---------------------------------------------------------------------------
' 3. Run KMeans clustering on the reduced table
' ---------------------------------------------------------------------------
dim clusters = manifold.kmeans(k:= 9)
dim labels = clusters.classLabels( "cluster")

call console.WriteLine($"kmeans cluster sizes: {String.Join(", ", cluster.clusterSizes(labels))}")

' ---------------------------------------------------------------------------
' 4. Scatter UMAP1 / UMAP2, coloured by the cluster id
' ---------------------------------------------------------------------------
dim figure = plot.scatter(
    table.feature(manifold, "dim_1"), table.feature(manifold, "dim_2"),
    classId:=labels .as_string(),
    title:="UMAP group of Rapunzel",
    subtitle:="UMAP scatter of the Rapunzel word2vector embedding result",
    xLabel:="UMAP1", yLabel:="UMAP2", size:="1200,800")

call console.WriteLine("plot: " & plot.save(figure, here("rapunzel-umap-groups.png")))

' ---------------------------------------------------------------------------
' 5. Export the clustering result table as a csv file
' ---------------------------------------------------------------------------
call console.WriteLine("csv : " & write.csv(clusters, here("rapunzel-umap-groups.csv")))

call console.WriteLine("done: rapunzel-umap-groups.png")
call console.WriteLine("done: rapunzel-umap-groups.csv")
