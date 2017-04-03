Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Text

Namespace SVG

    Public Module Extensions

        ''' <summary>
        ''' 将画布<see cref="GraphicsSVG"/>之中的内容写入SVG文件
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="path$">``*.svg``保存的SVG文件的路径</param>
        ''' <returns></returns>
        <Extension> Public Function WriteSVG(g As GraphicsSVG, path$, Optional size$ = "1440,900") As Boolean
            Using file As StreamWriter = path.OpenWriter(Encodings.Unicode)
                Call g.WriteSVG(out:=file.BaseStream, size:=size)
                Return True
            End Using
        End Function

        <Extension> Public Function WriteSVG(g As GraphicsSVG, out As Stream, Optional size$ = "1440,900") As Boolean
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

            Dim XML$ = SVG.GetSVGXml
            Dim bytes As Byte() = Encoding.Unicode.GetBytes(XML)

            Call out.Write(bytes, Scan0, bytes.Length)
            Call out.Flush()

            Return True
        End Function
    End Module
End Namespace