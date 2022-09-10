#Region "Microsoft.VisualBasic::73b53afdae67035ace0b81c145f77432, sciBASIC#\Data_science\Graph\Model\Tree\KdTree\KdNodeHeapItem.vb"

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

    '   Total Lines: 18
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 447 B


    '     Class KdNodeHeapItem
    ' 
    '         Properties: distance, node
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KdTree

    Public Class KdNodeHeapItem(Of T)

        Public Property node As KdTreeNode(Of T)
        Public Property distance As Double

        Sub New(node As KdTreeNode(Of T), dist As Double)
            Me.node = node
            Me.distance = dist
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{node}, {distance}]"
        End Function

    End Class
End Namespace
