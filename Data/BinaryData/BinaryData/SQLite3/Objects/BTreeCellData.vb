Imports System.Diagnostics

Namespace ManagedSqlite.Core.Objects
    <DebuggerDisplay("Page {Page}, Size {Cell.DataSizeInCell} / {Cell.DataSize}")>
    Friend Class BTreeCellData
        Friend Property Page() As UInteger
            Get
                Return m_Page
            End Get
            Set
                m_Page = Value
            End Set
        End Property
        Private m_Page As UInteger

        Friend Property Cell() As BTreeLeafTablePage.Cell
            Get
                Return m_Cell
            End Get
            Set
                m_Cell = Value
            End Set
        End Property
        Private m_Cell As BTreeLeafTablePage.Cell

        Friend Property CellOffset() As UShort
            Get
                Return m_CellOffset
            End Get
            Set
                m_CellOffset = Value
            End Set
        End Property
        Private m_CellOffset As UShort
    End Class
End Namespace
