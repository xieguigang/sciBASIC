#Region "Microsoft.VisualBasic::e8072516f973ec8a9f674c265e7cb746, Data_science\Graph\Network\NetworkGraph.vb"

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

    '     Class NetworkGraph
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

Namespace Network

    Public Class NetworkGraph(Of Node As {New, Network.Node}, Edge As {New, Network.Edge(Of Node)}) : Inherits Graph(Of Node, Edge, NetworkGraph(Of Node, Edge))

        Sub New()
        End Sub

        ''' <summary>
        ''' Network model copy
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <param name="edges"></param>
        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of Edge))
            Dim index As VBInteger = Scan0

            For Each node As Node In nodes
                ' because the edge object have a reference to current node
                ' so that the edge key will be updated automatic
                ' after this node id have been updated
                node.ID = ++index
                ' initialize internal components
                Call AddVertex(node)
            Next

            Me.edges = New Dictionary(Of Edge)(edges, overridesDuplicateds:=True)
        End Sub
    End Class
End Namespace
