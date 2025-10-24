Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
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
        Public Function EstimateResolution(pointCloud As Polygon2D) As Double
            If pointCloud.xpoints Is Nothing OrElse pointCloud.xpoints.Length < 2 Then
                Throw New ArgumentException("点云至少需要包含2个点才能计算分辨率")
            End If

            Dim totalDistance As Double = 0.0
            Dim validPoints As Integer = 0

            ' 遍历点云中的每个点
            For i As Integer = 0 To pointCloud.xpoints.Length - 1
                Dim minDistance As Double = Double.MaxValue

                ' 寻找当前点(i)的最近邻点（排除自身）
                For j As Integer = 0 To pointCloud.xpoints.Length - 1
                    If i = j Then Continue For ' 跳过自身

                    ' 计算两点之间的欧氏距离
                    Dim dx As Double = pointCloud.xpoints(i) - pointCloud.xpoints(j)
                    Dim dy As Double = pointCloud.ypoints(i) - pointCloud.ypoints(j)
                    Dim distance As Double = std.Sqrt(dx * dx + dy * dy)

                    ' 更新最小距离
                    If distance < minDistance Then
                        minDistance = distance
                    End If
                Next j

                ' 累加有效点的最小距离
                If minDistance < Double.MaxValue Then
                    totalDistance += minDistance
                    validPoints += 1
                End If
            Next i

            If validPoints = 0 Then
                Return 0.0
            End If

            ' 返回平均最近邻距离作为基础分辨率
            Return totalDistance / validPoints
        End Function

        ''' <summary>
        ''' 提供推荐栅格大小的两种策略
        ''' </summary>
        ''' <param name="pointCloud">输入的点云数据</param>
        ''' <param name="strategy">策略选择：0-保守，1-精细</param>
        ''' <returns>推荐的最佳栅格大小</returns>
        Public Function GetRecommendedResolution(pointCloud As Polygon2D, Optional strategy As Integer = 0) As Double
            Dim baseResolution As Double = EstimateResolution(pointCloud)

            ' 策略选择
            Select Case strategy
                Case 1 ' 精细模式：略高于基础分辨率，确保每个栅格有适量点
                    Return baseResolution * 1.3
                Case Else ' 保守模式（默认）：等于或略低于基础分辨率，确保捕捉细节
                    Return baseResolution
            End Select
        End Function
    End Module
End Namespace