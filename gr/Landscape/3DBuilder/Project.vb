Imports System.Drawing

Public Class Project

    ''' <summary>
    ''' ``/Metadata/thumbnail.png``
    ''' </summary>
    ''' <returns></returns>
    Public Property Thumbnail As Image
    Public Property model As XmlModel3D

    Public Shared Function FromZipDirectory(dir$) As Project
        Return New Project With {
            .Thumbnail = $"{dir}/Metadata/thumbnail.png".LoadImage,
            .model = IO.Load3DModel(dir & "/3D/3dmodel.model")
        }
    End Function

    Public Function GetSurfaces() As Drawing3D.Surface()
        If model Is Nothing Then
            Return {}
        Else
            Return model.GetSurfaces.ToArray
        End If
    End Function
End Class
