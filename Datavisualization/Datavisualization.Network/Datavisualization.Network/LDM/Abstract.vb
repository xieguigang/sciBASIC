Namespace LDM.Abstract

    ''' <summary>
    ''' Node model in the network
    ''' </summary>
    Public Interface INode
        Property locusId As String
        Property NodeType As String
    End Interface

    Public Interface I_InteractionModel
        Property locusId As String
        Property ToNode As String
    End Interface

    Public Interface INetworkEdge : Inherits I_InteractionModel
        Property Confidence As Double
        Property InteractionType As String
    End Interface

    Public Module ExtensionsAPI

    End Module
End Namespace
