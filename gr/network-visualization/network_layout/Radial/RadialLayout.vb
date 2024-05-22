#Region "Microsoft.VisualBasic::b4c1c79061aed40a7a95a96b011b262a, gr\network-visualization\network_layout\Radial\RadialLayout.vb"

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

    '   Total Lines: 51
    '    Code Lines: 37 (72.55%)
    ' Comment Lines: 4 (7.84%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (19.61%)
    '     File Size: 2.08 KB


    '     Module RadialLayout
    ' 
    '         Function: layoutCurrentCenter, LayoutNodes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports stdNum = System.Math

Namespace Radial

    Public Module RadialLayout

        Public Function LayoutNodes(g As NetworkGraph) As NetworkGraph
            ' 首先计算出所有节点的连接度
            ' 将最高连接度的节点作为布局的中心点
            Dim degreeOrders = g.ComputeNodeDegrees.OrderByDescending(Function(a) a.Value).ToArray
            Dim layout = g.layoutCurrentCenter(cid:=degreeOrders.First.Key, degreeOrders.ToDictionary)

            Return layout
        End Function

        <Extension>
        Private Function layoutCurrentCenter(g As NetworkGraph, cid As String, degrees As Dictionary(Of String, Integer)) As NetworkGraph
            ' 其余的节点与中心节点的距离与度有关，度越高距离越远
            Dim center As Node = g.GetElementByID(cid)
            Dim connected As Node() = g.GetEdges(center) _
                .Select(Function(a) a.Other(center)) _
                .Where(Function(a) degrees.ContainsKey(a.label)) _
                .OrderBy(Function(n) degrees(n.label)) _
                .ToArray

            degrees.Remove(cid)

            ' 当前节点为孤立节点或者已经被布局过了
            If connected.Length = 0 OrElse degrees.Count = 0 Then
                Return g
            End If

            Dim deltaAngle As Double = 2 * stdNum.PI / connected.Length
            Dim angle As Double

            For Each node As Node In connected
                node.data.initialPostion = New FDGVector2 With {
                    .x = center.data.initialPostion.x * stdNum.Cos(angle),
                    .y = center.data.initialPostion.y * stdNum.Sin(angle)
                }
                g.layoutCurrentCenter(node.label, degrees)
                angle += deltaAngle
            Next

            Return g
        End Function
    End Module
End Namespace
