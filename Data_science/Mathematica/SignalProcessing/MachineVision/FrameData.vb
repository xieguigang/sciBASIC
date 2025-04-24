Imports System.Drawing

Public Class FrameData
    Public Property FrameID As Integer
    Public Detections As New List(Of Detection)

End Class

Public Class Detection
    Public Property ObjectID As String
    Public Property Position As PointF
End Class

