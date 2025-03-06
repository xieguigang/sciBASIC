#Region "Microsoft.VisualBasic::87938b36097dcfff7ec21aadcc7fbbd5, Data_science\Visualization\Plots-statistics\PCA\PC2.vb"

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

    '   Total Lines: 115
    '    Code Lines: 79 (68.70%)
    ' Comment Lines: 22 (19.13%)
    '    - Xml Docs: 77.27%
    ' 
    '   Blank Lines: 14 (12.17%)
    '     File Size: 4.57 KB


    '     Module PCAPlot
    ' 
    '         Function: PC2, PlotPC2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace PCA

    Public Module PCAPlot

        ''' <summary>
        ''' Plot for PCA + kmeans clustering
        ''' 
        ''' 将目标数据集通过PCA降维到二维数据，然后绘制散点图
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="sampleGroup">k parameter for kmeans</param>
        ''' <param name="labels$"></param>
        ''' <param name="size$"></param>
        ''' <param name="colorSchema$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function PC2(input As MultivariateAnalysisResult,
                            sampleGroup%,
                            Optional labels$() = Nothing,
                            Optional size$ = "2000,1800",
                            Optional colorSchema$ = "Set1:c8") As GraphicsData

            Dim score = input.GetPCAScore
            Dim x As Vector = score!PC1.AsVector
            Dim y As Vector = score!PC2.AsVector
            Dim getlabel As Func(Of Integer, String)

            If labels.IsNullOrEmpty Then
                getlabel = Function(i) "#" & (i + 1).FormatZero()
            Else
                getlabel = Function(i) labels(i)
            End If

            Dim pts As ClusterEntity() = Points(x, y) _
                .SeqIterator _
                .Select(Function(pt)
                            Dim point As PointF = pt.value

                            Return New ClusterEntity With {
                                .uid = getlabel(pt.i),
                                .entityVector = {
                                    point.X,
                                    point.Y
                                }
                            }
                        End Function) _
                .ToArray

            ' 进行聚类获取得到分组
            Dim kmeans As ClusterCollection(Of ClusterEntity) = New KMeansAlgorithm(Of ClusterEntity)().ClusterDataSet(pts, k:=sampleGroup)
            ' 赋值颜色到分组上
            Dim colors() = Designer.GetColors(colorSchema)
            ' 点为黑色的，border则才是所上的颜色
            Dim serials As New List(Of SerialData)

            For Each group In kmeans.SeqIterator
                Dim color As Color = colors(group)
                Dim stroke$ = New Stroke With {
                    .dash = DashStyle.Solid,
                    .fill = color.RGBExpression,
                    .width = 20
                }.ToString
                Dim points As PointData() = group _
                    .value _
                    .Select(Function(o)
                                Return New PointData With {
                                    .pt = New PointF(o(0), o(1)),
                                    .stroke = stroke
                                }
                            End Function) _
                    .ToArray
                Dim s As New SerialData With {
                    .color = Color.Black,
                    .pointSize = 5,
                    .title = "Cluster #" & (group.i + 1),
                    .pts = points
                }

                serials += s
            Next

            Dim dx = x.Max - x.Min
            Dim xaxis = $"({x.Min - dx / 5},{x.Max + dx / 5}),n=10"

            Return Bubble.Plot(serials, size, xAxis:=xaxis, strokeColorAsMainColor:=True)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">row is the samples</param>
        ''' <param name="groups">the data group labels of each row, 
        ''' this array size should be equals to the rows of 
        ''' <see cref="data"/></param>
        ''' <returns></returns>
        Public Function PlotPC2(data As GeneralMatrix, groups As String()) As GraphicsData

        End Function
    End Module
End Namespace
