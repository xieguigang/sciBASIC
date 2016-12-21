Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module HeatmapTable

    ''' <summary>
    ''' 只能够用来表示两两变量之间的相关度
    ''' </summary>
    ''' <param name="triangularStyle">
    ''' 是否上部分显示圆，下部分显示数值？默认是全部都显示圆圈
    ''' </param>
    ''' <returns></returns>
    Public Function Plot(data As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double))),
                         Optional mapLevels% = 100,
                         Optional mapName$ = ColorMap.PatternJet,
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg$ = "white",
                         Optional triangularStyle As Boolean = False,
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

        Dim colors As Color() = Designer.GetColors(mapName, mapLevels)

    End Function
End Module
