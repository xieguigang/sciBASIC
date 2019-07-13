#Region "Microsoft.VisualBasic::342db05537f44878dd7cd0d8c3c904a3, gr\Landscape\3DBuilder\Project.vb"

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

    '     Class Project
    ' 
    '         Properties: model, Thumbnail
    ' 
    '         Function: FromZipDirectory, GetMatrix, GetSurfaces
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Landscape.Vendor_3mf.XML
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Vendor_3mf

    Public Class Project

        ''' <summary>
        ''' ``*.3mf/Metadata/thumbnail.png``
        ''' </summary>
        ''' <returns></returns>
        Public Property Thumbnail As Image
        ''' <summary>
        ''' ``*.3mf/3D/3dmodel.model``
        ''' </summary>
        ''' <returns></returns>
        Public Property model As XmlModel3D

        Public Shared Function FromZipDirectory(dir$) As Project
            Return New Project With {
                .Thumbnail = $"{dir}/Metadata/thumbnail.png".LoadImage,
                .model = IO.Load3DModel(dir & "/3D/3dmodel.model")
            }
        End Function

        ''' <summary>
        ''' Get all of the 3D surface model data in this 3mf project.
        ''' </summary>
        ''' <param name="centraOffset"></param>
        ''' <returns></returns>
        Public Function GetSurfaces(Optional centraOffset As Boolean = False) As Surface()
            If model Is Nothing Then
                Return {}
            Else
                Dim out As Surface() = model.GetSurfaces.ToArray

                If centraOffset Then
                    With out.Centra
                        out = .Offsets(out).ToArray
                    End With
                End If

                Return out
            End If
        End Function

        Public Function GetMatrix(Optional centraOffset As Boolean = False) As Matrix
            Return New Matrix(GetSurfaces(centraOffset))
        End Function
    End Class
End Namespace
