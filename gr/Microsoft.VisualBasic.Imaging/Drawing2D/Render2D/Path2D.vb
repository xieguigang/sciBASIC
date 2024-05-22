#Region "Microsoft.VisualBasic::bf9d1c76cfe9fc4a2da6486333a0b8b0, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Render2D\Path2D.vb"

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

    '   Total Lines: 153
    '    Code Lines: 104 (67.97%)
    ' Comment Lines: 31 (20.26%)
    '    - Xml Docs: 90.32%
    ' 
    '   Blank Lines: 18 (11.76%)
    '     File Size: 5.79 KB


    '     Class Path2D
    ' 
    '         Properties: Path
    ' 
    '         Function: ToString
    ' 
    '         Sub: CloseAllFigures, CurveTo, EllipticalArc, HorizontalTo, (+2 Overloads) LineTo
    '              (+2 Overloads) MoveTo, QuadraticBelzier, Rewind, SmoothCurveTo, VerticalTo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Line2D = Microsoft.VisualBasic.Imaging.Drawing2D.Shapes.Line

Namespace Drawing2D

    ''' <summary>
    ''' 通过模拟HTML之中的svg path的绘图操作来将html svg path转换为gdi+ path对象
    ''' </summary>
    Public Class Path2D

        Public ReadOnly Property Path As New GraphicsPath

        Dim last As PointF

        Public Sub MoveTo(x!, y!, Optional relative As Boolean = False)
            If Not relative Then
                last = New PointF(x, y)
            Else
                last = last.OffSet2D(x, y)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub MoveTo(location As PointF, Optional relative As Boolean = False)
            last = location
        End Sub

        Public Sub HorizontalTo(x!, Optional relative As Boolean = False)
            If Not relative Then
                Call LineTo(x, last.Y)
            Else
                Call LineTo(last.X + x, last.Y)
            End If
        End Sub

        Public Sub VerticalTo(y!, Optional relative As Boolean = False)
            If Not relative Then
                Call LineTo(last.X, y)
            Else
                Call LineTo(last.X, last.Y + y)
            End If
        End Sub

        Public Sub LineTo(x!, y!, Optional relative As Boolean = False)
            If Not relative Then
                Call LineTo(New PointF(x, y))
            Else
                Call LineTo(last.OffSet2D(x, y))
            End If
        End Sub

        Public Sub LineTo(location As PointF)
            Call Path.AddLine(last, location)
            last = location
        End Sub

        ''' <summary>
        ''' 三次贝塞曲线
        ''' </summary>
        ''' <param name="x1#">第一控制点X</param>
        ''' <param name="y1#">第一控制点Y</param>
        ''' <param name="x2#">第二控制点X</param>
        ''' <param name="y2#">第二控制点Y</param>
        ''' <param name="endX#">曲线结束点X</param>
        ''' <param name="endY#">曲线结束点Y</param>
        Public Sub CurveTo(x1#, y1#, x2#, y2#, endX#, endY#, Optional relative As Boolean = False)
            If relative Then
                With last.OffSet2D(endX, endY)
                    Call Path.AddBezier(
                        last, last.OffSet2D(x1, y1), last.OffSet2D(x2, y2), .ByRef
                    )
                    last = .ByRef
                End With
            Else
                With New PointF(endX, endY)
                    Call Path.AddBezier(
                        last, New PointF(x1, y1), New PointF(x2, y2), .ByRef
                    )
                    last = .ByRef
                End With
            End If
        End Sub

        Public Sub SmoothCurveTo(x2#, y2#, endX#, endY#, Optional relative As Boolean = False)
            If relative Then
                With last.OffSet2D(endX, endY)
                    Call Path.AddCurve({last, last.OffSet2D(x2, y2), .ByRef})
                    last = .ByRef
                End With
            Else
                With New PointF(endX, endY)
                    Call Path.AddCurve({last, New PointF(x2, y2), .ByRef})
                    last = .ByRef
                End With
            End If
        End Sub

        Public Sub QuadraticBelzier(x#, y#, endX#, endY#, Optional relative As Boolean = False)
            If Not relative Then
                With New PointF(endX, endY)
                    Call Path.AddLines(Line2D.QuadraticBelzier(last, New PointF(x, y), .ByRef).ToArray)
                    last = .ByRef
                End With
            Else
                With last.OffSet2D(endX, endY)
                    Call Path.AddLines(Line2D.QuadraticBelzier(last, last.OffSet2D(x, y), .ByRef).ToArray)
                    last = .ByRef
                End With
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rX!">所在椭圆的半轴大小</param>
        ''' <param name="rY!">所在椭圆的半轴大小</param>
        ''' <param name="xrotation!">椭圆的X轴与水平方向顺时针方向夹角，可以想像成一个水平的椭圆绕中心点顺时针旋转XROTATION的角度。</param>
        ''' <param name="flag1!">1表示大角度弧线，0为小角度弧线。</param>
        ''' <param name="flag2!">确定从起点至终点的方向，1为顺时针，0为逆时针</param>
        ''' <param name="x!">终点坐标</param>
        ''' <param name="y!">终点坐标</param>
        ''' <param name="relative"></param>
        Public Sub EllipticalArc(rX!, rY!, xrotation!, flag1!, flag2!, x!, y!, Optional relative As Boolean = False)

        End Sub

        ''' <summary>
        ''' 重置
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Rewind()
            Call Path.Reset()
        End Sub

        ''' <summary>
        ''' 关闭所有打开的数字，在此路径，并开始一个新图形。 通过将行从其终结点连接到其起始点，则关闭每个打开的图形。
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub CloseAllFigures()
            Call Path.CloseAllFigures()
        End Sub

        Public Overrides Function ToString() As String
            Return Path.ToString
        End Function
    End Class
End Namespace
