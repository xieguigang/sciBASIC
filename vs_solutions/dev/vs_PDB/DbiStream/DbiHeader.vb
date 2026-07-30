''' <summary>
''' Header information carried by the DBI stream.
''' </summary>
Public Class DbiHeader
    Public VersionSignature As Integer
    Public VersionHeader As Integer
    Public Age As Integer
    Public GlobalStreamIndex As UShort
    Public PublicStreamIndex As UShort
    Public SymRecordStreamIndex As UShort
    Public ModInfoSize As Integer
    Public SectionContributionSize As Integer
    Public SectionMapSize As Integer
    Public SourceInfoSize As Integer
    Public TypeServerMapSize As Integer
    Public OptionalDbgHdrSize As Integer
    Public ECSubstreamSize As Integer
    Public Machine As UShort
    Public Property PdbDllVersion As UShort
End Class