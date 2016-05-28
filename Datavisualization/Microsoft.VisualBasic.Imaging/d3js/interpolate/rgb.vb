Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.d3.color

Namespace d3.interpolate

    Public Module rgb

        Public Function interpolateRgb(a As Drawing.Color, b As Drawing.Color) As Func(Of Byte, String)
            Dim ar = a.R,
                ag = a.G,
                ab = a.B,
                br = b.R - ar,
                bg = b.G - ag,
                bb = b.B - ab

            Return Function(t As Byte) _
                       $"#{d3_rgb_hex(Math.Round(ar + br * t))}{d3_rgb_hex(Math.Round(ag + bg * t))}{d3_rgb_hex(Math.Round(ab + bb * t))}"
        End Function
    End Module
End Namespace