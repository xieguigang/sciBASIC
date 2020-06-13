Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Analysis.Model

    Public Class DirectedVertex : Implements IEnumerable(Of Edge)

        Public ReadOnly Property label As String

        ''' <summary>
        ''' me to target
        ''' </summary>
        Friend ReadOnly outgoingEdges As New HashSet(Of Edge)
        ''' <summary>
        ''' source to me
        ''' </summary>
        Friend ReadOnly incomingEdges As New HashSet(Of Edge)

        Sub New(label As String)
            Me.label = label

            If label.StringEmpty Then
                Throw New ArgumentNullException("vertex label can not be empty!")
            End If
        End Sub

        ''' <summary>
        ''' unsafe add outgoing edge
        ''' </summary>
        ''' <param name="edge"></param>
        Public Sub addOutgoingEdge(edge As Edge)
            Call outgoingEdges.Add(edge)
        End Sub

        ''' <summary>
        ''' unsafe add source to me edge
        ''' </summary>
        ''' <param name="e"></param>
        Public Sub addIncomingEdge(e As Edge)
            Call incomingEdges.Add(e)
        End Sub

        ''' <summary>
        ''' safely add an edge to current vertex
        ''' </summary>
        ''' <param name="edge"></param>
        Public Sub addEdge(edge As Edge)
            If edge.U.label = label Then
                ' me is source
                ' me to target
                Call addOutgoingEdge(edge)
            ElseIf edge.V.label = label Then
                ' me is target
                ' source to me
                Call addIncomingEdge(edge)
            Else
                Throw New InvalidConstraintException($"the given edge({edge.U} -> {edge.V}) object is not connected to current vertex {label}!")
            End If
        End Sub

        ''' <summary>
        ''' get first edge that connected to the given <paramref name="v2"/> node.
        ''' </summary>
        ''' <param name="v2">target to</param>
        ''' <returns></returns>
        Public Function getEdgeTo(v2 As DirectedVertex) As Edge
            For Each edge As Edge In outgoingEdges
                If edge.V.label = v2.label Then
                    Return edge
                End If
            Next

            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return label
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Edge) Implements IEnumerable(Of Edge).GetEnumerator
            For Each edge As Edge In outgoingEdges
                Yield edge
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace