#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ---------------------------------------------------------------------------
' MNIST UMAP demo (standard library edition)
'
'   read.mnist -> unified 2D table -> dimreduce.umap -> plot.scatter -> write.csv
'
' Run:
'     vbs.exe mnist_umap.vb --data=<MNIST dir>
'
' The whole MNIST training set contains 60000 samples and UMAP on the full set
' is very expensive, so only the first ``limit`` samples are used. Without a
' real MNIST folder the deterministic synthetic digit set of ml.syntheticDigits
' is used instead, so the tutorial always runs.
' ---------------------------------------------------------------------------

dim repo_data = ?"--data"
dim limit = 2000
dim images_file = repo_data & "\train-images-idx3-ubyte"
dim labels_file = repo_data & "\train-labels-idx1-ubyte"
dim dataset

if not String.IsNullOrEmpty(repo_data) andalso System.IO.File.Exists(images_file) andalso System.IO.File.Exists(labels_file) then
    call console.WriteLine($"[data] mnist: {images_file}")
    dataset = read.mnist(images_file, labels_file, limit)
else
    call console.WriteLine("[data] no mnist dataset given (--data=<dir>), using the synthetic digit set")
    dataset = ml.syntheticDigits(30, 2024)
end if

' ---------------------------------------------------------------------------
' 1. The handwritten digit samples as a unified 2D table object
'
'    + feature columns = p_1 .. p_784, i.e. the pixel grey levels of the image
'    + label column    = class, i.e. the digit category of every sample
' ---------------------------------------------------------------------------
call console.WriteLine($"mnist table: {dataset.nsamples} samples x {dataset.nfeatures} pixels")

' ---------------------------------------------------------------------------
' 2. Use UMAP to reduce the 784-dimensional pixel data to two dimensions
'
'    The umap function accepts the unified 2D table and returns a new table
'    whose features are the embedding coordinates dim_1..dim_2; the row names
'    and the existing class label column are inherited
' ---------------------------------------------------------------------------
dim manifold = dimreduce.umap(dataset, dims:=2, neighbors:=64)

call console.WriteLine($"umap embedding: {manifold.nsamples} samples x {manifold.nfeatures} dims")

' ---------------------------------------------------------------------------
' 3. Plot: UMAP1/UMAP2 scatter plot coloured by the handwritten digit class
' ---------------------------------------------------------------------------
dim classes = table.classLabelAt(dataset, 0)
dim class_id(classes.length - 1) as string

for i = 0 to classes.length - 1
    class_id(i) = classes(i).ToString()
next

dim figure = plot.scatter(
    table.feature(manifold, "dim_1"), table.feature(manifold, "dim_2"),
    classId:=class_id,
    title:="MNIST dataset UMAP embedding",
    subtitle:="UMAP manifold of 2 dimensions",
    xLabel:="UMAP1", yLabel:="UMAP2", size:="1200,800")

call console.WriteLine("plot: " & plot.save(figure, here("mnist-umap.png")))

' ---------------------------------------------------------------------------
' 4. Export the embedding table as csv
'    (dim_1, dim_2 feature columns + the label:class label column)
' ---------------------------------------------------------------------------
call console.WriteLine("csv : " & write.csv(manifold, here("mnist-umap.csv")))

call console.WriteLine("done: mnist-umap.png")
call console.WriteLine("done: mnist-umap.csv")
