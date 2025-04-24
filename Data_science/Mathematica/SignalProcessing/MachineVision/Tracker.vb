
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports std = System.Math

Public Class Tracker : Implements Enumeration(Of Trajectory)

    Public trajectories As New List(Of Trajectory)
    Public nextID As Integer = 0

    Private lastUpdated As New Dictionary(Of Integer, Integer)
    Private Const MAX_INACTIVE_FRAMES As Integer = 3 ' 轨迹最大不活跃帧数

    Dim objects As New List(Of Trajectory)

    Public Sub Update(frameData As FrameData)
        Dim currentFrameID = frameData.FrameID

        ' 1. 移除过期轨迹
        Dim toRemove As New List(Of Trajectory)
        For Each traj In trajectories
            If lastUpdated.ContainsKey(traj.TrajectoryID) Then
                If currentFrameID - lastUpdated(traj.TrajectoryID) > MAX_INACTIVE_FRAMES Then
                    toRemove.Add(traj)
                End If
            End If
        Next
        For Each traj In toRemove
            trajectories.Remove(traj)
        Next

        ' 构建代价矩阵（欧氏距离）
        Dim costMatrix(trajectories.Count - 1, frameData.Detections.Count - 1) As Double
        For i = 0 To trajectories.Count - 1
            For j = 0 To frameData.Detections.Count - 1
                Dim lastPos = trajectories(i).LastPosition
                Dim currPos = frameData.Detections(j).Position
                costMatrix(i, j) = std.Sqrt((lastPos.X - currPos.X) ^ 2 + (lastPos.Y - currPos.Y) ^ 2)
            Next
        Next

        ' 应用匈牙利算法[7](@ref)
        Dim assignments As Integer() = HungarianAlgorithm.FindAssignments(costMatrix)

        ' 3. 更新现有轨迹
        For i = 0 To assignments.Length - 1
            Dim j = assignments(i)
            If j >= 0 AndAlso i < trajectories.Count AndAlso j < frameData.Detections.Count Then
                trajectories(i).Update(frameData.Detections(j))
                lastUpdated(trajectories(i).TrajectoryID) = currentFrameID
            Else
                ' 未分配到检测，更新最后活跃时间
                If i < trajectories.Count Then
                    lastUpdated(trajectories(i).TrajectoryID) = currentFrameID
                End If
            End If
        Next

        ' 4. 处理新检测
        Dim assignedCols As New HashSet(Of Integer)(assignments)
        For j = 0 To frameData.Detections.Count - 1
            If Not assignedCols.Contains(j) Then
                trajectories.Add(New Trajectory(nextID, frameData.Detections(j)))
                lastUpdated.Add(nextID, currentFrameID)
                objects.Add(trajectories.Last)
                nextID += 1
            End If
        Next
    End Sub

    Public Iterator Function GenericEnumerator() As IEnumerator(Of Trajectory) Implements Enumeration(Of Trajectory).GenericEnumerator
        For Each obj As Trajectory In objects
            Yield obj
        Next
    End Function
End Class