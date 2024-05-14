#Region "Microsoft.VisualBasic::65975eadc33fb63cb7f11d8015e228d5, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shadow.vb"

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

    '   Total Lines: 148
    '    Code Lines: 85
    ' Comment Lines: 41
    '   Blank Lines: 22
    '     File Size: 6.73 KB


    '     Class Shadow
    ' 
    '         Properties: alphaLevels, gradientLevels, shadowColor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Circle, DrawCircleShadow, (+2 Overloads) DropdownShadows, RoundRectangle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

Namespace Drawing2D

    Public Class Shadow

        Dim offset As PointF
        Dim scale As SizeF

        Public Property shadowColor As String = NameOf(Color.Gray)
        Public Property alphaLevels As String = "0,120,150,200"
        Public Property gradientLevels As String = "[0,0.125,0.5,1]"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="distance">正实数</param>
        ''' <param name="angle">[0, 360]</param>
        ''' <param name="scaleX!"></param>
        ''' <param name="scaleY!"></param>
        Sub New(distance!, angle!, Optional scaleX! = 1, Optional scaleY! = 1)
            Dim alpha! = angle / 180 * stdNum.PI

            ' 计算出distance
            offset = New PointF With {
                .X = distance * stdNum.Sin(alpha),
                .Y = distance * stdNum.Cos(alpha)
            }
            scale = New SizeF With {.Width = scaleX, .Height = scaleY}
        End Sub

        Sub Circle(g As IGraphics, centra As PointF, radius!)
            Dim circle As New GraphicsPath()
            Dim points As PointF() = Shapes.Circle _
                .PathIterator(centra, radius, 100) _
                .Select(Function(pt)
                            Return pt.OffSet2D(offset)
                        End Function) _
                .Enlarge(scale)
            Dim a As PointF = points(Scan0)

            For Each vertex As PointF In points.Skip(1)
                Call circle.AddLine(a, vertex)
            Next

            Call DropdownShadows(g, circle, shadowColor, alphaLevels, gradientLevels)
        End Sub

        Sub RoundRectangle(g As IGraphics, rect As Rectangle, radius!)
            Dim modification As Rectangle = rect.OffSet2D(offset).Scale(scale)
            Dim rectangle As GraphicsPath = Shapes.RoundRect.GetRoundedRectPath(modification, radius)

            Call DropdownShadows(g, rectangle, shadowColor, alphaLevels, gradientLevels)
        End Sub

        Public Shared Sub DrawCircleShadow(g As IGraphics, centra As PointF, radius As Single,
                                           Optional shadowColor$ = NameOf(Color.Gray),
                                           Optional alphaLevels$ = "0,120,150,200",
                                           Optional gradientLevels$ = "[0,0.125,0.5,1]")

            Dim circle As New GraphicsPath()
            Dim points As PointF() = Shapes.Circle.PathIterator(centra, radius, 100).ToArray
            Dim a As PointF = points(Scan0)

            For Each vertex As PointF In points.Skip(1)
                Call circle.AddLine(a, vertex)
            Next

            Call DropdownShadows(g, circle, shadowColor, alphaLevels, gradientLevels)
        End Sub

        ''' <summary>
        ''' Draw shadow of a specifc <paramref name="rectangle"/>
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="rectangle"></param>
        ''' <param name="shadowColor$"></param>
        ''' <param name="alphaLevels$"></param>
        ''' <param name="gradientLevels$"></param>
        Public Shared Sub DropdownShadows(g As IGraphics, rectangle As RectangleF,
                                          Optional shadowColor$ = NameOf(Color.Gray),
                                          Optional alphaLevels$ = "0,120,150,200",
                                          Optional gradientLevels$ = "[0,0.125,0.5,1]")
            Dim path As New GraphicsPath

            Call path.AddRectangle(rectangle)
            Call path.CloseAllFigures()
            Call DropdownShadows(g, path, shadowColor, alphaLevels, gradientLevels)
        End Sub

        ''' <summary>
        ''' Draw shadow of a specifc <paramref name="polygon"/>
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="polygon"></param>
        ''' <param name="shadowColor$"></param>
        ''' <param name="alphaLevels$"></param>
        ''' <param name="gradientLevels$"></param>
        Public Shared Sub DropdownShadows(g As IGraphics, polygon As GraphicsPath,
                                          Optional shadowColor$ = NameOf(Color.Gray),
                                          Optional alphaLevels$ = "0,120,150,200",
                                          Optional gradientLevels$ = "[0,0.125,0.5,1]")

            Dim alphas As Vector = alphaLevels
            ' Create a color blend to manage our colors And positions And
            ' since we need 3 colors set the default length to 3
            Dim colorBlend As New ColorBlend(alphas.Length)
            Dim baseColor As Color = shadowColor.TranslateColor

            ' here Is the important part of the shadow making process, remember
            ' the clamp mode on the colorblend object layers the colors from
            ' the outside to the center so we want our transparent color first
            ' followed by the actual shadow color. Set the shadow color to a 
            ' slightly transparent DimGray, I find that it works best.|
            colorBlend.Colors = alphas _
                .Select(Function(a) Color.FromArgb(a, baseColor)) _
                .ToArray

            ' our color blend will control the distance of each color layer
            ' we want to set our transparent color to 0 indicating that the 
            ' transparent color should be the outer most color drawn, then
            ' our Dimgray color at about 10% of the distance from the edge
            colorBlend.Positions = CType(gradientLevels, Vector).AsSingle

            If TypeOf g Is Graphics2D Then
                ' this Is where we create the shadow effect, so we will use a 
                ' pathgradientbursh And assign our GraphicsPath that we created of a 
                ' Rounded Rectangle
                Using pgBrush As New PathGradientBrush(polygon) With {
                    .WrapMode = WrapMode.Clamp,
                    .InterpolationColors = colorBlend
                }
                    ' fill the shadow with our pathgradientbrush
                    Call g.FillPath(pgBrush, polygon)
                End Using
            Else
                ' not sure how to implements a gradient brush in svg/ps
                ' just do a normal shape fill
                Call g.FillPath(New SolidBrush(baseColor), polygon)
            End If
        End Sub
    End Class
End Namespace
