Imports System.Drawing
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace Heatmap

    Public Class PlotArguments

        Public left!

        ''' <summary>
        ''' 绘制矩阵之中的方格在xy上面的步进值
        ''' </summary>
        Public ReadOnly Property dStep As SizeF
            Get
                Return New SizeF With {
                    .Width = matrixPlotRegion.Width / ColOrders.Length,
                    .Height = matrixPlotRegion.Height / RowOrders.Length
                }
            End Get
        End Property

        ''' <summary>
        ''' 矩阵区域的大小和位置
        ''' </summary>
        Public matrixPlotRegion As Rectangle
        Public levels As Dictionary(Of String, DataSet)
        Public top!
        Public colors As SolidBrush()
        Public RowOrders$()
        Public ColOrders$()

    End Class

    ''' <summary>
    ''' Draw a specific heatmap element
    ''' </summary>
    Public Enum DrawElements As Byte
        ''' <summary>
        ''' Draw nothing
        ''' </summary>
        None = 0
        ''' <summary>
        ''' Only draw the heatmap element on matrix row
        ''' </summary>
        Rows = 2
        ''' <summary>
        ''' Only draw the heatmap element on the column
        ''' </summary>
        Cols = 4
        ''' <summary>
        ''' Draw both row and column heatmap elements
        ''' </summary>
        Both = 8
    End Enum

    Friend Delegate Sub HowtoDoPlot(g As IGraphics, region As GraphicsRegion, args As PlotArguments)

End Namespace