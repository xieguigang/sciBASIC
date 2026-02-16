Namespace KNearNeighbors.HNSW

    ''' <summary>
    ''' Type of heuristic to select best neighbours for a node.
    ''' </summary>
    Public Enum NeighbourSelectionHeuristic
        ''' <summary>
        ''' Marker for the Algorithm 3 (SELECT-NEIGHBORS-SIMPLE) from the article.
        ''' Implemented in <see cref="NodeAlg3"/>
        ''' </summary>
        SelectSimple

        ''' <summary>
        ''' Marker for the Algorithm 4 (SELECT-NEIGHBORS-HEURISTIC) from the article.
        ''' Implemented in <see cref="NodeAlg4"/>
        ''' </summary>
        SelectHeuristic
    End Enum
End Namespace