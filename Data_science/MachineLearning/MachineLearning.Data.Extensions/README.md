# Q-Learning and Labelled Dataset Serialization Extensions

Small helper layer for sciBASIC# machine learning data: persisting trained Q-learning results and labelled clustering matrices.

## Overview
- Exports a trained Q-table together with its state feature vectors to netCDF, storing the learning hyper-parameters as global attributes and one variable per state feature / Q-value column.
- Records Q-value curves per training iteration into a dictionary of index curves that can be written out as a delimited text/CSV table.
- Saves and loads labelled clustering matrices (`EntityClusterModel` rows with ID, cluster and feature vector) in a compact big-endian binary layout, with the feature names stored as a B-encoded header.
- Maps a CSV/dataframe row onto the normalised input vector expected by a trained `DataSet` model.

## Key Types
- `Microsoft.VisualBasic.MachineLearning.Data.Extensions` — `ExportQTable(Q, features, file)` (netCDF) and `GetInput(dataset, row)`.
- `Microsoft.VisualBasic.MachineLearning.Data.QTableDump` — `Dump(table, iteration)` accumulates curves, `Save(path)` writes them out.
- `Microsoft.VisualBasic.MachineLearning.Data.LabeledData` — `SaveLabelData(data, buffer)` / `LoadLabelData(buffer)` for labelled cluster matrices.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.Data

' dump a trained Q table with its state features into netCDF
Using file As Stream = "qtable.nc".Open(FileMode.OpenOrCreate)
    Call Q.ExportQTable(features, file)
End Using

' persist labelled clustering results
Using file As Stream = "clusters.dat".Open(FileMode.OpenOrCreate)
    Call clusters.SaveLabelData(file)
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.Data.Extensions` (RootNamespace `Microsoft.VisualBasic.MachineLearning.Data`)
- TargetFramework: `net10.0`
- Tags: `scibasic;machine-learning;q-learning;dataset;serialization`

## License
GPL-3.0-or-later
