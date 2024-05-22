#Region "Microsoft.VisualBasic::013564c353a88c8732e00c76039b437c, gr\Microsoft.VisualBasic.Imaging\Filters\Effects.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 202
    '    Code Lines: 140 (69.31%)
    ' Comment Lines: 27 (13.37%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 35 (17.33%)
    '     File Size: 7.70 KB


    '     Module Effects
    ' 
    '         Function: Diffusion, Emboss, Pencil, Sharp, Soften
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Filters

    Public Module Effects

        <Extension>
        Public Function Pencil(img As BitmapBuffer, Optional Sensitivity As Long = 25, Optional woodCarving As Boolean = False) As BitmapBuffer
            Dim M As Long
            Dim N As Long
            Dim Col As Long
            Dim ColNext As Long
            Dim OutPutWid = img.Width
            Dim OutPutHei = img.Height

            For I As Integer = 0 To OutPutWid - 2
                M = I + 1
                For L As Integer = 0 To OutPutHei - 2
                    N = L + 1
                    ' 当前点的灰度哦。
                    Col = img.GetPixel(0, I, L) * 3 + img.GetPixel(1, I, L) * 6 + img.GetPixel(2, I, L)
                    Col = Col / 10
                    ColNext = img.GetPixel(0, M, N) * 3 + img.GetPixel(1, M, N) * 6 + img.GetPixel(2, M, N)
                    ' 下一点的灰度哦。
                    ColNext = -ColNext / 10

                    ' 判断灰度变化是否超过设定的阀值
                    If Col + ColNext > Sensitivity Then
                        If Not woodCarving Then
                            ' RGB(0,0,0)表示黑色
                            img.SetPixel(I, L, 0, 0, 0)
                        Else
                            img.SetPixel(I, L, 255, 255, 255)
                        End If
                    Else
                        If Not woodCarving Then
                            ' RGB(255,255,255)表示白色
                            img.SetPixel(I, L, 255, 255, 255)
                        Else
                            img.SetPixel(I, L, 0, 0, 0)
                        End If
                    End If
                Next
            Next

            Return img
        End Function

        <Extension>
        Public Function Emboss(img As BitmapBuffer,
                               Optional directionX As Integer = 1,
                               Optional directionY As Integer = 1,
                               Optional Lighteness As Integer = 127) As BitmapBuffer

            Dim OutPutWid = img.Width
            Dim OutPutHei = img.Height

            For x As Integer = 1 To OutPutWid - 2
                For y As Integer = 1 To OutPutHei - 2
                    ' A  B  C  D
                    ' E  F  G  H
                    ' I  J  K  L
                    ' M  N  O  P

                    ' A = B - A + 127
                    Dim pB = img.GetPixel(x, y)
                    Dim pA = img.GetPixel(x + directionX, y + directionY)
                    Dim R = pB.R - pA.R + Lighteness
                    Dim G = pB.G - pA.G + Lighteness
                    Dim B = pB.B - pA.B + Lighteness

                    Call img.SetPixel(x, y, R, G, B)
                Next
            Next

            Return img
        End Function

        <Extension>
        Public Function Diffusion(img As BitmapBuffer) As BitmapBuffer
            Dim OutPutWid = img.Width
            Dim OutPutHei = img.Height

            Static point As Integer() = {-1, 0, 1}

            For x As Integer = 1 To OutPutWid - 2
                For y As Integer = 1 To OutPutHei - 2
                    ' A  B  C  D
                    ' E  F  G  H
                    ' I  J  K  L
                    ' M  N  O  P

                    ' F = random(A, B, C, E, F, G, I, J, K)
                    Call img.SetPixel(x, y, img.GetPixel(x + randf.Next(point), y + randf.Next(point)))
                Next
            Next

            Return img
        End Function

        <Extension>
        Public Function Soften(img As BitmapBuffer, Optional max As Double = 255) As BitmapBuffer
            Dim OutPutWid = img.Width
            Dim OutPutHei = img.Height

            If max > 255 Then
                max = 255
            End If

            For x As Integer = 1 To OutPutWid - 2
                For y As Integer = 1 To OutPutHei - 2
                    ' A  B  C  D
                    ' E  F  G  H
                    ' I  J  K  L
                    ' M  N  O  P

                    ' F = (A + B + C + E + F + G + I + J + K) / 9
                    Dim A = img.GetPixel(x - 1, y - 1)
                    Dim B = img.GetPixel(x, y - 1)
                    Dim C = img.GetPixel(x + 1, y - 1)
                    Dim E = img.GetPixel(x - 1, y)
                    Dim F = img.GetPixel(x, y)
                    Dim G = img.GetPixel(x + 1, y)
                    Dim I = img.GetPixel(x - 1, y + 1)
                    Dim J = img.GetPixel(x, y + 1)
                    Dim K = img.GetPixel(x + 1, y + 1)

                    Dim RR = New Integer() {A.R, B.R, C.R, E.R, F.R, G.R, I.R, J.R, K.R}.Average
                    Dim GG = New Integer() {A.G, B.G, C.G, E.G, F.G, G.G, I.G, J.G, K.G}.Average
                    Dim BB = New Integer() {A.B, B.B, C.B, E.B, F.B, G.B, I.B, J.B, K.B}.Average

                    If RR < 0 Then RR = 0
                    If RR > max Then RR = max
                    If GG < 0 Then GG = 0
                    If GG > max Then GG = max
                    If BB < 0 Then BB = 0
                    If BB > max Then BB = max

                    Call img.SetPixel(0, x, y, RR)
                    Call img.SetPixel(1, x, y, GG)
                    Call img.SetPixel(2, x, y, BB)
                Next
            Next

            Return img
        End Function

        <Extension>
        Public Function Sharp(img As BitmapBuffer, Optional SharpDgree As Single = 0.3, Optional max As Double = 255) As BitmapBuffer
            Dim OutPutWid = img.Width
            Dim OutPutHei = img.Height
            Dim Div1 As Single = 1 + SharpDgree
            Dim Div2 As Single = -SharpDgree / 3

            If max > 255 Then
                max = 255
            End If

            For x As Integer = 0 To OutPutWid - 2
                For y As Integer = 0 To OutPutHei - 2
                    Dim RR = img.GetPixel(0, x, y) * Div1
                    Dim GG = img.GetPixel(1, x, y) * Div1
                    Dim BB = img.GetPixel(2, x, y) * Div1
                    Dim Ix = x + 1
                    Dim Iy = y + 1

                    ' A  B  C  D
                    ' E  F  G  H
                    ' I  J  K  L
                    ' M  N  O  P

                    ' Delta = A - (B + E + F) / 3
                    ' F = F + Delta * Alpha

                    ' B + E + F
                    Dim R = img.GetPixel(0, Ix, Iy) + img.GetPixel(0, x, Iy) + img.GetPixel(0, Ix, y)
                    Dim G = img.GetPixel(1, Ix, Iy) + img.GetPixel(1, x, Iy) + img.GetPixel(1, Ix, y)
                    Dim B = img.GetPixel(2, Ix, Iy) + img.GetPixel(2, x, Iy) + img.GetPixel(2, Ix, y)

                    RR = RR + R * Div2
                    GG = GG + G * Div2
                    BB = BB + B * Div2

                    If RR < 0 Then RR = 0
                    If RR > max Then RR = max
                    If GG < 0 Then GG = 0
                    If GG > max Then GG = max
                    If BB < 0 Then BB = 0
                    If BB > max Then BB = max

                    Call img.SetPixel(0, x, y, RR)
                    Call img.SetPixel(1, x, y, GG)
                    Call img.SetPixel(2, x, y, BB)
                Next
            Next

            Return img
        End Function
    End Module
End Namespace
