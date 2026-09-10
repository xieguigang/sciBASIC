# Data/BinaryData/HDSPack/HDSPack.vbproj

- RootNamespace : Microsoft.VisualBasic.DataStorage.HDSPack
- AssemblyName  : Microsoft.VisualBasic.DataStorage.HDSPack
- TargetFramework: net10.0
- Source files  : 12
- Existing Title: StreamPack Virtual File System in a Single Binary File
- Existing Desc : A packable virtual file system that stores many named streams, folders and attributes inside one binary file. Supports block allocation, buffered read/write, file deletion, metadata trees and read-only mounting for dataset archives.
- Existing Tags : scibasic;hdspack;virtual-filesystem;stream-pack;archive;binary-format

## Namespaces
- FileSystem  [files: 5]

## Public types
- Module PackAttributeData (BinaryStream\PackAttributeData.vb)
- Module TreeParser (BinaryStream\TreeParser.vb)
- Module TreeWriter (BinaryStream\TreeWriter.vb)
- Module Debugger (Debugger.vb)
- Module Extensions (Extensions.vb)
- Class StreamBlock (FileSystem\StreamBlock.vb) - A data file reference
- Class StreamBuffer (FileSystem\StreamBuffer.vb) - an in-memory stream buffer for write new file data <strong>size is limited to 2GB</strong>, use the <see cref="IDisposable.Dispose()"/> method for save the memory data to the underlying stream, and this dispose method
- Class StreamGroup (FileSystem\StreamGroup.vb) - a data folder
- Class StreamPack (FileSystem\StreamPack.vb) - Hierarchical Data Stream Pack, A hdf5 liked file format
- Class AttributeMetadata (Metadata\AttributeMetadata.vb) - a name key tagged attribute metadata.
- Class LazyAttribute (Metadata\LazyAttribute.vb)

## Notable public members
- Public Function GetTypeCodes(registry As Index(Of String)) As Byte()
- Public Iterator Function GetTypeRegistry(buffer As Stream) As IEnumerable(Of NamedValue(Of Integer))
- Public Function Pack(file As StreamObject, type As Index(Of String)) As Byte()
- Public Function Pack(attrs As LazyAttribute, type As Index(Of String)) As Byte()
- Public Function Pack(attrs As AttributeMetadata(), description As String, type As Index(Of String)) As Byte()
- Public Function UnPack(buf As Stream, ByRef desc As String, registry As Dictionary(Of String, String)) As LazyAttribute
- Public Function Parse(buffer As Stream, registry As Dictionary(Of String, String)) As StreamGroup
- Public Function GetBuffer(root As StreamGroup, type As Index(Of String)) As Byte()
- Public Function ListFiles(hds As StreamPack, Optional recursive As Boolean = True) As IEnumerable(Of StreamObject)
- Public Function ListFiles(hds As StreamPack, dir As String, Optional recursive As Boolean = True) As IEnumerable(Of StreamObject)
- Public Iterator Function ListFiles(dir As StreamGroup,
- Public Sub Tree(dir As StreamGroup, text As TextWriter,
- Public Function WriteText(pack As StreamPack,
- Public Function WriteText(pack As StreamPack,
- Public Function ReadText(pack As StreamPack, filename As String, Optional encoding As Encodings = Encodings.UTF8) As String
- Public Function ReadText(pack As StreamPack, file As StreamBlock, Optional encoding As Encodings = Encodings.UTF8) As String
- Public Function LoadStream(pack As StreamPack, file As StreamBlock) As MemoryStream
- Public Function ReadBinary(pack As StreamPack, filename As String) As MemoryStream
- Public Property offset As Long
- Public Property size As Long
- Public ReadOnly Property mimeType As ContentType
- Public ReadOnly Property extensionSuffix As String
- Public ReadOnly Property fullName As String
- Public Overrides Function ToString() As String
- Public Function GetRegion() As BufferRegion
- Public Function StreamSpan(file As Stream) As Stream
- Public Overrides ReadOnly Property CanRead As Boolean
- Public Overrides ReadOnly Property CanSeek As Boolean
- Public Overrides ReadOnly Property CanWrite As Boolean
- Public Overrides ReadOnly Property Length As Long
- Public Overrides Property Position As Long
- Public ReadOnly Property IsPreallocated As Boolean
- Friend Sub New(buffer As Stream,
- Public Overrides Sub Flush()
- Public Overrides Sub SetLength(value As Long)
- Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
- Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
- Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
- Protected Overrides Sub Dispose(disposing As Boolean)
- Public ReadOnly Property totalSize As Long
- Public ReadOnly Property files As StreamObject()
- Public ReadOnly Property dirs As StreamGroup()
- Public Function FilePath(filename As String) As String
- Public Function hasName(nodeName As String) As Boolean
- Public Sub DeleteNode(nodeName As String)
- Public Function GetDataBlock(filepath As FilePath) As StreamBlock
- Public Function GetDataGroup(filepath As FilePath) As StreamGroup
- Public Function GetObject(filepath As FilePath, Optional throw_err As Boolean = True) As StreamObject
- Public Function AddDataBlock(filepath As FilePath) As StreamBlock
- Public Function AddDataGroup(filepath As FilePath) As StreamGroup
- Public Function BlockExists(filepath As FilePath) As Boolean
- Public Overrides Function ToString() As String
- Public Shared Function CreateRootTree() As StreamGroup
- Public ReadOnly Property referencePath As FilePath
- Public ReadOnly Property fileName As String
- Public Property description As String
- Public Property attributes As New LazyAttribute
- Public Function hasAttributes() As Boolean
- Public Function hasAttribute(name As String) As Boolean
- Public Function GetAttribute(name As String) As Object
- ... and 50 more

## Imports
- any = Microsoft.VisualBasic.Scripting
- Microsoft.VisualBasic.ApplicationServices
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.ComponentModel.Ranges.Unit
- Microsoft.VisualBasic.Data.IO
- Microsoft.VisualBasic.Data.IO.MessagePack
- Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
- Microsoft.VisualBasic.FileIO.Path
- Microsoft.VisualBasic.Language.UnixBash
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.My.FrameworkInternal
- Microsoft.VisualBasic.Net.Http
- Microsoft.VisualBasic.Net.Protocols.ContentTypes
- Microsoft.VisualBasic.Text
- Microsoft.VisualBasic.ValueTypes
- System.Data
- System.IO
- System.Reflection
- System.Runtime.CompilerServices
- System.Text

## File tree
- BinaryStream\PackAttributeData.vb
- BinaryStream\TreeParser.vb
- BinaryStream\TreeWriter.vb
- Debugger.vb
- Extensions.vb
- FileSystem\StreamBlock.vb
- FileSystem\StreamBuffer.vb
- FileSystem\StreamGroup.vb
- FileSystem\StreamObject.vb
- FileSystem\StreamPack.vb
- Metadata\AttributeMetadata.vb
- Metadata\LazyAttribute.vb

