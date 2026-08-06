Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Enum for the horizontal alignment of a cell 
    ''' </summary>
    Public Enum HorizontalAlignValue
        ''' <summary>Content will be aligned left</summary>
        left
        ''' <summary>Content will be aligned in the center</summary>
        center
        ''' <summary>Content will be aligned right</summary>
        right
        ''' <summary>Content will fill up the cell</summary>
        fill
        ''' <summary>justify alignment</summary>
        justify
        ''' <summary>General alignment</summary>
        general
        ''' <summary>Center continuous alignment</summary>
        centerContinuous
        ''' <summary>Distributed alignment</summary>
        distributed
        ''' <summary>No alignment. The alignment will not be used in a style</summary>
        none
    End Enum

    ''' <summary>
    ''' Enum for the vertical alignment of a cell 
    ''' </summary>
    Public Enum VerticalAlignValue
        ''' <summary>Content will be aligned on the bottom (default)</summary>
        bottom
        ''' <summary>Content will be aligned on the top</summary>
        top
        ''' <summary>Content will be aligned in the center</summary>
        center
        ''' <summary>justify alignment</summary>
        justify
        ''' <summary>Distributed alignment</summary>
        distributed
        ''' <summary>No alignment. The alignment will not be used in a style</summary>
        none
    End Enum
End Namespace