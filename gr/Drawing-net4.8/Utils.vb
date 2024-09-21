#Region "Microsoft.VisualBasic::6004c228798a107fdd01c9739bc50627, Microsoft.VisualBasic.Core\src\Extensions\Image\Bitmap\Utils.vb"

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

    '   Total Lines: 259
    '    Code Lines: 166 (64.09%)
    ' Comment Lines: 44 (16.99%)
    '    - Xml Docs: 61.36%
    ' 
    '   Blank Lines: 49 (18.92%)
    '     File Size: 9.51 KB


    '     Module Utils
    ' 
    '         Function: (+2 Overloads) ColorReplace, CorpBlank, ResizeUnscaled, ResizeUnscaledByHeight, TrimRoundAvatar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' Tools function for processing on <see cref="Image"/>/<see cref="Bitmap"/>
    ''' </summary>
    Public Module Utils

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ResizeUnscaled(image As Image, width%) As Image
            Return image.Resize(width, image.Height * (width / image.Width), onlyResizeIfWider:=width > image.Width)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ResizeUnscaledByHeight(image As Image, height%) As Image
            Return image.ResizeUnscaled(image.Width * (height / image.Height))
        End Function

        ''' <summary>
        ''' 图片剪裁为圆形的头像
        ''' </summary>
        ''' <param name="resAvatar">要求为正方形或者近似正方形</param>
        ''' <param name="OutSize"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function TrimRoundAvatar(resAvatar As Image, OutSize As Integer) As Image
            If resAvatar Is Nothing Then
                Return Nothing
            End If

            SyncLock resAvatar
                Dim Bitmap As New Bitmap(OutSize, OutSize)

                resAvatar = DirectCast(resAvatar.Clone, Image)
                resAvatar = resAvatar.ResizeUnscaledByHeight(OutSize)

                Using g = Graphics.FromImage(Bitmap)
                    Dim image As New TextureBrush(resAvatar)

                    With g
                        .CompositingQuality = CompositingQuality.HighQuality
                        .InterpolationMode = InterpolationMode.HighQualityBicubic
                        .SmoothingMode = SmoothingMode.HighQuality
                        .TextRenderingHint = TextRenderingHint.ClearTypeGridFit

                        Call .FillPie(image, Bitmap.EntireImage, 0, 360)
                    End With

                    Return Bitmap
                End Using
            End SyncLock
        End Function

        ''' <summary>
        ''' 将图像的多余的空白处给剪裁掉，确定边界，然后进行剪裁，使用这个函数需要注意下设置空白色，默认使用的空白色为<see cref="Color.White"/>
        ''' </summary>
        ''' <param name="res"></param>
        ''' <param name="margin"></param>
        ''' <param name="blankColor">默认白色为空白色</param>
        ''' <returns></returns>
        <ExportAPI("Image.CorpBlank")>
        <Extension>
        Public Function CorpBlank(res As Image,
                                  Optional margin% = 0,
                                  Optional blankColor As Color = Nothing,
                                  <CallerMemberName>
                                  Optional trace$ = Nothing) As Image

            If blankColor.IsNullOrEmpty Then
                blankColor = Color.White
            ElseIf blankColor.Name = NameOf(Color.Transparent) Then
                ' 系统的transparent颜色为 0,255,255,255
                ' 但是bitmap之中的transparent为 0,0,0,0
                ' 在这里要变换一下
                blankColor = New Color
            End If

            Dim bmp As BitmapBuffer
            Dim top%, left%

            Try
                bmp = BitmapBuffer.FromImage(res)
            Catch ex As Exception

                ' 2017-9-21 ???
                ' 未经处理的异常: 
                ' System.ArgumentException: 参数无效。
                '    在 System.Drawing.Bitmap..ctor(Int32 width, Int32 height, PixelFormat format)
                '    在 System.Drawing.Bitmap..ctor(Image original, Int32 width, Int32 height)
                '    在 System.Drawing.Bitmap..ctor(Image original)

                ex = New Exception(trace & " --> " & res.Size.GetJson, ex)
                Throw ex
            End Try

            ' top
            For top = 0 To res.Height - 1
                Dim find As Boolean = False

                For left = 0 To res.Width - 1
                    Dim p = bmp.GetPixel(left, top)

                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True

                        If top > 0 Then
                            top -= 1
                        End If

                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            Dim region As New Rectangle(0, top, res.Width, res.Height - top)
            res = res.ImageCrop(New Rectangle(region.Location, region.Size))
            bmp = BitmapBuffer.FromImage(res)

            ' left
            For left = 0 To res.Width - 1
                Dim find As Boolean = False

                For top = 0 To res.Height - 1
                    Dim p = bmp.GetPixel(left, top)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True

                        If left > 0 Then
                            left -= 1
                        End If

                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            region = New Rectangle(left, 0, res.Width - left, res.Height)
            res = res.ImageCrop(New Rectangle(region.Location, region.Size))
            bmp = BitmapBuffer.FromImage(res)

            Dim right As Integer
            Dim bottom As Integer

            ' bottom
            For bottom = res.Height - 1 To 0 Step -1
                Dim find As Boolean = False

                For right = res.Width - 1 To 0 Step -1
                    Dim p = bmp.GetPixel(right, bottom)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True

                        bottom += 1

                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            region = New Rectangle(0, 0, res.Width, bottom)
            res = res.ImageCrop(New Rectangle(region.Location, region.Size))
            bmp = BitmapBuffer.FromImage(res)

            ' right
            For right = res.Width - 1 To 0 Step -1
                Dim find As Boolean = False

                For bottom = res.Height - 1 To 0 Step -1
                    Dim p = bmp.GetPixel(right, bottom)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True
                        right += 1

                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            region = New Rectangle(0, 0, right, res.Height)
            res = res.ImageCrop(New Rectangle(region.Location, region.Size))

            If margin > 0 Then
                With New Size(res.Width + margin * 2, res.Height + margin * 2).CreateGDIDevice
                    Call .Clear(blankColor)
                    Call .DrawImage(res, New Point(margin, margin))

                    Return .ImageResource
                End With
            Else
                Return res
            End If
        End Function

        ''' <summary>
        ''' A, R, G, B
        ''' </summary>
        Friend Const PixelSize% = 4
        Friend Const RGBSize% = 3

        ''' <summary>
        ''' Color replace using memory pointer
        ''' </summary>
        ''' <param name="image"></param>
        ''' <param name="subject"></param>
        ''' <param name="replaceAs"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColorReplace(image As Bitmap, subject As Color, replaceAs As Color, Optional tolerance% = 3) As Bitmap
            Using bitmap As BitmapBuffer = BitmapBuffer.FromBitmap(image)
                Dim byts As BitmapBuffer = bitmap

                For x As Integer = 0 To byts.Width - 1
                    For y As Integer = 0 To byts.Height - 1
                        If GDIColors.Equals(byts.GetPixel(x, y), subject, tolerance) Then
                            Call byts.SetPixel(x, y, replaceAs)
                        End If
                    Next
                Next
            End Using

            Return image
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ColorReplace(image As Image, subject As Color, replaceAs As Color, Optional tolerance% = 3) As Bitmap
            Return New Bitmap(image).ColorReplace(subject, replaceAs, tolerance)
        End Function
    End Module
End Namespace
