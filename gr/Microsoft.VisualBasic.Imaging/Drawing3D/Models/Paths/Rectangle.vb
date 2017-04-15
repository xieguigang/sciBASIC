Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Isometric

Namespace Drawing3D.Isometric.Paths

    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Rectangle
        Inherits Path3D

        Public Sub New(origin As Point3D, width As Integer, height As Integer)
            MyBase.New()
            Push(origin)
            Push(New Point3D(origin.X + width, origin.Y, origin.Z))
            Push(New Point3D(origin.X + width, origin.Y + height, origin.Z))
            Push(New Point3D(origin.X, origin.Y + height, origin.Z))
        End Sub
    End Class
End Namespace