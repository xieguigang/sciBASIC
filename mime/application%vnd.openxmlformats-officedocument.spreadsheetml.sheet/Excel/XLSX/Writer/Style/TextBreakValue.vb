Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Enum for text break options
    ''' </summary>
    Public Enum TextBreakValue
        ''' <summary>Word wrap is active</summary>
        wrapText
        ''' <summary>Text will be resized to fit the cell</summary>
        shrinkToFit
        ''' <summary>Text will overflow in cell</summary>
        none
    End Enum

    ''' <summary>
    ''' Enum for the general text alignment direction
    ''' </summary>
    Public Enum TextDirectionValue
        ''' <summary>Text direction is horizontal (default)</summary>
        horizontal
        ''' <summary>Text direction is vertical</summary>
        vertical
    End Enum
End Namespace