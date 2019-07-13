#Region "Microsoft.VisualBasic::190be3f62c10744e750cbba41543e644, gr\Microsoft.VisualBasic.Imaging\SVG\Renderer.vb"

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

    '     Module Renderer
    ' 
    '         Function: DrawImage, PopulateLayers
    ' 
    '         Sub: drawLayer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace SVG

    ''' <summary>
    ''' Rendering svg vector image to gdi+ pixel image.
    ''' (将SVG图像渲染为gdi+图像<see cref="Drawing.Image"/>)
    ''' </summary>
    Public Module Renderer

        ''' <summary>
        ''' Rendering the SVG document as bitmap image.
        ''' </summary>
        ''' <param name="svg"></param>
        ''' <returns></returns>
        Public Function DrawImage(svg As SVGData) As Drawing.Image
            Using g As Graphics2D = svg.Size.CreateGDIDevice
                With g
                    Call .Clear(svg.SVG.bg.GetBrush)
                    Call .drawLayer(svg)

                    Return .ImageResource
                End With
            End Using
        End Function

        <Extension>
        Private Sub drawLayer(g As Graphics2D, layer As ICanvas)
            ' draw layer components, order by CSS zindex asc
            For Each element As CSSLayer In layer.PopulateLayers.OrderBy(Function(l) l.zIndex)
                Select Case element.GetType
                    Case GetType(g)

                        ' recursively draw svg document tree
                        Call g.drawLayer(element)

                    Case GetType(XML.circle)
                    Case GetType(XML.Image)
                    Case GetType(XML.line)
                    Case GetType(XML.node)
                    Case GetType(XML.path)
                    Case GetType(XML.polygon)
                    Case GetType(XML.polyline)
                    Case GetType(XML.rect)
                    Case GetType(XML.text)
                    Case GetType(XML.title)
                    Case Else
                        Throw New NotImplementedException(element.GetType.FullName)
                End Select
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PopulateLayers(obj As ICanvas) As IEnumerable(Of CSSLayer)
            Return obj.circles.Select(Function(e) DirectCast(e, CSSLayer)).AsList _
                 + obj.images _
                 + obj.lines _
                 + obj.path _
                 + obj.polygon _
                 + obj.polyline _
                 + obj.rect _
                 + obj.texts _
                 + obj.Layers
        End Function
    End Module
End Namespace
