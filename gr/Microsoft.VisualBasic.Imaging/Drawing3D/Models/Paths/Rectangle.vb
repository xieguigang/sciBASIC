Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric

Namespace Drawing3D.IsoMetric.Paths


    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>

    Public Class Rectangle
        Inherits Path3D

        Public Sub New(ByVal origin As Point3D, ByVal width As Integer, ByVal height As Integer)
            MyBase.New()
            Push(origin)
            Push(New Point3D(origin.X + width, origin.Y, origin.Z))
            Push(New Point3D(origin.X + width, origin.Y + height, origin.Z))
            Push(New Point3D(origin.X, origin.Y + height, origin.Z))
        End Sub
    End Class

End Namespace