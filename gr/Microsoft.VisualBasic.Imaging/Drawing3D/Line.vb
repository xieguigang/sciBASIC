
Imports System.Drawing

Namespace Drawing3D

    Public Structure Line3D
        Implements IEnumerable(Of Point3D)
        Implements I3DModel

        Public a, b As Point3D
        Public pen As Pen

        Public Sub Draw(ByRef canvas As Graphics, camera As Camera) Implements I3DModel.Draw
            Dim pts As Point() = camera.Project(Me).Select(Function(pt) pt.PointXY(camera.screen)).ToArray
            Call canvas.DrawLine(pen, pts(0), pts(1))
        End Sub

        Public Function Copy(data As IEnumerable(Of Point3D)) As I3DModel Implements I3DModel.Copy
            Dim array = data.ToArray
            Return New Line3D With {
                .a = data(0),
                .b = data(1),
                .pen = pen
            }
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Point3D) Implements IEnumerable(Of Point3D).GetEnumerator
            Yield a
            Yield b
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Structure
End Namespace