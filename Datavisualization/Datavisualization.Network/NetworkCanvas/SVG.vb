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
                .strokeOpacity = "0.6",
                .strokeWidth = "0.5"
            },
            .node = New CssValue With {
                .strokeWidth = "0.5px",
                .strokeOpacity = "0.8",
                .stroke = "#FFF",
                .opacity = "0.8"
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
            LinqAPI.Exec(Of SVG.node) <= From n As Graph.Node
                                         In graph.nodes
                                         Let pt As Point = Renderer.GraphToScreen(n.Data.initialPostion, rect)
                                         Let c As Color = If(
                                             TypeOf n.Data.Color Is SolidBrush,
                                             DirectCast(n.Data.Color, SolidBrush).Color,
                                             Color.Black)
                                         Select New circle With {
                                             .class = "node",
                                             .cx = pt.X,
                                             .cy = pt.Y,
                                             .r = n.Data.radius,
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
            .circles = nodes
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
