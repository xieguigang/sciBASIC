Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Public Structure TimePoint

    Dim [date] As Date
    Dim average As Double
    ''' <summary>
    ''' [min, max]
    ''' </summary>
    Dim range As DoubleRange

    Public Overrides Function ToString() As String
        Return $"<{[date].ToString}> {average} IN [{range.Min}, {range.Max}]"
    End Function
End Structure