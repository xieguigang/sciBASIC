#Region "Microsoft.VisualBasic::6b7d378f280752b7d8f84d6aedf50423, ..\sciBASIC#\Data_science\Mathematical\Plots\Heatmap.vb"

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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Correlations
Imports Microsoft.VisualBasic.Mathematical.Correlations.Correlations
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
                                Optional correlation As ICorrelation = Nothing) As NamedValue(Of Dictionary(Of String, Double))()

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

    Public Delegate Function ReorderProvider(data As NamedValue(Of Dictionary(Of String, Double))()) As NamedValue(Of Dictionary(Of String, Double))()

    ''' <summary>
    ''' 可以用来表示任意变量之间的相关度
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="customColors">
    ''' 可以使用这一组颜色来手动自定义heatmap的颜色，也可以使用<paramref name="mapName"/>来获取内置的颜色谱
    ''' </param>
    ''' <param name="mapLevels%"></param>
    ''' <param name="mapName$">The color map name. <see cref="Designer"/></param>
    ''' <param name="kmeans">Reorder datasets by using kmeans clustering</param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(data As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double))),
                         Optional customColors As Color() = Nothing,
                         Optional mapLevels% = 100,
                         Optional mapName$ = ColorMap.PatternJet,
                         Optional kmeans As ReorderProvider = Nothing,
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg$ = "white",
                         Optional fontStyle$ = CSSFont.Win10Normal,
                         Optional legendTitle$ = "Heatmap Color Legend",
                         Optional legendFont As Font = Nothing,
                         Optional min# = -1,
                         Optional max# = 1,
                         Optional mainTitle$ = "heatmap",
                         Optional titleFont As Font = Nothing,
                         Optional drawGrid As Boolean = True,
                         Optional drawValueLabel As Boolean = True,
                         Optional valuelabelFont As Font = Nothing) As Bitmap

        If valuelabelFont Is Nothing Then
            valuelabelFont = New Font(FontFace.CambriaMath, 16, Drawing.FontStyle.Bold)
        End If

        Return __plotInterval(
            Sub(g, region, array, left, font, dw, levels, top, colors)

                If Not kmeans Is Nothing Then
                    array = kmeans(array)  ' 因为可能会重新进行排序了，所以这里要在keys的申明之前完成
                End If

                Dim keys$() = array(Scan0).Value.Keys.ToArray
                Dim blockSize As New SizeF(dw, dw)

                margin = region.Margin

                For Each x As NamedValue(Of Dictionary(Of String, Double)) In array   ' 在这里绘制具体的矩阵
                    For Each key$ In keys
                        Dim c# = x.Value(key)
                        Dim level% = levels(c#)  '  得到等级
                        Dim color As Color = colors(
                                              If(level% > colors.Length - 1,
                                              colors.Length - 1,
                                              level))
                        Dim rect As New RectangleF(New PointF(left, top), blockSize)
                        Dim b As New SolidBrush(color)

                        Call g.FillRectangle(b, rect)

                        If drawGrid Then
                            Call g.DrawRectangles(Pens.WhiteSmoke, {rect})
                        End If
                        If drawValueLabel Then
                            key = c.FormatNumeric(2)
                            Dim ksz As SizeF = g.MeasureString(key, valuelabelFont)
                            Dim kpos As New PointF(rect.Left + (rect.Width - ksz.Width) / 2, rect.Top + (rect.Height - ksz.Height) / 2)
                            Call g.DrawString(key, valuelabelFont, Brushes.White, kpos)
                        End If

                        left.value += dw!
                    Next

                    left.value = margin.Width
                    top.value += dw!

                    Dim sz As SizeF = g.MeasureString(x.Name, font)
                    Dim y As Single = top.value - dw - (sz.Height - dw) / 2
                    Dim lx As Single =
                                          margin.Width - sz.Width - margin.Width * 0.1

                    Call g.DrawString(x.Name, font, Brushes.Black, New PointF(lx, y))
                Next
            End Sub, data.ToArray, customColors, mapLevels, mapName, size, margin, bg, fontStyle, legendTitle, legendFont, min, max, mainTitle, titleFont)
    End Function

    ''' <summary>
    ''' 一些共同的绘图元素过程
    ''' </summary>
    <Extension>
    Friend Function __plotInterval(plot As Action(Of Graphics, GraphicsRegion, NamedValue(Of Dictionary(Of String, Double))(), Value(Of Single), Font, Single, Dictionary(Of Double, Integer), Value(Of Single), Color()),
                                   array As NamedValue(Of Dictionary(Of String, Double))(),
                                   Optional colors As Color() = Nothing,
                                   Optional mapLevels% = 100,
                                   Optional mapName$ = ColorMap.PatternJet,
                                   Optional size As Size = Nothing,
                                   Optional margin As Size = Nothing,
                                   Optional bg$ = "white",
                                   Optional fontStyle$ = CSSFont.Win10Normal,
                                   Optional legendTitle$ = "Heatmap Color Legend",
                                   Optional legendFont As Font = Nothing,
                                   Optional min# = -1,
                                   Optional max# = 1,
                                   Optional mainTitle$ = "heatmap",
                                   Optional titleFont As Font = Nothing) As Bitmap

        Dim font As Font = CSSFont.TryParse(fontStyle).GDIObject
        Dim angle! = 45.0F

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

        If colors.IsNullOrEmpty Then
            colors = Designer.GetColors(mapName, mapLevels)
        End If

        Return GraphicsPlots(
            size, margin,
            bg$,
            Sub(ByRef g, region)
                Dim dw!? = CSng((size.Height - 2 * margin.Width) / array.Length)
                Dim correl#() = array _
                    .Select(Function(x) x.Value.Values) _
                    .IteratesALL _
                    .Join(min, max) _
                    .Distinct _
                    .ToArray
                Dim lvs As Dictionary(Of Double, Integer) =
                    correl _
                    .GenerateMapping(mapLevels, offset:=0) _
                    .SeqIterator _
                    .ToDictionary(Function(x) correl(x.i),
                                  Function(x) x.value)

                Dim left! = margin.Width, top! = margin.Height
                Dim keys$() = array(Scan0).Value.Keys.ToArray
                Dim getLeft As New Value(Of Single)(left)
                Dim getTop As New Value(Of Single)(top)

                Call plot(g, region, array, getLeft, font, dw, lvs, getTop, colors)

                left = getLeft
                top = getTop
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

                If titleFont Is Nothing Then
                    titleFont = New Font(FontFace.BookmanOldStyle, 30, Drawing.FontStyle.Bold)
                End If

                Dim titleSize = g.MeasureString(mainTitle, titleFont)
                Dim titlePosi As New PointF((left - titleSize.Width) / 2, (margin.Height - titleSize.Height) / 2)

                Call g.DrawString(mainTitle, titleFont, Brushes.Black, titlePosi)

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
End Module
