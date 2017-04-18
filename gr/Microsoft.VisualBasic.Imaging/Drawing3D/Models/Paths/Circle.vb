Namespace Drawing3D.Models.Isometric.Paths

    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Circle : Inherits Path3D

        <Obsolete>
        Public Sub New(origin As Point3D, radius As Double)
            Me.New(origin, radius, 20)
        End Sub

        Public Sub New(origin As Point3D, radius As Double, vertices As Double)
            MyBase.New()

            For i As Integer = 0 To vertices - 1
                Dim p As New Point3D(
                    (radius * Math.Cos(i * 2 * Math.PI / vertices)) + origin.X,
                    (radius * Math.Sin(i * 2 * Math.PI / vertices)) + origin.Y,
                    origin.Z)

                Call Push(p)
            Next
        End Sub
    End Class
End Namespace