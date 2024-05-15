#Region "Microsoft.VisualBasic::dcfdce64cd1fbb618465c881d185b61b, gr\network-visualization\Datavisualization.Network\Graph\Model\data\NameOf.vb"

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

    '   Total Lines: 31
    '    Code Lines: 20
    ' Comment Lines: 6
    '   Blank Lines: 5
    '     File Size: 1.67 KB


    '     Module NamesOf
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace FileStream.Generic

    ''' <summary>
    ''' The preset names value for edge type <see cref="EdgeData.Properties"/> and node type <see cref="NodeData.Properties"/>
    ''' </summary>
    Public Module NamesOf

        Public Const REFLECTION_ID_MAPPING_FROM_NODE As String = "fromNode"
        Public Const REFLECTION_ID_MAPPING_TO_NODE As String = "toNode"
        Public Const REFLECTION_ID_MAPPING_CONFIDENCE As String = "confidence"
        Public Const REFLECTION_ID_MAPPING_INTERACTION_TYPE As String = "interaction_type"
        Public Const REFLECTION_ID_MAPPING_EDGE_GUID As String = "guid"

        ''' <summary>
        ''' <see cref="Node.label"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_IDENTIFIER As String = "ID"
        Public Const REFLECTION_ID_MAPPING_NODETYPE As String = "nodeType"
        Public Const REFLECTION_ID_MAPPING_NODECOLOR As String = "color"
        Public Const REFLECTION_ID_MAPPING_DEGREE$ = "degree"
        Public Const REFLECTION_ID_MAPPING_RELATIVE_DEGREE_CENTRALITY$ = "degreeCentrality"
        Public Const REFLECTION_ID_MAPPING_RELATIVE_OUTDEGREE_CENTRALITY$ = "OutDegreeCentrality"
        Public Const REFLECTION_ID_MAPPING_DEGREE_IN$ = REFLECTION_ID_MAPPING_DEGREE & ".in"
        Public Const REFLECTION_ID_MAPPING_DEGREE_OUT$ = REFLECTION_ID_MAPPING_DEGREE & ".out"
        Public Const REFLECTION_ID_MAPPING_BETWEENESS_CENTRALITY$ = "BetweenessCentrality"
        Public Const REFLECTION_ID_MAPPING_RELATIVE_BETWEENESS_CENTRALITY$ = "RelativeBetweenessCentrality"

    End Module
End Namespace
