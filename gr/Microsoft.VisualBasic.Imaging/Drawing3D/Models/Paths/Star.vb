Namespace Drawing3D.Models.Isometric.Paths


    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Star : Inherits Path3D

        Public Sub New(origin As Point3D, outerRadius#, innerRadius#, points%)
            MyBase.New()

            For i As Integer = 0 To points * 2 - 1
                Dim r As Double = If(i Mod 2 = 0, outerRadius, innerRadius)
                Dim p As New Point3D(
                    (r * Math.Cos(i * Math.PI / points)) + origin.X,
                    (r * Math.Sin(i * Math.PI / points)) + origin.Y,
                    origin.Z)

                Call Push(p)
            Next
        End Sub
    End Class
End Namespace