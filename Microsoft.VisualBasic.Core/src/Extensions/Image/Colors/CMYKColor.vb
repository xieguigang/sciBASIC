Imports System.Drawing

Namespace Imaging

    ''' <summary>
    ''' The CMYK color model (also known as process color, or four color) 
    ''' is a subtractive color model, based on the CMY color model, used 
    ''' in color printing, and is also used to describe the printing process
    ''' itself. The abbreviation CMYK refers to the four ink plates used: 
    ''' cyan, magenta, yellow, and key (black).
    ''' </summary>
    ''' <remarks>
    ''' All color channel in this color class should be in value range of ``[0,1]``.
    ''' </remarks>
    Public Class CMYKColor

        Public Property C As Single
        Public Property M As Single
        Public Property Y As Single
        Public Property K As Single

        ''' <summary>
        ''' CMYK
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"({C}, {M}, {Y}, {K})"
        End Function

        Public Shared Function FromRGB(rgb As Color) As CMYKColor
            Dim R = rgb.R / 255
            Dim G = rgb.G / 255
            Dim B = rgb.B / 255
            Dim K = 1 - {R, G, B}.Max

            Return New CMYKColor With {
                .K = K,
                .C = (1 - R - K) / (1 - K),
                .M = (1 - G - K) / (1 - K),
                .Y = (1 - B - K) / (1 - K)
            }
        End Function

        Public Function ToRGB() As Color
            Dim R = 255 * (1 - C) * (1 - K)
            Dim G = 255 * (1 - M) * (1 - K)
            Dim B = 255 * (1 - Y) * (1 - K)
            Return Color.FromArgb(255, R, G, B)
        End Function

    End Class
End Namespace