#Region "Microsoft.VisualBasic::0e832b585e6d0d9e381e4fd00d830a2d, gr\network-visualization\Datavisualization.Network\Graph\Model\Handle\WayPointVector.vb"

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

    '   Total Lines: 186
    '    Code Lines: 73 (39.25%)
    ' Comment Lines: 89 (47.85%)
    '    - Xml Docs: 58.43%
    ' 
    '   Blank Lines: 24 (12.90%)
    '     File Size: 7.43 KB


    '     Class WayPointVector
    ' 
    '         Properties: isNaN, xabsoluteshift, xoffsetscale, yabsoluteshift, yoffsetscale
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) CreateVector, (+2 Overloads) GetPoint, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports std = System.Math

Namespace Graph.EdgeBundling

    ' XYMetaHandle 类实际上是一种相对坐标或比例坐标的表示方法。它不记录路径点的绝对像素位置（如x=100,y=200），而是记录该点相对于“起点-终点”向量在 X 轴和 Y 轴上的缩放比例。
    '
    ' 它是如何“矢量化”的？
    '
    ' 假设有一条边连接源节点S和目标节点T。
    '
    ' S 的坐标是(sx,sy)
    ' T 的坐标是(tx,ty)
    ' 路径上有一个拐点H，其绝对坐标是(hx,hy)
    ' 
    ' XYMetaHandle 存储的是 kx 和 ky，计算逻辑如下
    ' 
    ' kx = (hx-sx)/(tx-sx)
    ' ky = (hy-sy)/(ty-sy)
    '
    ' 当需要绘图或计算实际路径时，通过反向公式还原：
    '
    ' x = sx+(tx-sx)*kx
    ' y = sy+(ty-sy)*ky
    '
    ' 这意味着，只要节点 S 和T 的位置发生了变化， 路径点H 会按照相同的比例“跟随”移动，从而保持路径的拓扑形状不变。

    ''' <summary>
    ''' 相对于<see cref="Handle"/>模型，这个矢量模型则是单纯的以xy偏移比例来进行矢量比例缩放
    ''' </summary>
    Public Class WayPointVector

        ' 定义一个很小的浮点数用于判断是否重合
        Const TOLERANCE As Double = 0.0001

        ''' <summary>
        ''' X轴方向的比例偏移量 (当两点间X距离不为0时使用)
        ''' </summary>
        Public Property xoffsetscale As Double

        ''' <summary>
        ''' Y轴方向的比例偏移量 (当两点间Y距离不为0时使用)
        ''' </summary>
        Public Property yoffsetscale As Double

        ''' <summary>
        ''' X轴方向的绝对像素偏移量 (当两点间X距离为0时使用)
        ''' </summary>
        Public Property xabsoluteshift As Double

        ''' <summary>
        ''' Y轴方向的绝对像素偏移量 (当两点间Y距离为0时使用)
        ''' </summary>
        Public Property yabsoluteshift As Double

        Public ReadOnly Property isNaN As Boolean
            Get
                Return xoffsetscale.IsNaNImaginary OrElse yoffsetscale.IsNaNImaginary
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' make value copy
        ''' </summary>
        ''' <param name="clone"></param>
        Sub New(clone As WayPointVector)
            xoffsetscale = clone.xoffsetscale
            yoffsetscale = clone.yoffsetscale
            xabsoluteshift = clone.xabsoluteshift
            yabsoluteshift = clone.yabsoluteshift
        End Sub

        Public Overrides Function ToString() As String
            Return $"Scale({xoffsetscale:F2}, {yoffsetscale:F2}) | Abs({xabsoluteshift:F1}, {yabsoluteshift:F1})"
        End Function

        ''' <summary>
        ''' 将当前的这个矢量描述转换为实际的点位置
        ''' </summary>
        ''' <param name="sx"></param>
        ''' <param name="sy"></param>
        ''' <param name="tx"></param>
        ''' <param name="ty"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' location of source node [<paramref name="sx"/>, <paramref name="sy"/>]
        ''' location of target node [<paramref name="tx"/>, <paramref name="ty"/>]
        ''' </remarks>
        Public Function GetPoint(sx#, sy#, tx#, ty#) As PointF
            Dim dx = tx - sx
            Dim dy = ty - sy
            Dim resX, resY As Double

            ' --- 计算 X 坐标 ---
            If std.Abs(dx) < TOLERANCE Then
                ' 情况1：两点在X轴重合（垂直线）。
                ' 此时无法使用比例缩放（避免除以0），使用存储的绝对偏移量。
                ' 如果是拖拽导致的重合，xabsoluteshift通常为0，这会让点保持在sx上（符合挤压逻辑）。
                ' 如果是创建时就是重合的，xabsoluteshift记录了当时的偏移。
                resX = sx + xabsoluteshift
            Else
                ' 情况2：两点X轴有距离，正常使用比例缩放
                resX = sx + dx * xoffsetscale
            End If

            ' --- 计算 Y 坐标 ---
            If std.Abs(dy) < TOLERANCE Then
                ' 情况1：两点在Y轴重合（水平线）。使用绝对偏移量。
                resY = sy + yabsoluteshift
            Else
                ' 情况2：两点Y轴有距离。正常使用比例缩放。
                resY = sy + dy * yoffsetscale
            End If

            Return New PointF(resX, resY)
        End Function

        ''' <summary>
        ''' calculate point based on the source point, target point and current route point
        ''' </summary>
        ''' <param name="vs"></param>
        ''' <param name="vt"></param>
        ''' <returns></returns>
        Public Function GetPoint(vs As Node, vt As Node) As PointF
            Dim ps = vs.data.initialPostion
            Dim pt = vt.data.initialPostion

            Return GetPoint(ps.x, ps.y, pt.x, pt.y)
        End Function

        ''' <summary>
        ''' 创建矢量化描述
        ''' </summary>
        ''' <param name="ps">location of the source node</param>
        ''' <param name="pt">location of the target node</param>
        ''' <param name="hx">the location x of the turn point</param>
        ''' <param name="hy">the location x of the turn point</param>
        ''' <returns></returns>
        Public Shared Function CreateVector(ps As PointF, pt As PointF, hx!, hy!) As WayPointVector
            Dim dx = pt.X - ps.X
            Dim dy = pt.Y - ps.Y
            Dim offsetX = hx - ps.X
            Dim offsetY = hy - ps.Y

            Dim handle As New WayPointVector()

            ' --- 处理 X 轴 ---
            If std.Abs(dx) < TOLERANCE Then
                ' 如果两点垂直重合，比例无效，记录绝对偏移
                handle.xoffsetscale = 0 ' 占位，实际上不使用
                handle.xabsoluteshift = offsetX
            Else
                ' 如果有距离，记录比例，绝对偏移归零
                handle.xoffsetscale = offsetX / dx
                handle.xabsoluteshift = 0
            End If

            ' --- 处理 Y 轴 ---
            If std.Abs(dy) < TOLERANCE Then
                ' 如果两点水平重合，比例无效，记录绝对偏移
                handle.yoffsetscale = 0 ' 占位
                handle.yabsoluteshift = offsetY
            Else
                ' 如果有距离，记录比例
                handle.yoffsetscale = offsetY / dy
                handle.yabsoluteshift = 0
            End If

            Return handle
        End Function

        ''' <summary>
        ''' Create an edge route path point
        ''' </summary>
        ''' <param name="ps">location of source vertex</param>
        ''' <param name="pt">location of target vertex</param>
        ''' <param name="handle">当前的这个需要进行矢量化描述的未知点坐标数据</param>
        ''' <returns></returns>
        Public Shared Function CreateVector(ps As PointF, pt As PointF, [handle] As PointF) As WayPointVector
            Return CreateVector(ps, pt, handle.X, handle.Y)
        End Function
    End Class
End Namespace
