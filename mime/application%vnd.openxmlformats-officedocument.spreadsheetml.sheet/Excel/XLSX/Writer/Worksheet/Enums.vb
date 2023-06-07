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