#Region "Microsoft.VisualBasic::90a9f3e731f582c421ea50eb128e0ef6, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shadow.vb"

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

    '   Total Lines: 123
    '    Code Lines: 88 (71.54%)
    ' Comment Lines: 16 (13.01%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 19 (15.45%)
    '     File Size: 4.82 KB


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
Imports Microsoft.VisualBasic.Drawing.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

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
            Dim alpha! = angle / 180 * std.PI

            ' 计算出distance
            offset = New PointF With {
                .X = distance * std.Sin(alpha),
                .Y = distance * std.Cos(alpha)
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

#If NET48 Then
            Call Effects.DropdownShadows(g, circle, shadowColor, alphaLevels, gradientLevels)
#Else
            Throw New NotImplementedException
#End If
        End Sub

        Sub RoundRectangle(g As IGraphics, rect As Rectangle, radius!)
            Dim modification As Rectangle = rect.OffSet2D(offset).Scale(scale)
            Dim rectangle As GraphicsPath = Shapes.RoundRect.GetRoundedRectPath(modification, radius)

#If NET48 Then
            Call Effects.DropdownShadows(g, rectangle, shadowColor, alphaLevels, gradientLevels)
#Else
            Throw New NotImplementedException
#End If
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

#If NET48 Then
            Call Effects.DropdownShadows(g, circle, shadowColor, alphaLevels, gradientLevels)
#Else
            Throw New NotImplementedException
#End If
        End Sub

        Public Shared Sub DropdownShadows(g As IGraphics, polygon As GraphicsPath,
                                          Optional shadowColor$ = NameOf(Color.Gray),
                                          Optional alphaLevels$ = "0,120,150,200",
                                          Optional gradientLevels$ = "[0,0.125,0.5,1]")
#If NET48 Then
            Call Effects.DropdownShadows(g, polygon, shadowColor, alphaLevels, gradientLevels)
#Else
            Throw New NotImplementedException
#End If
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

#If NET48 Then
            Call Effects.DropdownShadows(g, path, shadowColor, alphaLevels, gradientLevels)
#Else
            Throw New NotImplementedException
#End If
        End Sub
    End Class
End Namespace
