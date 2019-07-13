#Region "Microsoft.VisualBasic::0f61a2d513e29652188a886d4659906c, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shapes\Octahedron.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Octahedron
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports System.Math

Namespace Drawing3D.Models.Isometric.Shapes

    ''' <summary>
    ''' Created by fabianterhorst on 02.04.17.
    ''' </summary>
    Public Class Octahedron : Inherits Shape3D

        Public Sub New(origin As Point3D)
            MyBase.New()

            Dim center As Point3D = origin.Translate(0.5, 0.5, 0.5)
            Dim paths As Path3D() = New Path3D(7) {}
            Dim count As Integer = 0
            Dim upperTriangle As Path3D = {
                origin.Translate(0, 0, 0.5),
                origin.Translate(0.5, 0.5, 1),
                origin.Translate(0, 1, 0.5)
            }
            Dim lowerTriangle As Path3D = {
                origin.Translate(0, 0, 0.5),
                origin.Translate(0, 1, 0.5),
                origin.Translate(0.5, 0.5, 0)
            }

            For i As Integer = 0 To 3
                paths(count) = upperTriangle.RotateZ(center, i * Math.PI / 2.0)
                count += 1
                paths(count) = lowerTriangle.RotateZ(center, i * Math.PI / 2.0)
                count += 1
            Next

            MyBase.paths = paths.AsList
            ScalePath3Ds(center, Sqrt(2) / 2.0, Sqrt(2) / 2.0, 1)
        End Sub
    End Class
End Namespace
