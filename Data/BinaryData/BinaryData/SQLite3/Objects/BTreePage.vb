Imports MBW.Utilities.ManagedSqlite.Core.Internal
Imports MBW.Utilities.ManagedSqlite.Core.Objects.Enums
Imports MBW.Utilities.ManagedSqlite.Core.Objects.Headers

Namespace MBW.Utilities.ManagedSqlite.Core.Objects
	Friend MustInherit Class BTreePage
		Public ReadOnly Property Page() As UInteger

		Protected Friend ReadOnly Property Reader() As ReaderBase
		Protected Friend ReadOnly Property Header() As BTreeHeader

		Protected Friend ReadOnly Property CellOffsets() As UShort()

		Protected Sub New(reader__1 As ReaderBase, page__2 As UInteger, header__3 As BTreeHeader, cellOffsets__4 As UShort())
			Reader = reader__1
			Page = page__2
			Header = header__3
			CellOffsets = cellOffsets__4
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
					Exit Select
				Case BTreeType.LeafIndexBtreePage
					Throw New ArgumentOutOfRangeException()
				Case BTreeType.LeafTableBtreePage
					res = New BTreeLeafTablePage(reader, page, header, cellOffsets)
					Exit Select
				Case Else
					Throw New ArgumentOutOfRangeException()
			End Select

			res.ParseInternal()

			Return res
		End Function

		Protected MustOverride Sub ParseInternal()
	End Class
End Namespace
