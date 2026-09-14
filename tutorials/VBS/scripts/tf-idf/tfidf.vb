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

' ============================================================================
'  TF-IDF document distance tutorial
'
'  Build a TF-IDF vectorizer over a handful of short documents, compute the
'  pairwise squared Euclidean distance between the document vectors, and render
'  the resulting distance matrix as a heatmap.
'
'  Run:
'      vbs.exe tutorials\VBS\tf-idf\tfidf.vb
'
'  Flow:
'      1) collect the raw documents
'      2) feed every document into the TF-IDF index (id + token stream)
'      3) compute the N x N pairwise squared distance matrix
'      4) render the distance matrix as a heatmap and save it to PNG
' ============================================================================

' ---------------------------------------------------------------------------
'  1) A small in-memory corpus; each element is one document.
'     The documents deliberately share the vocabulary of
'     "knowledge / creative / environments", so the TF-IDF distances between
'     them stay meaningful.
' ---------------------------------------------------------------------------
Dim docs = New String() {
    "knowledge building needs innovative environments are better at helping their inhabitants explore the adjacent possible",
    "As a basis for evaluating explanations, creative knowledge building weight of evidence is a poor substitute for the first two criteria listed above.",
    "A public idea database makes every passing idea visible to everyone else in the organization and do creative work.",
    "questioning and various disturbances initiate cycles of innovation and creative organization knowledge.",
    "We need some way to ensure knowledge to spread among environments that any notes that are dropped are dropped."
}

' ---------------------------------------------------------------------------
'  2) Build the TF-IDF index.
'
'     * ``Add`` takes a numeric document id together with the token sequence of
'       that document
'     * ``StringSplit`` applies the regular expression "\s+" so that each
'       sentence is split into words on any whitespace run
'     * document ids start at 2 because ``++i`` is evaluated before the call
' ---------------------------------------------------------------------------
Dim tfIdf As New TFIDF
Dim i As i32 = 1

For Each seq As String In docs
    Call tfIdf.Add(++i, seq.StringSplit("\s+"))
Next

' ---------------------------------------------------------------------------
'  3) Pairwise document distance matrix.
'
'     * ``N`` is the number of documents
'     * ``RectangularArray.Matrix`` allocates an N x N jagged matrix
'     * ``TfidfVectorizer(id)`` returns the TF-IDF vector of a document
'     * ``SquareDistance`` compares two vectors
'     The matrix is symmetric with a zero diagonal, since the distance of a
'     document to itself is 0.
' ---------------------------------------------------------------------------
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

' ---------------------------------------------------------------------------
'  4) Render the distance matrix as a heatmap.
'
'     Row and column labels are generated as doc_1 .. doc_N by ``fieldName``;
'     the Plasma colour map highlights that the diagonal distance is 0 while
'     unrelated documents are far apart.
' ---------------------------------------------------------------------------
call SkiaDriver.Register()

Using plt As New HeatmapPlot(800, 600, PlotTheme.Light())
    plt.Title = "TF-IDF Document Distance Heatmap"
    plt.Matrix = dist.ToMatrix()
    plt.RowLabels = fieldName("doc", dist.length, sep := "_").toarray()
    plt.ColLabels = fieldName("doc", dist.length, sep := "_").ToArray()
    plt.ColorMap = HeatmapPlot.ColorMapType.Plasma
    plt.ShowValues = False
    plt.Plot()
    plt.SavePng(here("tfidf-heatmap.png"), 300)
End Using
