Imports System.Drawing
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Namespace Drawing3D.Math3D

    Public Class Matrix : Implements IEnumerable(Of Surface)

        Public ReadOnly Property Matrix As New Vector3D With {
            .X = New Vector,
            .Y = New Vector,
            .Z = New Vector
        }
        ReadOnly surfaces As New List(Of SurfaceVector)

        Public Structure SurfaceVector
            Public brush As Brush
            Public vertices As Integer()
        End Structure

        Sub New(surface As IEnumerable(Of Surface))
            Dim i As int = 0

            For Each s As Surface In surface
                Dim v As New List(Of Integer)

                For Each p3D As Point3D In s.vertices
                    v += ++i
                    Matrix.Add(p3D)
                Next

                surfaces += New SurfaceVector With {
                    .brush = s.brush,
                    .vertices = v
                }
            Next
        End Sub

        ''' <summary>
        ''' 投影
        ''' </summary>
        ''' <param name="v">经过转换之后的向量，例如旋转或者位移，在这个函数值中会利用摄像机进行投影</param>
        ''' <returns></returns>
        Public Function TranslateBuffer(camera As Camera, v As Vector3D) As IEnumerable(Of Polygon)
            Dim surfaces As New List(Of Surface)

            For Each s As SurfaceVector In Me.surfaces
                surfaces += New Surface With {
                    .vertices = v(s.vertices),
                    .brush = s.brush
                }
            Next

            Return camera.PainterBuffer(surfaces)
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Surface) Implements IEnumerable(Of Surface).GetEnumerator
            For Each s As SurfaceVector In surfaces
                Yield New Surface With {
                    .vertices = Matrix(s.vertices),
                    .brush = s.brush
                }
            Next
        End Function
    End Class
End Namespace