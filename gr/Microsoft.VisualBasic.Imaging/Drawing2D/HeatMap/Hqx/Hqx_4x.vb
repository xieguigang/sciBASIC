#Region "Microsoft.VisualBasic::8a808d27a120b35962de673c4519b320, gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\Hqx\Hqx_4x.vb"

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

    '   Total Lines: 4276
    '    Code Lines: 4019
    ' Comment Lines: 54
    '   Blank Lines: 203
    '     File Size: 310.03 KB


    '     Class Hqx_4x
    ' 
    '         Sub: case0, case144, case148, case152, case16
    '              case192, case194, case2, case20, case224
    '              case24, case25, case28, case3, case40
    '              case41, case56, case6, case64, case66
    '              case67, case7, case70, case8, case9
    '              case96, case98, (+2 Overloads) hq4x_32_rb
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

Option Strict On

Namespace Drawing2D.HeatMap.hqx
    Public Class Hqx_4x : Inherits HqxScaling

        ''' <summary>
        ''' This is the extended Java port of the hq4x algorithm.
        ''' <b>The destination image must be exactly 4 times as large in both dimensions as the source image</b>
        ''' The Y, U, V, A parameters will be set as 48, 7, 6 and 0, respectively. Also, wrapping will be false.
        ''' </summary>
        ''' <param name="sp"> the source image data array in ARGB format </param>
        ''' <param name="dp"> the destination image data array in ARGB format </param>
        ''' <param name="Xres"> the horizontal resolution of the source image </param>
        ''' <param name="Yres"> the vertical resolution of the source image
        ''' </param>
        Public Shared Sub hq4x_32_rb(sp As UInteger(), dp As UInteger(), Xres As Integer, Yres As Integer)
            hq4x_32_rb(sp, dp, Xres, Yres, 48, 7, 6, 0, False, False)
        End Sub

        ''' <summary>
        ''' This and the next caseXXX methods were used to reduce the code size of the main
        ''' #hq4x_32_rb(int[], int[], int, int, int, int, int, int, boolean, boolean) method because of the Java 65K bytecode limit.
        ''' Only the necessary methods were created, to leave the maximum code on the original one to avoid excessive calling.
        ''' However, this is a very bad design (too much code in the same method)
        ''' </summary>
        Private Shared Sub case0(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
        End Sub

        Private Shared Sub case2(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
        End Sub

        Private Shared Sub case16(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case64(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case8(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
        End Sub

        Private Shared Sub case3(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
        End Sub

        Private Shared Sub case6(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
        End Sub

        Private Shared Sub case20(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case144(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))
        End Sub

        Private Shared Sub case192(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
        End Sub

        Private Shared Sub case96(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case40(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
        End Sub

        Private Shared Sub case9(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
        End Sub

        Private Shared Sub case66(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case24(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case7(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
        End Sub

        Private Shared Sub case148(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))
        End Sub

        Private Shared Sub case224(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
        End Sub

        Private Shared Sub case41(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
        End Sub

        Private Shared Sub case67(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub


        Private Shared Sub case70(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case28(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case152(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))
        End Sub

        Private Shared Sub case194(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
        End Sub

        Private Shared Sub case98(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case56(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        Private Shared Sub case25(dp As UInteger(), dpIdx As Integer, dpL As Integer, w As UInteger())
            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
        End Sub

        ''' <summary>
        ''' This is the extended Java port of the hq4x algorithm.
        ''' <b>The destination image must be exactly 4 times as large in both dimensions as the source image</b> </summary>
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
        Public Shared Sub hq4x_32_rb(sp As UInteger(), dp As UInteger(), Xres As Integer, Yres As Integer, [trY] As UInteger, trU As UInteger, trV As UInteger, trA As UInteger, wrapX As Boolean, wrapY As Boolean)
            Dim spIdx = 0, dpIdx = 0

            'Don't shift trA, as it uses shift right instead of a mask for comparisons.
            [trY] <<= 2 * 8
            trU <<= 1 * 8

            Dim dpL = Xres * 4
            Dim prevline, nextline As Integer
            Dim w = New UInteger(8) {}

            For j As Integer = 0 To Yres - 1
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
                            If HqxScaling.diff(w(4), w(k), [trY], trU, trV, trA) Then
                                pattern = pattern Or flag
                            End If
                        End If
                        flag <<= 1
                    Next

                    Select Case pattern
                        Case 0, 1, 4, 32, 128, 5, 132, 160, 33, 129, 36, 133, 164, 161, 37, 165
                            case0(dp, dpIdx, dpL, w)

                        Case 2, 34, 130, 162
                            case2(dp, dpIdx, dpL, w)

                        Case 16, 17, 48, 49
                            case16(dp, dpIdx, dpL, w)

                        Case 64, 65, 68, 69
                            case64(dp, dpIdx, dpL, w)

                        Case 8, 12, 136, 140
                            case8(dp, dpIdx, dpL, w)

                        Case 3, 35, 131, 163
                            case3(dp, dpIdx, dpL, w)

                        Case 6, 38, 134, 166
                            case6(dp, dpIdx, dpL, w)

                        Case 20, 21, 52, 53
                            case20(dp, dpIdx, dpL, w)

                        Case 144, 145, 176, 177
                            case144(dp, dpIdx, dpL, w)

                        Case 192, 193, 196, 197
                            case192(dp, dpIdx, dpL, w)

                        Case 96, 97, 100, 101
                            case96(dp, dpIdx, dpL, w)

                        Case 40, 44, 168, 172
                            case40(dp, dpIdx, dpL, w)

                        Case 9, 13, 137, 141
                            case9(dp, dpIdx, dpL, w)

                        Case 18, 50
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 80, 81
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 72, 76
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 10, 138
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 66
                            case66(dp, dpIdx, dpL, w)

                        Case 24
                            case24(dp, dpIdx, dpL, w)

                        Case 7, 39, 135
                            case7(dp, dpIdx, dpL, w)

                        Case 148, 149, 180
                            case148(dp, dpIdx, dpL, w)

                        Case 224, 228, 225
                            case224(dp, dpIdx, dpL, w)

                        Case 41, 169, 45
                            case41(dp, dpIdx, dpL, w)

                        Case 22, 54
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 208, 209
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 104, 108
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 11, 139
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 19, 51
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 2) = Interpolation.Mix5To3(w(1), w(5))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix2To1To1(w(5), w(4), w(1))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 146, 178
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(1), w(4), w(5))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(5), w(1))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))

                        Case 84, 85
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(5), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(7), w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 112, 113
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(5), w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(7), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If

                        Case 200, 204
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(3), w(4), w(7))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 73, 77
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(3), w(7))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix2To1To1(w(7), w(4), w(3))
                            End If
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 42, 170
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix2To1To1(w(1), w(4), w(3))
                                dp(dpIdx + dpL) = Interpolation.Mix5To3(w(3), w(1))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 14, 142
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix5To3(w(1), w(3))
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix2To1To1(w(3), w(4), w(1))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 67
                            case67(dp, dpIdx, dpL, w)

                        Case 70
                            case70(dp, dpIdx, dpL, w)

                        Case 28
                            case28(dp, dpIdx, dpL, w)

                        Case 152
                            case152(dp, dpIdx, dpL, w)

                        Case 194
                            case194(dp, dpIdx, dpL, w)

                        Case 98
                            case98(dp, dpIdx, dpL, w)

                        Case 56
                            case56(dp, dpIdx, dpL, w)

                        Case 25
                            case25(dp, dpIdx, dpL, w)

                        Case 26, 31
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 82, 214
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 88, 248
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If

                        Case 74, 107
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 27
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 86
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 216
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 106
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 30
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 210
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 120
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 75
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 29
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 198
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 184
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 99
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 57
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 71
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 156
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 226
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 60
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 195
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 102
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 153
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 58
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 83
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 92
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 202
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 78
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 154
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 114
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))

                        Case 89
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 90
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 55, 23
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 2) = Interpolation.Mix5To3(w(1), w(5))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix2To1To1(w(5), w(4), w(1))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 182, 150
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(1), w(4), w(5))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(5), w(1))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))

                        Case 213, 212
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(5), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(7), w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 241, 240
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(5), w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(7), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If

                        Case 236, 232
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(3), w(4), w(7))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 109, 105
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(3), w(7))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix2To1To1(w(7), w(4), w(3))
                            End If
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 171, 43
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix2To1To1(w(1), w(4), w(3))
                                dp(dpIdx + dpL) = Interpolation.Mix5To3(w(3), w(1))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 143, 15
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix5To3(w(1), w(3))
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix2To1To1(w(3), w(4), w(1))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 124
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 203
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 62
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 211
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 118
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 217
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 110
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 155
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 188
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 185
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 61
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 157
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 103
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 227
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 230
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 199
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 220
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If

                        Case 158
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 234
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 242
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))

                        Case 59
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 121
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 87
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 79
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 122
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 94
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 218
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If

                        Case 91
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 229
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 167
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 173
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 181
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 186
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 115
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))

                        Case 93
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 206
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 205, 201
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 174, 46
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL + 1) = w(4)
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 179, 147
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 117, 116
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))
                            Else
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))

                        Case 189
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 231
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 126
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 219
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 125
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(3))
                                dp(dpIdx + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(3), w(7))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix2To1To1(w(7), w(4), w(3))
                            End If
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 221
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix3To1(w(4), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(5), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix2To1To1(w(7), w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 207
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + 1) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix5To3(w(1), w(3))
                                dp(dpIdx + 2) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + dpL) = Interpolation.Mix2To1To1(w(3), w(4), w(1))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            End If
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 238
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix2To1To1(w(3), w(4), w(7))
                                dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(7))
                            End If
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 190
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))
                            Else
                                dp(dpIdx + 2) = Interpolation.Mix2To1To1(w(1), w(4), w(5))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(5), w(1))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))

                        Case 187
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                                dp(dpIdx + dpL + 1) = w(4)
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix2To1To1(w(1), w(4), w(3))
                                dp(dpIdx + dpL) = Interpolation.Mix5To3(w(3), w(1))
                                dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                                dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix3To1(w(4), w(3))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 243
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(5), w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix3To1(w(4), w(7))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(7), w(5))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If

                        Case 119
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                                dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 2) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix3To1(w(4), w(1))
                                dp(dpIdx + 1) = Interpolation.Mix3To1(w(1), w(4))
                                dp(dpIdx + 2) = Interpolation.Mix5To3(w(1), w(5))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                                dp(dpIdx + dpL + 3) = Interpolation.Mix2To1To1(w(5), w(4), w(1))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 237, 233
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(1))
                            dp(dpIdx + dpL + dpL) = w(4)
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 175, 47
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix6To1To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))

                        Case 183, 151
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = w(4)
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = w(4)
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 245, 244
                            dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(3))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix6To1To1(w(4), w(3), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 250
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If

                        Case 123
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 95
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 222
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 252
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix4To2To1(w(4), w(1), w(0))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 249
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix4To2To1(w(4), w(1), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = w(4)
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + dpL + 1) = w(4)

                        Case 235
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(2))
                            dp(dpIdx + dpL + dpL) = w(4)
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 111
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix4To2To1(w(4), w(5), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 63
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix4To2To1(w(4), w(7), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 159
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = w(4)
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = w(4)
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix4To2To1(w(4), w(7), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 215
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = w(4)
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = w(4)
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 246
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix4To2To1(w(4), w(3), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 254
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(0))
                            dp(dpIdx + 1) = Interpolation.Mix3To1(w(4), w(0))
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix3To1(w(4), w(0))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(0))
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 253
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 1) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 2) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(1))
                            dp(dpIdx + dpL) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix7To1(w(4), w(1))
                            dp(dpIdx + dpL + dpL) = w(4)
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL + 3) = w(4)
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 251
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(2))
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(2))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix3To1(w(4), w(2))
                            dp(dpIdx + dpL + dpL) = w(4)
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + dpL + 1) = w(4)

                        Case 239
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            dp(dpIdx + dpL + dpL) = w(4)
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(5))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(5))

                        Case 127
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 2) = w(4)
                                dp(dpIdx + 3) = w(4)
                                dp(dpIdx + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + 2) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + 3) = Interpolation.MixEven(w(1), w(5))
                                dp(dpIdx + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL) = Interpolation.MixEven(w(3), w(4))
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.MixEven(w(7), w(3))
                                dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.MixEven(w(7), w(4))
                            End If
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix3To1(w(4), w(8))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(8))

                        Case 191
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = w(4)
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = w(4)
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 2) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + 3) = Interpolation.Mix7To1(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.Mix5To3(w(4), w(7))
                            dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix5To3(w(4), w(7))

                        Case 223
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                                dp(dpIdx + 1) = w(4)
                                dp(dpIdx + dpL) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.MixEven(w(1), w(3))
                                dp(dpIdx + 1) = Interpolation.MixEven(w(1), w(4))
                                dp(dpIdx + dpL) = Interpolation.MixEven(w(3), w(4))
                            End If
                            dp(dpIdx + 2) = w(4)
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = w(4)
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix3To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(6))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + 3) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + 3) = Interpolation.MixEven(w(5), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 2) = Interpolation.MixEven(w(7), w(4))
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.MixEven(w(7), w(5))
                            End If
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(6))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix3To1(w(4), w(6))

                        Case 247
                            dp(dpIdx) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + 2) = w(4)
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = w(4)
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix5To3(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 1) = Interpolation.Mix7To1(w(4), w(3))
                            dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                        Case 255
                            If HqxScaling.diff(w(3), w(1), [trY], trU, trV, trA) Then
                                dp(dpIdx) = w(4)
                            Else
                                dp(dpIdx) = Interpolation.Mix2To1To1(w(4), w(1), w(3))
                            End If
                            dp(dpIdx + 1) = w(4)
                            dp(dpIdx + 2) = w(4)
                            If HqxScaling.diff(w(1), w(5), [trY], trU, trV, trA) Then
                                dp(dpIdx + 3) = w(4)
                            Else
                                dp(dpIdx + 3) = Interpolation.Mix2To1To1(w(4), w(1), w(5))
                            End If
                            dp(dpIdx + dpL) = w(4)
                            dp(dpIdx + dpL + 1) = w(4)
                            dp(dpIdx + dpL + 2) = w(4)
                            dp(dpIdx + dpL + 3) = w(4)
                            dp(dpIdx + dpL + dpL) = w(4)
                            dp(dpIdx + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + 2) = w(4)
                            dp(dpIdx + dpL + dpL + 3) = w(4)
                            If HqxScaling.diff(w(7), w(3), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL) = Interpolation.Mix2To1To1(w(4), w(7), w(3))
                            End If
                            dp(dpIdx + dpL + dpL + dpL + 1) = w(4)
                            dp(dpIdx + dpL + dpL + dpL + 2) = w(4)
                            If HqxScaling.diff(w(5), w(7), [trY], trU, trV, trA) Then
                                dp(dpIdx + dpL + dpL + dpL + 3) = w(4)
                            Else
                                dp(dpIdx + dpL + dpL + dpL + 3) = Interpolation.Mix2To1To1(w(4), w(7), w(5))
                            End If

                    End Select
                    spIdx += 1
                    dpIdx += 4
                Next
                dpIdx += dpL * 3
            Next
        End Sub
    End Class

End Namespace
