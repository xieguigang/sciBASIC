Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Html.CSS

''' <summary>
''' .NET clr gdi+ component convert to css model
''' </summary>
Public Module CssInterop

    <Extension>
    Public Function CreateCss(font As System.Drawing.Font) As CSSFont
        Return New CSSFont() With {
            .style = font.Style,
            .family = font.Name,
            .size = font.Size
        }
    End Function
End Module
