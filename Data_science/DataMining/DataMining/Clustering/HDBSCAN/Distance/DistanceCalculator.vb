Namespace HDBSCAN.Distance
    ''' <summary>
    ''' An interface for classes which compute the distance between two points (where points are
    ''' represented as arrays of doubles).
    ''' </summary>
    Public Interface IDistanceCalculator(Of T)
        ''' <summary>
        ''' Computes the distance between two points.
        ''' Note that larger values indicate that the two points are farther apart.
        ''' </summary>
        ''' <param name="indexOne">The index of the first attribute</param>
        ''' <param name="indexTwo">The index of the second attribute</param>
        ''' <param name="attributesOne">The attributes of the first point</param>
        ''' <param name="attributesTwo">The attributes of the second point</param>
        ''' <returns>A double for the distance between the two points</returns>
        Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As T, attributesTwo As T) As Double
    End Interface
End Namespace
