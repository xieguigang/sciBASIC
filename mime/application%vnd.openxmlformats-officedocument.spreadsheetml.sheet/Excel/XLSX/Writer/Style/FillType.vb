Namespace XLSX.Writer.Styling


    ''' <summary>
    ''' Enum for the type of the color
    ''' </summary>
    Public Enum FillType
        ''' <summary>Color defines a pattern color </summary>
        patternColor
        ''' <summary>Color defines a solid fill color </summary>
        fillColor
    End Enum

    ''' <summary>
    ''' Enum for the pattern values
    ''' </summary>
    Public Enum PatternValue
        ''' <summary>No pattern (default)</summary>
        none
        ''' <summary>Solid fill (for colors)</summary>
        solid
        ''' <summary>Dark gray fill</summary>
        darkGray
        ''' <summary>Medium gray fill</summary>
        mediumGray
        ''' <summary>Light gray fill</summary>
        lightGray
        ''' <summary>6.25% gray fill</summary>
        gray0625
        ''' <summary>12.5% gray fill</summary>
        gray125
    End Enum
End Namespace