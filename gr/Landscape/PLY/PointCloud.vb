Namespace Ply

    Public Class PointCloud

        Public Property x As Double
        Public Property y As Double
        Public Property z As Double
        Public Property color As String
        Public Property intensity As Double

        Public Overrides Function ToString() As String
            Return $"[{x},{y},{z}] {intensity}"
        End Function

    End Class
End Namespace