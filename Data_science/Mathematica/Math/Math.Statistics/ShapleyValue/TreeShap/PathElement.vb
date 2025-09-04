Namespace ShapleyValue.TreeShape

    ''' <summary>
    ''' Path element for use in <seealso cref="ShapAlgo2"/>
    ''' </summary>
    Public Class PathElement
        ''' <summary>
        ''' (m.d)
        ''' </summary>
        Public featureIndex As Integer = 0
        ''' <summary>
        ''' (m.z)
        ''' </summary>
        Public zeroFraction As Double = Double.NaN
        ''' <summary>
        ''' (m.o)
        ''' </summary>
        Public oneFraction As Double = Double.NaN
        ''' <summary>
        ''' (m.w)
        ''' </summary>
        Public weight As Double = Double.NaN

        Public Overrides Function ToString() As String
            Return "PE{f=" & featureIndex.ToString() & ", zf=" & zeroFraction.ToString() & ", of=" & oneFraction.ToString() & ", pw=" & weight.ToString() & "}"c.ToString()
        End Function
    End Class

End Namespace
