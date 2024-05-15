#Region "Microsoft.VisualBasic::5ddcd862d0d56eff40c232466b909aa6, gr\Microsoft.VisualBasic.Imaging\Filters\GaussBlur.vb"

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

    '   Total Lines: 186
    '    Code Lines: 145
    ' Comment Lines: 22
    '   Blank Lines: 19
    '     File Size: 11.70 KB


    '     Class GaussBlur
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GaussBlur
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices.Marshal

Namespace Filters

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' 20220621, warning: not working as expected on LINUX platform 
    ''' due to the reason of call un-managed code.
    ''' </remarks>
    Public NotInheritable Class GaussBlur

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 对一幅图片进行快速模糊处理，函数由 [小鱼儿] 提供
        ''' </summary>
        ''' <param name="imgValue">待处理的图像数据</param>
        ''' <returns>经过模糊处理之后的图像</returns>
        ''' <remarks></remarks>
        Public Shared Function GaussBlur(imgValue As Bitmap) As Bitmap
            Dim iRow As Integer
            Dim iCol As Integer
            Dim oriWidth As Integer = imgValue.Width, oriHeight As Integer = imgValue.Height
            Dim img2 As Bitmap = imgValue.Clone
            Dim h As Integer = img2.Height
            Dim w As Integer = img2.Width - 1
            Dim bd As BitmapData = img2.LockBits(New Rectangle(0, 0, oriWidth, oriHeight), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb)
            Dim s As Integer = bd.Stride
            Dim ptr As IntPtr = bd.Scan0
            Dim rgb(s * h - 1) As Byte
            Dim rgb2(s * h - 1) As Byte

            Call Copy(bd.Scan0, rgb, 0, s * h - 1)

            ' 处理第一行第一个像素
            rgb2(0) = (CInt(0) + rgb(0) + rgb(3) + rgb(s) + rgb(s + 3)) / 4
            rgb2(1) = (CInt(0) + rgb(1) + rgb(4) + rgb(w + 1) + rgb(s + 4)) / 4
            rgb2(2) = (CInt(0) + rgb(2) + rgb(5) + rgb(s + 2) + rgb(s + 5)) / 4

            Dim ends As Integer = oriWidth - 2

            For iCol = 1 To ends
                ' 处理第一行第2个到倒数第2个像素
                rgb2(iCol * 3) = (CInt(0) +
                                        rgb((iCol - 1) * 3) +
                                        rgb(iCol * 3) +
                                        rgb((iCol + 1) * 3) +
                                        rgb(s + (iCol - 1) * 3) +
                                        rgb(s + iCol * 3) +
                                        rgb(s + (iCol + 1) * 3)) / 6
                rgb2(iCol * 3 + 1) = (CInt(0) +
                                        rgb((iCol - 1) * 3 + 1) +
                                        rgb(iCol * 3 + 1) +
                                        rgb((iCol + 1) * 3 + 1) +
                                        rgb(s + (iCol - 1) * 3 + 1) +
                                        rgb(s + iCol * 3 + 1) +
                                        rgb(s + (iCol + 1) * 3 + 1)) / 6
                rgb2(iCol * 3 + 2) = (CInt(0) +
                                        rgb((iCol - 1) * 3 + 2) +
                                        rgb(iCol * 3 + 2) +
                                        rgb((iCol + 1) * 3 + 2) +
                                        rgb(s + (iCol - 1) * 3 + 2) +
                                        rgb(s + iCol * 3 + 2) +
                                        rgb(s + (iCol + 1) * 3 + 2)) / 6
            Next

            ' 处理第一行最后一个像素
            rgb2(w * 3) = (CInt(0) + rgb(w * 3 - 3) + rgb(w * 3) +
                                        rgb(w * 3 - 3 + s) + rgb(w * 3 + s)) / 4
            rgb2(w * 3 + 1) = (CInt(0) + rgb(w * 3 - 2) + rgb(w * 3 - 1) +
                                        rgb(w * 3 - 2 + s) + rgb(w * 3 + s - 1)) / 4
            rgb2(w * 3 + 2) = (CInt(0) + rgb(w * 3 - 1) + rgb(w * 3 + 2) +
                                        rgb(w * 3 - 1 + s) + rgb(w * 3 + s + 2)) / 4
            ends = h - 2

            For iRow = 1 To ends
                ' 处理每行第一个像素
                rgb2(iRow * s) = (CInt(0) + rgb((iRow - 1) * s) + rgb((iRow - 1) * s + 3) +
                                        rgb(iRow * s) + rgb(iRow * s + 3) +
                                        rgb((iRow + 1) * s) + rgb((iRow + 1) * s + 3)) / 6
                rgb2(iRow * s + 1) = (CInt(0) + rgb((iRow - 1) * s + 1) + rgb((iRow - 1) * s + 3 + 1) +
                                        rgb(iRow * s + 1) + rgb(iRow * s + 3 + 1) +
                                        rgb((iRow + 1) * s + 1) + rgb((iRow + 1) * s + 3 + 1)) / 6
                rgb2(iRow * s + 2) = (CInt(0) + rgb((iRow - 1) * s + 2) + rgb((iRow - 1) * s + 3 + 2) +
                                        rgb(iRow * s + 2) + rgb(iRow * s + 3 + 2) +
                                        rgb((iRow + 1) * s + 2) + rgb((iRow + 1) * s + 3 + 2)) / 6

                For iCol = 1 To img2.Width - 2
                    ' 处理每行其他像素
                    rgb2(iRow * s + iCol * 3) = (CInt(0) +
                                                        rgb((iRow - 1) * s + (iCol - 1) * 3) +
                                                        rgb((iRow - 1) * s + iCol * 3) +
                                                        rgb((iRow - 1) * s + (iCol + 1) * 3) +
                                                        rgb(iRow * s + (iCol - 1) * 3) +
                                                        rgb(iRow * s + iCol * 3) +
                                                        rgb(iRow * s + (iCol + 1) * 3) +
                                                        rgb((iRow + 1) * s + (iCol - 1) * 3) +
                                                        rgb((iRow + 1) * s + iCol * 3) +
                                                        rgb((iRow + 1) * s + (iCol + 1) * 3)) / 9
                    rgb2(iRow * s + iCol * 3 + 1) = (CInt(0) +
                                                        rgb((iRow - 1) * s + (iCol - 1) * 3 + 1) +
                                                        rgb((iRow - 1) * s + iCol * 3 + 1) +
                                                        rgb((iRow - 1) * s + (iCol + 1) * 3 + 1) +
                                                        rgb(iRow * s + (iCol - 1) * 3 + 1) +
                                                        rgb(iRow * s + iCol * 3 + 1) +
                                                        rgb(iRow * s + (iCol + 1) * 3 + 1) +
                                                        rgb((iRow + 1) * s + (iCol - 1) * 3 + 1) +
                                                        rgb((iRow + 1) * s + iCol * 3 + 1) +
                                                        rgb((iRow + 1) * s + (iCol + 1) * 3 + 1)) / 9
                    rgb2(iRow * s + iCol * 3 + 2) = (CInt(0) +
                                                        rgb((iRow - 1) * s + (iCol - 1) * 3 + 2) +
                                                        rgb((iRow - 1) * s + iCol * 3 + 2) +
                                                        rgb((iRow - 1) * s + (iCol + 1) * 3 + 2) +
                                                        rgb(iRow * s + (iCol - 1) * 3 + 2) +
                                                        rgb(iRow * s + iCol * 3 + 2) +
                                                        rgb(iRow * s + (iCol + 1) * 3 + 2) +
                                                        rgb((iRow + 1) * s + (iCol - 1) * 3 + 2) +
                                                        rgb((iRow + 1) * s + iCol * 3 + 2) +
                                                        rgb((iRow + 1) * s + (iCol + 1) * 3 + 2)) / 9
                Next

                ' 处理每行最后一个像素
                rgb2(iRow * s + w * 3) = (CInt(0) + rgb((iRow - 1) * s + w * 3) + rgb((iRow - 1) * s + w * 3 - 3) +
                            rgb(iRow * s + w * 3) + rgb(iRow * s + w * 3 - 3) +
                            rgb((iRow + 1) * s + w * 3) + rgb((iRow + 1) * s + w * 3 - 3)) / 6
                rgb2(iRow * s + w * 3 + 1) = (CInt(0) + rgb((iRow - 1) * s + w * 3 + 1) + rgb((iRow - 1) * s + w * 3 - 3 + 1) +
                            rgb(iRow * s + w * 3 + 1) + rgb(iRow * s + w * 3 - 3 + 1) +
                            rgb((iRow + 1) * s + w * 3 + 1) + rgb((iRow + 1) * s + w * 3 - 3 + 1)) / 6
                rgb2(iRow * s + w * 3 + 2) = (CInt(0) + rgb((iRow - 1) * s + w * 3 + 2) + rgb((iRow - 1) * s + w * 3 - 3 + 2) +
                            rgb(iRow * s + w * 3 + 2) + rgb(iRow * s + w * 3 - 3 + 2) +
                            rgb((iRow + 1) * s + w * 3 + 2) + rgb((iRow + 1) * s + w * 3 - 3 + 2)) / 6

            Next

            ' 处理最后一行第一个像素
            rgb2((h - 1) * s) = (CInt(0) + rgb((h - 2) * s) + rgb((h - 2) * s + 3) + rgb((h - 1) * s) + rgb((h - 1) * s)) / 4
            rgb2((h - 1) * s + 1) = (CInt(0) + rgb((h - 2) * s + 1) + rgb((h - 2) * s + 3 + 1) + rgb((h - 1) * s + 1) + rgb((h - 1) * s + 1)) / 4
            rgb2((h - 1) * s + 2) = (CInt(0) + rgb((h - 2) * s + 2) + rgb((h - 2) * s + 3 + 2) + rgb((h - 1) * s + 2) + rgb((h - 1) * s + 2)) / 4

            ends = oriWidth - 2

            For iCol = 1 To ends
                ' 处理最后一行第2个到倒数第2个像素
                rgb2((h - 1) * s + iCol * 3) = (CInt(0) +
                                        rgb((h - 2) * s + (iCol - 1) * 3) +
                                        rgb((h - 2) * s + iCol * 3) +
                                        rgb((h - 2) * s + (iCol + 1) * 3) +
                                        rgb((h - 1) * s + (iCol - 1) * 3) +
                                        rgb((h - 1) * s + iCol * 3) +
                                        rgb((h - 1) * s + (iCol + 1) * 3)) / 6
                rgb2((h - 1) * s + iCol * 3 + 1) = (CInt(0) +
                                        rgb((h - 2) * s + (iCol - 1) * 3 + 1) +
                                        rgb((h - 2) * s + iCol * 3 + 1) +
                                        rgb((h - 2) * s + (iCol + 1) * 3 + 1) +
                                        rgb((h - 1) * s + (iCol - 1) * 3 + 1) +
                                        rgb((h - 1) * s + iCol * 3 + 1) +
                                        rgb((h - 1) * s + (iCol + 1) * 3 + 1)) / 6
                rgb2((h - 1) * s + iCol * 3 + 2) = (CInt(0) +
                                        rgb((h - 2) * s + (iCol - 1) * 3 + 2) +
                                        rgb((h - 2) * s + iCol * 3 + 2) +
                                        rgb((h - 2) * s + (iCol + 1) * 3 + 2) +
                                        rgb((h - 1) * s + (iCol - 1) * 3 + 2) +
                                        rgb((h - 1) * s + iCol * 3 + 2) +
                                        rgb((h - 1) * s + (iCol + 1) * 3 + 2)) / 6
            Next

            ' 处理最后一行最后一个像素
            rgb2((h - 1) * s + w * 3) = (CInt(0) + rgb((h - 1) * s + w * 3 - 3) + rgb((h - 1) * s + w * 3) +
                                rgb((h - 2) * s + w * 3 - 3) + rgb((h - 2) * s + w * 3)) / 4
            rgb2((h - 1) * s + w * 3 + 1) = (CInt(0) + rgb((h - 1) * s + w * 3 - 3 + 1) + rgb((h - 1) * s + w * 3 + 1) +
                                rgb((h - 2) * s + w * 3 - 3 + 1) + rgb((h - 2) * s + w * 3 + 1)) / 4
            rgb2((h - 1) * s + w * 3 + 2) = (CInt(0) + rgb((h - 1) * s + w * 3 - 3 + 2) + rgb((h - 1) * s + w * 3 + 2) +
                                rgb((h - 2) * s + w * 3 - 3 + 2) + rgb((h - 2) * s + w * 3 + 2)) / 4

            Call Copy(rgb2, 0, ptr, s * h - 1)
            Call img2.UnlockBits(bd)

            Return img2
        End Function
    End Class
End Namespace
