Imports System.Linq
Imports MBW.Utilities.ManagedSqlite.Core.Internal
Imports MBW.Utilities.ManagedSqlite.Core.Objects.Enums

Namespace MBW.Utilities.ManagedSqlite.Core.Objects.Headers
    Public Class DatabaseHeader
        Public Const HeaderSize As Integer = 100

        ''' <summary>
        ''' SQLite format 3
        ''' </summary>
        Private Shared ReadOnly _expectedHeader As Byte() = "SQLite format 3".Select(AddressOf Asc).Select(Function(i) CByte(i)).AsList + 0

        Public Property PageSize() As UShort
            Get
                Return m_PageSize
            End Get
            Friend Set
                m_PageSize = Value
            End Set
        End Property
        Private m_PageSize As UShort

        Public Property WriteVersion() As FileWriteVersion
            Get
                Return m_WriteVersion
            End Get
            Private Set
                m_WriteVersion = Value
            End Set
        End Property
        Private m_WriteVersion As FileWriteVersion

        Public Property ReadVersion() As FileReadVersion
            Get
                Return m_ReadVersion
            End Get
            Private Set
                m_ReadVersion = Value
            End Set
        End Property
        Private m_ReadVersion As FileReadVersion

        Public Property ReservedSpaceAtEndOfPage() As Byte
            Get
                Return m_ReservedSpaceAtEndOfPage
            End Get
            Friend Set
                m_ReservedSpaceAtEndOfPage = Value
            End Set
        End Property
        Private m_ReservedSpaceAtEndOfPage As Byte

        Public Property MaximumEmbeddedPayloadFraction() As Byte
            Get
                Return m_MaximumEmbeddedPayloadFraction
            End Get
            Private Set
                m_MaximumEmbeddedPayloadFraction = Value
            End Set
        End Property
        Private m_MaximumEmbeddedPayloadFraction As Byte

        Public Property MinimumEmbeddedPayloadFraction() As Byte
            Get
                Return m_MinimumEmbeddedPayloadFraction
            End Get
            Private Set
                m_MinimumEmbeddedPayloadFraction = Value
            End Set
        End Property
        Private m_MinimumEmbeddedPayloadFraction As Byte

        Public Property LeafPayloadFraction() As Byte
            Get
                Return m_LeafPayloadFraction
            End Get
            Private Set
                m_LeafPayloadFraction = Value
            End Set
        End Property
        Private m_LeafPayloadFraction As Byte

        Public Property ChangeCounter() As UInteger
            Get
                Return m_ChangeCounter
            End Get
            Private Set
                m_ChangeCounter = Value
            End Set
        End Property
        Private m_ChangeCounter As UInteger

        Public Property DatabaseSizeInPages() As UInteger
            Get
                Return m_DatabaseSizeInPages
            End Get
            Private Set
                m_DatabaseSizeInPages = Value
            End Set
        End Property
        Private m_DatabaseSizeInPages As UInteger

        Public Property FirstFreelistTrunkPage() As UInteger
            Get
                Return m_FirstFreelistTrunkPage
            End Get
            Private Set
                m_FirstFreelistTrunkPage = Value
            End Set
        End Property
        Private m_FirstFreelistTrunkPage As UInteger

        Public Property FreeListPages() As UInteger
            Get
                Return m_FreeListPages
            End Get
            Private Set
                m_FreeListPages = Value
            End Set
        End Property
        Private m_FreeListPages As UInteger

        Public Property SchemaCookie() As UInteger
            Get
                Return m_SchemaCookie
            End Get
            Private Set
                m_SchemaCookie = Value
            End Set
        End Property
        Private m_SchemaCookie As UInteger

        Public Property SchemaFormat() As UInteger
            Get
                Return m_SchemaFormat
            End Get
            Private Set
                m_SchemaFormat = Value
            End Set
        End Property
        Private m_SchemaFormat As UInteger

        Public Property DefaultPageCacheSize() As UInteger
            Get
                Return m_DefaultPageCacheSize
            End Get
            Private Set
                m_DefaultPageCacheSize = Value
            End Set
        End Property
        Private m_DefaultPageCacheSize As UInteger

        Public Property Value7() As UInteger
            Get
                Return m_Value7
            End Get
            Private Set
                m_Value7 = Value
            End Set
        End Property
        Private m_Value7 As UInteger

        Public Property TextEncoding() As SqliteEncoding
            Get
                Return m_TextEncoding
            End Get
            Friend Set
                m_TextEncoding = Value
            End Set
        End Property
        Private m_TextEncoding As SqliteEncoding

        Public Property UserVersion() As UInteger
            Get
                Return m_UserVersion
            End Get
            Private Set
                m_UserVersion = Value
            End Set
        End Property
        Private m_UserVersion As UInteger

        Public Property IncrementalVacuumMode() As UInteger
            Get
                Return m_IncrementalVacuumMode
            End Get
            Private Set
                m_IncrementalVacuumMode = Value
            End Set
        End Property
        Private m_IncrementalVacuumMode As UInteger

        Public Property ApplicationId() As UInteger
            Get
                Return m_ApplicationId
            End Get
            Private Set
                m_ApplicationId = Value
            End Set
        End Property
        Private m_ApplicationId As UInteger

        Public Property VersionValidFor() As UInteger
            Get
                Return m_VersionValidFor
            End Get
            Private Set
                m_VersionValidFor = Value
            End Set
        End Property
        Private m_VersionValidFor As UInteger

        Public Property Version() As UInteger
            Get
                Return m_Version
            End Get
            Private Set
                m_Version = Value
            End Set
        End Property
        Private m_Version As UInteger

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
