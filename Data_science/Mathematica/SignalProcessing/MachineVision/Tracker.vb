#Region "Microsoft.VisualBasic::fe398e718de5169b1f41d664b93dc9b2, Data_science\Mathematica\SignalProcessing\MachineVision\Tracker.vb"

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

    '   Total Lines: 101
    '    Code Lines: 55 (54.46%)
    ' Comment Lines: 29 (28.71%)
    '    - Xml Docs: 41.38%
    ' 
    '   Blank Lines: 17 (16.83%)
    '     File Size: 3.87 KB


    ' Class Tracker
    ' 
    '     Function: GenericEnumerator
    ' 
    '     Sub: Update
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing

Public Class Tracker : Implements Enumeration(Of Trajectory)

    ''' <summary>
    ''' current tracking objects
    ''' </summary>
    Public ReadOnly currentTrajectories As New List(Of Trajectory)

    ''' <summary>
    ''' the global object id
    ''' </summary>
    Dim nextID As Integer = 0
    Dim lastUpdated As New Dictionary(Of Integer, Integer)

    ''' <summary>
    ''' 轨迹最大不活跃帧数
    ''' </summary>
    Public Const MAX_INACTIVE_FRAMES As Integer = 3

    ''' <summary>
    ''' Contains all objects for make tracking
    ''' </summary>
    Dim objects As New List(Of Trajectory)

    Public Sub Update(Of T As Detection)(frameData As FrameData(Of T))
        Dim currentFrameID = frameData.FrameID

        ' 1. 移除过期轨迹
        Dim toRemove As New List(Of Trajectory)

        For Each traj In currentTrajectories
            If lastUpdated.ContainsKey(traj.TrajectoryID) Then
                If currentFrameID - lastUpdated(traj.TrajectoryID) > MAX_INACTIVE_FRAMES Then
                    toRemove.Add(traj)
                End If
            End If
        Next
        For Each traj In toRemove
            currentTrajectories.Remove(traj)
        Next

        ' 构建代价矩阵（欧氏距离）
        ' nrows = current trajectories
        ' ncols = current frame detections
        Dim costMatrix(currentTrajectories.Count - 1, frameData.Detections.Length - 1) As Double

        ' compares object position distance between the
        ' last frame and current frame data
        For i As Integer = 0 To currentTrajectories.Count - 1
            For j As Integer = 0 To frameData.Detections.Length - 1
                Dim lastPos = currentTrajectories(i).LastPosition
                Dim currPos = frameData(j).Position

                costMatrix(i, j) = lastPos.Distance(currPos)
            Next
        Next

        ' 应用匈牙利算法[7](@ref)
        ' size of the assignments vector is equals to the current trajectories
        Dim assignments As Integer() = HungarianAlgorithm.FindAssignments(costMatrix)

        ' 3. 更新现有轨迹
        ' loop through each current trajectories
        For i As Integer = 0 To assignments.Length - 1
            ' get current trajectory assignment detection object in current frame
            Dim j As Integer = assignments(i)

            If j >= 0 AndAlso i < currentTrajectories.Count AndAlso j < frameData.Detections.Length Then
                currentTrajectories(i).Update(frameData.Detections(j))
                lastUpdated(currentTrajectories(i).TrajectoryID) = currentFrameID
            Else
                ' 未分配到检测，更新最后活跃时间
                ' just do nothing
                'If i < currentTrajectories.Count Then
                '    lastUpdated(currentTrajectories(i).TrajectoryID) = currentFrameID
                'End If
            End If
        Next

        ' 4. 处理新检测
        Dim assignedCols As New HashSet(Of Integer)(assignments)

        For j As Integer = 0 To frameData.Detections.Length - 1
            If Not assignedCols.Contains(j) Then
                currentTrajectories.Add(New Trajectory(nextID, frameData.Detections(j)))
                lastUpdated.Add(nextID, currentFrameID)
                objects.Add(currentTrajectories.Last)
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
