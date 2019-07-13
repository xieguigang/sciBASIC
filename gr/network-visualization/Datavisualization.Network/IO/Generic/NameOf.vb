#Region "Microsoft.VisualBasic::e83918c71948532ae816052639a6ff51, gr\network-visualization\Datavisualization.Network\IO\Generic\NameOf.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module NamesOf
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace FileStream.Generic

    ''' <summary>
    ''' The preset names value for edge type <see cref="NetworkEdge"/> and node type <see cref="Node"/>
    ''' </summary>
    Public Module NamesOf

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
