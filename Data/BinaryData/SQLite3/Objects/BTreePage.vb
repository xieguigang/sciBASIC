#Region "Microsoft.VisualBasic::1e92223cb2024c55eca44f2de2a1da06, Data\BinaryData\SQLite3\Objects\BTreePage.vb"

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

    '   Total Lines: 68
    '    Code Lines: 47 (69.12%)
    ' Comment Lines: 3 (4.41%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (26.47%)
    '     File Size: 2.42 KB


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
            Dim res As BTreePage

            ' Read cells
            Dim cellOffsets As UShort() = New UShort(header.CellCount - 1) {}

            If header.CellCount > 0 Then
                For i As UShort = 0 To header.CellCount - 1
                    cellOffsets(i) = reader.ReadUInt16()
                Next

                Call Array.Sort(cellOffsets)
            End If

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
