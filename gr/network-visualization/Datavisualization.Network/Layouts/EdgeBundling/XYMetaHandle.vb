#Region "Microsoft.VisualBasic::14ea50db7e080b8446bb4d673f2cd2d1, gr\network-visualization\Datavisualization.Network\Layouts\EdgeBundling\XYMetaHandle.vb"

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

    '     Class XYMetaHandle
    ' 
    '         Properties: isNaN, xoffsetscale, yoffsetscale
    ' 
    '         Function: (+2 Overloads) CreateVector, GetPoint, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Layouts.EdgeBundling

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

        ''' <summary>
        ''' 将当前的这个矢量描述转换为实际的点位置
        ''' </summary>
        ''' <param name="sx#"></param>
        ''' <param name="sy#"></param>
        ''' <param name="tx#"></param>
        ''' <param name="ty#"></param>
        ''' <returns></returns>
        Public Function GetPoint(sx#, sy#, tx#, ty#) As PointF
            Dim dx = (tx - sx) * xoffsetscale
            Dim dy = (ty - sy) * yoffsetscale

            Return New PointF(sx + dx, sy + dy)
        End Function

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
