#Region "Microsoft.VisualBasic::55a1f58e73b7002671a21c1cf674097d, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\KalmanFilterDemotest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 71
    '    Code Lines: 50 (70.42%)
    ' Comment Lines: 8 (11.27%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (18.31%)
    '     File Size: 2.94 KB


    ' Module KalmanFilterDemotest
    ' 
    '     Function: GenerateNoisyMeasurements, GenerateTrueTrajectory
    ' 
    '     Sub: Main, VisualizeResults
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.SignalProcessing.KalmanFilter

Module KalmanFilterDemotest

    Sub Main()
        ' 初始化测试参数
        Dim timeSteps As Integer = 50
        Dim truePosition As Single()() = GenerateTrueTrajectory(timeSteps)
        Dim measurements As Single() = GenerateNoisyMeasurements(truePosition)

        ' 初始化卡尔曼滤波器
        Dim kf As New kalman2_state()
        Dim initX As Single() = {measurements(0), 0} ' [初始位置, 初始速度]
        Dim initP As Single()() = {
            New Single() {1000.0F, 0},    ' 初始位置协方差
            New Single() {0, 1000.0F}}    ' 初始速度协方差

        Algorithm.kalman2_init(kf, initX, initP)

        ' 运行卡尔曼滤波
        Dim estimates As New List(Of Single)
        For i As Integer = 0 To measurements.Length - 1
            Dim filteredValue As Single = Algorithm.kalman2_filter(kf, measurements(i))
            estimates.Add(filteredValue)
            Console.WriteLine($"Step {i}: Measured={measurements(i):0.00}, Filtered={filteredValue:0.00}")
        Next

        ' 可视化结果 (需引用System.Windows.Forms.DataVisualization)
        VisualizeResults(truePosition, measurements, estimates.ToArray())

        Pause()
    End Sub

    ' 生成真实运动轨迹（匀速直线运动）
    Private Function GenerateTrueTrajectory(steps As Integer) As Single()()
        Dim trajectory As New List(Of Single())
        Dim position As Single = 0
        Dim velocity As Single = 2.0F ' 恒定速度

        For i As Integer = 0 To steps - 1
            position += velocity
            trajectory.Add({position, velocity})
        Next
        Return trajectory.ToArray()
    End Function

    ' 添加高斯噪声生成观测值
    Private Function GenerateNoisyMeasurements(trueData As Single()()) As Single()
        Dim rnd As New Random()
        Dim measurements As New List(Of Single)

        For Each point In trueData
            ' 添加标准差为5的高斯噪声
            Dim noise As Single = 5.0F * Math.Sqrt(-2.0 * Math.Log(rnd.NextDouble())) *
                                  Math.Sin(2.0 * Math.PI * rnd.NextDouble())
            measurements.Add(point(0) + CSng(noise))
        Next
        Return measurements.ToArray()
    End Function

    ' 简单文本可视化（实际项目建议使用图表控件）
    Private Sub VisualizeResults(truePos As Single()(), measurements As Single(), estimates As Single())
        Console.WriteLine(vbCrLf & "=== 结果对比 ===")
        Console.WriteLine("Step | True | Measured | Filtered")
        Console.WriteLine("-----|------|----------|---------")

        For i As Integer = 0 To truePos.Length - 1
            Console.WriteLine($"{i,4} | {truePos(i)(0),4:0} | {measurements(i),7:0.00} | {estimates(i),7:0.00}")
        Next
    End Sub
End Module

