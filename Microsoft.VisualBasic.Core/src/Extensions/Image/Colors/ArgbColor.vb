
Imports System.IO

Namespace Imaging

    Public Structure ArgbColor

        Dim A As Byte
        Dim R As Byte
        Dim G As Byte
        Dim B As Byte

        Public Shared ReadOnly Property White As New ArgbColor(255, 255, 255)
        Public Shared ReadOnly Property Transparent As New ArgbColor(0, 255, 255, 255)
        Public Shared ReadOnly Property Red As New ArgbColor(255, 255, 0, 0)
        Public Shared ReadOnly Property Green As New ArgbColor(255, 0, 255, 0)
        Public Shared ReadOnly Property Blue As New ArgbColor(255, 0, 0, 255)

        Sub New(r As Byte, g As Byte, b As Byte)
            Call Me.New(255, r, g, b)
        End Sub

        Sub New(a As Byte, r As Byte, g As Byte, b As Byte)
            Me.A = a
            Me.R = r
            Me.G = g
            Me.B = b
        End Sub

        Public Overrides Function ToString() As String
            Return $"#{A.ToString("x")}{R.ToString("x")}{G.ToString("x")}{B.ToString("x")}"
        End Function

        Public Shared Function TranslateColor(color As String) As ArgbColor
            If color.IsPattern("#?[0-9a-f]{6,}") Then
                ' is html color 
                Return ParseHtmlColor(color)
            Else
                Throw New NotImplementedException(color)
            End If
        End Function

        Public Shared Function ParseHtmlColor(htmlColor As String) As ArgbColor
            If htmlColor.StartsWith("#"c) Then
                htmlColor = htmlColor.Substring(1)
            End If

            Dim alpha, red, green, blue As Integer

            Select Case htmlColor.Length
                Case 3 '#rgb
                    red = Convert.ToInt32(htmlColor.Substring(0, 1), 16) * 17
                    green = Convert.ToInt32(htmlColor.Substring(1, 1), 16) * 17
                    blue = Convert.ToInt32(htmlColor.Substring(2, 1), 16) * 17
                    alpha = 255
                Case 4 '#rgba
                    alpha = Convert.ToInt32(htmlColor.Substring(0, 1), 16) * 17
                    red = Convert.ToInt32(htmlColor.Substring(1, 1), 16) * 17
                    green = Convert.ToInt32(htmlColor.Substring(2, 1), 16) * 17
                    blue = Convert.ToInt32(htmlColor.Substring(3, 1), 16) * 17
                Case 6 '#rrggbb
                    red = Convert.ToInt32(htmlColor.Substring(0, 2), 16)
                    green = Convert.ToInt32(htmlColor.Substring(2, 2), 16)
                    blue = Convert.ToInt32(htmlColor.Substring(4, 2), 16)
                    alpha = 255
                Case 8 '#rrggbbaa
                    alpha = Convert.ToInt32(htmlColor.Substring(0, 2), 16)
                    red = Convert.ToInt32(htmlColor.Substring(2, 2), 16)
                    green = Convert.ToInt32(htmlColor.Substring(4, 2), 16)
                    blue = Convert.ToInt32(htmlColor.Substring(6, 2), 16)
                Case Else
                    Throw New InvalidDataException("#" & htmlColor)
            End Select

            Return New ArgbColor(alpha, red, green, blue)
        End Function

    End Structure
End Namespace