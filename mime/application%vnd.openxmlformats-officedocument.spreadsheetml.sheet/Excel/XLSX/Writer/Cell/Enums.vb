Namespace XLSX.Writer

    ''' <summary>
    ''' Enum defines the basic data types of a cell
    ''' </summary>
    Public Enum CellType
        ''' <summary>Type for single characters and strings</summary>
        [STRING]
        ''' <summary>Type for all numeric types (long, integer, float, double, short, byte and decimal; signed and unsigned, if available)</summary>
        NUMBER
        ''' <summary>Type for dates (Note: Dates before 1900-01-01 and after 9999-12-31 are not allowed)</summary>
        [DATE]
        ''' <summary>Type for times (Note: Internally handled as OAdate, represented by <see cref="TimeSpan"/>)</summary>
        TIME
        ''' <summary>Type for boolean</summary>
        BOOL
        ''' <summary>Type for Formulas (The cell will be handled differently)</summary>
        FORMULA
        ''' <summary>Type for empty cells. This type is only used for merged cells (all cells except the first of the cell range)</summary>
        EMPTY
        ''' <summary>Default Type, not specified</summary>
        [DEFAULT]
    End Enum

    ''' <summary>
    ''' Enum for the referencing style of the address
    ''' </summary>
    Public Enum AddressType
        ''' <summary>Default behavior (e.g. 'C3')</summary>
        [Default]
        ''' <summary>Row of the address is fixed (e.g. 'C$3')</summary>
        FixedRow
        ''' <summary>Column of the address is fixed (e.g. '$C3')</summary>
        FixedColumn
        ''' <summary>Row and column of the address is fixed (e.g. '$C$3')</summary>
        FixedRowAndColumn
    End Enum

    ''' <summary>
    ''' Enum to define the scope of a passed address string (used in static context)
    ''' </summary>
    Public Enum AddressScope
        ''' <summary>The address represents a single cell</summary>
        SingleAddress
        ''' <summary>The address represents a range of cells</summary>
        Range
        ''' <summary>The address expression is invalid</summary>
        Invalid
    End Enum
End Namespace