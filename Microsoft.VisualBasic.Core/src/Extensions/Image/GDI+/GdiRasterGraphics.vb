#If NET48 Then
Imports System.Drawing
#Else
Imports Microsoft.VisualBasic.Imaging
#End If

Namespace Imaging.Driver

    Public Interface GdiRasterGraphics

        ReadOnly Property ImageResource As Image

    End Interface
End Namespace