Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Drawing3D

    ''' <summary>
    ''' ``PAINTERS ALGORITHM`` provider
    ''' </summary>
    Public Module Painter

        ''' <summary>
        ''' 请注意，这个并没有rotate，只会利用camera进行project
        ''' </summary>
        ''' <param name="canvas"></param>
        ''' <param name="camera"></param>
        ''' <param name="surfaces"></param>
        <Extension>
        Public Sub SurfacePainter(ByRef canvas As Graphics, camera As Camera, surfaces As IEnumerable(Of Surface))
            Dim sv As New List(Of Surface), order As New List(Of Integer)

            For Each s As Surface In surfaces
                Dim v As Point3D() = camera _
                    .Project(s.vertices) _
                    .ToArray

                sv += New Surface With {
                    .vertices = v,
                    .brush = s.brush
                }
            Next

            ' Compute the average Z value of each face.
            Dim avgZ#() = New Double(sv.Count) {}

            For i As Integer = 0 To sv.Count - 1
                avgZ(i) = sv(i).vertices.Average(Function(z) z.Z)
                order.Add(i)
            Next

            Dim iMax%
            Dim tmp#

            ' Next we sort the faces in descending order based on the Z value.
            ' The objective is to draw distant faces first. This is called
            ' the PAINTERS ALGORITHM. So, the visible faces will hide the invisible ones.
            ' The sorting algorithm used is the SELECTION SORT.
            For i% = 0 To sv.Count - 1
                iMax = i

                For j = i + 1 To sv.Count - 1
                    If avgZ(j) > avgZ(iMax) Then
                        iMax = j
                    End If
                Next

                If iMax <> i Then
                    tmp = avgZ(i)
                    avgZ(i) = avgZ(iMax)
                    avgZ(iMax) = tmp

                    tmp = order(i)
                    order(i) = order(iMax)
                    order(iMax) = tmp
                End If
            Next

            ' Draw the faces using the PAINTERS ALGORITHM (distant faces first, closer faces last).
            For i As Integer = 0 To sv.Count - 1
                Dim index As Integer = order(i)
                Dim s As Surface = sv(index)
                Dim points() As Point = s _
                    .vertices _
                    .Select(Function(p3D) p3D.PointXY(camera.screen)) _
                    .ToArray

                Call canvas.FillPolygon(s.brush, points)
            Next
        End Sub
    End Module
End Namespace