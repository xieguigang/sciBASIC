#Region "Microsoft.VisualBasic::80fc21a29455d209471dd84d3d0d73e6, ..\sciBASIC#\Data_science\DataMining\Visualize\Kmeans.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module Kmeans

    Public Function Scatter2D()

    End Function

    ''' <summary>
    ''' 这个函数主要是生成<see cref="Serial3D"/>数据模型组
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="catagory">
    ''' How to read the data and construct the <see cref="Serial3D"/> model group
    ''' </param>
    ''' <param name="size$"></param>
    ''' <param name="bg$"></param>
    ''' <param name="padding$"></param>
    ''' <param name="clusterN">Expected kmeans cluster resulted number, default is 6 cluster</param>
    ''' <returns></returns>
    <Extension>
    Public Function Scatter3D(data As IEnumerable(Of DataSet),
                              catagory As Dictionary(Of NamedCollection(Of String)),
                              camera As Camera,
                              Optional size$ = "1200,1000",
                              Optional bg$ = "white",
                              Optional padding$ = g.DefaultPadding,
                              Optional clusterN% = 6,
                              Optional schema$ = Designer.Clusters,
                              Optional shapes As LegendStyles = LegendStyles.Circle Or LegendStyles.Square Or LegendStyles.Triangle,
                              Optional pointSize! = 20,
                              Optional boxStroke$ = Stroke.StrongHighlightStroke,
                              Optional axisStroke$ = Stroke.AxisStroke) As GraphicsData

        Dim clusters As Dictionary(Of String, EntityLDM()) = data _
            .ToKMeansModels _
            .Kmeans(expected:=clusterN) _
            .GroupBy(Function(point) point.Cluster) _
            .ToDictionary(Function(cluster) cluster.Key,
                          Function(group) group.ToArray)

        ' 相同的cluster的对象都会被染上同一种颜色
        ' 不同的分组之中的数据点则会被绘制为不同的形状
        Dim clusterColors As Color() = Designer.GetColors(schema)
        Dim serials As New List(Of Serial3D)
        Dim shapeList As LegendStyles() = GetAllEnumFlags(Of LegendStyles)(shapes)
        Dim keys$() = catagory.Keys.ToArray

        For Each cluster In clusters.SeqIterator
            Dim color As Color = clusterColors(cluster)
            Dim point3D As New List(Of Point3D)

            For Each member As EntityLDM In (+cluster).Value
                With keys _
                    .Select(Function(cat)
                                Return member(catagory(cat).Value).Average
                            End Function) _
                    .ToArray

                    Dim point As New Point3D(.ref(0), .ref(1), .ref(2))
                    point3D += point
                End With
            Next

            serials += New Serial3D With {
                .Title = (+cluster).Key,
                .Color = color,
                .Points = point3D,
                .Shape = LegendStyles.Triangle,
                .PointSize = pointSize
            }
        Next

        Return serials.Plot(
            camera, bg, padding,
            boxStroke:=boxStroke,
            axisStroke:=axisStroke)
    End Function
End Module

