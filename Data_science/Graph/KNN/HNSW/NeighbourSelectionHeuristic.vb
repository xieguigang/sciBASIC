#Region "Microsoft.VisualBasic::fdd335039ff995b71c76beffb6f6688d, Data_science\Graph\KNN\HNSW\NeighbourSelectionHeuristic.vb"

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

    '   Total Lines: 19
    '    Code Lines: 6 (31.58%)
    ' Comment Lines: 11 (57.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (10.53%)
    '     File Size: 625 B


    '     Enum NeighbourSelectionHeuristic
    ' 
    '         SelectHeuristic, SelectSimple
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
