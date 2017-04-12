Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D.IsoMetric.Shapes


    ''' <summary>
    ''' Created by fabianterhorst on 02.04.17.
    ''' </summary>

    Public Class Stairs : Inherits Shape3D

        Public Sub New(ByVal origin As Point3D, ByVal stepCount As Double)
            Dim ___paths As Path3D() = New Path3D(CInt(Fix(stepCount)) * 2 + 2 - 1) {}
            Dim zigzag As New Path3D
            Dim points As Point3D() = New Point3D(CInt(Fix(stepCount)) * 2 + 2 - 1) {}
            points(0) = origin
            Dim i As Integer, count As Integer = 1
            For i = 0 To stepCount - 1
                Dim stepCorner As Point3D = Math3D.ORIGIN.Translate(0, i / stepCount, (i + 1) / stepCount)
                ___paths(count - 1) = New Path3D({stepCorner, stepCorner.Translate(0, 0, -1 / stepCount), stepCorner.Translate(1, 0, -1 / stepCount), stepCorner.Translate(1, 0, 0)})
                points(count) = stepCorner
                count += 1
                ___paths(count - 1) = New Path3D({stepCorner, stepCorner.Translate(1, 0, 0), stepCorner.Translate(1, 1 / stepCount, 0), stepCorner.Translate(0, 1 / stepCount, 0)})
                points(count) = stepCorner.Translate(0, 1 / stepCount, 0)
                count += 1
            Next i
            points(count) = origin.Translate(0, 1, 0)
            zigzag.Points = points.AsList
            ___paths(count - 1) = zigzag
            count += 1
            ___paths(count - 1) = zigzag.Reverse().TranslatePoints(1, 0, 0)
            Me.paths = ___paths.AsList
        End Sub
    End Class

End Namespace