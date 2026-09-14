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
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.DataMining.Kmeans
Imports Microsoft.VisualBasic.DataMining.UMAP
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

' ---------------------------------------------------------------------------
' Word2Vec + UMAP + KMeans demo
'
'   Rapunzel text -> Word2Vec training -> word vector table
'    -> UMAP embedding -> KMeans clustering -> scatter plot -> export csv
' ---------------------------------------------------------------------------
dim textfile = here("../../data/Rapunzel.txt")
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

' ---------------------------------------------------------------------------
' 1. Build the trained word vector collection into a unified 2D table object
'    (NumericTable)
'
'    + row names  = the word token
'    + features   = v_1 .. v_n, i.e. the word vector of each token
'
'    All the following operations (dimension reduction, clustering, export) are
'    performed directly on this table
' ---------------------------------------------------------------------------
dim vector = wv.outputVector()
dim tokens = vector.tokens
dim colnames = fieldname("v", n := vector.vectorSize).toarray()
dim rows As Double()() = New Double(tokens.length - 1)() {}

for i = 0 to tokens.length - 1
    dim raw = vector.wordMap(tokens(i))
    dim vals(raw.length - 1) as double

    for j = 0 to raw.length - 1
        vals(j) = CDbl(raw(j))
    next

    rows(i) = vals
next

dim table = NumericTable.FromRows(tokens, rows, colnames)

call console.WriteLine($"word vectors: {table.nsamples} tokens x {table.nfeatures} dims")

' ---------------------------------------------------------------------------
' 2. Use UMAP to reduce the word vectors to 9 dimensions
'
'    The umap method accepts the unified 2D table and returns a new table whose
'    features are the embedding coordinates dim_1..dim_n; the row names and the
'    existing label columns are inherited
' ---------------------------------------------------------------------------
dim manifold = table.umap(dims := 9, neighbors := 64)

call console.WriteLine($"umap embedding: {manifold.nsamples} samples x {manifold.nfeatures} dims")

' ---------------------------------------------------------------------------
' 3. Run KMeans clustering on the reduced table
'
'    The clustering result is written into the label matrix of the table as the
'    label column cluster
' ---------------------------------------------------------------------------
dim clusters = manifold.kmeans(k := 9)

' ---------------------------------------------------------------------------
' 4. Extract the data needed for plotting from the result table
' ---------------------------------------------------------------------------
dim x = manifold.Feature("dim_1")
dim y = manifold.Feature("dim_2")
dim class_id = clusters.ClusterLabels()
dim classes(class_id.length - 1) as string
dim sizes(8) as integer

for i = 0 to class_id.length - 1
    classes(i) = class_id(i).ToString()
    sizes(class_id(i) - 1) += 1
next

call console.WriteLine($"kmeans cluster sizes: {String.Join(", ", sizes)}")

call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "UMAP group of Rapunzel"
    plt.SubTitle = "UMAP scatter of the 'Rapunzel' word2vector embedding result"
    plt.XLabel = "UMAP1"
    plt.YLabel = "UMAP2"
    plt.Plot(DataSerials(x, y, classes).tolist())
    plt.SavePng(here("rapunzel-umap-groups.png"), 300)
End Using

' ---------------------------------------------------------------------------
' 5. Export the clustering result table as csv
'
'    The exported layout also follows the convention of
'    "row names + feature columns + label: prefixed label columns", so it can
'    be loaded back losslessly through NumericTableIO.ReadCsv
' ---------------------------------------------------------------------------
call clusters.WriteCsv(here("rapunzel-umap-groups.csv"))

call console.WriteLine("done: rapunzel-umap-groups.png")
call console.WriteLine("done: rapunzel-umap-groups.csv")
