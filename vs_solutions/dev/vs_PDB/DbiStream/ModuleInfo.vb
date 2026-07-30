
''' <summary>
''' One module entry from the module-info substream.
''' </summary>
Public Class ModuleInfo
    Public ModuleName As String
    Public ObjFileName As String
    ''' <summary>Indices into the source-info substream file table.</summary>
    Public FileIndices As Integer()
    ''' <summary>Offset of this module's C13 line info, relative to the DBI debug-data substream.</summary>
    Public C13Offset As Integer
    Public C13Size As Integer
    ''' <summary>Offset/length of this module's symbols within the symbol stream.</summary>
    Public SymbolOffset As Integer
    Public SymbolSize As Integer
End Class