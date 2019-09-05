Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver

''' <summary>
''' 小提琴图
''' 
''' + 高度为数据的分布位置
''' + 宽度为对应的百分位上的数据点的数量
''' + 长度为最小值与最大值之间的差值
''' 
''' </summary>
Public Module VolinPlot

    Public Function Plot(dataset As IEnumerable(Of NamedCollection(Of Double)),
                         Optional size$ = "3100,2700",
                         Optional margin$ = g.DefaultPadding,
                         Optional bg$ = "white", Optional colorset$ = DesignerTerms.TSFShellColors) As GraphicsData

    End Function
End Module
