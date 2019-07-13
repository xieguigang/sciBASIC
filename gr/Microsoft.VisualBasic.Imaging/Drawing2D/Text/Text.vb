#Region "Microsoft.VisualBasic::7bde37a7efa216c325a9b29960090210, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\Text.vb"

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

    '     Module TextRender
    ' 
    '         Function: DrawHtmlText
    ' 
    '         Sub: (+2 Overloads) DrawHtmlString, RenderHTML
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.Render
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports GdiImage = System.Drawing.Image

Namespace Drawing2D.Text

    ''' <summary>
    ''' 基于HTML语法的字符串的绘制描述信息的解析
    ''' </summary>
    Public Module TextRender

        ReadOnly HTMLtemplate$ = (
            <html>
                <head>
                    <style type="text/css">
                    body {
                        $font
                    }
                    </style>
                </head>
                <body>
                    $text
                </body>
            </html>).ToString

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="text$"></param>
        ''' <param name="CSS_style$"></param>
        ''' <param name="location">默认是 ``(0, 0)``</param>
        ''' <param name="maxWidth%"></param>
        <Extension>
        Public Sub RenderHTML(ByRef g As Graphics, text$, CSS_style$, Optional location As PointF = Nothing, Optional maxWidth% = 1024)
            Dim table As New Dictionary(Of String, String) From {
                {"font", CSS_style},
                {"text", text}
            }
            Dim html$ = HTMLtemplate _
                .Interpolate(table, nullAsEmpty:=True)

            Call HtmlRenderer.Render(
                g, html,
                location, maxWidth
            )
        End Sub

        ''' <summary>
        ''' Rendering the html text as gdi+ image
        ''' </summary>
        ''' <param name="label$">HTML</param>
        ''' <param name="cssFont$">For html ``&lt;p>...&lt;/p>`` css style</param>
        ''' <param name="maxSize$"></param>
        ''' <returns></returns>
        Public Function DrawHtmlText(label$, cssFont$, Optional maxSize$ = "1600,600") As GdiImage
            Using g As Graphics2D = New Size(1600, 600).CreateGDIDevice(Color.Transparent)
                Dim out As GdiImage

                TextRender.RenderHTML(g.Graphics, label, cssFont,, maxWidth:=g.Width)
                out = g.ImageResource
                out = out.CorpBlank(blankColor:=Color.Transparent)

                Return out
            End Using
        End Function

        <Extension>
        Public Sub DrawHtmlString(g As IGraphics, text$, baseFontStyle As Font, defaultColor As Color, location As Point)
            Dim tokens As TextString() = TextAPI _
                .TryParse(text, baseFontStyle, defaultColor) _
                .ToArray

            Call g.RenderHTML(tokens, New PointF(location.X, location.Y))
        End Sub

        <Extension>
        Public Sub DrawHtmlString(g As IGraphics, text$, baseFontStyle As Font, defaultColor As Color, location As PointF)
            Dim tokens As TextString() = TextAPI _
                .TryParse(text, baseFontStyle, defaultColor) _
                .ToArray

            Call g.RenderHTML(tokens, location)
        End Sub
    End Module
End Namespace
