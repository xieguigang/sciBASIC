Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace Drawing2D.VectorElements

    Public Class Line : Inherits LayoutsElement

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