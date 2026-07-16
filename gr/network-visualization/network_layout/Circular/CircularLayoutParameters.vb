Imports System.ComponentModel

Namespace Circular

    ''' <summary>
    ''' 环形布局参数，可在 PropertyGrid 中编辑
    ''' </summary>
    Public Class CircularLayoutParameters

        <Category("布局"), DisplayName("半径"), Description("圆周半径（像素），<=0 时自动推算")>
        Public Property Radius As Double = Double.NaN

        <Category("布局"), DisplayName("圆心 X")>
        Public Property CenterX As Double = 500.0

        <Category("布局"), DisplayName("圆心 Y")>
        Public Property CenterY As Double = 500.0

        <Category("布局"), DisplayName("按度排序"), Description("按节点度排序以优化视觉")>
        Public Property SortByDegree As Boolean = True

        <Category("优化"), DisplayName("交叉优化"), Description("是否启用 2-opt 交叉最小化（较慢）")>
        Public Property OptimizeCrossing As Boolean = False

        <Category("优化"), DisplayName("最大交换次数"), Description("2-opt 局部搜索的最大交换次数")>
        Public Property MaxSwaps As Integer = 1000

        Public Overrides Function ToString() As String
            Return "Circular"
        End Function
    End Class

End Namespace
