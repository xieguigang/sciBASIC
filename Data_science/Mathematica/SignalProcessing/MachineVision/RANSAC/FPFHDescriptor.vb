Imports System.Drawing
Imports std = System.Math

''' <summary>
''' 一个带法向量的点
''' </summary>
Public Structure PointWithNormal
    Public Pt As PointF
    ''' <summary>
    ''' 法向量，应为单位向量
    ''' </summary>
    Public Normal As PointF
End Structure

''' <summary>
''' FPFH (Fast Point Feature Histograms)
''' </summary>
Public Class FPFHDescriptor
    Public Property Point As PointWithNormal
    ''' <summary>
    ''' FPFH 直方图，例如，将每个角度特征分成11个bin，共33维
    ''' </summary>
    ''' <returns></returns>
    Public Property Histogram As Double()

    Public Sub New(point As PointWithNormal, featureBins As Integer)
        Me.Point = point
        Me.Histogram = New Double(featureBins * 3 - 1) {} ' 假设3个特征
    End Sub
End Class

Public NotInheritable Class FPFHCalculator

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 为一组点计算 FPFH 描述子
    ''' </summary>
    Public Shared Function ComputeDescriptors(points As List(Of PointWithNormal),
                                             Optional searchRadius As Double = 0.5,
                                             Optional featureBins As Integer = 11) As List(Of FPFHDescriptor)
        If points Is Nothing OrElse points.Count < 2 Then Return New List(Of FPFHDescriptor)()

        Dim spfhHistograms(points.Count - 1)() As Double
        Dim n = points.Count

        ' 1. 计算每个点的 SPFH
        For i As Integer = 0 To n - 1
            spfhHistograms(i) = ComputeSPFH(points(i), points, searchRadius, featureBins)
        Next

        ' 2. 组合 SPFH 以创建 FPFH
        Dim fpfhDescriptors As New List(Of FPFHDescriptor)
        For i As Integer = 0 To n - 1
            Dim fpfhHist = New Double(featureBins * 3 - 1) {}
            Dim neighbors = FindNeighbors(points(i), points, searchRadius)

            ' 加权平均
            Dim weightSum As Double = 1.0 ' 自身权重为1
            For j As Integer = 0 To fpfhHist.Length - 1
                fpfhHist(j) = spfhHistograms(i)(j) ' 初始化为自身的SPFH
            Next

            For Each neighbor In neighbors
                Dim dist = Distance(points(i).Pt, neighbor.Pt)
                Dim weight = 1.0 / dist ' 简单的距离权重
                weightSum += weight

                For j As Integer = 0 To fpfhHist.Length - 1
                    fpfhHist(j) += weight * spfhHistograms(points.IndexOf(neighbor))(j)
                Next
            Next

            ' 归一化
            If weightSum > 0 Then
                For j As Integer = 0 To fpfhHist.Length - 1
                    fpfhHist(j) /= weightSum
                Next
            End If

            fpfhDescriptors.Add(New FPFHDescriptor(points(i), featureBins) With {.Histogram = fpfhHist})
        Next

        Return fpfhDescriptors
    End Function

    ''' <summary>
    ''' 计算单个点的简化点特征直方图 (SPFH)
    ''' </summary>
    Private Shared Function ComputeSPFH(queryPoint As PointWithNormal, allPoints As List(Of PointWithNormal), radius As Double, bins As Integer) As Double()
        Dim hist = New Double(bins * 3 - 1) {} ' 3个特征: alpha, phi, theta
        Dim neighbors = FindNeighbors(queryPoint, allPoints, radius)

        For Each neighbor In neighbors
            ' 计算三个角度特征 (这里是2D简化版)
            Dim u = queryPoint.Normal
            Dim v = New PointF(neighbor.Pt.X - queryPoint.Pt.X, neighbor.Pt.Y - queryPoint.Pt.Y)
            Dim w = neighbor.Normal

            Dim alpha = AngleBetween(u, v)
            Dim phi = AngleBetween(u, w)
            ' theta 在2D中可以简化或省略

            ' 将角度量化并加入直方图
            AddToHistogram(hist, alpha, 0, bins, std.PI)
            AddToHistogram(hist, phi, bins, bins, std.PI)
            ' AddToHistogram(hist, theta, 2 * bins, bins, std.PI)
        Next

        ' 归一化
        Dim sum = hist.Sum()
        If sum > 0 Then
            For i As Integer = 0 To hist.Length - 1
                hist(i) /= sum
            Next
        End If
        Return hist
    End Function

    ' --- 辅助函数 ---
    Private Shared Function FindNeighbors(query As PointWithNormal, points As List(Of PointWithNormal), radius As Double) As List(Of PointWithNormal)
        Return points.Where(Function(p) p.Pt <> query.Pt AndAlso Distance(query.Pt, p.Pt) <= radius).ToList()
    End Function

    Private Shared Function Distance(p1 As PointF, p2 As PointF) As Double
        Dim dx = p1.X - p2.X
        Dim dy = p1.Y - p2.Y
        Return std.Sqrt(dx * dx + dy * dy)
    End Function

    Private Shared Function AngleBetween(v1 As PointF, v2 As PointF) As Double
        Dim dot = v1.X * v2.X + v1.Y * v2.Y
        Dim det = v1.X * v2.Y - v1.Y * v2.X
        Return std.Atan2(det, dot) ' 返回 [-PI, PI]
    End Function

    Private Shared Sub AddToHistogram(hist As Double(), value As Double, startIndex As Integer, binCount As Integer, range As Double)
        Dim bin = CInt(std.Floor((value + range / 2) * binCount / range))
        bin = std.Max(0, std.Min(binCount - 1, bin))
        hist(startIndex + bin) += 1
    End Sub
End Class