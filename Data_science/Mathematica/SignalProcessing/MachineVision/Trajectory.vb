Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Trajectory of a object
''' </summary>
Public Class Trajectory

    Public ReadOnly Property TrajectoryID As Integer
    Public ReadOnly Property positions As PointF()
        Get
            Return objectSet _
                .SafeQuery _
                .Select(Function(o) CType(o, PointF)) _
                .ToArray
        End Get
    End Property

    Public ReadOnly Property LastPosition As PointF
        Get
            Return objectSet.Last()
        End Get
    End Property

    Public Property objectSet As New List(Of Detection)

    Public Sub New(id As Integer, t0 As Detection)
        TrajectoryID = id
        objectSet.Add(t0)
    End Sub

    ''' <summary>
    ''' Add last position to the current object trajectory
    ''' </summary>
    ''' <param name="detection"></param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Update(detection As Detection)
        Call objectSet.Add(detection)
    End Sub

End Class
