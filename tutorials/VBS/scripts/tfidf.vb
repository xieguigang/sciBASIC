#include "Microsoft.VisualBasic.Data.NLP.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Drawing.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.NLP
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Correlations
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing
imports Microsoft.VisualBasic.Data.Framework

Dim docs = New String() {
    "knowledge building needs innovative environments are better at helping their inhabitants explore the adjacent possible",
    "As a basis for evaluating explanations, creative knowledge building weight of evidence is a poor substitute for the first two criteria listed above.",
    "A public idea database makes every passing idea visible to everyone else in the organization and do creative work.",
    "questioning and various disturbances initiate cycles of innovation and creative organization knowledge.",
    "We need some way to ensure knowledge to spread among environments that any notes that are dropped are dropped."
}

Dim tfIdf As New TFIDF
Dim i As i32 = 1

For Each seq As String In docs
    Call tfIdf.Add(++i, seq.StringSplit("\s+"))
Next

Dim N As Integer = docs.Length
Dim dist As Double()() = RectangularArray.Matrix(Of Double)(N, N)

For id As Integer = 1 To tfIdf.N
    Dim v = tfIdf.TfidfVectorizer(id.ToString)
    Dim rd As Double() = New Double(N - 1) {}

    Console.Write(id.ToString() & vbTab)

    For j As Integer = 1 To tfIdf.N
        Dim d = v.SquareDistance(tfIdf.TfidfVectorizer(j.ToString))

        rd(j - 1) = d
        Call Console.Write(d.ToString("F4").PadLeft(8, "0"c) & vbTab)
    Next

    dist(id - 1) = rd.ToArray

    Call Console.WriteLine()
Next

call SkiaDriver.Register()

Using plt As New HeatmapPlot(800, 600, PlotTheme.Light())
    plt.Title = "TF-IDF Document Distance Heatmap"
    plt.Matrix = dist.ToMatrix()
    plt.RowLabels = fieldName("doc", dist.length, sep := "_").toarray()
    plt.ColLabels = fieldName("doc", dist.length, sep := "_").ToArray()
    plt.ColorMap = HeatmapPlot.ColorMapType.Plasma
    plt.ShowValues = False
    plt.Plot()
    plt.SavePng("Z:/tfidf-heatmap.png", 300)
End Using