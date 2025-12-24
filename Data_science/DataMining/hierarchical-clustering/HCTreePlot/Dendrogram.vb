#Region "Microsoft.VisualBasic::b28cf70fcdfffa706b10f8b0f4d27ceb, Data_science\DataMining\hierarchical-clustering\HCTreePlot\Dendrogram.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 61
'    Code Lines: 55 (90.16%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 6 (9.84%)
'     File Size: 2.62 KB


' Module Dendrogram
' 
'     Function: Plot
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports std = System.Math

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
            .pointSize = pointSize,
            .gridStrokeX = linkStroke,
            .gridStrokeY = linkStroke,
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
                                .factor = i,
                                .name = name
                            }
                        End Function) _
                .ToArray
        End If

        Select Case layout
            Case Layouts.Vertical : Return New DendrogramPanelV2(hist, theme, colors, classinfo).Plot(size)
            Case Layouts.Horizon : Return New Horizon(hist, theme, colors, classinfo).Plot(size)
            Case Layouts.Radial : Return New RadialDendrogram(hist, theme, colors, classinfo, maxRadial:=std.PI).Plot(size)
            Case Else
                Throw New NotImplementedException(layout.ToString)
        End Select
    End Function
End Module
