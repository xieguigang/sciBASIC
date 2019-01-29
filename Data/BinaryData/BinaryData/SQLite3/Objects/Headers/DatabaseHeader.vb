Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Internal
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums

Namespace ManagedSqlite.Core.Objects.Headers
    Public Class DatabaseHeader
        Public Const HeaderSize As Integer = 100

        ''' <summary>
        ''' SQLite format 3
        ''' </summary>
        Private Shared ReadOnly _expectedHeader As Byte() = "SQLite format 3".Select(AddressOf Asc).Select(Function(i) CByte(i)).AsList + 0

        Public Property PageSize() As UShort

        Public Property WriteVersion() As FileWriteVersion

        Public Property ReadVersion() As FileReadVersion

        Public Property ReservedSpaceAtEndOfPage() As Byte

        Public Property MaximumEmbeddedPayloadFraction() As Byte

        Public Property MinimumEmbeddedPayloadFraction() As Byte

        Public Property LeafPayloadFraction() As Byte

        Public Property ChangeCounter() As UInteger

        Public Property DatabaseSizeInPages() As UInteger

        Public Property FirstFreelistTrunkPage() As UInteger

        Public Property FreeListPages() As UInteger

        Public Property SchemaCookie() As UInteger

        Public Property SchemaFormat() As UInteger

        Public Property DefaultPageCacheSize() As UInteger

        Public Property Value7() As UInteger

        Public Property TextEncoding() As SqliteEncoding

        Public Property UserVersion() As UInteger

        Public Property IncrementalVacuumMode() As UInteger

        Public Property ApplicationId() As UInteger

        Public Property VersionValidFor() As UInteger

        Public Property Version() As UInteger

        Friend Shared Function Parse(reader As ReaderBase) As DatabaseHeader
            ' The header is 100 bytes
            reader.CheckSize(100)

            reader.CheckMagicBytes(_expectedHeader)

            ' Read header
            Dim res As New DatabaseHeader()

            res.PageSize = reader.ReadUInt16()
            res.WriteVersion = CType(reader.ReadByte(), FileWriteVersion)
            res.ReadVersion = CType(reader.ReadByte(), FileReadVersion)
            res.ReservedSpaceAtEndOfPage = reader.ReadByte()
            res.MaximumEmbeddedPayloadFraction = reader.ReadByte()
            res.MinimumEmbeddedPayloadFraction = reader.ReadByte()
            res.LeafPayloadFraction = reader.ReadByte()
            res.ChangeCounter = reader.ReadUInt32()

            res.DatabaseSizeInPages = reader.ReadUInt32()
            res.FirstFreelistTrunkPage = reader.ReadUInt32()
            res.FreeListPages = reader.ReadUInt32()
            res.SchemaCookie = reader.ReadUInt32()
            res.SchemaFormat = reader.ReadUInt32()
            res.DefaultPageCacheSize = reader.ReadUInt32()
            res.Value7 = reader.ReadUInt32()
            res.TextEncoding = CType(reader.ReadUInt32(), SqliteEncoding)
            res.UserVersion = reader.ReadUInt32()
            res.IncrementalVacuumMode = reader.ReadUInt32()
            res.ApplicationId = reader.ReadUInt32()

            reader.Skip(20)

            res.VersionValidFor = reader.ReadUInt32()
            res.Version = reader.ReadUInt32()

            ' TODO: Warn/err on mismatch with expected values

            Return res
        End Function
    End Class
End Namespace
