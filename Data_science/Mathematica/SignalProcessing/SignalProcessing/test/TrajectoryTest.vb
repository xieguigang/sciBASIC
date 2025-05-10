#Region "Microsoft.VisualBasic::1c38bfbe3276e05496fb1356ad66d3f5, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\TrajectoryTest.vb"

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

    '   Total Lines: 136
    '    Code Lines: 96 (70.59%)
    ' Comment Lines: 15 (11.03%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 25 (18.38%)
    '     File Size: 5.03 KB


    ' Class MotionSimulator
    ' 
    '     Function: GenerateTrackData
    ' 
    ' Module TrajectoryTest
    ' 
    '     Sub: Main, matchesTest, VisualizeTracks
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.MachineVision
Imports Microsoft.VisualBasic.Math.SignalProcessing.HungarianAlgorithm
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class MotionSimulator
    ' 生成ABC三个物体的模拟运动数据
    Friend Shared Iterator Function GenerateTrackData(frameCount As Integer) As IEnumerable(Of FrameData)
        Dim rnd As New Random()

        ' 初始位置设置（单位：像素）
        Dim positions = New Dictionary(Of String, PointF) From {
            {"A", New PointF(rnd.Next(50, 100), rnd.Next(50, 100))},
            {"B", New PointF(rnd.Next(200, 300), rnd.Next(150, 200))},
            {"C", New PointF(rnd.Next(400, 500), rnd.Next(250, 300))},
            {"D", New PointF(800, 850)}
        }
        Dim d_exit As Integer = frameCount / 2
        Dim newE As New PointF(rnd.Next(50, 500), rnd.Next(250, 500))

        For frame = 0 To frameCount - 1
            Dim frameData As New FrameData With {.FrameID = frame}

            ' 物体A：匀速直线运动
            positions("A") = New PointF(
                positions("A").X + 8,
                positions("A").Y + 4)

            ' 物体B：正弦曲线运动
            positions("B") = New PointF(
                positions("B").X + 5,
                positions("B").Y + 10 * CSng(Math.Sin(frame * 0.2)))

            ' 物体C：加速运动
            positions("C") = New PointF(
                positions("C").X + frame * 0.3F,
                positions("C").Y + frame * 0.2F)

            If d_exit > frame Then
                ' 物体D：余弦曲线运动
                positions("D") = New PointF(
                    positions("D").X + 5,
                    positions("D").Y + 10 * CSng(Math.Cos(frame * 0.2)))
            ElseIf positions.ContainsKey("D") Then
                positions.Remove("D")
            Else
                If Not positions.ContainsKey("E") Then
                    positions.Add("E", newE)
                Else
                    positions("E") = New PointF(
               positions("E").X - 18,
               positions("E").Y - 3)
                End If
            End If

            ' 添加高斯噪声（标准差=3）
            For Each kvp In positions
                frameData.Detections.Add(New Detection With {
                    .ObjectID = kvp.Key,
                    .Position = New PointF(
                        kvp.Value.X + CSng(rnd.NextDouble() * 6 - 3),
                        kvp.Value.Y + CSng(rnd.NextDouble() * 6 - 3))
                })
            Next

            frameData.Detections = frameData.Detections.Shuffles

            Yield frameData
        Next
    End Function
End Class

Module TrajectoryTest

    Sub matchesTest()
        ' 测试方阵
        Dim costSquare As Double(,) = {{1.0, 2.0}, {2.0, 1.0}}
        Console.WriteLine(HungarianAlgorithm.FindAssignments(costSquare).GetJson)

        ' 测试宽矩阵
        Dim costWide As Double(,) = {{1.0, 3.0, 5.0}, {2.0, 4.0, 6.0}}
        Console.WriteLine(HungarianAlgorithm.FindAssignments(costWide).GetJson)

        ' 测试高矩阵
        Dim costTall As Double(,) = {{1.0, 4.0}, {9.0, 15.0}, {3.0, 6.0}}
        Console.WriteLine(HungarianAlgorithm.FindAssignments(costTall).GetJson)
    End Sub

    Sub Main()
        Call matchesTest()

        ' 生成模拟数据（50帧）
        Dim simulatedData = MotionSimulator.GenerateTrackData(50).ToArray

        ' 初始化跟踪器
        Dim tracker = New Tracker()

        ' 逐帧处理
        For Each frame In simulatedData
            tracker.Update(frame)
        Next

        ' 输出轨迹信息
        Console.WriteLine("=== 轨迹关联结果 ===")
        For Each traj In tracker.AsEnumerable
            Console.WriteLine($"轨迹ID:{traj.TrajectoryID} 点数:{traj.positions.Count}")
        Next

        ' 可视化输出（需引用System.Drawing）
        VisualizeTracks(tracker.AsEnumerable)

        Pause()
    End Sub

    Private Sub VisualizeTracks(trajectories As IEnumerable(Of Trajectory))
        Using bmp As New Bitmap(1000, 900)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.White)
                Dim colors As New Dictionary(Of Integer, Color) From {
                {0, Color.Red}, {1, Color.Blue}, {2, Color.Green}, {3, Color.Purple}, {4, Color.Black}
            }

                For Each traj In trajectories
                    If traj.positions.Count > 1 Then
                        Dim pen As New Pen(colors(traj.TrajectoryID Mod 5), 2)
                        g.DrawLines(pen, traj.positions.Select(Function(p) New Point(CInt(p.X), CInt(p.Y))).ToArray())
                    End If
                Next
            End Using
            bmp.Save("tracks_visualization.png")
        End Using
    End Sub


End Module
