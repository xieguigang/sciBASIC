#Region "Microsoft.VisualBasic::e924ad3b5077799ccb8c858fd143ccf4, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Paths\Rectangle.vb"

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

    '     Class Rectangle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing3D.Models.Isometric.Paths

    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Rectangle : Inherits Path3D

        Public Sub New(origin As Point3D, width As Integer, height As Integer)
            MyBase.New()
            Push(origin)
            Push(New Point3D(origin.X + width, origin.Y, origin.Z))
            Push(New Point3D(origin.X + width, origin.Y + height, origin.Z))
            Push(New Point3D(origin.X, origin.Y + height, origin.Z))
        End Sub
    End Class
End Namespace
