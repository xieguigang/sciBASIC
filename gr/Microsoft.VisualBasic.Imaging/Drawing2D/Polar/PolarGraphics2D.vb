Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Drawing2D

    ''' <summary>
    ''' 以极坐标为主的作图系统模块
    ''' </summary>
    Public Class PolarGraphics2D

        ''' <summary>
        ''' 通过极坐标计算出二维直角坐标的圆心中心点
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property center As PointF
        ''' <summary>
        ''' the graphics canvas module
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property canvas As IGraphics

        Public ReadOnly Property Size As Size
            Get
                Return canvas.Size
            End Get
        End Property

        Sub New(canvas As IGraphics, center As PointF)
            Me.canvas = canvas
            Me.center = center
        End Sub

        ''' <summary>
        ''' Measures the specified string when drawn with the specified <see cref="System.Drawing.Font"/>.
        ''' </summary>
        ''' <param name="text">String to measure.</param>
        ''' <param name="font">System.Drawing.Font that defines the text format of the string.</param>
        ''' <returns>This method returns a System.Drawing.SizeF structure that represents the size,
        ''' in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        ''' string specified by the text parameter as drawn with the font parameter.
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function MeasureString(text As String, font As Font) As SizeF
            Return canvas.MeasureString(text, font)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Translate(point As PolarPoint) As PointF
            Return point.Translate(center)
        End Function

        ''' <summary>
        ''' Draws the specified text string at the specified location with the specified
        ''' <see cref="Brush"/> and <see cref="Font"/> objects.
        ''' </summary>
        ''' <param name="s">String to draw.</param>
        ''' <param name="font">System.Drawing.Font that defines the text format of the string.</param>
        ''' <param name="brush">System.Drawing.Brush that determines the color and texture of the drawn text.</param>
        ''' <param name="point">System.Drawing.PointF structure that specifies the upper-left corner of the drawn
        ''' text.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub DrawString(s As String, font As Font, brush As Brush, point As PolarPoint)
            Call canvas.DrawString(s, font, brush, Translate(point))
        End Sub

        ''' <summary>
        ''' Draws a line connecting two System.Drawing.PointF structures.
        ''' </summary>
        ''' <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the line.</param>
        ''' <param name="pt1"><see cref="PolarPoint"/> structure that represents the first point to connect.</param>
        ''' <param name="pt2"><see cref="PolarPoint"/> structure that represents the second point to connect.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub DrawLine(pen As Pen, pt1 As PolarPoint, pt2 As PolarPoint)
            Call canvas.DrawLine(pen, Translate(pt1), Translate(pt2))
        End Sub

        ''' <summary>
        ''' Draws the specified <see cref="Image"/>, using its original physical size, at
        ''' the specified location.
        ''' </summary>
        ''' <param name="image"><see cref="Image"/> to draw.</param>
        ''' <param name="point"><see cref="PolarPoint"/> structure that represents the upper-left corner of the
        ''' drawn image.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub DrawImage(image As Image, point As PolarPoint)
            Call canvas.DrawImage(image, Translate(point))
        End Sub

    End Class
End Namespace