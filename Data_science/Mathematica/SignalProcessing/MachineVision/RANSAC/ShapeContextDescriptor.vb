Imports System.Drawing
Imports std = System.Math

''' <summary>
''' 一个点的 Shape Context 描述子
''' </summary>
Public Class ShapeContextDescriptor
    Public Property Point As PointF
    ''' <summary>
    ''' 对数极坐标直方图，例如，5个径向bin * 12个角度bin = 60维向量
    ''' </summary>
    Public Property Histogram As Double()

    Public Sub New(point As PointF, radialBins As Integer, angularBins As Integer)
        Me.Point = point
        Me.Histogram = New Double(radialBins * angularBins - 1) {}
    End Sub
End Class

Public NotInheritable Class ShapeContextCalculator

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 为一组点计算 Shape Context 描述子
    ''' </summary>
    ''' <param name="points">输入点集</param>
    ''' <param name="radialBins">径向bin数量</param>
    ''' <param name="angularBins">角度bin数量</param>
    ''' <returns>描述子列表</returns>
    Public Shared Function ComputeDescriptors(points As List(Of PointF),
                                            Optional radialBins As Integer = 5,
                                            Optional angularBins As Integer = 12) As List(Of ShapeContextDescriptor)
        If points Is Nothing OrElse points.Count < 2 Then Return New List(Of ShapeContextDescriptor)()

        Dim descriptors As New List(Of ShapeContextDescriptor)
        Dim n = points.Count

        ' 1. 预计算所有点对之间的距离和角度
        Dim logDistances(n - 1)() As Double
        Dim angles(n - 1)() As Double
        For i As Integer = 0 To n - 1
            logDistances(i) = New Double(n - 1) {}
            angles(i) = New Double(n - 1) {}
            For j As Integer = 0 To n - 1
                If i = j Then Continue For
                Dim dx = points(j).X - points(i).X
                Dim dy = points(j).Y - points(i).Y
                Dim dist = std.Sqrt(dx * dx + dy * dy)
                ' 使用对数距离，并加1避免log(0)
                logDistances(i)(j) = std.Log(dist + 1)
                angles(i)(j) = std.Atan2(dy, dx)
            Next
        Next

        ' 2. 确定直方图的边界
        Dim minLogR As Double = Double.PositiveInfinity
        Dim maxLogR As Double = Double.NegativeInfinity
        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                If i <> j Then
                    If logDistances(i)(j) < minLogR Then minLogR = logDistances(i)(j)
                    If logDistances(i)(j) > maxLogR Then maxLogR = logDistances(i)(j)
                End If
            Next
        Next
        Dim logRStep = If(maxLogR > minLogR, (maxLogR - minLogR) / radialBins, 1.0)
        Dim angleStep = 2 * std.PI / angularBins

        ' 3. 为每个点计算直方图
        For i As Integer = 0 To n - 1
            Dim descriptor = New ShapeContextDescriptor(points(i), radialBins, angularBins)
            For j As Integer = 0 To n - 1
                If i = j Then Continue For

                Dim r = logDistances(i)(j)
                Dim theta = angles(i)(j)

                ' 找到对应的 bin
                Dim rBin = CInt(std.Floor((r - minLogR) / logRStep))
                Dim thetaBin = CInt(std.Floor((theta + std.PI) / angleStep))

                ' 边界检查
                rBin = std.Max(0, std.Min(radialBins - 1, rBin))
                thetaBin = std.Max(0, std.Min(angularBins - 1, thetaBin))

                ' 累加到直方图
                descriptor.Histogram(rBin * angularBins + thetaBin) += 1
            Next

            ' 4. 归一化直方图（使其总和为1）
            Dim sum = descriptor.Histogram.Sum()
            If sum > 0 Then
                For k As Integer = 0 To descriptor.Histogram.Length - 1
                    descriptor.Histogram(k) /= sum
                Next
            End If

            descriptors.Add(descriptor)
        Next

        Return descriptors
    End Function
End Class

