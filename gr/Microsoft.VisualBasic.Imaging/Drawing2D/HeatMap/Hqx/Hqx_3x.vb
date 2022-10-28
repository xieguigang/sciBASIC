' 
'  Copyright © 2003 Maxim Stepin (maxst@hiend3d.com)
' 
'  Copyright © 2010 Cameron Zemek (grom@zeminvaders.net)
' 
'  Copyright © 2011 Tamme Schichler (tamme.schichler@googlemail.com)
' 
'  Copyright © 2012 A. Eduardo García (arcnorj@gmail.com)
' 
'  This file is part of hqx-java.
' 
'  hqx-java is free software: you can redistribute it and/or modify
'  it under the terms of the GNU Lesser General Public License as published by
'  the Free Software Foundation, either version 3 of the License, or
'  (at your option) any later version.
' 
'  hqx-java is distributed in the hope that it will be useful,
'  but WITHOUT ANY WARRANTY; without even the implied warranty of
'  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
'  GNU Lesser General Public License for more details.
' 
'  You should have received a copy of the GNU Lesser General Public License
'  along with hqx-java. If not, see <http://www.gnu.org/licenses/>.
' 

Namespace Drawing2D.HeatMap.hqx
    Public Class Hqx_3x
        Inherits HqxScaling
        ''' <summary>
        ''' This is the extended Java port of the hq3x algorithm.
        ''' <b>The destination image must be exactly 3 times as large in both dimensions as the source image</b>
        ''' The Y, U, V, A parameters will be set as 48, 7, 6 and 0, respectively. Also, wrapping will be false.
        ''' </summary>
        ''' <param name="sp"> the source image data array in ARGB format </param>
        ''' <param name="dp"> the destination image data array in ARGB format </param>
        ''' <param name="Xres"> the horizontal resolution of the source image </param>
        ''' <param name="Yres"> the vertical resolution of the source image
        ''' </param>
        ''' <seealsocref=""/> </seealso>
        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: 'final' parameters are not available in .NET:
        'ORIGINAL LINE: public static void hq3x_32_rb(final int[] sp, final int[] dp, final int Xres, final int Yres)
        Public Shared Sub hq3x_32_rb(sp As Integer(), dp As Integer(), Xres As Integer, Yres As Integer)
            hq3x_32_rb(sp, dp, Xres, Yres, 48, 7, 6, 0, False, False)
        End Sub

        ''' <summary>
        ''' This is the extended Java port of the hq3x algorithm.
        ''' <b>The destination image must be exactly 3 times as large in both dimensions as the source image</b> </summary>
        ''' <param name="sp"> the source image data array in ARGB format </param>
        ''' <param name="dp"> the destination image data array in ARGB format </param>
        ''' <param name="Xres"> the horizontal resolution of the source image </param>
        ''' <param name="Yres"> the vertical resolution of the source image </param>
        ''' <param name="trY"> the Y (luminance) threshold </param>
        ''' <param name="trU"> the U (chrominance) threshold </param>
        ''' <param name="trV"> the V (chrominance) threshold </param>
        ''' <param name="trA"> the A (transparency) threshold </param>
        ''' <param name="wrapX"> used for images that can be seamlessly repeated horizontally </param>
        ''' <param name="wrapY"> used for images that can be seamlessly repeated vertically </param>
        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: 'final' parameters are not available in .NET:
        'ORIGINAL LINE: public static void hq3x_32_rb(final int[] sp, final int[] dp, final int Xres, final int Yres, int trY, int trU, final int trV, final int trA, final boolean wrapX, final boolean wrapY)
        Public Shared Sub hq3x_32_rb(sp As Integer(), dp As Integer(), Xres As Integer, Yres As Integer, [trY] As Integer, trU As Integer, trV As Integer, trA As Integer, wrapX As Boolean, wrapY As Boolean)
            Dim spIdx = 0, dpIdx = 0
            'Don't shift trA, as it uses shift right instead of a mask for comparisons.
            [trY] <<= 2 * 8
            trU <<= 1 * 8
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: The original Java variable was marked 'final':
            'ORIGINAL LINE: final int dpL = Xres * 3;
            Dim dpL = Xres * 3

            Dim prevline, nextline As Integer
            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: The original Java variable was marked 'final':
            'ORIGINAL LINE: final int[] w = new int[9];
            Dim w = New Integer(8) {}

            For j = 0 To Yres - 1
                prevline = If(j > 0, -Xres, If(wrapY, Xres * (Yres - 1), 0))
                nextline = If(j < Yres - 1, Xres, If(wrapY, -(Xres * (Yres - 1)), 0))
                For i = 0 To Xres - 1
                    w(1) = sp(spIdx + prevline)
                    w(4) = sp(spIdx)
                    w(7) = sp(spIdx + nextline)

                    If i > 0 Then
                        w(0) = sp(spIdx + prevline - 1)
                        w(3) = sp(spIdx - 1)
                        w(6) = sp(spIdx + nextline - 1)
                    Else
                        If wrapX Then
                            w(0) = sp(spIdx + prevline + Xres - 1)
                            w(3) = sp(spIdx + Xres - 1)
                            w(6) = sp(spIdx + nextline + Xres - 1)
                        Else
                            w(0) = w(1)
                            w(3) = w(4)
                            w(6) = w(7)
                        End If
                    End If

                    If i < Xres - 1 Then
                        w(2) = sp(spIdx + prevline + 1)
                        w(5) = sp(spIdx + 1)
                        w(8) = sp(spIdx + nextline + 1)
                    Else
                        If wrapX Then
                            w(2) = sp(spIdx + prevline - Xres + 1)
                            w(5) = sp(spIdx - Xres + 1)
                            w(8) = sp(spIdx + nextline - Xres + 1)
                        Else
                            w(2) = w(1)
                            w(5) = w(4)
                            w(8) = w(7)
                        End If
                    End If

                    Dim pattern = 0
                    Dim flag = 1

                    For k = 0 To 8
                        If k = 4 Then
                            Continue For
                        End If

                        If w(k) <> w(4) Then
                            If HqxScaling.Hqx.diff(w(4), w(k), [trY], trU, trV, trA) Then
                                pattern = pattern Or flag
                            End If
                        End If
                        flag <<= 1
                    Next
                    Select Case pattern
                        Case 0, 1, 4, 32, 128, 5, 132, 160, 33, 129, 36, 133, 164, 161, 37, 165
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 2, 34, 130, 162
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 16, 17, 48, 49
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 64, 65, 68, 69
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 8, 12, 136, 140
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 3, 35, 131, 163
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 6, 38, 134, 166
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 20, 21, 52, 53
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 144, 145, 176, 177
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 192, 193, 196, 197
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 96, 97, 100, 101
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 40, 44, 168, 172
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 9, 13, 137, 141
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 18, 50
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 80, 81
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 72, 76
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 10, 138
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 66
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 24
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 7, 39, 135
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 148, 149, 180
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 224, 228, 225
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 41, 169, 45
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 22, 54
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 208, 209
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 104, 108
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 11, 139
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 19, 51
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 146, 178
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 84, 85
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.MixEven(w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Exit Select
                        Case 112, 113
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.MixEven(w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            Exit Select
                        Case 200, 204
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 73, 77
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 42, 170
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 14, 142
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 67
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 70
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 28
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 152
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 194
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 98
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 56
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 25
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 26, 31
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 82, 214
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 88, 248
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 74, 107
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 27
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 86
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 216
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 106
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 30
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 210
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 120
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 75
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 29
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 198
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 184
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 99
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 57
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 71
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 156
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 226
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 60
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 195
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 102
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 153
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 58
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 83
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 92
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 202
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 78
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 154
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 114
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 89
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 90
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 55, 23
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 182, 150
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 213, 212
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.MixEven(w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Exit Select
                        Case 241, 240
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.MixEven(w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            Exit Select
                        Case 236, 232
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 109, 105
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 171, 43
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 143, 15
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 124
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 203
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 62
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 211
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 118
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 217
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 110
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 155
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 188
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 185
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 61
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 157
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 103
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 227
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 230
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 199
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 220
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 158
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 234
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 242
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 59
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 121
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 87
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 79
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 122
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 94
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 218
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 91
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 229
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 167
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 173
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 181
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 186
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 115
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 93
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 206
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 205, 201
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 174, 46
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 179, 147
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 117, 116
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 189
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 231
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 126
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 219
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 125
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 221
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.MixEven(w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            Exit Select
                        Case 207
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 238
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 190
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 187
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 243
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.MixEven(w(5), w(7))
                            End If
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            Exit Select
                        Case 119
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 237, 233
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 175, 47
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            Exit Select
                        Case 183, 151
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 245, 244
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 250
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 123
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 95
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 222
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 252
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 249
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 235
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 111
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 63
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 159
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 215
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 246
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 254
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                            End If
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 253
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 251
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 239
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(5))
                            Exit Select
                        Case 127
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To7To7(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To7To7(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            Exit Select
                        Case 191
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                            Exit Select
                        Case 223
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To7To7(w(4), w(3), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(3))
                            End If
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To7To7(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 247
                            dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                        Case 255
                            If HqxScaling.Hqx.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.Hqx.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.Hqx.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(4), w(5), w(7))
                            End If
                            Exit Select
                    End Select
                    spIdx += 1
                    dpIdx += 3
                Next
                dpIdx += dpL * 2
            Next
        End Sub
    End Class

End Namespace
