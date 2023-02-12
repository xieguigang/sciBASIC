Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Wavefront

    Public Class OBJ

        ''' <summary>
        ''' lib file name of mtl data
        ''' </summary>
        ''' <returns></returns>
        Public Property mtllib As String
        Public Property parts As ObjectPart()
        Public Property comment As String

    End Class

    Public Class ObjectPart

        Public Property vertex As Point3D()

    End Class
End Namespace