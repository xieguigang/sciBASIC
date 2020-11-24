Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module Dendrogram

    <Extension>
    Public Function Plot(hist As Cluster,
                         Optional classinfo As Dictionary(Of String, String) = Nothing,
                         Optional size$ = "2700,2100",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional colorSet$ = DesignerTerms.ClusterCategory10,
                         Optional axisTickCss$ = CSSFont.PlotLabelNormal,
                         Optional labelCss$ = CSSFont.PlotLabelNormal,
                         Optional pointSize% = 5,
                         Optional linkStroke$ = Stroke.HighlightStroke,
                         Optional axisStroke As String = Stroke.AxisStroke,
                         Optional layout As Layouts = Layouts.Vertical) As GraphicsData

        Dim theme As New Theme With {
            .background = bg,
            .padding = padding,
            .axisTickCSS = axisTickCss,
            .tagCSS = labelCss,
            .PointSize = pointSize,
            .gridStroke = linkStroke,
            .axisStroke = axisStroke
        }
        Dim colors As ColorClass() = Nothing

        If Not classinfo.IsNullOrEmpty Then
            Dim classNames = classinfo.Values.Distinct.ToArray
            Dim colorList = Designer.GetColors(colorSet).AsLoop

            colors = classNames _
                .Select(Function(name, i)
                            Return New ColorClass With {
                                .color = colorList.Next.ToHtmlColor,
                                .enumInt = i,
                                .name = name
                            }
                        End Function) _
                .ToArray
        End If

        If layout = Layouts.Vertical Then
            Return New DendrogramPanelV2(hist, theme, colors, classinfo).Plot(size)
        ElseIf layout = Layouts.Horizon Then
            Return New Horizon(hist, theme, colors, classinfo).Plot(size)
        Else
            Throw New NotImplementedException(layout.ToString)
        End If
    End Function
End Module
