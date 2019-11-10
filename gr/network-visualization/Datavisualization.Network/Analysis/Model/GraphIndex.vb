Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract

Namespace Analysis.Model

    Public Class GraphIndex(Of Node As INamedValue, Edge As IInteraction)

        Dim _nodes As Dictionary(Of String, Node)

        Public Function nodes(nodeSet As IEnumerable(Of Node)) As GraphIndex(Of Node, Edge)
            _nodes = nodeSet.ToDictionary(Function(n) n.Key)
            Return Me
        End Function

    End Class
End Namespace