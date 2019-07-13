#Region "Microsoft.VisualBasic::03139aafad0bf0b8ac5db6c599bdfb58, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Triangle.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Triangle
    ' 
    '         Properties: Angle, Color, Size, Vertex1, Vertex2
    '                     Vertex3
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: DrawAsRightTriangle
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Shapes

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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="topLeft"></param>
        ''' <param name="size"></param>
        ''' <param name="br"></param>
        ''' <param name="border"></param>
        ''' <param name="reversed">默认是顶部向上，如果reverse为真的话，则顶部向下</param>
        Public Overloads Shared Sub Draw(ByRef g As IGraphics,
                                         topLeft As Point,
                                         size As Size,
                                         Optional br As Brush = Nothing,
                                         Optional border As Stroke = Nothing,
                                         Optional reversed As Boolean = False)
            Dim t As New GraphicsPath

            If Not reversed Then
                Dim a As New Point(topLeft.X + size.Width / 2, topLeft.Y)
                Dim rect As New Rectangle(topLeft, size)
                Dim b As New Point(rect.Right, rect.Bottom)
                Dim c As New Point(rect.Left, rect.Bottom)

                Call t.AddLine(a, b)
                Call t.AddLine(b, c)
                Call t.AddLine(c, a)
            Else
                Dim b = topLeft
                Dim c = New Point(topLeft.X + size.Width, topLeft.Y)
                Dim a = New Point(topLeft.X + size.Width / 2, topLeft.Y + size.Height)

                Call t.AddLine(b, c)
                Call t.AddLine(c, a)
                Call t.AddLine(a, b)
            End If

            Call t.CloseAllFigures()
            Call g.FillPath(br Or BlackBrush, t)

            If Not border Is Nothing Then
                Call g.DrawPath(border.GDIObject, t)
            End If
        End Sub
    End Class
End Namespace
