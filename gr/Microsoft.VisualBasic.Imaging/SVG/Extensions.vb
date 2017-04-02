Imports System.Runtime.CompilerServices

Namespace SVG

    Public Module Extensions

        ''' <summary>
        ''' 将画布<see cref="GraphicsSVG"/>之中的内容写入SVG文件
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="path$">``*.svg``保存的SVG文件的路径</param>
        ''' <returns></returns>
        <Extension> Public Function WriteSVG(g As GraphicsSVG, path$) As Boolean
            Dim SVG As New SVGXml With {
                .circles = g.circles,
                .polygon = g.polygons,
                .rect = g.rects,
                .texts = g.texts,
                .lines = g.lines
            }
            Return SVG.SaveAsXml(path,)
        End Function
    End Module
End Namespace