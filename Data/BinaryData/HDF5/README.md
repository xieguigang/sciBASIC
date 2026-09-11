# Pure Managed HDF5 Scientific Data File Reader

Reads HDF5 files without native libraries by decoding superblocks, B-trees, object headers, symbol tables and global heaps, exposing groups, attributes and datasets to sciBASIC# code.

## Overview
- Pure managed decoding of the HDF5 structure layer: `Superblock`, `ObjectHeader` and its messages (dataspace, datatype, data layout, fill value, filter pipeline, link, attribute, group), `LocalHeap`, `GlobalHeap`, `SymbolTableEntry`, `DataBTree` and `GroupBTree`.
- Dataset layouts: contiguous, compact and chunked (V3) storage, with chunk enumeration for streaming large arrays.
- Filter pipeline support including deflate (GZip), shuffle and Fletcher32 checksum, extensible through `IFilter`.
- Dataset readers for primitive, enum and variable-length types, plus `HDF5Sparse` helpers that build sparse matrices without materialising a whole dataset in memory.

## Key Types
- `Microsoft.VisualBasic.Data.IO.HDF5.HDF5File` — opens an HDF5 file, parses the superblock and resolves objects by symbol name.
- `Microsoft.VisualBasic.Data.IO.HDF5.HDF5Reader` — reads a single dataset, exposing `data`, `attributes`, `dataSpace`, `dataType` and `layout`.
- `Microsoft.VisualBasic.Data.IO.HDF5.struct.Superblock` — the file superblock (versions, sizes, root group address).
- `Microsoft.VisualBasic.Data.IO.HDF5.struct.Group` — a group of `DataObjectFacade` entries.
- `Microsoft.VisualBasic.Data.IO.HDF5.dataset.ContiguousDataset` / `CompactDataset` / `ChunkedDatasetV3` — dataset storage layouts.
- `Microsoft.VisualBasic.Data.IO.HDF5.dataset.filters.DeflatePipelineFilter` / `ShufflePipelineFilter` — filter pipeline implementations.
- `Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryFileReader` — low-level, address-based byte reader over the HDF5 file.
- `Microsoft.VisualBasic.Data.IO.HDF5.type.FloatingPoint` / `FixedPoint` / `StringData` / `VariableLength` — HDF5 datatype model.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.IO.HDF5

Using h5 As New HDF5File("sample.h5")
    Dim ds As HDF5Reader = h5("/group/dataset")
    Dim values As Object = ds.data
    Dim attrs As Dictionary(Of String, Object) = ds.attributes
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.IO.HDF5`
- TargetFramework: `net10.0`
- Tags: `scibasic;hdf5;scientific-data;dataset;binary-format;reader`

## License
GPL-3.0-or-later
