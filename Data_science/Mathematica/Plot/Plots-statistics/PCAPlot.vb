Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.DataMining.PCA
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module PCAPlot

    <Extension> Public Function PC2(input As GeneralMatrix,
                                    sampleGroup%,
                                    Optional labels$() = Nothing,
                                    Optional size$ = "2000,1800",
                                    Optional colorSchema$ = "Set1:c8") As GraphicsData

        Dim result = input.PrincipalComponentAnalysis(nPC:=2)  ' x,y
        Dim x As Vector = result.ColumnVector(0)
        Dim y As Vector = result.ColumnVector(1)
        Dim getlabel As Func(Of Integer, String)

        If labels.IsNullOrEmpty Then
            getlabel = Function(i) "#" & (i + 1).FormatZero()
        Else
            getlabel = Function(i) labels(i)
        End If

        Dim pts As Entity() = Points(x, y) _
            .SeqIterator _
            .Select(Function(pt)
                        Dim point As PointF = pt.value
                        Return New Entity With {
                            .uid = getlabel(pt.i),
                            .Properties = {point.X, point.Y}
                        }
                    End Function) _
            .ToArray

        ' 进行聚类获取得到分组
        Dim kmeans As ClusterCollection(Of Entity) = pts.ClusterDataSet(sampleGroup)
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
                .PointSize = 5,
                .title = "Cluster #" & (group.i + 1),
                .pts = points
            }

            serials += s
        Next

        Dim dx = x.Max - x.Min
        Dim xaxis = $"({x.Min - dx / 5},{x.Max + dx / 5}),n=10"

        Return Bubble.Plot(serials, size.SizeParser, xAxis:=xaxis, strokeColorAsMainColor:=True)
    End Function
End Module
