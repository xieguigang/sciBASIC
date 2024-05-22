#Region "Microsoft.VisualBasic::538b725a890188152fb7d16dee349015, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Pentacle.vb"

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

    '   Total Lines: 62
    '    Code Lines: 39 (62.90%)
    ' Comment Lines: 10 (16.13%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (20.97%)
    '     File Size: 2.54 KB


    '     Class Pentacle
    ' 
    '         Function: PathData, RotateTheta
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports stdNum = System.Math

Namespace Drawing2D.Shapes

    ''' <summary>
    ''' 五角星
    ''' </summary>
    Public Class Pentacle

        Public Shared Sub Draw(ByRef g As IGraphics, topLeft As PointF, size As SizeF, Optional br As Brush = Nothing, Optional border As Stroke = Nothing)
            Dim pts As Point() = PathData(topLeft, size)

            If br Is Nothing Then
                Call g.DrawPolygon(If(border Is Nothing, New Pen(Color.Black), border.GDIObject), pts) ' 画一个空心五角星
            Else
                g.FillPolygon(br, pts) ' 画一个实心的五角星

                If Not border Is Nothing Then
                    Call g.DrawPolygon(border.GDIObject, pts)
                End If
            End If
        End Sub

        Public Shared Function PathData(topleft As PointF, size As SizeF) As Point()
            Dim pts(9) As Point
            Dim center As New Point(topleft.X + size.Width / 2, topleft.Y + size.Height / 2)
            Dim radius As Integer = Min(size.Width, size.Height) / 2

            pts(0) = New Point(center.X, center.Y - radius)
            pts(1) = RotateTheta(pts(0), center, 36.0)

            Dim len As Single = radius * Sin((18.0 * stdNum.PI / 180.0)) / Sin((126.0 * stdNum.PI / 180.0))

            pts(1).X = CInt(center.X + len * (pts(1).X - center.X) / radius)
            pts(1).Y = CInt(center.Y + len * (pts(1).Y - center.Y) / radius)

            For i As Integer = 1 To 4
                pts((2 * i)) = RotateTheta(pts((2 * (i - 1))), center, 72.0)
                pts((2 * i + 1)) = RotateTheta(pts((2 * i - 1)), center, 72.0)
            Next

            Return pts
        End Function

        ''' <summary>
        ''' 旋转
        ''' </summary>
        ''' <param name="pt"></param>
        ''' <param name="center"></param>
        ''' <param name="theta"></param>
        ''' <returns></returns>
        Public Shared Function RotateTheta(pt As Point, center As Point, theta As Single) As Point
            Dim x As Integer = CInt(center.X + (pt.X - center.X) * Cos((theta * stdNum.PI / 180)) - (pt.Y - center.Y) * Sin((theta * stdNum.PI / 180)))
            Dim y As Integer = CInt(center.Y + (pt.X - center.X) * Sin((theta * stdNum.PI / 180)) + (pt.Y - center.Y) * Cos((theta * stdNum.PI / 180)))

            Return New Point(x, y)
        End Function
    End Class
End Namespace
