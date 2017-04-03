Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Driver

    Public Module Extensions

        Public Function CreateDevice(size As Size, padding As Padding, driver As Drivers) As GraphicsData
            If driver = Drivers.GDI Then
            Else

            End If
        End Function

        Public Function CreateSVGGraphics(size As Size, padding As Padding) As GraphicsSVG

        End Function

        Public Function CreateGDIGraphics(size As Size, padding As Padding) As Graphics2D

        End Function
    End Module
End Namespace