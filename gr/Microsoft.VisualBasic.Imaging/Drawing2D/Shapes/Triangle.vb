#Region "Microsoft.VisualBasic::270a2a3ddb8251ab5ba9d98d66947b31, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Triangle.vb"

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
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging

Namespace Drawing2D.Vector.Shapes

    Public Class Triangle : Inherits Shape

        Public Property Color As Color

        Public Property Vertex1 As Point
        Public Property Vertex2 As Point
        Public Property Vertex3 As Point
        Public Property Angle As Single

        Sub New(Location As Point, Color As Color)
            Call MyBase.New(Location)
            Me.Color = Color
        End Sub

        ''' <summary>
        ''' 直角三角形
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DrawAsRightTriangle(a As Integer, b As Integer) As Triangle
            Throw New NotImplementedException
        End Function

        Public Overrides ReadOnly Property Size As Size
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Overloads Shared Sub Draw(ByRef g As Graphics, topLeft As Point, size As Size, Optional br As Brush = Nothing, Optional border As Border = Nothing)
            Dim t As New GraphicsPath
            Dim a As New Point(topLeft.X + size.Width / 2, topLeft.Y)
            Dim rect As New Rectangle(topLeft, size)
            Dim b As New Point(rect.Right, rect.Bottom)
            Dim c As New Point(rect.Left, rect.Bottom)

            Call t.AddLine(a, b)
            Call t.AddLine(b, c)
            Call t.AddLine(c, a)
            Call t.CloseAllFigures()

            Call g.FillPath(If(br Is Nothing, Brushes.Black, br), t)

            If Not border Is Nothing Then
                Call g.DrawPath(border.GetPen, t)
            End If
        End Sub
    End Class
End Namespace
