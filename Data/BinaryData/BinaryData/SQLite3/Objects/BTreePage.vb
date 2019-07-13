#Region "Microsoft.VisualBasic::47770afa704726ef24093a50a2c179f5, Data\BinaryData\BinaryData\SQLite3\Objects\BTreePage.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class BTreePage
    ' 
    '         Properties: CellOffsets, Header, Page, Reader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Parse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Internal
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Headers

Namespace ManagedSqlite.Core.Objects

    Friend MustInherit Class BTreePage

        Public ReadOnly Property Page() As UInteger

        Protected Friend ReadOnly Property Reader() As ReaderBase
        Protected Friend ReadOnly Property Header() As BTreeHeader

        Protected Friend ReadOnly Property CellOffsets() As UShort()

        Protected Sub New(reader As ReaderBase, page As UInteger, header As BTreeHeader, cellOffsets As UShort())
            Me.Reader = reader
            Me.Page = page
            Me.Header = header
            Me.CellOffsets = cellOffsets
        End Sub

        Friend Shared Function Parse(reader As ReaderBase, page As UInteger) As BTreePage
            ' Read header
            reader.SeekPage(page)

            If page = 1 Then
                ' Skip the first 100 bytes
                reader.Skip(DatabaseHeader.HeaderSize)
            End If

            Dim header As BTreeHeader = BTreeHeader.Parse(reader)

            ' Read cells
            Dim cellOffsets As UShort() = New UShort(header.CellCount - 1) {}

            For i As UShort = 0 To header.CellCount - 1
                cellOffsets(i) = reader.ReadUInt16()
            Next

            Array.Sort(cellOffsets)

            Dim res As BTreePage
            Select Case header.Type
                Case BTreeType.InteriorIndexBtreePage
                    Throw New ArgumentOutOfRangeException()
                Case BTreeType.InteriorTableBtreePage
                    res = New BTreeInteriorTablePage(reader, page, header, cellOffsets)

                Case BTreeType.LeafIndexBtreePage
                    Throw New ArgumentOutOfRangeException()
                Case BTreeType.LeafTableBtreePage
                    res = New BTreeLeafTablePage(reader, page, header, cellOffsets)

                Case Else
                    Throw New ArgumentOutOfRangeException()
            End Select

            res.ParseInternal()

            Return res
        End Function

        Protected MustOverride Sub ParseInternal()
    End Class
End Namespace
