#Region "Microsoft.VisualBasic::97f07864701dc2c44330410752eb92de, ..\Datavisualization.Network\Datavisualization.Network\LDM\Graph\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network.Abstract

Namespace Graph

    Public Module Extensions

        <Extension>
        Public Sub ApplyAnalysis(ByRef net As NetworkGraph)

            For Each node In net.nodes
                node.Data.Neighbours = net.GetNeighbours(node.ID).ToArray
            Next
        End Sub

        <Extension>
        Public Iterator Function GetNeighbours(net As NetworkGraph, node As String) As IEnumerable(Of Integer)
            For Each edge As Edge In net.edges
                Dim connected As String = edge.GetConnectedNode(node)
                If Not String.IsNullOrEmpty(connected) Then
                    Yield CInt(connected)
                End If
            Next
        End Function
    End Module
End Namespace
