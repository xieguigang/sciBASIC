#Region "Microsoft.VisualBasic::04ce147213c685696ea8ec07c40a4e91, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\IO\Generic\NameOf.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace FileStream.Generic

    ''' <summary>
    ''' The preset names value for edge type <see cref="NetworkEdge"/> and node type <see cref="Node"/>
    ''' </summary>
    Public Module [NameOf]

        ''' <summary>
        ''' <see cref="NetworkEdge.FromNode"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_FROM_NODE As String = "fromNode"
        ''' <summary>
        ''' <see cref="NetworkEdge.ToNode"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_TO_NODE As String = "toNode"
        ''' <summary>
        ''' <see cref="NetworkEdge.value"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_CONFIDENCE As String = "confidence"
        ''' <summary>
        ''' <see cref="NetworkEdge.Interaction"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_INTERACTION_TYPE As String = "interaction_type"

        ''' <summary>
        ''' <see cref="Node.ID"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_IDENTIFIER As String = "ID"
        ''' <summary>
        ''' <see cref="Node.NodeType"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_NODETYPE As String = "nodeType"
        Public Const REFLECTION_ID_MAPPING_DEGREE$ = "degree"
        Public Const REFLECTION_ID_MAPPING_DEGREE_IN$ = REFLECTION_ID_MAPPING_DEGREE & ".in"
        Public Const REFLECTION_ID_MAPPING_DEGREE_OUT$ = REFLECTION_ID_MAPPING_DEGREE & ".out"

    End Module
End Namespace
