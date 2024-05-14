#Region "Microsoft.VisualBasic::6f00de03fb66661dd9d739dbf2548f51, gr\Microsoft.VisualBasic.Imaging\Filters\GaussianSmooth.vb"

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

    '   Total Lines: 114
    '    Code Lines: 80
    ' Comment Lines: 19
    '   Blank Lines: 15
    '     File Size: 4.57 KB


    '     Class GaussianSmooth
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Smooth
    ' 
    '         Sub: Convolution
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Namespace Filters

    Public Class GaussianSmooth

        ''' <summary>
        ''' 声明私有的高斯模糊卷积核函数
        ''' </summary>
        ReadOnly GaussianBlur As Double(,)

        ''' <summary>
        ''' 初始化高斯模糊卷积核
        ''' </summary>
        ''' <param name="k"></param>
        Sub New(Optional k As Integer = 273)
            GaussianBlur = New Double(4, 4) {
                {1 / k, 4 / k, 7 / k, 4 / k, 1 / k},
                {4 / k, 16 / k, 26 / k, 16 / k, 4 / k},
                {7 / k, 26 / k, 41 / k, 26 / k, 7 / k},
                {4 / k, 16 / k, 26 / k, 16 / k, 4 / k},
                {1 / k, 4 / k, 7 / k, 4 / k, 1 / k}
            }
        End Sub

        Public Function Smooth(bitmap As Bitmap) As Bitmap
            Using img As BitmapBuffer = BitmapBuffer.FromBitmap(bitmap)
                Return Smooth(bitmap:=img)
            End Using
        End Function

        ''' <summary>
        ''' 对图像进行平滑处理（利用高斯平滑Gaussian Blur）
        ''' </summary>
        ''' <param name="bitmap">要处理的位图</param>
        ''' <returns>返回平滑处理后的位图</returns>
        Public Function Smooth(bitmap As BitmapBuffer) As Bitmap
            Dim inputPicture = New Integer(2, bitmap.Width - 1, bitmap.Height - 1) {}
            Dim color As Color

            For i As Integer = 0 To bitmap.Width - 1
                For j As Integer = 0 To bitmap.Height - 1
                    color = bitmap.GetPixel(i, j)
                    inputPicture(0, i, j) = color.R
                    inputPicture(1, i, j) = color.G
                    inputPicture(2, i, j) = color.B
                Next
            Next

            Using img As BitmapBuffer = BitmapBuffer.FromBitmap(New Bitmap(bitmap.Width, bitmap.Height))
                Call Convolution(
                    lsmooth:=img,
                    inputPic:=inputPicture,
                    width:=bitmap.Width,
                    height:=bitmap.Height
                )

                Return img.GetImage
            End Using
        End Function

        Private Sub Convolution(lsmooth As BitmapBuffer, inputPic As Integer(,,), width%, height%)
            ' 循环计算使得OutputPicture数组得到计算后位图的RGB
            For i As Integer = 0 To width - 1
                For j As Integer = 0 To height - 1
                    Dim lR As Integer = 0
                    Dim lG As Integer = 0
                    Dim lB As Integer = 0

                    ' 每一个像素计算使用高斯模糊卷积核进行计算
                    ' 循环卷积核的每一行
                    For r As Integer = 0 To 4
                        ' 循环卷积核的每一列
                        For f As Integer = 0 To 4
                            '控制与卷积核相乘的元素
                            Dim row As Integer = i - 2 + r
                            Dim index As Integer = j - 2 + f

                            ' 当超出位图的大小范围时，选择最边缘的像素值作为该点的像素值
                            row = If(row < 0, 0, row)
                            index = If(index < 0, 0, index)
                            row = If(row >= width, width - 1, row)
                            index = If(index >= height, height - 1, index)

                            ' 输出得到像素的RGB值
                            lR += CInt(GaussianBlur(r, f) * inputPic(0, row, index))
                            lG += CInt(GaussianBlur(r, f) * inputPic(1, row, index))
                            lB += CInt(GaussianBlur(r, f) * inputPic(2, row, index))
                        Next
                    Next

                    If lR > 255 Then
                        lR = 255
                    ElseIf lR < 0 Then
                        lR = 0
                    End If
                    If lG > 255 Then
                        lG = 255
                    ElseIf lG < 0 Then
                        lG = 0
                    End If
                    If lB > 255 Then
                        lB = 255
                    ElseIf lB < 0 Then
                        lB = 0
                    End If

                    lsmooth.SetPixel(i, j, Color.FromArgb(lR, lG, lB))
                Next
            Next
        End Sub
    End Class
End Namespace
