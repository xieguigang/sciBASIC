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

        Public Function RotateX(pt As Point3D) As Point3D
            Return pt.RotateX(angle).RotateY(angle).RotateZ(angle)
        End Function

        Public Function RotateY(pt As Point3D) As Point3D
            Return pt.RotateY(angle)
        End Function

        Public Function RotateZ(pt As Point3D) As Point3D
            Return pt.RotateZ(angle)
        End Function

        Public Iterator Function Rotate(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt _
                    .RotateX(angle) _
                    .RotateY(angle) _
                    .RotateZ(angle)
            Next
        End Function

        Public Iterator Function Project(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.Project(screen.Width, screen.Height, fov, ViewDistance)
            Next
        End Function

        Public Iterator Function RotateX(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.RotateX(angle)
            Next
        End Function

        Public Iterator Function RotateY(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.RotateY(angle)
            Next
        End Function

        Public Iterator Function RotateZ(pts As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
            For Each pt As Point3D In pts
                Yield pt.RotateZ(angle)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace