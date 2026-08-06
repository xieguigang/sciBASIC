Namespace XLSX.Writer.Styling


    ''' <summary>
    ''' Range or validity of the format number
    ''' </summary>
    Public Enum FormatRange
        ''' <summary>
        ''' Format from 0 to 164 (with gaps)
        ''' </summary>
        defined_format
        ''' <summary>
        ''' Custom defined formats from 164 and higher. Although 164 is already custom, it is still defined as enum value
        ''' </summary>
        custom_format
        ''' <summary>
        ''' Probably invalid format numbers (e.g. negative value)
        ''' </summary>
        invalid
        ''' <summary>
        ''' Values between 0 and 164 that are not defined as enum value. This may be caused by changes of the OOXML specifications or Excel versions that have encoded loaded files
        ''' </summary>
        undefined
    End Enum
End Namespace