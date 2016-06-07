Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.Imaging.SVG.CSS
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MarkupLanguage.HTML
Imports Microsoft.VisualBasic.Language.UnixBash

''' <summary>
''' <see cref="NetworkGraph"/> to svg doc
''' </summary>
Public Module SVGExtensions

    Public Function DefaultStyle() As CSS.DirectedForceGraph
        Return New DirectedForceGraph With {
            .link = New CssValue With {
                .stroke = "#CCC",
                .strokeOpacity = "0.85",
                .strokeWidth = "3"
            },
            .node = New CssValue With {
                .strokeWidth = "0.5px",
                .strokeOpacity = "0.8",
                .stroke = "#FFF",
                .opacity = "0.85"
            }
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="graph"></param>
    ''' <param name="style">Default value is <see cref="DefaultStyle"/></param>
    ''' <param name="size">The export canvas size</param>
    ''' <returns></returns>
    <Extension>
    Public Function ToSVG(graph As NetworkGraph, size As Size, Optional style As CSS.DirectedForceGraph = Nothing) As SVGXml
        Dim rect As New Rectangle(New Point, size)
        Dim nodes As SVG.circle() =
            LinqAPI.Exec(Of SVG.circle) <= From n As Graph.Node
                                           In graph.nodes
                                           Let pos As Point = Renderer.GraphToScreen(n.Data.initialPostion, rect)
                                           Let c As Color = If(
                                               TypeOf n.Data.Color Is SolidBrush,
                                               DirectCast(n.Data.Color, SolidBrush).Color,
                                               Color.Black)
                                           Let r = n.Data.radius
                                           Let rd = If(r = 0!, If(n.Data.Neighborhoods < 30, n.Data.Neighborhoods * 9, n.Data.Neighborhoods * 7), r)
                                           Let r2 = If(rd = 0, 10, rd) / 2.5
                                           Let pt = New Point(pos.X - r2 / 2, pos.Y - r2 / 2)
                                           Select New circle With {
                                               .class = "node",
                                               .cx = pt.X,
                                               .cy = pt.Y,
                                               .r = r2,
                                               .style = $"fill: rgb({c.R}, {c.G}, {c.B});"
                                           }
        Dim links As line() =
            LinqAPI.Exec(Of line) <= From edge As Edge
                                     In graph.edges
                                     Let source As Graph.Node = edge.Source
                                     Let target As Graph.Node = edge.Target
                                     Let pts As Point = Renderer.GraphToScreen(source.Data.initialPostion, rect)
                                     Let ptt As Point = Renderer.GraphToScreen(target.Data.initialPostion, rect)
                                     Select New line With {
                                         .class = "link",
                                         .x1 = pts.X,
                                         .x2 = ptt.X,
                                         .y1 = pts.Y,
                                         .y2 = ptt.Y
                                     }
        Dim svg As New SVGXml With {
            .defs = New CSSStyles With {
                .styles = {
                    New XmlMeta.CSS With {
                        .style = If(style Is Nothing, DefaultStyle(), style).ToString
                    }
                }
            },
            .width = size.Width & "px",
            .height = size.Height & "px",
            .lines = links,
            .circles = nodes,
            .fill = "#dbf3ff"
        }

        Return svg
    End Function

    <Extension>
    Public Sub WriteLayouts(ByRef graph As NetworkGraph, engine As ForceDirected2D)
        For Each node As Graph.Node In graph.nodes
            node.Data.initialPostion =
                New FDGVector2(engine.GetPoint(node).position.Point2D)
        Next
    End Sub
End Module
