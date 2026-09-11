# Tabular Frame Serialization with SAS XPORT and ASN.1 Readers

Reads SAS XPORT (XPT) transport files and ASN.1 encoded byte streams, and persists sciBASIC# `DataFrame` objects into a single seekable binary file with a typed column schema.

## Overview
- Binary tabular frame format: `FrameWriter.WriteFrame` stores a magic header, per-column data blocks and a JSON schema trailer; `FrameReader.ReadFrame` restores the `DataFrame` by seeking to each column offset recorded in the schema.
- Typed column schema (`Schema` / `VectorSchema`) records column name, ordinal, `TypeCode`, element count, scalar flag, stream offset and attributes.
- SAS XPORT (XPT) import: `SASXportConverter`, `SASXportFileIterator` and `XPTReaderUtils` decode IBM/IEEE floating point, missing values and header metadata.
- ASN.1 decoding helpers (`ASN1.StreamReader`, `ASN1.Index`) for inspecting tag-length-value encoded byte streams.

## Key Types
- `Microsoft.VisualBasic.Data.IO.FrameReader` — reads a binary data frame file or a SAS XPT file into a `DataFrame`.
- `Microsoft.VisualBasic.Data.IO.FrameWriter` — writes a `DataFrame` plus its column schema to a seekable stream.
- `Microsoft.VisualBasic.Data.IO.Schema` / `VectorSchema` — frame-level and column-level binary layout metadata.
- `Microsoft.VisualBasic.Data.IO.Xpt.SASXportConverter` — converts an XPT transport file into tabular data.
- `Microsoft.VisualBasic.Data.IO.Xpt.SASXportFileIterator` — streams the datasets contained in an XPT file.
- `Microsoft.VisualBasic.Data.IO.Xpt.XPTReaderUtils` — low-level XPT primitives (IBM float conversion, date parsing, missingness).
- `Microsoft.VisualBasic.Data.IO.Xpt.Types.ReadstatType` — XPT/ReadStat column type enumeration.
- `Microsoft.VisualBasic.Data.IO.ASN1.StreamReader` — ASN.1 tag/length/value reader.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.IO

Dim df As DataFrame = FrameReader.ReadFrame("data.frame")
Dim sas As DataFrame = FrameReader.ReadSasXPT("demo.xpt")

Using file As Stream = "copy.frame".Open(FileMode.OpenOrCreate, doClear:=True)
    Call df.WriteFrame(file)
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.Storage`
- TargetFramework: `net10.0`
- Tags: `scibasic;data-storage;sas-xport;asn1;dataframe;binary-serialization`

## License
GPL-3.0-or-later
