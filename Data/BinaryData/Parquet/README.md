# Microsoft.VisualBasic.DataStorage.Parquet

A pure VB.NET implementation of the Apache Parquet columnar file format for sciBASIC#, with no native parquet-mr or Arrow dependency.

## Features

- Thrift compact protocol footer metadata, row groups and column statistics
- PLAIN, RLE/bit-packed hybrid, dictionary, DELTA and BYTE_STREAM_SPLIT decoding
- UNCOMPRESSED, SNAPPY and GZIP page compression
- DataPageV1 writing and DataPageV1/V2 reading
- LIST / MAP / STRUCT nested schema shredding and record assembly
- Direct read/write of `Microsoft.VisualBasic.Data.Framework.DataFrame` objects via a `pq` facade

## Usage

```vbnet
Call pq.write_table(df, "data.parquet")
Dim table As ParquetTable = pq.read_table("data.parquet")
```
