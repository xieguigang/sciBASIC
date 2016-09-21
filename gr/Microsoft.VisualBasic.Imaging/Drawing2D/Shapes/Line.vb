#Region "Microsoft.VisualBasic::3fc6488c3c9f31856c4183df8852b14d, ..\visualbasic_App\Datavisualization\Microsoft.VisualBasic.Imaging\Drawing2D\VectorElements\Line.vb"

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
Imports Microsoft.VisualBasic.Imaging

Namespace Drawing2D.Vector.Shapes

    Public Class Line : Inherits Shape

        Dim pt1 As Point, pt2 As Point
        Dim BrushPen As Pen

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="pt1">在进行位移的时候，这两个点之间的相对位置不会发生改变</param>
        ''' <param name="pt2"></param>
        ''' <param name="Color"></param>
        ''' <param name="Width"></param>
        ''' <param name="GDI"></param>
        ''' <remarks></remarks>
        Sub New(pt1 As Point, pt2 As Point, Color As Color, Width As Integer, GDI As GDIPlusDeviceHandle)
            Call MyBase.New(GDI, pt1)
            Me.pt1 = pt1
            Me.pt2 = pt2
            Me.BrushPen = New Pen(New SolidBrush(Color), Width)
        End Sub

        Protected Overloads Overrides Sub InvokeDrawing()
            Dim p1 = Location
            Dim p2 = New Point(Me.Location.X + pt2.X - pt1.X, Me.Location.Y + pt2.Y - pt1.Y)
            Call Me._GDIDevice.Graphics.DrawLine(BrushPen, p1, p2)
        End Sub

        Public Overrides ReadOnly Property Size As Size
            Get
                Return New Size(pt2.X - pt1.X, pt2.Y - pt1.Y)
            End Get
        End Property
    End Class
End Namespace
