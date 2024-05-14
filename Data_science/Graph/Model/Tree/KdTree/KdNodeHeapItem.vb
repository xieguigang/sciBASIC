#Region "Microsoft.VisualBasic::cf405d25fbd65eb76fe390698b72bc9e, Data_science\Graph\Model\Tree\KdTree\KdNodeHeapItem.vb"

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

    '   Total Lines: 31
    '    Code Lines: 13
    ' Comment Lines: 12
    '   Blank Lines: 6
    '     File Size: 856 B


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

    ''' <summary>
    ''' A KD-tree node bind with the distance with the target query point.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class KdNodeHeapItem(Of T)

        ''' <summary>
        ''' the node KNN query result
        ''' </summary>
        ''' <returns></returns>
        Public Property node As KdTreeNode(Of T)

        ''' <summary>
        ''' the distance value to the query point
        ''' </summary>
        ''' <returns></returns>
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
