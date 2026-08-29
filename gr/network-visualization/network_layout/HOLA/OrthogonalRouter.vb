#Region "Microsoft.VisualBasic::f7b9ea77bddb33b5b9e394aa9462fa0c, gr\network-visualization\network_layout\HOLA\OrthogonalRouter.vb"

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

    '   Total Lines: 45
    '    Code Lines: 25 (55.56%)
    ' Comment Lines: 12 (26.67%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 8 (17.78%)
    '     File Size: 2.00 KB


    '     Module OrthogonalRouter
    ' 
    '         Function: AsPoint
    ' 
    '         Sub: Route
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.EdgeBundling
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Language

Namespace Hola

    ''' <summary>
    ''' HOLA 阶段 6：最终正交路由（Final Orthogonal Route）。
    ''' 把相邻节点之间的连线生成为轴对齐的正交折线（Z 形），并把折点写入
    ''' <see cref="EdgeData.bends"/>（用相对比例偏移的 WayPointVector 描述，
    ''' 使节点位置变化时路径形状可跟随保持）。
    ''' </summary>
    Public Module OrthogonalRouter

        ''' <summary>
        ''' 为图中每条边生成正交路由折点并写回 <see cref="EdgeData.bends"/>。
        ''' </summary>
        Public Sub Route(graph As NetworkGraph, opts As HolaOptions)
            For Each e As Edge In graph.graphEdges
                Dim U = e.U, V = e.V
                If U Is Nothing OrElse V Is Nothing Then Continue For

                Dim pu = AsPoint(U.data.initialPostion)
                Dim pv = AsPoint(V.data.initialPostion)

                ' Z 形正交路径：U -> (midX, U.y) -> (midX, V.y) -> V
                ' 两个拐点都必须相对整条边 (U -> V) 用 CreateVector 生成，
                ' 以保证比例语义一致，渲染层用 GetPoint(U, V) 还原
                Dim midX = (pu.X + pv.X) / 2.0F

                Dim bend1 = WayPointVector.CreateVector(pu, pv, midX, pu.Y)
                Dim bend2 = WayPointVector.CreateVector(pu, pv, midX, pv.Y)

                e.data.bends = {bend1, bend2}
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function AsPoint(v As AbstractVector) As System.Drawing.PointF
            Return New System.Drawing.PointF(CSng(v.x), CSng(v.y))
        End Function
    End Module
End Namespace
