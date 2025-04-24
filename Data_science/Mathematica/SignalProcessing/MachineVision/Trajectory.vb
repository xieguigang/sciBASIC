Imports System.Drawing

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
