Namespace FileStream

    ''' <summary>
    ''' The names value for edge type <see cref="NetworkEdge"/> and node type <see cref="Node"/>
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
        ''' <see cref="NetworkEdge.Confidence"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_CONFIDENCE As String = "confidence"
        ''' <summary>
        ''' <see cref="NetworkEdge.InteractionType"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_INTERACTION_TYPE As String = "InteractionType"

        ''' <summary>
        ''' <see cref="Node.Identifier"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_IDENTIFIER As String = "Identifier"
        ''' <summary>
        ''' <see cref="Node.NodeType"/>
        ''' </summary>
        Public Const REFLECTION_ID_MAPPING_NODETYPE As String = "NodeType"
    End Module
End Namespace