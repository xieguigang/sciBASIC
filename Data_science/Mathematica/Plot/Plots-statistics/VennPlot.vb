Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver

''' <summary>
''' + 圆的半径大小直接与集合的大小相关
''' + 圆的交集部分的面积大小与两个集合之间的重合度相关
'''   1. 当没有重合度的时候，两个圆心距离为r1+r2，完全没有交集部分
'''   2. 当完全重合的时候，两个圆心距离为0，最小的集合的圆将会完全容纳在大一些的集合的圆之中
''' </summary>
Public Module VennPlot

    ''' <summary>
    ''' 绘制两个集合间的文氏图
    ''' </summary>
    ''' <returns></returns>
    Public Function Venn2(a As VennSet, b As VennSet,
                          Optional size$ = "3000,2600",
                          Optional margin$ = g.DefaultPadding,
                          Optional bg$ = "white") As GraphicsData

    End Function
End Module

Public Class VennSet : Implements INamedValue

    ''' <summary>
    ''' 对当前的这个集合的唯一标记字符串
    ''' </summary>
    ''' <returns></returns>
    Public Property Name As String Implements IKeyedEntity(Of String).Key

    ''' <summary>
    ''' 显示标题(可能不唯一)
    ''' </summary>
    ''' <returns></returns>
    Public Property Title As String
    ''' <summary>
    ''' 集合之中的元素数量
    ''' </summary>
    ''' <returns></returns>
    Public Property Size As Integer
    Public Property color As Brush

    ''' <summary>
    ''' 当前的这个集合与其他的集合之间的交集大小
    ''' </summary>
    ''' <returns></returns>
    Public Property intersections As Dictionary(Of String, Integer)

End Class