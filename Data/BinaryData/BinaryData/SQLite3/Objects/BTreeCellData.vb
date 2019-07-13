#Region "Microsoft.VisualBasic::1beaf146e17036c13e16fcba9bd80608, Data\BinaryData\BinaryData\SQLite3\Objects\BTreeCellData.vb"

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

    '     Class BTreeCellData
    ' 
    '         Properties: Cell, CellOffset, Page
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics

Namespace ManagedSqlite.Core.Objects

    <DebuggerDisplay("Page {Page}, Size {Cell.DataSizeInCell} / {Cell.DataSize}")>
    Friend Class BTreeCellData

        Friend Property Page As UInteger
        Friend Property Cell As BTreeLeafTablePage.Cell
        Friend Property CellOffset As UShort

    End Class
End Namespace
