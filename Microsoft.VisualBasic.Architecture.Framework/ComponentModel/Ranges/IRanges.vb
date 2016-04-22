Namespace ComponentModel.Ranges

    ''' <summary>
    ''' Represents a generic range with minimum and maximum values
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IRanges(Of T As IComparable)

        ''' <summary>
        ''' Minimum value
        ''' </summary>
        ReadOnly Property Min As T

        ''' <summary>
        ''' Maximum value
        ''' </summary>
        ReadOnly Property Max As T

        ''' <summary>
        ''' Check if the specified value is inside this range
        ''' </summary>
        ''' <param name="x">Value to check</param>
        ''' <returns><b>True</b> if the specified value is inside this range or
        ''' <b>false</b> otherwise.</returns>
        Function IsInside(x As T) As Boolean

        ''' <summary>
        ''' Check if the specified range is inside this range
        ''' </summary>
        ''' <param name="range">Range to check</param>
        ''' <returns><b>True</b> if the specified range is inside this range or
        ''' <b>false</b> otherwise.</returns>
        Function IsInside(range As IRanges(Of T)) As Boolean

        ''' <summary>
        ''' Check if the specified range overlaps with this range
        ''' </summary>
        ''' <param name="range">Range to check for overlapping</param>
        ''' <returns><b>True</b> if the specified range overlaps with this range or
        ''' <b>false</b> otherwise.</returns>
        Function IsOverlapping(range As IRanges(Of T)) As Boolean
    End Interface
End Namespace