#include "Microsoft.VisualBasic.DataMining.Framework.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Math.Statistics.ANOVA.dll"
#include "Microsoft.VisualBasic.Drawing.dll"

imports microsoft.visualbasic.data
imports microsoft.visualbasic.data.framework
imports microsoft.visualbasic.datamining.kmeans
imports microsoft.visualbasic.datamining
imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

dim file = here("../../data/bezdekIris.csv")
dim k = 3

' ---------------------------------------------------------------------------
' 1. Load the unified 2D table object (NumericTable) directly from the csv file
'
'    The table layout convention is "row names + feature columns + label
'    columns prefixed by label:", so the columns whitelist below loads only the
'    four numeric feature columns D1..D4; the first column of the csv file
'    (the sample id) and the text class column are both ignored
' ---------------------------------------------------------------------------
dim table = NumericTableIO.ReadCsv(file, columns := {"D1","D2","D3","D4"})

call console.WriteLine($"load table: {table.nsamples} samples x {table.nfeatures} features")

' ---------------------------------------------------------------------------
' 2. Run the KMeans clustering
'
'    The clustering result is written into the label matrix of the table as
'    the label column cluster, so the following analysis steps can reuse the
'    very same table
' ---------------------------------------------------------------------------
dim result = table.kmeans(k := k)

' ---------------------------------------------------------------------------
' 3. Run PCA on the clustered result table
'
'    The pca method accepts the unified 2D table and its result is still a
'    MultivariateAnalysisResult object; the ScoreTable extension method then
'    extracts the projected coordinates (scores) into a new table:
'
'    + rows     = samples
'    + features = the PC1, PC2 principal component scores
' ---------------------------------------------------------------------------
dim score = result.pca(maxPC := 2).ScoreTable()

' ---------------------------------------------------------------------------
' 4. Extract the data needed for plotting from the result table
' ---------------------------------------------------------------------------
dim pc1 = score.Feature("PC1")
dim pc2 = score.Feature("PC2")
dim classes = result.ClusterLabels()
dim class_id(classes.length - 1) as string
dim sizes(k - 1) as integer

for i = 0 to classes.length - 1
    class_id(i) = classes(i).ToString()
    sizes(classes(i) - 1) += 1
next

call console.WriteLine($"kmeans cluster sizes: {String.Join(", ", sizes)}")

Call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "PCA group of bezdek-Iris"
    plt.SubTitle = "PCA score scatter with 3 iris species colors"
    plt.XLabel = "PC1"
    plt.YLabel = "PC2"
    plt.Plot(DataSerials(x := pc1, y := pc2, class_id).tolist())
    plt.SavePng(here("bezdekIris-pca-groups.png"), 300)
End Using

' ---------------------------------------------------------------------------
' 5. Export the clustering result table as a csv file
'
'    The exported file layout still follows the convention of
'    "row names + feature columns + label: prefixed label columns", so it can
'    be loaded back losslessly through NumericTableIO.ReadCsv
' ---------------------------------------------------------------------------
call result.WriteCsv(here("bezdekIris-kmeans.csv"))

call console.WriteLine("done: bezdekIris-pca-groups.png")
call console.WriteLine("done: bezdekIris-kmeans.csv")
