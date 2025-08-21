Namespace Math.Statistics

    ''' <summary>
    ''' a general abstract object model for statistics result which 
    ''' contains p value information
    ''' </summary>
    Public Interface IStatPvalue

        ''' <summary>
        ''' p value of current sample statistics result
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property pValue As Double

    End Interface
End Namespace