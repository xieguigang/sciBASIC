#Region "Microsoft.VisualBasic::0b733e779ea99531c730f6f3103cb8f6, Microsoft.VisualBasic.Core\src\Drawing\GDI+\ImageTools.vb"

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

    '   Total Lines: 241
    '    Code Lines: 145 (60.17%)
    ' Comment Lines: 52 (21.58%)
    '    - Xml Docs: 63.46%
    ' 
    '   Blank Lines: 44 (18.26%)
    '     File Size: 9.13 KB


    '     Module ImageTools
    ' 
    '         Function: BufferInternal, (+2 Overloads) CorpBlank, CorpBlankInternal, (+2 Overloads) ImageCrop
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Imaging

    Public Module ImageTools

        ''' <summary>
        ''' Crop an image 
        ''' </summary>
        ''' <param name="img">image to crop</param>
        ''' <param name="cropArea">rectangle to crop</param>
        ''' <returns>resulting image</returns>
        ''' 
        <Extension>
        Public Function ImageCrop(img As Image, cropArea As Rectangle) As Image
            Return New Bitmap(img).ImageCrop(cropArea)
        End Function

        ''' <summary>
        ''' Crop region on an bitmap image 
        ''' </summary>
        ''' <param name="bmpImage">bitmap image to crop</param>
        ''' <param name="cropArea">rectangle to crop</param>
        ''' <returns>resulting bitmap image object</returns>
        ''' 
        <Extension>
        Public Function ImageCrop(bmpImage As Bitmap, cropArea As Rectangle) As Bitmap
            Dim bmpCrop As Bitmap
#If NET48 Then
            bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat)
#Else
            Dim buffer As BitmapBuffer = BitmapBuffer.FromBitmap(bmpImage)
            Dim crop As BitmapBuffer = BitmapTools.CropBitmapBuffer(buffer, cropArea)

            bmpCrop = New Bitmap(crop)
