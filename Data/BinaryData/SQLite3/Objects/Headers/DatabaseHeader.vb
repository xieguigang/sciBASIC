#Region "Microsoft.VisualBasic::c6d82c51e03b36f10fb04ab48e71f437, Data\BinaryData\SQLite3\Objects\Headers\DatabaseHeader.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 97
    '    Code Lines: 57 (58.76%)
    ' Comment Lines: 6 (6.19%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 34 (35.05%)
    '     File Size: 3.39 KB


    '     Class DatabaseHeader
    ' 
    '         Properties: ApplicationId, ChangeCounter, DatabaseSizeInPages, DefaultPageCacheSize, FirstFreelistTrunkPage
    '                     FreeListPages, IncrementalVacuumMode, LeafPayloadFraction, MaximumEmbeddedPayloadFraction, MinimumEmbeddedPayloadFraction
    '                     PageSize, ReadVersion, ReservedSpaceAtEndOfPage, SchemaCookie, SchemaFormat
    '                     TextEncoding, UserVersion, Value7, Version, VersionValidFor
    '                     WriteVersion
    ' 
    '         Function: Parse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Internal
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums

Namespace ManagedSqlite.Core.Objects.Headers

    Public Class DatabaseHeader

        Public Const HeaderSize As Integer = 100

        ''' <summary>
        ''' SQLite format 3
        ''' </summary>
        Shared ReadOnly expectedHeader As Byte() = "SQLite format 3".Select(AddressOf Asc).Select(Function(i) CByte(i)).AsList + 0

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

            reader.CheckMagicBytes(expectedHeader)

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
