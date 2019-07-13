#Region "Microsoft.VisualBasic::af18493fe141e76e55cdd4ad74edb5e8, gr\Landscape\Graphics.vb"

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

    '     Class Graphics
    ' 
    '         Properties: bg, Surfaces
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Data

    ''' <summary>
    ''' The data model of the landscape 3D model.
    ''' </summary>
    Public Class Graphics

        Public Property Surfaces As Surface()
        ''' <summary>
        ''' The scene paint background, its value definition is the same as <see cref="Surface.paint"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property bg As String

    End Class
End Namespace
