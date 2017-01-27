#Region "Microsoft.VisualBasic::bc2850cc0919229d1a464b7964da0be4, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\GrayBitmap.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Win32

Namespace Imaging

    ''' <summary>
    ''' Download by http://www.codefans.net
    ''' </summary>
    ''' <remarks></remarks>
    Public Module GrayBitmap

        Private Function GetRValue(rgbColor As Integer) As Integer
            Return rgbColor And &HFF
        End Function

        '
        Private Function GetGValue(rgbColor As Integer) As Integer
            Return (rgbColor And &HFF00) / &HFF
        End Function

        '
        Private Function GetBValue(rgbColor As Integer) As Integer
            Return (rgbColor And &HFF0000) / &HFF00
        End Function

        '
        Private Sub ChangetoGray(SrcDC As Integer, nx As Integer, ny As Integer, Optional nMaskColor As Integer = -1)

            Dim rgbColor As Integer, Gray As Integer
            Dim RValue As Integer, GValue As Integer, BValue As Integer
            Dim dl As Integer

            'get color.
            rgbColor = GetPixel(SrcDC, nx, ny)

            'if rgbColor=MaskColor, don't chang the color
            If rgbColor = nMaskColor Then GoTo Release

            'get color rgb heft.
            RValue = GetRValue(rgbColor)
            GValue = GetGValue(rgbColor)
            BValue = GetBValue(rgbColor)

            'set new color
            Gray = CInt((9798 * RValue + 19235 * GValue + 3735 * BValue) / 32768) 'Change wffs

            rgbColor = RGB(Gray, Gray, Gray)

            dl = SetPixelV(SrcDC, nx, ny, rgbColor)

Release:
            rgbColor = 0 : Gray = 0
            RValue = 0 : GValue = 0 : BValue = 0
            dl = 0
        End Sub

        ''' <summary>
        ''' Chang the bitmap to gray bitmap in hdc.
        ''' </summary>
        ''' <param name="hdc"></param>
        ''' <param name="nx"></param>
        ''' <param name="ny"></param>
        ''' <param name="nWidth"></param>
        ''' <param name="nHeight"></param>
        ''' <param name="nMaskColor"></param>
        ''' <remarks></remarks>
        Private Sub DrawGrayBitmap(hdc As Integer, nx As Integer, ny As Integer, nWidth As Integer, nHeight As Integer, Optional nMaskColor As Integer = -1)
            For i As Integer = nx To nWidth
                For j As Integer = ny To nHeight
                    'Call ChangetoGray function
                    ChangetoGray(hdc, i, j, nMaskColor)
                Next
            Next
        End Sub

        Public Function CreateGrayBitmap(res As Image) As Bitmap
            Dim Bitmap As Bitmap = New Bitmap(original:=DirectCast(res.Clone, Bitmap))
            Dim hdc = Bitmap.GetHbitmap
            Call DrawGrayBitmap(hdc, 0, 0, Bitmap.Width - 1, Bitmap.Height - 1, nMaskColor:=0)
            Return Bitmap
        End Function
    End Module
End Namespace
