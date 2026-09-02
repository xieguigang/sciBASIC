#Region "Microsoft.VisualBasic::d9893fd0cb82d9a4aa6e78f4fccb1d25, Data_science\DataMining\Bonsai\Layout.vb"

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

    '   Total Lines: 123
    '    Code Lines: 65 (52.85%)
    ' Comment Lines: 35 (28.46%)
    '    - Xml Docs: 48.57%
    ' 
    '   Blank Lines: 23 (18.70%)
    '     File Size: 4.66 KB


    ' Module TreeLayout
    ' 
    '     Function: DendrogramLayout, RadialLayout
    ' 
    '     Sub: AssignLeafAngle, AssignLeafY, ComputeX
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/

'   Author:

'       xie (genetics@smrucc.org)

'   Copyright (c) 2026 GPL3 Licensed


'   GNU GENERAL PUBLIC LICENSE (GPL3)


'   This program is free software: you can redistribute it and/or modify
'   it under the terms of the GNU General Public License as published by
'   the Free Software Foundation, either version 3 of the License, or
'   (at your option) any later version.

'   This program is distributed in the hope that it will be useful,
'   but WITHOUT ANY WARRANTY; without even the implied warranty of
'   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'   GNU General Public License for more details.

'   You should have received a copy of the GNU General Public License
'   along with this program. If not, see <http://www.gnu.org/licenses/>.

' /********************************************************************************/

Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Tree layout routines that turn the reconstructed Bonsai topology (parent/child + branch times) into a
''' two-dimensional embedding, mirroring the distortion-free 2D visualisation that distinguishes Bonsai from
''' UMAP / t-SNE. Only the topology and the branch lengths (<see cref="BonsaiNode.tParent"/>) are used, so the
''' layout is fully independent of the high-dimensional effective coordinates (the <see cref="BonsaiNode.ltqs"/>).
''' </summary>
Public Module TreeLayout

    ''' <summary>
    ''' Dendrogram (rectangular tree) layout. The horizontal coordinate of a node is the cumulative branch
    ''' time from the root to that node (the pseudotime axis); leaves are stacked evenly on the vertical axis
    ''' and every internal node's vertical coordinate is the mean of its children's. Returns one (x, y) pair
    ''' per data leaf, in the order returned by <see cref="BonsaiNode.getLeafs"/>.
    ''' </summary>
    Public Function DendrogramLayout(root As BonsaiNode) As Double()()
        Dim leafs = root.getLeafs()
        Dim n = leafs.Count
        If n = 0 Then Return New Double()() {}

        ' Pass 1: cumulative branch time (x) via DFS from the root.
        root.x = 0.0
        ComputeX(root)

        ' Pass 2: assign leaf slots on the y axis (evenly spaced), then propagate means upward.
        Dim slot = 0
        AssignLeafY(root, slot)

        Dim out(n - 1)() As Double
        For i = 0 To n - 1
            out(i) = New Double() {leafs(i).x, leafs(i).y}
        Next
        Return out
    End Function

    ''' <summary>
    ''' Radial layout: leaves are placed on a circle by their cumulative branch angle, and a node's radius is
    ''' its cumulative branch time from the root. Useful for large trees where a rectangular dendrogram would
    ''' be too wide. Returns (x, y) per leaf in <see cref="BonsaiNode.getLeafs"/> order.
    ''' </summary>
    Public Function RadialLayout(root As BonsaiNode, Optional radiusScale As Double = 1.0) As Double()()
        Dim leafs = root.getLeafs()
        Dim n = leafs.Count
        If n = 0 Then Return New Double()() {}

        root.x = 0.0
        ComputeX(root)

        Dim slot = 0
        AssignLeafAngle(root, slot, n)

        Dim out(n - 1)() As Double
        For i = 0 To n - 1
            Dim r = leafs(i).x * radiusScale
            Dim theta = leafs(i).y
            out(i) = New Double() {r * System.Math.Cos(theta), r * System.Math.Sin(theta)}
        Next
        Return out
    End Function

    Private Sub ComputeX(node As BonsaiNode)
        For Each child In node.childs
            child.x = node.x + child.tParent
            ComputeX(child)
        Next
    End Sub

    Private Sub AssignLeafY(node As BonsaiNode, ByRef slot As Integer)
        If node.isLeafNode() Then
            node.y = slot
            slot += 1
            Return
        End If
        Dim sum = 0.0
        For Each child In node.childs
            AssignLeafY(child, slot)
            sum += child.y
        Next
        node.y = sum / node.childs.Count
    End Sub

    Private Sub AssignLeafAngle(node As BonsaiNode, ByRef slot As Integer, totalLeaves As Integer)
        If node.isLeafNode() Then
            node.y = 2.0 * System.Math.PI * slot / CDbl(totalLeaves)
            slot += 1
            Return
        End If
        Dim sum = 0.0
        For Each child In node.childs
            AssignLeafAngle(child, slot, totalLeaves)
            sum += child.y
        Next
        node.y = sum / node.childs.Count
    End Sub
End Module