#End If
            Return bmpCrop
        End Function

        ''' <summary>
        ''' 将图像的多余的空白处给剪裁掉，确定边界，然后进行剪裁，使用这个函数需要注意下设置空白色，默认使用的空白色为<see cref="Color.White"/>
        ''' </summary>
        ''' <param name="res"></param>
        ''' <param name="margin"></param>
        ''' <param name="blankColor">默认白色为空白色</param>
        ''' <returns></returns>
        <Extension>
        Public Function CorpBlank(res As Image,
                                  Optional margin% = 0,
                                  Optional blankColor As Color = Nothing,
                                  <CallerMemberName>
                                  Optional trace$ = Nothing) As Image

            Return New Bitmap(res).CorpBlank(margin, blankColor, trace)
        End Function

        ''' <summary>
        ''' 将图像的多余的空白处给剪裁掉，确定边界，然后进行剪裁，使用这个函数需要注意下设置空白色，默认使用的空白色为<see cref="Color.White"/>
        ''' </summary>
        ''' <param name="res"></param>
        ''' <param name="margin"></param>
        ''' <param name="blankColor">默认白色为空白色</param>
        ''' <returns></returns>
        <Extension>
        Public Function CorpBlank(res As Bitmap,
                                  Optional margin% = 0,
                                  Optional blankColor As Color = Nothing,
                                  <CallerMemberName>
                                  Optional trace$ = Nothing) As Image

            Dim isTransparent As Boolean = False

            If blankColor.IsNullOrEmpty Then
                blankColor = Color.White
            ElseIf blankColor.Name = NameOf(Color.Transparent) Then
                ' 系统的transparent颜色为 0,255,255,255
                ' 但是bitmap之中的transparent为 0,0,0,0
                ' 在这里要变换一下
                blankColor = New Color
                isTransparent = True
            End If

            Return res.CorpBlankInternal(margin, blankColor, isTransparent, trace)
        End Function

        Private Function BufferInternal(res As Bitmap, trace As String) As BitmapBuffer
            Try
                Return BitmapBuffer.FromBitmap(res)
            Catch ex As Exception
                ' 2017-9-21 ???
                ' 未经处理的异常: 
                ' System.ArgumentException: 参数无效。
                '    在 System.Drawing.Bitmap..ctor(Int32 width, Int32 height, PixelFormat format)
                '    在 System.Drawing.Bitmap..ctor(Image original, Int32 width, Int32 height)
                '    在 System.Drawing.Bitmap..ctor(Image original)
                Throw New Exception(trace & " -> " & res.Size.GetJson, ex)
            End Try
        End Function

        ''' <summary>
        ''' 将图像的多余的空白处给剪裁掉，确定边界，然后进行剪裁，使用这个函数需要注意下设置空白色，默认使用的空白色为<see cref="Color.White"/>
        ''' </summary>
        ''' <param name="res"></param>
        ''' <param name="blankColor">默认白色为空白色</param>
        ''' <returns></returns>
        <Extension>
        Private Function CorpBlankInternal(res As Bitmap, blankColor As Color, isTransparent As Boolean, trace$) As Image
            ' 通过 BitmapBuffer 直接读取内存中的像素数据，比 bitmap.GetPixel 更快
            Dim bmp As BitmapBuffer = BufferInternal(res, trace)
            Dim width As Integer = res.Width
            Dim height As Integer = res.Height

            ' 单次扫描整张图像，记录所有非背景色像素的最小/最大 x 与 y，
            ' 从而得到内容的精确外接矩形。这样裁剪矩形严格等于内容包围盒，
            ' 从数学上杜绝把真实内容（非背景区域）误裁掉的过度减裁问题。
            Dim minX As Integer = width
            Dim minY As Integer = height
            Dim maxX As Integer = -1
            Dim maxY As Integer = -1

            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    If Not GDIColors.Equals(bmp.GetPixel(x, y), blankColor) Then
                        If x < minX Then minX = x
                        If x > maxX Then maxX = x
                        If y < minY Then minY = y
                        If y > maxY Then maxY = y
                    End If
                Next
            Next

            If maxX < 0 Then
                ' 整张图片都是背景色，没有任何内容需要保留，直接返回原图，
                ' 避免退化裁剪为极小尺寸（原实现在此情况下会得到 1x1 之类的错误结果）
                Return res
            End If

            ' 基于精确包围盒裁剪（max - min + 1 确保包含边界内容像素），
            ' 不再做任何额外的 ±1 偏移调整，消除原实现上下/左右不对称的问题
            Dim cropRect As New Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1)
            Return res.ImageCrop(cropRect)
        End Function

        ''' <summary>
        ''' 将图像的多余的空白处给剪裁掉，确定边界，然后进行剪裁，使用这个函数需要注意下设置空白色，默认使用的空白色为<see cref="Color.White"/>
        ''' </summary>
        ''' <param name="res"></param>
        ''' <param name="margin"></param>
        ''' <param name="blankColor">默认白色为空白色</param>
        ''' <returns></returns>
        <Extension>
        Private Function CorpBlankInternal(res As Bitmap, margin%, blankColor As Color, isTransparent As Boolean, trace$) As Image
            res = res.CorpBlankInternal(blankColor, isTransparent, trace)

            If margin > 0 Then
                Dim paddedSize As New Size(res.Width + margin * 2, res.Height + margin * 2)
                Dim gfx As IGraphics = DriverLoad.CreateDefaultRasterGraphics(paddedSize, If(isTransparent, Color.Transparent, blankColor))

                ' 20260729 直接使用DrawImageUnscaled似乎会让图片被稍微放大
                ' 直接使用DrawImage方法，并限定图片大小来解决掉这个bug
                Const scale As Single = 0.9999

                Call gfx.Clear(blankColor)
                Call gfx.DrawImage(res, CSng(margin), CSng(margin), CSng(res.Width * scale), CSng(res.Height * scale))

                Return DirectCast(gfx, GdiRasterGraphics).ImageResource
            Else
                Return res
            End If
        End Function
    End Module
End Namespace
