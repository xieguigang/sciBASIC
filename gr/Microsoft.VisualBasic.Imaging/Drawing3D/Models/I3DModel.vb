#Region "Microsoft.VisualBasic::af5d3871d19674398339c3dc10c7167e, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\I3DModel.vb"

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

    '     Interface I3DModel
    ' 
    '         Function: Copy
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Drawing3D.Models

    Public Interface I3DModel : Inherits IEnumerable(Of Point3D)

        Function Copy(data As IEnumerable(Of Point3D)) As I3DModel
        Sub Draw(ByRef canvas As Graphics, camera As Camera)
    End Interface
End Namespace
