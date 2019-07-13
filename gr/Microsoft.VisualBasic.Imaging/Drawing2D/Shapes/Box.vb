#Region "Microsoft.VisualBasic::24b279c51eaa0ba051197529d1650c0b, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Box.vb"

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

    '     Class Box
    ' 
    '         Properties: Size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: DrawRectangle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Shapes

    Public Class Box : Inherits Shape

        Sub New(Location As Point, Size As Size, Color As Color)
            Call MyBase.New(Location)
        End Sub

        Public Overrides ReadOnly Property Size As Size

        Public Shared Sub DrawRectangle(ByRef g As IGraphics,
                                        topLeft As Point,
                                        size As Size,
                                        Optional br As Brush = Nothing,
                                        Optional border As Stroke = Nothing)

            Call g.FillRectangle(br Or BlackBrush, New Rectangle(topLeft, size))

            If Not border Is Nothing Then
                Call g.DrawRectangle(border.GDIObject, New Rectangle(topLeft, size))
            End If
        End Sub
    End Class
End Namespace
