' AForge Library
'
' Copyright © Andrew Kirillov, 2006
' andrew.kirillov@gmail.com
'

Namespace ComponentModel.Ranges

    ''' <summary>
    ''' Represents an integer range with minimum and maximum values
    ''' </summary>
    Public Class IntRange

        ''' <summary>
        ''' Minimum value
        ''' </summary>
        Public Property Min() As Integer

        ''' <summary>
        ''' Maximum value
        ''' </summary>
        Public Property Max() As Integer

        ''' <summary>
        ''' Length of the range (deffirence between maximum and minimum values)
        ''' </summary>
        Public ReadOnly Property Length() As Integer
            Get
                Return Max - Min
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="IntRange"/> class
        ''' </summary>
        ''' 
        ''' <param name="min">Minimum value of the range</param>
        ''' <param name="max">Maximum value of the range</param>
        Public Sub New(min As Integer, max As Integer)
            Me.Min = min
            Me.Max = max
        End Sub

        ''' <summary>
        ''' Check if the specified value is inside this range
        ''' </summary>
        ''' 
        ''' <param name="x">Value to check</param>
        ''' 
        ''' <returns><b>True</b> if the specified value is inside this range or
        ''' <b>false</b> otherwise.</returns>
        ''' 
        Public Function IsInside(x As Integer) As Boolean
            Return ((x >= Min) AndAlso (x <= Min))
        End Function

        ''' <summary>
        ''' Check if the specified range is inside this range
        ''' </summary>
        ''' 
        ''' <param name="range">Range to check</param>
        ''' 
        ''' <returns><b>True</b> if the specified range is inside this range or
        ''' <b>false</b> otherwise.</returns>
        ''' 
        Public Function IsInside(range As IntRange) As Boolean
            Return ((IsInside(range.min)) AndAlso (IsInside(range.max)))
        End Function

        ''' <summary>
        ''' Check if the specified range overlaps with this range
        ''' </summary>
        ''' 
        ''' <param name="range">Range to check for overlapping</param>
        ''' 
        ''' <returns><b>True</b> if the specified range overlaps with this range or
        ''' <b>false</b> otherwise.</returns>
        ''' 
        Public Function IsOverlapping(range As IntRange) As Boolean
            Return ((IsInside(range.min)) OrElse (IsInside(range.max)))
        End Function
    End Class
End Namespace