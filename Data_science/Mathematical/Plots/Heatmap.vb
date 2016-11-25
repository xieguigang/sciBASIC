#Region "Microsoft.VisualBasic::8a4e5a02bf6dc8cc25a7fae28c5bfbc8, ..\sciBASIC#\Data_science\Mathematical\Plots\Heatmap.vb"

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
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module Heatmap

    ''' <summary>
    ''' 相比于<see cref="LoadDataSet(String, String, Boolean, Correlations.ICorrelation)"/>函数，这个函数处理的是没有经过归一化处理的原始数据
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="correlation">假若这个参数为空，则默认使用<see cref="Correlations.GetPearson(Double(), Double())"/></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function CorrelatesNormalized(
                                    data As IEnumerable(Of DataSet),
                    Optional correlation As Correlations.ICorrelation = Nothing) _
                                         As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))

        Dim dataset As DataSet() = data.ToArray
        Dim keys$() = dataset(Scan0) _
            .Properties _
            .Keys _
            .ToArray

        If correlation Is Nothing Then
            correlation = AddressOf Correlations.GetPearson
        End If

        For Each x As DataSet In dataset
            Dim out As New Dictionary(Of String, Double)
            Dim array As Double() = keys.ToArray(Function(o$) x(o))

            For Each y As DataSet In dataset
                out(y.Identifier) = correlation(
                    array,
                    keys.ToArray(Function(o) y(o)))
            Next

            Yield New NamedValue(Of Dictionary(Of String, Double)) With {
                .Name = x.Identifier,
                .Value = out
            }
        Next
    End Function

    ''' <summary>
    ''' (这个函数是直接加在已经计算好了的相关度数据).假若使用这个直接加载数据来进行heatmap的绘制，
    ''' 请先要确保数据集之中的所有数据都是经过归一化的，假若没有归一化，则确保函数参数
    ''' <paramref name="normalization"/>的值为真
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="uidMap$"></param>
    ''' <param name="normalization">是否对输入的数据集进行归一化处理？</param>
    ''' <param name="correlation">
    ''' 默认为<see cref="Correlations.GetPearson(Double(), Double())"/>方法
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadDataSet(path As String,
                                Optional uidMap$ = Nothing,
                                Optional normalization As Boolean = False,
                                Optional correlation As Correlations.ICorrelation = Nothing) As NamedValue(Of Dictionary(Of String, Double))()

        Dim ds As IEnumerable(Of DataSet) =
            DataSet.LoadDataSet(path, uidMap)

        If normalization Then
            Return ds.CorrelatesNormalized(correlation).ToArray
        Else
            Return LinqAPI.Exec(Of NamedValue(Of Dictionary(Of String, Double))) _
               () <= From x As DataSet
                     In ds
                     Select New NamedValue(Of Dictionary(Of String, Double)) With {
                         .Name = x.Identifier,
                         .Value = x.Properties
                     }
        End If
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
                         Optional bg$ = "white",
                         Optional fontStyle$ = CSSFont.Win10Normal,
                         Optional legendTitle$ = "Heatmap Color Legend",
                         Optional legendFont As Font = Nothing,
                         Optional angle! = 45.0F) As Bitmap

        Dim font As Font = CSSFont.TryParse(fontStyle).GDIObject
        Dim array As NamedValue(Of
            Dictionary(Of String, Double))() = data.ToArray

        If margin.IsEmpty Then
            Dim maxLabel As String = LinqAPI.DefaultFirst(Of String) <=
                From x
                In array
                Select x.Name
                Order By Name.Length Descending

            Dim sz As Size = maxLabel.MeasureString(font)

            margin = New Size(sz.Width * 1.5, sz.Width * 1.5)
        End If

        size = If(size.IsEmpty, New Size(2000, 1600), size)

        Return GraphicsPlots(
            size, margin, bg$,
            Sub(ByRef g, region)
                Dim dw!? = CSng((size.Height - 2 * margin.Width) / array.Length)
                Dim correl#() = array _
                    .Select(Function(x) x.Value.Values) _
                    .IteratesALL _
                    .Distinct _
                    .ToArray
                Dim lvs As Dictionary(Of Double, Integer) =
                    correl _
                    .GenerateMapping(mapLevels, offset:=0) _
                    .SeqIterator _
                    .ToDictionary(Function(x) correl(x.i),
                                  Function(x) x.obj)

                If kmeans Then
                    array = array.KmeansReorder
                End If

                Dim left! = margin.Width, top! = margin.Height
                Dim blockSize As New SizeF(dw, dw)
                Dim keys$() = array(Scan0).Value.Keys.ToArray

                If colors.IsNullOrEmpty Then
                    colors = Designer.GetColors(mapName, mapLevels)
                End If

                For Each x As NamedValue(Of Dictionary(Of String, Double)) In array
                    For Each key$ In keys
                        Dim c# = x.Value(key)
                        Dim level% = lvs(c#)  '  得到等级
                        Dim color As Color = colors(
                            If(level% > colors.Length - 1,
                            colors.Length - 1,
                            level))
                        Dim rect As New RectangleF(New PointF(left, top), blockSize)
                        Dim b As New SolidBrush(color)

                        Call g.FillRectangle(b, rect)

                        left += dw!
                    Next

                    left = margin.Width
                    top += dw!

                    Dim sz As SizeF = g.MeasureString(x.Name, font)
                    Dim y As Single = top - dw - (sz.Height - dw) / 2
                    Dim lx As Single =
                        margin.Width - sz.Width - margin.Width * 0.1

                    Call g.DrawString(x.Name, font, Brushes.Black, New PointF(lx, y))
                Next

                angle = -angle
                left += dw / 2

                For Each key$ In keys
                    Dim sz = g.MeasureString(key$, font) ' 得到斜边的长度
                    Dim dx! = sz.Width * Math.Cos(angle)
                    Dim dy! = sz.Width * Math.Sin(angle)
                    Call g.DrawString(key$, font, Brushes.Black, left - dx, top - dy, angle)
                    left += dw
                Next

                ' Draw legends
                Dim legend As Bitmap = colors.ColorMapLegend(
                    haveUnmapped:=False,
                    min:=Math.Round(correl.Min, 1),
                    max:=Math.Round(correl.Max, 1),
                    title:=legendTitle,
                    titleFont:=legendFont)
                Dim lsize As Size = legend.Size
                Dim lmargin As Integer = size.Width - size.Height + margin.Width

                left = size.Width - lmargin
                top = size.Height / 3

                Dim scale# = lmargin / lsize.Width
                Dim lh% = CInt(scale * (size.Height * 2 / 3))

                Call g.DrawImage(
                    legend, CInt(left), CInt(top), lmargin, lh)

            End Sub)
    End Function

    ''' <summary>
    ''' 绘制按照任意角度旋转的文本
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="text"></param>
    ''' <param name="font"></param>
    ''' <param name="brush"></param>
    ''' <param name="x!"></param>
    ''' <param name="y!"></param>
    ''' <param name="angle!"></param>
    <Extension>
    Public Sub DrawString(g As Graphics, text$, font As Font, brush As Brush, x!, y!, angle!)
        g.TranslateTransform(x, y)     ' 先转换坐标系原点
        g.RotateTransform(angle)
        g.DrawString(text, font, brush, New PointF)
        g.ResetTransform()
    End Sub

    <Extension>
    Public Function KmeansReorder(data As NamedValue(Of Dictionary(Of String, Double))()) As NamedValue(Of Dictionary(Of String, Double))()
        Dim keys$() = data(Scan0%).Value.Keys.ToArray
        Dim entityList As Entity() = LinqAPI.Exec(Of Entity) <=
            From x As NamedValue(Of Dictionary(Of String, Double))
            In data
            Select New Entity With {
                .uid = x.Name,
                .Properties = keys.ToArray(Function(k) x.Value(k))
            }
        Dim clusters = ClusterDataSet(entityList.Length / 5, entityList)
        Dim out As New List(Of NamedValue(Of Dictionary(Of String, Double)))

        ' 通过kmeans计算出keys的顺序
        Dim keysEntity = keys.ToArray(
            Function(k) New Entity With {
                .uid = k,
                .Properties = data.ToArray(Function(x) x.Value(k))
            })
        Dim keysOrder As New List(Of String)

        For Each cluster In ClusterDataSet(CInt(keys.Length / 5), keysEntity)
            For Each k In cluster
                keysOrder += k.uid
            Next
        Next

        For Each cluster In clusters
            For Each entity As Entity In cluster
                out += New NamedValue(Of Dictionary(Of String, Double)) With {
                    .Name = entity.uid,
                    .Value = keysOrder _
                        .SeqIterator _
                        .ToDictionary(Function(x) x.obj,
                                      Function(x) entity.Properties(x.i))
                }
            Next
        Next

        Return out
    End Function
End Module
