Namespace Drawing3D.Models.Isometric.Shapes

    Public Class Line : Inherits Shape3D

        Public Sub New(a As Point3D, b As Point3D)
            Call MyBase.New
            Call Push(line3D(a, b))
        End Sub

        Private Shared Function line3D(a As Point3D, b As Point3D) As Path3D
            Return New Path3D().Push(a).Push(b)
        End Function
    End Class
End Namespace