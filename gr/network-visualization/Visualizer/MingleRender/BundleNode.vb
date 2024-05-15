#Region "Microsoft.VisualBasic::c2be6c6811f4f9ecdd124cd61ef25f85, gr\network-visualization\Visualizer\MingleRender\BundleNode.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 90
    '    Code Lines: 73
    ' Comment Lines: 1
    '   Blank Lines: 16
    '     File Size: 3.27 KB


    ' Class BundleNode
    ' 
    '     Properties: expandedEdges, node, unbundledEdges
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: expandEdges, ToString, unbundleEdges
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling.Mingle
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports number = System.Double

Public Class BundleNode

    Public ReadOnly Property node As Node
    Public ReadOnly Property expandedEdges As PosItem()()
    Public ReadOnly Property unbundledEdges As Dictionary(Of String, PosItem()())

    Sub New(v As Node)
        node = v
    End Sub

    Public Overrides Function ToString() As String
        Return node.ToString
    End Function

    Public Function expandEdges() As PosItem()()
        If Not expandedEdges.IsNullOrEmpty Then
            Return expandedEdges
        End If

        Dim ans As PosItem()() = {}
        expandEdgesRichHelper(node, {}, ans)
        _expandedEdges = ans
        Return ans
    End Function

    Public Function unbundleEdges(Optional delta As number = 0) As PosItem()()
        Dim expandedEdges = expandEdges(),
            ans = New PosItem(expandedEdges.Length - 1)() {},
            edge As PosItem(), edgeCopy As PosItem()
        Dim normal, x0 As Vector, xk As Vector, xk_x0 As Vector, xi As Vector,
            xi_x0 As Vector, xi_bar As Vector, dot As Double, norm As Double, norm2 As Double,
            c As Double, last As Integer

        If unbundledEdges Is Nothing Then
            _unbundledEdges = New Dictionary(Of String, PosItem()())
        End If

        If ((delta = 0 OrElse delta = 1) AndAlso unbundledEdges.ContainsKey(delta)) Then
            Return unbundledEdges(delta.ToString)
        End If

        Dim l = expandedEdges.Length

        For i As Integer = 0 To expandedEdges.Length - 1
            edge = expandedEdges(i)
            last = edge.Length - 1
            edgeCopy = cloneEdge(edge)
            ' edgeCopy = cloneJSON(edge)
            x0 = edge(0).pos
            xk = edge(last).pos
            xk_x0 = xk - x0

            edgeCopy(0).unbundledPos = edgeCopy(0).pos.ToArray
            normal = edgeCopy(1).pos - edgeCopy(0).pos
            normal = New Vector({-normal(1), normal(0)}).Unit
            edgeCopy(0).normal = normal

            edgeCopy(last).unbundledPos = edgeCopy(edge.Length - 1).pos.ToArray
            normal = edgeCopy(last).pos - edgeCopy(last - 1).pos
            normal = New Vector({-normal(1), normal(0)}).Unit
            edgeCopy(last).normal = normal

            For j As Integer = 1 To edge.Length - 2
                xi = edge(j).pos
                xi_x0 = xi - x0
                dot = xi_x0.DotProduct(xk_x0)
                norm = dist(xk, x0)
                norm2 = norm * norm
                c = dot / norm2
                xi_bar = x0 + (c * xk_x0)
                edgeCopy(j).unbundledPos = lerp(xi_bar, xi, delta)
                normal = edgeCopy(j + 1).pos - edgeCopy(j - 1).pos
                normal = New Vector({-normal(1), normal(0)}).Unit
                edgeCopy(j).normal = normal
            Next
            ans(i) = edgeCopy
        Next

        If (delta = 0 OrElse delta = 1) Then
            unbundledEdges(delta.ToString) = ans
        End If

        Return ans
    End Function
End Class
