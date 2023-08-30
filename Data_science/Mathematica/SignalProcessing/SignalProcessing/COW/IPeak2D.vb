Namespace COW

    Public Interface IPeak2D

        ''' <summary>
        ''' the unique reference id of current signal peak
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property ID As String
        ''' <summary>
        ''' the signal peak dimension 1 data
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' in LCMS signal processing, usually could be the [Mass] data.
        ''' </remarks>
        ReadOnly Property Dimension1 As Double
        ''' <summary>
        ''' the signal peak dimension 2 data
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' in LCMS signal processing, usually could be the [RT] time data.
        ''' </remarks>
        ReadOnly Property Dimension2 As Double
        ''' <summary>
        ''' the signal intensity value
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Intensity As Double

    End Interface
End Namespace