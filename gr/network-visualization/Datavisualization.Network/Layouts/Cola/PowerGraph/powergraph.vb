#Region "Microsoft.VisualBasic::e2bfba5c1186c74bc739c4ba0c6cfb21, gr\network-visualization\Datavisualization.Network\Layouts\Cola\PowerGraph\powergraph.vb"

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

    '     Interface network
    ' 
    '         Properties: links, nodes
    ' 
    '     Class PowerGraph
    ' 
    '         Properties: groups, powerEdges
    ' 
    '     Class LayoutGraph
    ' 
    '         Properties: cola, powerGraph
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace Layouts.Cola

    Public Interface network

        Property nodes() As Node()
        Property links() As Link(Of Node)()

    End Interface

    Public Class PowerGraph
        Public Property groups As List(Of Node)
        Public Property powerEdges As List(Of PowerEdge(Of Node))
    End Class

    Public Class LayoutGraph
        Public Property cola As Layout
        Public Property powerGraph As PowerGraph
    End Class
End Namespace
