Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.Scripting

Namespace SVG

    Public Module Extensions

        ''' <summary>
        ''' 将画布<see cref="GraphicsSVG"/>之中的内容写入SVG文件
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="path$">``*.svg``保存的SVG文件的路径</param>
        ''' <returns></returns>
        <Extension> Public Function WriteSVG(g As GraphicsSVG, path$, Optional size$ = "1440,900") As Boolean
            Dim sz As Size = size.SizeParser
            Dim SVG As New SVGXml With {
                .circles = g.circles,
                .polygon = g.polygons,
                .rect = g.rects,
                .path = g.paths,
                .texts = g.texts,
                .lines = g.lines,
                .width = sz.Width,
                .height = sz.Height
            }

            If Not g.bg.StringEmpty Then
                SVG.style = New XmlMeta.CSS With {
                    .style = "svg{ background-color:" & g.bg & "}"
                }
            End If
            Return SVG.SaveAsXml(path,)
        End Function
    End Module
End Namespace