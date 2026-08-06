Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Enum for the font scheme
    ''' </summary>
    Public Enum SchemeValue
        ''' <summary>Font scheme is major</summary>
        major
        ''' <summary>Font scheme is minor (default)</summary>
        minor
        ''' <summary>No Font scheme is used</summary>
        none
    End Enum

    ''' <summary>
    ''' Enum for the style of the underline property of a stylized text
    ''' </summary>
    Public Enum UnderlineValue
        ''' <summary>Text contains a single underline</summary>
        u_single
        ''' <summary>Text contains a double underline</summary>
        u_double
        ''' <summary>Text contains a single, accounting underline</summary>
        singleAccounting
        ''' <summary>Text contains a double, accounting underline</summary>
        doubleAccounting
        ''' <summary>Text contains no underline (default)</summary>
        none
    End Enum
End Namespace