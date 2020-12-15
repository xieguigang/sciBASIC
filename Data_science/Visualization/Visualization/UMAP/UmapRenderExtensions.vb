#Region "Microsoft.VisualBasic::7898971dbc71f1b96e14e29cd4d61b6d, Data_science\Visualization\Visualization\UMAP\UmapRender.vb"

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

' Module UmapRender
' 
'     Function: DrawUmap2D, GetPoint2D
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

<HideModuleName>
Public Module UmapRenderExtensions

    <Extension>
    Public Function GetPoint2D(umap As Umap) As PointF()
        If umap.dimension <> 2 Then
            Throw New InvalidProgramException($"the given umap projection result(dimension={umap.dimension}) is not a 2D data!")
        Else
            Return umap.GetEmbedding() _
                .Select(Function(vec)
                            Return New PointF With {.X = vec(0), .Y = vec(1)}
                        End Function) _
                .ToArray()
        End If
    End Function

    <Extension>
    Public Function GetPoint3D(umap As Umap) As Point3D()
        If umap.dimension <> 3 Then
            Throw New InvalidProgramException($"the given umap projection result(dimension={umap.dimension}) is not a 3D data!")
        Else
            Return umap.GetEmbedding _
                .Select(Function(vec)
                            Return New Point3D With {.X = vec(Scan0), .Y = vec(1), .Z = vec(2)}
                        End Function) _
                .ToArray
        End If
    End Function

    ''' <summary>
    ''' plot 2D projection result of umap
    ''' </summary>
    ''' <param name="umap"></param>
    ''' <param name="labels"></param>
    ''' <returns></returns>
    <Extension>
    Public Function DrawUmap2D(umap As Umap,
                               Optional labels As IEnumerable(Of String) = Nothing,
                               Optional size$ = "2048,1600",
                               Optional padding$ = g.DefaultPadding,
                               Optional colorSet$ = "Set1:c8") As GraphicsData
        Dim theme As New Theme With {
            .padding = padding
        }

        Return New Umap2D(
            umap:=umap,
            labels:=labels.SafeQuery.ToArray,
            clusters:=Nothing,
            colorSet:=colorSet,
            theme:=theme
        ).Plot(size)
    End Function

    <Extension>
    Public Function DrawUmap3D(umap As Umap, camera As Camera,
                               Optional labels As IEnumerable(Of String) = Nothing,
                               Optional size$ = "2048,2048",
                               Optional padding$ = g.DefaultPadding,
                               Optional bg$ = "white",
                               Optional colorSet$ = "Set1:c8",
                               Optional axisLabelCSS$ = CSSFont.PlotLabelNormal,
                               Optional axisStroke$ = Stroke.AxisStroke,
                               Optional labelCSS$ = CSSFont.Win10Normal,
                               Optional showLabels As Boolean = True) As GraphicsData

        Dim theme As New Theme With {
            .padding = padding,
            .axisLabelCSS = axisLabelCSS,
            .axisStroke = axisStroke,
            .tagCSS = labelCSS,
            .background = bg,
            .drawLabels = showLabels
        }

        Return New Umap3D(
            umap:=umap,
            labels:=labels.SafeQuery.ToArray,
            clusters:=Nothing,
            colorSet:=colorSet,
            theme:=theme,
            camera:=camera
        ).Plot(size)
    End Function
End Module

