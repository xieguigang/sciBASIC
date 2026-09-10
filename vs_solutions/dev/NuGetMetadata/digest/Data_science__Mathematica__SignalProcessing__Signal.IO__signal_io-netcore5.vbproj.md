# Data_science/Mathematica/SignalProcessing/Signal.IO/signal_io-netcore5.vbproj

- RootNamespace : Microsoft.VisualBasic.Data.Signal
- AssemblyName  : Microsoft.VisualBasic.Data.Signal.IO
- TargetFramework: net10.0
- Source files  : 2
- Existing Title: Signal Data IO: netCDF Reader and Writer for Signals
- Existing Desc : Reads and writes time-series signal collections with their metadata, chunk indices and measure units to netCDF files, interoperating with R-based signal analysis pipelines. Part of sciBASIC#.
- Existing Tags : scibasic;signal-io;netcdf;time-series;data-exchange;file-format

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Data.Signal)

## Public types
- Module SignalsReader (SignalsReader.vb)
- Module SignalsWriter (SignalsWriter.vb) - cdf writer of the signals

## Notable public members
- Public Function ReadCDF(file As netCDFReader) As IEnumerable(Of GeneralSignal)
- Public Function WriteCDF(signals As IEnumerable(Of GeneralSignal), file As String,

## Imports
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.DataStorage.netCDF
- Microsoft.VisualBasic.DataStorage.netCDF.Components
- Microsoft.VisualBasic.DataStorage.netCDF.Data
- Microsoft.VisualBasic.DataStorage.netCDF.DataVector
- Microsoft.VisualBasic.Language.Vectorization
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Math.SignalProcessing
- Microsoft.VisualBasic.Serialization.JSON
- System.Runtime.CompilerServices

## File tree
- SignalsReader.vb
- SignalsWriter.vb

