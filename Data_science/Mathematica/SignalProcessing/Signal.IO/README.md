# Signal Data IO: netCDF Reader and Writer for Signals

Persists time-series signal collections to netCDF files in sciBASIC#, preserving signal values, measure units, reference ids, per-signal metadata and chunk indices so the data can be exchanged with downstream analysis pipelines.

## Overview
- Writes an `IEnumerable(Of GeneralSignal)` into a netCDF file as contiguous measure and signal buffers plus a chunk-size index.
- Encodes per-signal metadata columns with automatic type inference (byte/int16/i32/i64, f64, flags or JSON).
- Stores measure units and signal reference ids as JSON attribute vectors, with optional netCDF extension types.
- Reads the file back and reconstructs each `GeneralSignal` with its measures, strengths, metadata and measure unit.

## Key Types
- `Microsoft.VisualBasic.Data.Signal.SignalsWriter` — `WriteCDF(signals, file, description, enableCDFExtension)` netCDF writer.
- `Microsoft.VisualBasic.Data.Signal.SignalsReader` — `ReadCDF(netCDFReader)` netCDF reader returning `GeneralSignal` objects.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.Signal
Imports Microsoft.VisualBasic.DataStorage.netCDF

' persist a signal collection
Call signals.WriteCDF("./chromatogram.nc", "LC-MS chromatograms")

' read it back with metadata, chunk indices and measure units
Using reader As New netCDFReader("./chromatogram.nc")
    For Each s In reader.ReadCDF()
        Console.WriteLine($"{s.reference} [{s.measureUnit}] {s.Measures.Length}")
    Next
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.Signal.IO`
- TargetFramework: `net10.0`
- Tags: `scibasic;signal-io;netcdf;time-series;data-exchange;file-format`

## License
GPL-3.0-or-later
