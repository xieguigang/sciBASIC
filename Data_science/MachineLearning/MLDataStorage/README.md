# Machine Learning Dataset Storage: DataPack, MNIST and DataFrame Import

Dataset persistence layer for the sciBASIC# machine learning stack: it turns data frames into normalised sample datasets, streams MNIST IDX files, and packs large sample collections into HDSPack archives.

## Overview
- Imports a `DataFrame` into a `DataSet`: non-label columns become feature inputs, the label columns become the output vector, and every feature gets a `SampleDistribution` entry in `NormalizeMatrix`.
- Reads the MNIST IDX image/label file pair (magic numbers 2051 / 2049), validating that both files agree on the sample count, and exposes raw byte vectors, `Bitmap` images or generic `INamedValue`/`IVector`/`IClusterPoint` entities.
- Packs a whole `DataSet` into an HDSPack stream (`/.etc/attributes.json` + `/.etc/dimension.json` metadata, one block per sample, one block per feature), so training can stream instead of holding everything in memory.
- Reads packs back with random access by sample id (`ReadSample`) or full enumeration (`GetAllSamples`), plus the stored normalisation matrix (`GetMatrix`).

## Key Types
- `Microsoft.VisualBasic.MachineLearning.DataStorage.DataImports` — extension method `Imports(df, labels)` converting a data frame into a machine learning `DataSet`.
- `Microsoft.VisualBasic.MachineLearning.DataStorage.MNIST` — forward-only reader over MNIST IDX images and labels (`ExtractVectors`, `ExtractImages`, `ExtractDataSet(Of T)`, `GetImageSize`).
- `Microsoft.VisualBasic.MachineLearning.DataStorage.DataPack.PackWriter` — serialises a `DataSet` into an HDSPack stream.
- `Microsoft.VisualBasic.MachineLearning.DataStorage.DataPack.PackReader` — reads samples, labels and the normalisation matrix back from a pack.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.DataStorage
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.DataPack

' read an MNIST training set
Using mnist As New MNIST("train-images.idx3-ubyte", "train-labels.idx1-ubyte")
    Console.WriteLine(mnist.ImageSize)

    For Each img In mnist.ExtractImages()
        ' img.Name = unique id, img.Description = class label
    Next
End Using

' pack a dataset, then stream it back later
Using out As New PackWriter(New FileStream("trainset.pack", FileMode.Create))
    Call out.WriteDataSet(dataset)
End Using

Using input As New PackReader(New FileStream("trainset.pack", FileMode.Open))
    Dim labels As String() = input.output_labels
    Dim one As Sample = input.ReadSample("sample-id")
    Dim matrix As NormalizeMatrix = input.GetMatrix()
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.DataStorage`
- TargetFramework: `net10.0`
- Tags: `scibasic;machine-learning;dataset;mnist;data-storage`

## License
GPL-3.0-or-later
