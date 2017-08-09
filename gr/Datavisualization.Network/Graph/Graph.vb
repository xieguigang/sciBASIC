#Region "Microsoft.VisualBasic::77c0a0dc940244d91e3fc03d4f0d5d5a, ..\sciBASIC#\gr\Datavisualization.Network\Graph\Graph.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Language

''' <summary>
''' A graph ``G = (V, E)`` consists of a set V of vertices and a set E edges, that is, unordered
''' pairs Of vertices. Unless explicitly stated otherwise, we assume that the graph Is simple,
''' that Is, it has no multiple edges And no self-loops.
''' </summary>
Public Class Graph

#Region "Let G=(V, E) be a simple graph"
    Dim edges As List(Of Edge)
    Dim vertices As Dictionary(Of Vertex)
#End Region

    Public Function AddEdge(u As Vertex, v As Vertex) As Graph
        edges += New Edge With {
            .U = u,
            .V = v
        }
        If Not vertices.ContainsKey(u.ID) Then
            vertices += u
        End If
        If Not vertices.ContainsKey(v.ID) Then
            vertices += v
        End If

        Return Me
    End Function
End Class

''' <summary>
''' 图之中的节点
''' </summary>
Public Class Vertex : Inherits MassPoint
End Class

''' <summary>
''' 节点之间的边
''' </summary>
Public Class Edge

    Public Property U As Vertex
    Public Property V As Vertex

    Public Overrides Function ToString() As String
        Return $"{U} => {V}"
    End Function
End Class
