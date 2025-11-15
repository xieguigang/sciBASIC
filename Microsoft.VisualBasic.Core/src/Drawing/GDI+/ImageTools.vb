Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Serialization.JSON

#If NET48 Then
Imports Bitmap = System.Drawing.Bitmap
#End If

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

            If blankColor.IsNullOrEmpty Then
                blankColor = Color.White
            ElseIf blankColor.Name = NameOf(Color.Transparent) Then
                ' 系统的transparent颜色为 0,255,255,255
                ' 但是bitmap之中的transparent为 0,0,0,0
                ' 在这里要变换一下
                blankColor = New Color
            End If

            Return res.CorpBlankInternal(margin, blankColor, trace)
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
        ''' <param name="margin"></param>
        ''' <param name="blankColor">默认白色为空白色</param>
        ''' <returns></returns>
        <Extension>
        Private Function CorpBlankInternal(res As Bitmap, margin%, blankColor As Color, trace$) As Image
            Dim bmp As BitmapBuffer = BufferInternal(res, trace)
            Dim top%, left%

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
            bmp = BufferInternal(res, trace)

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
            bmp = BufferInternal(res, trace)

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
            bmp = BufferInternal(res, trace)

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
                Dim paddedSize As New Size(res.Width + margin * 2, res.Height + margin * 2)
                Dim gfx As IGraphics = DriverLoad.CreateGraphicsDevice(paddedSize, fill:=blankColor.Rgba, driver:=Drivers.GDI)

                Call gfx.Clear(blankColor)
                Call gfx.DrawImage(res, New Point(margin, margin))

                Return DirectCast(gfx, GdiRasterGraphics).ImageResource
            Else
                Return res
            End If
        End Function
    End Module
End Namespace