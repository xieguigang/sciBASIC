Namespace math.functions.distance

    ''' <summary>
    ''' kenny
    ''' 
    ''' Distance is analogous to the 1 - normalized(similarityScore)
    ''' </summary>
    Public Interface DistanceFunction
        Function distance(item1 As DenseMatrix, item2 As DenseMatrix) As Double
    End Interface

End Namespace
