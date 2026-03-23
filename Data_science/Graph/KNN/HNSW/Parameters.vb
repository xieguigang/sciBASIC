#Region "Microsoft.VisualBasic::7ec42631679eb721e9a735518660cf19, Data_science\Graph\KNN\HNSW\Parameters.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 82
    '    Code Lines: 28 (34.15%)
    ' Comment Lines: 43 (52.44%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (13.41%)
    '     File Size: 3.34 KB


    '     Class Parameters
    ' 
    '         Properties: ConstructionPruning, ExpandBestSelection, KeepPrunedConnections, LevelLambda, M
    '                     NeighbourHeuristic
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class KNNSearchResult
    ' 
    '         Properties: Distance, Id, Item
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
