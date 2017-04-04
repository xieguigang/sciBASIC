Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Text

Namespace SVG

    ''' <summary>
    ''' Write the data in <see cref="GraphicsSVG"/> as a xml file.
    ''' </summary>
    Public Module SVGWriter

        ''' <summary>
        ''' Get the current svg model data from current graphics engine.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="size$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function SVG(g As GraphicsSVG, Optional size$ = "1440,900") As SVGXml
            Return g.__svgData.GetSVG(size.SizeParser)
        End Function

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
            Dim svg As SVGXml = g.__svgData.GetSVG(sz)
            Dim XML$ = svg.GetSVGXml
            Dim bytes As Byte() = Encoding.Unicode.GetBytes(XML)

            Call out.Write(bytes, Scan0, bytes.Length)
            Call out.Flush()

            Return True
        End Function
    End Module
End Namespace