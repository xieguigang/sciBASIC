#Region "Microsoft.VisualBasic::05c87baeacab72defabcecbe7a50cc9c, Data_science\Graph\API\PQDijkstra\HeapNode.vb"

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

    '     Structure HeapNode
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Text

Namespace Dijkstra.PQDijkstra

    Public Structure HeapNode
        Public Sub New(i As Integer, w As Single)
            index = i

            weight = w
        End Sub
        Public index As Integer
        Public weight As Single
    End Structure
End Namespace
