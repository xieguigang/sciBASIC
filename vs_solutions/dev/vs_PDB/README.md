# PDB Debug Symbol File Reader For Classic And Portable Formats

Reads both classic MSF and Portable PDB debug symbol files and decodes them into one uniform model for sciBASIC# tooling.

## Overview
- Format auto-detection in `PDB.Open`: classic MSF containers (SuperBlock magic) and Portable PDB (PE `MZ` header or standalone `BSJB` metadata root).
- Classic PDB streams: DBI stream (modules, source documents, line numbers), TPI stream (CodeView type records) and the public symbol stream (S_PUB32 and friends).
- Portable PDB: PE/COR20/metadata root walk, `#Pdb` custom stream, document table and sequence points.
- Uniform result model: source documents with checksums/language GUIDs, line-number mappings, symbols and type records, plus a GitHub URL rebasing helper.

## Key Types
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.PDB` — unified entry point; `Open(path)` returns documents, line numbers, symbols and type records.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.MSFReader` — page-based reader for the classic MSF container and its streams.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.PortablePdbReader` — PE/metadata decoder for Portable PDB files.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.DbiReader` — DBI stream parser (modules, source documents, line numbers).
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.TpiReader` — decodes CodeView `LF_*` type records from the TPI stream.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.PublicSymbolReader` — parses the classic public symbol stream.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.Models.SourceDocument` — one referenced source file with checksum, language and optional GitHub URL.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase

Dim pdb As PDB = PDB.Open("D:\build\MyApp.pdb")

For Each doc In pdb.SourceDocuments
    Console.WriteLine(doc.FilePath)
Next

For Each sym In pdb.Symbols
    Console.WriteLine($"{sym.Name} @ {sym.Section}:{sym.Offset}")
Next
```

## Package
- Assembly: `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase`
- TargetFramework: `net10.0`
- Tags: `scibasic;pdb;debug-symbols;portable-pdb;source-mapping`

## License
GPL-3.0-or-later
