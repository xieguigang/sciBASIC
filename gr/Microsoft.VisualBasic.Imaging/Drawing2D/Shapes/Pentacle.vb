#Region "Microsoft.VisualBasic::dd3423602f49b87c7ddacac3a9b385f4, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Pentacle.vb"

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
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Shapes

    ''' <summary>
    ''' 五角星
    ''' </summary>
    Public Class Pentacle

        Public Shared Sub Draw(ByRef g As IGraphics, topLeft As Point, size As SizeF, Optional br As Brush = Nothing, Optional border As Stroke = Nothing)
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

        Public Shared Function PathData(topleft As Point, size As SizeF) As Point()
            Dim pts(9) As Point
            Dim center As New Point(topleft.X + size.Width / 2, topleft.Y + size.Height / 2)
            Dim radius As Integer = Min(size.Width, size.Height) / 2

            pts(0) = New Point(center.X, center.Y - radius)
            pts(1) = RotateTheta(pts(0), center, 36.0)

            Dim len As Single = radius * Sin((18.0 * Math.PI / 180.0)) / Sin((126.0 * Math.PI / 180.0))

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
            Dim x As Integer = CInt(center.X + (pt.X - center.X) * Cos((theta * Math.PI / 180)) - (pt.Y - center.Y) * Sin((theta * Math.PI / 180)))
            Dim y As Integer = CInt(center.Y + (pt.X - center.X) * Sin((theta * Math.PI / 180)) + (pt.Y - center.Y) * Cos((theta * Math.PI / 180)))

            Return New Point(x, y)
        End Function
    End Class
End Namespace
