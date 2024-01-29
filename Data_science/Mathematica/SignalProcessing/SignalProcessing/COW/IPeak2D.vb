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

    ''' <summary>
    ''' Function to create a signal peak object
    ''' </summary>
    ''' <param name="id"><see cref="IPeak2D.ID"/></param>
    ''' <param name="dim1"><see cref="IPeak2D.Dimension1"/></param>
    ''' <param name="dim2"><see cref="IPeak2D.Dimension2"/></param>
    ''' <param name="intensity"><see cref="IPeak2D.Intensity"/></param>
    ''' <returns></returns>
    Public Delegate Function IDelegateCreatePeak2D(Of T)(id As String, dim1 As Double, dim2 As Double, intensity As Double) As T

End Namespace