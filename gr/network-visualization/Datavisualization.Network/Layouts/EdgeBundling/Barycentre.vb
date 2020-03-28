#Region "Microsoft.VisualBasic::3400aebf48d132542aefa2b678fefb60, gr\network-visualization\Datavisualization.Network\Layouts\EdgeBundling\Barycentre.vb"

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

    '     Module Barycentre
    ' 
    '         Function: Barycentre, DoBarycentreEdgeLayout
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Layouts.EdgeBundling

    ''' <summary>
    ''' 质心法进行边链接线条的插值
    ''' </summary>
    Public Module Barycentre

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g">请注意，在这个网络图中的节点应该是在完成了布局之后的，具有了各自的布局位置之后的节点</param>
        ''' <returns></returns>
        <Extension>
        Public Function DoBarycentreEdgeLayout(g As NetworkGraph) As NetworkGraph
            ' 遍历每一个节点
            ' 得到与该节点连接的所有的边
            'For Each node As Node In g.vertex
            '    Dim links As Edge() = node.adjacencies _
            '        .EnumerateAllEdges _
            '        .Distinct _
            '        .ToArray
            '    Dim centras As New List(Of PointF)

            '    If links.IsNullOrEmpty Then
            '        Continue For
            '    End If

            '    ' 然后计算出每一条边的质心
            '    For Each link As Edge In links
            '        centras += link.Barycentre
            '        link.data!Barycentre = centras.Last.GetJson
            '    Next

            '    ' 然后计算出所有的这些质心的中心点
            '    ' 这个中心点就是插值的控制点
            '    Dim centra As PointF = centras.Centre

            '    For Each link As Edge In links
            '        If Not link.data.bends.IsNullOrEmpty Then
            '            link.data.bends = {
            '                (link.data.bends.AsList + New FDGVector3(centra)).Average
            '            }
            '        Else
            '            link.data.bends = {
            '                New Handle(centra)
            '            }
            '        End If
            '    Next
            'Next

            Throw New NotImplementedException

            'Return g
        End Function

        <Extension>
        Public Function Barycentre(link As Edge) As PointF
            Dim w1 = link.weight + 1.0E-20
            Dim w2 = link.weight + 1.0E-20

            w1 = w1 / (w1 + w2)
            w2 = w2 / (w1 + w2)

            Dim pu = link.U.data.initialPostion
            Dim pv = link.V.data.initialPostion
            Dim x! = (pu.x * w1 + pv.x * w2)
            Dim y! = (pu.x * w1 + pv.x * w2)

            Return New PointF With {.X = x, .Y = y}
        End Function
    End Module
End Namespace
