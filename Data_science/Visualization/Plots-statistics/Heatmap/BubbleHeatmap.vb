Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Data.csv.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace Heatmap

    ''' <summary>
    ''' 普通的热图是整体进行比对，使用填充在方格内的颜色来区分数值
    ''' 而泡泡热图，则是进行单个行数据在样本间的比较，行之间可以使用不同的颜色区分，数值使用泡泡的半径大小来表示
    ''' </summary>
    Public Module BubbleHeatmap

        <Extension>
        Public Function Plot(data As IEnumerable(Of DataSet),
                             Optional size$ = "300,2700",
                             Optional bg$ = "white",
                             Optional margin$ = g.DefaultLargerPadding,
                             Optional colors$ = "",
                             Optional minRadius! = 1) As GraphicsData

        End Function
    End Module
End Namespace