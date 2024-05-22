#Region "Microsoft.VisualBasic::4ebdd9b13ebb6b453ce9fe3039e0a2b1, gr\network-visualization\network_layout\EdgeBundling\Mingle\MingleNodeData.vb"

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

    '   Total Lines: 60
    '    Code Lines: 53 (88.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (11.67%)
    '     File Size: 2.15 KB


    '     Class MingleNodeData
    ' 
    '         Properties: alpha, bundle, coords, group, ink
    '                     m1, m2, nodeArray, nodes, parents
    '                     parentsInk
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Clone
    ' 
    '     Class MingleData
    ' 
    '         Properties: bundle, combined, inkTotal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace EdgeBundling.Mingle

    Public Class MingleNodeData : Inherits NodeData

        Public Property nodes As Node()
        Public Property m1 As Double()
        Public Property m2 As Double()
        Public Property coords As Double()
        Public Property bundle As Node
        Public Property ink As New Value(Of Double)
        Public Property parents As Node()
        Public Property nodeArray As Node()
        Public Property parentsInk As Double
        Public Property group As Integer
        Public Property alpha As Double

        Sub New()
        End Sub

        Sub New(copy As NodeData)
            Call MyBase.New(copy)
        End Sub

        Public Overrides Function Clone() As NodeData
            Return New MingleNodeData With {
                .betweennessCentrality = betweennessCentrality,
                .bundle = bundle,
                .color = color,
                .coords = coords.SafeQuery.ToArray,
                .force = force,
                .group = group,
                .initialPostion = initialPostion + 0,
                .ink = ink?.Value,
                .label = label,
                .m1 = m1.SafeQuery.ToArray,
                .m2 = m2.SafeQuery.ToArray,
                .mass = mass,
                .neighbours = neighbours.SafeQuery.ToArray,
                .nodeArray = nodeArray.SafeQuery.ToArray,
                .nodes = nodes.SafeQuery.ToArray,
                .origID = origID,
                .parents = parents.SafeQuery.ToArray,
                .parentsInk = parentsInk,
                .Properties = New Dictionary(Of String, String)(Properties),
                .size = size.SafeQuery.ToArray,
                .weights = weights.SafeQuery.ToArray
            }
        End Function
    End Class

    Public Class MingleData
        Public Property bundle As Node()
        Public Property inkTotal As Double
        Public Property combined As Node
    End Class
End Namespace
