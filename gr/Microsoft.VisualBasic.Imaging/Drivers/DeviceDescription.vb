Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Driver

    Public Class DeviceDescription

        Public Property size As Size
        Public Property padding As Padding
        Public Property driverUsed As Drivers
        Public Property dpi As Size

        Public ReadOnly Property background As Color
        Public ReadOnly Property bgHtmlColor As String

        Sub New(bg As String)
            bgHtmlColor = bg
            background = bg.TranslateColor
        End Sub

        Public Overrides Function ToString() As String
            Return $"{bgHtmlColor} [{size.Width},{size.Height}]"
        End Function

        Public Function GetRegion() As GraphicsRegion
            Return New GraphicsRegion With {
                .Size = size,
                .Padding = padding
            }
        End Function

    End Class
End Namespace