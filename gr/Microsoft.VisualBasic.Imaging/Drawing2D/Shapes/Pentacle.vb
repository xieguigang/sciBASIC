#Region "Microsoft.VisualBasic::a82daa8f9131c4e6534d53be9fe01316, ..\visualbasic_App\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Pentacle.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing

Namespace Drawing2D.Vector.Shapes

    Public Class Pentacle

        Public Shared Sub Draw(ByRef g As Graphics,
                               topLeft As Point,
                               size As SizeF,
                               Optional br As Brush = Nothing,
                               Optional border As Border = Nothing)

            Dim pts(9) As Point
            Dim center As New Point(topLeft.X + size.Width / 2, topLeft.Y + size.Height / 2)
            Dim radius As Integer = Math.Min(size.Width, size.Height) / 2

            pts(0) = New Point(center.X, center.Y - radius)
            pts(1) = RotateTheta(pts(0), center, 36.0)

            Dim len As Double = radius * Math.Sin((18.0 * Math.PI / 180.0)) / Math.Sin((126.0 * Math.PI / 180.0))

            pts(1).X = CInt(center.X + len * (pts(1).X - center.X) / radius)
            pts(1).Y = CInt(center.Y + len * (pts(1).Y - center.Y) / radius)

            For i As Integer = 1 To 4
                pts((2 * i)) = RotateTheta(pts((2 * (i - 1))), center, 72.0)
                pts((2 * i + 1)) = RotateTheta(pts((2 * i - 1)), center, 72.0)
            Next

            If br Is Nothing Then
                Call g.DrawPolygon(If(border Is Nothing, New Pen(Color.Black), border.GetPen), pts) ' 画一个空心五角星
            Else
                g.FillPolygon(br, pts) ' 画一个实心的五角星

                If Not border Is Nothing Then
                    Call g.DrawPolygon(border.GetPen, pts)
                End If
            End If
        End Sub

        ''' <summary>
        ''' 旋转
        ''' </summary>
        ''' <param name="pt"></param>
        ''' <param name="center"></param>
        ''' <param name="theta"></param>
        ''' <returns></returns>
        Public Shared Function RotateTheta(ByVal pt As Point, ByVal center As Point, ByVal theta As Double) As Point
            Dim x As Integer = CInt(center.X + (pt.X - center.X) * Math.Cos((theta * Math.PI / 180)) - (pt.Y - center.Y) * Math.Sin((theta * Math.PI / 180)))
            Dim y As Integer = CInt(center.Y + (pt.X - center.X) * Math.Sin((theta * Math.PI / 180)) + (pt.Y - center.Y) * Math.Cos((theta * Math.PI / 180)))

            Return New Point(x, y)
        End Function
    End Class
End Namespace
