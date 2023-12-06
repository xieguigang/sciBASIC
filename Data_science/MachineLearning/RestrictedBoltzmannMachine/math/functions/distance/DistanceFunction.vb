Namespace math.functions.distance

    ''' <summary>
    ''' kenny
    ''' 
    ''' Distance is analogous to the 1 - normalized(similarityScore)
    ''' </summary>
    Public Interface DistanceFunction
        Function distance(item1 As Matrix, item2 As Matrix) As Double
    End Interface

End Namespace
