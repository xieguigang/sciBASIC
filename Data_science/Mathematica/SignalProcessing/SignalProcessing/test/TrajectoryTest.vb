Imports System.Drawing
Imports Microsoft.VisualBasic.Math.SignalProcessing.HungarianAlgorithm

Public Class MotionSimulator
    ' 生成ABC三个物体的模拟运动数据
    Friend Shared Iterator Function GenerateTrackData(frameCount As Integer) As IEnumerable(Of FrameData)
        Dim rnd As New Random()

        ' 初始位置设置（单位：像素）
        Dim positions = New Dictionary(Of String, PointF) From {
            {"A", New PointF(rnd.Next(50, 100), rnd.Next(50, 100))},
            {"B", New PointF(rnd.Next(200, 300), rnd.Next(150, 200))},
            {"C", New PointF(rnd.Next(400, 500), rnd.Next(250, 300))}
        }

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

            ' 添加高斯噪声（标准差=3）
            For Each kvp In positions
                frameData.Detections.Add(New Detection With {
                    .ObjectID = kvp.Key,
                    .Position = New PointF(
                        kvp.Value.X + CSng(rnd.NextDouble() * 6 - 3),
                        kvp.Value.Y + CSng(rnd.NextDouble() * 6 - 3))
                })
            Next

            Yield frameData
        Next
    End Function
End Class

Module TrajectoryTest

    Sub Main()
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
        For Each traj In tracker.trajectories
            Console.WriteLine($"轨迹ID:{traj.TrajectoryID} 点数:{traj.Positions.Count}")
        Next

        ' 可视化输出（需引用System.Drawing）
        VisualizeTracks(tracker.trajectories)
    End Sub

    Private Sub VisualizeTracks(trajectories As List(Of Trajectory))
        Using bmp As New Bitmap(800, 600)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.White)
                Dim colors As New Dictionary(Of Integer, Color) From {
                {0, Color.Red}, {1, Color.Blue}, {2, Color.Green}
            }

                For Each traj In trajectories
                    If traj.positions.Count > 1 Then
                        Dim pen As New Pen(colors(traj.TrajectoryID Mod 3), 2)
                        g.DrawLines(pen, traj.positions.Select(Function(p) New Point(CInt(p.X), CInt(p.Y))).ToArray())
                    End If
                Next
            End Using
            bmp.Save("tracks_visualization.png")
        End Using
    End Sub

    Public Class FrameData
        Public Property FrameID As Integer
        Public Detections As New List(Of Detection)
    End Class

    Public Class Detection
        Public Property ObjectID As String
        Public Property Position As PointF
    End Class

    Public Class Trajectory
        Public ReadOnly Property TrajectoryID As Integer
        Public positions As New List(Of PointF)

        Public Sub New(id As Integer, initialDetection As Detection)
            TrajectoryID = id
            positions.Add(initialDetection.Position)
        End Sub

        Public Sub Update(detection As Detection)
            positions.Add(detection.Position)
        End Sub

        Public ReadOnly Property LastPosition As PointF
            Get
                Return positions.Last()
            End Get
        End Property
    End Class

    Public Class Tracker
        Public trajectories As New List(Of Trajectory)
        Public nextID As Integer = 0

        Public Sub Update(frameData As FrameData)
            ' 构建代价矩阵（欧氏距离）
            Dim costMatrix(trajectories.Count - 1, frameData.Detections.Count - 1) As Double
            For i = 0 To trajectories.Count - 1
                For j = 0 To frameData.Detections.Count - 1
                    Dim lastPos = trajectories(i).LastPosition
                    Dim currPos = frameData.Detections(j).Position
                    costMatrix(i, j) = Math.Sqrt((lastPos.X - currPos.X) ^ 2 + (lastPos.Y - currPos.Y) ^ 2)
                Next
            Next

            ' 应用匈牙利算法[7](@ref)
            Dim assignments = HungarianAlgorithm.FindAssignments(costMatrix)

            ' 更新轨迹
            ' assignments数组索引表示代理ID，元素值表示分配的检测列索引
            For i = 0 To assignments.Length - 1
                Dim j = assignments(i) ' 当前代理i分配到的检测索引
                If i < trajectories.Count AndAlso j < frameData.Detections.Count Then
                    trajectories(i).Update(frameData.Detections(j))
                End If
            Next

            ' 处理未匹配检测（新目标）
            Dim assignedCols As New HashSet(Of Integer)(assignments) ' 获取所有已分配的列索引
            For j = 0 To frameData.Detections.Count - 1
                If Not assignedCols.Contains(j) Then ' 检查检测是否未被分配
                    trajectories.Add(New Trajectory(nextID, frameData.Detections(j)))
                    nextID += 1
                End If
            Next
        End Sub
    End Class
End Module
