' ---------------------------------------------------------------------------
' 点云“平均点距”的稳健估计
'
' 为什么不用 bbox.Volume / n：
'   三维重建得到的点云是<b>薄壳</b>（表面采样），不是体填充。用体积除以点数
'   会把“曲面内部的空气”也摊到每个点上，得到的间距比真实值大 1~2 个数量级，
'   进而让体素降采样把 18 万个点直接压到几百个（本项目第一次冒烟测试就是这么翻车的）。
'
' 这里改成：抽样 → k 近邻平均距离 → 取中位数。中位数对“稀疏的离群点”免疫，
' 比均值稳健得多。
' ---------------------------------------------------------------------------

Imports CC = Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports CP = Microsoft.VisualBasic.MachineVision.Cloudy.Preprocessing
Imports std = System.Math

Namespace Pipeline

    ''' <summary>
    ''' 点云尺度估计工具。
    ''' </summary>
    Public NotInheritable Class PointSpacing

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 用 k 近邻平均距离的中位数估计点距。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="k">近邻数量（默认 8，足以覆盖曲面上的一圈邻域）</param>
        ''' <param name="maxSamples">抽样上限，控制 KD 树查询成本</param>
        Public Shared Function MedianNeighborSpacing(cloud As CC.PointCloudBuffer,
                                                     Optional k As Integer = 8,
                                                     Optional maxSamples As Integer = 20000) As Double
            If cloud Is Nothing OrElse cloud.Count < 2 Then Return 1.0

            Dim sample As CC.PointCloudBuffer = cloud

            If cloud.Count > maxSamples Then
                sample = CP.Sampling.UniformCount(cloud, maxSamples)
            End If

            Dim distances As Double() = CP.StatisticalOutlierRemoval.ComputeMeanDistances(sample, std.Min(k, std.Max(2, sample.Count - 1)))
            Dim values As New List(Of Double)(distances.Length)

            For i As Integer = 0 To distances.Length - 1
                Dim d As Double = distances(i)

                If Double.IsNaN(d) OrElse d <= 0.0 Then Continue For

                Call values.Add(d)
            Next

            If values.Count = 0 Then Return 1.0

            values.Sort()

            Return values(values.Count \ 2)
        End Function

        ''' <summary>
        ''' 把 COLMAP 稀疏点（扁平 [N,3] 数组）转成 Cloudy 点云，便于复用其 KD 树/统计工具。
        ''' </summary>
        Public Shared Function FromFlat(xyz As Double(), count As Integer) As CC.PointCloudBuffer
            Dim cloud As CC.PointCloudBuffer = CC.PointCloudBuffer.NewCloud(count)

            For i As Integer = 0 To count - 1
                cloud.X(i) = xyz(i * 3)
                cloud.Y(i) = xyz(i * 3 + 1)
                cloud.Z(i) = xyz(i * 3 + 2)
            Next

            Return cloud
        End Function

    End Class

End Namespace
