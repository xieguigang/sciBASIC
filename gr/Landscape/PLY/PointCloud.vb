Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Ply

    Public Class PointCloud : Implements PointF3D, IPointCloud

        Public Property x As Double Implements PointF3D.X
        Public Property y As Double Implements PointF3D.Y
        Public Property z As Double Implements PointF3D.Z
        Public Property color As String
        Public Property intensity As Double Implements IPointCloud.intensity

        Public Overrides Function ToString() As String
            Return $"[{x},{y},{z}] {intensity}"
        End Function

    End Class
End Namespace