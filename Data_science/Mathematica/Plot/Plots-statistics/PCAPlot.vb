Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.DataMining.PCA
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix

Public Module PCAPlot

    <Extension> Public Function PC2(input As GeneralMatrix, sampleGroup%, Optional labels$() = Nothing, Optional colorSchema$ = "Set1:c12") As Image
        Dim result = input.PrincipalComponentAnalysis(nPC:=2)  ' x,y
        Dim x As Vector = result.ColumnVector(0)
        Dim y As Vector = result.ColumnVector(1)
        Dim pts As Entity() = Points(x, y) _
            .SeqIterator _
            .Select(Function(pt)
                        Dim point As PointF = pt.value
                        Return New Entity With {
                            .uid = "#" & pt.i,
                            .Properties = {point.X, point.Y}
                        }
                    End Function) _
            .ToArray

        ' 进行聚类获取得到分组
        Dim kmeans As ClusterCollection(Of Entity) = pts.ClusterDataSet(sampleGroup)
        ' 赋值颜色到分组上
        Dim colors$() = Designer.GetColors(colorSchema) _
            .Select(AddressOf Imaging.RGB2Hexadecimal) _
            .ToArray

        ' 点为黑色的，border则才是所上的颜色

    End Function
End Module
