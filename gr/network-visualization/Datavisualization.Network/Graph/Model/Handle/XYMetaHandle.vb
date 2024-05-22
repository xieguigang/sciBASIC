#Region "Microsoft.VisualBasic::41519e22a949d1b41e068c6ac15da2d9, gr\network-visualization\Datavisualization.Network\Graph\Model\Handle\XYMetaHandle.vb"

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

    '   Total Lines: 88
    '    Code Lines: 44 (50.00%)
    ' Comment Lines: 30 (34.09%)
    '    - Xml Docs: 93.33%
    ' 
    '   Blank Lines: 14 (15.91%)
    '     File Size: 3.09 KB


    '     Class XYMetaHandle
    ' 
    '         Properties: isNaN, xoffsetscale, yoffsetscale
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) CreateVector, (+2 Overloads) GetPoint, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Graph.EdgeBundling

    ''' <summary>
    ''' 相对于<see cref="Handle"/>模型，这个矢量模型则是单纯的以xy偏移比例来进行矢量比例缩放
    ''' </summary>
    Public Class XYMetaHandle

        Public Property xoffsetscale As Double
        Public Property yoffsetscale As Double

        Public ReadOnly Property isNaN As Boolean
            Get
                Return xoffsetscale.IsNaNImaginary OrElse yoffsetscale.IsNaNImaginary
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(clone As XYMetaHandle)
            xoffsetscale = clone.xoffsetscale
            yoffsetscale = clone.yoffsetscale
        End Sub

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
            Dim dx = (tx - sx) * xoffsetscale
            Dim dy = (ty - sy) * yoffsetscale

            Return New PointF(sx + dx, sy + dy)
        End Function

        Public Function GetPoint(vs As Node, vt As Node) As PointF
            Dim ps = vs.data.initialPostion
            Dim pt = vt.data.initialPostion

            Return GetPoint(ps.x, ps.y, pt.x, pt.y)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ps">location of the source node</param>
        ''' <param name="pt">location of the target node</param>
        ''' <param name="hx"></param>
        ''' <param name="hy"></param>
        ''' <returns></returns>
        Public Shared Function CreateVector(ps As PointF, pt As PointF, hx!, hy!) As XYMetaHandle
            Dim dx = pt.X - ps.X
            Dim dy = pt.Y - ps.Y
            Dim offsetX = hx - ps.X
            Dim offsetY = hy - ps.Y

            Return New XYMetaHandle With {
                .xoffsetscale = offsetX / dx,
                .yoffsetscale = offsetY / dy
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ps"></param>
        ''' <param name="pt"></param>
        ''' <param name="handle">当前的这个需要进行矢量化描述的未知点坐标数据</param>
        ''' <returns></returns>
        Public Shared Function CreateVector(ps As PointF, pt As PointF, [handle] As PointF) As XYMetaHandle
            Return CreateVector(ps, pt, handle.X, handle.Y)
        End Function

        Public Overrides Function ToString() As String
            Return $"{xoffsetscale},{yoffsetscale}"
        End Function
    End Class
End Namespace
