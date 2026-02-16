Imports System.Diagnostics.CodeAnalysis
Imports std = System.Math

Namespace KNearNeighbors.HNSW

    ''' <summary>
    ''' Parameters of the algorithm.
    ''' </summary>
    <SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification:="By Design")>
    <Serializable>
    Public Class Parameters(Of TItem, TDistance)
        ''' <summary>
        ''' Initializes a new instance of the <see cref="Parameters"/> class.
        ''' </summary>
        Public Sub New()
            M = 10
            LevelLambda = 1 / std.Log(M)
            NeighbourHeuristic = NeighbourSelectionHeuristic.SelectSimple
            ConstructionPruning = 200
            ExpandBestSelection = False
            KeepPrunedConnections = True
        End Sub

        ''' <summary>
        ''' Gets or sets the parameter which defines the maximum number of neighbors in the zero and above-zero layers.
        ''' The maximum number of neighbors for the zero layer is 2 * M.
        ''' The maximum number of neighbors for higher layers is M.
        ''' </summary>
        Public Property M As Integer

        ''' <summary>
        ''' Gets or sets the max level decay parameter.
        ''' https://en.wikipedia.org/wiki/Exponential_distribution
        ''' See 'mL' parameter in the HNSW article.
        ''' </summary>
        Public Property LevelLambda As Double

        ''' <summary>
        ''' Gets or sets parameter which specifies the type of heuristic to use for best neighbours selection.
        ''' </summary>
        Public Property NeighbourHeuristic As NeighbourSelectionHeuristic

        ''' <summary>
        ''' Gets or sets the number of candidates to consider as neighbousr for a given node at the graph construction phase.
        ''' See 'efConstruction' parameter in the article.
        ''' </summary>
        Public Property ConstructionPruning As Integer

        ''' <summary>
        ''' Gets or sets a value indicating whether to expand candidates if <see cref="NeighbourSelectionHeuristic.SelectHeuristic"/> is used.
        ''' See 'extendCandidates' parameter in the article.
        ''' </summary>
        Public Property ExpandBestSelection As Boolean

        ''' <summary>
        ''' Gets or sets a value indicating whether to keep pruned candidates if <see cref="NeighbourSelectionHeuristic.SelectHeuristic"/> is used.
        ''' See 'keepPrunedConnections' parameter in the article.
        ''' </summary>
        Public Property KeepPrunedConnections As Boolean
    End Class

    ''' <summary>
    ''' Representation of knn search result.
    ''' </summary>
    <SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification:="By Design")>
    Public Class KNNSearchResult(Of TItem, TDistance)
        ''' <summary>
        ''' Gets or sets the id of the item = rank of the item in source collection.
        ''' </summary>
        Public Property Id As Integer

        ''' <summary>
        ''' Gets or sets the item itself.
        ''' </summary>
        Public Property Item As TItem

        ''' <summary>
        ''' Gets or sets the distance between the item and the knn search query.
        ''' </summary>
        Public Property Distance As TDistance
    End Class
End Namespace