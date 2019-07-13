#Region "Microsoft.VisualBasic::c4bebf35bbfcc74dddece24e02485123, gr\network-visualization\Datavisualization.Network\Graph\Abstract.vb"

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

    '     Interface INode
    ' 
    '         Properties: Id, NodeType
    ' 
    '     Interface IInteraction
    ' 
    '         Properties: source, target
    ' 
    '     Interface INetworkEdge
    ' 
    '         Properties: Interaction, value
    ' 
    '     Module ExtensionsAPI
    ' 
    '         Function: Contains, Equals, GetConnectedNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Graph.Abstract

    ''' <summary>
    ''' Node model in the network
    ''' </summary>
    Public Interface INode
        Property Id As String
        Property NodeType As String
    End Interface

    Public Interface IInteraction
        Property source As String
        Property target As String
    End Interface

    Public Interface INetworkEdge : Inherits IInteraction
        Property value As Double
        Property Interaction As String
    End Interface

    Public Module ExtensionsAPI

        <Extension>
        Public Function Equals(Model As IInteraction, Node1 As String, Node2 As String) As Boolean
            If String.Equals(Model.source, Node1, StringComparison.OrdinalIgnoreCase) Then
                Return String.Equals(Model.target, Node2, StringComparison.OrdinalIgnoreCase)
            ElseIf String.Equals(Model.target, Node1, StringComparison.OrdinalIgnoreCase) Then
                Return String.Equals(Model.source, Node2, StringComparison.OrdinalIgnoreCase)
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function GetConnectedNode(Node As IInteraction, a As String) As String
            If String.Equals(Node.source, a) Then
                Return Node.target
            ElseIf String.Equals(Node.target, a) Then
                Return Node.source
            Else
                Return ""
            End If
        End Function

        <Extension>
        Public Function Contains(edge As IInteraction, node As String) As Boolean
            Return String.Equals(node, edge.source, StringComparison.OrdinalIgnoreCase) OrElse
                String.Equals(node, edge.target, StringComparison.OrdinalIgnoreCase)
        End Function
    End Module
End Namespace
