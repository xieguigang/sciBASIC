#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ============================================================================
'  TF-IDF document distance tutorial (standard library edition)
'
'  Build a TF-IDF vectorizer over a handful of short documents, compute the
'  pairwise squared Euclidean distance between the document vectors, and render
'  the resulting distance matrix as a heatmap.
'
'  Flow:
'      1) collect the raw documents
'      2) nlp.tfidfTable builds the vectorizer and the N x N distance table
'      3) print the matrix, render it as a heatmap and export it as csv
' ============================================================================

' ---------------------------------------------------------------------------
'  1) A small in-memory corpus; each element is one document.
'     The documents deliberately share the vocabulary of
'     "knowledge / creative / environments", so the TF-IDF distances between
'     them stay meaningful.
' ---------------------------------------------------------------------------
dim docs = New String() {
    "knowledge building needs innovative environments are better at helping their inhabitants explore the adjacent possible",
    "As a basis for evaluating explanations, creative knowledge building weight of evidence is a poor substitute for the first two criteria listed above.",
    "A public idea database makes every passing idea visible to everyone else in the organization and do creative work.",
    "questioning and various disturbances initiate cycles of innovation and creative organization knowledge.",
    "We need some way to ensure knowledge to spread among environments that any notes that are dropped are dropped."
}

' ---------------------------------------------------------------------------
'  2) TF-IDF distance table (rows / columns are doc_1 .. doc_N)
' ---------------------------------------------------------------------------
dim dist = nlp.tfidfTable(docs)
dim matrix = table.matrix(dist)

call console.WriteLine($"vocabulary: {nlp.vocabulary(docs).length} terms, documents: {dist.nsamples}")

for i = 0 to matrix.length - 1
    dim line = ""

    for j = 0 to matrix(i).length - 1
        line &= matrix(i)(j).ToString("F4").PadLeft(9) & " "
    next

    call console.WriteLine($"{dist.rowNames(i)} {line}")
next

' ---------------------------------------------------------------------------
'  3) Render the distance matrix as a heatmap; the Plasma colour map
'     highlights that the diagonal distance is 0 while unrelated documents are
'     far apart.
' ---------------------------------------------------------------------------
dim heatmap = plot.heatmap(
    matrix,
    rowLabels:=table.rowNames(dist),
    colLabels:=table.featureNames(dist),
    title:="TF-IDF Document Distance Heatmap",
    palette:="plasma",
    theme:="light",
    size:="1200,800")

call console.WriteLine("plot: " & plot.save(heatmap, here("tfidf-heatmap.png")))

' ---------------------------------------------------------------------------
'  4) Export the distance matrix through the standard table writer
' ---------------------------------------------------------------------------
call console.WriteLine("csv : " & write.csv(dist, here("tfidf-distance.csv")))

call console.WriteLine("done: tfidf-heatmap.png")
call console.WriteLine("done: tfidf-distance.csv")
