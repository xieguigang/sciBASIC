Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Isometric
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D.Isometric.Shapes


    ''' <summary>
    ''' Created by fabianterhorst on 02.04.17.
    ''' </summary>
    Public Class Stairs : Inherits Shape3D

        Public Sub New(origin As Point3D, stepCount As Double)
            Dim paths As Path3D() = New Path3D(CInt(Fix(stepCount)) * 2 + 2 - 1) {}
            Dim zigzag As New Path3D
            Dim points As Point3D() = New Point3D(CInt(Fix(stepCount)) * 2 + 2 - 1) {}
            Dim count As Integer = 1

            points(0) = origin

            For i As Integer = 0% To stepCount - 1
                Dim stepCorner As Point3D = origin.Translate(0, i / stepCount, (i + 1) / stepCount)

                paths(count - 1) = {
                    stepCorner,
                    stepCorner.Translate(0, 0, -1 / stepCount),
                    stepCorner.Translate(1, 0, -1 / stepCount),
                    stepCorner.Translate(1, 0, 0)
                }
                points(count) = stepCorner
                count += 1
                paths(count - 1) = {
                    stepCorner,
                    stepCorner.Translate(1, 0, 0),
                    stepCorner.Translate(1, 1 / stepCount, 0),
                    stepCorner.Translate(0, 1 / stepCount, 0)
                }
                points(count) = stepCorner.Translate(0, 1 / stepCount, 0)
                count += 1
            Next

            points(count) = origin.Translate(0, 1, 0)
            zigzag.Points = points.AsList
            paths(count - 1) = zigzag
            count += 1
            paths(count - 1) = zigzag.Reverse().TranslatePoints(1, 0, 0)
            Me.paths = paths.AsList
        End Sub
    End Class
End Namespace