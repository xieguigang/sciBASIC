#Region "Microsoft.VisualBasic::13ac48f74261f4bf71476fddfb4520d0, ..\visualbasic_App\Data_science\Mathematical\Plots\Heatmap.vb"

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
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module Heatmap

    <Extension>
    Public Iterator Function Pearson(data As IEnumerable(Of DataSet)) As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))
        Dim dataset As DataSet() = data.ToArray
        Dim keys$() = dataset(Scan0).Properties.Keys.ToArray

        For Each x As DataSet In dataset
            Dim out As New Dictionary(Of String, Double)
            Dim array As Double() = keys.ToArray(Function(o$) x(o))

            For Each y As DataSet In dataset
                out(y.Identifier) = Correlations.GetPearson(
                    array,
                    keys.ToArray(Function(o) y(o)))
            Next

            Yield New NamedValue(Of Dictionary(Of String, Double)) With {
                .Name = x.Identifier,
                .x = out
            }
        Next
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="colors"></param>
    ''' <param name="mapLevels%"></param>
    ''' <param name="mapName$"></param>
    ''' <param name="kmeans">Reorder datasets by using kmeans clustering</param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(data As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double))),
                         Optional colors As Color() = Nothing,
                         Optional mapLevels% = 100,
                         Optional mapName$ = ColorMap.PatternJet,
                         Optional kmeans As Boolean = True,
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg$ = "white") As Bitmap

        Return GraphicsPlots(
            If(size.IsEmpty, New Size(1600, 1600), size),
            margin,
            bg,
            Sub(ByRef g, region)
                Dim array As NamedValue(Of Dictionary(Of String, Double))() =
                    data.ToArray
                Dim dw! = CSng(region.GraphicsRegion.Width / array.Length)
                Dim correl#() = array _
                    .Select(Function(x) x.x.Values) _
                    .MatrixAsIterator _
                    .Distinct _
                    .ToArray
                Dim lvs As Dictionary(Of Double, Integer) =
                    correl _
                    .GenerateMapping(CInt(mapLevels / 2%) - 1) _
                    .SeqIterator _
                    .ToDictionary(Function(x) correl(x.i),
                                  Function(x) x.obj)

                If kmeans Then
                    array = array.KmeansReorder
                End If

                Dim left! = margin.Width, top! = margin.Height
                Dim blockSize As New SizeF(dw, dw)
                Dim keys$() = array(Scan0).x.Keys.ToArray

                If colors.IsNullOrEmpty Then
                    colors = New ColorMap(mapLevels).ColorSequence(mapName)
                End If

                For Each x As NamedValue(Of Dictionary(Of String, Double)) In array
                    For Each key$ In keys
                        Dim c# = x.x(key)
                        Dim level% = lvs(c#) - 1 '  得到等级
                        Dim color As Color = colors(level%)
                        Dim rect As New RectangleF(New PointF(left, top), blockSize)
                        Dim b As New SolidBrush(color)

                        Call g.FillRectangle(b, rect)

                        left += dw!
                    Next

                    left = margin.Width
                    top += dw!
                Next
            End Sub)
    End Function

    <Extension>
    Public Function KmeansReorder(data As NamedValue(Of Dictionary(Of String, Double))()) As NamedValue(Of Dictionary(Of String, Double))()
        Dim keys$() = data(Scan0%).x.Keys.ToArray
        Dim entityList As Entity() = LinqAPI.Exec(Of Entity) <=
            From x As NamedValue(Of Dictionary(Of String, Double))
            In data
            Select New Entity With {
                .uid = x.Name,
                .Properties = keys.ToArray(Function(k) x.x(k))
            }
        Dim clusters = ClusterDataSet(entityList.Length / 5, entityList)
        Dim out As New List(Of NamedValue(Of Dictionary(Of String, Double)))

        For Each cluster In clusters
            For Each entity As Entity In cluster
                out += New NamedValue(Of Dictionary(Of String, Double)) With {
                    .Name = entity.uid,
                    .x = keys _
                        .SeqIterator _
                        .ToDictionary(Function(x) x.obj,
                                      Function(x) entity.Properties(x.i))
                }
            Next
        Next

        Return out
    End Function
End Module

