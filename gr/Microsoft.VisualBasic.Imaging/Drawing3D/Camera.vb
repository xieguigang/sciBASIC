Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing3D

    Public Class Camera

        Public ViewDistance!, angle!
        Public fov! = 256.0!
        Public screen As Size

        Public Function Rotate(pt As Point3D) As Point3D
            Return pt.RotateX(angle).RotateY(angle).RotateZ(angle)
        End Function

        Public Function Project(pt As Point3D) As Point3D
            Return pt.Project(screen.Width, screen.Height, fov, ViewDistance)
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace