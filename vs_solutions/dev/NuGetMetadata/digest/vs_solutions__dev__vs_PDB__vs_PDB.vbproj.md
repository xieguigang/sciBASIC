# vs_solutions/dev/vs_PDB/vs_PDB.vbproj

- RootNamespace : Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase
- AssemblyName  : Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase
- TargetFramework: net10.0
- Source files  : 17
- Existing Title: PDB Debug Symbol File Reader For Classic And Portable Formats
- Existing Desc : Reads both classic MSF and Portable PDB debug symbol files, decoding source documents, sequence points, line numbers, public symbols and type records into one uniform model for sciBASIC# tooling.
- Existing Tags : scibasic;pdb;debug-symbols;portable-pdb;source-mapping

## Namespaces
- Models  [files: 5]

## Public types
- Module CodeView (CodeView.vb) - CodeView symbol / type record constants and a small helper to walk the length-prefixed record stream used by both the classic PDB public-symbol stream and the TPI type stream.
- Structure CvRecord (CodeView.vb) - A single CodeView record: a 2-byte length (covering <see cref="Type"/> + <see cref="Payload"/>), the 2-byte record <see cref="Type"/>, then the payload bytes.
- Class DbiHeader (DbiStream\DbiHeader.vb) - Header information carried by the DBI stream.
- Class DbiReader (DbiStream\DbiStream.vb) - Parses the DBI stream (stream #3) of a classic PDB: it exposes the module list, the referenced source documents, and (best-effort) the line-number information.
- Class ModuleInfo (DbiStream\ModuleInfo.vb) - One module entry from the module-info substream.
- Module Extensions (Extensions.vb) - Extension helpers for the unified <see cref="PDB"/> model.
- Enum SymbolKind (Models\Flags.vb) - Classification of a <see cref="Symbol"/>.
- Enum TypeKind (Models\Flags.vb) - Classification of a <see cref="TypeRecord"/>.
- Module LanguageGuids (Models\Flags.vb) - Well-known language GUIDs used by source documents.
- Class LineInfo (Models\LineInfo.vb) - A line-number / sequence-point mapping between a method and a source document.
- Class SourceDocument (Models\SourceDocument.vb) - A source file referenced by the debug symbols (the ``*.vb`` / ``*.cs`` / ``*.cpp`` source that was compiled into the binary).
- Class Symbol (Models\Symbol.vb) - A single symbol (function / public / data) extracted from the symbol stream.
- Class TypeRecord (Models\TypeRecord.vb) - A type record decoded from the TPI stream (classic PDB) or the metadata type tables (Portable PDB).
- Class MSFReader (MSF.vb) - Reader for the classic MSF (multi-stream file) PDB container, produced by Visual C++ / .NET Framework builds. The file is organised as a fixed-size page store; the header (SuperBlock) describes the page size and the location
- Class PDB (PDB.vb) - Unified entry point for reading PDB debug-symbol files. <see cref="Open"/> inspects the file header and dispatches to the classic MSF reader (SuperBlock magic) or the Portable PDB reader (DOS <c>MZ</c> header), then aggregates the result into one uniform debug-information model.
- Enum FormatKind (PDB.vb)
- Class PdbStreamInfo (PdbStreamInfo.vb) - Header information decoded from the PDB stream (stream #1).
- Class PortablePdbReader (PortablePdb.vb) - Reader for Portable PDB files. A Portable PDB is a PE file (starting with the DOS <c>MZ</c> header) that carries the CLI metadata in the <c>#~</c> table stream together with the custom <c>#Pdb</c> stream. This reader walks the PE / COR20 / metadata root, locates the
- Class Stream (Stream.vb) - Sub file for multiple stream pdb file. Each stream in the PDB occupies several pages, which aren't necessarily consecutively numbered. The stream has a number and a length. The stream content is the concatenation of its pages,
- Class PublicSymbolReader (SymbolStream.vb) - Parses the public-symbol stream of a classic PDB. The stream is located through <see cref="DbiReader.Header.PublicStreamIndex"/> (falling back to stream #0 for the old format) and contains CodeView symbol records (S_PUB32 and friends).
- Class TpiReader (TpiReader.vb) - Parses the TPI (type information) stream (stream #2) of a classic PDB and decodes the CodeView type records (<c>LF_*</c> leaves) into <see cref="TypeRecord"/> objects. Type ids start at <c>TypeIndexBegin</c> (usually 0x1000); each leaf record consumes one id.

## Notable public members
- Public Const S_CONSTANT As UShort = &H1107
- Public Const S_UDT As UShort = &H1108
- Public Const S_LDATA32 As UShort = &H110D
- Public Const S_GDATA32 As UShort = &H110C
- Public Const S_PUB32 As UShort = &H110E
- Public Const S_LPROC32 As UShort = &H110F
- Public Const S_GPROC32 As UShort = &H1110
- Public Const S_REGREL32 As UShort = &H1111
- Public Const S_LTHREAD32 As UShort = &H1112
- Public Const S_GTHREAD32 As UShort = &H1113
- Public Const S_PROCREF As UShort = &H1125
- Public Const S_LPROCREF As UShort = &H1126
- Public Const LF_MODIFIER As UShort = &H1001
- Public Const LF_POINTER As UShort = &H1002
- Public Const LF_PROCEDURE As UShort = &H1008
- Public Const LF_ARGLIST As UShort = &H1201
- Public Const LF_FIELDLIST As UShort = &H1203
- Public Const LF_ARRAY As UShort = &H1503
- Public Const LF_CLASS As UShort = &H1504
- Public Const LF_STRUCTURE As UShort = &H1505
- Public Const LF_UNION As UShort = &H1506
- Public Const LF_ENUM As UShort = &H1507
- Public Const LF_VTSHAPE As UShort = &H1509
- Public Const LF_FUNC_ID As UShort = &H1601
- Public Const LF_MFUNC_ID As UShort = &H1602
- Public Const LF_BUILDINFO As UShort = &H1603
- Public Const LF_STRING_ID As UShort = &H1605
- Public Iterator Function Enumerate(data As Byte(), offset As Integer, length As Integer) As IEnumerable(Of CvRecord)
- Public Function ReadNullString(data As Byte(), offset As Integer, encoding As Encoding) As String
- Public Property PdbDllVersion As UShort
- Public Property Header As DbiHeader
- Public ReadOnly Property Modules As New List(Of ModuleInfo)()
- Public ReadOnly Property SourceDocuments As New List(Of SourceDocument)()
- Public ReadOnly Property LineNumbers As New List(Of LineInfo)()
- Friend Shared Function ReadNullString(data As Byte(), offset As Integer) As String
- Public Function PointLocal2Github(pdb As PDB, userName$, repoName$, commitID$) As PDB
- Public Function PointLocal2Github(pdb As PDB, userName$, repoName$, commitID$, localRoot As String) As PDB
- Public Function TryParse(guidText As String, ByRef value As Guid) As Boolean
- Public Property Document As SourceDocument
- Public Property Offset As Long
- Public Property MethodName As String
- Public Property StartLine As Integer
- Public Property EndLine As Integer
- Public Property StartColumn As Integer
- Public Property EndColumn As Integer
- Public Overrides Function ToString() As String
- Public Property FilePath As String
- Public Property GitHubUrl As String
- Public Property Language As Guid
- Public Property HashAlgorithm As Guid
- Public Property Checksum As Byte()
- Public ReadOnly Property LanguageName As String
- Public Overrides Function ToString() As String
- Public Property Name As String
- Public Property Section As UShort
- Public Property Offset As UInteger
- Public Property Length As UInteger
- Public Property Kind As SymbolKind
- Public Property Flags As UShort
- Public Overrides Function ToString() As String
- ... and 51 more

## Imports
- Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.Models
- Microsoft.VisualBasic.Data.IO
- std = System.Math
- System.IO
- System.Runtime.CompilerServices
- System.Text

## File tree
- CodeView.vb
- DbiStream\DbiHeader.vb
- DbiStream\DbiStream.vb
- DbiStream\ModuleInfo.vb
- Extensions.vb
- Models\Flags.vb
- Models\LineInfo.vb
- Models\SourceDocument.vb
- Models\Symbol.vb
- Models\TypeRecord.vb
- MSF.vb
- PDB.vb
- PdbStreamInfo.vb
- PortablePdb.vb
- Stream.vb
- SymbolStream.vb
- TpiReader.vb

