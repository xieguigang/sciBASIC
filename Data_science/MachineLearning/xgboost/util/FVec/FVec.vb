Namespace util

    ''' <summary>
    ''' Interface of feature vector.
    ''' </summary>
    Public Interface FVec

        ''' <summary>
        ''' Gets index-th value.
        ''' </summary>
        ''' <param name="index"> index </param>
        ''' <returns> value </returns>
        Function fvalue(index As Integer) As Double

    End Interface

End Namespace
