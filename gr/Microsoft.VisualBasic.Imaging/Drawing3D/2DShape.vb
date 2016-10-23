Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes

Namespace Drawing3D

    Public Class Shape2D : Implements I3DModel

        Public focus As Point3D
        Public shape As Shape

        Public Sub Draw(ByRef canvas As Graphics, camera As Camera) Implements I3DModel.Draw
            Dim pt As Point = camera.Project(focus).PointXY(camera.screen)
            shape.Location = pt
            Call shape.Draw(canvas)
        End Sub

        Public Function Copy(data As IEnumerable(Of Point3D)) As I3DModel Implements I3DModel.Copy
            Return New Shape2D With {.focus = data.First, .shape = shape}
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Point3D) Implements IEnumerable(Of Point3D).GetEnumerator
            Yield focus
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace