Imports System.Drawing

#If NET48 Then
#Else
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
#End If

Namespace Imaging.Driver

    Public MustInherit Class DeviceInterop

        Public MustOverride Function CreateGraphic(size As Size, fill As Color, dpi As Integer) As IGraphics

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="background"></param>
        ''' <param name="direct_access">
        ''' create the graphics canvas directly based on the input background image resource if set this parameter to true, 
        ''' or make a copy of the image and then create the graphics canvas if set this parameter false.
        ''' </param>
        ''' <returns></returns>
        Public MustOverride Function CreateCanvas2D(background As Bitmap, direct_access As Boolean) As IGraphics

        Public MustOverride Function GetData(g As IGraphics) As IGraphicsData

    End Class
End Namespace