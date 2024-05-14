#Region "Microsoft.VisualBasic::3e06631da238e415f20cb9099de1fe51, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Worksheet\Enums.vb"

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

    '   Total Lines: 66
    '    Code Lines: 30
    ' Comment Lines: 32
    '   Blank Lines: 4
    '     File Size: 3.29 KB


    '     Enum CellDirection
    ' 
    '         ColumnToColumn, Disabled, RowToRow
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum SheetProtectionValue
    ' 
    '         autoFilter, deleteColumns, deleteRows, formatCells, formatColumns
    '         formatRows, insertColumns, insertHyperlinks, insertRows, objects
    '         pivotTables, scenarios, selectLockedCells, selectUnlockedCells, sort
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum WorksheetPane
    ' 
    '         bottomLeft, bottomRight, topLeft, topRight
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace XLSX.Writer

    ''' <summary>
    ''' Enum to define the direction when using AddNextCell method
    ''' </summary>
    Public Enum CellDirection
        ''' <summary>The next cell will be on the same row (A1,B1,C1...)</summary>
        ColumnToColumn
        ''' <summary>The next cell will be on the same column (A1,A2,A3...)</summary>
        RowToRow
        ''' <summary>The address of the next cell will be not changed when adding a cell (for manual definition of cell addresses)</summary>
        Disabled
    End Enum

    ''' <summary>
    ''' Enum to define the possible protection types when protecting a worksheet
    ''' </summary>
    Public Enum SheetProtectionValue
        ' sheet, // Is always on 1 if protected
        ''' <summary>If selected, the user can edit objects if the worksheets is protected</summary>
        objects
        ''' <summary>If selected, the user can edit scenarios if the worksheets is protected</summary>
        scenarios
        ''' <summary>If selected, the user can format cells if the worksheets is protected</summary>
        formatCells
        ''' <summary>If selected, the user can format columns if the worksheets is protected</summary>
        formatColumns
        ''' <summary>If selected, the user can format rows if the worksheets is protected</summary>
        formatRows
        ''' <summary>If selected, the user can insert columns if the worksheets is protected</summary>
        insertColumns
        ''' <summary>If selected, the user can insert rows if the worksheets is protected</summary>
        insertRows
        ''' <summary>If selected, the user can insert hyper links if the worksheets is protected</summary>
        insertHyperlinks
        ''' <summary>If selected, the user can delete columns if the worksheets is protected</summary>
        deleteColumns
        ''' <summary>If selected, the user can delete rows if the worksheets is protected</summary>
        deleteRows
        ''' <summary>If selected, the user can select locked cells if the worksheets is protected</summary>
        selectLockedCells
        ''' <summary>If selected, the user can sort cells if the worksheets is protected</summary>
        sort
        ''' <summary>If selected, the user can use auto filters if the worksheets is protected</summary>
        autoFilter
        ''' <summary>If selected, the user can use pivot tables if the worksheets is protected</summary>
        pivotTables
        ''' <summary>If selected, the user can select unlocked cells if the worksheets is protected</summary>
        selectUnlockedCells
    End Enum

    ''' <summary>
    ''' Enum to define the pane position or active pane in a slip worksheet
    ''' </summary>
    Public Enum WorksheetPane
        ''' <summary>The pane is located in the bottom right of the split worksheet</summary>
        bottomRight
        ''' <summary>The pane is located in the top right of the split worksheet</summary>
        topRight
        ''' <summary>The pane is located in the bottom left of the split worksheet</summary>
        bottomLeft
        ''' <summary>The pane is located in the top left of the split worksheet</summary>
        topLeft
    End Enum

End Namespace
