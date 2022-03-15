#Region "Microsoft.VisualBasic::905db0976cd26f4a77eb621287c91b09, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\Orthogonal\Router.vb"

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

    '   Total Lines: 42
    '    Code Lines: 19
    ' Comment Lines: 16
    '   Blank Lines: 7
    '     File Size: 1.85 KB


    '     Module Router
    ' 
    '         Function: DoEdgeLayout
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling
Imports Microsoft.VisualBasic.Language

Namespace Layouts.Orthogonal

    Public Module Router

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g">请注意，在这个网络图中的节点应该是在完成了布局之后的，具有了各自的布局位置之后的节点</param>
        ''' <returns></returns>
        <Extension>
        Public Function DoEdgeLayout(g As NetworkGraph) As NetworkGraph
            ' 初始化基本布局
            For Each edge As Edge In g.graphEdges
                Dim a = edge.U.data.initialPostion.Point2D
                Dim b = edge.V.data.initialPostion.Point2D
                Dim points As New List(Of XYMetaHandle)
                'Dim points As New List(Of Handle)

                'points += HandleCreator.defineHandle(a, b, a.X, a.Y)
                'points += HandleCreator.defineHandle(a, b, a.X, (a.Y + b.Y) / 2)
                'points += HandleCreator.defineHandle(a, b, a.X, b.Y)
                'points += HandleCreator.defineHandle(a, b, (a.X + b.X) / 2, b.Y)
                'points += HandleCreator.defineHandle(a, b, b.X, b.Y)

                ' points += XYMetaHandle.CreateVector(a, b, a.X, a.Y)
                ' points += XYMetaHandle.CreateVector(a, b, a.X, (a.Y + b.Y) / 2)
                points += XYMetaHandle.CreateVector(a, b, a.X, b.Y)
                ' points += XYMetaHandle.CreateVector(a, b, (a.X + b.X) / 2, b.Y)
                ' points += XYMetaHandle.CreateVector(a, b, b.X, b.Y)

                edge.data.bends = points.ToArray
            Next

            Return g
        End Function
    End Module
End Namespace
