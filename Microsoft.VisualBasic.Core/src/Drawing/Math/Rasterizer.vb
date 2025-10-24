Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' 栅格化结果容器：每个栅格存储该区域内点的列表
    ''' </summary>
    Public Class RasterData

        Public Property Grid As List(Of List(Of List(Of PointF)))
        Public Property Resolution As Double
        Public Property MinX As Double
        Public Property MinY As Double
        Public Property GridWidth As Integer
        Public Property GridHeight As Integer

        Default Public ReadOnly Property Cell(i As Integer, j As Integer) As List(Of PointF)
            Get
                Return Grid(i)(j)
            End Get
        End Property

        Public ReadOnly Property MeanDensity As Double
            Get
                Return Aggregate cell As List(Of PointF)
                       In Grid.IteratesALL
                       Into Average(cell.Count)
            End Get
        End Property

        Public Sub New(resolution As Double, minX As Double, minY As Double, gridWidth As Integer, gridHeight As Integer)
            Me.Resolution = resolution
            Me.MinX = minX
            Me.MinY = minY
            Me.GridWidth = gridWidth
            Me.GridHeight = gridHeight
            ' 初始化三维列表：网格行 × 网格列 → 点列表
            Grid = New List(Of List(Of List(Of PointF)))
            For i As Integer = 0 To gridHeight - 1
                Dim row As New List(Of List(Of PointF))
                For j As Integer = 0 To gridWidth - 1
                    row.Add(New List(Of PointF))
                Next
                Grid.Add(row)
            Next
        End Sub

        Public Function GetRasterPolygon(n As Integer) As Polygon2D
            Dim x As New List(Of Double)
            Dim y As New List(Of Double)

            For i As Integer = 0 To GridHeight - 1
                For j As Integer = 0 To GridWidth - 1
                    If Me(i, j).Count > n Then
                        Call x.Add(j + 1)
                        Call y.Add(i + 1)
                    End If
                Next
            Next

            Return New Polygon2D(x.ToArray, y.ToArray)
        End Function
    End Class

    Public Module Rasterizer

        ''' <summary>
        ''' 对Polygon2D点云进行栅格化
        ''' </summary>
        ''' <param name="pointCloud">输入点云数据</param>
        ''' <param name="resolution">栅格分辨率（每个栅格的边长）</param>
        ''' <returns>栅格化结果，包含每个栅格内的点集</returns>
        Public Function Rasterize(pointCloud As Polygon2D, resolution As Double) As RasterData
            ' 1. 计算点云边界范围
            Dim minX, maxX, minY, maxY As Double

            With New DoubleRange(pointCloud.xpoints)
                minX = .Min
                maxX = .Max
            End With
            With New DoubleRange(pointCloud.ypoints)
                minY = .Min
                maxY = .Max
            End With

            ' 2. 计算栅格网格尺寸
            Dim gridWidth As Integer = CInt(std.Ceiling((maxX - minX) / resolution))
            Dim gridHeight As Integer = CInt(std.Ceiling((maxY - minY) / resolution))

            ' 3. 初始化栅格结果容器
            Dim result As New RasterData(resolution, minX, minY, gridWidth, gridHeight)

            ' 4. 遍历每个点，分配到对应栅格（优化：单次循环直接映射）
            For i As Integer = 0 To pointCloud.xpoints.Length - 1
                Dim x As Double = pointCloud.xpoints(i)
                Dim y As Double = pointCloud.ypoints(i)

                ' 计算点所在的栅格索引
                Dim colIndex As Integer = CInt(std.Floor((x - minX) / resolution))
                Dim rowIndex As Integer = CInt(std.Floor((y - minY) / resolution))

                ' 检查索引是否在有效范围内
                If colIndex >= 0 AndAlso colIndex < gridWidth AndAlso rowIndex >= 0 AndAlso rowIndex < gridHeight Then
                    result.Grid(rowIndex)(colIndex).Add(New PointF(CSng(x), CSng(y)))
                End If
            Next

            Return result
        End Function

        ''' <summary>
        ''' 计算点云的平均最近邻距离，作为分辨率估算的基础
        ''' </summary>
        ''' <param name="pointCloud">输入的点云数据</param>
        ''' <returns>估算出的平均点间距（分辨率）</returns>
        Public Function EstimateResolution(pointCloud As Polygon2D, size As Integer) As Double
            Dim x As New DoubleRange(pointCloud.xpoints)
            Dim y As New DoubleRange(pointCloud.ypoints)

            Return ((x.Length / size) + (y.Length / size)) / 2
        End Function
    End Module
End Namespace